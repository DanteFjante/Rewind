using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Rewind.Extensions.Users;
using Rewind.Server.Builders;
using Rewind.Server.Sync.Builder;
using Rewind.Server.Users;
using System.Security.Claims;
using System.Text;

namespace Rewind.Server
{
    public static class ServerExtensionMethods
    {
        public const string SyncUriRelative = "/rewind-sync";

        public static ServerBuilder AddAuth(this ServerBuilder serverBuilder)
        {
            serverBuilder.AddService(sc => sc.AddAuthentication().AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("12345678901234567890123456789012")),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),

                    // set these if you issued them
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                options.Events.OnMessageReceived = async context =>
                {
                    var path = context.HttpContext.Request.Path;

                    if (path.StartsWithSegments("/rewind-sync"))
                    {
                        string? accessToken = context.HttpContext.Request.Query["access_token"];
                        if (accessToken?.StartsWith(JwtBearerDefaults.AuthenticationScheme) ?? false)
                        {
                            context.Token = accessToken;
                            return;
                        }

                        string? jsonToken = context.HttpContext.Request.Headers["Authorization"].ToString();
                        if (jsonToken?.StartsWith(JwtBearerDefaults.AuthenticationScheme) ?? false)
                        {
                            string token = jsonToken["Bearer ".Length..].Trim();
                            context.Token = token;
                        }
                    }

                };
            }));

            serverBuilder.AddService(sc => sc.AddAuthorization());
            serverBuilder.AddService<UserContext>();
            serverBuilder.AddService<IUserContext>(x => x.SetFactory(sp => sp.GetRequiredService<UserContext>()));
            return serverBuilder;
        }

        public static IEndpointRouteBuilder MapRewind(this WebApplication app, Func<MapBuilder, MapBuilder> builder)
        {
            MapBuilder mp = new();
            mp = builder(mp);

            mp.Build(app);

            return app;
        }

        public static MapBuilder UseIdentityApi<TUser>(this MapBuilder builder)
            where TUser : class, new()
        {
            builder.AddApplicationAction(app =>
                app.MapIdentityApi<TUser>());

            return builder;
        }

        public static MapBuilder UseAuth(this MapBuilder builder)
        {
            string userid;
            builder.AddApplicationAction(app => app.UseAuthorization());
            builder.AddApplicationAction(app => app.UseAuthentication());
            builder.AddApplicationAction(app => 
                app.MapPost("/auth/login", (LoginRequest request) =>
                {

                    var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes("12345678901234567890123456789012"));
                    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                    DateTime expiresAt = DateTime.UtcNow.AddDays(1);

                    SecurityTokenDescriptor tokenDesc = new SecurityTokenDescriptor()
                    {
                        Issuer = "RewindServer",
                        Audience = "RewindClient",
                        Expires = expiresAt,
                        SigningCredentials = creds,

                        Subject = new ClaimsIdentity([
                            new Claim(ClaimTypes.NameIdentifier, request.userName),
                        ])

                    };
                    string token = new JsonWebTokenHandler().CreateToken(tokenDesc);
                    return Results.Ok(new LoginResponse(token, expiresAt));
                }));
            return builder;
        }
    }
}

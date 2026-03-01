using Microsoft.EntityFrameworkCore;
using Rewind.Server.Base.Store;

namespace Rewind.Data
{
    public class RewindDbContext : DbContext
    {
        public RewindDbContext(DbContextOptions<RewindDbContext> options) : base(options)
        {
        }

        public DbSet<ServerStore> Stores => Set<ServerStore>();
        public DbSet<ServerUserStore> UserStores => Set<ServerUserStore>();
        public DbSet<ServerBranch> Branches => Set<ServerBranch>();
        public DbSet<ServerState> States => Set<ServerState>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServerState>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasOne(x => x.Branch)
                    .WithMany(x => x.States)
                    .HasForeignKey(x => x.BranchId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => new { x.BranchId, x.Version });
                b.HasIndex(x => new { x.StoreName, x.StoreType, x.Version });
            });

            modelBuilder.Entity<ServerBranch>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasMany(x => x.States)
                    .WithOne(x => x.Branch)
                    .HasForeignKey(x => x.BranchId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);
                b.HasIndex(x => new { x.StoreName, x.StoreType });
            });

            modelBuilder.Entity<ServerStore>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasIndex(x => new { x.StoreName, x.StoreType }).IsUnique();
            });

            modelBuilder.Entity<ServerUserStore>(b =>
            {
                b.HasKey(x => x.Id);
                b.HasOne(x => x.Branch)
                    .WithMany()
                    .HasForeignKey(x => x.BranchId)
                    .IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);


                b.HasIndex(x => new { x.OwnerId, x.BranchId }).IsUnique();
            });

        }
    }
}

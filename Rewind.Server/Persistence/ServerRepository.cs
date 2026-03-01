using Microsoft.EntityFrameworkCore;
using Rewind.Data;
using Rewind.Extensions.Persistence;
using Rewind.Server.Base.Persistence;
using Rewind.Server.Base.Store;
using Rewind.Server.Cache;
using Rewind.Server.Users;
namespace Rewind.Server.Persistence;

internal class ServerRepository : IUserServerRepository
{
    RewindDbContext db;
    ICacheService? cache;
    IUserContext? user;

    public ServerRepository(RewindDbContext db, ICacheService? cache = null, IUserContext? user = null)
    {
        this.db = db;
        this.cache = cache;
        this.user = user;

    }
    #region Read

    public ValueTask<bool?> IsPublicAsync(PersistenceKey key)
    {
        return new((from store in db.Stores
                    where store.StoreName == key.Name
                    && store.StoreType == key.Type
                    select (bool?) store.IsPublic)
                    .FirstOrDefaultAsync());
    }

    public ValueTask<HashSet<PersistenceKey>> GetKeysAsync()
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new(new HashSet<PersistenceKey>());

        return new((from us in db.UserStores
                    join br in db.Branches
                    on us.BranchId equals br.Id
                    where (br.IsPublic || us.OwnerId == userId)
                    select new PersistenceKey(br.StoreType, br.StoreName)
                    ).AsNoTracking().ToHashSetAsync());
    }
    public ValueTask<HashSet<string>> GetTypeKeysAsync(string type)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new(new HashSet<string>());

        return new((from us in db.UserStores
                    join br in db.Branches
                    on us.BranchId equals br.Id
                    where (br.IsPublic || us.OwnerId == userId)
                    && br.StoreType == type
                    select br.StoreName
                    ).AsNoTracking().ToHashSetAsync());
    }
    public ValueTask<HashSet<ServerBranch>> GetBranches(PersistenceKey key)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new(new HashSet<ServerBranch>());

        return new((from br in db.Branches
                where br.StoreName == key.Name
                && br.StoreType == key.Type
                select br).ToHashSetAsync());
    }

    public ValueTask<long?> GetVersionAsync(PersistenceKey key)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new((long?)null);

        return new ((from us in db.UserStores
                      join br in db.Branches
                      on us.BranchId equals br.Id
                      where br.StoreName == key.Name
                      && br.StoreType == key.Type
                      && (br.IsPublic || us.OwnerId == userId)
                      select (long?)br.LastVersion)
                      .SingleOrDefaultAsync());
    }

    public ValueTask<long?> GetOldestVersionAsync(PersistenceKey key)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new((long?)null);

        return new((from us in db.UserStores
                    join br in db.Branches
                    on us.BranchId equals br.Id
                    where br.StoreName == key.Name
                    && br.StoreType == key.Type
                    && (br.IsPublic || us.OwnerId == userId)
                    select (long?)br.OldestVersion)
                    .SingleOrDefaultAsync());
    }

    public ValueTask<bool> HasVersion(PersistenceKey key, long version)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new(false);

        return new((from us in db.UserStores
                    join st in db.States
                    on us.BranchId equals st.BranchId
                    where st.StoreName == key.Name
                    && st.StoreType == key.Type
                    && st.Version == version
                    && (st.IsPublic || us.OwnerId == userId)
                    select st.Id).AnyAsync());
    }

    public ValueTask<bool> HasState(PersistenceKey key)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new(false);

        return new((from us in db.UserStores
                    join st in db.States
                    on us.BranchId equals st.BranchId
                    where st.StoreName == key.Name
                    && st.StoreType == key.Type
                    && (st.IsPublic || us.OwnerId == userId)
                    select st.Id).AnyAsync());
    }

    public  ValueTask<ServerState?> GetStateAsync(PersistenceKey key, bool includeLinks = false)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new((ServerState?) null);

        var q = (from br in db.Branches
             join us in db.UserStores
             on br.Id equals us.BranchId
             join st in db.States
             on br.Id equals st.BranchId
             where br.StoreName == key.Name
             && br.StoreType == key.Type
             && (br.IsPublic || us.OwnerId == userId)
             where st.Id == br.LastStateId
             select st);

        if (includeLinks)
            q = q
                .Include(x => x.Branch)
                .ThenInclude(x => x.States)
                .Include(x => x.Branch)
                .ThenInclude(x => x.Store);

        return new(q.AsNoTracking().SingleOrDefaultAsync());
    }

    public ValueTask<ServerState?> GetStateAsync(PersistenceKey key, long version, bool includeLinks = false)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new((ServerState?)null);

        var q = (from st in db.States
                 join us in db.UserStores
                 on st.BranchId equals us.BranchId
                 where st.StoreName == key.Name
                 && st.StoreType == key.Type
                 && st.Version == version
                 && (st.IsPublic || us.OwnerId == userId)
                 select st);

        if (includeLinks)
            q = q
                .Include(x => x.Branch)
                .ThenInclude(x => x.States)
                .Include(x => x.Branch)
                .ThenInclude(x => x.Store);

        return new(q.AsNoTracking().SingleOrDefaultAsync());
    }

    public ValueTask<HashSet<ServerState>> GetStatesAsync(PersistenceKey key, bool includeLinks = false)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new(new HashSet<ServerState>());

        var q = (from st in db.States
                 join us in db.UserStores
                 on st.BranchId equals us.BranchId
                 where st.StoreName == key.Name
                 && st.StoreType == key.Type
                 && (st.IsPublic || us.OwnerId == userId)
                 select st);

        if (includeLinks)
            q = q
                .Include(x => x.Branch)
                .ThenInclude(x => x.States)
                .Include(x => x.Branch)
                .ThenInclude(x => x.Store);

        return new(q.AsNoTracking().ToHashSetAsync());
    }
    public ValueTask<ServerStore?> GetStoreAsync(PersistenceKey key)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new((ServerStore?)null);

        var q = (from st in db.Stores
                 where st.StoreName == key.Name
                 && st.StoreType == key.Type
                 select st);

        return new(q.AsNoTracking().SingleOrDefaultAsync());
    }

    public ValueTask<HashSet<ServerStore>> GetStoresAsync(IEnumerable<PersistenceKey> keys)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new(new HashSet<ServerStore>());

        var wanted = keys
            .Select(k => $"{k.Type}::{k.Name}")
            .Distinct()
            .ToList();

        var stores = db.Stores
            .Where(st => wanted.Contains(st.StoreType + "::" + st.StoreName))
            .AsNoTracking()
            .ToHashSetAsync();

        return new(stores);
    }

    public ValueTask<ServerUserStore?> GetUserStore(PersistenceKey key, bool includeLinks = false)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new((ServerUserStore?)null);

        var q = (from us in db.UserStores
                 join br in db.Branches
                 on us.BranchId equals br.Id
                 where br.StoreName == key.Name
                 && br.StoreType == key.Type
                 && (br.IsPublic || us.OwnerId == userId)
                 select us);

        if (includeLinks)
            q = q
                .Include(x => x.Branch)
                .ThenInclude(x => x.States)
                .Include(x => x.Branch)
                .ThenInclude(x => x.Store);

        return new(q.AsNoTracking().SingleOrDefaultAsync());
    }

    public ValueTask<HashSet<ServerUserStore>> GetUserStoresOfType(string type, bool includeLinks = false)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new(new HashSet<ServerUserStore>());

        var q = (from us in db.UserStores
                 join br in db.Branches
                 on us.BranchId equals br.Id
                 where br.StoreType == type
                 && (br.IsPublic || us.OwnerId == userId)
                 select us);

        if (includeLinks)
            q = q
                .Include(x => x.Branch)
                .ThenInclude(x => x.States)
                .Include(x => x.Branch)
                .ThenInclude(x => x.Store);

        return new(q.AsNoTracking().ToHashSetAsync());
    }

    #endregion

    #region Create

    public async ValueTask<bool> CreateInitialStateAsync(bool isPublic, PersistenceData data)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return false;
        string name = GenerateBranchName(data.ToKey());

        var store = await (from st in db.Stores
                           where data.Name == st.StoreName
                           && data.Type == st.StoreType
                           select st)
                           .SingleOrDefaultAsync();

        var branch = await (from br in db.Branches
                            where br.Name == name
                            select br)
                            .SingleOrDefaultAsync();

        ServerState? state;

        if (store == null)
        {
            state = ServerState.CreateStoreWithState(userId, name, isPublic, data);
        }
        else
        {
            if (branch == null)
            {
                state = ServerState.CreateStateWithBranch(store, name, userId, data);
            }
            else
            {

                state = ServerState.CreateState(store, branch, userId, data);
            }
        }


        if (state == null)
            return false;

        db.States.Add(state);


        var userStore = ServerUserStore.CreateUserStore(state, userId!);
        db.UserStores.Add(userStore);

        await db.SaveChangesAsync();

        return true;
    }

    public async ValueTask<bool> CreateInitialStateAsync(PersistenceData data)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return false;

        string name = GenerateBranchName(data.ToKey());

        var store = await (from st in db.Stores
                           where data.Name == st.StoreName
                           && data.Type == st.StoreType
                           select st)
                           .SingleOrDefaultAsync();

        var branch = await (from br in db.Branches
                            where br.Name == name
                            select br)
                            .SingleOrDefaultAsync();

        ServerState? state;

        if (store == null)
            return false;

        if (branch == null)
        {
            state = ServerState.CreateStateWithBranch(store, name, userId, data);
        }
        else
        {

            state = ServerState.CreateState(store, branch, userId, data);
        }

        if (state == null)
            return false;

        db.States.Add(state);

        var userStore = ServerUserStore.CreateUserStore(state, userId!);
        db.UserStores.Add(userStore);

        await db.SaveChangesAsync();

        return false;
    }
    public async ValueTask<bool> CreateStoreAsync(string storeType, string storeName, bool isPublic)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return false;

        if (await HasState(new PersistenceKey(storeType, storeName)))
            return false;

        var store = new ServerStore(Guid.NewGuid(), storeType, storeName, isPublic);

        db.Stores.Add(store);

        await db.SaveChangesAsync();

        return true;
    }

    public async ValueTask<bool> JoinBranch(string branchName)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return false;

        var branch = await (from br in db.Branches
                            where br.Name == branchName
                            select br)
                            .AsNoTracking()
                           .SingleOrDefaultAsync();

        if (branch == null)
            return false;

        var userStore = await
                        (from us in db.UserStores
                         where us.OwnerId == userId
                         && us.Branch.StoreType == branch.StoreType
                         && us.Branch.StoreName == branch.StoreName
                         select us)
                         .SingleOrDefaultAsync();

        if (userStore != null)
        {
            if (userStore.BranchId == branch.Id)
                return true;

            var userStores = await
                                (from us in db.UserStores
                                 where us.BranchId == branch.Id
                                 select us)
                                 .AsNoTracking()
                                 .ToHashSetAsync();
            
            if (userStores.All(x => x.Id == userStore.Id))
            {
                db.Branches.Remove(branch);
            }

            db.UserStores.Remove(userStore);

        }

        db.UserStores.Add(new ServerUserStore(Guid.NewGuid(), branch.Id, userId));

        await db.SaveChangesAsync();
        return true;
    }

    #endregion

    #region Update
    public async ValueTask<bool> SetStateAsync(PersistenceData data)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return false;

        var state = await GetStateAsync(data.ToKey());

        if (state == null)
        {
            var store = await (from st in db.Stores
                               where st.StoreName == data.Name
                               && st.StoreType == data.Type
                               select st)
                               .SingleOrDefaultAsync();
            if (store == null)
            {
                bool created = await CreateInitialStateAsync(user == null, data);
                if (!created)
                    return false;

                state = await GetStateAsync(data.ToKey());

                if (state == null)
                    return false;
            }
            else
            {
                var userStore = await (from us in db.UserStores
                                       join br in db.Branches
                                       on us.BranchId equals br.Id
                                       where br.StoreName == data.Name
                                       && br.StoreType == data.Type
                                       && us.OwnerId == userId
                                       select us)
                                   .SingleOrDefaultAsync();
                if (userStore == null)
                {
                    bool created = await JoinBranch(GenerateBranchName(data.ToKey()));
                }
            }
        }


        if (data.Version != state.Version + 1)
        {
            return false;
        }

        var newState = state.Update(data);

        var branch = await (from br in db.Branches
                            where state.BranchId == br.Id
                            select br)
                            .SingleAsync();

        branch.LastStateId = newState.Id;
        branch.LastVersion = newState.Version;

        db.Branches.Update(branch);
        db.States.Add(newState);

        var result = await db.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Delete

    public async ValueTask<bool> RemoveUserStore(PersistenceKey key)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return false;

        var store = await GetUserStore(key);

        if (store == null)
            return false;

        var branchId = store.BranchId;

        var keys = await (from us in db.UserStores
                          where us.BranchId == branchId
                          select us.Id).ToHashSetAsync();

        if (keys.Count == 1)
        {
            var branch = await (from br in db.Branches
                                where br.Id == branchId
                                select br)
                                .SingleAsync();

            db.Branches.Remove(branch);
            db.UserStores.Remove(store);
        }
        else
        {
            db.UserStores.Remove(store);
        }

        await db.SaveChangesAsync();

        return true;
    }

    public ValueTask<int> RemoveStatesBefore(PersistenceKey key, long version)
    {

        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        if (!auth)
            return new(0);

        var q = (from br in db.Branches
                 join us in db.UserStores
                 on br.Id equals us.BranchId
                 where us.OwnerId == userId
                 && br.StoreName == key.Name
                 && br.StoreType == key.Type
                 join st in db.States
                 on br.Id equals st.BranchId
                 where st.Version < version
                 select st);

        return new(q.ExecuteDeleteAsync());
    }
    #endregion
    private string GenerateBranchName(PersistenceKey key)
    {
        bool auth = user is { IsAuthenticated: true } || user == null;
        string? userId = auth ? user?.UserId : null;

        //var names = await(from br in db.Branches
        //                  where br.StoreName == key.Name
        //                  && br.StoreType == key.Type
        //                  select br.Name)
        //           .AsNoTracking()
        //           .ToListAsync();

        string dataname = string.IsNullOrWhiteSpace(key.Name) ? "default" : key.Name;
        string datatype = key.Type;
        string username = userId ?? "public";
        string name = $"{username}:{dataname}:{datatype}";

        return name;
    }

    private static string GenerateRandomAlphanumericString(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        var random = new Random();
        var randomString = new string(Enumerable.Repeat(chars, length)
                                                .Select(s => s[random.Next(s.Length)]).ToArray());
        return randomString;
    }
}
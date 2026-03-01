namespace Rewind.Extensions.Persistence.Client;

public interface ILocalStorageService : IPersistanceService
{
    public ValueTask<bool> ClearStorageAsync();
}

namespace DocumentRepository;

public interface IRepository
{
    public IRepositoryItem RootFolder { get; }

    public void OpenRepository(string databaseName);
    public Task OpenRepositoryAsync(string databaseName);
    public Task VacuumAsync();
}
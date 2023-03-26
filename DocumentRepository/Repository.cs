using Microsoft.Data.Sqlite;

namespace DocumentRepository;

public sealed class Repository : IRepository, IDisposable, IAsyncDisposable
{
    private SqliteConnection? _sqliteConnection;
    private RepositoryItem? _rootFolder;

    public IRepositoryItem RootFolder
    {
        get
        {
            return _rootFolder ?? throw new InvalidOperationException("Repository is not open.");
        }
    }

    public void OpenRepository(string databaseName)
    {
        if (_sqliteConnection is not null)
        {
            throw new InvalidOperationException("Database is already open.");
        }

        bool databaseExists = File.Exists(databaseName);
        _sqliteConnection = new($"Data Source={databaseName}");

        _sqliteConnection.Open();

        try
        {
            if (!databaseExists)
            {
                using SqliteCommand commandDb = _sqliteConnection.CreateCommand();
                commandDb.CommandText = "CREATE TABLE Documents(ID INTEGER PRIMARY KEY, Name, DocumentType, Content, CreatedTime, ChangedTime, Parent);\nINSERT INTO Documents(ID, Name, DocumentType, CreatedTime, ChangedTime) VALUES (0, 'Dokumenter', 0, @CreatedTime, @CreatedTime);";
                commandDb.Parameters.AddWithValue("@CreatedTime", DateTime.UtcNow).SqliteType = SqliteType.Real;
                commandDb.ExecuteNonQuery();
            }

            using SqliteCommand command = _sqliteConnection.CreateCommand();

            command.CommandText = "SELECT Name, CreatedTime, ChangedTime FROM Documents WHERE ID=0;";

            using SqliteDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                _rootFolder = new RepositoryItem(0, reader.GetString(0), 0, reader.GetDateTime(1).ToLocalTime(), reader.GetDateTime(2).ToLocalTime(), null, _sqliteConnection);
            }
            else
            {
                _sqliteConnection.Dispose();
                _sqliteConnection = null;
                throw new InvalidOperationException("Database error. No root element.");
            }
        }
        catch
        {
            _sqliteConnection?.Dispose();
            _sqliteConnection = null;
            throw;
        }
    }

    public Task OpenRepositoryAsync(string databaseName)
    {
        return Task.Run(() =>
        {
            OpenRepository(databaseName);
        });
    }

    public async Task VacuumAsync()
    {
        if (_sqliteConnection is null)
        {
            throw new InvalidOperationException("Database is not open.");
        }

        using SqliteCommand command = _sqliteConnection.CreateCommand();

        command.CommandText = "VACUUM;";

        await command.ExecuteNonQueryAsync();
    }

    public void Dispose()
    {
        if ( _sqliteConnection is not null )
        {
            _sqliteConnection.Dispose();
            _sqliteConnection = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_sqliteConnection is not null)
        {
            await _sqliteConnection.DisposeAsync();
            _sqliteConnection = null;
        }
    }
}
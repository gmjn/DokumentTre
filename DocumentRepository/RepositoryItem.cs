using Microsoft.Data.Sqlite;
using System.ComponentModel;

namespace DocumentRepository;

public class RepositoryItem : IRepositoryItem, INotifyPropertyChanged
{
    private static PropertyChangedEventArgs? s_nameChangedEventArgs;
    private static PropertyChangedEventArgs? s_documentTypeChangedEventArgs;
    private static PropertyChangedEventArgs? s_changedTimeEventArgs;

    private string _name;
    private DocumentTypes _documentType;
    private readonly DateTime _createdTime;
    private DateTime _changedTime;
    private readonly RepositoryItem? _parent;
    private readonly SqliteConnection _sqliteConnection;

    internal RepositoryItem(int id, string name, DocumentTypes documentType, DateTime CreatedTime, DateTime ChangedTime, RepositoryItem? parent, SqliteConnection sqliteConnection)
    {
        Id = id;
        _name = name;
        _documentType = documentType;
        _createdTime = CreatedTime;
        _changedTime = ChangedTime;
        _parent = parent;
        _sqliteConnection = sqliteConnection;
    }

    public int Id { get; }
    public string Name => _name;
    public DocumentTypes DocumentType => _documentType;
    public DateTime CreatedTime => _createdTime;
    public DateTime ChangedTime => _changedTime;
    public IRepositoryItem? Parent => _parent;

    public void SetName(string name)
    {
        if (_name != name)
        {
            DateTime timeNow = DateTime.UtcNow;

            using SqliteCommand command = _sqliteConnection.CreateCommand();

            command.CommandText = "UPDATE Documents SET Name=@Name, ChangedTime=@ChangedTime WHERE ID=@ID;";
            command.Parameters.AddWithValue("@Name", name);
            command.Parameters.AddWithValue("@ChangedTime", timeNow).SqliteType = SqliteType.Real;
            command.Parameters.AddWithValue("@ID", Id);

            command.ExecuteNonQuery();

            _name = name;
            _changedTime = timeNow.ToLocalTime();
            PropertyChanged?.Invoke(this, s_nameChangedEventArgs ??= new PropertyChangedEventArgs(nameof(Name)));
            PropertyChanged?.Invoke(this, s_changedTimeEventArgs ??= new PropertyChangedEventArgs(nameof(ChangedTime)));
        }
    }

    public Task SetNameAsync(string name)
    {
        return Task.Run(() => SetName(name));
    }

    public void SetDocumentType(DocumentTypes documentType)
    {
        if (_documentType != documentType)
        {
            DateTime timeNow = DateTime.UtcNow;

            using SqliteCommand command = _sqliteConnection.CreateCommand();

            command.CommandText = "UPDATE Documents SET DocumentType=@DocumentType, ChangedTime=@ChangedTime WHERE ID=@ID;";
            command.Parameters.AddWithValue("@DocumentType", (int)documentType);
            command.Parameters.AddWithValue("@ChangedTime", timeNow).SqliteType = SqliteType.Real;
            command.Parameters.AddWithValue("@ID", Id);

            command.ExecuteNonQuery();

            _documentType = documentType;
            _changedTime = timeNow.ToLocalTime();
            PropertyChanged?.Invoke(this, s_documentTypeChangedEventArgs ??= new PropertyChangedEventArgs(nameof(DocumentType)));
            PropertyChanged?.Invoke(this, s_changedTimeEventArgs ??= new PropertyChangedEventArgs(nameof(ChangedTime)));
        }
    }

    public Task SetDocumentTypeAsync(DocumentTypes documentType)
    {
        return Task.Run(() => SetDocumentType(documentType));
    }

    public IEnumerable<IRepositoryItem> GetChildren()
    {
        using SqliteCommand command = _sqliteConnection.CreateCommand();

        command.CommandText = "SELECT ID, Name, DocumentType, CreatedTime, ChangedTime FROM Documents WHERE Parent=@Parent ORDER BY Name;";
        command.Parameters.AddWithValue("@Parent", Id);

        using SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            yield return new RepositoryItem(reader.GetInt32(0), reader.GetString(1), (DocumentTypes)reader.GetInt32(2), reader.GetDateTime(3).ToLocalTime(), reader.GetDateTime(4).ToLocalTime(), this, _sqliteConnection);
        }
    }

    public async IAsyncEnumerable<IRepositoryItem> GetChildrenAsync()
    {
        IEnumerator<IRepositoryItem> enumerator = GetChildren().GetEnumerator();

        while (true)
        {
            if (await Task.Run(enumerator.MoveNext))
            {
                yield return enumerator.Current;
            }
            else
            {
                break;
            }
        }
    }

    public byte[] GetContent()
    {
        using SqliteCommand command = _sqliteConnection.CreateCommand();

        command.CommandText = "SELECT Content FROM Documents WHERE ID=@ID;";
        command.Parameters.AddWithValue("@ID", Id);

        using SqliteDataReader reader = command.ExecuteReader();

        if (reader.Read())
        {
            if (reader.IsDBNull(0))
            {
                return Array.Empty<byte>();
            }
            else
            {
                return (byte[])reader.GetValue(0);
            }
        }
        else
        {
            throw new InvalidOperationException($"Document with id: {Id} not exists.");
        }
    }

    public Task<byte[]> GetContentAsync()
    {
        return Task.Run(GetContent);
    }

    public void SetContent(byte[] content)
    {
        DateTime timeNow = DateTime.UtcNow;

        using SqliteCommand command = _sqliteConnection.CreateCommand();

        command.CommandText = "UPDATE Documents SET Content=@Content, ChangedTime=@ChangedTime WHERE ID=@ID;";
        command.Parameters.AddWithValue("@Content", content.Length > 0 ? content : DBNull.Value);
        command.Parameters.AddWithValue("@ChangedTime", timeNow).SqliteType = SqliteType.Real;
        command.Parameters.AddWithValue("@ID", Id);

        command.ExecuteNonQuery();

        _changedTime = timeNow.ToLocalTime();
        PropertyChanged?.Invoke(this, s_changedTimeEventArgs ??= new PropertyChangedEventArgs(nameof(ChangedTime)));
        ContentChanged?.Invoke(this, EventArgs.Empty);
    }

    public Task SetContentAsync(byte[] content)
    {
        return Task.Run(() => SetContent(content));
    }

    public IRepositoryItem CreateChild(string name, DocumentTypes documentType, byte[] content)
    {
        DateTime timeNow = DateTime.UtcNow;

        using SqliteCommand command = _sqliteConnection.CreateCommand();

        command.CommandText = "INSERT INTO Documents(Name, DocumentType, Content, CreatedTime, ChangedTime, Parent) VALUES (@Name, @DocumentType, @Content, @CreatedTime, @CreatedTime, @Parent);\nSELECT last_insert_rowid();";
        command.Parameters.AddWithValue("@Name", name);
        command.Parameters.AddWithValue("@DocumentType", (int)documentType);
        command.Parameters.AddWithValue("@Content", content.Length > 0 ? content : DBNull.Value);
        command.Parameters.AddWithValue("@CreatedTime", timeNow).SqliteType = SqliteType.Real;
        command.Parameters.AddWithValue("@Parent", Id);

        int id = (int)(long)command.ExecuteScalar()!;

        return new RepositoryItem(id, name, documentType, timeNow.ToLocalTime(), timeNow.ToLocalTime(), this, _sqliteConnection);
    }

    public Task<IRepositoryItem> CreateChildAsync(string name, DocumentTypes documentType, byte[] content)
    {
        return Task.Run(() => CreateChild(name, documentType, content));
    }

    private void DeleteChilds(RepositoryItem child)
    {
        foreach (RepositoryItem subChild in child.GetChildren().Cast<RepositoryItem>())
        {
            subChild.DeleteChilds(subChild);
        }

        using SqliteCommand command = _sqliteConnection.CreateCommand();

        command.CommandText = "DELETE FROM Documents WHERE ID=@ID;";
        command.Parameters.AddWithValue("@ID", child.Id);

        command.ExecuteNonQuery();
    }

    public void DeleteChild(IRepositoryItem child)
    {
        using SqliteTransaction transaction = _sqliteConnection.BeginTransaction();
        DeleteChilds((RepositoryItem)child);

        transaction.Commit();
    }

    public Task DeleteChildAsync(IRepositoryItem child)
    {
        return Task.Run(() => DeleteChild(child));
    }

    public event EventHandler? ContentChanged;

    public event PropertyChangedEventHandler? PropertyChanged;
}
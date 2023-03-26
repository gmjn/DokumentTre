namespace DocumentRepository;

public interface IRepositoryItem
{
    public int Id { get; }
    public string Name { get; }
    public DocumentTypes DocumentType { get; }
    public IRepositoryItem? Parent { get; }

    public void SetName(string name);
    public Task SetNameAsync(string name);

    public void SetDocumentType(DocumentTypes documentType);
    public Task SetDocumentTypeAsync(DocumentTypes documentType);

    public byte[] GetContent();
    public Task<byte[]> GetContentAsync();

    public void SetContent(byte[] content);
    public Task SetContentAsync(byte[] content);

    public IEnumerable<IRepositoryItem> GetChildren();
    public IAsyncEnumerable<IRepositoryItem> GetChildrenAsync();

    public IRepositoryItem CreateChild(string name, DocumentTypes documentType, byte[] content);
    public Task<IRepositoryItem> CreateChildAsync(string name, DocumentTypes documentType, byte[] content);

    public void DeleteChild(IRepositoryItem child);
    public Task DeleteChildAsync(IRepositoryItem child);

    public event EventHandler? ContentChanged;
}
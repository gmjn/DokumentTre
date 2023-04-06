namespace DokumentTre.Factory;

public interface IFactory
{
    public T Create<T>() where T : class;
    public T? TryCreate<T>() where T : class;
}
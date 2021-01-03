namespace FluffyBunny4.DotNetCore.Services
{
    public interface IScopedContext<T> where T : class
    {
        T Context { get; }
    }
}
namespace FluffyBunny4.DotNetCore.Services
{
    public class ScopedContext<T> : IScopedContext<T> where T : class
    {
        private T _context;
        public ScopedContext(T context)
        {
            _context = context;
        }
        public T Context => _context;
    }
}
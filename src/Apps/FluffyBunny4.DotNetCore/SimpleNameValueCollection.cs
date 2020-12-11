using System.Collections.Generic;

namespace FluffyBunny4.DotNetCore
{
    public class SimpleNameValueCollection : Dictionary<string, string>
    {
        public KeyCollection AllKeys
        {
            get
            {
                return Keys;
            }
        }
        public string Get(string name)
        {
            string value = null;
            if (TryGetValue(name, out value))
            {
                return value;
            }
            return null;

        }
    }
}
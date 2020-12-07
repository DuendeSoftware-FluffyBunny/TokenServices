using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny4.Services.Default
{
    public class HashFixer : IHashFixer
    {
        public string FixHash(string hash)
        {
            // COSMOS
            // >> The following characters are restricted and cannot be used in the Id property: '/', '\\', '?', '#'
            hash = hash.Replace('/', '_');
            hash = hash.Replace('-', '+');
            return hash;
        }
    }
}

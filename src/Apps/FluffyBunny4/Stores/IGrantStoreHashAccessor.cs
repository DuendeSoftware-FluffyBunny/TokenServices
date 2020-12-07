using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluffyBunny4.Stores
{
    public interface IGrantStoreHashAccessor
    {
        string GetHashedKey(string value);
    }
}

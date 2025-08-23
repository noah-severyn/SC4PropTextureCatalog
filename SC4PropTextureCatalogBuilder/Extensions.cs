using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SC4PropTextureCatalogBuilder {
    public static class Extensions {
        public static bool ContainsAny(this string stringToCheck, params string[] parameters) {
            return parameters.Any(parameter => stringToCheck.Contains(parameter));
        }
    }
}

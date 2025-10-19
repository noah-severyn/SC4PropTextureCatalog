

namespace SC4PropTextureCatalogBuilder {
    public static class Extensions {
        public static bool ContainsAny(this string stringToCheck, params string[] parameters) {
            return parameters.Any(parameter => stringToCheck.Contains(parameter));
        }

        /// <summary>
        /// Returns a list of distinct file names, even if they may be stored in differing folders
        /// </summary>
        public static List<string> GetUniqueFilenamesAcrossFolders(this IEnumerable<string> filePaths) {
            Dictionary<string, string> uniques = [];
            foreach (string file in filePaths) {
                string fileName = Path.GetFileName(file);
                uniques.TryAdd(fileName, file);
            }
            return uniques.Values.ToList();
        }
    }
}

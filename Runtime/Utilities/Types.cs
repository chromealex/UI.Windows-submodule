
namespace UnityEngine.UI.Windows.Utilities {

    [System.Serializable]
    public struct UIWSLayer {

        public int value;

    }

    public static class TypesCache {

        public static readonly System.Collections.Generic.Dictionary<System.Type, string> cacheFullName = new System.Collections.Generic.Dictionary<System.Type, string>();

        public static string GetFullName(System.Type type) {
            if (cacheFullName.TryGetValue(type, out var result) == false) {
                cacheFullName.Add(type, result = type.FullName);
            }

            return result;
        }

    }
    
}
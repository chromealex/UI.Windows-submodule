using UnityEngine;
using UnityEditor;

public static class AddressableEditorExtension {

    /// <summary>
    /// Set Addressables Key/ID of an gameObject.
    /// </summary>
    /// <param name="gameObject">GameObject to set Key/ID</param>
    /// <param name="id">Key/ID</param>
    public static void SetAddressableID(this GameObject gameObject, string id, UnityEditor.AddressableAssets.Settings.AddressableAssetGroup group = null) {
        AddressableEditorExtension.SetAddressableID(gameObject as Object, id, group);
    }

    /// <summary>
    /// Set Addressables Key/ID of an object.
    /// </summary>
    /// <param name="o">Object to set Key/ID</param>
    /// <param name="id">Key/ID</param>
    public static void SetAddressableID(this Object o, string id, UnityEditor.AddressableAssets.Settings.AddressableAssetGroup group = null) {
        if (id.Length == 0) {
            Debug.LogWarning($"Can not set an empty adressables ID.");
        }

        var entry = AddressableEditorExtension.GetAddressableAssetEntry(o, createNew: group != null, group: group);
        if (entry != null) {
            entry.address = id;
        }
    }

    /// <summary>
    /// Get Addressables Key/ID of an gameObject.
    /// </summary>
    /// <param name="gameObject">gameObject to recive addressables Key/ID</param>
    /// <returns>Addressables Key/ID</returns>
    public static string GetAddressableID(this GameObject gameObject) {
        return AddressableEditorExtension.GetAddressableID(gameObject as Object);
    }

    /// <summary>
    /// Get Addressables Key/ID of an object.
    /// </summary>
    /// <param name="o">object to recive addressables Key/ID</param>
    /// <returns>Addressables Key/ID</returns>
    public static string GetAddressableID(this Object o) {
        var entry = AddressableEditorExtension.GetAddressableAssetEntry(o);
        if (entry != null) {
            return entry.address;
        }

        return "";
    }

    /// <summary>
    /// Get addressable asset entry of an object.
    /// </summary>
    /// <param name="o">>object to recive addressable asset entry</param>
    /// <returns>addressable asset entry</returns>
    public static UnityEditor.AddressableAssets.Settings.AddressableAssetEntry GetAddressableAssetEntry(Object o, bool createNew = true,
                                                                                                        UnityEditor.AddressableAssets.Settings.AddressableAssetGroup group = null) {
        var aaSettings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;

        UnityEditor.AddressableAssets.Settings.AddressableAssetEntry entry = null;
        var guid = string.Empty;
        long localID = 0;
        string path;

        var foundAsset = AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o, out guid, out localID);
        path = AssetDatabase.GUIDToAssetPath(guid);

        if (foundAsset && path.ToLower().Contains("assets")) {
            if (aaSettings != null) {
                entry = aaSettings.FindAssetEntry(guid);
            }
        }

        if (createNew == true) {

            group = group == null ? aaSettings.DefaultGroup : group;
            if (group != null) {
                entry = aaSettings.CreateOrMoveEntry(guid, group, group.ReadOnly);
                return entry;
            }

        }

        if (entry != null) {
            return entry;
        }

        return null;
    }

}
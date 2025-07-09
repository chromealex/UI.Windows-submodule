using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class ScriptTemplates {

    internal class DoCreateScriptAsset : UnityEditor.ProjectWindowCallback.EndNameEditAction {

        private System.Action<Object> onCreated;
        
        private static UnityEngine.Object CreateScriptAssetFromTemplate(string pathName, string resourceFile) {

            var str1 = resourceFile.Replace("#NOTRIM#", "");
            var withoutExtension = System.IO.Path.GetFileNameWithoutExtension(pathName);
            var str2 = str1.Replace("#NAME#", withoutExtension);
            var str3 = withoutExtension.Replace(" ", "");
            var str4 = str2.Replace("#SCRIPTNAME#", str3);
            string templateContent;
            if (char.IsUpper(str3, 0)) {
                var newValue = char.ToLower(str3[0]).ToString() + str3.Substring(1);
                templateContent = str4.Replace("#SCRIPTNAME_LOWER#", newValue);
            } else {
                var newValue = "my" + (object)char.ToUpper(str3[0]) + str3.Substring(1);
                templateContent = str4.Replace("#SCRIPTNAME_LOWER#", newValue);
            }

            return DoCreateScriptAsset.CreateScriptAssetWithContent(pathName, templateContent);

        }

        public UnityEditor.ProjectWindowCallback.EndNameEditAction SetCallback(System.Action<Object> onCreated) {

            this.onCreated = onCreated;
            return this;

        }

        private static string SetLineEndings(string content, LineEndingsMode lineEndingsMode) {

            string replacement;
            switch (lineEndingsMode) {
                case LineEndingsMode.OSNative:
                    replacement = Application.platform != RuntimePlatform.WindowsEditor ? "\n" : "\r\n";
                    break;

                case LineEndingsMode.Unix:
                    replacement = "\n";
                    break;

                case LineEndingsMode.Windows:
                    replacement = "\r\n";
                    break;

                default:
                    replacement = "\n";
                    break;
            }

            content = System.Text.RegularExpressions.Regex.Replace(content, "\\r\\n?|\\n", replacement);
            return content;

        }

        private static UnityEngine.Object CreateScriptAssetWithContent(string pathName, string templateContent) {

            templateContent = DoCreateScriptAsset.SetLineEndings(templateContent, EditorSettings.lineEndingsForNewScripts);
            var fullPath = System.IO.Path.GetFullPath(pathName);
            var utF8Encoding = new System.Text.UTF8Encoding(true);
            System.IO.File.WriteAllText(fullPath, templateContent, (System.Text.Encoding)utF8Encoding);
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));

        }

        public override void Action(int instanceId, string pathName, string resourceFile) {

            var instance = DoCreateScriptAsset.CreateScriptAssetFromTemplate(pathName, resourceFile);
            ProjectWindowUtil.ShowCreatedAsset(instance);
            if (this.onCreated != null) this.onCreated.Invoke(instance);

        }

    }

    private static Texture2D scriptIcon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;

    internal static void Create(string fileName, string templateName, System.Collections.Generic.Dictionary<string, string> customDefines = null, bool allowRename = true, System.Action<Object> onCreated = null) {

        var obj = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(obj);
        if (System.IO.File.Exists(path) == true) {
            path = System.IO.Path.GetDirectoryName(path);
        }

        if (string.IsNullOrEmpty(path) == true) {
            path = "Assets/";
        }
        
        ScriptTemplates.Create(path, fileName, templateName, customDefines, allowRename, onCreated);
        
    }
    
    internal static void Create(string path, string fileName, string templateName, System.Collections.Generic.Dictionary<string, string> customDefines = null, bool allowRename = true, System.Action<Object> onCreated = null) {

        var stateTypeStr = "StateClassType";
        
        var templatePath = Resources.Load<TextAsset>(templateName);
        if (templatePath == null) {
            
            Debug.LogError("Template was not found at path " + templateName);
            return;

        }

        var content = templatePath.text;
        if (customDefines != null) {

            foreach (var def in customDefines) {

                content = content.Replace("#" + def.Key + "#", def.Value);

            }

        }
        
        var @namespace = path.Replace("Assets/", "").Replace("/", ".");
        content = content.Replace(@"#NAMESPACE#", @namespace);
        content = content.Replace(@"#PROJECTNAME#", @namespace.Split('.')[0]);
        content = content.Replace(@"#STATENAME#", stateTypeStr);
        content = content.Replace(@"#REFERENCES#", string.Empty);

        if (allowRename == true) {

            var defaultNewFileName = fileName;
            var image = ScriptTemplates.scriptIcon;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
        0,
        ScriptableObject.CreateInstance<DoCreateScriptAsset>().SetCallback((instance) => {
         
                    if (onCreated != null) onCreated.Invoke(instance);

                }),
                defaultNewFileName,
                image,
                content);

        } else {

            var fullDir = path + "/" + fileName;
            if (System.IO.File.Exists(fullDir) == true) {

                var contentExists = System.IO.File.ReadAllText(fullDir);
                if (contentExists == content) return;

            }
            
            var withoutExtension = System.IO.Path.GetFileNameWithoutExtension(fullDir);
            withoutExtension = withoutExtension.Replace(" ", "");
            content = content.Replace("#SCRIPTNAME#", withoutExtension);

            var dir = System.IO.Path.GetDirectoryName(fullDir);
            if (System.IO.Directory.Exists(dir) == false) return;
            
            System.IO.File.WriteAllText(fullDir, content);
            AssetDatabase.ImportAsset(fullDir, ImportAssetOptions.ForceSynchronousImport);

            if (onCreated != null) onCreated.Invoke(AssetDatabase.LoadAssetAtPath<Object>(fullDir));
            
        }

    }

    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded() {

        ScriptTemplates.CreateScreen_AfterCompilation();

    }

    private static void CreateScreen_AfterCompilation() {
            
        var waitForCompilation = EditorPrefs.GetBool("Temp.EditorWaitCompilation.UIWindows.CreateScreen");
        if (waitForCompilation == true) {
                
            EditorPrefs.DeleteKey("Temp.EditorWaitCompilation.UIWindows.CreateScreen");
                
            var dir = EditorPrefs.GetString("Temp.EditorWaitCompilation.UIWindows.CreateScreen.Dir");
            var assetName = EditorPrefs.GetString("Temp.EditorWaitCompilation.UIWindows.CreateScreen.Name");
            var assetNameOrig = EditorPrefs.GetString("Temp.EditorWaitCompilation.UIWindows.CreateScreen.Name.Orig");
            var assetPath = EditorPrefs.GetString("Temp.EditorWaitCompilation.UIWindows.CreateScreen.ScriptPath");
            var newAssetPath = EditorPrefs.GetString("Temp.EditorWaitCompilation.UIWindows.CreateScreen.NewScriptPath");

            if (assetNameOrig.EndsWith("Screen") == false) {

                var content = System.IO.File.ReadAllText(assetPath);
                content = content.Replace(@"/*#SCRIPTNAME_POST#*/", "Screen");
                System.IO.File.WriteAllText(assetPath, content);

            } else {

                var content = System.IO.File.ReadAllText(assetPath);
                content = content.Replace(@"/*#SCRIPTNAME_POST#*/", "");
                System.IO.File.WriteAllText(assetPath, content);

            }

            if (assetPath != newAssetPath) {
                AssetDatabase.MoveAsset(assetPath, newAssetPath);
                AssetDatabase.ImportAsset(newAssetPath, ImportAssetOptions.ForceSynchronousImport);
                EditorPrefs.SetBool("Temp.EditorWaitCompilation.UIWindows.CreateScreen", true);
                EditorPrefs.SetString("Temp.EditorWaitCompilation.UIWindows.CreateScreen.Name.Orig", System.IO.Path.GetFileNameWithoutExtension(newAssetPath));
                EditorPrefs.SetString("Temp.EditorWaitCompilation.UIWindows.CreateScreen.ScriptPath", newAssetPath);
                return;
            }
            
            var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(newAssetPath);
            ScriptTemplates.AssignIcon(asset, new GUIContent(Resources.Load<Texture>("EditorAssets/Scripts/window_icon")));

            //var asset = AssetDatabase.LoadAssetAtPath<MonoScript>(newAssetPath);
            var guid = AssetDatabase.AssetPathToGUID(newAssetPath);
            //AssetDatabase.TryGetGUIDAndLocalFileIdentifier(asset, out var guid, out long localId);
            var defs = new Dictionary<string, string>() {
                { "GUID", guid },
            };
            
            ScriptTemplates.CreateEmptyDirectory(dir, "Screens");
            ScriptTemplates.CreateEmptyDirectory(dir, "Components");
            ScriptTemplates.CreateEmptyDirectory(dir, "Layouts");

            #if UNITY_URP
            var templateName = "61-ScreenAsset-URP";
            #else
            var templateName = "61-ScreenAsset";
            #endif
            ScriptTemplates.Create(dir + "/Screens", assetName + "Screen.prefab", templateName, allowRename: false, customDefines: defs);

        }
            
    }

    internal static void CreateEmptyDirectory(string path, string dir) {

        var fullDir = path + "/" + dir;
        System.IO.Directory.CreateDirectory(fullDir);
        System.IO.File.WriteAllText(fullDir + "/.dummy", string.Empty);
        AssetDatabase.ImportAsset(fullDir);

    }

    private static string GetDirectoryFromAsset(Object obj) {
            
        var path = AssetDatabase.GetAssetPath(obj);
        if (System.IO.File.Exists(path) == true) {
            path = System.IO.Path.GetDirectoryName(path);
        }

        if (string.IsNullOrEmpty(path) == true) {
            path = "Assets";
        }

        return path;

    }

    [UnityEditor.MenuItem("Assets/Create/UI.Windows/Create Screen")]
    public static void CreateScreen() {
        
        ScriptTemplates.Create("New Screen.cs", "01-ScreenTemplate", onCreated: (asset) => {

            var path = ScriptTemplates.GetDirectoryFromAsset(asset);
            var assetName = asset.name;
            if (assetName.EndsWith("Screen") == true) assetName = assetName.Replace("Screen", string.Empty);
            ScriptTemplates.CreateEmptyDirectory(path, assetName);
            var dir = path + "/" + assetName;
            var newAssetPath = dir + "/" + assetName + "Screen.cs";

            EditorPrefs.SetBool("Temp.EditorWaitCompilation.UIWindows.CreateScreen", true);
            EditorPrefs.SetString("Temp.EditorWaitCompilation.UIWindows.CreateScreen.Dir", dir);
            EditorPrefs.SetString("Temp.EditorWaitCompilation.UIWindows.CreateScreen.Name", assetName);
            EditorPrefs.SetString("Temp.EditorWaitCompilation.UIWindows.CreateScreen.Name.Orig", asset.name);
            EditorPrefs.SetString("Temp.EditorWaitCompilation.UIWindows.CreateScreen.ScriptPath", AssetDatabase.GetAssetPath(asset));
            EditorPrefs.SetString("Temp.EditorWaitCompilation.UIWindows.CreateScreen.NewScriptPath", newAssetPath);
            
        });

    }
    
    static void AssignIcon(Object target, GUIContent icon){
        if (target == null || icon == null)
            throw new System.ArgumentNullException ();
 
        Texture2D tex = icon.image as Texture2D;
        if (tex == null) {
            Debug.LogError ("Invalid Icon format : Not a Texture2D");
            return;
        }
        
        EditorGUIUtility.SetIconForObject(target, tex);
        
    }
    
}

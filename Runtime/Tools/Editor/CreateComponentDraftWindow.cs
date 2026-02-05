using System.Linq;

#if UNITY_EDITOR
namespace UnityEditor.UI.Windows {

    using UnityEditor;
    using UnityEngine;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;
    using UnityEngine.UI.Windows;
    using UnityEngine.ResourceManagement.Util;
    using UnityEngine.UI.Windows.Utilities;

    public class CreateComponentDraftWindow : EditorWindow {

        public class TempObj : ScriptableObject {

            public Item[] items = new Item[1];

        }
        
        [System.Serializable]
        public struct Item {

            public string fieldName;
            [SearchComponentsByTypePopup(typeof(WindowComponent), menuName: "Type", singleOnly: true)]
            public SerializedType type;

        }

        private GameObject gameObject;
        private string path;
        private string namespaceRoot;
        private string screenName;

        private TempObj tempObj;

        [MenuItem("Tools/My Tool Window")]
        public static void Open(GameObject go) {
            var path = string.Empty;
            var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
            if (stage.scene != go.scene) {
                if (PrefabUtility.IsPartOfPrefabAsset(go) == false) {
                    Debug.LogError("Select a prefab asset");
                    return;
                }
                path = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(go));
            } else {
                path = stage.assetPath;
            }
            
            var instance = CreateInstance<CreateComponentDraftWindow>();
            instance.name = "UIWS: Create Component Tool";
            instance.gameObject = go;
            instance.path = path.Replace("\\", "/");
            var pathParts = instance.path.Split('/');
            
            var screenName = string.Empty;
            var idx = 2;
            if (pathParts.Length > 2) {
                screenName = pathParts[pathParts.Length - 2];
                if (screenName == "Layouts" || screenName == "Components" || screenName == "Screens") {
                    screenName = pathParts[pathParts.Length - 3];
                    idx = 3;
                }
            }

            instance.namespaceRoot = string.Join(".", pathParts, 1, pathParts.Length - idx);
            instance.screenName = screenName;

            instance.tempObj = ScriptableObject.CreateInstance<TempObj>();
            
            Debug.Log(instance.namespaceRoot + " :: " + screenName + " :: " + instance.path);
            
            //instance.ShowModal();
            instance.Show();

        }

        private void OnDestroy() {
            if (this.tempObj != null) {
                DestroyImmediate(this.tempObj);
                this.tempObj = null;
            }
        }

        private void CreateGUI() {
            
            var root = new VisualElement();
            this.rootVisualElement.Add(root);
            
            var error = new Label(string.Empty);
            root.Add(error);

            var title = new TextField("Component Name");
            root.Add(title);
            
            var nmsp = new TextField("Namespace");
            this.namespaceField = nmsp;
            root.Add(nmsp);

            var screenName = new TextField("Screen Name");
            this.screenNameField = screenName;
            root.Add(screenName);

            var path = new TextField("Path");
            this.pathField = path;
            root.Add(path);
            
            this.UpdateFields();

            var editor = Editor.CreateEditor(this.tempObj);
            var gui = new IMGUIContainer(() => {
                editor.OnInspectorGUI();
            });
            root.Add(gui);
            
            var button = new Button(() => {
                if (this.CheckFields(title.text, out var err) == false) {
                    error.text = err;
                    return;
                }

                this.namespaceRoot = nmsp.text;
                this.screenName = screenName.text;
                this.path = path.text;
                Generate(this, title.text, this.tempObj.items, this.gameObject);
            });
            
            button.text = "Create Component";
            root.Add(button);
        }

        private TextField namespaceField;
        private TextField screenNameField;
        private TextField pathField;

        public void UpdateFields() {
            if (this.namespaceField == null) return;
            this.namespaceField.value = this.namespaceRoot;
            this.screenNameField.value = this.screenName;
            this.pathField.value = this.path;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        public static void OnScriptsReloaded() {
            var path = EditorPrefs.GetString("UI.Windows.Editor.ComponentTemplate.path");
            if (string.IsNullOrEmpty(path) == false) {
                var componentName = EditorPrefs.GetString("UI.Windows.Editor.ComponentTemplate.name");
                var @namespace = EditorPrefs.GetString("UI.Windows.Editor.ComponentTemplate.namespace");
                var instanceId = EditorPrefs.GetInt("UI.Windows.Editor.ComponentTemplate.gameObject");
                var go = (GameObject)EditorUtility.InstanceIDToObject(instanceId);
                go.AddComponent(System.Type.GetType($"{@namespace}.{componentName}"));
            }
            EditorPrefs.DeleteKey("UI.Windows.Editor.ComponentTemplate.path");
        }

        private static void Generate(CreateComponentDraftWindow window, string componentName, Item[] items, GameObject go) {
            
            var tpl = Resources.Load<TextAsset>("UI.Windows/ComponentTemplate-Custom");
            if (tpl == null) tpl = Resources.Load<TextAsset>("UI.Windows/ComponentTemplate");

            if (tpl != null) {
                var text = tpl.text;
                text = text.Replace("{{NAMESPACE_ROOT}}", window.namespaceRoot);
                text = text.Replace("{{SCREEN_NAME}}", window.screenName);
                text = text.Replace("{{COMPONENT_NANE}}", componentName);

                var itemsText = string.Empty;
                for (int i = 0; i < items.Length; ++i) {
                    var item = items[i];
                    itemsText += $"public {item.type.Value.Name} {item.fieldName}\n";
                }
                text = text.Replace("{{ITEMS}}", itemsText);
                
                var path = $"{System.IO.Path.GetDirectoryName(window.path)}/{componentName}.cs";
                System.IO.File.WriteAllText(path, text);
                AssetDatabase.ImportAsset(path);
                EditorPrefs.SetString("UI.Windows.Editor.ComponentTemplate.path", path);
                EditorPrefs.SetString("UI.Windows.Editor.ComponentTemplate.name", componentName);
                EditorPrefs.SetInt("UI.Windows.Editor.ComponentTemplate.gameObject", go.GetInstanceID());
                EditorPrefs.SetString("UI.Windows.Editor.ComponentTemplate.namespace", $"{window.namespaceRoot}.{window.screenName}");
            }
            
        }

        private bool CheckFields(string componentName, out string err) {

            err = null;
            
            // component name
            if (IsFieldNameValid(componentName) == false) {
                err = $"Field `{componentName}` is not a valid class name";
                return false;
            }
            
            // unique check
            var count = this.tempObj.items.Select(x => x.fieldName).Distinct().Count();
            if (count != this.tempObj.items.Length) {
                err = "Field must be with unique names";
                return false;
            }

            // names check
            foreach (var item in this.tempObj.items) {
                if (IsFieldNameValid(item.fieldName) == false) {
                    err = $"Field `{item.fieldName}` is not a valid field name";
                    return false;
                }
            }

            return true;

            static bool IsFieldNameValid(string fieldName) {
                if (string.IsNullOrEmpty(fieldName)) return false;
                return System.Text.RegularExpressions.Regex.IsMatch(fieldName, @"^[_a-zA-Z][_a-zA-Z0-9]*$");
            }
            
        }

    }

}
#endif
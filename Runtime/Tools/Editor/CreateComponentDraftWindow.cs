using System.Linq;
using System.IO;

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

        private const string PRESETS_PATH = "Assets/EditorResources/UI.Windows/AddComponentDraftPresets.asset";

        public class TempObj : ScriptableObject {

            public Item[] items = new Item[1];

        }
        
        private int gameObject;
        private string path;
        private string namespaceRoot;
        private string screenName;

        private TempObj tempObj;

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
            instance.titleContent = new GUIContent("UIWS: Create Draft Component Tool");
            instance.gameObject = go.GetInstanceID();
            instance.path = GetComponentsPath(path, out var screenPath, out var screenName);

            instance.namespaceRoot = screenPath.Replace("Assets/", string.Empty).Replace("/", ".").Replace("UIScreens", "Screens");
            instance.screenName = screenName;

            instance.tempObj = ScriptableObject.CreateInstance<TempObj>();
            
            instance.Show();
            
        }

        private void OnDestroy() {
            if (this.tempObj != null) {
                DestroyImmediate(this.tempObj);
                this.tempObj = null;
            }
        }

        private void Update() {
            if (this.tempObj == null) {
                this.Close();
                return;
            }
            if (Event.current == null) return;
            if (Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyDown) {
                this.Close();
            }
        }

        private void CreateGUI() {
            
            var root = new VisualElement();
            this.root = root;
            root.styleSheets.Add(Resources.Load<StyleSheet>("UI.Windows/AddComponentDraftStyle"));
            this.rootVisualElement.Add(root);
            
            var error = new Label(string.Empty);
            error.AddToClassList("error");
            root.Add(error);

            var title = new TextField("Component Name");
            title.AddToClassList("component-name-field");
            title.AddToClassList("text-field");
            root.Add(title);
            
            var nmsp = new TextField("Namespace");
            nmsp.AddToClassList("text-field");
            this.namespaceField = nmsp;
            root.Add(nmsp);

            var screenName = new TextField("Screen Name");
            screenName.AddToClassList("text-field");
            this.screenNameField = screenName;
            root.Add(screenName);

            var path = new TextField("Path");
            path.AddToClassList("text-field");
            this.pathField = path;
            root.Add(path);
            
            this.UpdateFields();

            this.ReDrawPresets(root);
            
            var editor = Editor.CreateEditor(this.tempObj);
            var gui = new IMGUIContainer(() => {
                editor?.DrawDefaultInspector();
            });
            gui.AddToClassList("gui-container");
            root.Add(gui);
            
            var savePresetButton = new Button(() => {
                error.style.display = DisplayStyle.None;
                if (this.CheckFields(out var err) == false) {
                    error.text = err;
                    error.style.display = DisplayStyle.Flex;
                    return;
                }
                SavePreset(this, this.tempObj.items);
                this.ReDrawPresets(root);
            });
            savePresetButton.text = "Save Preset";
            savePresetButton.AddToClassList("save-preset-button");
            root.Add(savePresetButton);
            
            var createButton = new Button(() => {
                error.style.display = DisplayStyle.None;
                if (this.Check(title.text, out var err) == false) {
                    error.text = err;
                    error.style.display = DisplayStyle.Flex;
                    return;
                }

                this.namespaceRoot = nmsp.text;
                this.screenName = screenName.text;
                this.path = path.text;
                Generate(this, title.text, this.tempObj.items, this.gameObject);
                this.Close();
            });
            createButton.AddToClassList("create-button");
            createButton.text = "Create Component";
            root.Add(createButton);
            
        }

        private Foldout presetsContainer;
        private VisualElement lastPresets;
        private VisualElement ReDrawPresets(VisualElement root) {
            if (this.presetsContainer == null) {
                this.presetsContainer = new Foldout();
                this.presetsContainer.AddToClassList("presets-container");
                root.Add(this.presetsContainer);
            }

            var presets = this.lastPresets;
            if (presets != null) this.presetsContainer.Remove(presets);
            presets = this.DrawPresets(out var count);
            this.presetsContainer.text = $"Presets ({count})";
            if (presets != null) this.presetsContainer.Add(presets);
            this.lastPresets = presets;
            return presets;
        }

        private VisualElement DrawPresets(out int count) {
            var presets = this.GetPresets();
            if (presets.presets != null) {
                count = presets.presets.Count;
                var presetsList = new ListView(presets.presets, makeItem: () => {
                    var container = new VisualElement();
                    container.AddToClassList("preset-item");
                    var lbl = new Label();
                    lbl.AddToClassList("text");
                    container.Add(lbl);
                    var addButton = new Button();
                    addButton.text = "Use Preset";
                    addButton.AddToClassList("button-add");
                    container.Add(addButton);
                    var deleteButton = new Button();
                    deleteButton.text = "Delete Preset";
                    deleteButton.AddToClassList("button-delete");
                    container.Add(deleteButton);
                    return container;
                }, bindItem: (obj, index) => {
                    var data = presets.presets[index];
                    var text = obj.Q<Label>();
                    text.text = data.ToString();
                    
                    var useButton = obj.Q<Button>(className: "button-add");
                    useButton.clicked += () => {
                        this.ApplyPreset(data);
                    };
                    
                    var deleteButton = obj.Q<Button>(className: "button-delete");
                    deleteButton.clicked += () => {
                        if (EditorUtility.DisplayDialog("Delete Preset", "Are you sure?", "Yes", "No") == true) {
                            this.DeletePreset(index);
                        }
                    };
                });
                presetsList.virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                presetsList.AddToClassList("presets-list");
                return presetsList;
            } else {
                count = 0;
            }

            return null;
        }

        private void DeletePreset(int index) {
            var presets = this.GetPresets();
            presets.presets.RemoveAt(index);
            this.ReDrawPresets(this.root);
        }

        private void ApplyPreset(PresetsData.Preset data) {
            this.tempObj.items = new Item[data.items.Length];
            System.Array.Copy(data.items, this.tempObj.items, data.items.Length);
        }

        public PresetsData GetPresets() {
            var presets = AssetDatabase.LoadAssetAtPath<AddComponentDraftPresets>(PRESETS_PATH);
            return presets?.data ?? default;
        }

        private static void SavePreset(CreateComponentDraftWindow window, Item[] items) {

            var presets = AssetDatabase.LoadAssetAtPath<AddComponentDraftPresets>(PRESETS_PATH);
            if (presets == null) {
                presets = CreateInstance<AddComponentDraftPresets>();
                presets.data = new PresetsData() {
                    presets = new System.Collections.Generic.List<PresetsData.Preset>(),
                };
                AssetDatabase.CreateAsset(presets, PRESETS_PATH);
                presets = AssetDatabase.LoadAssetAtPath<AddComponentDraftPresets>(PRESETS_PATH);
            }
            var data = presets.data;
            data.presets.Add(new PresetsData.Preset() {
                items = items,
            });
            presets.data = data;
            EditorUtility.SetDirty(presets);

        }

        private TextField namespaceField;
        private TextField screenNameField;
        private TextField pathField;
        private VisualElement root;

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
                var type = System.AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetTypes().FirstOrDefault(x => x.FullName.Contains($"{@namespace}.{componentName}"))).Where(x => x != null).FirstOrDefault();
                if (type != null) {
                    go.AddComponent(type);
                } else {
                    Debug.LogWarning($"Type was not found in assemblies: {@namespace}.{componentName}");
                }
            }
            EditorPrefs.DeleteKey("UI.Windows.Editor.ComponentTemplate.path");
        }

        private static void Generate(CreateComponentDraftWindow window, string componentName, Item[] items, int go) {

            if (componentName.EndsWith("Component") == false) {
                componentName += "Component";
            }
            
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
                    itemsText += $"public {item.type.Value.Name} {item.fieldName};\n";
                }
                text = text.Replace("{{ITEMS}}", itemsText);
                text = FormatIndentation(text);
                
                var path = $"{System.IO.Path.GetDirectoryName(window.path)}/Components/{componentName}.cs";
                System.IO.File.WriteAllText(path, text);
                AssetDatabase.ImportAsset(path);
                EditorPrefs.SetString("UI.Windows.Editor.ComponentTemplate.path", path);
                EditorPrefs.SetString("UI.Windows.Editor.ComponentTemplate.name", componentName);
                EditorPrefs.SetInt("UI.Windows.Editor.ComponentTemplate.gameObject", go);
                EditorPrefs.SetString("UI.Windows.Editor.ComponentTemplate.namespace", window.namespaceRoot);
            }
            
        }
        
        public static string GetComponentsPath(string anyPath, out string screenPath, out string screenName) {
            screenPath = null;
            screenName = null;

            if (string.IsNullOrEmpty(anyPath) == true) return null;

            anyPath = anyPath.Replace("\\", "/");

            var dir = new DirectoryInfo(anyPath);
            if (File.Exists(anyPath) == true) dir = dir.Parent;

            while (dir != null) {
                if (dir.Name == "Components") {
                    screenPath = ToUnityPath(dir.Parent.FullName);
                    screenName = dir.Parent.Name;
                    return ToUnityPath(dir.FullName);
                }

                if (dir.Name == "Layouts" || dir.Name == "Screens") {
                    var screenDir = dir.Parent;
                    var components = Path.Combine(screenDir.FullName, "Components");

                    screenPath = ToUnityPath(screenDir.FullName);
                    screenName = screenDir.Name;
                    return ToUnityPath(components);
                }

                var componentsDir = Path.Combine(dir.FullName, "Components");
                if (Directory.Exists(componentsDir) == true) {
                    screenPath = ToUnityPath(dir.FullName);
                    screenName = dir.Name;
                    return ToUnityPath(componentsDir);
                }

                dir = dir.Parent;
            }

            return null;
        }
        
        private static string ToUnityPath(string fullPath) {
            fullPath = fullPath.Replace("\\", "/");
            var dataPath = Application.dataPath.Replace("\\", "/");
            if (fullPath.StartsWith(dataPath)) return $"Assets{fullPath.Substring(dataPath.Length)}";
            return fullPath;
        }

        private bool Check(string componentName, out string err) {
            
            err = null;

            // component name
            if (IsFieldNameValid(componentName) == false) {
                err = $"Field `{componentName}` is not a valid class name";
                return false;
            }

            return true;

        }
        
        private bool CheckFields(out string err) {

            err = null;
            
            // unique check
            var count = this.tempObj.items.Select(x => x.fieldName).Distinct().Count();
            if (count != this.tempObj.items.Length) {
                err = "Fields must be with unique names";
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

        }
        
        private static bool IsFieldNameValid(string fieldName) {
            if (string.IsNullOrEmpty(fieldName)) return false;
            return System.Text.RegularExpressions.Regex.IsMatch(fieldName, @"^[_a-zA-Z][_a-zA-Z0-9]*$");
        }

        public static string FormatIndentation(string code, int indentSize = 4) {
            var sb = new System.Text.StringBuilder();
            int indentLevel = 0;

            var lines = code.Replace("\r\n", "\n").Split('\n');

            foreach (var rawLine in lines) {
                var line = rawLine.Trim();

                if (line.StartsWith("}")) {
                    indentLevel = System.Math.Max(0, indentLevel - 1);
                }
                
                sb.Append(new string(' ', indentLevel * indentSize));
                sb.AppendLine(line);

                if (line.EndsWith("{")) {
                    ++indentLevel;
                }
            }

            return sb.ToString();
        }

    }

}
#endif
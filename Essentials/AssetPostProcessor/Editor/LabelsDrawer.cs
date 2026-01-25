using System.Linq;

namespace UnityEditor.UI.Windows.Essentials.AssetPostProcessor.Editor {

    #if UIWS_ASSET_POSTPROCESSOR
    using UnityEngine.UI.Windows.Essentials.AssetPostProcessor.Runtime;
    using UnityEditor;
    using UnityEngine;
    
    [UnityEditor.CustomPropertyDrawer(typeof(LabelAttribute))]
    public class LabelsDrawer : UnityEditor.PropertyDrawer {

        //private static System.Collections.Generic.List<string> labels;
        
        public override void OnGUI(UnityEngine.Rect position, SerializedProperty property, UnityEngine.GUIContent label) {
            var config = UnityEditor.UI.Windows.Essentials.AssetPostProcessor.Editor.Processors.CustomAssetPostProcessor.ValidateConfig();
            /*if (labels == null) {
                var flags = System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.InvokeMethod;
                var dicLabels = (System.Collections.Generic.Dictionary<string, float>)typeof(AssetDatabase).InvokeMember("GetAllLabels", flags, null, null, null);
                labels = dicLabels.Keys.ToList();
            }*/
            
            var value = property.stringValue;

            if (GUILayoutExt.DrawDropdown(position, new GUIContent(value), FocusType.Passive) == true) {
                var rect = position;
                var vector = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
                rect.x = vector.x;
                rect.y = vector.y;
                var popup = new Popup() {
                    title = value, 
                    autoClose = true,
                    autoHeight = false,
                    screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 400f),
                };
                foreach (var lbl in config.labels) {
                    popup.Item(lbl, () => {
                        property.serializedObject.Update();
                        property.stringValue = lbl;
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                    });
                }
                popup.Show();
            }

        }

    }
    #endif

}
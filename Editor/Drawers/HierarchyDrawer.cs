namespace UnityEditor.UI.Windows {
    
    using UnityEngine;
    
    [InitializeOnLoad]
    public class HierarchyDrawer {
        private static readonly System.Collections.Generic.Dictionary<int, Rect> rects = new System.Collections.Generic.Dictionary<int, Rect>();
        private static readonly UnityEngine.Texture2D hierarchyHiddenByDefaultIcon;
        static HierarchyDrawer() {
            hierarchyHiddenByDefaultIcon = UnityEngine.Resources.Load<UnityEngine.Texture2D>("EditorAssets/hierarchy_hbd");
            if (hierarchyHiddenByDefaultIcon == null) {
                return;
            }
            EditorApplication.hierarchyWindowItemOnGUI -= DrawIconOnWindowItem;
            EditorApplication.hierarchyWindowItemOnGUI += DrawIconOnWindowItem;
        }

        private static void DrawIconOnWindowItem(int instanceID, UnityEngine.Rect rect) {
            
            if (hierarchyHiddenByDefaultIcon == null) {
                return;
            }

            UnityEngine.GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as UnityEngine.GameObject;
            if (gameObject == null) {
                return;
            }

            var component = gameObject.GetComponent<UnityEngine.UI.Windows.WindowComponent>();
            if (component != null) {
                rects[instanceID] = rect;

                if (component.hiddenByDefault == true) {
                    const float iconWidth = 15f;
                    EditorGUIUtility.SetIconSize(new Vector2(iconWidth, iconWidth));
                    var padding = new Vector2(5f, 0f);
                    var iconDrawRect = new UnityEngine.Rect(
                        rect.xMax - (iconWidth + padding.x), 
                        rect.yMin, 
                        rect.width, 
                        rect.height);
                    var iconGUIContent = new UnityEngine.GUIContent(hierarchyHiddenByDefaultIcon);
                    EditorGUI.LabelField(iconDrawRect, iconGUIContent);
                    EditorGUIUtility.SetIconSize(Vector2.zero);
                }

                if (component.hiddenByDefault == true || component.allowRegisterInRoot == false) {
                    var registry = UnityEngine.UI.Windows.Editor.WindowObjectRegistry.GetRegistry(component);
                    if (registry != null) {
                        foreach (var reg in registry) {
                            var holder = reg.GetHolder();
                            if (holder is UnityEngine.UI.Windows.WindowObject obj) {
                                if (rects.TryGetValue(obj.gameObject.GetInstanceID(), out var objRect) == true) {
                                    // draw line
                                    var from = new Vector2(rect.xMin - 8f * (Mathf.Clamp01(component.transform.childCount)), rect.yMin + EditorGUIUtility.singleLineHeight * 0.5f);
                                    var to = new Vector2(objRect.xMin - 8f * (Mathf.Clamp01(obj.transform.childCount)), objRect.yMin + EditorGUIUtility.singleLineHeight * 0.5f);
                                    var mid = new Vector2(Mathf.Min(from.x, to.x), Mathf.Max(from.y, to.y));
                                    Handles.color = Color.gray;
                                    Handles.DrawDottedLine(from, mid, 1f);
                                    Handles.DrawDottedLine(mid, to, 1f);
                                }
                            }
                        }
                    }
                }
            }
            
        }
    }
}
using UnityEngine;
using UnityEngine.UI.Windows;
#if UNITY_EDITOR
using UnityEditor;
#endif

[AddComponentMenu("UI.Windows/Add Component Draft")]
public class OpenAddWindowComponent : WindowObject {
    #if UNITY_EDITOR
    private void Reset() {
        UnityEditor.UI.Windows.CreateComponentDraftWindow.Open(this.gameObject);
        EditorApplication.delayCall += () => {
            if (this != null) DestroyImmediate(this, true);
        };
    }
    #endif
}
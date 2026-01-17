namespace UnityEditor.UI.Windows {
    
    using UnityEngine;
    using uiws = UnityEngine.UI.Windows;

    [InitializeOnLoad]
    public static class EditorChangesTracker {

        static EditorChangesTracker() {
            
            UnityEditor.ObjectChangeEvents.changesPublished -= EditorChangesPublished;
            UnityEditor.ObjectChangeEvents.changesPublished += EditorChangesPublished;
            
        }

        private static void EditorChangesPublished(ref ObjectChangeEventStream stream) {
            for (int i = 0; i < stream.length; ++i) {
                var evt = stream.GetEventType(i);
                if (evt == UnityEditor.ObjectChangeKind.CreateGameObjectHierarchy) {
                    stream.GetCreateGameObjectHierarchyEvent(i, out var data);
                    var obj = UnityEditor.EditorUtility.InstanceIDToObject(data.instanceId) as GameObject;
                    if (obj == null) continue;
                    var layout = obj.GetComponentInParent<uiws::WindowLayout>(true);
                    if (layout.ApplyTagsEditor() == true) {
                        UnityEditor.EditorUtility.SetDirty(layout.gameObject);
                    }
                } else if (evt == UnityEditor.ObjectChangeKind.ChangeGameObjectStructure) {
                    stream.GetChangeGameObjectStructureEvent(i, out var data);
                    var obj = UnityEditor.EditorUtility.InstanceIDToObject(data.instanceId) as GameObject;
                    if (obj == null) continue;
                    var layout = obj.GetComponentInParent<uiws::WindowLayout>(true);
                    if (layout.ApplyTagsEditor() == true) {
                        UnityEditor.EditorUtility.SetDirty(layout.gameObject);
                    }
                } else if (evt == UnityEditor.ObjectChangeKind.DestroyGameObjectHierarchy) {
                    stream.GetChangeGameObjectStructureEvent(i, out var data);
                    var obj = UnityEditor.EditorUtility.InstanceIDToObject(data.instanceId) as GameObject;
                    if (obj == null) continue;
                    var layout = obj.GetComponentInParent<uiws::WindowLayout>(true);
                    if (layout.ApplyTagsEditor() == true) {
                        UnityEditor.EditorUtility.SetDirty(layout.gameObject);
                    }
                }
            }
        }

    }

}
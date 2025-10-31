using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI.Windows;

namespace UnityEditor.UI.Windows {

// Alternative version, with redundant code removed

    [CustomEditor(typeof(GameObject))]
    public class PrefabOverrideUIWindowsEditor : UnityEditor.Editor {

        private UnityEditor.Editor builtInEditor;

        void OnEnable() {
            this.builtInEditor =
                CreateEditor(this.targets, Type.GetType("UnityEditor.GameObjectInspector, UnityEditor"));
        }

        void OnDisable() {
            if (this.builtInEditor != null)
                DestroyImmediate(this.builtInEditor);
        }

        protected override void OnHeaderGUI() {
            // Draw the default GameObject editor header
            this.builtInEditor.DrawHeader();
        }

        public override void OnInspectorGUI() {
            if (this.builtInEditor != null) {
                this.builtInEditor.OnInspectorGUI();
            }

            var instance = Selection.activeGameObject;
            if (instance == null) return;

            var hasWindowBase = instance.GetComponent<WindowBase>() != null;
            var hasWindowLayout = instance.GetComponent<WindowLayout>() != null;

            if (hasWindowBase == false && hasWindowLayout == false) return;

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("✅Apply UI.Windows", "It applies all UI.Windows-related changes for this GameObject and removes all UI.Windows-related components from the GameObject"))) {
                this.ApplyAndClean();
            }

            if (GUILayout.Button(new GUIContent("❌Revert UI.Windows", "It reverts all UI.Windows-related changes for this GameObject and adds all UI.Windows-related components back to the GameObject"))) {
                this.RevertAndClean();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

        }

        private void ApplyAndClean() {
            if (Selection.activeGameObject == null) {
                Debug.LogWarning("Select a prefab instance in the scene.");
                return;
            }

            var instance = Selection.activeGameObject;
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(instance);

            if (prefab == null) {
                Debug.LogWarning("Selected object is not a prefab instance.");
                return;
            }

            WindowLayout[] windowLayouts = Array.Empty<WindowLayout>();
            if (instance.GetComponent<WindowBase>()) {
                windowLayouts = instance.GetComponentsInChildren<WindowLayout>(true);
                foreach (var layout in windowLayouts) {
                    layout.gameObject.transform.SetParent(null);
                }
            }

            var savedGameObjects = WindowLayoutUtility.DetachWindowComponentsInOrder(instance);

            PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.AutomatedAction);

            foreach (var layout in windowLayouts) {
                layout.gameObject.transform.SetParent(instance.transform);
            }

            WindowLayoutUtility.RestoreWindowComponentParents(savedGameObjects);

            Debug.Log($"UI.Windows applied {PrefabOverrideUIWindowsEditor.GetHierarchyPath(instance)}");
            savedGameObjects.Reverse();
            foreach (var go in savedGameObjects) {
                Debug.Log($"Ignored: {PrefabOverrideUIWindowsEditor.GetHierarchyPath(go.component.gameObject)}");
            }

        }

        private void RevertAndClean() {
            if (Selection.activeGameObject == null) {
                Debug.LogWarning("Select a prefab instance in the scene.");
                return;
            }

            var instance = Selection.activeGameObject;
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(instance);

            if (prefab == null) {
                Debug.LogWarning("Selected object is not a prefab instance.");
                return;
            }

            WindowLayout[] windowLayouts = Array.Empty<WindowLayout>();
            if (instance.GetComponent<WindowBase>()) {
                windowLayouts = instance.GetComponentsInChildren<WindowLayout>(true);
                foreach (var layout in windowLayouts) {
                    layout.gameObject.transform.SetParent(null);
                }
            }

            var savedGameObjects = WindowLayoutUtility.DetachWindowComponentsInOrder(instance);

            PrefabUtility.RevertPrefabInstance(instance, InteractionMode.AutomatedAction);

            foreach (var layout in windowLayouts) {
                layout.gameObject.transform.SetParent(instance.transform);
            }

            WindowLayoutUtility.RestoreWindowComponentParents(savedGameObjects);

            Debug.Log($"UI.Windows reverted {PrefabOverrideUIWindowsEditor.GetHierarchyPath(instance)}");
            savedGameObjects.Reverse();
            foreach (var go in savedGameObjects) {
                Debug.Log($"Ignored: {PrefabOverrideUIWindowsEditor.GetHierarchyPath(go.component.gameObject)}");
            }

        }

        private static string GetHierarchyPath(GameObject obj) {
            string path = obj.name;
            Transform current = obj.transform.parent;

            while (current != null) {
                path = current.name + "/" + path;
                current = current.parent;
            }

            return path;
        }

    }

    internal static class WindowLayoutUtility {

        public static List<(WindowComponent component, Transform originalParent, int)> DetachWindowComponentsInOrder(GameObject root) {

            var result = new List<(WindowComponent, Transform, int siblingIndex)>();

            var layouts = root.GetComponentsInChildren<WindowLayout>(true);

            foreach (var layout in layouts) {

                var components = layout.GetComponentsInChildren<WindowComponent>(true);
                var siblingIndices = new Dictionary<Transform, int>();
                for (int i = 0; i < components.Length; i++) {
                    siblingIndices.Add(components[i].transform, components[i].transform.GetSiblingIndex());
                }

                var movedParents = new HashSet<Transform>();

                for (int i = 0; i < components.Length; i++) {

                    var comp = components[i];

                    if (movedParents.Any(p => WindowLayoutUtility.IsRecursiveParent(p, comp.transform))) continue;

                    var compType = comp.GetType();
                    if (compType == typeof(WindowLayoutElement)) continue;

                    if (comp != null && comp.transform.parent != null) {
                        result.Add((comp, comp.transform.parent, siblingIndices[comp.transform]));
                        comp.transform.SetParent(null, false);
                        movedParents.Add(comp.transform);
                    }

                }

            }

            return result;
        }

        public static void RestoreWindowComponentParents(List<(WindowComponent component, Transform originalParent, int siblingIndex)> list) {
            for (int i = list.Count - 1; i >= 0; i--) {
                var (comp, parent, index) = list[i];

                if (comp != null && parent != null) {
                    comp.transform.SetParent(parent, false);
                    comp.transform.SetSiblingIndex(index);
                }
            }

        }

        private static bool IsRecursiveParent(Transform parent, Transform child) {

            Transform current = child.parent;
            while (current != null) {
                if (current == parent) return true;
                current = current.parent;
            }

            return false;

        }

    }

}

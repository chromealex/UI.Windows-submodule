using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Utilities {

    public struct TransformData {

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;

        public Vector2 sizeDelta;
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot;

    }

    public static class UIWSUtils {

        public static void SetParent(Transform transform, Transform parent) {

            var data = UIWSUtils.GetTransformData(transform);
            transform.SetParent(parent);
            UIWSUtils.SetTransformData(transform, data);

        }

        public static TransformData GetTransformData(Transform tr) {

            var data = new TransformData();
            data.position = tr.localPosition;
            data.rotation = tr.localRotation;
            data.scale = tr.localScale;

            if (tr is RectTransform rect) {

                data.sizeDelta = rect.sizeDelta;
                data.pivot = rect.pivot;
                data.anchorMin = rect.anchorMin;
                data.anchorMax = rect.anchorMax;

            }

            return data;

        }

        public static void SetTransformData(Transform tr, TransformData data) {

            tr.localPosition = data.position;
            tr.localRotation = data.rotation;
            tr.localScale = data.scale;

            if (tr is RectTransform rect) {

                rect.sizeDelta = data.sizeDelta;
                rect.pivot = data.pivot;
                rect.anchorMin = data.anchorMin;
                rect.anchorMax = data.anchorMax;

            }

        }

    }

}
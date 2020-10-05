using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    public class WindowLayoutSafeZone : WindowComponent {

        [ContextMenu("Apply")]
        public void Apply() {
            
            var rect = this.rectTransform;
            rect.localScale = Vector3.one;

            var w = Screen.width;
            var h = Screen.height;
            var safeZone = Screen.safeArea;
            safeZone.xMin /= w;
            safeZone.yMin /= h;
            safeZone.xMax /= w;
            safeZone.yMax /= h;
            var anchorMin = rect.anchorMin;
            var anchorMax = rect.anchorMax;
            anchorMin.x = safeZone.xMin;
            anchorMax.x = safeZone.xMax;
            anchorMin.y = safeZone.yMin;
            anchorMax.y = safeZone.yMax;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;

        }

    }

}
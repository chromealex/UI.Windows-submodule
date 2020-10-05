using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    [CreateAssetMenu(menuName = "UI.Windows/Window System Target")]
    public class WindowSystemTarget : ScriptableObject {

        [System.Serializable]
        public struct AspectItem {

            public Vector2 aspectFrom;
            public Vector2 aspectTo;

        }

        [System.Serializable]
        public struct Aspects {

            public AspectItem[] items;

        }

        public bool checkPlatform;
        public RuntimePlatform[] runtimePlatforms;

        [Space(10f)]
        public bool checkAspects;
        public Aspects aspects;

        public bool IsValid(TargetData data) {

            if (this.checkPlatform == true) {

                if (System.Array.IndexOf(this.runtimePlatforms, data.platform) < 0) return false;

            }

            if (this.checkAspects == true) {

                var aspect = data.screenSize.x / data.screenSize.y;
                for (int i = 0; i < this.aspects.items.Length; ++i) {

                    var item = this.aspects.items[i];
                    var from = item.aspectFrom.x / item.aspectFrom.y;
                    var to = item.aspectTo.x / item.aspectTo.y;
                    var min = Mathf.Min(from, to);
                    var max = Mathf.Min(from, to);
                    if (aspect < min || aspect > max) return false;

                }

            }

            return true;

        }

    }

}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    using Utilities;

    [System.Serializable]
    public struct TargetData {

        public RuntimePlatform platform;
        public Vector2 screenSize;

    }

    [System.Serializable]
    public struct WindowSystemTargets {

        [SearchAssetsByTypePopup(typeof(WindowSystemTarget), menuName: "Targets", noneOption: "Any")]
        public WindowSystemTarget target;

        public bool IsValid(TargetData data) {

            if (this.target == null) return true;

            return this.target.IsValid(data);

        }

    }

}
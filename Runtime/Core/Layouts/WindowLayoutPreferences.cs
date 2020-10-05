using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    [CreateAssetMenu(menuName = "UI.Windows/Window Layout Preferences")]
    public class WindowLayoutPreferences : ScriptableObject {

        [System.Serializable]
        public struct CanvasScalerData {

            public static CanvasScalerData Default => new CanvasScalerData() {
                uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize,
                referenceResolution = new Vector2(800, 600),
                scaleFactor = 1f,
                referencePixelsPerUnit = 100,
                screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight,
                matchWidthOrHeight = 0f,
                physicalUnit = UnityEngine.UI.CanvasScaler.Unit.Points,
                fallbackScreenDPI = 96,
                defaultSpriteDPI = 96,
                dynamicPixelsPerUnit = 1f,
            };

            public float scaleFactor;
            public UnityEngine.UI.CanvasScaler.Unit physicalUnit;
            public Vector2 referenceResolution;
            public UnityEngine.UI.CanvasScaler.ScreenMatchMode screenMatchMode;
            public UnityEngine.UI.CanvasScaler.ScaleMode uiScaleMode;
            public float dynamicPixelsPerUnit;
            [Range(0f, 1f)] public float matchWidthOrHeight;
            public float referencePixelsPerUnit;
            public float defaultSpriteDPI;
            public float fallbackScreenDPI;

        }

        public CanvasScalerData canvasScalerData = CanvasScalerData.Default;

        public void Apply(UnityEngine.UI.CanvasScaler canvasScaler) {

            canvasScaler.scaleFactor = this.canvasScalerData.scaleFactor;
            canvasScaler.physicalUnit = this.canvasScalerData.physicalUnit;
            canvasScaler.referenceResolution = this.canvasScalerData.referenceResolution;
            canvasScaler.screenMatchMode = this.canvasScalerData.screenMatchMode;
            canvasScaler.uiScaleMode = this.canvasScalerData.uiScaleMode;
            canvasScaler.dynamicPixelsPerUnit = this.canvasScalerData.dynamicPixelsPerUnit;
            canvasScaler.matchWidthOrHeight = this.canvasScalerData.matchWidthOrHeight;
            canvasScaler.referencePixelsPerUnit = this.canvasScalerData.referencePixelsPerUnit;
            canvasScaler.defaultSpriteDPI = this.canvasScalerData.defaultSpriteDPI;
            canvasScaler.fallbackScreenDPI = this.canvasScalerData.fallbackScreenDPI;

        }

    }

}
namespace UnityEngine.UI.Windows {

    [CreateAssetMenu(menuName = "UI.Windows/Safe Area Config")]
    public class SafeAreaConfig : ScriptableObject {

        public WindowLayoutSafeZone.PaddingType paddingType = WindowLayoutSafeZone.PaddingType.All;
        public WindowLayoutSafeZone.CustomPaddings customPaddings;

    }

    [System.Serializable]
    public struct SafeArea {

        [SerializeField] internal SafeAreaConfig config;
        [SerializeField] internal WindowLayoutSafeZone.PaddingType paddingType;
        [SerializeField] internal WindowLayoutSafeZone.CustomPaddings customPaddings;

        public WindowLayoutSafeZone.PaddingType PaddingType {
            get {
                if (this.config != null) {
                    return this.config.paddingType;
                }
                return this.paddingType;
            }
        }

        public WindowLayoutSafeZone.CustomPaddings CustomPaddings {
            get {
                if (this.config != null) {
                    return this.config.customPaddings;
                }
                return this.customPaddings;
            }
        }

        public static SafeArea Default => new SafeArea() { paddingType = WindowLayoutSafeZone.PaddingType.All };
        public static SafeArea None => new SafeArea() { paddingType = WindowLayoutSafeZone.PaddingType.None };

    }

}
namespace UnityEngine.UI.Windows {

    [CreateAssetMenu(menuName = "UI.Windows/Window System Settings")]
    public class WindowSystemSettings : ScriptableObject {

        [System.Serializable]
        public struct Layer {

            public string name;
            public float minDepth;
            public float maxDepth;
            public float minZDepth;
            public float maxZDepth;
            
        }

        public int windowsPerLayer = 1000;

        [System.Serializable]
        public struct Layers {

            public float depthStep;
            public float zDepthStep;

        }

        public Layers layers = new Layers() { depthStep = 10f, zDepthStep = 1000f };

        [System.Serializable]
        public struct Components {

            public RenderBehaviour renderBehaviourOnHidden;

        }

        public Components components = new Components() { renderBehaviourOnHidden = RenderBehaviour.HideGameObject };

        [System.Serializable]
        public struct Canvas {

            public RenderMode renderMode;
            public string sortingLayerName;

        }

        public Canvas canvas = new Canvas() { renderMode = RenderMode.ScreenSpaceCamera, sortingLayerName = "Default" };

        [System.Serializable]
        public struct Camera {

            public bool orthographicDefault;

            [Space(10f)] public float orthographicSize;
            public float orthographicNearClippingPlane;
            public float orthographicFarClippingPlane;

            [Space(10f)] public float perspectiveSize;
            public float perspectiveNearClippingPlane;
            public float perspectiveFarClippingPlane;

        }

        public Camera camera = new Camera() {
            orthographicDefault = true,

            orthographicSize = 5f,
            orthographicNearClippingPlane = 0.3f,
            orthographicFarClippingPlane = 1000f,

            perspectiveSize = 60f,
            perspectiveNearClippingPlane = 0.01f,
            perspectiveFarClippingPlane = 1000f,
        };

        [System.Serializable]
        public struct Controllers {

            public float sliderStep;

        }

        public Controllers controllers = new Controllers() {
            sliderStep = 0.1f,
        };

        [Tooltip("In StrictMode all API's will not cast objects automatically (for example ImageComponent::SetImage(sprite) will set in Image component only).")]
        public bool strictMode;
        public bool collectDebugInfo;
        
        public Layer GetLayerInfo(int index) {

            return new Layer() {
                name = index.ToString(),
                minDepth = index * this.layers.depthStep,
                minZDepth = index * this.layers.zDepthStep,
                maxDepth = index * this.layers.depthStep + this.layers.depthStep + 1f * Mathf.Sign(index),
                maxZDepth = index * this.layers.zDepthStep + this.layers.zDepthStep + 1f * Mathf.Sign(index),
            };

        }

    }

}
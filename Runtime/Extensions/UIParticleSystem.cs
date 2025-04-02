namespace UnityEngine.UI.Windows {

    public class UIParticleSystem : MonoBehaviour {

        public WindowObject windowObject;
        public int sortingOrder;
        public string sortingLayerName;
        public ParticleSystemRenderer particleSystemRenderer;
        public CanvasGroup alphaCanvasGroup;
        public ParticleSystem[] particleSystems;
        private float prevAlpha;

        public void OnValidate() {

            if (this.particleSystemRenderer == null) this.particleSystemRenderer = this.GetComponent<ParticleSystemRenderer>();
            this.particleSystems = this.GetComponentsInChildren<ParticleSystem>(true);
        }

        public void OnEnable() {

            if (this.windowObject.GetState() >= ObjectState.Showing) {

                this.ApplyOrder(this.windowObject);

            } else {

                WindowSystem.GetEvents().RegisterOnce(this.windowObject, WindowEvent.OnShowBegin, this.ApplyOrder);

            }

        }

        private void ApplyOrder(WindowObject obj) {

            this.particleSystemRenderer.sortingOrder = this.windowObject.GetWindow().GetCanvasOrder() + this.sortingOrder;
            if (string.IsNullOrEmpty(this.sortingLayerName) == false) this.particleSystemRenderer.sortingLayerName = this.sortingLayerName;

        }

        private void Update() {

            if (this.windowObject.GetState() is ObjectState.Showing or ObjectState.Shown or ObjectState.Hiding && this.alphaCanvasGroup != null) {
                var currentAlpha = this.alphaCanvasGroup.alpha;
                if (currentAlpha != this.prevAlpha) {
                    this.prevAlpha = currentAlpha;
                    this.ApplyAlpha(currentAlpha);
                }
            }

        }

        private static ParticleSystem.Particle[] particles = new ParticleSystem.Particle[1000];

        private void ApplyAlpha(float alpha) {

            foreach (var system in this.particleSystems) {
                var systemMain = system.main;
                int count = system.GetParticles(particles);
                for (int i = 0; i < count; i++) {
                    ref var particle = ref particles[i];
                    {
                        var color = particle.startColor;
                        color.a = (byte)(alpha * 255);
                        particle.startColor = color;
                    }
                }
                system.SetParticles(particles, count);
                
                var startColor = systemMain.startColor;
                {
                    var color = startColor.color;
                    color.a = alpha;
                    startColor.color = color;
                }
                {
                    var color = startColor.colorMin;
                    color.a = alpha;
                    startColor.colorMin = color;
                }
                {
                    var color = startColor.colorMax;
                    color.a = alpha;
                    startColor.colorMax = color;
                }

                systemMain.startColor = startColor;
            }

        }


    }

}
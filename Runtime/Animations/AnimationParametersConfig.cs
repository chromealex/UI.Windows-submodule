namespace UnityEngine.UI.Windows.Modules {

    using Utilities;

    public abstract class AnimationParametersConfig : ScriptableObject {

        public float durationShow = 0.3f;
        public float durationHide = 0.3f;
        public float delayShow = 0f;
        public float delayHide = 0f;
        public Tweener.EaseFunction easeShow = Tweener.EaseFunction.Linear;
        public Tweener.EaseFunction easeHide = Tweener.EaseFunction.Linear;

        public virtual void Apply(AnimationParameters parameters) {

            parameters.durationShow = this.durationShow;
            parameters.durationHide = this.durationHide;
            parameters.delayShow = this.delayShow;
            parameters.delayHide = this.delayHide;
            parameters.easeShow = this.easeShow;
            parameters.easeHide = this.easeHide;

        }

    }

}
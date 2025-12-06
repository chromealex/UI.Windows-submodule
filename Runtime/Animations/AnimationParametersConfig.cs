namespace UnityEngine.UI.Windows.Modules {

    using Utilities;

    public abstract class AnimationParametersConfig : ScriptableObject {

        public AnimationParameters.ShowHideParameters show;
        public AnimationParameters.ShowHideParameters hide;
        [HideInInspector][System.Obsolete("Use show instead")]
        public float durationShow = 0.3f;
        [HideInInspector][System.Obsolete("Use hide instead")]
        public float durationHide = 0.3f;
        [HideInInspector][System.Obsolete("Use show instead")]
        public float delayShow = 0f;
        [HideInInspector][System.Obsolete("Use show instead")]
        public AnimationParameters.Delay delayShowParameters;
        [HideInInspector][System.Obsolete("Use hide instead")]
        public float delayHide = 0f;
        [HideInInspector][System.Obsolete("Use hide instead")]
        public AnimationParameters.Delay delayHideParameters;
        [HideInInspector][System.Obsolete("Use show instead")]
        public Tweener.EaseFunction easeShow = Tweener.EaseFunction.Linear;
        [HideInInspector][System.Obsolete("Use hide instead")]
        public Tweener.EaseFunction easeHide = Tweener.EaseFunction.Linear;

        protected virtual void OnValidate() {
            
            #pragma warning disable
            if (this.show.IsEmpty() == true) this.show = new AnimationParameters.ShowHideParameters(this.durationShow, this.delayShow, this.delayShowParameters, this.easeShow);
            if (this.hide.IsEmpty() == true) this.hide = new AnimationParameters.ShowHideParameters(this.durationHide, this.delayHide, this.delayHideParameters, this.easeHide);
            #pragma warning restore
            
        }

        public virtual void Apply(AnimationParameters parameters) {

            parameters.show = this.show;
            parameters.hide = this.hide;

        }

    }

}
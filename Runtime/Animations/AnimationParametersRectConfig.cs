namespace UnityEngine.UI.Windows.Modules {

    [CreateAssetMenu(menuName = "UI.Windows/Animations/Rect Parameters Config")]
    public class AnimationParametersRectConfig : AnimationParametersConfig {

        [System.Obsolete("Use animatedParameters instead")]
        [HideInInspector]
        public RectAnimationParameters.AnimationParameter parameters = RectAnimationParameters.AnimationParameter.Position | RectAnimationParameters.AnimationParameter.Rotation | RectAnimationParameters.AnimationParameter.Scale;
        public RectAnimationParameters.AnimationParameter animatedParameters = RectAnimationParameters.AnimationParameter.Position | RectAnimationParameters.AnimationParameter.Rotation | RectAnimationParameters.AnimationParameter.Scale;
        public RectAnimationParameters.RectState resetState = new RectAnimationParameters.RectState() {
            anchorPosition = new Vector2(0f, -100f),
            rotation = Vector3.zero,
            scale = Vector3.one,
        };
        public RectAnimationParameters.RectState shownState = new RectAnimationParameters.RectState() {
            anchorPosition = Vector2.zero,
            rotation = Vector3.zero,
            scale = Vector3.one,
        };
        public RectAnimationParameters.RectState hiddenState = new RectAnimationParameters.RectState() {
            anchorPosition = new Vector2(0f, 100f),
            rotation = Vector3.zero,
            scale = Vector3.one,
        };

        protected override void OnValidate() {
            
            base.OnValidate();
            
            #pragma warning disable
            if (this.parameters == (RectAnimationParameters.AnimationParameter)(-1)) {
                // set default parameters as a part of migration
                this.animatedParameters = RectAnimationParameters.AnimationParameter.Position | RectAnimationParameters.AnimationParameter.Rotation | RectAnimationParameters.AnimationParameter.Scale;
            }
            #pragma warning restore
            
            this.hiddenState.visible = this.animatedParameters;
            this.shownState.visible = this.animatedParameters;
            this.resetState.visible = this.animatedParameters;

        }

        public override void Apply(AnimationParameters parameters) {

            base.Apply(parameters);
            
            var target = parameters as RectAnimationParameters;
            if (target == null) return;

            target.animatedParameters = this.animatedParameters;
            target.resetState.CopyFrom(this.resetState);
            target.shownState.CopyFrom(this.shownState);
            target.hiddenState.CopyFrom(this.hiddenState);

        }

    }

}
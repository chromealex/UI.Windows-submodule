namespace UnityEngine.UI.Windows.Modules {

    [CreateAssetMenu(menuName = "UI.Windows/Animations/Rect Parameters Config")]
    public class AnimationParametersRectConfig : AnimationParametersConfig {

        public RectAnimationParameters.AnimationParameter parameters = (RectAnimationParameters.AnimationParameter)(-1);
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
        
        public override void Apply(AnimationParameters parameters) {

            base.Apply(parameters);
            
            var target = parameters as RectAnimationParameters;
            if (target == null) return;

            target.parameters = this.parameters;
            target.resetState.CopyFrom(this.resetState);
            target.shownState.CopyFrom(this.shownState);
            target.hiddenState.CopyFrom(this.hiddenState);

        }

    }

}
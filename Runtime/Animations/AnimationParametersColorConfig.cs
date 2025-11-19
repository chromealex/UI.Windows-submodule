namespace UnityEngine.UI.Windows.Modules {

    [CreateAssetMenu(menuName = "UI.Windows/Animations/Color Parameters Config")]
    public class AnimationParametersColorConfig : AnimationParametersConfig {

        public ColorAnimationParameters.ColorState resetState = new ColorAnimationParameters.ColorState() {  };
        public ColorAnimationParameters.ColorState shownState = new ColorAnimationParameters.ColorState() {  };
        public ColorAnimationParameters.ColorState hiddenState = new ColorAnimationParameters.ColorState() {  };
        
        public override void Apply(AnimationParameters parameters) {

            base.Apply(parameters);
            
            var target = parameters as ColorAnimationParameters;
            if (target == null) return;

            target.resetState.CopyFrom(this.resetState);
            target.shownState.CopyFrom(this.shownState);
            target.hiddenState.CopyFrom(this.hiddenState);

        }

    }

}
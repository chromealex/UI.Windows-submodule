namespace UnityEngine.UI.Windows.Modules {

    [CreateAssetMenu(menuName = "UI.Windows/Animations/Alpha Parameters Config")]
    public class AnimationParametersAlphaConfig : AnimationParametersConfig {

        public AlphaAnimationParameters.AlphaState resetState = new AlphaAnimationParameters.AlphaState() { alpha = 0f };
        public AlphaAnimationParameters.AlphaState shownState = new AlphaAnimationParameters.AlphaState() { alpha = 1f };
        public AlphaAnimationParameters.AlphaState hiddenState = new AlphaAnimationParameters.AlphaState() { alpha = 0f };
        
        public override void Apply(AnimationParameters parameters) {

            base.Apply(parameters);
            
            var target = parameters as AlphaAnimationParameters;
            if (target == null) return;

            target.resetState.CopyFrom(this.resetState);
            target.shownState.CopyFrom(this.shownState);
            target.hiddenState.CopyFrom(this.hiddenState);

        }

    }

}
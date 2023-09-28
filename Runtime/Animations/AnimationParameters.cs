namespace UnityEngine.UI.Windows.Modules {

    using Utilities;
    
    public enum AnimationState {

        Current = 0,
        Reset = 1,
        Show = 2,
        Hide = 3,

    }

    public abstract class AnimationParameters : MonoBehaviour {

        [Tooltip("Use config file or parameters at this component")]
        public AnimationParametersConfig config;
        
        [Space]
        public float durationShow = 0.3f;
        public float durationHide = 0.3f;
        public float delayShow = 0f;
        public float delayHide = 0f;
        public Tweener.EaseFunction easeShow = Tweener.EaseFunction.Linear;
        public Tweener.EaseFunction easeHide = Tweener.EaseFunction.Linear;

        public abstract class State {

            public abstract void CopyFrom(State other);
            public abstract void Recycle();

        }

        public abstract void OnValidate();

        public float GetDuration(AnimationState animationState) {

            var delay = 0f;
            switch (animationState) {

                case AnimationState.Show:
                    delay = this.durationShow;
                    break;

                case AnimationState.Hide:
                    delay = this.durationHide;
                    break;

            }

            return delay;

        }

        public float GetDelay(AnimationState animationState) {

            var delay = 0f;
            switch (animationState) {

                case AnimationState.Show:
                    delay = this.delayShow;
                    break;

                case AnimationState.Hide:
                    delay = this.delayHide;
                    break;

            }

            return delay;

        }

        public abstract State LerpState(State from, State to, float value);

        public abstract void ApplyState(State state);

        public State GetState(AnimationState state, bool clone = false) {

            State copy = null;
            if (clone == true) {

                copy = this.CreateState();

            }

            State result = null;
            switch (state) {

                case AnimationState.Current:
                    result = this.GetCurrentState();
                    break;

                case AnimationState.Reset:
                    result = this.GetResetState();
                    break;

                case AnimationState.Show:
                    result = this.GetInState();
                    break;

                case AnimationState.Hide:
                    result = this.GetOutState();
                    break;

            }

            if (clone == true) {
                
                copy.CopyFrom(result);
                result = copy;

            }

            return result;

        }

        public abstract State CreateState();
        public abstract State GetCurrentState();
        public abstract State GetResetState();
        public abstract State GetInState();
        public abstract State GetOutState();

    }

}
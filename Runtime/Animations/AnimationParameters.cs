namespace UnityEngine.UI.Windows.Modules {

    using Utilities;
    
    public enum AnimationState {

        Current = 0,
        Reset = 1,
        Show = 2,
        Hide = 3,

    }

    public abstract class AnimationParameters : MonoBehaviour {

        [System.Serializable]
        public struct ShowHideParameters {

            public float duration;
            public Delay delay;
            public Tweener.EaseFunction ease;

            public ShowHideParameters(float duration, float delay, Delay additionalDelayParameters, Tweener.EaseFunction ease) {
                this.duration = duration;
                this.delay = additionalDelayParameters;
                if (this.delay.random == true) {
                    this.delay.randomFromTo.x = delay;
                }
                this.ease = ease;
            }

            public bool IsEmpty() {
                return this.duration == 0f && this.delay.IsEmpty() == true && this.ease == default;
            }

        }

        [System.Serializable]
        public struct Delay {

            public bool random;
            public Vector2 randomFromTo;

            public float GetValue() {
                if (this.random == false) return this.randomFromTo.x;
                return Random.Range(this.randomFromTo.x, this.randomFromTo.y);
            }

            public bool IsEmpty() {
                return this.random == false && this.randomFromTo == default;
            }

        }
        
        [Tooltip("Use config file or parameters at this component")]
        public AnimationParametersConfig config;
        
        [Space]
        [HideInInspector][System.Obsolete("Use show instead")]
        public float durationShow = 0.3f;
        [HideInInspector][System.Obsolete("Use hide instead")]
        public float durationHide = 0.3f;
        [HideInInspector][System.Obsolete("Use show instead")]
        public float delayShow = 0f;
        [HideInInspector][System.Obsolete("Use show instead")]
        public Delay delayShowParameters;
        [HideInInspector][System.Obsolete("Use hide instead")]
        public float delayHide = 0f;
        [HideInInspector][System.Obsolete("Use hide instead")]
        public Delay delayHideParameters;
        [HideInInspector][System.Obsolete("Use show instead")]
        public Tweener.EaseFunction easeShow = Tweener.EaseFunction.Linear;
        [HideInInspector][System.Obsolete("Use hide instead")]
        public Tweener.EaseFunction easeHide = Tweener.EaseFunction.Linear;
        public ShowHideParameters show;
        public ShowHideParameters hide;

        public abstract class State {

            public abstract void CopyFrom(State other);
            public abstract void Recycle();

        }

        public virtual void OnValidate() {
            #pragma warning disable
            if (this.show.IsEmpty() == true) this.show = new ShowHideParameters(this.durationShow, this.delayShow, this.delayShowParameters, this.easeShow);
            if (this.hide.IsEmpty() == true) this.hide = new ShowHideParameters(this.durationHide, this.delayHide, this.delayHideParameters, this.easeHide);
            #pragma warning restore
        }

        public float GetDuration(AnimationState animationState) {

            var delay = 0f;
            switch (animationState) {

                case AnimationState.Show:
                    delay = this.show.duration;
                    break;

                case AnimationState.Hide:
                    delay = this.hide.duration;
                    break;

            }

            return delay;

        }

        public float GetDelay(AnimationState animationState) {

            var delay = 0f;
            switch (animationState) {

                case AnimationState.Show:
                    delay = this.show.delay.GetValue();
                    break;

                case AnimationState.Hide:
                    delay = this.hide.delay.GetValue();
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
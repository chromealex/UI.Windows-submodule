using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Modules {

    public enum AnimationState {

        Reset,
        Show,
        Hide,

    }

    public abstract class AnimationParameters : MonoBehaviour {

        public float durationShow = 1f;
        public float durationHide = 1f;
        public float delayShow = 0f;
        public float delayHide = 0f;

        public abstract class State {

            public abstract void CopyFrom(State other);

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

        public State GetState(AnimationState state) {

            switch (state) {

                case AnimationState.Reset:
                    return this.GetResetState();

                case AnimationState.Show:
                    return this.GetInState();

                case AnimationState.Hide:
                    return this.GetOutState();

            }

            return default;

        }

        public abstract State GetResetState();
        public abstract State GetInState();
        public abstract State GetOutState();

    }

}
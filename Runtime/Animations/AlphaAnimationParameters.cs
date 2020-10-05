using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Modules {

    [RequireComponent(typeof(CanvasGroup))]
    public class AlphaAnimationParameters : AnimationParameters {

        [System.Serializable]
        public class AlphaState : State {

            [Range(0f, 1f)] public float alpha;

            public override void CopyFrom(State other) {

                var _other = (AlphaState)other;
                this.alpha = _other.alpha;

            }

        }

        [Space(10f)] public CanvasGroup canvasGroup;
        public AlphaState resetState = new AlphaState() { alpha = 0f };
        public AlphaState shownState = new AlphaState() { alpha = 1f };
        public AlphaState hiddenState = new AlphaState() { alpha = 0f };

        private readonly AlphaState currentState = new AlphaState();

        public override void OnValidate() {

            this.canvasGroup = this.GetComponent<CanvasGroup>();

        }

        public override State LerpState(State from, State to, float value) {

            var toState = (AlphaState)to;
            if (from != null) {

                var fromState = (AlphaState)from;
                this.currentState.alpha = Mathf.Lerp(fromState.alpha, toState.alpha, value);

            } else {

                this.currentState.alpha = Mathf.Lerp(this.currentState.alpha, toState.alpha, value);

            }

            return this.currentState;

        }

        public override void ApplyState(State state) {

            var toState = (AlphaState)state;
            this.canvasGroup.alpha = toState.alpha;

            this.currentState.CopyFrom(state);

        }

        public override State GetResetState() {

            return this.resetState;

        }

        public override State GetInState() {

            return this.shownState;

        }

        public override State GetOutState() {

            return this.hiddenState;

        }

    }

}
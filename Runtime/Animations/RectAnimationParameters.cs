using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Modules {

    [RequireComponent(typeof(RectTransform))]
    public class RectAnimationParameters : AnimationParameters {

        [System.Serializable]
        public class RectState : State {

            public Vector2 anchorPosition;
            public Vector3 rotation;
            public Vector3 scale;

            public override void CopyFrom(State other) {

                var _other = (RectState)other;
                this.anchorPosition = _other.anchorPosition;
                this.rotation = _other.rotation;
                this.scale = _other.scale;

            }

        }

        [Space(10f)] public RectTransform rectTransform;
        public RectState resetState = new RectState() {
            anchorPosition = new Vector2(0f, -100f),
            rotation = Vector3.zero,
            scale = Vector3.one
        };
        public RectState shownState = new RectState() {
            anchorPosition = Vector2.zero,
            rotation = Vector3.zero,
            scale = Vector3.one
        };
        public RectState hiddenState = new RectState() {
            anchorPosition = new Vector2(0f, 100f),
            rotation = Vector3.zero,
            scale = Vector3.one
        };

        private readonly RectState currentState = new RectState();

        public override void OnValidate() {

            this.rectTransform = this.GetComponent<RectTransform>();

        }

        public override State LerpState(State from, State to, float value) {

            RectState fromState = null;
            var toState = (RectState)to;
            if (from != null) {

                fromState = (RectState)from;
                
            } else {

                fromState = this.currentState;

            }
            
            this.currentState.anchorPosition = Vector2.Lerp(fromState.anchorPosition, toState.anchorPosition, value);
            this.currentState.rotation = Vector3.Slerp(fromState.rotation, toState.rotation, value);
            this.currentState.scale = Vector3.Lerp(fromState.scale, toState.scale, value);

            return this.currentState;

        }

        public override void ApplyState(State state) {

            var toState = (RectState)state;
            this.rectTransform.anchoredPosition = toState.anchorPosition;
            this.rectTransform.rotation = Quaternion.Euler(toState.rotation);
            this.rectTransform.localScale = toState.scale;

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
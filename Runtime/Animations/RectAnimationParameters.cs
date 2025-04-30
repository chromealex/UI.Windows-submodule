namespace UnityEngine.UI.Windows.Modules {

    [RequireComponent(typeof(RectTransform))]
    public class RectAnimationParameters : AnimationParameters {

        [System.Flags]
        public enum AnimationParameter : int {

            Position = 0x1,
            Rotation = 0x2,
            Scale = 0x4,

        }

        [System.Serializable]
        public class RectState : State {

            public Vector2 anchorPosition;
            public bool useInitialAnchorPosition;
            public Vector3 rotation;
            public bool useInitialRotation;
            public Vector3 scale;
            public bool useInitialScale;

            public override void CopyFrom(State other) {

                var _other = (RectState)other;
                this.anchorPosition = _other.anchorPosition;
                this.rotation = _other.rotation;
                this.scale = _other.scale;

            }

            public override void Recycle() {

                this.anchorPosition = default;
                this.rotation = default;
                this.scale = default;
                PoolClass<RectState>.Recycle(this);
                
            }

        }

        [Space(10f)] public RectTransform rectTransform;
        public AnimationParameter parameters = (AnimationParameter)(-1);
        public RectState resetState = new RectState() {
            anchorPosition = new Vector2(0f, -100f),
            rotation = Vector3.zero,
            scale = Vector3.one,
        };
        public RectState shownState = new RectState() {
            anchorPosition = Vector2.zero,
            rotation = Vector3.zero,
            scale = Vector3.one,
        };
        public RectState hiddenState = new RectState() {
            anchorPosition = new Vector2(0f, 100f),
            rotation = Vector3.zero,
            scale = Vector3.one,
        };

        private readonly RectState currentState = new RectState();

        public override void OnValidate() {

            this.rectTransform = this.GetComponent<RectTransform>();
            this.SetInitialValues(this.hiddenState);
            this.SetInitialValues(this.shownState);
            this.SetInitialValues(this.resetState);
        }

        public override State LerpState(State from, State to, float value) {

            RectState fromState = null;
            var toState = (RectState)to;
            if (from != null) {

                fromState = (RectState)from;
                
            } else {

                fromState = this.currentState;

            }
            
            if ((this.parameters & AnimationParameter.Position) != 0) this.currentState.anchorPosition = Vector2.Lerp(fromState.anchorPosition, toState.anchorPosition, value);
            if ((this.parameters & AnimationParameter.Rotation) != 0) this.currentState.rotation = Vector3.Slerp(fromState.rotation, toState.rotation, value);
            if ((this.parameters & AnimationParameter.Scale) != 0) this.currentState.scale = Vector3.Lerp(fromState.scale, toState.scale, value);

            return this.currentState;

        }

        public override void ApplyState(State state) {

            if (this.rectTransform != null) {
            
                var toState = (RectState)state;
                if ((this.parameters & AnimationParameter.Position) != 0) this.rectTransform.anchoredPosition = toState.anchorPosition;
                if ((this.parameters & AnimationParameter.Rotation) != 0) this.rectTransform.rotation = Quaternion.Euler(toState.rotation);
                if ((this.parameters & AnimationParameter.Scale) != 0) this.rectTransform.localScale = toState.scale;
                
            }

            this.currentState.CopyFrom(state);

        }

        public override State CreateState() {
            
            return PoolClass<RectState>.Spawn();
            
        }

        public override State GetCurrentState() {

            return this.currentState;

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

        private void SetInitialValues(RectState state) {
            
            if (state.useInitialAnchorPosition == true) state.anchorPosition = this.rectTransform.anchoredPosition;
            if (state.useInitialRotation == true) state.rotation = this.rectTransform.localRotation.eulerAngles;
            if (state.useInitialScale == true) state.scale = this.rectTransform.localScale;
            
        }

    }

}
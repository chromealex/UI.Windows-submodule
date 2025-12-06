namespace UnityEngine.UI.Windows.Modules {

    [RequireComponent(typeof(RectTransform))]
    public class RectAnimationParameters : AnimationParameters {

        [System.Flags]
        public enum AnimationParameter : int {
            Position = 1 << 0,
            Rotation = 1 << 1,
            Scale    = 1 << 2,
            Size     = 1 << 3,
            Pivot    = 1 << 4,
            AnchorsMin = 1 << 5,
            AnchorsMax = 1 << 6,
        }

        [System.Serializable]
        public class RectState : State {

            public AnimationParameter visible = (AnimationParameter)(-1);
            public AnimationParameter parameters = (AnimationParameter)(-1);
            public Vector2 anchorPosition;
            public Vector3 rotation;
            public Vector3 scale;
            public Vector2 size;
            public Vector2 pivot;
            public Vector2 anchorsMin;
            public Vector2 anchorsMax;
            
            public override void CopyFrom(State other) {

                var _other = (RectState)other;
                this.anchorPosition = _other.anchorPosition;
                this.rotation = _other.rotation;
                this.scale = _other.scale;
                this.size = _other.size;
                this.pivot = _other.pivot;
                this.anchorsMin = _other.anchorsMin;
                this.anchorsMax = _other.anchorsMax;
                
                this.parameters = _other.parameters;
                this.visible = _other.visible;

            }

            public override void Recycle() {

                this.visible = default;
                this.parameters = default;
                this.anchorPosition = default;
                this.rotation = default;
                this.scale = default;
                this.size = default;
                this.pivot = default;
                this.anchorsMin = default;
                this.anchorsMax = default;
                PoolClass<RectState>.Recycle(this);
                
            }

        }

        [Space(10f)] public RectTransform rectTransform;
        [System.Obsolete("Use animatedParameters instead")]
        [HideInInspector]
        public AnimationParameter parameters = AnimationParameter.Position | AnimationParameter.Rotation | AnimationParameter.Scale;
        public AnimationParameter animatedParameters = AnimationParameter.Position | AnimationParameter.Rotation | AnimationParameter.Scale;
        public RectState resetState = new RectState() {
            anchorPosition = new Vector2(0f, -100f),
            rotation = Vector3.zero,
            scale = Vector3.one,
            size = Vector3.zero,
        };
        public RectState shownState = new RectState() {
            anchorPosition = Vector2.zero,
            rotation = Vector3.zero,
            scale = Vector3.one,
            size = Vector3.zero,
        };
        public RectState hiddenState = new RectState() {
            anchorPosition = new Vector2(0f, 100f),
            rotation = Vector3.zero,
            scale = Vector3.one,
            size = Vector3.zero,
        };

        private readonly RectState currentState = new RectState();

        public override void OnValidate() {

            #pragma warning disable
            if (this.parameters == (AnimationParameter)(-1)) {
                // set default parameters as a part of migration
                this.animatedParameters = AnimationParameter.Position | AnimationParameter.Rotation | AnimationParameter.Scale;
            }
            #pragma warning restore

            if (this.rectTransform == null) this.rectTransform = this.GetComponent<RectTransform>();
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
            
            if ((this.animatedParameters & AnimationParameter.Position) != 0) this.currentState.anchorPosition = Vector2.Lerp(fromState.anchorPosition, toState.anchorPosition, value);
            if ((this.animatedParameters & AnimationParameter.Rotation) != 0) this.currentState.rotation = Vector3.Slerp(fromState.rotation, toState.rotation, value);
            if ((this.animatedParameters & AnimationParameter.Scale) != 0) this.currentState.scale = Vector3.Lerp(fromState.scale, toState.scale, value);
            if ((this.animatedParameters & AnimationParameter.Size) != 0) this.currentState.size = Vector2.Lerp(fromState.size, toState.size, value);
            if ((this.animatedParameters & AnimationParameter.Pivot) != 0) this.currentState.pivot = Vector2.Lerp(fromState.pivot, toState.pivot, value);
            if ((this.animatedParameters & AnimationParameter.AnchorsMin) != 0) this.currentState.anchorsMin = Vector2.Lerp(fromState.anchorsMin, toState.anchorsMin, value);
            if ((this.animatedParameters & AnimationParameter.AnchorsMax) != 0) this.currentState.anchorsMax = Vector2.Lerp(fromState.anchorsMax, toState.anchorsMax, value);

            return this.currentState;

        }

        public override void ApplyState(State state) {

            if (this.rectTransform != null) {
                var toState = (RectState)state;
                if ((this.animatedParameters & AnimationParameter.Pivot) != 0) this.rectTransform.pivot = toState.pivot;
                if ((this.animatedParameters & AnimationParameter.AnchorsMin) != 0) this.rectTransform.anchorMin = toState.anchorsMin;
                if ((this.animatedParameters & AnimationParameter.AnchorsMax) != 0) this.rectTransform.anchorMax = toState.anchorsMax;
                if ((this.animatedParameters & AnimationParameter.Position) != 0) this.rectTransform.anchoredPosition = toState.anchorPosition;
                if ((this.animatedParameters & AnimationParameter.Rotation) != 0) this.rectTransform.rotation = Quaternion.Euler(toState.rotation);
                if ((this.animatedParameters & AnimationParameter.Scale) != 0) this.rectTransform.localScale = toState.scale;
                if ((this.animatedParameters & AnimationParameter.Size) != 0) this.rectTransform.sizeDelta = toState.size;
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

            state.visible = this.animatedParameters;
            if ((state.parameters & AnimationParameter.Position) == 0) state.anchorPosition = this.rectTransform.anchoredPosition;
            if ((state.parameters & AnimationParameter.Rotation) == 0) state.rotation = this.rectTransform.localRotation.eulerAngles;
            if ((state.parameters & AnimationParameter.Scale) == 0) state.scale = this.rectTransform.localScale;
            if ((state.parameters & AnimationParameter.Size) == 0) state.size = this.rectTransform.sizeDelta;
            if ((state.parameters & AnimationParameter.Pivot) == 0) state.pivot = this.rectTransform.pivot;
            if ((state.parameters & AnimationParameter.AnchorsMin) == 0) state.anchorsMin = this.rectTransform.anchorMin;
            if ((state.parameters & AnimationParameter.AnchorsMax) == 0) state.anchorsMax = this.rectTransform.anchorMax;
            
        }

    }

}
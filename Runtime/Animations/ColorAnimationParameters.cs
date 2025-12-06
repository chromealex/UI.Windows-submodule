namespace UnityEngine.UI.Windows.Modules {

    public class ColorAnimationParameters : AnimationParameters {

        [System.Serializable]
        public class ColorState : State {

            [System.Serializable]
            public struct Item {

                public Graphic graphic;
                public Color color;

            }
            
            public Item[] items;

            public override void CopyFrom(State other) {

                var _other = (ColorState)other;
                if (_other.items != null && _other.items.Length > 0) {
                    this.items = System.Buffers.ArrayPool<Item>.Shared.Rent(_other.items.Length);
                    System.Array.Copy(_other.items, 0, this.items, 0, _other.items.Length);
                }

            }

            public override void Recycle() {

                System.Buffers.ArrayPool<Item>.Shared.Return(this.items);
                this.items = null;
                PoolClass<ColorState>.Recycle(this);
                
            }

            public void Lerp(ColorState fromState, ColorState toState, float value) {
                var count = Mathf.Min(this.items.Length, Mathf.Min(fromState.items.Length, toState.items.Length));
                for (int i = 0; i < count; ++i) {
                    var item = this.items[i];
                    if (item.graphic == null) {
                        item.graphic = toState.items[i].graphic;
                    }
                    if (item.graphic == null) break;
                    item.graphic.color = Color.Lerp(fromState.items[i].color, toState.items[i].color, value);
                }
            }

        }

        public ColorState resetState = new ColorState() {  };
        public ColorState shownState = new ColorState() {  };
        public ColorState hiddenState = new ColorState() {  };

        private readonly ColorState currentState = new ColorState();

        public override void OnValidate() {
            
            base.OnValidate();

            if (this.resetState.items == null) this.resetState.items = System.Array.Empty<ColorState.Item>();
            
            if (this.shownState.items == null || this.shownState.items.Length != this.resetState.items.Length) {
                System.Array.Resize(ref this.shownState.items, this.resetState.items.Length);
            }
            
            if (this.hiddenState.items == null || this.hiddenState.items.Length != this.resetState.items.Length) {
                System.Array.Resize(ref this.hiddenState.items, this.resetState.items.Length);
            }

            for (int i = 0; i < this.resetState.items.Length; ++i) {
                this.shownState.items[i].graphic = this.resetState.items[i].graphic;
                this.hiddenState.items[i].graphic = this.resetState.items[i].graphic;
            }

        }

        public override State LerpState(State from, State to, float value) {

            var toState = (ColorState)to;
            if (from != null) {

                var fromState = (ColorState)from;
                this.currentState.Lerp((ColorState)fromState, (ColorState)toState, value);

            } else {

                this.currentState.Lerp(this.currentState, (ColorState)toState, value);

            }

            return this.currentState;

        }

        public override void ApplyState(State state) {

            var toState = (ColorState)state;
            for (int i = 0; i < toState.items.Length; ++i) {
                var item = toState.items[i];
                if (item.graphic == null) break;
                item.graphic.color = item.color;
            }

            this.currentState.CopyFrom(state);

        }

        public override State CreateState() {
            
            return PoolClass<ColorState>.Spawn();
            
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

    }

}
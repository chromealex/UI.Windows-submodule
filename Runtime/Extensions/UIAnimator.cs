namespace UnityEngine.UI.Windows {
    
    using Utilities;

    [System.Serializable]
    public class UIAnimator {

        public Animator animator;

        public struct State {

            public Animator animator;
            public int id;
            public string name;
            public bool boolValue;
            public float floatValue;
            public int intValue;

        }

        public void SetBool(int id, bool value) {

            this.Play(new State() { id = id, boolValue = value }, (state) => state.animator.SetBool(state.id, state.boolValue));

        }

        public void SetBool(string name, bool value) {
            
            this.Play(new State() { name = name, boolValue = value }, (state) => state.animator.SetBool(state.name, state.boolValue));

        }

        public void SetInteger(int id, int value) {
            
            this.Play(new State() { id = id, intValue = value }, (state) => state.animator.SetInteger(state.id, state.intValue));

        }

        public void SetInteger(string name, int value) {
            
            this.Play(new State() { name = name, intValue = value }, (state) => state.animator.SetInteger(state.name, state.intValue));

        }

        public void SetFloat(int id, float value) {
            
            this.Play(new State() { id = id, floatValue = value }, (state) => state.animator.SetFloat(state.id, state.floatValue));

        }

        public void SetFloat(string name, float value) {
            
            this.Play(new State() { name = name, floatValue = value }, (state) => state.animator.SetFloat(state.name, state.floatValue));

        }

        public void SetTrigger(int id) {
            
            this.Play(new State() { id = id }, (state) => state.animator.SetTrigger(state.id));

        }

        public void SetTrigger(string name) {
            
            this.Play(new State() { name = name }, (state) => state.animator.SetTrigger(state.id));

        }

        private void Play(State state, System.Action<State> action) {

            if (this.animator != null) {

                state.animator = this.animator;
                if (this.animator.isInitialized == false) {
                    
                    Coroutines.NextFrame(state, action);
                    
                } else {
                    
                    action.Invoke(state);
                    
                }
                
            }
            
        }

    }

}
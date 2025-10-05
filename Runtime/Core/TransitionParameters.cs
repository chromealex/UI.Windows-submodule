namespace UnityEngine.UI.Windows {

    internal struct TransitionInternalData {

        public WindowObject context;
        public TransitionParametersData data;
        public bool internalCall;
        public object userData;

    }

    [System.Serializable]
    public struct TransitionParametersData {

        internal bool resetAnimation;
        internal bool immediately;
        
        internal bool replaceDelay;
        internal float delay;

        internal bool replaceAffectChilds;
        internal bool affectChilds;

        internal bool replaceIgnoreTouch;
        internal bool ignoreTouch;

        internal System.Action callback;
        internal System.Action<object> callbackUserData;

        internal object userData;

    }

    [System.Serializable]
    public struct TransitionParameters {

        internal TransitionParametersData data;

        private System.Action<WindowObject, TransitionParameters, bool> contextCallback;
        private TransitionInternalData internalData;

        public static TransitionParameters Default => new TransitionParameters() {
            data = new TransitionParametersData() { resetAnimation = false },
        };

        public void RaiseCallback() {

            if (this.contextCallback != null) this.contextCallback.Invoke(this.internalData.context, new TransitionParameters() { data = this.internalData.data, internalData = this.internalData, }, this.internalData.internalCall);
            if (this.data.callback != null) this.data.callback.Invoke();
            if (this.data.callbackUserData != null) this.data.callbackUserData.Invoke(this.data.userData);

        }

        public TransitionParameters ReplaceIgnoreTouch(bool state) {

            var instance = this;
            instance.data.replaceIgnoreTouch = true;
            instance.data.ignoreTouch = state;
            return instance;

        }

        public TransitionParameters ReplaceAffectChilds(bool state) {

            var instance = this;
            instance.data.replaceAffectChilds = true;
            instance.data.affectChilds = state;
            return instance;

        }

        public TransitionParameters ReplaceResetAnimation(bool state) {

            var instance = this;
            instance.data.resetAnimation = state;
            return instance;

        }

        public TransitionParameters ReplaceImmediately(bool state) {

            var instance = this;
            instance.data.immediately = state;
            return instance;

        }

        public TransitionParameters ReplaceDelay(float value) {

            var instance = this;
            instance.data.delay = value;
            instance.data.replaceDelay = value > 0f;
            return instance;

        }

        public TransitionParameters ReplaceCallback(System.Action callback) {

            var instance = this;
            instance.data.callback = callback;
            instance.data.callbackUserData = null;
            instance.contextCallback = null;
            return instance;

        }

        public TransitionParameters ReplaceCallback(object userData, System.Action<object> callback) {

            var instance = this;
            instance.data.callback = null;
            instance.data.callbackUserData = callback;
            instance.data.userData = userData;
            instance.contextCallback = null;
            return instance;

        }

        public TransitionParameters ReplaceCallbackWithContext(System.Action<WindowObject, TransitionParameters, bool> callback, WindowObject context, TransitionParameters other, bool internalCall) {

            var instance = this;
            instance.data.callback = null;
            instance.data.callbackUserData = null;
            instance.contextCallback = callback;
            instance.internalData = new TransitionInternalData() { context = context, data = other.data, internalCall = internalCall, };
            return instance;

        }

    }
    
}
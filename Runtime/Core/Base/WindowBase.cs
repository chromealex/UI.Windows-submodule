namespace UnityEngine.UI.Windows {

    using Modules;
    
    public enum FocusState {

        None,
        Focused,
        Unfocused,

    }

    public abstract class WindowBase : WindowObject, IHasPreview {

        public WindowPreferences preferences = WindowPreferences.Default;
        public WindowModules modules = new WindowModules();
        public Breadcrumb breadcrumb;

        public int identifier;
        public int windowSourceId;
        
        public FocusState focusState;

        [HideInInspector] public Camera workCamera;

        private float currentDepth;
        private float currentZDepth;
        
        public virtual void OnParametersPass() {
        }
        
        public virtual void OnEmptyPass() {}

        protected internal override void SendEvent<T>(T data) {
            
            base.SendEvent(data);

            this.modules.SendEvent<T>(data);

        }

        public WindowSystem.WindowItem GetBreadcrumbPrevious() {

            return this.breadcrumb.GetPreviousWindow(this);

        }
        
        public void SetAsPerspective() {

            this.preferences.cameraMode = UIWSCameraMode.Perspective;
            this.ApplyCamera();

        }

        public void SetAsOrthographic() {

            this.preferences.cameraMode = UIWSCameraMode.Orthographic;
            this.ApplyCamera();

        }

        public override void Hide(TransitionParameters parameters = default) {

            var cbParameters = parameters.ReplaceCallback(() => {

                this.PushToPool();
                parameters.RaiseCallback();

            });

            base.Hide(cbParameters);

        }

        public virtual int GetCanvasOrder() {

            return 0;

        }

        public virtual Canvas GetCanvas() {

            return null;

        }

        public void SetInitialParameters(InitialParameters parameters) {

            {

                if (parameters.overrideLayer == true) this.preferences.layer = parameters.layer;
                if (parameters.overrideSingleInstance == true) this.preferences.singleInstance = parameters.singleInstance;

            }

            this.ApplyDepth();
            this.ApplyCamera();

        }

        internal void DoFocusTookInternal() {

            if (this.focusState == FocusState.Focused) return;
            this.focusState = FocusState.Focused;

            this.DoSendFocusTook();

        }

        internal void DoFocusLostInternal() {

            if (this.focusState == FocusState.Unfocused) return;
            this.focusState = FocusState.Unfocused;

            this.DoSendFocusLost();

        }

        internal override void OnHideEndInternal() {
            
            WindowSystem.RemoveWindow(this);

            base.OnHideEndInternal();
            
        }

        internal override void OnShowEndInternal() {

            WindowSystem.SendFullCoverageOnShowEnd(this);

            base.OnShowEndInternal();

        }

        internal override void OnShowBeginInternal() {

            WindowSystem.SendFocusOnShowBegin(this);

            base.OnShowBeginInternal();

        }

        internal override void OnHideBeginInternal() {

            WindowSystem.SendFullCoverageOnHideBegin(this);
            WindowSystem.SendFocusOnHideBegin(this);

            base.OnHideBeginInternal();

        }

        public float GetZDepth() {

            return this.currentZDepth;

        }

        public float GetDepth() {

            return this.currentDepth;

        }

        internal void ApplyCamera() {

            var settings = WindowSystem.GetSettings();
            switch (this.preferences.cameraMode) {

                case UIWSCameraMode.UseSettings:
                    this.workCamera.orthographic = settings.camera.orthographicDefault;
                    break;

                case UIWSCameraMode.Orthographic:
                    this.workCamera.orthographic = true;
                    this.workCamera.orthographicSize = settings.camera.orthographicSize;
                    this.workCamera.nearClipPlane = settings.camera.orthographicNearClippingPlane;
                    this.workCamera.farClipPlane = settings.camera.orthographicFarClippingPlane;
                    break;

                case UIWSCameraMode.Perspective:
                    this.workCamera.orthographic = false;
                    this.workCamera.fieldOfView = settings.camera.perspectiveSize;
                    this.workCamera.nearClipPlane = settings.camera.perspectiveNearClippingPlane;
                    this.workCamera.farClipPlane = settings.camera.perspectiveFarClippingPlane;
                    break;

            }

        }

        public override void TurnOffRender() {
            
            base.TurnOffRender();
            
            this.workCamera.enabled = false;
            
        }

        public override void TurnOnRender() {
            
            base.TurnOffRender();
            
            this.workCamera.enabled = true;
            
        }

        internal void ApplyDepth() {

            var depth = WindowSystem.GetNextDepth(this.preferences.layer);
            var zDepth = WindowSystem.GetNextZDepth(this.preferences.layer);

            this.currentDepth = depth;
            this.currentZDepth = zDepth;

            var tr = this.transform;
            this.workCamera.depth = depth;
            var pos = tr.position;
            pos.z = zDepth;
            tr.position = pos;

        }

        #if UNITY_EDITOR
        public override void ValidateEditor() {
            
            base.ValidateEditor();

            if (this.workCamera == null) this.workCamera = this.GetComponent<Camera>();
            if (this.workCamera == null) this.workCamera = this.GetComponentInChildren<Camera>(true);
            if (this.workCamera != null) {

                this.workCamera.clearFlags = CameraClearFlags.Depth;
                
            }

        }
        #endif

        public virtual void LoadAsync(System.Action onComplete) {

            this.modules.LoadAsync(this, onComplete);

        }

        public virtual WindowLayoutPreferences GetCurrentLayoutPreferences() {

            return null;

        }

    }

}
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

        public int identifier;
        public int windowSourceId;
        
        [HideInInspector] public Camera workCamera;

        private float currentDepth;
        private float currentZDepth;
        private int currentCanvasDepth;
        
        private FocusState focusState;

        public virtual void OnParametersPass() {
        }
        
        public virtual void OnEmptyPass() {}

        public FocusState GetFocusState() => this.focusState;

        internal override void OnDeInitInternal() {

            this.modules.Unload();
            
            base.OnDeInitInternal();
            
        }

        public void SetAsPerspective() {

            this.preferences.cameraMode = UIWSCameraMode.Perspective;
            this.ApplyCamera();

        }

        public void SetAsOrthographic() {

            this.preferences.cameraMode = UIWSCameraMode.Orthographic;
            this.ApplyCamera();

        }

        public override void Hide(TransitionParameters parameters) {

            if (this.GetState() == ObjectState.Hiding || this.GetState() == ObjectState.Hidden) {
                
                parameters.RaiseCallback();
                return;
                
            }
            
            parameters = parameters.ReplaceIgnoreTouch(true);
            var cbParameters = parameters.ReplaceCallback(() => {

                this.PushToPool();
                parameters.RaiseCallback();

            });

            if (cbParameters.data.replaceDelay == true) {

                var tweener = WindowSystem.GetTweener();
                tweener.Add(this, cbParameters.data.delay, 0f, 0f).Tag(this).OnComplete((obj) => {
                    
                    base.Hide(cbParameters.ReplaceDelay(0f));

                });

            } else {

                base.Hide(cbParameters);

            }

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

        public int GetCanvasDepth() {

            return this.currentCanvasDepth;

        }

        internal void ApplyCamera() {

            var settings = WindowSystem.GetSettings();
            if (this.preferences.cameraMode == UIWSCameraMode.UseSettings) {
            
                if (settings.camera.orthographicDefault == true) {
                    
                    this.ApplyCameraSettings(UIWSCameraMode.Orthographic);
                    
                } else {
                    
                    this.ApplyCameraSettings(UIWSCameraMode.Perspective);
                    
                }
                
                return;
                
            }

            this.ApplyCameraSettings(this.preferences.cameraMode);

        }

        private void ApplyCameraSettings(UIWSCameraMode mode) {
            
            var settings = WindowSystem.GetSettings();
            switch (mode) {

                case UIWSCameraMode.Orthographic:
                    this.workCamera.orthographic = true;
                    this.workCamera.orthographicSize = settings.camera.orthographicSize;
                    this.workCamera.nearClipPlane = settings.camera.orthographicNearClippingPlane;
                    this.workCamera.farClipPlane = settings.camera.orthographicFarClippingPlane;
                    if (settings.canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                        this.workCamera.enabled = false;
                    }
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
            
            base.TurnOnRender();
            
            var settings = WindowSystem.GetSettings();
            if (settings.canvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                this.workCamera.enabled = false;
            } else {
                this.workCamera.enabled = true;
            }
            
        }

        internal void ApplyDepth() {

            var depth = WindowSystem.GetNextDepth(this.preferences.layer);
            var canvasDepth = WindowSystem.GetNextCanvasDepth(this.preferences.layer);
            var zDepth = WindowSystem.GetNextZDepth(this.preferences.layer);

            this.currentDepth = depth;
            this.currentZDepth = zDepth;
            this.currentCanvasDepth = canvasDepth;

            var tr = this.transform;
            this.workCamera.depth = depth;
            var pos = tr.position;
            pos.z = zDepth;
            tr.position = pos;
            
        }

        #if UNITY_EDITOR
        public override void ValidateEditor() {
            
            base.ValidateEditor();

            var helper = new UnityEngine.UI.Windows.Utilities.DirtyHelper(this);
            if (this.workCamera == null) helper.SetObj(ref this.workCamera, this.GetComponent<Camera>());
            if (this.workCamera == null) helper.SetObj(ref this.workCamera, this.GetComponentInChildren<Camera>(true));
            if (this.workCamera != null) {

                var workCameraClearFlags = this.workCamera.clearFlags;
                if (helper.SetEnum(ref workCameraClearFlags, CameraClearFlags.Depth) == true) {
                    this.workCamera.clearFlags = workCameraClearFlags;
                }

            }

            helper.Apply();

        }
        #endif

        public virtual void LoadAsync(InitialParameters initialParameters, System.Action onComplete) {

            this.modules.LoadAsync(initialParameters, this, onComplete);

        }

        public virtual WindowLayoutPreferences GetCurrentLayoutPreferences() {

            return null;

        }

    }

}
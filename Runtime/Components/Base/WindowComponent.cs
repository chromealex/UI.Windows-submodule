namespace UnityEngine.UI.Windows {

    [DisallowMultipleComponent]
    public class WindowComponent : WindowObject, IHasPreview {

        [System.Serializable]
        public struct ComponentModules {

            public WindowComponent windowComponent;
            
            [UnityEngine.UI.Windows.Utilities.SearchComponentsByTypePopup(typeof(WindowComponentModule), "Window Component Module", allowClassOverrides: true, singleOnly: false)]
            public WindowComponentModule[] modules;

            public T GetModule<T>() {
	            
	            if (this.modules == null) return default;
                
	            for (int i = 0; i < this.modules.Length; ++i) {

		            if (this.modules[i] is T module) {

			            return module;

		            }

	            }

	            return default;

            }

            public T[] GetModules<T>() {
                
                var modulesOfGivenType = new System.Collections.Generic.List<T>();
                
                if (this.modules == null) return modulesOfGivenType.ToArray();
                
                for (int i = 0; i < this.modules.Length; ++i) {

                    if (this.modules[i] is T module) {

                        modulesOfGivenType.Add(module);

                    }

                }

                return modulesOfGivenType.ToArray();
                
            }
            
            public void ValidateEditor() {

                if (this.modules == null) return;
                
                for (int i = 0; i < this.modules.Length; ++i) {

                    if (this.modules[i] != null) {

                        this.modules[i].windowComponent = this.windowComponent;
                        this.modules[i].ValidateEditor();

                    }

                }
                
            }

            public void OnInteractableChanged(bool state) {
                
                if (this.modules == null) return;

                for (int i = 0; i < this.modules.Length; ++i) {
                    
                    if (this.modules[i] != null) this.modules[i].OnInteractableChanged(state);
                    
                }

            }
            
            public void OnLayoutChanged() {
                
                if (this.modules == null) return;

                for (int i = 0; i < this.modules.Length; ++i) {
                    
                    if (this.modules[i] != null) this.modules[i].OnLayoutChanged();
                    
                }

            }

            public void OnPoolAdd() {
                
                if (this.modules == null) return;

                for (int i = 0; i < this.modules.Length; ++i) {
                    
                    if (this.modules[i] != null) this.modules[i].OnPoolAdd();
                    
                }

            }
            
            public void OnInit() {

                if (this.modules == null) return;

                for (int i = 0; i < this.modules.Length; ++i) {
                    
                    if (this.modules[i] != null) this.modules[i].OnInit();
                    
                }
                
            }

            public void OnDeInit() {
            
                if (this.modules == null) return;

                for (int i = 0; i < this.modules.Length; ++i) {
                    
                    if (this.modules[i] != null) this.modules[i].OnDeInit();
                    
                }

            }

            public void OnShowBegin() {
            
                if (this.modules == null) return;

                for (int i = 0; i < this.modules.Length; ++i) {
                    
                    if (this.modules[i] != null) this.modules[i].OnShowBegin();
                    
                }

            }

            public void OnHideBegin() {
            
                if (this.modules == null) return;

                for (int i = 0; i < this.modules.Length; ++i) {
                    
                    if (this.modules[i] != null) this.modules[i].OnHideBegin();
                    
                }

            }

            public void OnShowEnd() {
            
                if (this.modules == null) return;

                for (int i = 0; i < this.modules.Length; ++i) {
                    
                    if (this.modules[i] != null) this.modules[i].OnShowEnd();
                    
                }

            }

            public void OnHideEnd() {
            
                if (this.modules == null) return;

                for (int i = 0; i < this.modules.Length; ++i) {
                    
                    if (this.modules[i] != null) this.modules[i].OnHideEnd();
                    
                }

            }

        }
        
        public ComponentModules componentModules;

        public T GetModule<T>() {

	        return this.componentModules.GetModule<T>();

        }
        
        public T[] GetModules<T>() {

            return this.componentModules.GetModules<T>();

        }
        
        public override void ValidateEditor() {
            
            base.ValidateEditor();

            this.componentModules.ValidateEditor();

        }

        public override void OnPoolAdd() {
            
            base.OnPoolAdd();

            this.componentModules.OnPoolAdd();

        }

        internal override void OnInitInternal() {
            
            base.OnInitInternal();
            
            this.componentModules.OnInit();
            
        }

        internal override void OnDeInitInternal() {
            
            base.OnDeInitInternal();
            
            this.componentModules.OnDeInit();

        }

        internal override void OnShowBeginInternal() {
            
            base.OnShowBeginInternal();
            
            this.componentModules.OnShowBegin();

        }

        internal override void OnHideBeginInternal() {
            
            base.OnHideBeginInternal();
            
            this.componentModules.OnHideBegin();

        }

        internal override void OnShowEndInternal() {
            
            base.OnShowEndInternal();
            
            this.componentModules.OnShowEnd();

        }

        internal override void OnHideEndInternal() {
            
            base.OnHideEndInternal();
            
            this.componentModules.OnHideEnd();

        }

    }

    public class ComponentModuleDisplayNameAttribute : System.Attribute {

        public string name;
        
        public ComponentModuleDisplayNameAttribute(string name) {

            this.name = name;

        }

    }

    public abstract class WindowComponentModule : MonoBehaviour, IHolder {

        public WindowComponent windowComponent;

        #if UNITY_EDITOR
        private void OnValidate() {

            if (Application.isPlaying == false) {

                if (WindowSystem.HasInstance() == false) return;
                UnityEditor.EditorApplication.delayCall += () => {
                    
                    if (this != null) this.ValidateEditor();
                    
                };

            }

        }
        #endif

        public WindowBase GetWindow() {

            return this.windowComponent.GetWindow();

        }
        
        public virtual void ValidateEditor() {
            
        }

        public virtual void OnInteractableChanged(bool state) {
            
        }

        public virtual void OnLayoutChanged() {
            
        }

        public virtual void OnPoolAdd() {
            
        }
        
        public virtual void OnInit() {
            
        }

        public virtual void OnDeInit() {
            
        }

        public virtual void OnShowBegin() {
            
        }

        public virtual void OnHideBegin() {
            
        }

        public virtual void OnShowEnd() {
            
        }

        public virtual void OnHideEnd() {
            
        }

    }

}

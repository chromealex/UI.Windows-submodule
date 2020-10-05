using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    [DisallowMultipleComponent]
    public class WindowComponent : WindowObject, IHasPreview {

        [System.Serializable]
        public struct ComponentModules {

            public WindowComponent windowComponent;
            
            [UnityEngine.UI.Windows.Utilities.SearchComponentsByTypePopup(typeof(WindowComponentModule), "Window Component Module", allowClassOverrides: true)]
            public WindowComponentModule[] modules;

            public void ValidateEditor() {

                if (this.modules == null) return;
                
                for (int i = 0; i < this.modules.Length; ++i) {

                    if (this.modules[i] != null) {

                        this.modules[i].windowComponent = this.windowComponent;
                        this.modules[i].ValidateEditor();

                    }

                }
                
            }

            public void OnLayoutChanged() {
                
                if (this.modules == null) return;

                for (int i = 0; i < this.modules.Length; ++i) {
                    
                    if (this.modules[i] != null) this.modules[i].OnLayoutChanged();
                    
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

        public override void ValidateEditor() {
            
            base.ValidateEditor();

            this.componentModules.ValidateEditor();

        }

        public override void OnInit() {
            
            base.OnInit();
            
            this.componentModules.OnInit();
            
        }

        public override void OnDeInit() {
            
            base.OnDeInit();
            
            this.componentModules.OnDeInit();

        }

        public override void OnShowBegin() {
            
            base.OnShowBegin();
            
            this.componentModules.OnShowBegin();

        }

        public override void OnHideBegin() {
            
            base.OnHideBegin();
            
            this.componentModules.OnHideBegin();

        }

        public override void OnShowEnd() {
            
            base.OnShowEnd();
            
            this.componentModules.OnShowEnd();

        }

        public override void OnHideEnd() {
            
            base.OnHideEnd();
            
            this.componentModules.OnHideEnd();

        }

    }

    public class ComponentModuleDisplayNameAttribute : System.Attribute {

        public string name;
        
        public ComponentModuleDisplayNameAttribute(string name) {

            this.name = name;

        }

    }

    public abstract class WindowComponentModule : MonoBehaviour {

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

        public virtual void OnLayoutChanged() {
            
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
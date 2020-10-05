using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    [System.Serializable]
    public struct Breadcrumb {

        public List<WindowSystem.WindowItem> history;

    }
    
    public class WindowSystemBreadcrumbs : MonoBehaviour {

        private Breadcrumb main;
        
        public void Initialize() {

            this.main = this.Create();

        }

        public Breadcrumb GetMain() {

            return this.main;

        }
        
        public Breadcrumb Create() {
            
            return new Breadcrumb() {
                history = new List<WindowSystem.WindowItem>()
            };
            
        }

        public void OnWindowRemoved(WindowBase instance) {

            if (this.main.history.Count > 0) {

                if (this.main.history[this.main.history.Count - 1].instance == instance) {

                    this.MovePrevious(ref this.main, true);
                    return;

                }
                
            }
            
            for (int i = 0; i < this.main.history.Count; ++i) {

                if (this.main.history[i].instance == instance) {

                    var item = this.main.history[i];
                    item.instance = null;
                    this.main.history[i] = item;
                    return;

                }
                
            }
            
        }

        public WindowSystem.WindowItem GetPrevious(WindowBase instance, bool activeOnly = true) {

            return this.GetPrevious(instance, ref this.main, activeOnly);

        }

        public WindowSystem.WindowItem GetPrevious(WindowBase instance, ref Breadcrumb breadcrumb, bool activeOnly = true) {

            var idx = -1;
            for (int i = breadcrumb.history.Count - 1; i >= 0; --i) {

                if (breadcrumb.history[i].instance == instance) {

                    idx = i;
                    break;

                }
                
            }
            
            while (idx >= 0) {

                var item = breadcrumb.history[idx];
                if (activeOnly == false || item.instance != null) return item;

                --idx;

            }
            
            return default;

        }

        public WindowSystem.WindowItem GetPrevious(bool activeOnly = true) {

            return this.GetPrevious(ref this.main, activeOnly);

        }

        public WindowSystem.WindowItem GetPrevious(ref Breadcrumb breadcrumb, bool activeOnly = true) {

            var idx = breadcrumb.history.Count - 1;
            while (idx >= 0) {

                var item = breadcrumb.history[idx];
                if (activeOnly == false || item.instance != null) return item;

                --idx;

            }
            
            return default;

        }

        public WindowSystem.WindowItem MovePrevious(bool activeOnly = true) {

            return this.MovePrevious(ref this.main, activeOnly);

        }

        public WindowSystem.WindowItem MovePrevious(ref Breadcrumb breadcrumb, bool activeOnly = true) {

            var idx = breadcrumb.history.Count - 1;
            if (idx >= 0) breadcrumb.history.RemoveAt(idx);

            idx = breadcrumb.history.Count - 1;
            while (idx >= 0) {

                var item = breadcrumb.history[idx];
                if (activeOnly == false || item.instance != null) return item;

                --idx;

            }
            
            return default;

        }
        
        public void Add(WindowSystem.WindowItem window) {
            
            this.Add(ref this.main, window);
            
        }
        
        public void Add(ref Breadcrumb breadcrumb, WindowSystem.WindowItem window) {
            
            breadcrumb.history.Add(window);
            
        }
        
    }

    public static class BreadcrumbExtensions {

        public static WindowSystem.WindowItem GetPreviousWindow(this ref Breadcrumb breadcrumb, WindowBase instance) {

            var breadcrumbs = WindowSystem.GetBreadcrumbs();
            return breadcrumbs.GetPrevious(instance, ref breadcrumb, true);

        }

    }


}

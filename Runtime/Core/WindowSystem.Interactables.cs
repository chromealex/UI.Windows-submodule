using System.Linq;
using UnityEngine.UI.Windows.Utilities;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows {

    using Components;
    
    public partial class WindowSystem {

        private System.Action waitInteractableOnComplete;
        private IInteractable waitInteractable;
        private IInteractable[] waitInteractables;
        private bool lockInteractables;
        private System.Action<IInteractable> callbackOnAnyInteractable;
        private readonly List<WindowObject> interactablesIgnoreContainers = new List<WindowObject>();

        public static void LockAllInteractables() {

            WindowSystem.instance.lockInteractables = true;

        }
        
        public static void UnlockAllInteractables() {

            WindowSystem.instance.lockInteractables = false;

        }

        public static void SetCallbackOnAnyInteractable(System.Action<IInteractable> callback) {

            if (WindowSystem.instance == null) return;
            WindowSystem.instance.callbackOnAnyInteractable = callback;

        }

        public static void AddCallbackOnAnyInteractable(System.Action<IInteractable> callback) {

            if (WindowSystem.instance == null) return;
            WindowSystem.instance.callbackOnAnyInteractable += callback;

        }

        public static void RemoveCallbackOnAnyInteractable(System.Action<IInteractable> callback) {

            if (WindowSystem.instance == null) return;
            WindowSystem.instance.callbackOnAnyInteractable -= callback;

        }

        public static void AddWaitInteractable(System.Action onComplete, IInteractable interactable) {
            
            WindowSystem.instance.waitInteractableOnComplete = onComplete;
            
            ref var arr = ref WindowSystem.instance.waitInteractables;
            if (arr == null) {
                arr = new IInteractable[1] {
                    interactable,
                };
            } else {
                var list = arr.ToList();
                list.Add(interactable);
                arr = list.ToArray();
            }
            
        }

        public static bool HasWaitInteractable() {
            return WindowSystem.instance.waitInteractables == null && WindowSystem.instance.waitInteractable == null;
        }

        public static void WaitInteractable(System.Action onComplete, IInteractable interactable) {

            WindowSystem.instance.waitInteractableOnComplete = onComplete;
            WindowSystem.instance.waitInteractable = interactable;
            WindowSystem.instance.waitInteractables = null;
            WindowSystem.instance.lockInteractables = false;

        }

        public static void WaitInteractable(System.Action onComplete, params IInteractable[] interactables) {

            WindowSystem.instance.waitInteractableOnComplete = onComplete;
            WindowSystem.instance.waitInteractable = null;
            WindowSystem.instance.waitInteractables = interactables;
            WindowSystem.instance.lockInteractables = false;

        }

        public static void CancelWaitInteractables() {

            WindowSystem.instance.waitInteractable = null;
            WindowSystem.instance.waitInteractables = null;
            WindowSystem.instance.waitInteractableOnComplete = null;
            WindowSystem.instance.lockInteractables = false;

        }

        public static void RaiseAndCancelWaitInteractables() {

            if (WindowSystem.instance.waitInteractable != null && WindowSystem.InteractWith(WindowSystem.instance.waitInteractable) == true) {
                
            }
            
            if (WindowSystem.instance.waitInteractables != null) {

                foreach (var item in WindowSystem.instance.waitInteractables) {

                    var comp = (item as WindowComponent);
                    if (comp != null && comp.GetState() == ObjectState.Shown) WindowSystem.InteractWith(item);

                }
                
            }
            
            WindowSystem.CancelWaitInteractables();

        }

        public static void AddInteractablesIgnoreContainer(WindowObject container) {
            
            WindowSystem.instance.interactablesIgnoreContainers.Add(container);
            
        }

        public static void RemoveInteractablesIgnoreContainer(WindowObject container) {
            
            WindowSystem.instance.interactablesIgnoreContainers.Remove(container);
            
        }

        public static bool CanInteractWith(IInteractable interactable) {

            if (WindowSystem.instance.lockInteractables == true) return false;

            for (int i = 0; i < WindowSystem.instance.interactablesIgnoreContainers.Count; ++i) {

                var container = WindowSystem.instance.interactablesIgnoreContainers[i];
                if (container != null) {
                    
                    if (interactable is WindowObject interactableObj) {

                        var parent = interactableObj.FindComponentParent<WindowObject, WindowObject>(container, (obj, x) => {
                            return x == obj;
                        });
                        if (parent != null) return true;

                    }
                    
                }

            }
            
            if (WindowSystem.instance.waitInteractables == null) {

                if (WindowSystem.instance.waitInteractable == null) return true;

                return WindowSystem.instance.waitInteractable == interactable;

            } else {

                for (int i = 0; i < WindowSystem.instance.waitInteractables.Length; ++i) {

                    var interactableItem = WindowSystem.instance.waitInteractables[i];
                    if (interactableItem == interactable) return true;

                }
                
                return false;
                
            }
            
        }

        public static bool InteractWith(IInteractable interactable) {
            
            WindowSystem.instance.callbackOnAnyInteractable?.Invoke(interactable);
            
            if (WindowSystem.instance.lockInteractables == true) return false;

            if (WindowSystem.instance.waitInteractables == null) {

                if (WindowSystem.instance.waitInteractable == null ||
                    WindowSystem.instance.waitInteractable == interactable) {
                    
                    WindowSystem.instance.waitInteractableOnComplete?.Invoke();
                    return true;

                }

            } else {

                for (int i = 0; i < WindowSystem.instance.waitInteractables.Length; ++i) {

                    var interactableItem = WindowSystem.instance.waitInteractables[i];
                    if (interactableItem == interactable) {

                        WindowSystem.instance.waitInteractableOnComplete?.Invoke();
                        return true;

                    }

                }
                
            }
            
            return false;
                
        }

    }

}
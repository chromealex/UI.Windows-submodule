namespace UnityEngine.UI.Windows.Components {
    
    using System.Collections.Generic;

    public class GenericComponent : WindowComponent {

        public WindowComponent[] components;
        private readonly Dictionary<System.Type, int> requestedIndexes = new Dictionary<System.Type, int>();

        public struct ClosureAPI<TClosure> {

            internal GenericComponent genericComponent;
            internal TClosure data;
            
            public ClosureAPI<TClosure> Get<T>(System.Action<T, TClosure> onComponent, Algorithm algorithm = Algorithm.GetFirstTypeAny) where T : WindowComponent {
                var component = this.genericComponent.Get<T>(algorithm);
                onComponent.Invoke(component, this.data);
                return this;
            }

            public void GetOnce<T>(System.Action<T, TClosure> onComponent, Algorithm algorithm = Algorithm.GetFirstTypeAny) where T : WindowComponent {
                this.Get(onComponent, algorithm);
                this.Forget();
            }

            public void Forget() {
                this.genericComponent.Forget();
            }

        }

        public ClosureAPI<TClosure> Closure<TClosure>(TClosure data) {
            return new ClosureAPI<TClosure>() {
                genericComponent = this,
                data = data,
            };
        }
        
        public void Get<T, TClosure>(TClosure closure, System.Action<T, TClosure> onComponent, Algorithm algorithm = Algorithm.GetFirstTypeAny) where T : WindowComponent {
            var component = this.Get<T>(algorithm);
            onComponent.Invoke(component, closure);
        }
        
        public void Forget() {
            this.requestedIndexes.Clear();
        }

        public T Get<T>(Algorithm algorithm = Algorithm.GetFirstTypeAny) where T : WindowComponent {

            T component = default;
            switch (algorithm) {

                case Algorithm.GetNextTypeAny:
                case Algorithm.GetNextTypeStrong: {
                    
                    var key = typeof(T);
                    var addNew = false;
                    if (this.requestedIndexes.TryGetValue(key, out var lastIndex) == false) {
                        addNew = true;
                        lastIndex = -1;
                    }

                    this.GetComponent(out component, ref lastIndex, algorithm);

                    if (addNew == true) {
                        this.requestedIndexes.Add(key, lastIndex);
                    } else {
                        this.requestedIndexes[key] = lastIndex;
                    }

                }
                    break;

                case Algorithm.GetFirstTypeAny:
                case Algorithm.GetFirstTypeStrong: {
                    var idx = -1;
                    this.GetComponent(out component, ref idx, algorithm);
                }
                    break;
                
            }
            
            return component;
            
        }
        
        private bool GetComponent<T>(out T component, ref int lastIndex, Algorithm algorithm) where T : WindowComponent {

            if (algorithm == Algorithm.GetFirstTypeAny || algorithm == Algorithm.GetNextTypeAny) {

                for (int i = 0; i < this.components.Length; ++i) {

                    var comp = this.components[i];
                    if (comp != null && comp is T c && lastIndex < i) {

                        lastIndex = i;
                        component = c;
                        return true;

                    }

                }

            } else if (algorithm == Algorithm.GetFirstTypeStrong || algorithm == Algorithm.GetNextTypeStrong) {

                var typeOf = typeof(T);
                for (int i = 0; i < this.components.Length; ++i) {

                    var comp = this.components[i];
                    if (comp != null && comp.GetType() == typeOf && lastIndex < i) {

                        lastIndex = i;
                        component = (T)comp;
                        return true;

                    }

                }

            }
            
            component = default;
            return false;

        }

    }

}
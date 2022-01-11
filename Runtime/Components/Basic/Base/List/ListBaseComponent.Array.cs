using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Components {

    public partial class ListBaseComponent {

        public interface IListItemArrayClosureParameters<T> : IListItemClosureParameters<T> {

            public T[] arr { get; set; }

        }
        
        public struct DefaultItemArrayParameters<T> : IListItemArrayClosureParameters<T> {

            public int index { get; set; }
            public T data { get; set; }
            public T[] arr { get; set; }

        }

        public virtual void SetItems<T, TItem>(TItem[] list, System.Action<T, DefaultItemArrayParameters<TItem>> onItem, System.Action<DefaultItemArrayParameters<TItem>> onComplete = null) where T : WindowComponent {
            
            this.SetItems(list, onItem, new DefaultItemArrayParameters<TItem>() { arr = list }, onComplete);
            
        }

        public virtual void SetItems<T, TItem, TClosure>(TItem[] list, System.Action<T, TClosure> onItem, TClosure closure, System.Action<TClosure> onComplete) where T : WindowComponent where TClosure : IListItemArrayClosureParameters<TItem> {

            if (this.isLoadingRequest == true) {

                return;

            }
            
            var emitItems = 0;
            var i = 0;
            foreach (var item in list) {

                closure.index = i;
                closure.data = item;

                if (i < this.Count) {

                    onItem.Invoke((T)this.items[i], closure);

                } else {

                    ++emitItems;

                }

                ++i;

            }

            if (emitItems > 0) {

                this.Emit(emitItems, this.source, onItem, closure, onComplete, (c) => {

                    c.data = c.arr[c.index];
                    return c;

                });

            } else {

                if (this.Count > i) {

                    var from = this.Count - i;
                    this.RemoveRange(from, this.Count);
                    
                }
                
                onComplete?.Invoke(closure);
                
            }

        }

    }

}
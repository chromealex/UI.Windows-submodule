using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Components {

    public partial class ListBaseComponent {

        public interface IListItemListClosureParameters<T> : IListItemClosureParameters<T> {

            public List<T> arr { get; set; }

        }
        
        public struct DefaultItemListParameters<T> : IListItemListClosureParameters<T> {

            public int index { get; set; }
            public T data { get; set; }
            public List<T> arr { get; set; }

        }

        public virtual void SetItems<T, TItem>(List<TItem> list, System.Action<T, DefaultItemListParameters<TItem>> onItem, System.Action<DefaultItemListParameters<TItem>, bool> onComplete = null) where T : WindowComponent {
            
            this.SetItems(list, onItem, new DefaultItemListParameters<TItem>() { arr = list }, onComplete);
            
        }

        public virtual void SetItems<T, TItem, TClosure>(List<TItem> list, System.Action<T, TClosure> onItem, TClosure closure, System.Action<TClosure, bool> onComplete) where T : WindowComponent where TClosure : IListItemListClosureParameters<TItem> {

            if (this.isLoadingRequest == true) {

                onComplete?.Invoke(closure, false);
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

                this.Emit(emitItems, this.source, onItem, closure, onComplete, static (c) => {

                    c.data = c.arr[c.index];
                    return c;

                });

            } else {

                if (this.Count > i) {

                    var from = this.Count - i;
                    this.RemoveRange(from, this.Count);
                    
                }
                
                onComplete?.Invoke(closure, true);
                
            }

        }

    }

}
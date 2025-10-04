using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Components {

    public partial class ListBaseComponent {

        public interface IListItemIEnumerableClosureParameters<T> : IListItemClosureParameters<T> {

            public IEnumerable<T> arr { get; set; }
            internal int offset { get; set; }
            internal List<T> temp { get; set; }

        }
        
        public struct DefaultItemIEnumerableParameters<T> : IListItemIEnumerableClosureParameters<T> {

            public int index { get; set; }
            public T data { get; set; }
            public IEnumerable<T> arr { get; set; }
            int IListItemIEnumerableClosureParameters<T>.offset { get; set; }
            List<T> IListItemIEnumerableClosureParameters<T>.temp { get; set; }

        }

        public virtual void SetItems<T, TItem>(IEnumerable<TItem> list, System.Action<T, DefaultItemIEnumerableParameters<TItem>> onItem, System.Action<DefaultItemIEnumerableParameters<TItem>> onComplete = null) where T : WindowComponent {
            
            this.SetItems(list, onItem, new DefaultItemIEnumerableParameters<TItem>() { arr = list }, onComplete);
            
        }

        public virtual async void SetItems<T, TItem, TClosure>(IEnumerable<TItem> list, System.Action<T, TClosure> onItem, TClosure closure, System.Action<TClosure> onComplete) where T : WindowComponent where TClosure : IListItemIEnumerableClosureParameters<TItem> {

            if (this.isLoadingRequest == true) {

                return;

            }

            var temp = PoolList<TItem>.Spawn();
            var emitItems = 0;
            var i = 0;
            foreach (var item in list) {

                closure.index = i;
                closure.data = item;

                if (i < this.Count) {

                    onItem.Invoke((T)this.items[i], closure);

                } else {

                    ++emitItems;
                    temp.Add(item);

                }

                ++i;

            }

            if (emitItems > 0) {

                closure.offset = this.Count - emitItems;
                closure.temp = temp;
                await this.Emit(emitItems, this.source, onItem, closure, static (c) => {

                    var tmp = c.temp;
                    PoolList<TItem>.Recycle(ref tmp);
                    
                }, (c) => {

                    c.data = c.temp[c.index - c.offset];
                    return c;

                });

            } else {

                PoolList<TItem>.Recycle(ref temp);
                if (this.Count > i) {

                    var from = this.Count - i;
                    this.RemoveRange(from, this.Count);
                    
                }
                
                onComplete?.Invoke(closure);
                
            }

        }

    }

}
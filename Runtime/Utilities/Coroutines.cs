using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows.Utilities {

    public class Coroutines : MonoBehaviour {

        private static Coroutines instance;

        public void Awake() {

            Coroutines.instance = this;

        }

        public static async void NextFrame<T>(T state, System.Action<T> callback) {
	        
	        await Coroutines.WaitFrames_INTERNAL(state, callback, 1);
	        
        }

        private static async System.Threading.Tasks.Task WaitFrames_INTERNAL<T>(T state, System.Action<T> callback, int frames) {

	        while (frames > 0) {
		        
		        await System.Threading.Tasks.Task.Yield();
		        --frames;

	        }
	        callback.Invoke(state);

        }
        
        public static async void NextFrame(System.Action callback) {
	        
	        await Coroutines.WaitFrames_INTERNAL(callback, 1);
	        
        }

        private static async System.Threading.Tasks.Task WaitFrames_INTERNAL(System.Action callback, int frames) {

	        while (frames > 0) {
		        
		        await System.Threading.Tasks.Task.Yield();
		        --frames;

	        }
	        callback.Invoke();

        }
        
        public static async void WaitTime(float time, System.Action callback) {
	        
            await Coroutines.TimeWaiter_INTERNAL(time, callback);
	        
        }

        private static async System.Threading.Tasks.Task TimeWaiter_INTERNAL(float time, System.Action callback) {

	        await System.Threading.Tasks.Task.Delay((int)(time * 1000));
            callback.Invoke();

        }

        public static async void WaitTime<TState>(TState state, float time, System.Action<TState> callback) {
	        
	        await Coroutines.TimeWaiter_INTERNAL(state, time, callback);
	        
        }

        private static async System.Threading.Tasks.Task TimeWaiter_INTERNAL<TState>(TState state, float time, System.Action<TState> callback) {
	        
            await System.Threading.Tasks.Task.Delay((int)(time * 1000));
            callback.Invoke(state);

        }

        public static async void Wait(System.Func<bool> waitFor, System.Action callback) {
	        
	        await Coroutines.Waiter_INTERNAL(waitFor, callback);
	        
        }

        public static async void Wait<TState>(TState state, System.Func<TState, bool> waitFor, System.Action<TState> callback) {
	        
	        await Coroutines.Waiter_INTERNAL(state, waitFor, callback);
	        
        }

        private static async System.Threading.Tasks.Task Waiter_INTERNAL<TState>(TState state, System.Func<TState, bool> waitFor, System.Action<TState> callback) {

	        while (waitFor.Invoke(state) == false) await System.Threading.Tasks.Task.Yield();
	        callback.Invoke(state);

        }
        
        private static async System.Threading.Tasks.Task Waiter_INTERNAL(System.Func<bool> waitFor, System.Action callback) {

	        while (waitFor.Invoke() == false) await System.Threading.Tasks.Task.Yield();
	        callback.Invoke();

        }
        
        public static Coroutine Run(IEnumerator coroutine) {

	        return Coroutines.instance.StartCoroutine(coroutine);

        }

        public static void Cancel(IEnumerator coroutine) {

	        Coroutines.instance.StopCoroutine(coroutine);

        }

        public static void Cancel(Coroutine coroutine) {

	        if (Coroutines.instance != null && coroutine != null) Coroutines.instance.StopCoroutine(coroutine);

        }

        private static bool MoveNext<T>(IEnumerator<T> ie, IEnumerable<T> collection) {

            var next = false;
            try {
                next = ie.MoveNext();
            } catch (System.Exception ex) {
                // collection was modified
                var info = string.Empty;
                foreach (var item in collection) {
                    info += item + "\n";
                }
                Debug.LogWarning("Exception caught while iterating the collection: " + ex.Message + "\n" + info);
	            throw ex;
            }

            return next;

        }

		public static void CallInSequence<T, TState>(System.Action<TState> callback, TState state, IList<T> collection, System.Action<T, System.Action, TState> each, bool waitPrevious = false) {

			if (collection == null) {

				if (callback != null) callback.Invoke(state);
				return;

			}

			var count = collection.Count();

			var completed = false;
			var counter = 0;
			System.Action callbackItem = () => {

				++counter;
				if (counter < count) return;

				completed = true;

				if (callback != null) callback.Invoke(state);
				
			};

			if (waitPrevious == true) {

				var ie = collection.GetEnumerator();

				System.Action doNext = null;
				doNext = () => {

                    if (Coroutines.MoveNext(ie, collection) == true) {

						if (ie.Current != null) {

							each(ie.Current, () => {
								
								callbackItem();
								doNext();

							}, state);

						} else {

							callbackItem();
							doNext();

						}

					}

				};
				doNext();

			} else {

                var ie = collection.GetEnumerator();
                while (Coroutines.MoveNext(ie, collection) == true) {

					if (ie.Current != null) {

						each(ie.Current, callbackItem, state);

					} else {

						callbackItem();

					}

					if (completed == true) break;

				}

			}

			if (count == 0 && callback != null) callback(state);

		}

		public delegate void ClosureDelegateCallback<T>(ref T obj);
		public delegate void ClosureDelegateCallbackContext<T>(WindowObject context, ref T obj);
		public delegate void ClosureDelegateCallbackContext<T, TC>(WindowObject context, ref T obj, TC custom);
		public delegate void ClosureDelegateEachCallback<in T, TClosure>(T item, ClosureDelegateCallback<TClosure> cb, ref TClosure obj);

		public interface ICallInSequenceClosure<T, TClosure> {

			internal int counter { get; set; }
			internal bool completed { get; set; }
			internal ClosureDelegateCallback<TClosure> callback { get; set; }
			internal ClosureDelegateEachCallback<T, TClosure> each { get; set; }
			internal ClosureDelegateCallback<TClosure> doNext { get; set; }
			internal ClosureDelegateCallback<TClosure> callbackItem { get; set; }
			internal IList<T> collection { get; set; }
			internal IEnumerator<T> ie { get; set; }

		}
		
		public static void CallInSequence<T, TClosure>(ref TClosure closure, ClosureDelegateCallback<TClosure> callback, IList<T> collection, ClosureDelegateEachCallback<T, TClosure> each, bool waitPrevious = false) where TClosure : ICallInSequenceClosure<T, TClosure> {
			
			if (collection == null) {

				if (callback != null) callback.Invoke(ref closure);
				return;

			}

			var count = collection.Count();

			closure.counter = count;
			closure.completed = false;
			closure.callback = callback;
			closure.collection = collection;
			closure.each = each;
			
			ClosureDelegateCallback<TClosure> callbackItem = static (ref TClosure cParamsInner) => {

				--cParamsInner.counter;
				if (cParamsInner.counter > 0) return;

				cParamsInner.completed = true;

				if (cParamsInner.callback != null) cParamsInner.callback.Invoke(ref cParamsInner);
				
			};
			closure.callbackItem = callbackItem;

			if (waitPrevious == true) {

				var ie = collection.GetEnumerator();
				closure.ie = ie;

				ClosureDelegateCallback<TClosure> doNext = null;
				doNext = static (ref TClosure cParamsInner) => {

					if (Coroutines.MoveNext(cParamsInner.ie, cParamsInner.collection) == true) {

						if (cParamsInner.ie.Current != null) {

							cParamsInner.each.Invoke(cParamsInner.ie.Current, static (ref TClosure cParams) => {
								
								cParams.callbackItem.Invoke(ref cParams);
								cParams.doNext(ref cParams);

							}, ref cParamsInner);

						} else {

							cParamsInner.callbackItem.Invoke(ref cParamsInner);
							cParamsInner.doNext.Invoke(ref cParamsInner);

						}

					}

				};
				closure.doNext = doNext;
				doNext.Invoke(ref closure);

			} else {

				var ie = collection.GetEnumerator();
				while (Coroutines.MoveNext(ie, collection) == true) {

					if (ie.Current != null) {

						each.Invoke(ie.Current, callbackItem, ref closure);

					} else {

						callbackItem.Invoke(ref closure);

					}

					if (closure.completed == true) break;

				}
				ie.Dispose();

			}

			if (count == 0 && callback != null) callback(ref closure);

		}

    }

}
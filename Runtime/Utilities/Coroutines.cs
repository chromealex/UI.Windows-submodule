using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows.Utilities {

    public class Coroutines : MonoBehaviour {

        private static Coroutines instance;

        public void Awake() {

            Coroutines.instance = this;

        }

        public static void NextFrame<T>(T state, System.Action<T> callback) {
	        
	        Coroutines.instance.StartCoroutine(Coroutines.WaitFrames_INTERNAL(state, callback, 1));
	        
        }

        private static IEnumerator WaitFrames_INTERNAL<T>(T state, System.Action<T> callback, int frames) {

	        while (frames > 0) {
		        
		        yield return null;
		        --frames;

	        }
	        callback.Invoke(state);

        }
        
        public static void NextFrame(System.Action callback) {
	        
	        Coroutines.instance.StartCoroutine(Coroutines.WaitFrames_INTERNAL(callback, 1));
	        
        }

        private static IEnumerator WaitFrames_INTERNAL(System.Action callback, int frames) {

	        while (frames > 0) {
		        
		        yield return null;
		        --frames;

	        }
	        callback.Invoke();

        }
        
        public static void WaitTime(float time, System.Action callback) {
	        
            Coroutines.instance.StartCoroutine(Coroutines.TimeWaiter_INTERNAL(time, callback));
	        
        }

        private static IEnumerator TimeWaiter_INTERNAL(float time, System.Action callback) {

            yield return new WaitForSecondsRealtime(time);
            callback.Invoke();

        }

        public static void WaitTime<TState>(TState state, float time, System.Action<TState> callback) {
	        
            Coroutines.instance.StartCoroutine(Coroutines.TimeWaiter_INTERNAL(state, time, callback));
	        
        }

        private static IEnumerator TimeWaiter_INTERNAL<TState>(TState state, float time, System.Action<TState> callback) {

            yield return new WaitForSecondsRealtime(time);
            callback.Invoke(state);

        }

        public static void Wait(System.Func<bool> waitFor, System.Action callback) {
	        
	        Coroutines.instance.StartCoroutine(Coroutines.Waiter_INTERNAL(waitFor, callback));
	        
        }

        public static void Wait<TState>(TState state, System.Func<TState, bool> waitFor, System.Action<TState> callback) {
	        
	        Coroutines.instance.StartCoroutine(Coroutines.Waiter_INTERNAL(state, waitFor, callback));
	        
        }

        private static IEnumerator Waiter_INTERNAL<TState>(TState state, System.Func<TState, bool> waitFor, System.Action<TState> callback) {

	        while (waitFor.Invoke(state) == false) yield return null;
	        callback.Invoke(state);

        }
        
        private static IEnumerator Waiter_INTERNAL(System.Func<bool> waitFor, System.Action callback) {

	        while (waitFor.Invoke() == false) yield return null;
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

        /*public static void CallInSequence<T, TState>(TState state, System.Action<TState> callback, System.Action<T, System.Action<TState>, TState> each, params T[] collection) {

	        Coroutines.CallInSequence(state, callback, (IEnumerable<T>)collection, each);

        }*/

		public static void CallInSequence<T>(System.Action callback, System.Action<T, System.Action> each, params T[] collection) {

			Coroutines.CallInSequence(callback, (IEnumerable<T>)collection, each);

		}

		public static void CallInSequence<T>(System.Action callback, bool waitPrevious, System.Action<T, System.Action> each, params T[] collection) {

			Coroutines.CallInSequence(callback, (IEnumerable<T>)collection, each, waitPrevious);

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

		public static void CallInSequence<T, TState>(System.Action<TState> callback, TState state, IEnumerable<T> collection, System.Action<T, System.Action, TState> each, bool waitPrevious = false) {

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

		public static void CallInSequence<T, TClosure>(ref TClosure closure, ClosureDelegateCallback<TClosure> callback, IEnumerable<T> collection, ClosureDelegateEachCallback<T, TClosure> each, bool waitPrevious = false) {
			
			if (collection == null) {

				if (callback != null) callback.Invoke(ref closure);
				return;

			}

			var count = collection.Count();

			var completed = false;
			var counter = 0;
			ClosureDelegateCallback<TClosure> callbackItem = (ref TClosure cParamsInner) => {

				++counter;
				if (counter < count) return;

				completed = true;

				if (callback != null) callback.Invoke(ref cParamsInner);
				
			};

			if (waitPrevious == true) {

				var ie = collection.GetEnumerator();

				ClosureDelegateCallback<TClosure> doNext = null;
				doNext = (ref TClosure cParamsInner) => {

					if (Coroutines.MoveNext(ie, collection) == true) {

						if (ie.Current != null) {

							each.Invoke(ie.Current, (ref TClosure cParams) => {
								
								callbackItem(ref cParams);
								doNext(ref cParams);

							}, ref cParamsInner);

						} else {

							callbackItem.Invoke(ref cParamsInner);
							doNext.Invoke(ref cParamsInner);

						}

					}

				};
				doNext.Invoke(ref closure);

			} else {

				var ie = collection.GetEnumerator();
				while (Coroutines.MoveNext(ie, collection) == true) {

					if (ie.Current != null) {

						each.Invoke(ie.Current, callbackItem, ref closure);

					} else {

						callbackItem.Invoke(ref closure);

					}

					if (completed == true) break;

				}

			}

			if (count == 0 && callback != null) callback(ref closure);

		}

		public static void CallInSequence<T>(System.Action callback, IEnumerable<T> collection, System.Action<T, System.Action> each, bool waitPrevious = false) {

			if (collection == null) {

				if (callback != null) callback.Invoke();
				return;

			}

			var count = collection.Count();

			var completed = false;
			var counter = 0;
			System.Action callbackItem = () => {

				++counter;
				if (counter < count) return;

				completed = true;

				if (callback != null) callback.Invoke();
				
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

							});

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

						each(ie.Current, callbackItem);

					} else {

						callbackItem();

					}

					if (completed == true) break;

				}

			}

			if (count == 0 && callback != null) callback();

		}

    }

}
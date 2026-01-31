using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.UI.Windows.Utilities {

    public class Coroutines : MonoBehaviour {

        private static Coroutines instance;

        public class EndOfFrameUpdateLoop { }

        public void Awake() {

            Coroutines.instance = this;
            
            var loop = UnityEngine.LowLevel.PlayerLoop.GetCurrentPlayerLoop();
            // Create a custom update system to replace an existing one
            var endOfFrameUpdate = new UnityEngine.LowLevel.PlayerLoopSystem() {
	            updateDelegate = this.EndOfFrameUpdate,
	            type = typeof(EndOfFrameUpdateLoop),
            };
            var index = 7;
            loop.subSystemList[index].subSystemList = loop.subSystemList[index].subSystemList.Append(endOfFrameUpdate).ToArray();
            UnityEngine.LowLevel.PlayerLoop.SetPlayerLoop(loop);

        }

        public void Update() {
	        
	        WaitTasks.Update();
			
        }

        public void EndOfFrameUpdate() {
	        
	        WaitTasks.EndOfFrameUpdate();
			
        }

        public static void ResolveWaitTasks() {
	        WaitTasks.Update();
	        WaitTasks.EndOfFrameUpdate();
        }

        private struct WaitFrameClosure {

	        public System.Action callback;
	        public int frame;

        }

        private struct WaitFrameClosure<T> {

	        public T state;
	        public System.Action<T> callback;
	        public int frame;

        }

        private struct WaitTimeClosure {

	        public System.Action callback;
	        public float time;

        }

        private struct WaitTimeClosure<T> {

	        public T state;
	        public System.Action<T> callback;
	        public float time;

        }

        public static void NextFrame<T>(T state, System.Action<T> callback) {
	        
	        WaitFrames(state, callback, 1);

        }

        public static void NextFrame(System.Action callback) {
	        
	        WaitFrames(callback, 1);

        }

        public static void WaitFrames<T>(T state, System.Action<T> callback, int frames) {
	        
	        WaitTasks.Add(new WaitFrameClosure<T>() { state = state, frame = Time.frameCount + frames, callback = callback }, static (t) => Time.frameCount >= t.frame, static (t) => t.callback.Invoke(t.state));

        }

        public static void WaitFrames(System.Action callback, int frames) {
	        
	        WaitTasks.Add(new WaitFrameClosure() { frame = Time.frameCount + frames, callback = callback }, static (t) => Time.frameCount >= t.frame, static (t) => t.callback.Invoke());

        }

        public static void WaitTime(float time, System.Action callback) {
	        
	        WaitTasks.Add(new WaitTimeClosure() { time = Time.time + time, callback = callback }, static (t) => Time.time >= t.time, static (t) => t.callback.Invoke());
            
        }

        public static void WaitTime<TState>(TState state, float time, System.Action<TState> callback) {
	        
	        WaitTasks.Add(new WaitTimeClosure<TState>() { state = state, time = Time.time + time, callback = callback }, static (t) => Time.time >= t.time, static (t) => t.callback.Invoke(t.state));

        }

        public static void Wait(System.Func<bool> waitFor, System.Action callback) {

	        WaitTasks.Add(waitFor, callback);
	        
        }

        public static void Wait<TState>(TState state, System.Func<TState, bool> waitFor, System.Action<TState> callback) {

	        WaitTasks.Add(state, waitFor, callback);
	        
        }

        public static void WaitEndOfFrame<T>(T state, System.Action<T> callback) {
	        
	        WaitTasks.AddEndOfFrame(state, static s => true, callback);

        }

        public static void WaitEndOfFrame(System.Action callback) {
	        
	        WaitTasks.AddEndOfFrame(static () => true, callback);

        }

        public static void WaitEndOfFrame(System.Func<bool> waitFor, System.Action callback) {

	        WaitTasks.AddEndOfFrame(waitFor, callback);
	        
        }

        public static void WaitEndOfFrame<TState>(TState state, System.Func<TState, bool> waitFor, System.Action<TState> callback) {

	        WaitTasks.AddEndOfFrame(state, waitFor, callback);
	        
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

        private static bool MoveNext<T>(ref List<T>.Enumerator ie, IList<T> collection) {

	        var next = false;
	        try {
		        next = ie.MoveNext();
	        } catch (System.Exception ex) {
		        // collection was modified
		        var info = string.Empty;
		        foreach (var item in collection) {
			        info += item + "\n";
		        }
		        Debug.LogWarning($"Exception caught while iterating the collection: {ex.Message}\n{info}");
		        throw ex;
	        }

	        return next;

        }

        private static bool MoveNext<T>(ref SZArrayEnumerator<T> ie, T[] collection) {

	        var next = false;
	        try {
		        next = ie.MoveNext();
	        } catch (System.Exception ex) {
		        // collection was modified
		        var info = string.Empty;
		        foreach (var item in collection) {
			        info += item + "\n";
		        }
		        Debug.LogWarning($"Exception caught while iterating the collection: {ex.Message}\n{info}");
		        throw ex;
	        }

	        return next;

        }

		public delegate void ClosureDelegateCallback<T>(ref T obj);
		public delegate void ClosureDelegateCallbackContext<T>(WindowObject context, ref T obj);
		public delegate void ClosureDelegateCallbackContext<T, TC>(WindowObject context, ref T obj, TC custom);
		public delegate void ClosureDelegateEachCallback<in T, TClosure>(T item, ClosureDelegateCallback<TClosure> cb, ref TClosure obj);

		public interface ICallInSequenceClosure<T, TClosure> {

			internal int counter { get; set; }
			public int index { get; set; }
			internal bool completed { get; set; }
			internal ClosureDelegateCallback<TClosure> callback { get; set; }
			internal ClosureDelegateEachCallback<T, TClosure> each { get; set; }
			internal ClosureDelegateCallback<TClosure> doNext { get; set; }
			internal ClosureDelegateCallback<TClosure> callbackItem { get; set; }
			internal List<T> collection { get; set; }
			internal T[] collectionArr { get; set; }
			internal List<T>.Enumerator ie { get; set; }
			internal SZArrayEnumerator<T> ieArr { get; set; }

		}
		
		public static void CallInSequence<T, TClosure>(ref TClosure closure, ClosureDelegateCallback<TClosure> callback, List<T> collection, ClosureDelegateEachCallback<T, TClosure> each, bool waitPrevious = false) where TClosure : ICallInSequenceClosure<T, TClosure> {
			
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
				if (cParamsInner.counter != 0) return;

				cParamsInner.completed = true;

				if (cParamsInner.callback != null) cParamsInner.callback.Invoke(ref cParamsInner);
				
			};
			closure.callbackItem = callbackItem;

			if (waitPrevious == true) {

				var ie = collection.GetEnumerator();
				closure.ie = ie;

				ClosureDelegateCallback<TClosure> doNext = null;
				doNext = static (ref TClosure cParamsInner) => {

					var ie = cParamsInner.ie;
					if (Coroutines.MoveNext(ref ie, cParamsInner.collection) == true) {
						cParamsInner.ie = ie;
						
						if (cParamsInner.ie.Current != null) {

							cParamsInner.each.Invoke(cParamsInner.ie.Current, static (ref TClosure cParams) => {
								
								cParams.callbackItem.Invoke(ref cParams);
								cParams.doNext(ref cParams);
								++cParams.index;

							}, ref cParamsInner);

						} else {

							cParamsInner.callbackItem.Invoke(ref cParamsInner);
							cParamsInner.doNext.Invoke(ref cParamsInner);
							++cParamsInner.index;

						}

					} else {
						cParamsInner.ie = ie;
					}

				};
				closure.index = 0;
				closure.doNext = doNext;
				doNext.Invoke(ref closure);

			} else {

				var ie = collection.GetEnumerator();
				var idx = 0;
				while (Coroutines.MoveNext(ref ie, collection) == true) {

					closure.index = idx;
					if (ie.Current != null) {

						each.Invoke(ie.Current, callbackItem, ref closure);

					} else {

						callbackItem.Invoke(ref closure);

					}

					if (closure.completed == true) break;

					++idx;

				}
				ie.Dispose();

			}

			if (count == 0 && callback != null) callback(ref closure);

		}

		public static void CallInSequence<T, TClosure>(ref TClosure closure, ClosureDelegateCallback<TClosure> callback, T[] collection, ClosureDelegateEachCallback<T, TClosure> each, bool waitPrevious = false) where TClosure : ICallInSequenceClosure<T, TClosure> {
			
			if (collection == null) {

				if (callback != null) callback.Invoke(ref closure);
				return;

			}

			var count = collection.Count();

			closure.counter = count;
			closure.completed = false;
			closure.callback = callback;
			closure.collectionArr = collection;
			closure.each = each;
			
			ClosureDelegateCallback<TClosure> callbackItem = static (ref TClosure cParamsInner) => {

				--cParamsInner.counter;
				if (cParamsInner.counter != 0) return;

				cParamsInner.completed = true;

				if (cParamsInner.callback != null) cParamsInner.callback.Invoke(ref cParamsInner);
				
			};
			closure.callbackItem = callbackItem;

			if (waitPrevious == true) {

				var ie = new SZArrayEnumerator<T>(collection);
				closure.ieArr = ie;

				ClosureDelegateCallback<TClosure> doNext = null;
				doNext = static (ref TClosure cParamsInner) => {

					var ie = cParamsInner.ieArr;
					if (Coroutines.MoveNext(ref ie, cParamsInner.collectionArr) == true) {
						cParamsInner.ieArr = ie;

						if (cParamsInner.ie.Current != null) {

							cParamsInner.each.Invoke(cParamsInner.ie.Current, static (ref TClosure cParams) => {
								
								cParams.callbackItem.Invoke(ref cParams);
								cParams.doNext(ref cParams);

							}, ref cParamsInner);

						} else {

							cParamsInner.callbackItem.Invoke(ref cParamsInner);
							cParamsInner.doNext.Invoke(ref cParamsInner);

						}

					} else {
						cParamsInner.ieArr = ie;
					}

				};
				closure.doNext = doNext;
				doNext.Invoke(ref closure);

			} else {

				var ie = new SZArrayEnumerator<T>(collection);
				while (Coroutines.MoveNext(ref ie, collection) == true) {

					if (ie.Current != null) {

						each.Invoke(ie.Current, callbackItem, ref closure);

					} else {

						callbackItem.Invoke(ref closure);

					}

					if (closure.completed == true) break;

				}

			}

			if (count == 0 && callback != null) callback(ref closure);

		}

		internal struct SZArrayEnumerator<T> : IEnumerator {
			private readonly T[] array; 
			private int index; 
			private readonly int endIndex; // cache array length, since it's a little slow.
 
			internal SZArrayEnumerator(T[] array) {
				this.array = array;
				this.index = -1;
				this.endIndex = array.Length;
			} 
 
			public bool MoveNext() { 
				if (this.index < this.endIndex) {
					this.index++; 
					return (this.index < this.endIndex); 
				}
				return false; 
			}

			public T Current => this.array[this.index];

			object IEnumerator.Current => throw new System.Exception();

			public void Reset() {
				this.index = -1;
			} 
		}

    }

}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEngine.UI.Windows.Utilities {

    public class Coroutines : MonoBehaviour {

        private static Coroutines instance;

        public void Awake() {

            Coroutines.instance = this;

        }

        public static void Run(IEnumerator coroutine) {

            Coroutines.instance.StartCoroutine(coroutine);

        }

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

				if (callback != null) callback();
				
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
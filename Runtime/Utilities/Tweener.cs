using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace UnityEngine.UI.Windows.Utilities {

    public class Tweener : MonoBehaviour {

        public enum EaseFunction {

            Linear,

        }

        public static class EaseFunctions {

            public static System.Func<float, float, float, float>[] funcs = new System.Func<float, float, float, float>[] {
                (from, to, t) => { return Mathf.Lerp(from, to, t); },
            };

        }

        public interface ITween {

            bool Update(float dt);
            bool HasTag(object tag);
            void Stop();

        }

        internal interface ITweenInternal {

            object GetTag();
            float GetTimer();
            float GetDelay();
            float GetDuration();
            float GetFrom();
            float GetTo();

        }

        public class Tween<T> : ITween, ITweenInternal {

            internal T obj;
            internal float duration;
            internal float from;
            internal float to;
            internal object tag;
            internal float delay;

            internal System.Action<T> onComplete;
            internal System.Action onCompleteParameterless;
            internal System.Action<T, float> onUpdate;
            internal System.Action onCancel;

            internal float timer;

            private EaseFunction easeFunction;

            float ITweenInternal.GetTimer() { return this.timer; }
            float ITweenInternal.GetDelay() { return this.delay; }
            float ITweenInternal.GetDuration() { return this.duration; }
            float ITweenInternal.GetFrom() { return this.from; }
            float ITweenInternal.GetTo() { return this.to; }
            object ITweenInternal.GetTag() { return this.tag; }
            
            public bool Update(float dt) {

                this.delay -= dt;
                if (this.delay <= 0f) {

                    this.timer += dt / this.duration;

                    try {

                        if (this.onUpdate != null) this.onUpdate.Invoke(this.obj, EaseFunctions.funcs[(int)this.easeFunction].Invoke(this.@from, this.to, this.timer));

                        if (this.timer >= 1f) {

                            if (this.onComplete != null) this.onComplete.Invoke(this.obj);
                            if (this.onCompleteParameterless != null) this.onCompleteParameterless.Invoke();
                            return true;

                        }

                    } catch (System.Exception ex) {

                        Debug.LogException(ex);

                    }
                    
                    if (this.timer >= 1f) {

                        return true;

                    }

                }

                return false;

            }

            void ITween.Stop() {

                if (this.timer < 1f) {

                    if (this.onCancel != null) this.onCancel.Invoke();

                }

                this.delay = 0f;
                this.timer = 1f;

            }

            bool ITween.HasTag(object tag) {

                return this.tag == tag;

            }

            public Tween<T> Tag(object tag) {

                this.tag = tag;
                return this;

            }

            public Tween<T> Delay(float delay) {

                this.delay = delay;
                return this;

            }

            public Tween<T> Ease(EaseFunction easeFunction) {

                this.easeFunction = easeFunction;
                return this;

            }

            public Tween<T> OnComplete(System.Action<T> onResult) {

                this.onComplete = onResult;
                return this;

            }

            public Tween<T> OnComplete(System.Action onResult) {

                this.onCompleteParameterless = onResult;
                return this;

            }

            public Tween<T> OnCancel(System.Action onResult) {

                this.onCancel = onResult;
                return this;

            }

            public Tween<T> OnUpdate(System.Action<T, float> onResult) {

                this.onUpdate = onResult;
                return this;

            }

        }

        public List<ITween> tweens = new List<ITween>();
        private ITween tween;

        public Tween<T> Add<T>(T obj, float duration, float from, float to) {

            var tween = new Tweener.Tween<T>();
            tween.obj = obj;
            tween.duration = duration;
            tween.from = from;
            tween.to = to;

            this.tween = tween;
            this.tweens.Add(tween);

            return tween;

        }

        public void Stop(object tag) {

            for (int i = this.tweens.Count - 1; i >= 0; --i) {

                if (this.tweens[i].HasTag(tag) == true) {

                    this.tweens[i].Stop();
                    this.tweens.RemoveAt(i);

                }

            }

        }

        public void Update() {
            
            var dt = Time.deltaTime;
            for (int i = this.tweens.Count - 1; i >= 0; --i) {

                if (this.tweens[i].Update(dt) == true) {

                    this.tweens.RemoveAt(i);

                }

            }

        }

    }

}
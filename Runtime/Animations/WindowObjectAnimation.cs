using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Modules {

    public static class WindowObjectAnimation {

        private struct AnimationInfo<T> {

            public AnimationParameters animationParameters;
            public AnimationParameters.State fromState;
            public AnimationParameters.State toState;
            public System.Action<T> onComplete;
            public T closureParameters;

        }

        private struct AnimationGroupInfo<T> {

            public System.Action<T> onComplete;
            public T closureParameters;

        }

        public static void SetResetState(WindowObject instance) {

            if (instance.animationParameters.items == null) return;

            for (int i = 0; i < instance.animationParameters.items.Length; ++i) {

                var anim = instance.animationParameters.items[i];
                if (anim != null) {
                    
                    var state = anim.GetResetState();
                    anim.ApplyState(state);

                }

            }
            
        }

        public static void Show<T>(T closureParameters, WindowObject instance, TransitionParameters parameters, System.Action<T> onComplete) {

            WindowObjectAnimation.Play(closureParameters, AnimationState.Show, instance.animationParameters.items, parameters, onComplete);

        }

        public static void Hide<T>(T closureParameters, WindowObject instance, TransitionParameters parameters, System.Action<T> onComplete) {

            WindowObjectAnimation.Play(closureParameters, AnimationState.Hide, instance.animationParameters.items, parameters, onComplete);

        }

        private static void Play<T>(T closureParameters, AnimationState animationState, AnimationParameters[] animationParameters, TransitionParameters parameters,
                                    System.Action<T> onComplete) {

            if (animationParameters == null || animationParameters.Length == 0) {

                onComplete.Invoke(closureParameters);
                return;

            }

            if (parameters.IsImmediately() == true) {

                for (int i = 0; i < animationParameters.Length; ++i) {

                    var anim = animationParameters[i];
                    if (anim != null) {

                        anim.ApplyState(anim.GetState(animationState));

                    }

                }

                onComplete.Invoke(closureParameters);
                return;

            }

            var animationGroupInfo = new AnimationGroupInfo<T>();
            animationGroupInfo.closureParameters = closureParameters;
            animationGroupInfo.onComplete = onComplete;
            UnityEngine.UI.Windows.Utilities.Coroutines.CallInSequence(animationGroupInfo, (x) => {
                
                x.onComplete.Invoke(x.closureParameters);
                
            }, animationParameters, (anim, cb, state) => {
                
                if (anim != null) {

                    AnimationParameters.State fromState = null;
                    if (parameters.resetAnimation == true) {

                        fromState = anim.GetState(AnimationState.Reset);

                    }

                    var toState = anim.GetState(animationState);

                    var animationInfo = new AnimationInfo<AnimationGroupInfo<T>>() {
                        animationParameters = anim,
                        fromState = fromState,
                        toState = toState,
                        onComplete = cb,
                        closureParameters = state,
                    };

                    var ease = (animationState == AnimationState.Show ? anim.easeShow : anim.easeHide);
                    var tweener = WindowSystem.GetTweener();
                    tweener.Stop(anim);
                    tweener.Add(animationInfo, anim.GetDuration(animationState), 0f, 1f)
                           .Delay(anim.GetDelay(animationState))
                           .Tag(anim)
                           .Ease(ease)
                           .OnUpdate((obj, value) => { obj.animationParameters.ApplyState(obj.animationParameters.LerpState(obj.fromState, obj.toState, value)); })
                           .OnComplete((obj) => {
                               
                               obj.onComplete.Invoke(obj.closureParameters);
                               
                           });

                } else {
                    
                    cb.Invoke(state);
                    
                }
                
            }, waitPrevious: false);
            
        }

    }

}
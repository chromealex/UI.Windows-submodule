using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Modules {

    public static class WindowObjectAnimation {

        private struct AnimationInfo<T> {

            public AnimationParameters animationParameters;
            public AnimationParameters.State fromState;
            public AnimationParameters.State toState;
            public System.Action onComplete;
            public T closureParameters;

        }

        private struct AnimationGroupInfo<T> {

            public TransitionParameters transitionParameters;
            public AnimationState animationState;
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

            if (parameters.data.immediately == true) {
                
                var tweener = WindowSystem.GetTweener();
                
                for (int i = 0; i < animationParameters.Length; ++i) {

                    var anim = animationParameters[i];
                    if (anim != null) {

                        tweener.Stop(anim);
                        anim.ApplyState(anim.GetState(animationState));

                    }

                }

                onComplete.Invoke(closureParameters);
                return;

            }

            var animationGroupInfo = new AnimationGroupInfo<T> {
                transitionParameters = parameters,
                animationState = animationState,
                closureParameters = closureParameters,
                onComplete = onComplete,
            };
            UnityEngine.UI.Windows.Utilities.Coroutines.CallInSequence((x) => {
                
                x.onComplete.Invoke(x.closureParameters);
                
            }, animationGroupInfo, animationParameters, (anim, cb, state) => {
                
                if (anim != null) {

                    AnimationParameters.State fromState = null;
                    if (state.transitionParameters.data.resetAnimation == true) {

                        fromState = anim.GetState(AnimationState.Reset, true);

                    } else {

                        fromState = anim.GetState(AnimationState.Current, true);

                    }

                    var toState = anim.GetState(state.animationState);

                    var animationInfo = new AnimationInfo<AnimationGroupInfo<T>>() {
                        animationParameters = anim,
                        fromState = fromState,
                        toState = toState,
                        onComplete = cb,
                        closureParameters = state,
                    };

                    var ease = (state.animationState == AnimationState.Show ? anim.easeShow : anim.easeHide);
                    var tweener = WindowSystem.GetTweener();
                    tweener.Stop(anim);
                    tweener.Add(animationInfo, anim.GetDuration(state.animationState), 0f, 1f)
                           .Delay(state.transitionParameters.data.delay > 0f ? state.transitionParameters.data.delay : anim.GetDelay(state.animationState))
                           .Tag(anim)
                           .Ease(ease)
                           .OnUpdate((obj, value) => {
                               
                               obj.animationParameters.ApplyState(obj.animationParameters.LerpState(obj.fromState, obj.toState, value));
                               
                           })
                           .OnComplete((obj) => {

                               obj.fromState.Recycle();
                               obj.onComplete.Invoke();
                               
                           })
                           .OnCancel((obj) => {
                               
                               obj.fromState.Recycle();
                               obj.onComplete.Invoke();
                               
                           });

                } else {
                    
                    cb.Invoke();
                    
                }
                
            }, waitPrevious: false);
            
        }

    }

}
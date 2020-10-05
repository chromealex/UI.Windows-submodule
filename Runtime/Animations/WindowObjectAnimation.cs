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

        public static void SetResetState(WindowObject instance) {

            if (instance.animationParameters == null) return;

            var state = instance.animationParameters.GetResetState();
            instance.animationParameters.ApplyState(state);

        }

        public static void Show<T>(T closureParameters, WindowObject instance, TransitionParameters parameters, System.Action<T> onComplete) {

            WindowObjectAnimation.Play(closureParameters, AnimationState.Show, instance.animationParameters, parameters, onComplete);

        }

        public static void Hide<T>(T closureParameters, WindowObject instance, TransitionParameters parameters, System.Action<T> onComplete) {

            WindowObjectAnimation.Play(closureParameters, AnimationState.Hide, instance.animationParameters, parameters, onComplete);

        }

        private static void Play<T>(T closureParameters, AnimationState animationState, AnimationParameters animationParameters, TransitionParameters parameters,
                                    System.Action<T> onComplete) {

            if (animationParameters == null) {

                onComplete.Invoke(closureParameters);
                return;

            }

            if (parameters.IsImmediately() == true) {

                animationParameters.ApplyState(animationParameters.GetState(animationState));
                onComplete.Invoke(closureParameters);
                return;

            }
            
            AnimationParameters.State fromState = null;
            if (parameters.resetAnimation == true) {

                fromState = animationParameters.GetState(AnimationState.Reset);

            }

            var toState = animationParameters.GetState(animationState);

            var animationInfo = new AnimationInfo<T>() {
                animationParameters = animationParameters,
                fromState = fromState,
                toState = toState,
                onComplete = onComplete,
                closureParameters = closureParameters,
            };

            var tweener = WindowSystem.GetTweener();
            tweener.Stop(animationParameters);
            tweener.Add(animationInfo, animationParameters.GetDuration(animationState), 0f, 1f).Delay(animationParameters.GetDelay(animationState)).Tag(animationParameters)
                   .OnUpdate((obj, value) => { obj.animationParameters.ApplyState(obj.animationParameters.LerpState(obj.fromState, obj.toState, value)); })
                   .OnComplete((obj) => { obj.onComplete.Invoke(obj.closureParameters); });

        }

    }

}
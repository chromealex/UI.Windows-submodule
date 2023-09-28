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
            public System.Action<T, float> onUpdate;
            public System.Action<T> onCancel;
            public T closureParameters;
            public TweenerCustomParameters customParameters;

        }

        [System.Serializable]
        public struct TweenerCustomParameters {

            public int loops;
            public bool reflect;

        }

        public static void SetResetState(WindowObject instance) {

            WindowObjectAnimation.SetResetState(instance.animationParameters.items);

        }

        public static void SetResetState(AnimationParameters[] animationParameters) {

            if (animationParameters == null) return;
            
            for (int i = 0; i < animationParameters.Length; ++i) {

                var anim = animationParameters[i];
                if (anim != null) {

                    if (anim.config != null) {
                        anim.config.Apply(anim);
                    }
                    var state = anim.GetResetState();
                    anim.ApplyState(state);

                }

            }
            
        }

        public static void BreakState(WindowObject instance) {

            WindowObjectAnimation.BreakState(instance.animationParameters.items);

        }

        public static void BreakState(AnimationParameters[] animationParameters) {
            
            if (animationParameters == null) return;

            var tweener = WindowSystem.GetTweener();
            for (int i = 0; i < animationParameters.Length; ++i) {

                var anim = animationParameters[i];
                if (anim != null) {
                    
                    tweener.Stop(anim, ignoreEvents: false);

                }

            }
            
        }

        public static void SetState(WindowObject instance, AnimationState state) {
            
            if (instance.animationParameters.items == null) return;

            for (int i = 0; i < instance.animationParameters.items.Length; ++i) {

                var anim = instance.animationParameters.items[i];
                if (anim != null) {
                    
                    var stateData = anim.GetState(state);
                    anim.ApplyState(stateData);

                }

            }
            
        }

        public static void Show<T>(T closureParameters, WindowObject instance, TransitionParameters parameters, System.Action<T> onComplete) {

            WindowObjectAnimation.Play(closureParameters, AnimationState.Show, instance.animationParameters.items, parameters, default, onComplete);

        }

        public static void Hide<T>(T closureParameters, WindowObject instance, TransitionParameters parameters, System.Action<T> onComplete) {

            WindowObjectAnimation.Play(closureParameters, AnimationState.Hide, instance.animationParameters.items, parameters, default, onComplete);

        }

        internal static void Play<T>(T closureParameters,
                                     AnimationState animationState,
                                     AnimationParameters[] animationParameters,
                                     TransitionParameters parameters,
                                     TweenerCustomParameters customParameters,
                                     System.Action<T> onComplete,
                                     System.Action<T, float> onUpdate = null,
                                     System.Action<T> onCancel = null) {

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
                customParameters = customParameters,
                onUpdate = onUpdate,
                onCancel = onCancel,
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
                    var tween = tweener.Add(animationInfo, anim.GetDuration(state.animationState), 0f, 1f);
                    tween.Delay(state.transitionParameters.data.replaceDelay == true ? state.transitionParameters.data.delay : anim.GetDelay(state.animationState))
                    .Tag(anim)
                    .Ease(ease)
                    .OnUpdate((obj, value) => {
                        
                        if (obj.closureParameters.onUpdate != null) obj.closureParameters.onUpdate.Invoke(obj.closureParameters.closureParameters, value); 
                        obj.animationParameters.ApplyState(obj.animationParameters.LerpState(obj.fromState, obj.toState, value));
                       
                    })
                    .OnComplete((obj) => {

                        obj.fromState.Recycle(); 
                        obj.onComplete.Invoke();
                       
                    })
                    .OnCancel((obj) => {
                       
                        if (obj.closureParameters.onCancel != null) obj.closureParameters.onCancel.Invoke(obj.closureParameters.closureParameters);
                        obj.fromState.Recycle();
                        obj.onComplete.Invoke();
                       
                    });

                    if (customParameters.loops != 0) {

                        tween.Loop(customParameters.loops);

                    }

                    if (customParameters.reflect == true) {
                        
                        tween.Reflect();

                    }
                    
                } else {
                    
                    cb.Invoke();
                    
                }
                
            }, waitPrevious: false);
            
        }

    }

}
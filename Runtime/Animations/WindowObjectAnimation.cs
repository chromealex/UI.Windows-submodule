namespace UnityEngine.UI.Windows.Modules {
    
    using UnityEngine.UI.Windows.Utilities;

    public static class WindowObjectAnimation {

        private struct AnimationInfo<T> {

            public AnimationParameters animationParameters;
            public AnimationParameters.State fromState;
            public AnimationParameters.State toState;
            public Coroutines.ClosureDelegateCallback<T> onComplete;
            public T closureParameters;

        }

        private class AnimationGroupInfo<T> : Coroutines.ICallInSequenceClosure<AnimationParameters, AnimationGroupInfo<T>> {

            public TransitionParameters transitionParameters;
            public AnimationState animationState;
            public System.Action<T> onComplete;
            public System.Action<T, float> onUpdate;
            public System.Action<T> onCancel;
            public T closureParameters;
            public TweenerCustomParameters customParameters;
            public WindowObject instance;

            int Coroutines.ICallInSequenceClosure<AnimationParameters, AnimationGroupInfo<T>>.counter { get; set; }
            bool Coroutines.ICallInSequenceClosure<AnimationParameters, AnimationGroupInfo<T>>.completed { get; set; }
            Coroutines.ClosureDelegateCallback<AnimationGroupInfo<T>> Coroutines.ICallInSequenceClosure<AnimationParameters, AnimationGroupInfo<T>>.callback { get; set; }
            Coroutines.ClosureDelegateEachCallback<AnimationParameters, AnimationGroupInfo<T>> Coroutines.ICallInSequenceClosure<AnimationParameters, AnimationGroupInfo<T>>.each { get; set; }
            Coroutines.ClosureDelegateCallback<AnimationGroupInfo<T>> Coroutines.ICallInSequenceClosure<AnimationParameters, AnimationGroupInfo<T>>.doNext { get; set; }
            Coroutines.ClosureDelegateCallback<AnimationGroupInfo<T>> Coroutines.ICallInSequenceClosure<AnimationParameters, AnimationGroupInfo<T>>.callbackItem { get; set; }
            System.Collections.Generic.List<AnimationParameters> Coroutines.ICallInSequenceClosure<AnimationParameters, AnimationGroupInfo<T>>.collection { get; set; }
            AnimationParameters[] Coroutines.ICallInSequenceClosure<AnimationParameters, AnimationGroupInfo<T>>.collectionArr { get; set; }
            System.Collections.Generic.List<AnimationParameters>.Enumerator Coroutines.ICallInSequenceClosure<AnimationParameters, AnimationGroupInfo<T>>.ie { get; set; }
            Coroutines.SZArrayEnumerator<AnimationParameters> Coroutines.ICallInSequenceClosure<AnimationParameters, AnimationGroupInfo<T>>.ieArr { get; set; }

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

            WindowObjectAnimation.Play(closureParameters, instance, AnimationState.Show, instance.animationParameters.items, parameters, default, onComplete);

        }

        public static void Hide<T>(T closureParameters, WindowObject instance, TransitionParameters parameters, System.Action<T> onComplete) {

            WindowObjectAnimation.Play(closureParameters, instance, AnimationState.Hide, instance.animationParameters.items, parameters, default, onComplete);

        }

        internal static void Play<T>(T closureParameters,
                                     WindowObject instance,
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

            var animationGroupInfo = PoolClass<AnimationGroupInfo<T>>.Spawn();
            animationGroupInfo.instance = instance;
            animationGroupInfo.transitionParameters = parameters;
            animationGroupInfo.animationState = animationState;
            animationGroupInfo.closureParameters = closureParameters;
            animationGroupInfo.customParameters = customParameters;
            animationGroupInfo.onUpdate = onUpdate;
            animationGroupInfo.onCancel = onCancel;
            animationGroupInfo.onComplete = onComplete;
            
            Coroutines.CallInSequence(ref animationGroupInfo, static (ref AnimationGroupInfo<T> x) => {
                
                x.onComplete.Invoke(x.closureParameters);
                PoolClass<AnimationGroupInfo<T>>.Recycle(x);

            }, animationParameters, static (AnimationParameters anim, Coroutines.ClosureDelegateCallback<AnimationGroupInfo<T>> cb, ref AnimationGroupInfo<T> state) => {
                
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

                    var prevRandomState = Random.state;
                    Random.InitState(Mathf.Abs(state.instance.GetInstanceID()));
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
                        obj.onComplete.Invoke(ref obj.closureParameters);
                       
                    })
                    .OnCancel((obj) => {
                       
                        if (obj.closureParameters.onCancel != null) obj.closureParameters.onCancel.Invoke(obj.closureParameters.closureParameters);
                        obj.fromState.Recycle();
                        obj.onComplete.Invoke(ref obj.closureParameters);
                       
                    });

                    if (state.customParameters.loops != 0) {

                        tween.Loop(state.customParameters.loops);

                    }

                    if (state.customParameters.reflect == true) {
                        
                        tween.Reflect();

                    }

                    Random.state = prevRandomState;

                } else {
                    
                    cb.Invoke(ref state);
                    
                }
                
            }, waitPrevious: false);
            
        }

    }

}
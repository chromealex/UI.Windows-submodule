using UnityEngine;

namespace ME.Taptic {

    public enum TapticType : byte {

        None = 0,
        Success,
        Warning,
        Failure,
        Light,
        Medium,
        Heavy,
        Vibrate, // default
        Selection,
        Curve,

    }

    public interface ITapticEngineInternal : ITapticEngine {

        void Play(TapticType type, float duration, float strength);

    }

    public interface ITapticEngine {

        ITapticEngine SetActiveModule(ITapticModule module);

        void PlaySingle(TapticType type);
        void PlayCurve(AnimationCurve curve, bool randomize);
        void SetMaxChannels(int value);

        void Mute();
        void Unmute();
        bool IsMuted();

        bool IsPlaying();
        bool IsSupported();

        void Update();

    }

    public interface ITapticModule {

        void PlaySingle(TapticType type);
        void PlayCurve(AnimationCurve curve, bool randomize);
        void SetMaxChannels(int value);
        bool IsPlaying();

    }

    public interface ITapticBridge {

        void Play(TapticType type, float duration, float strength);
        bool IsSupported();

    }

    public interface ITapticModuleInternal : ITapticModule {

        void Initialize(ITapticEngineInternal engine);
        void Update();

    }

    public class TapticEngine : ITapticEngineInternal {

        private ITapticModuleInternal activeModule;
        private readonly ITapticBridge bridge;

        private bool isMuted = false;

        public TapticEngine(bool logs = true) {

            #if UNITY_IOS && !UNITY_EDITOR
            this.bridge = new TapticBridgeiOS();
            #elif UNITY_ANDROID && !UNITY_EDITOR
            this.bridge = new TapticBridgeAndroid();
            #else
            this.bridge = new TapticBridgeDummy() { logs = logs, };
            #endif

        }

        void ITapticEngineInternal.Play(TapticType type, float duration, float strength) {

            if (this.isMuted == true) {
                return;
            }

            this.bridge.Play(type, duration, strength);

        }

        ITapticEngine ITapticEngine.SetActiveModule(ITapticModule module) {

            if (this.IsSupported_INTERNAL() == false) {
                return this;
            }

            this.activeModule = module as ITapticModuleInternal;
            if (this.activeModule != null) {
                this.activeModule.Initialize(this);
            }

            return this;

        }

        void ITapticEngine.Mute() {

            this.isMuted = true;

        }

        void ITapticEngine.Unmute() {

            this.isMuted = false;

        }

        bool ITapticEngine.IsMuted() {

            return this.isMuted;

        }

        bool ITapticEngine.IsSupported() {

            return this.IsSupported_INTERNAL();

        }

        private bool IsSupported_INTERNAL() {

            if (this.bridge != null) {
                return this.bridge.IsSupported();
            }

            return false;

        }

        bool ITapticEngine.IsPlaying() {

            if (this.activeModule != null) {
                return this.activeModule.IsPlaying();
            }

            return false;

        }

        void ITapticEngine.PlayCurve(AnimationCurve curve, bool randomize) {

            if (this.isMuted == true) {
                return;
            }

            if (this.activeModule != null) {

                this.activeModule.PlayCurve(curve, randomize);

            }

        }

        void ITapticEngine.SetMaxChannels(int maxChannels) {

            if (this.activeModule != null) {

                this.activeModule.SetMaxChannels(maxChannels);

            }

        }

        void ITapticEngine.PlaySingle(TapticType type) {

            if (this.isMuted == true) {
                return;
            }

            if (this.activeModule != null) {

                this.activeModule.PlaySingle(type);

            }

        }

        void ITapticEngine.Update() {

            if (this.activeModule != null) {

                this.activeModule.Update();

            }

        }

    }

}
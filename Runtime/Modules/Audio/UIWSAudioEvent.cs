namespace UI.Windows.Runtime.Modules.Audio {

    [UnityEngine.CreateAssetMenu(menuName = "UI.Windows/Audio/Event")]
    public class UIWSAudioEvent : UnityEngine.ScriptableObject {

        [System.Serializable]
        public struct Parameters {

            public int maxCount;

            public bool changePitch;
            public bool randomPitch;
            public UnityEngine.Vector2 randomPitchValue;
            [UnityEngine.RangeAttribute(-3f, 3f)]
            public float pitchValue;
            
            [UnityEngine.SpaceAttribute(10f)]
            public bool changeVolume;
            public bool randomVolume;
            public UnityEngine.Vector2 randomVolumeValue;
            [UnityEngine.RangeAttribute(0f, 1f)]
            public float volumeValue;

        }

        public UnityEngine.UI.Windows.WindowSystemAudio.EventType eventType;
        
        [UnityEngine.HeaderAttribute("Audio")]
        public UnityEngine.AudioClip audioClip;
        public UnityEngine.AudioClip[] randomClips;

        public Parameters parameters = new Parameters() {
            pitchValue = 1f,
            volumeValue = 1f,
        };

        [UnityEngine.HeaderAttribute("Music")]
        public UnityEngine.UI.Windows.WindowSystemAudio.Behaviour behaviour;
        [UnityEngine.RangeAttribute(1, UnityEngine.UI.Windows.WindowSystemAudio.CHANNELS_COUNT)]
        public int musicChannel;

        [UnityEngine.HeaderAttribute("Vibration")]
        public bool vibrate;
        public bool tapticByCurve;
        public ME.Taptic.TapticType taptic;
        public UnityEngine.AnimationCurve tapticCurve;
        
        public void Play() {

            var audio = UnityEngine.UI.Windows.WindowSystem.GetAudio();
            if (audio == null) {
                UnityEngine.Debug.LogWarning("No audio module found. Did you forget to add Audio Module to your WindowSystem initializer?");
                return;
            }
            
            audio.Play(this);

        }

        public void Stop() {
            
            var audio = UnityEngine.UI.Windows.WindowSystem.GetAudio();
            if (audio == null) {
                UnityEngine.Debug.LogWarning("No audio module found. Did you forget to add Audio Module to your WindowSystem initializer?");
                return;
            }
            
            audio.Stop(this);

        }

    }

}
namespace UnityEngine.UI.Windows {

    public class WindowSystemAudio : MonoBehaviour {

        public AudioSource audioSource;

        public void Play(AudioClip clip) {
            
            this.audioSource.PlayOneShot(clip);
            
        }

        public void Play(AudioClip[] clips) {
            
            this.audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
            
        }

    }

    [System.Serializable]
    public struct ComponentAudio {

        public WindowEvent play;
        public WindowEvent stop;

        private System.Action onPlayCallback;
        private System.Action onStopCallback;

        public void Initialize(WindowObject handler) {

            if (this.play == WindowEvent.None && this.stop == WindowEvent.None) return;

            this.onPlayCallback = this.DoPlay;
            this.onStopCallback = this.DoStop;
            
            var events = WindowSystem.GetEvents();
            events.Register(handler, this.play, this.onPlayCallback);
            events.Register(handler, this.stop, this.onStopCallback);

        }

        public void DeInitialize(WindowObject handler) {
            
            if (this.play == WindowEvent.None && this.stop == WindowEvent.None) return;

            var events = WindowSystem.GetEvents();
            events.UnRegister(handler, this.play, this.onPlayCallback);
            events.UnRegister(handler, this.stop, this.onStopCallback);

            this.onPlayCallback = null;
            this.onStopCallback = null;

        }

        public void DoPlay() {
            
            #if FMOD_SUPPORT
            this.FMODPlay();
            #else
            var audio = WindowSystem.GetAudio();
            audio.Play(this.clip);
            #endif

        }

        public void DoStop() {
            
            #if FMOD_SUPPORT
            this.FMODStop();
            #else
            var audio = WindowSystem.GetAudio();
            audio.Play(this.clip);
            #endif

        }
        
        #if FMOD_SUPPORT
        [FMODUnity.EventRefAttribute]
        public string audioEvent;
        private FMOD.Studio.EventInstance instance;
        
        private void FMODPlay() {

            var eventDescription = FMODUnity.RuntimeManager.GetEventDescription(this.audioEvent);
            if (eventDescription.isValid() == false) return;
            eventDescription.createInstance(out this.instance);
            
            this.instance.start();

        }

        private void FMODStop() {

            this.instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        }
        #else
        public AudioClip clip;
        #endif

    }

}

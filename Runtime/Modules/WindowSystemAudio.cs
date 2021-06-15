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

        public WindowEvent onPlay;
        public WindowEvent onStop;
        
        #if FMOD_SUPPORT
        public FMOD.Studio.EventDescription eventDescription;
        
        private FMOD.Studio.EventInstance instance;
        
        private void Play() {

            if (this.eventDescription.isValid() == false) return;
            this.eventDescription.createInstance(out this.instance);
            this.instance.start();

        }

        private void Stop() {

            this.instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        }
        #endif

    }

}

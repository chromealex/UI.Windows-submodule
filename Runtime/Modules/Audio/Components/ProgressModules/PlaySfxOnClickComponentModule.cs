using UIWSAudioEvent = UnityEngine.UI.Windows.Runtime.Modules.Audio.UIWSAudioEvent;

namespace UnityEngine.UI.Windows {

    [ComponentModuleDisplayName("Play SFX on progress change")]
    public class PlaySfxOnSlideComponentModule : ProgressComponentModule, IAudioComponentModule {

        #if FMOD_SUPPORT
        public FMODAudioComponent data;
        #else
        public UIWSAudioEvent clip;
        #endif

        public override void OnValueChanged(float value) {

            #if FMOD_SUPPORT
            this.data.Play();
            #else
            this.clip.Play();
            #endif

        }

    }

}

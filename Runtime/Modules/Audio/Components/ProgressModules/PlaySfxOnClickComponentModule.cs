using UIWSAudioEvent = UnityEngine.UI.Windows.Runtime.Modules.Audio.UIWSAudioEvent;

namespace UnityEngine.UI.Windows {

    [ComponentModuleDisplayName("Play SFX on progress change")]
    public class PlaySfxOnSlideComponentModule : ProgressComponentModule, IAudioComponentModule {

        public UIWSAudioEvent clip;

        public override void OnValueChanged(float value) {

            this.clip.Play();

        }

    }

}

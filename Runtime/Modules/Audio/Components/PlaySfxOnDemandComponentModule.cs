using UnityEngine.UI.Windows.Runtime.Modules.Audio;

namespace UnityEngine.UI.Windows.Components {

    [ComponentModuleDisplayName("Play SFX on demand")]
    public class PlaySfxOnDemandComponentModule : WindowComponentModule, IAudioComponentModule {

        public UIWSAudioEvent clip;

        public void Play() {

            this.clip.Play();

        }

    }

}

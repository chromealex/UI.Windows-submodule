﻿using UIWSAudioEvent = UI.Windows.Runtime.Modules.Audio.UIWSAudioEvent;

namespace UnityEngine.UI.Windows {

    [ComponentModuleDisplayName("Play SFX on Click")]
    public class PlaySfxOnClickComponentModule : ButtonComponentModule, IAudioComponentModule {

        #if FMOD_SUPPORT
        public FMODAudioComponent data;
        #else
        public UIWSAudioEvent clip;
        #endif

        public override void OnInit() {
            
            this.buttonComponent.button.onClick.AddListener(this.OnClick);
            
            base.OnInit();
            
        }

        public override void OnDeInit() {
            
            this.buttonComponent.button.onClick.RemoveListener(this.OnClick);

            base.OnDeInit();
            
        }

        private void OnClick() {

            #if FMOD_SUPPORT
            this.data.Play();
            #else
            this.clip.Play();
            #endif
            
        }

    }

}

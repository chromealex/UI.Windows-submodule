namespace UnityEngine.UI.Windows {

    [ComponentModuleDisplayName("Play SFX on Click")]
    public class PlaySfxOnClickComponentModule : ButtonComponentModule {

        #if FMOD_SUPPORT
        public FMODAudioComponent data;
        #else
        public AudioClip clip;
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
            WindowSystem.GetAudio().Play(this.clip);
            #endif
            
        }

    }

}

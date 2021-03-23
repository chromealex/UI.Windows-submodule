using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    public class PlaySfxComponentModule : ButtonComponentModule {

        public AudioClip clip;

        public override void OnInit() {
            
            this.buttonComponent.button.onClick.AddListener(this.OnClick);
            
            base.OnInit();
            
        }

        public override void OnDeInit() {
            
            this.buttonComponent.button.onClick.RemoveListener(this.OnClick);

            base.OnDeInit();
            
        }

        private void OnClick() {

            WindowSystem.GetAudio().Play(this.clip);
            
        }

    }

}

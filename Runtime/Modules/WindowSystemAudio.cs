using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

}

﻿using UIWSAudioEvent = UI.Windows.Runtime.Modules.Audio.UIWSAudioEvent;

namespace UnityEngine.UI.Windows {
    
    using Components;

    public interface IAudioComponentModule { }

    public class WindowSystemAudio : MonoBehaviour {

        public enum EventType {
            SFX,
            Music,
        }

        public enum Behaviour {
            None,
            StopOtherChannels,
        }

        public const int CHANNELS_COUNT = 16;
        
        public UIWSAudioEvent defaultButtonClickEvent;
        public AudioSource audioSource;
        
        private AudioSource sfxSource;
        private AudioSource[] musicSources;
        private ME.Taptic.ITapticEngine taptic;
        
        public void Start() {

            this.taptic = new ME.Taptic.TapticEngine();
            this.taptic.SetActiveModule(new ME.Taptic.TapticAnimationCurveImpl());
            
            WindowSystem.AddCallbackOnAnyInteractable(this.OnAnyInteractable);

            this.sfxSource = Instantiate(this.audioSource);
            this.sfxSource.name = "[Audio] SFX";
            this.sfxSource.gameObject.SetActive(true);
            GameObject.DontDestroyOnLoad(this.sfxSource.gameObject);
            this.musicSources = new AudioSource[CHANNELS_COUNT - 1];
            for (int i = 1; i < CHANNELS_COUNT; ++i) {
                var source = Instantiate(this.audioSource);
                GameObject.DontDestroyOnLoad(source.gameObject);
                source.name = $"[Audio] Musix Channel {i}";
                source.gameObject.SetActive(true);
                this.musicSources[i - 1] = source;
            }

        }

        public void Update() {
            this.taptic.Update();
        }

        public void OnDestroy() {
            
            WindowSystem.RemoveCallbackOnAnyInteractable(this.OnAnyInteractable);
            
        }

        private void OnAnyInteractable(UnityEngine.UI.Windows.Components.IInteractable obj) {

            if (obj is ButtonComponent button) {
                if (button.GetModule<IAudioComponentModule>() == null) {
                    if (this.defaultButtonClickEvent != null) {
                        this.defaultButtonClickEvent.Play();
                    }
                }
            }
            
        }

        /// <summary>
        /// Set volume
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="volume">0..1</param>
        public void SetVolume(EventType eventType, float volume) {
            if (eventType == EventType.SFX) {
                this.sfxSource.volume = volume;
            } else if (eventType == EventType.Music) {
                for (int i = 1; i < CHANNELS_COUNT; ++i) {
                    var source = this.musicSources[i - 1];
                    source.volume = volume;
                }
            }
            this.audioSource.volume = volume;
        }

        /// <summary>
        /// Set vibration state
        /// </summary>
        /// <param name="state">True is on, False is off</param>
        public void SetVibration(bool state) {
            if (state == true) {
                this.taptic.Unmute();
            } else {
                this.taptic.Mute();
            }
        }
        
        public void Play(UIWSAudioEvent clip) {

            if (clip.eventType == EventType.SFX) {
                if (clip.randomClips?.Length > 0) {
                    this.ApplyParameters(this.sfxSource, clip);
                    this.sfxSource.PlayOneShot(clip.randomClips[Random.Range(0, clip.randomClips.Length)]);
                } else if (clip.audioClip != null) {
                    this.ApplyParameters(this.sfxSource, clip);
                    this.sfxSource.PlayOneShot(clip.audioClip);
                }
            } else if (clip.eventType == EventType.Music) {
                if (clip.behaviour == Behaviour.StopOtherChannels) {
                    this.StopAllChannels();
                }
                var source = this.GetChannel(clip.musicChannel);
                this.ApplyParameters(source, clip);
                source.clip = clip.audioClip;
                source.Play();
            }

            if (clip.vibrate == true) {
                if (clip.tapticByCurve == true) {
                    this.taptic.PlayCurve(clip.tapticCurve, false);
                } else {
                    this.taptic.PlaySingle(clip.taptic);
                }
            }

        }

        public void StopAllChannels() {
            for (int i = 0; i < this.musicSources.Length; ++i) {
                var source = this.musicSources[i];
                source.Stop();
            }
        }

        private void ApplyParameters(AudioSource source, UIWSAudioEvent clip) {

            var parameters = clip.parameters;
            if (parameters.changePitch == true) {
                if (parameters.randomPitch == true) {
                    source.pitch = Random.Range(parameters.randomPitchValue.x, parameters.randomPitchValue.y);
                } else {
                    source.pitch = parameters.pitchValue;
                }
            } else {
                source.pitch = this.audioSource.pitch;
            }

            if (parameters.changeVolume == true) {
                if (parameters.randomVolume == true) {
                    source.volume = Random.Range(parameters.randomVolumeValue.x, parameters.randomVolumeValue.y);
                } else {
                    source.volume = parameters.volumeValue;
                }
            } else {
                source.volume = this.audioSource.volume;
            }

        }

        public void Stop(UIWSAudioEvent clip) {

            if (clip.eventType == EventType.SFX) {
                this.sfxSource.Stop();
            } else if (clip.eventType == EventType.Music) {
                var source = this.GetChannel(clip.musicChannel);
                source.Stop();
            }
            
        }

        private AudioSource GetChannel(int musicChannel) {
            return this.musicSources[musicChannel - 1];
        }

    }

    [System.Serializable]
    public struct ComponentAudio {

        public WindowEvent play;
        public WindowEvent stop;

        #if FMOD_SUPPORT
        public FMODAudioComponent fmodAudioComponent;
        #else
        public UIWSAudioEvent clip;
        #endif

        private System.Action<WindowObject> onPlayCallback;
        private System.Action<WindowObject> onStopCallback;
        
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

        public void DoPlay(WindowObject obj) {
            
            #if FMOD_SUPPORT
            this.fmodAudioComponent.Play();
            #else
            this.clip.Play();
            #endif

        }

        public void DoStop(WindowObject obj) {
            
            #if FMOD_SUPPORT
            this.fmodAudioComponent.Stop();
            #else
            this.clip.Stop();
            #endif

        }

        public void DoRelease() {
            
            #if FMOD_SUPPORT
            this.fmodAudioComponent.Release();
            #endif

        }

    }

    #if FMOD_SUPPORT && FMOD_VERSION_2
    [System.Serializable]
    public struct FMODAudioComponent {
        
        [System.Serializable]
        public struct FMODParameter {

            public string name;
            public float value;

        }
        
        public FMODUnity.EventReference audioEvent;
        public bool stopOthersOnPlay;
        public FMODParameter[] parameters;
        public bool autoStopOnceParameters;
        public float autoStopDuration;
        public bool autoRelease;
        public float autoReleaseDuration;

        private static FMOD.Studio.EventInstance GetInstance(FMODUnity.EventReference audioEvent, FMOD.Studio.EventDescription eventDescription, bool stopOthersOnPlay = false) {
            
            if (eventDescription.getInstanceCount(out var count) == FMOD.RESULT.OK) {

                if (count > 0) {
                
                    if (eventDescription.getInstanceList(out var list) == FMOD.RESULT.OK) {

                        foreach (var item in list) {

                            if (item.isValid() == true) {
                            
                                if (stopOthersOnPlay == true) {

                                    item.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

                                }

                                if (item.getDescription(out var descr) == FMOD.RESULT.OK) {

                                    if (descr.getID(out var guid) == FMOD.RESULT.OK) {

                                        if (guid == audioEvent.Guid) {

                                            return item;

                                        }

                                    }

                                }

                            }
                        
                        }

                    }
                
                }
            
            }

            return default;

        }

        public void SetParameterOnce(string name, float value) {
            
            if (this.audioEvent.IsNull == true) return;

            var eventDescription = FMODUnity.RuntimeManager.GetEventDescription(this.audioEvent);
            if (eventDescription.isValid() == false) return;

            //Debug.Log("SetParameterOnce: " + name + ", value: " + value + " (" + this.audioEvent + ")");

            {
                var instance = FMODAudioComponent.GetInstance(this.audioEvent, eventDescription, this.stopOthersOnPlay);
                instance.setParameterByName(name, value);
            }

            if (this.autoStopOnceParameters == true) {

                var audioEvent = this.audioEvent;
                var stopOthersOnPlay = this.stopOthersOnPlay;
                UnityEngine.UI.Windows.Utilities.Coroutines.WaitTime(this.autoStopDuration, () => {
                    
                    var instance = FMODAudioComponent.GetInstance(audioEvent, eventDescription, stopOthersOnPlay);
                    //Debug.Log("SetParameterOnce: " + name + ", value: " + 0f + " (" + audioEvent + ")");
                    instance.setParameterByName(name, 0f);
    
                });

            }

        }
        
        public void SetParameter(string name, float value) {
            
            if (this.audioEvent.IsNull == true) return;

            var eventDescription = FMODUnity.RuntimeManager.GetEventDescription(this.audioEvent);
            if (eventDescription.isValid() == false) return;

            //Debug.Log("SetParameter: " + name + ", value: " + value + " (" + this.audioEvent + ")");

            var instance = FMODAudioComponent.GetInstance(this.audioEvent, eventDescription, this.stopOthersOnPlay);
            instance.setParameterByName(name, value);
            
        }

        public void Play() {
            
            this.Play_INTERNAL(default, false);
            
        }

        public void Play(Vector3 position) {
            
            this.Play_INTERNAL(position, true);
            
        }

        private void Play_INTERNAL(Vector3 position, bool usePosition) {

            if (this.audioEvent.IsNull == true) return;

            var eventDescription = FMODUnity.RuntimeManager.GetEventDescription(this.audioEvent);
            if (eventDescription.isValid() == false) return;

            var setPlay = false;
            var instance = FMODAudioComponent.GetInstance(this.audioEvent, eventDescription, this.stopOthersOnPlay);
            if (instance.isValid() == false) {
                
                setPlay = true;
                eventDescription.createInstance(out instance);
                
            }
            
            foreach (var p in this.parameters) {
                instance.setParameterByName(p.name, p.value);
            }

            if (setPlay == true) {

                if (usePosition == true) {

                    instance.set3DAttributes(new FMOD.ATTRIBUTES_3D() {
                        position = new FMOD.VECTOR() {
                            x = position.x,
                            y = position.y,
                            z = position.z,
                        },
                    });

                }

                instance.start();
                if (eventDescription.isOneshot(out var isOneShot) == FMOD.RESULT.OK && isOneShot == true) {
                    instance.release();
                } else {
                    if (this.autoRelease == true) {
                        UnityEngine.UI.Windows.Utilities.Coroutines.WaitTime(this.autoReleaseDuration, () => {
                            
                            instance.release();

                        });
                    }
                }

            }

        }

        public void Stop() {

            if (this.audioEvent.IsNull == true) return;

            var eventDescription = FMODUnity.RuntimeManager.GetEventDescription(this.audioEvent);
            if (eventDescription.isValid() == false) return;

            var instance = FMODAudioComponent.GetInstance(this.audioEvent, eventDescription, this.stopOthersOnPlay);
            foreach (var p in this.parameters) {
                instance.setParameterByName(p.name, 0f);
            }
            
        }

        public void Release() {
            
            if (this.audioEvent.IsNull == true) return;

            var eventDescription = FMODUnity.RuntimeManager.GetEventDescription(this.audioEvent);
            if (eventDescription.isValid() == false) return;

            var instance = FMODAudioComponent.GetInstance(this.audioEvent, eventDescription, this.stopOthersOnPlay);
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();

        }
        
    }
    #elif FMOD_SUPPORT
    [System.Serializable]
    public struct FMODAudioComponent {
        
        [System.Serializable]
        public struct FMODParameter {

            public string name;
            public float value;

        }
        [FMODUnity.EventRefAttribute]
        public string audioEvent;
        public bool stopOthersOnPlay;
        public FMODParameter[] parameters;
        public bool autoStopOnceParameters;
        public float autoStopDuration;
        public bool autoRelease;
        public float autoReleaseDuration;

        private static FMOD.Studio.EventInstance GetInstance(string audioEvent, FMOD.Studio.EventDescription eventDescription, bool stopOthersOnPlay = false) {
            
            if (eventDescription.getInstanceCount(out var count) == FMOD.RESULT.OK) {

                if (count > 0) {
                
                    if (eventDescription.getInstanceList(out var list) == FMOD.RESULT.OK) {

                        foreach (var item in list) {

                            if (item.isValid() == true) {
                            
                                if (stopOthersOnPlay == true) {

                                    item.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                                    return default;

                                }

                                if (item.getDescription(out var descr) == FMOD.RESULT.OK) {

                                    if (descr.getPath(out var path) == FMOD.RESULT.OK) {

                                        if (path == audioEvent) {

                                            return item;

                                        }

                                    }

                                }

                            }
                        
                        }

                    }
                
                }
            
            }

            return default;

        }

        public void SetParameterOnce(string name, float value) {
            
            if (string.IsNullOrEmpty(this.audioEvent) == true) return;

            FMOD.Studio.EventDescription eventDescription;
            try {
                eventDescription = FMODUnity.RuntimeManager.GetEventDescription(this.audioEvent);
                if (eventDescription.isValid() == false) return;
            } catch (System.Exception) {
                return;
            }

            //Debug.Log("SetParameterOnce: " + name + ", value: " + value + " (" + this.audioEvent + ")");

            {
                var instance = FMODAudioComponent.GetInstance(this.audioEvent, eventDescription, this.stopOthersOnPlay);
                instance.setParameterByName(name, value);
            }

            if (this.autoStopOnceParameters == true) {

                var audioEvent = this.audioEvent;
                var stopOthersOnPlay = this.stopOthersOnPlay;
                UnityEngine.UI.Windows.Utilities.Coroutines.WaitTime(this.autoStopDuration, () => {
                    
                    var instance = FMODAudioComponent.GetInstance(audioEvent, eventDescription, stopOthersOnPlay);
                    //Debug.Log("SetParameterOnce: " + name + ", value: " + 0f + " (" + audioEvent + ")");
                    instance.setParameterByName(name, 0f);
    
                });

            }

        }
        
        public void SetParameter(string name, float value) {
            
            if (string.IsNullOrEmpty(this.audioEvent) == true) return;

            FMOD.Studio.EventDescription eventDescription;
            try {
                eventDescription = FMODUnity.RuntimeManager.GetEventDescription(this.audioEvent);
                if (eventDescription.isValid() == false) return;
            } catch (System.Exception) {
                return;
            }
            //Debug.Log("SetParameter: " + name + ", value: " + value + " (" + this.audioEvent + ")");

            var instance = FMODAudioComponent.GetInstance(this.audioEvent, eventDescription, this.stopOthersOnPlay);
            instance.setParameterByName(name, value);
            
        }

        public readonly void Play() {
            
            this.Play_INTERNAL(default, false);
            
        }

        public readonly void Play(Vector3 position) {
            
            this.Play_INTERNAL(position, true);
            
        }

        private readonly void Play_INTERNAL(Vector3 position, bool usePosition) {

            if (string.IsNullOrEmpty(this.audioEvent) == true) return;

            FMOD.Studio.EventDescription eventDescription;
            try {
                eventDescription = FMODUnity.RuntimeManager.GetEventDescription(this.audioEvent);
                if (eventDescription.isValid() == false) return;
            } catch (System.Exception) {
                return;
            }

            var setPlay = false;
            var instance = FMODAudioComponent.GetInstance(this.audioEvent, eventDescription, this.stopOthersOnPlay);
            if (instance.isValid() == false) {
                
                setPlay = true;
                eventDescription.createInstance(out instance);
                
            }
            
            foreach (var p in this.parameters) {
                instance.setParameterByName(p.name, p.value);
            }

            if (setPlay == true) {

                if (usePosition == true) {

                    instance.set3DAttributes(new FMOD.ATTRIBUTES_3D() {
                        position = new FMOD.VECTOR() {
                            x = position.x,
                            y = position.y,
                            z = position.z,
                        },
                    });

                }

                instance.start();
                if (eventDescription.isOneshot(out var isOneShot) == FMOD.RESULT.OK && isOneShot == true) {
                    instance.release();
                } else {
                    if (this.autoRelease == true) {
                        UnityEngine.UI.Windows.Utilities.Coroutines.WaitTime(instance, this.autoReleaseDuration, static (inst) => {
                            
                            inst.release();

                        });
                    }
                }

            }

        }

        public void Stop() {

            if (string.IsNullOrEmpty(this.audioEvent) == true) return;

            FMOD.Studio.EventDescription eventDescription;
            try {
                eventDescription = FMODUnity.RuntimeManager.GetEventDescription(this.audioEvent);
                if (eventDescription.isValid() == false) return;
            } catch (System.Exception) {
                return;
            }

            var instance = FMODAudioComponent.GetInstance(this.audioEvent, eventDescription, this.stopOthersOnPlay);
            foreach (var p in this.parameters) {
                instance.setParameterByName(p.name, 0f);
            }
            
        }

        public void Release() {
            
            if (string.IsNullOrEmpty(this.audioEvent) == true) return;

            FMOD.Studio.EventDescription eventDescription;
            try {
                eventDescription = FMODUnity.RuntimeManager.GetEventDescription(this.audioEvent);
                if (eventDescription.isValid() == false) return;
            } catch (System.Exception) {
                return;
            }

            var instance = FMODAudioComponent.GetInstance(this.audioEvent, eventDescription, this.stopOthersOnPlay);
            instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            instance.release();

        }
        
    }
    #endif
    
}

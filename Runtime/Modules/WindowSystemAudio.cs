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

        #if FMOD_SUPPORT
        public FMODAudioComponent fmodAudioComponent;
        #else
        public AudioClip clip;
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
            var audio = WindowSystem.GetAudio();
            audio.Play(this.clip);
            #endif

        }

        public void DoStop(WindowObject obj) {
            
            #if FMOD_SUPPORT
            this.fmodAudioComponent.Stop();
            #else
            var audio = WindowSystem.GetAudio();
            audio.Play(this.clip);
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

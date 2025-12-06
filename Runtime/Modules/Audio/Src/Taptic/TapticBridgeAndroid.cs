#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine;

namespace ME.Taptic {

    public class TapticBridgeAndroid : ITapticBridge {

        private const int MIN_SDK_VERSION = 26;

        private static readonly AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        private static readonly AndroidJavaObject currentActivity = TapticBridgeAndroid.unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        private readonly AndroidJavaObject androidVibrator = TapticBridgeAndroid.currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
        private AndroidJavaClass vibrationEffectClass;
        private AndroidJavaObject vibrationEffect;
        private int defaultAmplitude;

        private int sdkVersion = -1;

        void ITapticBridge.Play(TapticType type, float duration, float strength) {

            if (duration <= 0f || strength <= 0f) {
                
                this.Haptic(type);
                
            } else {

                this.AndroidVibrate((long)(duration * 1000f), (byte)Mathf.Clamp(1, 255, strength));

            }

        }

        bool ITapticBridge.IsSupported() {

            if (this.androidVibrator == null || this.androidVibrator.Call<bool>("hasVibrator") == false) {

                return false;

            }

            return true;

        }

        public static long LightDuration = 20;
        public static long MediumDuration = 40;
        public static long HeavyDuration = 80;
        public static int LightAmplitude = 40;
        public static int MediumAmplitude = 120;
        public static int HeavyAmplitude = 255;
        private static long[] _successPattern = { 0, TapticBridgeAndroid.LightDuration, TapticBridgeAndroid.LightDuration, TapticBridgeAndroid.HeavyDuration };
        private static int[] _successPatternAmplitude = { 0, TapticBridgeAndroid.LightAmplitude, 0, TapticBridgeAndroid.HeavyAmplitude };
        private static long[] _warningPattern = { 0, TapticBridgeAndroid.HeavyDuration, TapticBridgeAndroid.LightDuration, TapticBridgeAndroid.MediumDuration };
        private static int[] _warningPatternAmplitude = { 0, TapticBridgeAndroid.HeavyAmplitude, 0, TapticBridgeAndroid.MediumAmplitude };
        private static long[] _failurePattern = {
            0, TapticBridgeAndroid.MediumDuration, TapticBridgeAndroid.LightDuration, TapticBridgeAndroid.MediumDuration, TapticBridgeAndroid.LightDuration,
            TapticBridgeAndroid.HeavyDuration, TapticBridgeAndroid.LightDuration, TapticBridgeAndroid.LightDuration
        };
        private static int[] _failurePatternAmplitude = {
            0, TapticBridgeAndroid.MediumAmplitude, 0, TapticBridgeAndroid.MediumAmplitude, 0, TapticBridgeAndroid.HeavyAmplitude, 0, TapticBridgeAndroid.LightAmplitude
        };

        public void Haptic(TapticType type) {
            try {
                switch (type) {
                    case TapticType.Selection:
                        this.AndroidVibrate(TapticBridgeAndroid.LightDuration, TapticBridgeAndroid.LightAmplitude);
                        break;

                    case TapticType.Success:
                        this.AndroidVibrate(TapticBridgeAndroid._successPattern, TapticBridgeAndroid._successPatternAmplitude, -1);
                        break;

                    case TapticType.Warning:
                        this.AndroidVibrate(TapticBridgeAndroid._warningPattern, TapticBridgeAndroid._warningPatternAmplitude, -1);
                        break;

                    case TapticType.Failure:
                        this.AndroidVibrate(TapticBridgeAndroid._failurePattern, TapticBridgeAndroid._failurePatternAmplitude, -1);
                        break;

                    case TapticType.Light:
                        this.AndroidVibrate(TapticBridgeAndroid.LightDuration, TapticBridgeAndroid.LightAmplitude);
                        break;

                    case TapticType.Medium:
                        this.AndroidVibrate(TapticBridgeAndroid.MediumDuration, TapticBridgeAndroid.MediumAmplitude);
                        break;

                    case TapticType.Heavy:
                        this.AndroidVibrate(TapticBridgeAndroid.HeavyDuration, TapticBridgeAndroid.HeavyAmplitude);
                        break;
                    
                    case TapticType.Vibrate:
                        Handheld.Vibrate();
                        break;
                }
            } catch (System.NullReferenceException e) {
                Debug.Log(e.StackTrace);
            }
        }
        
        public void AndroidVibrate(long milliseconds) {
            
            if (this.androidVibrator != null) {
                
                this.androidVibrator.Call("vibrate", milliseconds);
                
            }
            
        }

        public void AndroidVibrate(long[] pattern, int[] amplitudes, int repeat) {
            
            if (this.AndroidSDKVersion() < TapticBridgeAndroid.MIN_SDK_VERSION) {
                
                if (this.androidVibrator != null) {
                    
                    this.androidVibrator.Call("vibrate", pattern, repeat);
                    
                }
                
            } else {
                
                this.VibrationEffectClassInitialization();
                if (this.vibrationEffectClass != null) {

                    this.CreateVibrationEffect("createWaveform", new object[] { pattern, amplitudes, repeat });
                    
                }
                
            }
            
        }
        
        public void AndroidVibrate(long milliseconds, int amplitude) {
            
            if (this.AndroidSDKVersion() < TapticBridgeAndroid.MIN_SDK_VERSION) {
                
                this.AndroidVibrate(milliseconds);
                
            } else {
                
                this.VibrationEffectClassInitialization();
                if (this.vibrationEffectClass != null) {

                    if (this.CreateVibrationEffect("createOneShot", new object[] { milliseconds, amplitude }) == false) {
                        
                        this.AndroidVibrate(milliseconds);

                    }

                } else {
                    
                    this.AndroidVibrate(milliseconds);
                    
                }
                
            }
            
        }

        private bool CreateVibrationEffect(string function, params object[] args) {
            
            if (this.androidVibrator !=null) {
                
                var vibrationEffect = this.vibrationEffectClass.CallStatic<AndroidJavaObject>(function, args);
                if (vibrationEffect != null) {
                    
                    this.androidVibrator.Call("vibrate", vibrationEffect);
                    return true;

                }
                
            }

            return false;

        }
        
        private void VibrationEffectClassInitialization() {
            
            if (this.vibrationEffectClass == null) {
                
                this.vibrationEffectClass = new AndroidJavaClass("android.os.VibrationEffect");
                
            }
            
        }

        public int AndroidSDKVersion() {
            
            if (this.sdkVersion == -1) {
                
                var apiLevel = int.Parse(SystemInfo.operatingSystem.Substring(SystemInfo.operatingSystem.IndexOf("-") + 1, 3));
                this.sdkVersion = apiLevel;
                return apiLevel;
                
            } else {
                
                return this.sdkVersion;
                
            }
            
        }

    }

}
#endif
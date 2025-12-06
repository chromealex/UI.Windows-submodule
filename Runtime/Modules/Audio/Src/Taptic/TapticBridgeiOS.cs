#if UNITY_IOS && !UNITY_EDITOR
using UnityEngine;
using System.Runtime.InteropServices;

namespace ME.Taptic {

    public class TapticBridgeiOS : ITapticBridge {

        [DllImport("__Internal")] private static extern void ME_PlayTaptic(int type);
        [DllImport("__Internal")] private static extern void ME_PlayTaptic6s(int type);

        void ITapticBridge.Play(TapticType type, float duration, float strength) {

            if (type == TapticType.Vibrate) {
                
                Handheld.Vibrate();
                return;
                
            }

            if (TapticBridgeiOS.iPhone6s() == true) {
                
                TapticBridgeiOS.ME_PlayTaptic6s((int)type);
                
            } else {
                
                TapticBridgeiOS.ME_PlayTaptic((int)type);
                
            }
            
        }

        private static bool iPhone6s() {
            
            return SystemInfo.deviceModel == "iPhone8,1" || SystemInfo.deviceModel == "iPhone8,2";
            
        }

        bool ITapticBridge.IsSupported() {

            if ((UnityEngine.iOS.Device.generation.ToString().ToLower()).Contains("ipad") == true) {

                return false;

            }

            return true;

        }

    }

}
#endif
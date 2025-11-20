namespace ME.Taptic {

    public class TapticBridgeDummy : ITapticBridge {

        public bool logs;

        void ITapticBridge.Play(TapticType type, float duration, float strength) {

            #if UNITY_EDITOR
            if (this.logs == true) UnityEngine.Debug.Log($"Play Vibration {type} with duration {duration}, amplitude: {strength}");
            #endif

        }

        bool ITapticBridge.IsSupported() {

            #if UNITY_EDITOR
            return true;
            #else
            return false;
            #endif

        }

    }

}
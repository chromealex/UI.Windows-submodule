namespace UnityEngine.UI.Windows {

    public class WindowSystemInput {

        public static readonly Unity.Burst.SharedStatic<bool> hasPointerClickThisFrame = Unity.Burst.SharedStatic<bool>.GetOrCreate<WindowSystemInput, WindowSystemInput>();
        
    }

}
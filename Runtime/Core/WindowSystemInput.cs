namespace UnityEngine.UI.Windows {

    public class WindowSystemInput {

        private class HasPointerClickThisFrameFieldKey {}
        
        public static readonly Unity.Burst.SharedStatic<bool> hasPointerClickThisFrame = 
            Unity.Burst.SharedStatic<bool>.GetOrCreate<WindowSystemInput, HasPointerClickThisFrameFieldKey>();
    }

}
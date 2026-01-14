namespace UnityEngine.UI.Windows {

    public enum ControllerButton {
        Share,    // PS: Share       XBOX: Start
        Options,  // PS: Options     XBOX: Back
        Click,    // PS: X           XBOX: A
        Back,     // PS: O           XBOX: B
        Square,   // PS: Square      XBOX: X
        Triangle, // PS: Triangle    XBOX: Y
        L,        // PS: L3          XBOX: L
        R,        // PS: R3          XBOX: R
        L1,       // PS: L1          XBOX: LB
        L2,       // PS: L2          XBOX: LT
        R1,       // PS: R1          XBOX: RB
        R2,       // PS: R2          XBOX: RT
        Up,
        Down,
        Left,
        Right,
    }

    public interface IInteractableNavigation {

        IInteractableNavigation GetNext(Vector2 direction);
        void DoAction(ControllerButton button);

    }

}
namespace UnityEngine.UI.Windows {

    public abstract class WindowSystemModule : ScriptableObject {

        public abstract void OnStart();
        public virtual void OnUpdate() {}
        public abstract void OnDestroy();

    }

    public interface ICanClickCheckModule {

        bool CanClick(UnityEngine.UI.Windows.Components.IInteractable obj);

    }

}
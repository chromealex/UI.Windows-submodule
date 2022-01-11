using System.Collections.Generic;
using UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules;

namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    public enum TutorialWindowEvent {

        OnInitialize,
        OnShowBegin,
        OnShowEnd,
        OnFocusTook,

    }

    [System.Serializable]
    public struct Tag {

        public int id;
        public bool isList;
        public int listIndex;

    }

    public struct Context {

        public TutorialWindowEvent windowEvent;
        public TutorialSystem system;
        public TutorialData data;
        public int index;
        public UnityEngine.UI.Windows.WindowTypes.LayoutWindowType window;

    }

    [CreateAssetMenu(menuName = "UI.Windows/Tutorial/System Module")]
    public class TutorialSystem : WindowSystemModule {

        public List<TutorialData> currentTutorialItems;

        public override void OnStart() {

            var events = WindowSystem.GetEvents();
            events.Register(WindowEvent.OnInitialize, this.OnWindowInitialized);
            events.Register(WindowEvent.OnShowBegin, this.OnWindowShowBegin);
            events.Register(WindowEvent.OnShowEnd, this.OnWindowShowEnd);
            events.Register(WindowEvent.OnFocusTook, this.OnWindowFocusTook);
            
        }

        public override void OnDestroy() {

            var events = WindowSystem.GetEvents();
            events.UnRegister(WindowEvent.OnInitialize, this.OnWindowInitialized);
            events.UnRegister(WindowEvent.OnShowBegin, this.OnWindowShowBegin);
            events.UnRegister(WindowEvent.OnShowEnd, this.OnWindowShowEnd);
            events.UnRegister(WindowEvent.OnFocusTook, this.OnWindowFocusTook);

        }

        private void OnWindowInitialized(WindowBase window) {

            this.OnWindowEvent(window, TutorialWindowEvent.OnInitialize);

        }

        private void OnWindowShowBegin(WindowBase window) {

            this.OnWindowEvent(window, TutorialWindowEvent.OnShowBegin);

        }

        private void OnWindowShowEnd(WindowBase window) {

            this.OnWindowEvent(window, TutorialWindowEvent.OnShowEnd);

        }

        private void OnWindowFocusTook(WindowBase window) {

            this.OnWindowEvent(window, TutorialWindowEvent.OnFocusTook);

        }

        private void OnWindowEvent(WindowBase window, TutorialWindowEvent windowEvent) {

            var tutorialModule = window.modules.Get<TutorialModule>();
            if (tutorialModule != null) {

                var tutorialData = tutorialModule.data.Load(tutorialModule);
                this.TryToStart(window, tutorialData, windowEvent);

            } else {
                
                foreach (var item in this.currentTutorialItems) {

                    this.TryToStart(window, item, windowEvent);

                }

            }

        }

        public bool TryToStart(WindowBase window, TutorialData tutorialData, TutorialWindowEvent windowEvent) {

            if (tutorialData == null) return false;

            if (tutorialData.startEvent == windowEvent) {

                var context = new Context() {
                    system = this,
                    window = (UnityEngine.UI.Windows.WindowTypes.LayoutWindowType)window,
                    windowEvent = windowEvent,
                    data = tutorialData,
                };
                if (tutorialData.IsValid(window, in context) == true) {

                    tutorialData.OnStart(context);
                    return true;

                }

            }

            return false;

        }

    }

}
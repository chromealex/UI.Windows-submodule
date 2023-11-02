using System.Collections.Generic;
using UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules;

namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    public enum TutorialWindowEvent {

        OnInitialize,
        OnShowBegin,
        OnShowEnd,
        OnFocusTook,
        OnHideBegin,

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
        public WindowObject window;

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
            events.Register(WindowEvent.OnHideBegin, this.OnWindowHideBegin);
            
        }

        public override void OnDestroy() {

            var events = WindowSystem.GetEvents();
            
            if (events == null) return;
            
            events.UnRegister(WindowEvent.OnInitialize, this.OnWindowInitialized);
            events.UnRegister(WindowEvent.OnShowBegin, this.OnWindowShowBegin);
            events.UnRegister(WindowEvent.OnShowEnd, this.OnWindowShowEnd);
            events.UnRegister(WindowEvent.OnFocusTook, this.OnWindowFocusTook);
            events.UnRegister(WindowEvent.OnHideBegin, this.OnWindowHideBegin);

        }

        private void OnWindowInitialized(WindowObject window) {

            this.OnWindowEvent(window, TutorialWindowEvent.OnInitialize);

        }

        private void OnWindowShowBegin(WindowObject window) {

            this.OnWindowEvent(window, TutorialWindowEvent.OnShowBegin);

        }

        private void OnWindowShowEnd(WindowObject window) {

            this.OnWindowEvent(window, TutorialWindowEvent.OnShowEnd);

        }

        private void OnWindowFocusTook(WindowObject window) {

            this.OnWindowEvent(window, TutorialWindowEvent.OnFocusTook);

        }

        private void OnWindowHideBegin(WindowObject window) {
            
            this.OnWindowEvent(window, TutorialWindowEvent.OnHideBegin);
            
        }

        private void OnWindowEvent(WindowObject window, TutorialWindowEvent windowEvent) {

            TutorialModule tutorialModule = null;
            if (window is WindowBase windowBase) {
                tutorialModule = windowBase.modules.Get<TutorialModule>();
            }

            if (tutorialModule != null) {

                var tutorialData = tutorialModule.data.Load(tutorialModule);
                this.TryToStart(window, tutorialData, windowEvent);

            } else {
                
                foreach (var item in this.currentTutorialItems) {

                    this.TryToStart(window, item, windowEvent);

                }

            }

        }

        public bool TryToStart(WindowObject window, TutorialData tutorialData, TutorialWindowEvent windowEvent) {

            if (tutorialData == null) return false;

            if (tutorialData.startEvent == windowEvent) {

                var context = new Context() {
                    system = this,
                    window = window,
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
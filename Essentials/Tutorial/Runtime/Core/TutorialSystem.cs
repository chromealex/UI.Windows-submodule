using System.Collections.Generic;
using UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules;

namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    public enum TutorialWindowEvent {

        OnInitialize,
        OnShowBegin,
        OnShowEnd,

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
            
        }

        public override void OnDestroy() {

            var events = WindowSystem.GetEvents();
            events.UnRegister(WindowEvent.OnInitialize, this.OnWindowInitialized);
            events.UnRegister(WindowEvent.OnShowBegin, this.OnWindowShowBegin);
            events.UnRegister(WindowEvent.OnShowEnd, this.OnWindowShowEnd);

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

        private void OnWindowEvent(WindowBase window, TutorialWindowEvent windowEvent) {

            var tutorialModule = window.modules.Get<TutorialModule>();
            if (tutorialModule != null) {

                var tutorialData = tutorialModule.data.Load(tutorialModule);
                this.TryToStart(window, tutorialData, windowEvent);

            } else {
                
                foreach (var item in this.currentTutorialItems) {

                    if (this.TryToStart(window, item, windowEvent) == true) return;
                    
                }

            }

        }

        public bool TryToStart(WindowBase window, TutorialData tutorialData, TutorialWindowEvent windowEvent) {

            if (tutorialData.startEvent == windowEvent) {

                var context = new Context() {
                    system = this,
                    window = (UnityEngine.UI.Windows.WindowTypes.LayoutWindowType)window,
                    windowEvent = windowEvent,
                    data = tutorialData,
                };
                if (tutorialData.IsValid(window, in context) == true) {

                    tutorialData.OnStart(in context);
                    return true;

                }

            }

            return false;

        }

    }

}
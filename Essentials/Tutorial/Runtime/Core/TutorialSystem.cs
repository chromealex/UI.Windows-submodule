using System.Collections.Generic;
using UnityEngine.UI.Windows.Essentials.Tutorial.ComponentModules;

namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [System.Serializable]
    public struct Tag {

        public int id;
        public bool isList;
        public int listIndex;

    }

    public struct Context {

        public TutorialSystem system;
        public TutorialData data;
        public UnityEngine.UI.Windows.WindowTypes.LayoutWindowType window;

    }

    [CreateAssetMenu(menuName = "UI.Windows/Tutorial/System Module")]
    public class TutorialSystem : WindowSystemModule {

        public List<TutorialData> currentTutorialItems;

        public override void OnStart() {

            var events = WindowSystem.GetEvents();
            events.Register(WindowEvent.OnShowBegin, this.OnWindowShowBegin);
            
        }

        public override void OnDestroy() {

            var events = WindowSystem.GetEvents();
            events.UnRegister(WindowEvent.OnShowBegin, this.OnWindowShowBegin);

        }

        private void OnWindowShowBegin(WindowBase window) {

            var tutorialModule = window.modules.Get<TutorialModule>();
            if (tutorialModule != null) {

                var tutorialData = tutorialModule.data.Load(tutorialModule);
                this.TryToStart(window, tutorialData);

            } else {
                
                foreach (var item in this.currentTutorialItems) {

                    if (this.TryToStart(window, item) == true) return;
                    
                }

            }

        }

        private bool TryToStart(WindowBase window, TutorialData tutorialData) {

            var context = new Context() {
                system = this,
                window = (UnityEngine.UI.Windows.WindowTypes.LayoutWindowType)window,
                data = tutorialData,
            };
            if (tutorialData.IsValid(window, in context) == true) {

                tutorialData.OnStart(in context);
                return true;

            }

            return false;

        }

        public void Complete(Context context) {

            this.TryToStart(context.window, context.data.next);

        }

    }

}
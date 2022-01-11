using System.Linq;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.WindowTypes;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    public class FastLinkButtonComponent : ButtonComponent {

        public ImageComponent directoryIcon;
        public ImageComponent directoryUpIcon;
        public WindowComponent directoryContainer;
        
        public void SetInfo(WindowSystemConsole.FastLink data) {

            if (data.style == FastLinkType.Directory) {
                
                if (data.parentId > data.id) {
                 
                    this.directoryUpIcon.Show();
                    this.directoryIcon.Hide();
   
                } else {

                    this.directoryIcon.Show();
                    this.directoryUpIcon.Hide();
   
                }
                
                this.directoryContainer.Show();

            } else {
                
                this.directoryUpIcon.Hide();
                this.directoryIcon.Hide();
                this.directoryContainer.Hide();
                
            }
            
            this.Get<TextComponent>().SetText(data.caption);
            this.Get<ImageComponent>().SetColor(ConsoleScreen.GetColorByFastLinkStyle(data.style));

        }

    }

}
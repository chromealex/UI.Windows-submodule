using System.Linq;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.WindowTypes;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    public class LogScrollbarRectComponent : ImageComponent {

        public void SetInfo(ConsoleScreen.ScrollbarItem data, float fullHeight) {

            var color = ConsoleScreen.GetScrollbarColorByLogType(data.logType);
            this.SetColor(color);

            var topY = data.item.accumulatedSize - data.item.size;
            var size = data.item.size;
            
            this.rectTransform.anchorMax = new Vector2(1f, 1f - topY / fullHeight);
            this.rectTransform.anchorMin = new Vector2(0f, 1f - (topY + size) / fullHeight);
            this.rectTransform.sizeDelta = Vector2.zero;

        }

    }

}
using System.Linq;
using UnityEngine.UI.Windows.Components;
using UnityEngine.UI.Windows.WindowTypes;
using System.Collections.Generic;

namespace UnityEngine.UI.Windows.Runtime.Windows {

    public class LogsCounterComponent : WindowComponent {

        public TextComponent logsInfo;
        public TextComponent logsWarning;
        public TextComponent logsError;

        public void SetInfo() {

            var consoleScreen = this.GetWindow<ConsoleScreen>();
            
            this.logsInfo.SetValue(consoleScreen.GetCounter(LogType.Log));
            this.logsWarning.SetValue(consoleScreen.GetCounter(LogType.Warning));
            this.logsError.SetValue(consoleScreen.GetCounter(LogType.Error) + consoleScreen.GetCounter(LogType.Exception));

        }

    }

}
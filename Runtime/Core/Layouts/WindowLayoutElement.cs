using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows {

    public class WindowLayoutElement : WindowComponent, ILayoutInstance {

        public int tagId;
        public WindowLayout innerLayout;
        public bool hideInScreen;

        WindowLayout ILayoutInstance.windowLayoutInstance {
            get;
            set;
        }

    }

}
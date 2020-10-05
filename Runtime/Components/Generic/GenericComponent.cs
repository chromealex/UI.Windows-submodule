using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

    public class GenericComponent : WindowComponent {

        public WindowComponent[] components;

        public T Get<T>() where T : WindowComponent {

            for (int i = 0; i < this.components.Length; ++i) {

                var comp = this.components[i];
                if (comp is T c) {

                    return c;

                }

            }

            return default;

        }

    }

}
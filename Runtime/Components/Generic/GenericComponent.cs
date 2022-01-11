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

        public T GetStrong<T>() where T : WindowComponent {

            var type = typeof(T);

            for (int i = 0; i < this.components.Length; ++i) {

                var comp = this.components[i];
                if (comp != null && comp.GetType() == type) {

                    return comp as T;

                }

            }

            return default;

        }

    }

}
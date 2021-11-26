namespace UnityEngine.UI.Windows {

    [ComponentModuleDisplayName("Animate Value")]
    public class TextComponentModuleAnimateValue : TextComponentModule {

        public float animationTime = 1f;
        public bool roundToInt = true;
        public string formatValue;

        private bool setValueInternal;

        private struct Closure {

            public TextComponentModuleAnimateValue module;
            public UnityEngine.UI.Windows.Components.SourceValue sourceValue;
            public string strFormat;

        }
        
        public override void OnSetValue(double prevValue, double value, UnityEngine.UI.Windows.Components.SourceValue sourceValue, string strFormat) {

            if (this.setValueInternal == true) return;

            var closureData = new Closure() {
                module = this,
                sourceValue = sourceValue,
                strFormat = strFormat,
            };

            var tweener = WindowSystem.GetTweener();
            tweener.Stop(this.textComponent);
            tweener.Add(closureData, this.animationTime, (float)prevValue, (float)value).Tag(this.textComponent).OnUpdate((closure, val) => {

                closure.module.setValueInternal = true;
                if (closure.sourceValue == UnityEngine.UI.Windows.Components.SourceValue.Digits) {

                    if (string.IsNullOrEmpty(closure.module.formatValue) == true) {

                        if (closure.module.roundToInt == true) {

                            closure.module.textComponent.SetValue(Mathf.RoundToInt(val));

                        } else {

                            closure.module.textComponent.SetValue(val);

                        }

                    } else {

                        if (closure.module.roundToInt == true) {

                            closure.module.textComponent.SetText_INTERNAL(Mathf.RoundToInt(val).ToString(closure.module.formatValue));

                        } else {

                            closure.module.textComponent.SetText_INTERNAL(val.ToString(closure.module.formatValue));

                        }

                    }

                } else if (closure.sourceValue == UnityEngine.UI.Windows.Components.SourceValue.Seconds) {
                    
                    closure.module.textComponent.SetText_INTERNAL(new UnityEngine.UI.Windows.Components.TextComponent.TimeFormatFromSeconds() { format = closure.strFormat }.GetValue(val));
                    
                } else if (closure.sourceValue == UnityEngine.UI.Windows.Components.SourceValue.Milliseconds) {
                    
                    closure.module.textComponent.SetText_INTERNAL(new UnityEngine.UI.Windows.Components.TextComponent.TimeFormatFromMilliseconds() { format = closure.strFormat }.GetValue(val));
                    
                }

                closure.module.setValueInternal = false;

            });

        }

    }
    
}
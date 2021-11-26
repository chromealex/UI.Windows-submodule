namespace UnityEngine.UI.Windows {

    [ComponentModuleDisplayName("Animate Value")]
    public class TextComponentModuleAnimateValue : TextComponentModule {

        public float animationTime = 1f;
        public bool roundToInt = true;
        public string formatValue;

        private bool setValueInternal;
        
        public override void OnSetValue(double prevValue, double value, UnityEngine.UI.Windows.Components.SourceValue sourceValue, string strFormat) {

            if (this.setValueInternal == true) return;

            var tweener = WindowSystem.GetTweener();
            tweener.Stop(this.textComponent);
            tweener.Add(this, this.animationTime, (float)prevValue, (float)value).Tag(this.textComponent).OnUpdate((obj, val) => {

                obj.setValueInternal = true;
                if (sourceValue == UnityEngine.UI.Windows.Components.SourceValue.Digits) {

                    if (string.IsNullOrEmpty(obj.formatValue) == true) {

                        if (obj.roundToInt == true) {

                            obj.textComponent.SetValue(Mathf.RoundToInt(val));

                        } else {

                            obj.textComponent.SetValue(val);

                        }

                    } else {

                        if (obj.roundToInt == true) {

                            obj.textComponent.SetText_INTERNAL(Mathf.RoundToInt(val).ToString(obj.formatValue));

                        } else {

                            obj.textComponent.SetText_INTERNAL(val.ToString(obj.formatValue));

                        }

                    }

                } else if (sourceValue == UnityEngine.UI.Windows.Components.SourceValue.Seconds) {
                    
                    obj.textComponent.SetText_INTERNAL(new UnityEngine.UI.Windows.Components.TextComponent.TimeFormatFromSeconds() { format = strFormat }.GetValue(value));
                    
                } else if (sourceValue == UnityEngine.UI.Windows.Components.SourceValue.Milliseconds) {
                    
                    obj.textComponent.SetText_INTERNAL(new UnityEngine.UI.Windows.Components.TextComponent.TimeFormatFromMilliseconds() { format = strFormat }.GetValue(value));
                    
                }

                obj.setValueInternal = false;

            });

        }

    }
    
}
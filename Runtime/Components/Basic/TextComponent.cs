using System.Collections;

namespace UnityEngine.UI.Windows.Components {

    using Utilities;

    public interface IValueFormat {

        string GetValue(double value);

    }

    public enum SourceValue {

        Digits = 0,
        Seconds,
        Milliseconds,

    }

    public enum TimeValue {

        Milliseconds,
        Seconds,
        Minutes,
        Hours,
        Days,

    }

    public enum TimeResult {

        None = 0,
        TimeMS,
        TimeHM,
        TimeHMS,
        TimeDHMS,
        TimeMSmi,
        TimeHMSmi,
        TimeDHMSmi,

    }

    public partial class TextComponent : WindowComponent, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() {
            return typeof(TextComponentModule);
        }

        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() {
            return this.componentModules.modules;
        }

        public struct TimeFormatFromSeconds : IValueFormat {

            public string format;

            public string GetValue(double value) {

                var ts = System.TimeSpan.FromSeconds(value);
                return ts.ToString(this.format);

            }

        }

        public struct TimeFormatFromMilliseconds : IValueFormat {

            public string format;

            public string GetValue(double value) {

                var ts = System.TimeSpan.FromMilliseconds(value);
                return ts.ToString(this.format);

            }

        }

        public readonly struct TimeShort {

            private readonly System.TimeSpan timeSpan;
            private readonly TimeResultStrings timeResultStrings;

            public TimeShort(double value, TimeResultStrings timeResultStrings, SourceValue sourceValue) {

                this.timeResultStrings = timeResultStrings;
                this.timeSpan = default;

                switch (sourceValue) {

                    case SourceValue.Seconds:
                        this.timeSpan = System.TimeSpan.FromSeconds(value);
                        break;

                    case SourceValue.Milliseconds:
                        this.timeSpan = System.TimeSpan.FromMilliseconds(value);
                        break;

                }

            }

            public string GetShortestString(TimeResult shortestVariant, TimeResult sourceVariant) {

                var str = string.Empty;
                switch (shortestVariant) {

                    case TimeResult.TimeMSmi: {
                        var s = TimeResult.TimeMSmi;
                        if (s <= sourceVariant) shortestVariant = s;
                        if (this.timeSpan.TotalHours >= 1d) {
                            goto case TimeResult.TimeHMSmi;
                        }
                    }
                        break;

                    case TimeResult.TimeHMSmi: {
                        var s = TimeResult.TimeHMSmi;
                        if (s <= sourceVariant) shortestVariant = s;
                        if (this.timeSpan.TotalDays >= 1d) {
                            goto case TimeResult.TimeDHMSmi;
                        }
                    }
                        break;

                    case TimeResult.TimeDHMSmi: {
                        var s = TimeResult.TimeDHMSmi;
                        if (s <= sourceVariant) shortestVariant = s;
                        var format = new TimeFormat(this.timeResultStrings, TimeResult.TimeDHMSmi);
                        str = format.GetString();
                        return str;
                    }

                    case TimeResult.TimeMS: {
                        var s = TimeResult.TimeMS;
                        if (s <= sourceVariant) shortestVariant = s;
                        if (this.timeSpan.TotalHours >= 1d) {
                            goto case TimeResult.TimeHMS;
                        }
                    }
                        break;

                    case TimeResult.TimeHMS: {
                        var s = TimeResult.TimeHMS;
                        if (s <= sourceVariant) shortestVariant = s;
                        if (this.timeSpan.TotalDays >= 1d) {
                            goto case TimeResult.TimeDHMS;
                        }
                    }
                        break;

                    case TimeResult.TimeDHMS: {
                        var s = TimeResult.TimeDHMS;
                        if (s <= sourceVariant) shortestVariant = s;
                        var format = new TimeFormat(this.timeResultStrings, TimeResult.TimeDHMS);
                        str = format.GetString();
                        return str;
                    }

                }

                {
                    var format = new TimeFormat(this.timeResultStrings, shortestVariant);
                    str = format.GetString();
                }
                return str;

            }

        }

        public readonly struct TimeFormat {

            private readonly TimeResultStrings timeResultStrings;
            private readonly TimeResult result;

            public TimeFormat(TimeResultStrings timeResultStrings, TimeResult result) {

                this.timeResultStrings = timeResultStrings;
                this.result = result;

            }

            public string GetString() {

                var ts = this.timeResultStrings;
                var str = string.Empty;
                switch (this.result) {
                    case TimeResult.TimeMS:
                        str = ts.minutesString + ts.secondsString;
                        break;

                    case TimeResult.TimeHM:
                        str = ts.hoursString + ts.minutesStringEnd;
                        break;

                    case TimeResult.TimeHMS:
                        str = ts.hoursString + ts.minutesString + ts.secondsString;
                        break;

                    case TimeResult.TimeDHMS:
                        str = ts.daysString + ts.hoursString + ts.minutesString + ts.secondsString;
                        break;

                    case TimeResult.TimeMSmi:
                        str = ts.minutesString + ts.secondsString + ts.millisecondsString;
                        break;

                    case TimeResult.TimeHMSmi:
                        str = ts.hoursString + ts.minutesString + ts.secondsString;
                        break;

                    case TimeResult.TimeDHMSmi:
                        str = ts.daysString + ts.hoursString + ts.minutesString + ts.secondsString;
                        break;
                }

                return str;

            }

        }

        [System.Serializable]
        public struct TimeResultStrings {

            public string millisecondsString;
            public string secondsString;
            public string minutesString;
            public string minutesStringEnd;
            public string hoursString;
            public string daysString;

        }

        public readonly unsafe struct FormatTimeString {

            private readonly string str;
            private readonly string format;

            public FormatTimeString(string format, string str) {

                var size = str.Length * 2;
                var newStr = stackalloc char[size];
                var k = 0;
                for (var i = 0; i < str.Length; ++i) {

                    newStr[k] = '\\';
                    newStr[k + 1] = str[i];

                    k += 2;

                }

                this.format = format;
                this.str = new string(newStr);

            }

            public string GetValue() {

                return string.Format(this.format, this.str);

            }

        }

        [RequiredReference]
        public UnityEngine.UI.Graphic graphics;
        private TimeResultStrings timeResultStrings = new TimeResultStrings() {
            millisecondsString = @"",
            secondsString = @"ss",
            minutesString = @"mm\:",
            minutesStringEnd = @"mm",
            hoursString = @"hh\:",
            daysString = @"d\d\ ",
        };

        internal override void OnDeInitInternal() {

            base.OnDeInitInternal();

            this.ResetInstance();

        }

        private void ResetInstance() {

            this.lastValueData = default;
            this.lastText = null;

            #if UNITY_LOCALIZATION_SUPPORT

            if (this.lastLocalizationKey != null) {
                this.lastLocalizationKey.StringChanged -= this.OnLocalizationStringChanged;
                this.lastLocalizationKey = null;
            }

            #endif

        }

        public void SetColor(Color color) {

            if (this.graphics == null) {
                return;
            }

            this.graphics.color = color;

        }

        public Color GetColor() {

            if (this.graphics == null) {
                return Color.white;
            }

            return this.graphics.color;

        }

        public void SetTimeResultString(TimeValue timeValue, FormatTimeString str) {

            switch (timeValue) {
                case TimeValue.Milliseconds:
                    this.timeResultStrings.millisecondsString = str.GetValue();
                    break;

                case TimeValue.Seconds:
                    this.timeResultStrings.secondsString = str.GetValue();
                    break;

                case TimeValue.Minutes:
                    this.timeResultStrings.minutesString = str.GetValue();
                    break;

                case TimeValue.Hours:
                    this.timeResultStrings.hoursString = str.GetValue();
                    break;

                case TimeValue.Days:
                    this.timeResultStrings.daysString = str.GetValue();
                    break;
            }

        }

        private string GetFormatTimeString(TimeResultStrings ts, TimeResult result) {

            return new TimeFormat(ts, result).GetString();

        }

        [System.Serializable]
        public struct ValueData {

            public bool Equals(ValueData other) {

                return this.isCreated == other.isCreated &&
                       this.sourceValue == other.sourceValue &&
                       this.timeValueResult == other.timeValueResult &&
                       this.timeShortestVariant == other.timeShortestVariant &&
                       this.value.Equals(other.value);

            }

            public override bool Equals(object obj) {
                return obj is ValueData other && this.Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    var hashCode = this.value.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int)this.sourceValue;
                    hashCode = (hashCode * 397) ^ (int)this.timeValueResult;
                    hashCode = (hashCode * 397) ^ (int)this.timeShortestVariant;
                    return hashCode;
                }
            }

            public double value;
            public SourceValue sourceValue;
            public TimeResult timeValueResult;
            public TimeResult timeShortestVariant;
            public bool isCreated;

            public static bool operator ==(ValueData v1, ValueData v2) {

                return v1.Equals(v2);

            }

            public static bool operator !=(ValueData v1, ValueData v2) {

                return !(v1 == v2);

            }

        }

        private ValueData lastValueData;

        public void SetValue(double value, SourceValue sourceValue = SourceValue.Digits, TimeResult timeValueResult = TimeResult.None,
                             TimeResult timeShortestVariant = TimeResult.None) {

            var currentData = new ValueData() {
                value = value,
                sourceValue = sourceValue,
                timeValueResult = timeValueResult,
                timeShortestVariant = timeShortestVariant,
                isCreated = true,
            };
            if (this.lastValueData == currentData) {
                return;
            }

            var prevData = this.lastValueData;
            this.lastValueData = currentData;
            this.lastText = null;

            string strFormat;
            if (timeShortestVariant > TimeResult.None && timeShortestVariant < timeValueResult) {

                var ts = new TimeShort(value, this.timeResultStrings, sourceValue);
                strFormat = ts.GetShortestString(timeShortestVariant, timeValueResult);

            } else {

                strFormat = this.GetFormatTimeString(this.timeResultStrings, timeValueResult);

            }

            switch (sourceValue) {
                case SourceValue.Digits:
                    this.SetText_INTERNAL(value.ToString());
                    break;

                case SourceValue.Seconds:
                    this.SetText_INTERNAL(new TimeFormatFromSeconds() { format = strFormat }.GetValue(value));
                    break;

                case SourceValue.Milliseconds:
                    this.SetText_INTERNAL(new TimeFormatFromMilliseconds() { format = strFormat }.GetValue(value));
                    break;
            }
            
            this.OnSetValue(prevData.value, value, sourceValue, strFormat);

        }

        private void OnSetValue(double prevValue, double value, SourceValue sourceValue, string strFormat) {

            for (int i = 0; i < this.componentModules.modules.Length; ++i) {
                
                if (this.componentModules.modules[i] is TextComponentModule module) module.OnSetValue(prevValue, value, sourceValue, strFormat);
                
            }
            
        }

        private void OnSetText(string prevValue, string value) {

            for (int i = 0; i < this.componentModules.modules.Length; ++i) {
                
                if (this.componentModules.modules[i] is TextComponentModule module) module.OnSetText(prevValue, value);
                
            }
            
        }

        public string GetText() {

            if (this.graphics is UnityEngine.UI.Text textGraphic) {

                return textGraphic.text;

            }
            #if TEXTMESHPRO_SUPPORT
            else if (this.graphics is TMPro.TMP_Text textGraphicTmp) {

                return textGraphicTmp.text;

            }
            #endif

            return null;

        }

        #if UNITY_LOCALIZATION_SUPPORT

        public static bool localizationTestMode;
        private UnityEngine.Localization.LocalizedString lastLocalizationKey;
        private bool avoidLocalizationUnsubscribe;

        public virtual void SetText(UnityEngine.Localization.LocalizedString key, params object[] args) {

            if (this.lastLocalizationKey != key || (args != null && args.Length > 0)) {

                if (this.lastLocalizationKey != null) {
                    this.lastLocalizationKey.StringChanged -= this.OnLocalizationStringChanged;
                }

                if (localizationTestMode == true) {
                    this.OnLocalizationStringChanged($"#{key?.TableEntryReference.Key}#");
                } else {
                    this.lastLocalizationKey = key;
                    this.lastLocalizationKey.Arguments = args;
                    this.lastLocalizationKey.StringChanged += this.OnLocalizationStringChanged;
                    this.lastLocalizationKey.RefreshString();
                }

            }

        }

        private void OnLocalizationStringChanged(string text) {

            this.avoidLocalizationUnsubscribe = true;
            this.SetText(text);
            this.avoidLocalizationUnsubscribe = false;

        }

        #endif

        private string lastText;

        public virtual void SetText(string text) {

            if (this.lastText == text) {
                return;
            }

            var prevText = this.lastText;
            this.lastText = text;
            this.lastValueData = default;

            #if UNITY_LOCALIZATION_SUPPORT
            if (this.avoidLocalizationUnsubscribe == false && this.lastLocalizationKey != null) {
                this.lastLocalizationKey.StringChanged -= this.OnLocalizationStringChanged;
                this.lastLocalizationKey = null;
            }
            #endif

            this.OnSetText(prevText, text);

            this.SetText_INTERNAL(text);

        }

        internal void SetText_INTERNAL(string text) {
            
            if (this.graphics is UnityEngine.UI.Text textGraphic) {

                textGraphic.text = text;

            }
            #if TEXTMESHPRO_SUPPORT
            else if (this.graphics is TMPro.TMP_Text textGraphicTmp) {

                textGraphicTmp.text = text;

            }
            #endif

        }

        public override void ValidateEditor() {

            base.ValidateEditor();

            if (this.graphics == null) {
                this.graphics = this.GetComponent<Graphic>();
            }

        }

    }

}
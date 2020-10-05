using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Components {

    using Modules;
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
        TimeHMS,
        TimeDHMS,
        TimeMSmi,
        TimeHMSmi,
        TimeDHMSmi,

    }
    
    public class TextComponent : WindowComponent, ISearchComponentByTypeEditor, ISearchComponentByTypeSingleEditor {

        System.Type ISearchComponentByTypeEditor.GetSearchType() { return typeof(TextComponentModule); }
        IList ISearchComponentByTypeSingleEditor.GetSearchTypeArray() { return this.componentModules.modules;}

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
                
                switch (sourceValue) {
                    
                    case SourceValue.Seconds:
                        this.timeSpan = System.TimeSpan.FromSeconds(value);
                        break;

                    case SourceValue.Milliseconds:
                        this.timeSpan = System.TimeSpan.FromMilliseconds(value);
                        break;

                }

            }

            public string GetShortestString(TimeResult shortestVariant) {

                var str = string.Empty;
                switch (shortestVariant) {
                    
                    case TimeResult.TimeMSmi:
                        if (this.timeSpan.TotalHours >= 1d) goto case TimeResult.TimeHMSmi;
                        break;

                    case TimeResult.TimeHMSmi:
                        if (this.timeSpan.TotalDays >= 1d) goto case TimeResult.TimeHMSmi;
                        break;

                    case TimeResult.TimeDHMSmi: {
                        var format = new TimeFormat(this.timeResultStrings, TimeResult.TimeDHMSmi);
                        str = format.GetString();
                        return str;
                    }

                    case TimeResult.TimeMS:
                        if (this.timeSpan.TotalHours >= 1d) goto case TimeResult.TimeHMS;
                        break;

                    case TimeResult.TimeHMS:
                        if (this.timeSpan.TotalDays >= 1d) goto case TimeResult.TimeDHMS;
                        break;

                    case TimeResult.TimeDHMS: {
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
                for (int i = 0; i < str.Length; ++i) {

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
            hoursString = @"hh\:",
            daysString = @"d\d\ ",
        };

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
        
        public void SetValue(double value, SourceValue sourceValue = SourceValue.Digits, TimeResult timeValueResult = TimeResult.None, TimeResult timeShortestVariant = TimeResult.None) {

            string strFormat = string.Empty;
            if (timeShortestVariant > TimeResult.None && timeShortestVariant < timeValueResult) {

                var ts = new TimeShort(value, this.timeResultStrings, sourceValue);
                strFormat = ts.GetShortestString(timeShortestVariant);
                
            } else {

                strFormat = this.GetFormatTimeString(this.timeResultStrings, timeValueResult);

            }
            
            switch (sourceValue) {
                case SourceValue.Seconds:
                    this.SetText(new TimeFormatFromSeconds() { format = strFormat }.GetValue(value));
                    return;
                case SourceValue.Milliseconds:
                    this.SetText(new TimeFormatFromMilliseconds() { format = strFormat }.GetValue(value));
                    return;
            }

        }
        
        public string GetText() {
            
            if (this.graphics is UnityEngine.UI.Text textGraphic) {

                return textGraphic.text;

            }
            #if TEXTMESHPRO_SUPPORT
            else if (this.graphics is TextMeshPro.TMP_Text textGraphicTmp) {
                
                return textGraphicTmp.text;

            }
            #endif

            return null;

        }
        
        public void SetText(string text) {

            if (this.graphics is UnityEngine.UI.Text textGraphic) {

                textGraphic.text = text;

            }
            #if TEXTMESHPRO_SUPPORT
            else if (this.graphics is TextMeshPro.TMP_Text textGraphicTmp) {
                
                textGraphicTmp.text = text;

            }
            #endif

        }
        
        public override void ValidateEditor() {
            
            base.ValidateEditor();

            this.graphics = this.GetComponent<Graphic>();

        }

    }

}
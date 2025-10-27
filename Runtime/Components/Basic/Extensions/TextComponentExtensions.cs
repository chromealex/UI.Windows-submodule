namespace UnityEngine.UI.Windows.Components {
    
    using System.Runtime.InteropServices;

    public partial class InputFieldComponent {

        private Unity.Collections.FixedString512Bytes lastBytesText;
        public void SetText(Unity.Collections.FixedString32Bytes value) {
            if (this.lastBytesText == value) return;
            this.lastBytesText = value;
            this.SetText(value.ToString());
        }

        public void SetText(Unity.Collections.FixedString64Bytes value) {
            if (this.lastBytesText == value) return;
            this.lastBytesText = value;
            this.SetText(value.ToString());
        }

        public void SetText(Unity.Collections.FixedString128Bytes value) {
            if (this.lastBytesText == value) return;
            this.lastBytesText = value;
            this.SetText(value.ToString());
        }

        public void SetText(Unity.Collections.FixedString512Bytes value) {
            if (this.lastBytesText == value) return;
            this.lastBytesText = value;
            this.SetText(value.ToString());
        }

    }
    
    public partial class TextComponent {

        private const int DEFAULT_INTEGER_NUMBER_LENGTH = 2;
        private System.Text.StringBuilder sb;

        public void SetText(double value) {
            this.SetValue(value);
        }

        private Unity.Collections.FixedString512Bytes lastBytesText;
        public void SetText(Unity.Collections.FixedString32Bytes value) {
            this.ResetLastCacheOther(CacheLayer.Bytes);
            if (this.lastBytesText == value) return;
            this.lastBytesText = value;
            this.SetText(value.ToString());
        }

        public void SetText(Unity.Collections.FixedString64Bytes value) {
            this.ResetLastCacheOther(CacheLayer.Bytes);
            if (this.lastBytesText == value) return;
            this.lastBytesText = value;
            this.SetText(value.ToString());
        }

        public void SetText(Unity.Collections.FixedString128Bytes value) {
            this.ResetLastCacheOther(CacheLayer.Bytes);
            if (this.lastBytesText == value) return;
            this.lastBytesText = value;
            this.SetText(value.ToString());
        }

        public void SetText(Unity.Collections.FixedString512Bytes value) {
            this.ResetLastCacheOther(CacheLayer.Bytes);
            if (this.lastBytesText == value) return;
            this.lastBytesText = value;
            this.SetText(value.ToString());
        }

        public struct LastDataCache : System.IEquatable<LastDataCache> {

            [StructLayout(LayoutKind.Explicit)]
            public struct CacheData {

                [FieldOffset(0)]
                public int i0;
                [FieldOffset(4)]
                public int i1;
                [FieldOffset(8)]
                public int i2;
                [FieldOffset(12)]
                public int i3;
                
                [FieldOffset(0)]
                public float f0;
                [FieldOffset(4)]
                public float f1;
                [FieldOffset(8)]
                public float f2;
                [FieldOffset(12)]
                public float f3;
                
            }

            public CacheData cacheData;

            public int i0 {
                set => this.cacheData.i0 = value;
                get => this.cacheData.i0;
            }
            public int i1 {
                set => this.cacheData.i1 = value;
                get => this.cacheData.i1;
            }
            public int i2 {
                set => this.cacheData.i2 = value;
                get => this.cacheData.i2;
            }
            public int i3 {
                set => this.cacheData.i3 = value;
                get => this.cacheData.i3;
            }
            public float f0 {
                set => this.cacheData.f0 = value;
                get => this.cacheData.f0;
            }
            public float f1 {
                set => this.cacheData.f1 = value;
                get => this.cacheData.f1;
            }
            public float f2 {
                set => this.cacheData.f2 = value;
                get => this.cacheData.f2;
            }
            public float f3 {
                set => this.cacheData.f3 = value;
                get => this.cacheData.f3;
            }
            public string s0;
            public string s1;
            public string s2;
            public string s3;
            #if UNITY_LOCALIZATION_SUPPORT
            public UnityEngine.Localization.LocalizedString loc;
            #endif

            public static bool operator ==(LastDataCache lhs, LastDataCache rhs) {
                return lhs.Equals(rhs);
            }

            public static bool operator !=(LastDataCache lhs, LastDataCache rhs) {
                return !(lhs == rhs);
            }

            public bool Equals(LastDataCache other) {
                return this.s0 == other.s0 && this.s1 == other.s1 && this.s2 == other.s2 && this.s3 == other.s3 && this.i0 == other.i0 && this.i1 == other.i1 && this.i2 == other.i2 && this.i3 == other.i3
                       #if UNITY_LOCALIZATION_SUPPORT
                       && this.loc == other.loc
                       #endif
                       ;
            }

            public override bool Equals(object obj) {
                return obj is LastDataCache other && this.Equals(other);
            }

            public override int GetHashCode() {
                return System.HashCode.Combine(this.s0, this.s1, this.s2, this.s3, this.i0, this.i1, this.i2, this.i3) 
                       #if UNITY_LOCALIZATION_SUPPORT
                       ^ this.loc.GetHashCode()
                       #endif
                       ;
            }

        }

        private LastDataCache lastData;

        public void SetText(int i0, string s1) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { i0 = i0, s1 = s1, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+DEFAULT_INTEGER_NUMBER_LENGTH + s1.Length);
            }

            this.sb.Clear().Append(i0).Append(s1);
            this.SetText(this.sb.ToString());
        }

        public void SetText(string s0, int i1) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { s0 = s0, i1 = i1, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+s0.Length + DEFAULT_INTEGER_NUMBER_LENGTH);
            }

            this.sb.Clear().Append(s0).Append(i1);
            this.SetText(this.sb.ToString());
        }

        public void SetText(int i0, int i1) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { i0 = i0, i1 = i1, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH);
            }

            this.sb.Clear().Append(i0).Append(i1);
            this.SetText(this.sb.ToString());
        }

        public void SetText(string s0, string s1) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { s0 = s0, s1 = s1, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+s0.Length + s1.Length);
            }

            this.sb.Clear().Append(s0).Append(s1);
            this.SetText(this.sb.ToString());
        }

        public void SetText(string s0, string s1, string s2) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { s0 = s0, s1 = s1, s2 = s2, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+s0.Length + s1.Length + s2.Length);
            }

            this.sb.Clear().Append(s0).Append(s1).Append(s2);
            this.SetText(this.sb.ToString());
        }

        public void SetText(int i0, int i1, int i2) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { i0 = i0, i1 = i1, i2 = i2, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH);
            }

            this.sb.Clear().Append(i0).Append(i1).Append(i2);
            this.SetText(this.sb.ToString());
        }

        public void SetText(string s0, int i1, int i2) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { s0 = s0, i1 = i1, i2 = i2, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+s0.Length + DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH);
            }

            this.sb.Clear().Append(s0).Append(i1).Append(i2);
            this.SetText(this.sb.ToString());
        }

        public void SetText(string s0, string s1, int i2) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { s0 = s0, s1 = s1, i2 = i2, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+s0.Length + s1.Length + DEFAULT_INTEGER_NUMBER_LENGTH);
            }

            this.sb.Clear().Append(s0).Append(s1).Append(i2);
            this.SetText(this.sb.ToString());
        }

        public void SetText(int i0, string s1, string s2) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { i0 = i0, s1 = s1, s2 = s2, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+DEFAULT_INTEGER_NUMBER_LENGTH + s1.Length + s2.Length);
            }

            this.sb.Clear().Append(i0).Append(s1).Append(s2);
            this.SetText(this.sb.ToString());
        }

        public void SetText(int i0, int i1, string s2) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { i0 = i0, i1 = i1, s2 = s2, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH + s2.Length);
            }

            this.sb.Clear().Append(i0).Append(i1).Append(s2);
            this.SetText(this.sb.ToString());
        }

        public void SetText(string s0, int i1, string s2) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { s0 = s0, i1 = i1, s2 = s2, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+s0.Length + DEFAULT_INTEGER_NUMBER_LENGTH + s2.Length);
            }

            this.sb.Clear().Append(s0).Append(i1).Append(s2);
            this.SetText(this.sb.ToString());
        }

        public void SetText(int i0, string s1, int i2) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { i0 = i0, s1 = s1, i2 = i2, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+DEFAULT_INTEGER_NUMBER_LENGTH + s1.Length + DEFAULT_INTEGER_NUMBER_LENGTH);
            }

            this.sb.Clear().Append(i0).Append(s1).Append(i2);
            this.SetText(this.sb.ToString());
        }

        public void SetText(string s0, string s1, string s2, string s3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() {
                s0 = s0, s1 = s1, s2 = s2, s3 = s3,
            };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+s0.Length + s1.Length + s2.Length + s3.Length);
            }

            this.sb.Clear().Append(s0).Append(s1).Append(s2).Append(s3);
            this.SetText(this.sb.ToString());
        }

        public void SetText(int i0, int i1, int i2, int i3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() {
                i0 = i0, i1 = i1, i2 = i2, i3 = i3,
            };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH +
                                                        DEFAULT_INTEGER_NUMBER_LENGTH);
            }

            this.sb.Clear().Append(i0).Append(i1).Append(i2).Append(i3);
            this.SetText(this.sb.ToString());
        }

        public void SetText(string s0, int i1, int i2, int i3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() {
                s0 = s0, i1 = i1, i2 = i2, i3 = i3,
            };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+s0.Length + DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH);
            }

            this.sb.Clear().Append(s0).Append(i1).Append(i2).Append(i3);
            this.SetText(this.sb.ToString());
        }

        public void SetText(string s0, string s1, int i2, int i3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() {
                s0 = s0, s1 = s1, i2 = i2, i3 = i3,
            };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+s0.Length + s1.Length + DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH);
            }

            this.sb.Clear().Append(s0).Append(s1).Append(i2).Append(i3);
            this.SetText(this.sb.ToString());
        }

        public void SetText(string s0, string s1, string s2, int i3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() {
                s0 = s0, s1 = s1, s2 = s2, i3 = i3,
            };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+s0.Length + s1.Length + s2.Length + DEFAULT_INTEGER_NUMBER_LENGTH);
            }

            this.sb.Clear().Append(s0).Append(s1).Append(s2).Append(i3);
            this.SetText(this.sb.ToString());
        }

        public void SetText(int i0, string s1, string s2, string s3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() {
                i0 = i0, s1 = s1, s2 = s2, s3 = s3,
            };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+DEFAULT_INTEGER_NUMBER_LENGTH + s1.Length + s2.Length + s3.Length);
            }

            this.sb.Clear().Append(i0).Append(s1).Append(s2).Append(s3);
            this.SetText(this.sb.ToString());
        }

        public void SetText(int i0, int i1, string s2, string s3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() {
                i0 = i0, i1 = i1, s2 = s2, s3 = s3,
            };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH + s2.Length + s3.Length);
            }

            this.sb.Clear().Append(i0).Append(i1).Append(s2).Append(s3);
            this.SetText(this.sb.ToString());
        }

        public void SetText(int i0, int i1, int i2, string s3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() {
                i0 = i0, i1 = i1, i2 = i2, s3 = s3,
            };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH + s3.Length);
            }

            this.sb.Clear().Append(i0).Append(i1).Append(i2).Append(s3);
            this.SetText(this.sb.ToString());
        }

        public void SetText(string s0, int i1, int i2, string s3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() {
                s0 = s0, i1 = i1, i2 = i2, s3 = s3,
            };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+s0.Length + DEFAULT_INTEGER_NUMBER_LENGTH + DEFAULT_INTEGER_NUMBER_LENGTH + s3.Length);
            }

            this.sb.Clear().Append(s0).Append(i1).Append(i2).Append(s3);
            this.SetText(this.sb.ToString());
        }

        public void SetText(string s0, int i1, string s2, int i3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() {
                s0 = s0, i1 = i1, s2 = s2, i3 = i3,
            };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+s0.Length + DEFAULT_INTEGER_NUMBER_LENGTH + s2.Length + DEFAULT_INTEGER_NUMBER_LENGTH);
            }

            this.sb.Clear().Append(s0).Append(i1).Append(s2).Append(i3);
            this.SetText(this.sb.ToString());
        }

        public void SetText(int i0, string s1, int i2, string s3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() {
                i0 = i0, s1 = s1, i2 = i2, s3 = s3,
            };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+DEFAULT_INTEGER_NUMBER_LENGTH + s1.Length + DEFAULT_INTEGER_NUMBER_LENGTH + s3.Length);
            }

            this.sb.Clear().Append(i0).Append(s1).Append(i2).Append(s3);
            this.SetText(this.sb.ToString());
        }

        public void SetText(int i0, string s1, string s2, int i3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() {
                i0 = i0, s1 = s1, s2 = s2, i3 = i3,
            };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            if (this.sb == null) {
                this.sb = new System.Text.StringBuilder(+DEFAULT_INTEGER_NUMBER_LENGTH + s1.Length + s2.Length + DEFAULT_INTEGER_NUMBER_LENGTH);
            }

            this.sb.Clear().Append(i0).Append(s1).Append(s2).Append(i3);
            this.SetText(this.sb.ToString());
        }

        public void SetTextFormat(string template, int i1) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { s0 = template, i1 = i1, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(string.Format(template, i1));
        }

        public void SetTextFormat(string template, int i1, int i2) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { s0 = template, i1 = i1, i2 = i2 };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(string.Format(template, i1, i2));
        }

        public void SetTextFormat(string template, float f1) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { s0 = template, f1 = f1 };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(string.Format(template, f1));
        }

        public void SetTextFormat(string template, float f1, float f2) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { s0 = template, f1 = f1, f2 = f2 };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(string.Format(template, f1, f2));
        }

        public void SetTextFormat(string template, float f1, float f2, float f3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { s0 = template, f0 = f1, f1 = f2, f2 = f3 };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(string.Format(template, f1, f2, f3));
        }

        public void SetTextFormat(string template, float f1, float f2, float f3, float f4) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { s0 = template, f0 = f1, f1 = f2, f2 = f3, f3 = f4 };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(string.Format(template, f1, f2, f3, f4));
        }

        #if UNITY_LOCALIZATION_SUPPORT
        
        public void SetTextFormat(UnityEngine.Localization.LocalizedString template, int i1) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { loc = template, i1 = i1, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(template, i1);
        }

        public void SetTextFormat(UnityEngine.Localization.LocalizedString template, int i1, int i2) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { loc = template, i1 = i1, i2 = i2 };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(template, i1, i2);
        }

        public void SetTextFormat(UnityEngine.Localization.LocalizedString template, int i1, int i2, int i3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { loc = template, i0 = i1, i1 = i2, i2 = i3, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(template, i1, i2, i3);
        }

        public void SetTextFormat(UnityEngine.Localization.LocalizedString template, int i1, int i2, int i3, int i4) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { loc = template, i0 = i1, i1 = i2, i2 = i3, i3 = i4, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(template, i1, i2, i3, i4);
        }

        public void SetTextFormat(UnityEngine.Localization.LocalizedString template, float f1) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { loc = template, f0 = f1, };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(template, f1);
        }

        public void SetTextFormat(UnityEngine.Localization.LocalizedString template, float f1, float f2) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { loc = template, f0 = f1, f1 = f2 };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(template, f1, f2);
        }

        public void SetTextFormat(UnityEngine.Localization.LocalizedString template, float f1, float f2, float f3) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { loc = template, f0 = f1, f1 = f2, f2 = f3 };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(template, f1, f2, f3);
        }

        public void SetTextFormat(UnityEngine.Localization.LocalizedString template, float f1, float f2, float f3, float f4) {
            this.ResetLastCacheOther(CacheLayer.Data);
            var data = new LastDataCache() { loc = template, f0 = f1, f1 = f2, f2 = f3, f3 = f4 };
            if (this.lastData == data) {
                return;
            }

            this.lastData = data;
            this.SetText(template, f1, f2, f3, f4);
        }

        #endif

        /*private struct TypeInfo {

            public string type;
            public string letter;

        }
        
        [ContextMenu("Generate")]
        public void Generate() {

            var inp = new [] {
                new [] {
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                },
                new [] {
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                },
                new [] {
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                },
                new [] {
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                },
                
                new [] {
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                },
                new [] {
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                },
                new [] {
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                },
                new [] {
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                },
                new [] {
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                },
                new [] {
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                },
                new [] {
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                },
                new [] {
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                },
                
                new [] {
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                },
                new [] {
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                },
                new [] {
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                },
                new [] {
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                },
                new [] {
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                },
                new [] {
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                },
                new [] {
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                },
                new [] {
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                },
                new [] {
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                },
                new [] {
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                },
                new [] {
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                },
                new [] {
                    new TypeInfo { type = "int", letter = "i" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "string", letter = "s" },
                    new TypeInfo { type = "int", letter = "i" },
                },
            };
            var output = string.Empty;
            foreach (var item in inp) {
                var res = this.Gen(item);
                output += res + "\n";
            }

            Debug.Log(output);

        }

        private string Gen(TypeInfo[] typeInfos) {
            
            var arrParams = typeInfos.Select(x => $"{x.type} {x.letter}").ToArray();
            var arrParamsStr = new string[arrParams.Length];
            for (int i = 0; i < arrParams.Length; ++i) {
                arrParamsStr[i] = $"{arrParams[i]}{i}";
            }
            var arr = typeInfos.Select(x => x.letter).ToArray();
            var arrUseStr = new string[arr.Length];
            for (int i = 0; i < arr.Length; ++i) {
                arrUseStr[i] = $"{arr[i]}{i} = {arr[i]}{i}";
            }
            var appends = string.Empty;
            var lengths = string.Empty;
            for (var index = 0; index < typeInfos.Length; ++index) {
                var item = typeInfos[index];
                appends += $".Append({item.letter}{index})";
                if (item.type == "int") {
                    lengths += " + TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH";
                } else {
                    lengths += $" + {item.letter}{index}.Length";
                }
            }

            //var output = $"private System.ValueTuple<{string.Join(", ", arrTypes)}> lastText{string.Join("", arr)};\n";
            var output = $@"public void SetText({string.Join(", ", arrParamsStr)}) {{
                var data = new LastDataCache() {{
                    {string.Join(", ", arrUseStr)},
                }};
                if (this.lastData == data) return;
                this.lastData = data;
                if (this.sb == null) this.sb = new System.Text.StringBuilder({lengths});
                this.sb.Clear(){appends};
                this.SetText(this.sb.ToString());
            }}";
            return output;
            
        }*/

    }

}
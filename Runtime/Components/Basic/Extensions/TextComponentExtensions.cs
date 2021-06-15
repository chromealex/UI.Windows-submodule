namespace UnityEngine.UI.Windows.Components {

    public partial class TextComponent {

        private const int DEFAULT_INTEGER_NUMBER_LENGTH = 2;
        private System.Text.StringBuilder sb;
        
        private struct LastText2ss {

            public string s1;
            public string s2;

        }
        private LastText2ss lastText2ss;
        public void SetText(string text1, string text2) {

            if (this.lastText2ss.s1 == text1 && this.lastText2ss.s2 == text2) return;
            this.lastText2ss = new LastText2ss() { s1 = text1, s2 = text2 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(text1.Length + text2.Length);
            this.sb.Clear().Append(text1).Append(text2);
            this.SetText(this.sb.ToString());
            
        }

        private struct LastText2si {

            public string s1;
            public int s2;

        }
        private LastText2si lastText2si;
        public void SetText(string text1, int text2) {

            if (this.lastText2si.s1 == text1 && this.lastText2si.s2 == text2) return;
            this.lastText2si = new LastText2si() { s1 = text1, s2 = text2 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(text1.Length + TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH);
            this.sb.Clear().Append(text1).Append(text2);
            this.SetText(this.sb.ToString());
            
        }

        private struct LastText2is {

            public int s1;
            public string s2;

        }
        private LastText2is lastText2is;
        public void SetText(int text1, string text2) {

            if (this.lastText2is.s1 == text1 && this.lastText2is.s2 == text2) return;
            this.lastText2is = new LastText2is() { s1 = text1, s2 = text2 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH + text2.Length);
            this.sb.Clear().Append(text1).Append(text2);
            this.SetText(this.sb.ToString());
            
        }

        private struct LastText3ssi {

            public string s1;
            public string s2;
            public int s3;

        }
        private LastText3ssi lastText3ssi;
        public void SetText(string text1, string text2, int text3) {

            if (this.lastText3ssi.s1 == text1 && this.lastText3ssi.s2 == text2 && this.lastText3ssi.s3 == text3) return;
            this.lastText3ssi = new LastText3ssi() { s1 = text1, s2 = text2, s3 = text3 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(text1.Length + text2.Length);
            this.sb.Clear().Append(text1).Append(text2).Append(text3);
            this.SetText(this.sb.ToString());
            
        }

        private struct LastText3sis {

            public string s1;
            public int s2;
            public string s3;
            
        }
        private LastText3sis lastText3sis;
        public void SetText(string text1, int text2, string text3) {

            if (this.lastText3sis.s1 == text1 && this.lastText3sis.s2 == text2 && this.lastText3sis.s3 == text3) return;
            this.lastText3sis = new LastText3sis() { s1 = text1, s2 = text2, s3 = text3 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(text1.Length + text3.Length);
            this.sb.Clear().Append(text1).Append(text2).Append(text3);
            this.SetText(this.sb.ToString());
            
        }

        private struct LastText3isi {

            public int s1;
            public string s2;
            public int s3;

        }
        private LastText3isi lastText3isi;
        public void SetText(int text1, string text2, int text3) {

            if (this.lastText3isi.s1 == text1 && this.lastText3isi.s2 == text2 && this.lastText3isi.s3 == text3) return;
            this.lastText3isi = new LastText3isi() { s1 = text1, s2 = text2, s3 = text3 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH + text2.Length + TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH);
            this.sb.Clear().Append(text1).Append(text2).Append(text3);
            this.SetText(this.sb.ToString());

        }

        private struct LastText3iss {

            public int s1;
            public string s2;
            public string s3;

        }
        private LastText3iss lastText3iss;
        public void SetText(int text1, string text2, string text3) {

            if (this.lastText3iss.s1 == text1 && this.lastText3iss.s2 == text2 && this.lastText3iss.s3 == text3) return;
            this.lastText3iss = new LastText3iss() { s1 = text1, s2 = text2, s3 = text3 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH + text2.Length + text3.Length);
            this.sb.Clear().Append(text1).Append(text2).Append(text3);
            this.SetText(this.sb.ToString());

        }

        private struct LastText3sss {

            public string s1;
            public string s2;
            public string s3;

        }
        private LastText3sss lastText3sss;
        public void SetText(string text1, string text2, string text3) {

            if (this.lastText3sss.s1 == text1 && this.lastText3sss.s2 == text2 && this.lastText3sss.s3 == text3) return;
            this.lastText3sss = new LastText3sss() { s1 = text1, s2 = text2, s3 = text3 };

            if (this.sb == null) this.sb = new System.Text.StringBuilder(text1.Length + text2.Length + text3.Length);
            this.sb.Clear().Append(text1).Append(text2).Append(text3);
            this.SetText(this.sb.ToString());
            
        }

        private struct LastText4ssss {

            public string s1;
            public string s2;
            public string s3;
            public string s4;

        }
        private LastText4ssss lastText4ssss;
        public void SetText(string text1, string text2, string text3, string text4) {

            if (this.lastText4ssss.s1 == text1 && this.lastText4ssss.s2 == text2 && this.lastText4ssss.s3 == text3 && this.lastText4ssss.s4 == text4) return;
            this.lastText4ssss = new LastText4ssss() { s1 = text1, s2 = text2, s3 = text3, s4 = text4 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(text1.Length + text2.Length + text3.Length + text4.Length);
            this.sb.Clear().Append(text1).Append(text2).Append(text3).Append(text4);
            this.SetText(this.sb.ToString());
            
        }

        private struct LastText4isss {

            public int s1;
            public string s2;
            public string s3;
            public string s4;

        }
        private LastText4isss lastText4isss;
        public void SetText(int text1, string text2, string text3, string text4) {

            if (this.lastText4isss.s1 == text1 && this.lastText4isss.s2 == text2 && this.lastText4isss.s3 == text3 && this.lastText4isss.s4 == text4) return;
            this.lastText4isss = new LastText4isss() { s1 = text1, s2 = text2, s3 = text3, s4 = text4 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH + text2.Length + text3.Length + text4.Length);
            this.sb.Clear().Append(text1).Append(text2).Append(text3).Append(text4);
            this.SetText(this.sb.ToString());
            
        }

        private struct LastText4isis {

            public int s1;
            public string s2;
            public int s3;
            public string s4;

        }
        private LastText4isis lastText4isis;
        public void SetText(int text1, string text2, int text3, string text4) {

            if (this.lastText4isis.s1 == text1 && this.lastText4isis.s2 == text2 && this.lastText4isis.s3 == text3 && this.lastText4isis.s4 == text4) return;
            this.lastText4isis = new LastText4isis() { s1 = text1, s2 = text2, s3 = text3, s4 = text4 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH + text2.Length + TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH + text4.Length);
            this.sb.Clear().Append(text1).Append(text2).Append(text3).Append(text4);
            this.SetText(this.sb.ToString());
            
        }

        private struct LastText4siss {

            public string s1;
            public int s2;
            public string s3;
            public string s4;

        }
        private LastText4siss lastText4siss;
        public void SetText(string text1, int text2, string text3, string text4) {

            if (this.lastText4siss.s1 == text1 && this.lastText4siss.s2 == text2 && this.lastText4siss.s3 == text3 && this.lastText4siss.s4 == text4) return;
            this.lastText4siss = new LastText4siss() { s1 = text1, s2 = text2, s3 = text3, s4 = text4 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(text1.Length + TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH + text3.Length + text4.Length);
            this.sb.Clear().Append(text1).Append(text2).Append(text3).Append(text4);
            this.SetText(this.sb.ToString());
            
        }

        private struct LastText4sisi {

            public string s1;
            public int s2;
            public string s3;
            public int s4;

        }
        private LastText4sisi lastText4sisi;
        public void SetText(string text1, int text2, string text3, int text4) {

            if (this.lastText4sisi.s1 == text1 && this.lastText4sisi.s2 == text2 && this.lastText4sisi.s3 == text3 && this.lastText4sisi.s4 == text4) return;
            this.lastText4sisi = new LastText4sisi() { s1 = text1, s2 = text2, s3 = text3, s4 = text4 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(text1.Length + TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH + text3.Length + TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH);
            this.sb.Clear().Append(text1).Append(text2).Append(text3).Append(text4);
            this.SetText(this.sb.ToString());
            
        }

        private struct LastText4ssis {

            public string s1;
            public string s2;
            public int s3;
            public string s4;

        }
        private LastText4ssis lastText4ssis;
        public void SetText(string text1, string text2, int text3, string text4) {

            if (this.lastText4ssis.s1 == text1 && this.lastText4ssis.s2 == text2 && this.lastText4ssis.s3 == text3 && this.lastText4ssis.s4 == text4) return;
            this.lastText4ssis = new LastText4ssis() { s1 = text1, s2 = text2, s3 = text3, s4 = text4 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(text1.Length + text2.Length + TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH + text4.Length);
            this.sb.Clear().Append(text1).Append(text2).Append(text3).Append(text4);
            this.SetText(this.sb.ToString());
            
        }

        private struct LastText4sssi {

            public string s1;
            public string s2;
            public string s3;
            public int s4;

        }
        private LastText4sssi lastText4sssi;
        public void SetText(string text1, string text2, string text3, int text4) {

            if (this.lastText4sssi.s1 == text1 && this.lastText4sssi.s2 == text2 && this.lastText4sssi.s3 == text3 && this.lastText4sssi.s4 == text4) return;
            this.lastText4sssi = new LastText4sssi() { s1 = text1, s2 = text2, s3 = text3, s4 = text4 };
            
            if (this.sb == null) this.sb = new System.Text.StringBuilder(text1.Length + text2.Length + text3.Length + TextComponent.DEFAULT_INTEGER_NUMBER_LENGTH);
            this.sb.Clear().Append(text1).Append(text2).Append(text3).Append(text4);
            this.SetText(this.sb.ToString());
            
        }

    }

}
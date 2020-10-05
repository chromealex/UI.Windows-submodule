using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Utilities {

    public static class UIWSMath {

        public static long GetKey(int a, int b) {

            long res = b;
            res <<= 32;
            res |= (uint)a;
            return res;

        }

        public static void GetKey(long key, out int a, out int b) {

            a = (int)(key & uint.MaxValue);
            b = (int)(key >> 32);

        }

    }

}
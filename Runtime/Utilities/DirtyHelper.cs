namespace UnityEngine.UI.Windows.Utilities {

    public class DirtyHelper {

        public bool isDirty = false;
        private Object obj;

        public DirtyHelper(Object obj) {

            this.isDirty = false;
            this.obj = obj;

        }

        public bool SetObj<T>(ref System.Collections.Generic.List<T> fieldValue, System.Collections.Generic.List<T> newValue) where T : class {

            bool CheckArr(System.Collections.Generic.List<T> arr1, System.Collections.Generic.List<T> arr2) {

                if (arr1 == null && arr2 == null) return true;
                if ((arr1 == null && arr2 != null) ||
                    (arr1 != null && arr2 == null) ||
                    (arr1.Count != arr2.Count)) {

                    return false;

                }

                var areEquals = true;
                for (int i = 0; i < arr1.Count; ++i) {

                    var temp = arr1[i];
                    if (this.SetObj(ref temp, arr2[i]) == true) {
                        arr1[i] = arr2[i];
                        areEquals = false;
                    }

                }
                
                return areEquals;
                
            }

            if (CheckArr(fieldValue, newValue) == false) {

                fieldValue = newValue;
                this.isDirty = true;
                return true;

            }

            return false;

        }

        public bool Set<T>(ref System.Collections.Generic.List<T> fieldValue, System.Collections.Generic.List<T> newValue) where T : System.IEquatable<T> {

            bool CheckArr(System.Collections.Generic.List<T> arr1, System.Collections.Generic.List<T> arr2) {

                if (arr1 == null && arr2 == null) return true;
                if ((arr1 == null && arr2 != null) ||
                    (arr1 != null && arr2 == null) ||
                    (arr1.Count != arr2.Count)) {

                    return false;

                }

                var areEquals = true;
                for (int i = 0; i < arr1.Count; ++i) {

                    var temp = arr1[i];
                    if (this.Set(ref temp, arr2[i]) == true) {
                        arr1[i] = arr2[i];
                        areEquals = false;
                    }

                }
                
                return areEquals;
                
            }

            if (CheckArr(fieldValue, newValue) == false) {

                fieldValue = newValue;
                this.isDirty = true;
                return true;

            }

            return false;

        }

        public bool Set<T>(ref T[] fieldValue, T[] newValue) where T : System.IEquatable<T> {

            bool CheckArr(T[] arr1, T[] arr2) {

                if (arr1 == null && arr2 == null) return true;
                if ((arr1 == null && arr2 != null) ||
                    (arr1 != null && arr2 == null) ||
                    (arr1.Length != arr2.Length)) {

                    return false;

                }

                var areEquals = true;
                for (int i = 0; i < arr1.Length; ++i) {

                    if (this.Set(ref arr1[i], arr2[i]) == true) {
                        arr1[i] = arr2[i];
                        areEquals = false;
                    }

                }
                
                return areEquals;
                
            }

            if (CheckArr(fieldValue, newValue) == false) {

                fieldValue = newValue;
                this.isDirty = true;
                return true;

            }

            return false;

        }

        public bool Set<T>(ref T fieldValue, T newValue) where T : System.IEquatable<T> {

            if (fieldValue.Equals(newValue) == false) {

                fieldValue = newValue;
                this.isDirty = true;
                return true;

            }

            return false;

        }

        public bool SetObj<T>(ref T fieldValue, T newValue) where T : class {

			if (fieldValue == null && newValue != null) {
				fieldValue = newValue;
				this.isDirty = true;
				return true;
			} else if (fieldValue == null && newValue == null) {
				return false;
			}
            if (fieldValue.Equals(newValue) == false) {

                fieldValue = newValue;
                this.isDirty = true;
                return true;

            }

            return false;

        }

        public bool SetEnum<T>(ref T fieldValue, T newValue) where T : System.Enum {

            if (Equals(fieldValue, newValue) == false) {

                fieldValue = newValue;
                this.isDirty = true;
                return true;

            }

            return false;

        }

        public void Apply() {
            
            #if UNITY_EDITOR
            if (this.isDirty == true) {
                UnityEditor.EditorUtility.SetDirty(this.obj);
                this.isDirty = false;
                //var root = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(this.obj);
                //Debug.Log($"Set Dirty: {this.obj}", this.obj);
            }
            #endif
            
        }

    }

}

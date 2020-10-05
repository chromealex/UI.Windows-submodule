using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Utilities {

    public class SearchAssetsByTypePopupAttribute : PropertyAttribute {

        public System.Type filterType;
        public string filterDir;
        public string menuName;
        public string innerField;
        public string noneOption;
        
        public SearchAssetsByTypePopupAttribute(System.Type filterType = null, string filterDir = null, string menuName = null, string innerField = null, string noneOption = "None") {

            this.filterType = filterType;
            this.filterDir = filterDir;
            this.menuName = menuName;
            this.innerField = innerField;
            this.noneOption = noneOption;

        }

    }

    public interface ISearchComponentByTypeEditor {

        System.Type GetSearchType();

    }

    public interface ISearchComponentByTypeSingleEditor {

        IList GetSearchTypeArray();

    }

    public class SearchComponentsByTypePopupAttribute : PropertyAttribute {

        public System.Type baseType;
        public string menuName;
        public string innerField;
        public string noneOption;
        public bool allowClassOverrides;

        public SearchComponentsByTypePopupAttribute(System.Type baseType, string menuName = null, string innerField = null, string noneOption = "None", bool allowClassOverrides = false) {

            this.baseType = baseType;
            this.menuName = menuName;
            this.innerField = innerField;
            this.noneOption = noneOption;
            this.allowClassOverrides = allowClassOverrides;

        }

    }

}
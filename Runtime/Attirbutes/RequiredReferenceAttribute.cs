using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI.Windows.Utilities {

    public enum RequiredType {

        None,
        Error,
        Warning,

    }

    public class RequiredReferenceAttribute : PropertyAttribute {

    }

}
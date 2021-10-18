
namespace UnityEngine.UI.Windows.Essentials.Tutorial {

    [UnityEngine.UI.Windows.ComponentModuleDisplayNameAttribute("Storage/Has Key")]
    public struct HasKey : ICondition {

        public string text => $"Check if key `{this.key}` value is `{this.conditionValue}` `{this.value}`";

        public string key;
        public ConditionValue conditionValue;
        public int value;
        
        public ConditionResult IsValid(in Context context) {

            if (new ConditionValueChecker(this.conditionValue, this.value, PlayerPrefs.GetInt(this.key, 0)).GetResult() == true) {
                
                return ConditionResult.Success;
                
            }

            return ConditionResult.Failed;

        }

    }

}
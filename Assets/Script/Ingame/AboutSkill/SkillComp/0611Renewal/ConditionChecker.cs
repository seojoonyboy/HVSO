using UnityEngine;

namespace SkillModules {
    public class ConditionChecker : MonoBehaviour {
        public virtual bool IsConditionSatisfied() {
            return true;
        }
    }
}
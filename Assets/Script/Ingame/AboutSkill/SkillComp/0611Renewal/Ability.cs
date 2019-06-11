using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SkillModules;
using System.Linq;

namespace SkillModules {
    public class Ability : MonoBehaviour {
        protected bool IsAllConditionsSatisfied() {
            List<ConditionChecker> checkers = GetComponents<ConditionChecker>().ToList();
            if (checkers.Exists(x => x.IsConditionSatisfied() == false)) return false;
            return true;
        }
    }
}

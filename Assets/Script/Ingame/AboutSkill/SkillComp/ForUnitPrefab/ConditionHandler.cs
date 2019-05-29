using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using dataModules;

public class ConditionHandler : SerializedMonoBehaviour {
    public List<Set> conditions = new List<Set>();
    public Skill skillData;

    public void ChangeCondition(MonoBehaviour behaviour) {
        var set = conditions.Find(x => x.behaviour == behaviour);
        set.toggle = true;

        CheckAllConditionSatified();
    }

    public void CheckAllConditionSatified() {
        if(!conditions.Exists(x => x.toggle == false)) {
            //조건 모두 만족
            switch (skillData.effects[0].method) {
                case "gain":
                    break;
            }
        }
    }

    public class Set {
        public bool toggle;
        public MonoBehaviour behaviour;

        public Set(bool toggle, MonoBehaviour behaviour) {
            this.toggle = toggle;
            this.behaviour = behaviour;
        }
    }
}

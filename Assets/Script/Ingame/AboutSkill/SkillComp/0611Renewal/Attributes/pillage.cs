using UnityEngine;

namespace SkillModules {
    public class pillage : UnitAttribute {
        // Start is called before the first frame update
        void Start() {
            gameObject.GetComponent<PlaceMonster>().AddAttackProperty("pillage");
        }
    }
}


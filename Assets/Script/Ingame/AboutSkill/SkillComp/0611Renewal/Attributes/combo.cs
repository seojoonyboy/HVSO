using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillModules {
    public class combo : UnitAttribute {
        // Start is called before the first frame update
        void Start() {
            var placeMonsterComp = GetComponent<PlaceMonster>();
            placeMonsterComp.AddAttribute("combo");
        }
    }
}
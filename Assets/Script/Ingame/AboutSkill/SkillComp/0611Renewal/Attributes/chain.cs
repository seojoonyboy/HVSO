using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class chain : MonoBehaviour {
        // Start is called before the first frame update
        void Start() {
            var placeMonsterComp = GetComponent<PlaceMonster>();
            if(placeMonsterComp.unit.attributes == null) {
                placeMonsterComp.unit.attributes = new string[1];
            }
            int index = placeMonsterComp.unit.attributes.Length;
            placeMonsterComp.unit.attributes[index] = "chain";
        }
    }
}
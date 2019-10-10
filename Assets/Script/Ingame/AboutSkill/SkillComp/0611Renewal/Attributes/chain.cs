using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace SkillModules {
    public class chain : UnitAttribute {
        // Start is called before the first frame update
        void Start() {
            var placeMonsterComp = GetComponent<PlaceMonster>();
            var attrList = placeMonsterComp.unit.attributes.ToList();
            if (attrList == null || attrList.Count == 0) {
                attrList = new List<string>();
            }
            attrList.Add("chain");
            placeMonsterComp.unit.attributes = attrList.ToArray();
        }
    }
}
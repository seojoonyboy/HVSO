using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class Ability_gain : Ability {
        public override void EndCardPlay() {
            Debug.Log("버프를 줄 대상을 골라주세요.");

            PlaceMonster[] units = FindObjectsOfType<PlaceMonster>();
            foreach(PlaceMonster monster in units) {
                if(monster.isPlayer && monster.gameObject != gameObject) {
                    //monster.transform.Find("ClickableUI").gameObject.SetActive(true);
                }
            }
        }
    }
}

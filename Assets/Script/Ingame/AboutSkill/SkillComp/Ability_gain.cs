using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillModules {
    public class Ability_gain : Ability {
        public override void EndCardPlay(ref GameObject card) {
            base.EndCardPlay(ref card);

            foreach(var cond in skillData.activate.conditions) {
                var newComp = card.AddComponent(System.Type.GetType("SkillModules." + cond.method));

                Debug.Log(cond.method); 
            }
        }

        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                LayerMask mask = (1 << LayerMask.NameToLayer("PlayerUnit")) | (1 << LayerMask.NameToLayer("EnemyUnit"));
                RaycastHit2D[] hits = Physics2D.RaycastAll(
                    new Vector2(mousePos.x, mousePos.y),
                    Vector2.zero,
                    Mathf.Infinity,
                    mask
                );

                if (hits != null) {
                    foreach(RaycastHit2D hit in hits) {
                        Debug.Log(hit.collider.gameObject.name + "감지됨");
                    }
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using dataModules;
using SkillModules;
using UnityEngine;
using System.Linq;
using TMPro;

namespace SkillModules {
    public class Ability_hook : Ability {
        FieldUnitsObserver enemyUnitsObserver, playerUnitsObserver;
        bool isHook;
        Pos myPos;

        List<GameObject> allEnemyUnitList = new List<GameObject>();
        protected override void OnEventCallback(object parm) {
            object[] parms = (object[])parm;
            bool isPlayer = (bool)parms[0];
            GameObject summonedObj = (GameObject)parms[1];

            Debug.Log("On Event Callback");
            if (isPlayer) {
                enemyUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
                playerUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
            }
            else {
                enemyUnitsObserver = PlayMangement.instance.PlayerUnitsObserver;
                playerUnitsObserver = PlayMangement.instance.EnemyUnitsObserver;
            }

            //같은 라인에 적이 없는 경우
            if (!isEnemyExistInSameLine()) {
                StartHook();
            }
        }

        private bool isEnemyExistInSameLine() {
            myPos = playerUnitsObserver.GetMyPos(gameObject);
            //Debug.Log(myPos.row);
            var selectedList = enemyUnitsObserver.GetAllFieldUnits(myPos.row);

            if (selectedList.Count == 0) return false;
            return true;
        }

        private void StartHook() {
            allEnemyUnitList = enemyUnitsObserver.GetAllFieldUnits();
            allEnemyUnitList = allEnemyUnitList.Except(
                enemyUnitsObserver.GetAllFieldUnits(
                    playerUnitsObserver.GetMyPos(gameObject).row
                )
            ).ToList();

            if (allEnemyUnitList.Count == 0) return;

            isHook = true;
            PlayMangement.instance.OnBlockPanel("끌어올 대상을 선택하세요.");
            foreach (GameObject unit in allEnemyUnitList) {
                unit.transform.Find("ClickableUI").gameObject.SetActive(true);
            }
        }

        private void Update() {
            if (isHook & Input.GetMouseButtonDown(0)) {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                LayerMask mask = (1 << LayerMask.NameToLayer("PlayerUnit")) | (1 << LayerMask.NameToLayer("EnemyUnit"));
                RaycastHit2D[] hits = Physics2D.RaycastAll(
                    new Vector2(mousePos.x, mousePos.y),
                    Vector2.zero,
                    Mathf.Infinity,
                    mask
                );

                if (hits != null) {
                    foreach (RaycastHit2D hit in hits) {
                        GameObject selectedTarget = hit.collider.gameObject.GetComponentInParent<PlaceMonster>().gameObject;
                        foreach (GameObject unit in allEnemyUnitList) {
                            unit.transform.Find("ClickableUI").gameObject.SetActive(false);
                        }
                        selectedTarget.GetComponent<PlaceMonster>().myTarget = gameObject;
                        Debug.Log("x : " + gameObject.GetComponent<PlaceMonster>().x);
                        enemyUnitsObserver.UnitChangePosition(selectedTarget, myPos.row, 0);
                    }
                    PlayMangement.instance.OffBlockPanel();
                    Destroy(GetComponent<Ability_hook>());
                }
            }
        }
    }
}
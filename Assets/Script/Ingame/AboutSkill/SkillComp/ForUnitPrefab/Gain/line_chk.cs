using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataModules;

namespace SkillModules {
    public class line_chk : Base_gain {
        public override void Init() {
            //base.Init();

            EventDelegates[IngameEventHandler.EVENT_TYPE.END_CARD_PLAY].AddListener(() => {
                OnEndCardPlay();
            });

            SetMyActivateCondition("line_chk");
        }

        private void OnEndCardPlay() {
            try {
                if (condition.args[0] == "enemy") {
                    Pos pos = enemyUnitsObserver.GetMyRow(gameObject);
                    var units = enemyUnitsObserver.GetAllFieldUnits(pos.row);

                    if (units.Count == 0) {
                        Debug.Log("적이 같은 라인에 존재하지 않아 버프를 받습니다.");

                        int atk = 0;
                        int hp = 0;
                        int.TryParse(effectData.args[0], out atk);
                        int.TryParse(effectData.args[1], out hp);

                        gameObject.GetComponent<PlaceMonster>().RequestChangeStat(atk, hp);
                    }
                }
            }
            catch(Exception ex) {
                Debug.LogError("버프 처리 과정에 문제가 발생하였습니다.\n" + ex.Message.ToString());
            }
        }

        public override void GetMouseButtonDownEvent() {

        }
    }
}

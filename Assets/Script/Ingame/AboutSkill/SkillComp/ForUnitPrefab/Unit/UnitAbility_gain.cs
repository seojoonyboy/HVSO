using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace SkillModules {
    public class UnitAbility_gain : Ability {
        protected override void OnEventCallback(object parm) {
            if(parm == null) return;
            object[] parms = (object[])parm;
            bool isPlayer = (bool)parms[0];
            GameObject playedObj = (GameObject)parms[1];

            if((skillData.activate.scope == "playing") && (playedObj != gameObject)) return;

            //condition.method가 존재하지 않는 경우에는 그냥 True임.
            if(!IsSubConditionValid(isPlayer, playedObj)) return;

            IEnumerable<string> query = from target in skillData.targets.ToList()
                                        select target.method;
            IEnumerable<List<string>> query2 = from effect in skillData.effects.ToList()
                                         select effect.args.ToList();
            IEnumerable<List<string>> query3 = from target in skillData.targets.ToList()
                                               select target.args.ToList();

            List<string> targetMethods = query.ToList();
            List<string> effectArgs = new List<string>();
            List<string> targetArgs = new List<string>();
            foreach (List<string> args in query2) {
                effectArgs.AddRange(args);
            }
            foreach(List<string> args in query3) {
                targetArgs.AddRange(args);
            }

            int atk = 0;
            int hp = 0;

            int.TryParse(effectArgs[0], out atk);
            int.TryParse(effectArgs[1], out hp);

            //위의 IsSubConditionValid에서 부수조건(ex. played_camp_chk, played_type_chk)을 먼저 만족하는지 처리한 상태
            var name = gameObject.GetComponent<PlaceMonster>().unit.name;
            //자기 자신이 대상
            if (targetMethods.Contains("self")) {
                //상대방이 카드를 쓴 경우에 따른 버프 부여
                if (targetArgs.Contains("enemy")) {
                    //상대방이 카드를 쓸 때마다 효과
                    if (isPlayer != gameObject.GetComponent<PlaceMonster>().isPlayer) {
                        gameObject.GetComponent<PlaceMonster>().RequestChangeStat(atk, hp);
                        Debug.Log("상대방이 마법을 사용함에 따른 자신에게 버프 부여");
                    }
                }

                //내가 카드를 쓴 경우에 따른 버프 부여
                if (targetArgs.Contains("my")) {
                    //마법카드를 자신이 사용한 경우
                    if (isPlayer == gameObject.GetComponent<PlaceMonster>().isPlayer) {
                        gameObject.GetComponent<PlaceMonster>().RequestChangeStat(atk, hp);
                        Debug.Log("내가 마법을 사용함에 따른 자신에게 버프 부여");
                    }
                }

                if(targetArgs.Count == 0) {
                    gameObject.GetComponent<PlaceMonster>().RequestChangeStat(atk, hp);
                    Debug.Log("자신에게 버프 부여");
                }
            }

            //지목한 대상
            if (targetMethods.Contains("played_target")) {
                //아군
                if (targetArgs.Contains("my")) {
                    Debug.Log("아군 유닛 " + playedObj + "에게 atk : " + atk + "지목 부여");
                    Debug.Log("아군 유닛 " + playedObj + "에게 hp : " + hp + "지목 부여");

                    playedObj.GetComponent<PlaceMonster>().RequestChangeStat(atk, hp);
                }
                //적군
                else if (targetArgs.Contains("enemy")) {
                    Debug.Log("적 유닛 " + playedObj + "에게 atk : " + atk + "지목 부여");
                    Debug.Log("적 유닛 " + playedObj + "에게 hp : " + hp + "지목 부여");

                    playedObj.GetComponent<PlaceMonster>().RequestChangeStat(atk, hp);
                }
            }

            //불특정 대상
            if (targetMethods.Contains("played")) {
                //나에게(일단은 played이고 my인 경우는 자신으로 가정)
                if (targetArgs.Contains("my")) {
                    if (playedObj.GetComponent<PlaceMonster>().isPlayer && gameObject != playedObj) {
                        Debug.Log("소환된 아군유닛 atk : " + atk + "부여");
                        Debug.Log("소환된 아군유닛 hp : " + hp + "부여");

                        playedObj.GetComponent<PlaceMonster>().RequestChangeStat(atk, hp);
                    }
                }
            }
            
            //방패병은 Tmp_Buff로 따로 처리함...
        }
    }
}

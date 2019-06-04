using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SkillModules {
    public class MagicalCasting_r_return : MagicalCasting {
        public override void RequestUseMagic() {
            isRequested = true;
            GetComponent<MagicDragHandler>().AttributeUsed();
        }

        public override void UseMagic() {
            IEnumerable<string> query = from effect in skillData.effects.ToList()
                                        select effect.args[0];
            
            SocketFormat.GameState state = PlayMangement.instance.socketHandler.gameState;
            PlayMangement playMangement = PlayMangement.instance;
            FieldUnitsObserver observer = playMangement.EnemyUnitsObserver;
            List<GameObject> enemyList = observer.GetAllFieldUnits();
            List<SocketFormat.Unit> socketList = state.map.allMonster;
            foreach(GameObject mon in enemyList) {
                bool found = false;
                PlaceMonster mondata = mon.GetComponent<PlaceMonster>();
                foreach(SocketFormat.Unit unit in socketList) {
                    if(unit.itemId.CompareTo(mondata.itemId) == 0) {
                        found = true;
                        break;
                    }
                }
                if(!found) {
                    Pos pos = observer.GetMyPos(mon);
                    observer.UnitRemoved(pos.row, pos.col);
                    Destroy(mon);
                    GameObject enemyCard = Instantiate(playMangement.player.isHuman ? playMangement.enemyPlayer.back : playMangement.player.back);
                    enemyCard.transform.SetParent(playMangement.enemyPlayer.playerUI.transform.Find("CardSlot").GetChild(PlayMangement.instance.CountEnemyCard()));
                    enemyCard.transform.localScale = new Vector3(1, 1, 1);
                    enemyCard.transform.localPosition = new Vector3(0, 0, 0);
                    enemyCard.SetActive(true);
                    break;
                }
            }
        }
    }
}
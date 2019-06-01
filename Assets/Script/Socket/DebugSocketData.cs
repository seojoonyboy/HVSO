using Newtonsoft.Json;
using SocketFormat;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocketFormat {
    public class DebugSocketData : MonoBehaviour {
        public static void ShowHandCard(Card[] cards) {
            Debug.Log("적의 핸드 리스트 : ");
            foreach(Card card in cards) {
                Debug.Log(string.Format("이름 : {0}, 가격 : {1}, 종류 : {2}, cardId : {3}, itemId : {4}, 공격력 : {5}, 체력 : {6}",
                                card.name, card.cost, card.type, card.id, card.itemId, card.attack, card.hp));
            }
        }
        
        public static void ShowBattleData(GameState state, int line, bool isBattle) {
            string mapData = JsonConvert.SerializeObject(state.map.lines[line]);
            Hero human = state.players.human.hero;
            Hero orc = state.players.orc.hero;
            Debug.Log(isBattle ? "======= 싸운 후 State =======" : "======= 에너지 체크 후 State =======");
            Debug.Log(string.Format("{0}번째줄 맵 : {1}", line, mapData));
            Debug.Log(string.Format("휴먼 체력 : {0}, 방어갯수 : {1}, 방어게이지 : {2}", human.currentHp, human.shildCount, human.shildGauge));
            Debug.Log(string.Format("오크 체력 : {0}, 방어갯수 : {1}, 방어게이지 : {2}", orc.currentHp, orc.shildCount, orc.shildGauge));
            Debug.Log("=======================================");
        }

        public static void SummonCardData(PlayHistory history) {
            string historyData = JsonConvert.SerializeObject(history);
            Debug.Log(string.Format("사용 된 카드 : {0}", historyData));
        }

        public static void CheckBattleSynchronization(GameState state, int line) {
            //GameObject[] playerUnits = PlayMangement.instance.PlayerUnitsObserver.units[line];
            //GameObject[] enemyUnits = PlayMangement.instance.EnemyUnitsObserver.units[line];
            foreach(Unit unit in state.map.lines[line].human) {

            }
            foreach(Unit unit in state.map.lines[line].orc) {

            }
        }
    }  
}
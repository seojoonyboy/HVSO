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
            string heroData = JsonConvert.SerializeObject(state.players.human.hero);
            string heroData2 = JsonConvert.SerializeObject(state.players.orc.hero);
            Debug.Log(isBattle ? "======= 싸운 후 State =======" : "======= 에너지 체크 후 State =======");
            Debug.Log(string.Format("{0}번째줄 맵 : {1}", line, mapData));
            Debug.Log(string.Format("{0}번째줄 휴먼 플레이어 상태 : {1}", line, heroData));
            Debug.Log(string.Format("{0}번째줄 오크 플레이어 상태 : {1}", line, heroData2));
            Debug.Log("=======================================");
        }

        public static void SummonCardData(PlayHistory history) {
            string historyData = JsonConvert.SerializeObject(history);
            Debug.Log(string.Format("사용 된 카드 : {0}", historyData));
        }
    }  
}
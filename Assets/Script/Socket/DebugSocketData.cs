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

        public static void CheckBattleSynchronization(GameState state) {
            PlayMangement manager = PlayMangement.instance;
            FieldUnitsObserver humanUnits, orcUnits;
            humanUnits = manager.player.isHuman ? manager.PlayerUnitsObserver : manager.EnemyUnitsObserver;
            orcUnits = (!manager.player.isHuman) ? manager.PlayerUnitsObserver : manager.EnemyUnitsObserver;
            for(int i = 0; i < state.map.lines.Length; i++) {
                CheckUnits(state.map.lines[i].human, humanUnits.GetAllFieldUnits(i));
                CheckUnits(state.map.lines[i].orc, orcUnits.GetAllFieldUnits(i));
            }
            List<GameObject> list = humanUnits.GetAllFieldUnits();
            list.AddRange(orcUnits.GetAllFieldUnits());
            CheckUnitsReverse(list, state.map);
            
        }

        public static void CheckHeros(GameState state) {
            PlayMangement manager = PlayMangement.instance;
            bool isPlayerHuman = manager.player.isHuman;
            if(state.players.myPlayer(isPlayerHuman).hero.currentHp != manager.player.HP.Value) {
                manager.player.HP.Value = state.players.myPlayer(isPlayerHuman).hero.currentHp;
                FoundMisMatchData("플레이어 영웅", "hp");
            }
            if(state.players.myPlayer(!isPlayerHuman).hero.currentHp != manager.enemyPlayer.HP.Value)  {
                manager.enemyPlayer.HP.Value = state.players.myPlayer(!isPlayerHuman).hero.currentHp;
                FoundMisMatchData("컴퓨터 영웅", "hp");
            }
        }

        public static void CheckUnits(Unit[] units, List<GameObject> mons) {
            foreach(Unit unit in units) {
                bool foundUnit = false;
                foreach(GameObject mon in mons) {
                    PlaceMonster mondata = mon.GetComponent<PlaceMonster>();
                    IngameClass.Unit monUnit = mondata.unit;
                    if(mondata.itemId.CompareTo(unit.itemId) == 0) {
                        if(CompareUnit(unit, monUnit))
                            mondata.UpdateStat();
                        foundUnit = true;
                        break;
                    }
                }
                if(!foundUnit) {
                    FoundMisMatchData(unit.name, "not_found");
                }
            }
        }

        public static void CheckUnitsReverse(List<GameObject> mons, Map map) {
            foreach(GameObject mon in mons) {
                bool found = false;
                PlaceMonster mondata = mon.GetComponent<PlaceMonster>();
                foreach(Unit unit in map.allMonster) {
                    if(unit.itemId.CompareTo(mondata.itemId) == 0) {
                        found = true;
                        break;
                    }
                }
                if(!found) FoundMisMatchData(mondata.unit.name, "not_found_reverse");
            }
        }

        public static bool CompareUnit(Unit socketData, IngameClass.Unit monData) {
            bool isDiff = false;
            if(socketData.attack != monData.attack) {
                isDiff = true;
                FoundMisMatchData(monData.name, "attack");
                monData.attack = socketData.attack.Value;
            }
            if(socketData.cost != monData.cost) {
                isDiff = true;
                FoundMisMatchData(monData.name, "cost");
                monData.cost = socketData.cost;
            }
            if(socketData.currentHp != monData.currentHP) {
                isDiff = true;
                FoundMisMatchData(monData.name, "hp");
                monData.currentHP = socketData.currentHp;
            }
            return isDiff;
        }

        public static void FoundMisMatchData(string name, string status) {
            string log = string.Empty;
            switch(status) {
                case "not_found_reverse" : log = "유닛이 클라이언트에만 존재합니다.";
                break;
                case "not_found" : log = "유닛이 서버에만 존재합니다.";
                break;
                case "attack" : log = "공격력이 다릅니다 조정 들어갑니다.";
                break;
                case "cost" : log = "가격이 다릅니다 조정 들어갑니다.";
                break;
                case "hp" : log = "체력이 다릅니다 조정 들어갑니다.";
                break;
                default :
                log = "이 문제는 개발자가 코딩을 잘못한겁니다.";
                break;
            }
            Debug.LogWarning(string.Format("{0} : {1}", name, log));
        }
    }  
}
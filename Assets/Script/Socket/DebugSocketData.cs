using Newtonsoft.Json;
using SocketFormat;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocketFormat {
    public class DebugSocketData : MonoBehaviour {
        public static void ShowHandCard(Card[] cards) {
            Logger.Log("적의 핸드 리스트 : ");
            foreach(Card card in cards) {
               Logger.Log(string.Format("이름 : {0}, 가격 : {1}, 종류 : {2}, cardId : {3}, itemId : {4}, 공격력 : {5}, 체력 : {6}",
                               card.name, card.cost, card.type, card.id, card.itemId, card.attack, card.hp));
            }
        }
        
        public static void ShowBattleData(GameState state, int line, bool isBattle) {
            string mapData = JsonConvert.SerializeObject(state.map.lines[line]);
            dataModules.Hero human = state.players.human.hero;
            dataModules.Hero orc = state.players.orc.hero;
            Logger.Log(isBattle ? "======= 싸운 후 State =======" : "======= 에너지 체크 후 State =======");
            Logger.Log(string.Format("{0}번째줄 맵 : {1}", line, mapData));

            GameObject[,] mySlots = null;
            GameObject[,] enemySlots = null;

            PlayMangement playMangement = PlayMangement.instance;
            bool isHuman = PlayMangement.instance.player.isHuman;

            if (isHuman) {
                mySlots = playMangement.UnitsObserver.humanUnits;
                enemySlots = playMangement.UnitsObserver.orcUnits;
            }
            else {
                mySlots = playMangement.UnitsObserver.orcUnits;
                enemySlots = playMangement.UnitsObserver.humanUnits;
            }

            Logger.Log(string.Format("클라이언트 유닛 뒤: {0}, {1}", mySlots[line,0], enemySlots[line,0]));
            Logger.Log(string.Format("클라이언트 유닛 앞: {0}, {1}", mySlots[line,1], enemySlots[line,1]));
            Logger.Log(string.Format("휴먼 체력 : {0}, 방어갯수 : {1}, 방어게이지 : {2}", human.currentHp, human.shieldCount, human.shieldGauge));
            Logger.Log(string.Format("오크 체력 : {0}, 방어갯수 : {1}, 방어게이지 : {2}", orc.currentHp, orc.shieldCount, orc.shieldGauge));
            Logger.Log("=======================================");
        }

        public static void SummonCardData(PlayHistory history) {
            string historyData = JsonConvert.SerializeObject(history);
            Logger.Log(string.Format("사용 된 카드 : {0}", historyData));
        }

        public static void CheckBattleSynchronization(GameState state) {
            PlayMangement manager = PlayMangement.instance;
            bool isHuman = manager.player.isHuman;
            var observer = manager.UnitsObserver;
            for(int i = 0; i < state.map.lines.Length; i++) {
                CheckUnits(state.map.lines[i].human, observer.GetAllFieldUnits(i, true));
                CheckUnits(state.map.lines[i].orc, observer.GetAllFieldUnits(i, false));
            }
            List<GameObject> list = observer.GetAllFieldUnits(isHuman);
            list.AddRange(observer.GetAllFieldUnits(!isHuman));
            CheckUnitsReverse(list, state.map);
            CheckHeros(state);
        }

        public static void CheckHeros(GameState state) {
            PlayMangement manager = PlayMangement.instance;
            bool isPlayerHuman = manager.player.isHuman;
            if(state.players.myPlayer(isPlayerHuman).hero.currentHp != manager.player.HP.Value) {
                FoundMisMatchData("플레이어 영웅", "hp", manager.player.HP.Value);
                manager.player.HP.Value = state.players.myPlayer(isPlayerHuman).hero.currentHp;
                manager.player.shieldStack.Value = state.players.myPlayer(isPlayerHuman).hero.shieldGauge;
            }
            if(state.players.myPlayer(!isPlayerHuman).hero.currentHp != manager.enemyPlayer.HP.Value)  {
                FoundMisMatchData("컴퓨터 영웅", "hp", manager.enemyPlayer.HP.Value);
                manager.enemyPlayer.HP.Value = state.players.myPlayer(!isPlayerHuman).hero.currentHp;
                manager.enemyPlayer.shieldStack.Value = state.players.myPlayer(!isPlayerHuman).hero.shieldGauge;
                
            }
        }

        public static void CheckUnits(Unit[] units, List<GameObject> mons) {
            foreach(Unit unit in units) {
                bool foundUnit = false;
                foreach(GameObject mon in mons) {
                    PlaceMonster mondata = mon.GetComponent<PlaceMonster>();
                    dataModules.Unit monUnit = mondata.unit;
                    if(mondata.itemId.CompareTo(unit.itemId) == 0) {
                        if(CompareUnit(unit, monUnit))
                            mondata.UpdateStat();
                        foundUnit = true;
                        break;
                    }
                }
                if(!foundUnit) {
                    summonMonster(unit.pos.col, units, unit.origin.camp.CompareTo("human")==0);
                    FoundMisMatchData(unit.origin.name, "not_found");
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
                if(!found) {
                    mondata.UnitDead();
                    FoundMisMatchData(mondata.unit.name, "not_found_reverse");
                }
            }
        }

        public static bool CompareUnit(Unit socketData, dataModules.Unit monData) {
            bool isDiff = false;
            if(socketData.attack != monData.attack) {
                isDiff = true;
                FoundMisMatchData(monData.name, "attack", (int)monData.attack);
                monData.attack = socketData.attack;
            }
            if(socketData.origin.cost != monData.cost) {
                isDiff = true;
                FoundMisMatchData(monData.name, "cost", monData.cost);
                monData.cost = socketData.origin.cost;
            }
            if(socketData.currentHp != monData.currentHp) {
                isDiff = true;
                FoundMisMatchData(monData.name, "hp", monData.currentHp);
                monData.currentHp = socketData.currentHp;
            }
            return isDiff;
        }

        public static void FoundMisMatchData(string name, string status, int value = 0) {
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
            Logger.LogError(string.Format("{0} : {1}, 클라 수치 : {2}", name, log,value));
        }

        public static void CheckMapPosition(GameState state) {
            PlayMangement playMangement = PlayMangement.instance;
            var observer = playMangement.UnitsObserver;

            List<GameObject> clientHumanLine = observer.GetAllFieldUnits(isHuman: true);
            List<GameObject> clientOrcLine = observer.GetAllFieldUnits(isHuman: false);
            Line[] lines = state.map.lines;

            for(int i = 0; i < lines.Length; i++) {
                CheckMonsterPosition(lines[i].orc, i, false);
                CheckMonsterPosition(lines[i].human, i, true);
            }
        }

        public static void CheckMonsterPosition(Unit[] units, int line, bool isHuman) {
            var observer = PlayMangement.instance.UnitsObserver;
            if (units.Length == 0) return;
            List<GameObject> mons = observer.GetAllFieldUnits(isHuman);
            foreach(Unit unit in units) {
                GameObject mon = mons.Find(x => x.GetComponent<PlaceMonster>().itemId.CompareTo(unit.itemId) == 0);
                if(mon == null) {
                    Logger.LogWarning("클라이언트에서 해당 유닛이 없는 버그가 발생했습니다 : " + unit.origin.name);
                    return;
                }
                PlaceMonster monData = mon.GetComponent<PlaceMonster>();
                FieldUnitsObserver.Pos pos = observer.GetMyPos(mon);
                if(pos.col == line) continue;

                observer.UnitChangePosition(mon, new FieldUnitsObserver.Pos(line, 0), isHuman);
            }
        }

        public static void StartCheckMonster(SocketFormat.GameState state) {
            for(int i = 0; i < state.map.lines.Length; i++) {
                if(state.map.lines[i].human.Length > 0) summonMonster(i, state.map.lines[i].human, true);
                if(state.map.lines[i].orc.Length > 0) summonMonster(i, state.map.lines[i].orc, false);
            }
        }

        private static void summonMonster(int line, SocketFormat.Unit[] units, bool isHuman) {
            PlayMangement playMangement = PlayMangement.instance;
            bool isPlayer = playMangement.player.isHuman == isHuman;
            
            for (int i = 0; i < units.Length; i++) {
                GameObject field_unit = playMangement.UnitsObserver.GetUnitToItemID(units[i].itemId);
                if(field_unit != null) continue;
                var unit = playMangement.SummonUnit(isPlayer, units[i].origin.id, line, i, units[i].itemId, -1, null, true);
                unit.GetComponent<PlaceMonster>().UpdateGranted();
            }
        }
    }  
}
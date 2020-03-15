using Newtonsoft.Json;
using dataModules;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace SocketFormat {
    public class GameState {
        public string gameState;
        public TurnState turn;
        public string gameId;
        public Map map;
        public int turnCount;
        public PlayHistory[] playHistory;
        public string gameType;
        public object gameResult;
        public int messageNumber;
        public string[] battleMessageHistory;

        public Players players;



        public PlayHistory lastUse { get { return playHistory.Length == 0 ? null : playHistory[0]; }}
        public bool SearchUseItem(string itemId) {
            return playHistory.ToList().Exists(x => x.cardItem.itemId.CompareTo(itemId) == 0);
        }
    }

    [Serializable]
    public class Map {
        public string type;
        public Line[] lines;
        public List<Unit> allMonster {
            get {
                List<Unit> list = new List<Unit>();
                for(int i = 0; i < lines.Length; i++) {
                    for(int j = 0; j < lines[i].human.Length; j++) {
                        list.Add(lines[i].human[j]);
                    }
                    for(int j = 0; j < lines[i].orc.Length; j++) {
                        list.Add(lines[i].orc[j]);
                    }
                }
                return list;
            }
        }
    }    

    [Serializable]
    public class Line {
        public string terrain;
        public int lineNumber;
        public Unit[] orc;
        public Unit[] human;
    }

    [Serializable]
    public class PlayHistory {
        public Card cardItem;
        public Target[] targets;
    }

    [Serializable]
    public class Target {
        public string method;
        public string[] filter;
        public string[] args;
    }

    [Serializable]
    public class Players {
        public Player orc;
        public Player human;

        public Player myPlayer(bool race) {
            return race ? human : orc;
        }
        public Player enemyPlayer(bool race) {
            return race ? human : orc;
        } 
    }

    [Serializable]
    public class Player {
        public string uuid;
        public AccountManager.UserInfo user;
        public string state;
        public string camp;
        public int resource;
        public int bonusResource;
        public Deck deck;
        public Hero hero;

        public Card[] FirstCards { 
            get {
                List<Card> ids = new List<Card>();
                foreach(Card card in deck.handCards) ids.Add(card);
                return ids.ToArray();
            }
        }

        public Card newCard { 
            get { return deck.handCards[deck.handCards.Length-1]; }
        }
    }

    [Serializable]
    public class User {
        public string nickName;
    }

    [Serializable]
    public class Deck {
        public string deckType;
        public Card[] heroCards;
        public Card[] handCards;
    }

    [Serializable]
    public class Card : CardInventory {
        #pragma warning disable CS0108
        public string id;
        #pragma warning restore CS0108
        public string itemId;
        public bool unownable;
    }

    [Serializable]
    public class Unit {
        public int currentHp;
        public int maxHp;
        public int attack;
        public string itemId;
        public int attackCount;
        public Card origin;
        public int damaged;
        public Granted[] granted;
        public FieldUnitsObserver.Pos pos { get{ return GetPos();} }

        private FieldUnitsObserver.Pos GetPos() {
            FieldUnitsObserver.Pos pos = new FieldUnitsObserver.Pos();
            Line[] lines = PlayMangement.instance.socketHandler.gameState.map.lines;
            bool isOrc = origin.camp.CompareTo("orc")==0;
            PropertyInfo info = lines[0].GetType().GetProperty(origin.camp);
            for(int i = 0; i < lines.Length; i++) {
                Unit[] units = isOrc ? lines[i].orc : lines[i].human;
                for(int j = 0; j < units.Length; j++) {
                    if(units[j].itemId.CompareTo(itemId) == 0) {
                        pos.col = i;
                        pos.row = j;
                        return pos;
                    }
                }
            }
            pos.col = -1;
            pos.row = -1;
            return pos;
        }
    }
    
    [Serializable]
    public class ShieldCharge {
        public int shieldCount;
        public string camp;
    }

    public class AttackArgs {
        public string attacker;
        public string[] affected;
    }

    public class MagicArgs {
        public string itemId;
        public Target[] targets;
        public object skillInfo;
    }

    public class AttackInfo {
        public string attacker;
        public string[] affected;
    }


    public class TurnState {
        public string turnName;
        public string turnState;
    }


    public class TimeState {
        public string begin;
    }

}
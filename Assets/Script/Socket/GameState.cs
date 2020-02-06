using Newtonsoft.Json;
using dataModules;
using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;

namespace SocketFormat {
    public class GameState {
        public string state;
        public string gameId;
        public Map map;
        public Players players;
        public int turnCount;
        public PlayHistory[] playHistory;

        public PlayHistory lastUse { get { return playHistory.Length == 0 ? null : playHistory[0]; }}
        public bool SearchUseItem(int itemId) {
            return playHistory.ToList().Exists(x => x.cardItem.itemId == itemId);
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
        public User user;
        public string state;
        public string camp;
        public int resource;
        public Deck deck;
        public Hero hero;
        public bool shieldActivate;

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
        public int itemId;
    }

    [Serializable]
    public class Unit : Card {
        public int currentHp;
        public FieldUnitsObserver.Pos pos { get{ return GetPos();} }

        private FieldUnitsObserver.Pos GetPos() {
            FieldUnitsObserver.Pos pos = new FieldUnitsObserver.Pos();
            Line[] lines = PlayMangement.instance.socketHandler.gameState.map.lines;
            bool isOrc = camp.CompareTo("orc")==0;
            PropertyInfo info = lines[0].GetType().GetProperty(camp);
            for(int i = 0; i < lines.Length; i++) {
                Unit[] units = isOrc ? lines[i].orc : lines[i].human;
                for(int j = 0; j < units.Length; j++) {
                    if(units[j].itemId == itemId) {
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
    public class Hero {
        public int tier;
        public string[] heroClasses;
        public string id;
        public string camp;
        public string name;
        public int maxHp;
        public int currentHp;
        public int shieldCount;
        public int shieldGauge;
    }

    [Serializable]
    public class ShieldCharge {
        public int shieldCount;
        public string camp;
    }
}
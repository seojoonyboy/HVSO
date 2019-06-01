using Newtonsoft.Json;
using dataModules;
using System.Collections.Generic;

namespace SocketFormat {
    public class GameState {
        public string state;
        public string gameId;
        public Map map;
        public Players players;
        public int turnCount;
        public PlayHistory[] playHistory;

        public PlayHistory lastUse { get { return playHistory[0]; }}
    }

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

    public class Line {
        public string terrain;
        public Unit[] orc;
        public Unit[] human;
    }

    public class PlayHistory {
        public Card cardItem;
        public Target target;
    }

    public class Target {
        public string method;
        public string[] args;
    }

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

    public class Player {
        public string uuid;
        public User user;
        public string state;
        public string camp;
        public int resource;
        public Deck deck;
        public Hero hero;
        public bool shildActivate;

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

    public class User {
        public string nickName;
    }

    public class Deck {
        public string deckType;
        public Card[] heroCards;
        public Card[] handCards;
    }

    public class Card : CardInventory {
        #pragma warning disable CS0108
        public string id;
        #pragma warning restore CS0108
        public int itemId;
    }

    public class Unit : Card {
        public int currentHp;
    }

    public class Hero {
        public string[] heroClasses;
        public string id;
        public string camp;
        public string name;
        public int maxHp;
        public int currentHp;
        public int shildCount;
        public int shildGauge;
    }
}
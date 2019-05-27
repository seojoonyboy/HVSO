using Newtonsoft.Json;
using dataModules;
using System.Collections.Generic;

namespace SocketFormat {
    public class GameState {
        public string state;
        public string gameId;
        public Map map;
        public Players players;
    }

    public class Map {
        public string type;
        public Line[] lines;
    }

    public class Line {
        public string terrain;
        public Card[] orc;
        public Card[] human;
    }

    public class Players {
        public Player orc;
        public Player human;
    }

    public class Player {
        public string uuid;
        public User user;
        public string state;
        public string camp;
        public int resource;
        public Deck deck;
        public Hero hero;

        public string[] CardsId { 
            get {
                List<string> ids = new List<string>();
                foreach(Card card in deck.handCards) ids.Add(card.id);
                return ids.ToArray();
            }
        }

        public string newCardId { 
            get { return deck.handCards[deck.handCards.Length-1].id; }
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
        public string id;
        public int itemId;
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
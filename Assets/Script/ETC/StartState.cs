namespace IngameEditor {
    public class StartState {
        public Map map;
        public Players players;
    }

    public class Map {
        public string type;
        public Line[] lines;
    }

    public class Players {
        public Player human;
        public Player orc;
    }

    public class Player {
        public Hero hero;
        public Deck deck;
        public int resource;
    }

    public class Hero {
        public int hp;
        public int shieldGage;
    }

    public class Line {
        public string terrain;
        public string[] orc;
        public string[] human;
    }

    public class Deck {
        public string deckType;
        public string[] heroCards;
        public string[] handCards;
    }
}


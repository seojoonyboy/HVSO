namespace IngameEditor {
    public class StartState {
        public Map map;
        public Players players;
    }

    public class Map {
        public string type;
        public SocketFormat.Line[] lines;
    }

    public class Players {
        public Player human;
        public Player orc;
    }

    public class Player {
        public HeroInfo resource;
        public SocketFormat.Deck deck;
        public int mana;
    }

    public class HeroInfo {
        public int hp;
        public int shieldGage;
    }
}


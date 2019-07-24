using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IngameEditor {
    public class StartState {
        public Map map;
        public Players players;

        public StartState() {
            map = new Map();
            map.lines = new Line[5];
            for(int i = 0; i < 5; i++) {
                map.lines[i] = new Line();
                map.lines[i].human = new string[2];
                map.lines[i].orc = new string[2];
                map.lines[i].terrain = "normal";
            }
            players = new Players();
            PlayerSet(ref players.human);
            PlayerSet(ref players.orc);
        }

        private void PlayerSet(ref Player player) {
            player = new Player();
            player.resource = 1;
            player.deck = new Deck();
            player.deck.handCards = new string[10];
            player.deck.heroCards = new string[4];
            player.hero = new Hero();
            player.hero.currentHp = 20;
            player.hero.shieldGuage = 0;
            player.hero.shieldCount = 3;
        }

        public JToken toJson() {
            JToken jtoken = JToken.FromObject(this);
            jtoken = JsonHelper.RemoveEmptyChildren(jtoken);
            return jtoken;
        }
    }

    public class Map {
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
        public int currentHp;
        public int shieldCount;
        public int shieldGuage;
    }

    public class Line {
        public string terrain;
        public string[] orc;
        public string[] human;
    }

    public class Deck {
        public string[] heroCards;
        public string[] handCards;
    }


    public static class JsonHelper {
        public static JToken RemoveEmptyChildren(JToken token) {
            if (token.Type == JTokenType.Object) {
                JObject copy = new JObject();
                foreach (JProperty prop in token.Children<JProperty>()) {
                    JToken child = prop.Value;
                    if (child.HasValues) {
                        child = RemoveEmptyChildren(child);
                    }
                    if (!IsEmpty(child)) {
                        copy.Add(prop.Name, child);
                    }
                }
                return copy;
            }
            else if (token.Type == JTokenType.Array) {
                JArray copy = new JArray();
                foreach (JToken item in token.Children()) {
                    JToken child = item;
                    if (child.HasValues) {
                        child = RemoveEmptyChildren(child);
                    }
                    if (!IsEmpty(child)) {
                        copy.Add(child);
                    }
                }
                return copy;
            }
            return token;
        }

        public static bool IsEmpty(JToken token){
            return (token.Type == JTokenType.Null);
        }
    }
}


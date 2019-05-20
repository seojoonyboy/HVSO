using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace dataModules {
    //TODO : HTTP Pro의 LitJson과 통합
    public class JsonReader {
        public static T Read<T>(string data) {
            return (T)Convert.ChangeType(JsonConvert.DeserializeObject<T>(data), typeof(T));
        }
    }

    public class CardInventory {
        public int id;
        public string rarelity;
        public string camp;
        public string type;
        public string name;
        public int cost;
        public int? attack;
        public int? hp;
        public bool isHeroCard;
        public int cardCount;
        public string cardId;
        public string createdAt;
        public string updatedAt;
        public string[] cardCategories;
        public string[] cardClasses;
    }

    public class HeroInventory {
        public string[] heroClasses;
        public int id;
        public string camp;
        public string name;
        public string heroId;
        public string createAt;
        public string updateAt;
        public CardInventory[] heroCards;
    }

    public class HumanDecks {
        public List<Hero> heros;
        public List<Deck> basicDecks;
    }

    public class OrcDecks {
        public List<Hero> heros;
        public List<Deck> basicDecks;
    }

    public class Hero {
        public string[] heroClassese;
        public string id;
        public string camp;
        public string name;
        public List<HeroCard> heroCards;
    }

    public class Deck {
        public bool userHas;
        public int cardTotalCount;
        public string id;
        public string name;
        public string camp;
        public string flavorText;
        public List<Item> items;
    }

    public class HeroCard : BaseCard { }

    public class Item : BaseCard {
        public string basicDeckId;
        public int cardHasCount;
        public int cardCount;
    }

    public class BaseCard {
        public string[] cardCategories;
        public string[] cardClasses;
        public int id;
        public string rarelity;
        public string camp;
        public string type;
        public string name;
        public int cost;
        public int? attack;
        public int? hp;
        public bool isHeroCard;
        public string cardId;
    }
}
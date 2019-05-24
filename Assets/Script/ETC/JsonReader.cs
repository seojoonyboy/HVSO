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

    public class CardInventory : BaseCard {
        public Skill[] skills;
    }

    public class HeroInventory {
        public string[] heroClasses;
        public int id;
        public string camp;
        public string name;
        public string heroId;
        public string createAt;
        public string updateAt;
        public HeroCard[] heroCards;
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

    public class Skill {
        public Activate activate;
        public Deactivate deactivate;
        public Target[] targets;
        public Effect[] effects;
    }

    public class Activate : BaseActivate {
        public string scope;
    }

    public class Deactivate : BaseActivate { }

    public class BaseActivate {
        public string trigger;
        public Condition[] conditions;
    }

    public class Target {
        public string[] args;
        public string method;
    }

    public class Effect {
        public string[] args;
        public string method;
    }

    public class Condition {
        public string[] args;
        public int id;
        public int deactiveId;
        public string method;
    }
}
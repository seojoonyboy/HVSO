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
        public List<Deck> customDecks;
    }

    public class OrcDecks {
        public List<Hero> heros;
        public List<Deck> basicDecks;
        public List<Deck> customDecks;
    }

    public class Hero {
        public string[] _heroClassese;
        public string id;
        public string camp;
        public string name;
        public List<HeroCard> _heroCards;
    }

    public class Deck {
        //public bool userHas;
        public int cardTotalCount;
        public string id;
        public string name;
        public string flavorText;
        public Hero _hero;
        public List<Item> items;
    }

    public class HeroCard : BaseCard {
        public Skill[] skills;
    }

    public class Item : BaseCard {
        public int cardCount;
    }
    
    public class BaseCard {
        public string[] attackTypes;
        public string[] attributes;
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
        public string attackRange;
        public bool isHeroCard;
        public string cardId;
    }

    public class Skill {
        //public int id;
        public string cardId;
        public string trigger;
        public string scope;
        public string desc;
        public Condition[] conditions;
        public Target target;
        public Effect effect;
    }

    public class Target {
        public string[] args;
        public int skillId;
        public string method;
    }

    public class Effect {
        public string[] args;
        public int skillId;
        public string method;
    }

    public class Condition {
        public string[] args;
        //public int id;
        public int skillId;
        public string method;
    }
}
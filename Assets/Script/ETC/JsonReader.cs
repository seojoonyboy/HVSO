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

    public class MyCardsInfo {
        public CardInventory[] cardInventories;
        public HeroInventory[] heroInventories;
    }

    public class CardInventory : BaseCard {
        public Skill[] skills;
        public int cardCount;
        public string flavorText;
    }

    /// <summary>
    /// 모든 카드 요청시 형식
    /// </summary>
    [Serializable]
    public class CollectionCard {
        public string[] attackTypes;
        public string[] attributes;
        public string[] cardCategories;
        public string[] cardClasses;
        public string id;
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
        public Skill[] skills;
        public string flavorText;
    }

    public class HeroInventory {
        public string[] heroClasses;
        public string id;
        public string camp;
        public string name;
        public string heroId;
        public bool userHas;
        public HeroCard[] heroCards;
    }

    public class Decks {
        public List<Deck> orc;
        public List<Deck> human;
    }

    public class Hero {
        public string[] heroClasses;
        public string id;
        public string camp;
        public string name;
        public List<HeroCard> heroCards;
    }

    public class Templates : HeroInventory {
        public List<Deck> templates;
    }

    public class Deck {
        //public bool userHas;
        public int totalCardCount;
        public string id;
        public string name;
        public string flavorText;
        public string camp;
        public string heroId;
        public string bannerImage;
        public List<Item> items;
        public bool deckValidate;
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
        public string[] cardClasses;
        public string[] cardCategories;
        public string id;
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

    public class Token {
        public string token;
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

    public class BoxInfo {
        public List<RewardClass> rewardList;
    }
}
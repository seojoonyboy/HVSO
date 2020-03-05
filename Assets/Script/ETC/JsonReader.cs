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
        public Attr[] attributes;
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
        public bool indestructible;
        public bool unownable;
    }

    public class Attr {
        public string name;
        public int? value;
    };

    public class HeroInventory {
        public string[] heroClasses;
        public string[] traitText;
        public string id;
        public string camp;
        public string name;
        public string heroId;
        public string flavorText;
        public bool userHas;
        public bool unownable;
        public int piece;
        public int tier;
        public HeroLevel next_level;
        public HeroCard[] heroCards;
    }

    public class HeroLevel {
        public int piece;
        public int crystal;
    }

    public class Decks {
        public List<Deck> orc;
        public List<Deck> human;
    }

    public class Hero : HeroInventory {
        public int hp;
        public int userId;
        public string createdAt;
        public string updatedAt;
        public int shieldCount;
        public int shieldGauge;
        public Attr[] attributes;
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
        public Attr[] attributes;
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
        public bool indestructible;
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

    public class Shop {
        public string _price;
        public string id;
        public string name;
        public string desc;
        public string category;
        public string sellType;
        public ShopPrices prices;
        public bool isRealMoney;
        public RewardItem[] items;
        public int? expiresIn;
        public bool enabled;
        public string valuablity;
        public string inAppId;
        public string updatedAt;
        public string deletedAt;
    }

    public class RewardItem {
        public string kind;
        public string amount;
    }

    public class ShopPrices {
        public int GOLD;
        public int KRW;
        public float USD;
    }

    [System.Serializable]
    public class Unit {
        public int currentHp;
        public string[] attackTypes;
        public Attr[] attributes;
        public string[] cardClasses;
        public string[] cardCategories;
        public string rarelity;
        public string camp;
        public string type;
        public string name;
        public int cost;
        public int attack;
        public int hp;
        public string attackRange;
        public string flavorText;
        public string[] skills;
        public string itemId;
        public string cardId;
        public int attackCount;
        public bool ishuman;
        public int originalAttack;
    }

    public class Mail {
        public int id;
        public int userId;
        public string sender;
        public string context;
        public string expiredAt;
        public bool isRead;
        public bool itemReceived;
        public string createdAt;
        public string updatedAt;
        public MailItem[] items;
    }
    
    public class MailItem {
        public int id;
        public int postId;
        public string kind;
        public int? amount;
    }

    public class MailReward {
        public string kind;
        public string amount;
        public MailCard[] cards;
        public List<List<RewardClass>> boxes;
    }

    public class MailCard {
        public string cardId;
        public int crystal;
    }

    public class AdReward {
        public string kind;
        public int amount;
        public bool claimed;
    }

    public class AdRewardRequestResult {
        public bool claimComplete;
        public AdRewardItem[] items;
    }

    public class AdRewardItem {
        public string kind;
        public int amount;
        public List<RewardClass>[] boxes;
    }

    public class AttendanceResult {
        public AttendanceType attendance;
        public AttendanceReward tables;
        public bool attendChk;
    }

    public class AttendanceType {
        public int monthly;
        public int welcome;
        public int comeback;
    }

    public class AttendanceReward {
        public AttendanceItem[] monthly;
        public AttendanceItem[] welcome;
        public AttendanceItem[] comeback;
    }

    public class AttendanceItem {
        public RewardItem reward;
        public int id;
        public string type;
        public int day;
        public bool attend;
    }

    public class BuyBoxInfo {
        public bool result;
        public AdRewardItem[] items;
    }
}
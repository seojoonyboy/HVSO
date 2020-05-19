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
        public Skill skills;
        public Target[] targets;
        public int cardCount;
        public string flavorText;

        public void PasteData(CollectionCard cardData) {
            this.attributes = cardData.attributes;
            this.attack = cardData.attack;
            this.attackRange = cardData.attackRange;
            this.camp = cardData.camp;
            this.cardCategories = cardData.cardCategories;
            this.cardClasses = cardData.cardClasses;
            this.type = cardData.type;
            this.name = cardData.name;
            this.rarelity = cardData.rarelity;
            this.hp = cardData.hp;
            this.cost = cardData.cost;
            this.isHeroCard = cardData.isHeroCard;
            this.flavorText = cardData.flavorText;
            this.indestructible = cardData.indestructible;
            this.skills = cardData.skills;
            this.targets = cardData.targets;
        }
    }
    

    /// <summary>
    /// 모든 카드 요청시 형식
    /// </summary>
    [Serializable]
    public class CollectionCard {
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
        public Skill skills;
        public Target[] targets;
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
        public int currentHp;
        public int damaged;
        public int hp;
        public int userId;
        public string createdAt;
        public string updatedAt;
        public int shieldCount;
        public int shieldGauge;
        public int shieldGaugeBuff;
        public bool shieldGaugeFix;
        public bool shieldActivate;
        public int defaultHp;
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
        public Skill skills;
        public Target[] targets;
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
        public int id;
        public string cardId;
        //public string trigger;
        //public string scope;
        public string desc;
        //public Condition[] conditions;
        //public Target target;
        //public Effect effect;
    }

    public class Target {        
        public string method;
        public string[] filter;
        public string[] args;
    }   

    public class Token {
        public string token;
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
        public string packageName;
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
        public string cardId;
    }

    public class ShopPrices {
        public int GOLD;
        public int KRW;
        public float USD;
    }

    [System.Serializable]
    public class Unit : CollectionCard {
        public int currentHp;
        public string[] attackTypes;        
        public string itemId;
        public int attackCount;
        public bool ishuman;
        public int originalAttack;
        public int maxHp;
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
        public string detail;
    }

    public class MailReward {
        public string kind;
        public string amount;
        public string cardId;
        public string heroId;
        public MailCard[] cards;
        public MailHero[] heroes;
        public List<List<RewardClass>> boxes;
    }

    public class MailCard {
        public string cardId;
        public int crystal;
    }

    public class MailHero {
        public string heroId;
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
        public RewardItem[] reward;
        public int id;
        public string type;
        public int day;
        public bool attend;
    }

    public class BuyBoxInfo {
        public bool result;
        public AdRewardItem[] items;
    }

    public class AchievementData {
        public RewardItem reward;
        public string id;
        public string acvId;
        public int level;
        public string desc;
        public int progMax;
        public bool rewardGet;
        public bool cleared;
        public int progress;
        public string name;
        public string check;
    }

    public class ShopAdBox {
        public bool claimComplete;
        public RewardClass[] items;
    }

    public class UserRank {
        public string serverId;
        public string suid;
        public int rank;
        public string nickName;
        public int rankId;
        public int score;
    }

    public class Statistics {
        public int winning;
        public string mainCamp;
        public int heroLvTotal;
        public int? medalMax;
        public int? rankTop;
        public string gradeTop;
        public int famePointTop;
    }

    public class UserStatistics {
        public UserRank[] rankTable;
        public Statistics playStatistics;
    }

    public class BattleRank {
        public UserRank[] top;
        public UserRank[] my;
    }
}
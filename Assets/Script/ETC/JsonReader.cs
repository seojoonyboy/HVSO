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
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace IngameClass
{
    [System.Serializable]
    public class Unit{
        public bool ishuman;
        public string rarelity;
        public string[] cardCategories; 
        public string type;
        public string id;
        public string name;
        public string[] attackType;
        public string[] atrributes;
        public int HP;
        public int currentHP;
        public int attack;
        public int cost;
        public int placeable;
        public bool waterPlace;
        public string attackRange;
    }
    

}
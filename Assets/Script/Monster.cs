using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Monster
{
    [System.Serializable]
    public class Unit
    {
        public bool race;
        public int id;
        public int HP;
        public int power;
        public int cost;
        public int placeable;
        public bool waterPlace;
    }
}
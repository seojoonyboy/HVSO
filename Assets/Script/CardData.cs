using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]
public class CardData : ScriptableObject{

    public new string id;
    public Rarelity rarelity;
    public Camp camp;
    public CardType type;
    public UnitClass class_1;
    public UnitClass class_2;
    public Category category_1;
    public Category category_2;
    public string name;
    public int cost;
    public string attack;
    public string hp;
    public bool hero_chk;


    public enum Rarelity {
        common,
        uncommon,
        rare,
        superrare,
        legend,
    }

    public enum Camp {
        humman,
        orc,
    }

    public enum CardType {
        unit,
        type,
    }

    public enum UnitClass {
        none,
        order,
        knowledge,
        scheme,
        sorcery,
    }

    public enum Category {
        none,
        merc,
        army,
        beast,
        hunter,
        underling,
        rider,
        stealth,
        stabbing,
        battlepet,
    }
}

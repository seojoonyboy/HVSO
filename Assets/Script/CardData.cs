
public class CardData {
    //public new string id;
    public string rarelity;
    public string camp;
    public string type;
    public string class_1;
    public string class_2;
    public string category_1;
    public string category_2;
    public string name;
    public int cost;
    public int? attack;
    public int? hp;
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
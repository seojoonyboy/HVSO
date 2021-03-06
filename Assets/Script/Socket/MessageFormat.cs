using System.Collections;
using System.Collections.Generic;

namespace SocketFormat {

    public class MessageFormat {
        public string itemId;
        public Arguments[] targets;
    }

    public class ChangeCardFormat {
        public string camp;
    }
    public class LineNumber {
        public int lineNumber;
    }


    public class Arguments {
        public Arguments(){}
        public Arguments(string method, object[] args) {
            this.method = method;
            this.args = args;
        }
        public string method;
        public object[] args;
    }

    public class ResultFormat {
        public string result;
        public Reward reward;
        public AccountManager.LeagueInfo leagueInfo;
        public int pointUp;
        
        public LevelUp lvUp;
        public LevelUp heroLvUp;
        
        public int leagueWinCount;
        public List<leagueWinReward> leagueWinReward;
    }

    public class LevelUp {
        public int lv;
        public LevelReward[] rewards;
    }

    public class LevelReward {
        public string kind;
        public int amount;
    }

    public class Reward {
        public int supply;
        public int x2supply;
        public int additionalSupply;
        public int userExp;
        public int heroExp;
        public int couponLeft;
    }

    public class leagueWinReward : LevelReward { }
}
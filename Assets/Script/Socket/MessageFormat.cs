using System.Collections;
using System.Collections.Generic;

namespace SocketFormat {

    public class MessageFormat {
        public int itemId;
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
}
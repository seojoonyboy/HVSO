using System.Collections;
using System.Collections.Generic;

namespace SocketFormat {

    public class MessageFormat {
        public int itemId;
        public Arguments[] args;
    }

    public class ChangeCardFormat {
        public string camp;
    }
    public class LineNumber {
        public int lineNumber;
    }


    public class Arguments {
        public string method;
        public string[] args;
    }
}
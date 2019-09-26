namespace SocketFormat {
    public class SendFormat : Base {
        public SendFormat(string method, object args) : base(method, args) { }
        public SendFormat() : base() { }
    }
    public class ReceiveFormat : Base {
        public ReceiveFormat(string method, object args, GameState gameState, string error) : base(method, args) {
            this.gameState = gameState;
            this.error = error;
         }
        public ReceiveFormat() : base() { }
        public int? id;

        public GameState gameState;
        public object error;
    }

    public class Base {
        public string method;
        public object args;

        public Base() { }

        public Base(string method, object args) {
            this.method = method;
            this.args = args;
        }
    }
}

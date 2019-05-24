namespace SocketFormat {
    public class SendFormat : Base {
        public SendFormat(string method, string[] args) : base(method, args) { }
        public SendFormat() : base() { }
    }
    public class ReceiveFormat : Base {
        public ReceiveFormat(string method, string[] args, GameState gameState, string error) : base(method, args) {
            this.gameState = gameState;
            this.error = error;
         }
        public ReceiveFormat() : base() { }

        public GameState gameState;
        public object error;
    }

    public class Base {
        public string method;
        public string[] args;

        public Base() { }

        public Base(string method, string[] args) {
            this.method = method;
            this.args = args;
        }
    }
}

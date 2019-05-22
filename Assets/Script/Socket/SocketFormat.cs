namespace SocketFormat {
    public class SendFormat : Base {
        public SendFormat(string method, string[] args) : base(method, args) { }
        public SendFormat() : base() { }
    }
    public class ReceiveFormat : Base {
        public ReceiveFormat(string method, string[] args) : base(method, args) { }
        public ReceiveFormat() : base() { }
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

using System.IO;
using System.Text;
using Haegin;
using Newtonsoft.Json;

public class GameUserToCompare {
    public int Lv { get; set; }
    public string NickName { get; set; }
    public int Gold { get; set; }
    public int Crystal { get; set; }
    public int CardCount { get; set; }
}

public class Compare {
    public GameUserToCompare local;
    public GameUserToCompare server;

    public static Compare GetCompare(byte[] bytes) {
        string json = Encoding.UTF8.GetString(bytes);
        Compare value = JsonConvert.DeserializeObject<Compare>(json);
        return value;
    }
}
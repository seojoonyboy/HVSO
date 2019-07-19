using System.Collections;
using OtpNet;
using System;
using System.Text;
using System.Threading.Tasks;
using BestHTTP;
using Newtonsoft.Json.Linq;

public class OTPCode {
    byte[] secretKey = new byte[]
    {0x68, 0x8C, 0x81, 0x54,
     0x6B, 0x33, 0xA3, 0x6A, 
     0x21, 0x5B, 0x4D, 0xD6, 
     0x28, 0xBE, 0x68, 0x66, 
     0x4D, 0x65, 0x0A, 0xFC};

    private Totp totp;
    public bool isDone;
    public int computeTotp {
        get {
            return int.Parse(totp.ComputeTotp());
        }
    }
        

    public OTPCode() {
        GetServerTime();
        isDone = false;
    }

    private void GetServerTime() {
        string a = NetworkManager.Instance.baseUrl;
        HTTPRequest www = new HTTPRequest(new Uri(string.Format("{0}api/user/time", NetworkManager.Instance.baseUrl)), GotTime);
        www.Send();
    }

    private void GotTime(HTTPRequest originalRequest, HTTPResponse response) {
        JObject a = JObject.Parse(response.DataAsText);
        double serverTime = double.Parse(a["now"].ToString());
        DateTime convertTime = ConvertFromUnixTimestamp(serverTime);
        TimeCorrection aa = new TimeCorrection(convertTime);
        totp = new Totp(secretKey, timeCorrection:aa);
        long b = 0;
        
        isDone = totp.VerifyTotp(convertTime, totp.ComputeTotp(), out b);
        Logger.Log("OTP Check : " + isDone);
        if(!isDone) GetServerTime();
        //string computeTotp = totp.ComputeTotp();
        //PostOTP(computeTotp);
    }

    private void PostOTP(string computeTotp) {
        JObject body = new JObject();
        body.Add("pass", new JRaw(computeTotp));
    }

    static DateTime ConvertFromUnixTimestamp(double timestamp) {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return origin.AddMilliseconds(timestamp);
    }

    static double ConvertToUnixTimestamp(DateTime date) {
        DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        TimeSpan diff = date - origin;
        return Math.Floor(diff.TotalSeconds);
    }
}
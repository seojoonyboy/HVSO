using HaeginGame;
using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using G.Util;

namespace Haegin
{
    public delegate void Logged(string log);
    public delegate void Handshaked(bool versionOK);
    public delegate void Authenticated();
    public delegate void Processing(ReqAndRes rar);
    public delegate void RetryOccurred(Protocol protocol, int retryCount);
    public delegate void RetryFailed(Protocol protocol);
    public delegate void ErrorOccurred(int error);
    public delegate void MaintenanceStarted();

    public class ReqAndRes
    {
        public HttpWebRequest WebRequest { get; set; }
        public byte Encrypted { get; private set; }
        public ProtocolReq Req { get; private set; }
        public ProtocolRes Res { get; set; }

        public ReqAndRes(long suid, Protocol protocol)
        {
            switch (protocol.ProtocolId)
            {
                case ProtocolId.MaintenanceCheck: Encrypted = 0; break;
                case ProtocolId.MaintenanceCheckV2: Encrypted = 0; break;
                case ProtocolId.Handshake: Encrypted = 0; break;
                case ProtocolId.Handshake2: Encrypted = 0; break;
                case ProtocolId.Auth: Encrypted = 1; break;
                case ProtocolId.AuthApple: Encrypted = 1; break;
                case ProtocolId.AuthGoogle: Encrypted = 1; break;
                case ProtocolId.AuthFacebook: Encrypted = 1; break;
                case ProtocolId.AuthSteam: Encrypted = 1; break;
                case ProtocolId.AuthAppleId: Encrypted = 1; break;
                case ProtocolId.Error: Encrypted = 0; break;
                case ProtocolId.KeyCount: Encrypted = 0; break;
                default: Encrypted = 2; break;
            }
            Req = new ProtocolReq(suid, protocol);
        }

        public byte IncreaseRetry()
        {
            return ++Req.Retry;
        }
    }


    public class ProtoWebClient
    {
        public static int PWC_SerializeError = -10001;
        public static int PWC_DeserializeError = -10002;
        public static int PWC_DequeueError = -10003;
        public static int PWC_HttpWebRequestError = -10004;
        public static int PWC_UnknownError = -10005;

        public event Logged Logged = delegate { };
        public event Handshaked Handshaked = delegate { };
        public event Authenticated Authenticated = delegate { };
        public event Processing Processing = delegate { };
        public event RetryOccurred RetryOccurred = delegate { };
        public event RetryFailed RetryFailed = delegate { };
        public event ErrorOccurred ErrorOccurred = delegate { };
        public event MaintenanceStarted MaintenanceStarted = delegate { };

        public byte RetryMax { get; set; }
        public uint TimeOut { get; set; }
        public uint ElapsedTime { get { return (uint)(DateTime.Now - startTime).TotalMilliseconds; } }

        private DateTime startTime;

        private ReqAndRes currentRAR = null;

        private Queue<ReqAndRes> readyQueue = new Queue<ReqAndRes>();
        private Queue<ReqAndRes> doneQueue = new Queue<ReqAndRes>();

        private XXTea xxtea1 = new XXTea();
        private XXTea xxtea2 = new XXTea();

        private Uri _uri;
        public string URL
        {
            get { return _uri.ToString(); }
            set { _uri = new Uri(value); }
        }

        public long Suid { get; set; }
        public uint Hash { get; set; }

        static ProtoWebClient()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 10;
        }

        public void CallErrorOccurred(int error)
        {
            ErrorOccurred(error);
        }

        public ProtoWebClient(string url)
        {
            URL = url;

            RetryMax = 5;
            TimeOut = 5000;
            startTime = DateTime.Now;
        }

        public void Request(Protocol protocol)
        {
            lock (readyQueue)
            {
                readyQueue.Enqueue(new ReqAndRes(Suid, protocol));
            }
        }

        private void Abort()
        {
            ReqAndRes rar = currentRAR;
            currentRAR = null;
            startTime = DateTime.Now;

            if (rar != null && rar.WebRequest != null)
            {
                rar.WebRequest.Abort();
            }
        }

        public void Reset()
        {
            Abort();

            lock (readyQueue)
            {
                readyQueue.Clear();
            }
        }

        public void Process()
        {
            lock (doneQueue)
            {
                try
                {
                    while (doneQueue.Count > 0)
                    {
                        ReqAndRes rar = doneQueue.Dequeue();
                        Process(rar);
                    }
                }
                catch (Exception ex)
                {
                    Logged(ex.ToString());
#if !DO_NOT_CALL_PROTOWEB_ERROR
                    CallErrorOccurred(PWC_DequeueError);
#endif
                }
            }

            lock (readyQueue)
            {
                if (currentRAR != null)
                {
                    if (ElapsedTime > TimeOut)
                        Retry(currentRAR.Req.Protocol);
                    return;
                }

                if (readyQueue.Count <= 0) return;

                try
                {
                    currentRAR = readyQueue.Peek();

                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(_uri);
                    request.Method = "POST";
                    request.ContentType = "application/octet-stream";

                    startTime = DateTime.Now;

                    currentRAR.WebRequest = request;
                    request.BeginGetRequestStream(new AsyncCallback(OnGetRequestStream), currentRAR);
                }
                catch (Exception ex)
                {
                    Logged(ex.ToString());
#if !DO_NOT_CALL_PROTOWEB_ERROR
                    CallErrorOccurred(PWC_HttpWebRequestError);
#endif
                }
            }
        }

        private void OnGetRequestStream(IAsyncResult ar)
        {
            ReqAndRes rar = (ReqAndRes)ar.AsyncState;

            try
            {
                using (Stream stream = rar.WebRequest.EndGetRequestStream(ar))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    writer.Write(rar.Encrypted);
                    writer.Write(Hash);

                    if (rar.Encrypted >= 2)
                        writer.Write(SerializeAndEncrypt(xxtea2, rar.Req));
                    else if (rar.Encrypted == 1)
                        writer.Write(SerializeAndEncrypt(xxtea1, rar.Req));
                    else
                        Serialize(stream, rar.Req);
                }

                rar.WebRequest.BeginGetResponse(new AsyncCallback(OnGetResponse), rar);
            }
            catch (Exception ex)
            {
                Logged(ex.ToString());
#if !DO_NOT_CALL_PROTOWEB_ERROR
                CallErrorOccurred(PWC_SerializeError);
#endif
            }
        }

        private void OnGetResponse(IAsyncResult ar)
        {
            ReqAndRes rar = (ReqAndRes)ar.AsyncState;

            try
            {
                using (WebResponse response = rar.WebRequest.EndGetResponse(ar))
                {
                    int length = (int)response.ContentLength;
                    byte[] buffer = new byte[length];

                    using (Stream stream = response.GetResponseStream())
                    {
                        int total = 0;
                        while (true)
                        {
                            int n = stream.Read(buffer, total, length - total);
                            if (n <= 0) break;
                            total += n;
                            if (total >= length) break;
                        }

                        if (total != length)
                            throw new Exception("Received Data Length is Wrong");

                        using (MemoryStream ms = new MemoryStream(buffer))
                        using (BinaryReader reader = new BinaryReader(ms))
                        {
                            byte encrypted = reader.ReadByte();

                            if (encrypted >= 2)
                                rar.Res = DecryptAndDeserialize(xxtea2, buffer, 1, length - 1);
                            else if (encrypted == 1)
                                rar.Res = DecryptAndDeserialize(xxtea1, buffer, 1, length - 1);
                            else
                                rar.Res = Deserialize(ms);
                        }

                        lock (readyQueue)
                        {
                            readyQueue.Dequeue();

                            lock (doneQueue)
                            {
                                doneQueue.Enqueue(rar);
                            }

                            currentRAR = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logged(ex.ToString());
#if !DO_NOT_CALL_PROTOWEB_ERROR
                CallErrorOccurred(PWC_DeserializeError);
#endif
            }
        }

        private void Retry(Protocol protocol)
        {
            byte retry = currentRAR.IncreaseRetry();

            Abort();

            if (retry <= RetryMax)
            {
                RetryOccurred(protocol, retry);
            }
            else
            {
                Reset();
                RetryFailed(protocol);
            }
        }

        private byte[] SerializeAndEncrypt(XXTea xxtea, ProtocolReq req)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Serialize(ms, req);
                    return xxtea.Encrypt(ms.GetBuffer(), 0, (int)ms.Position);
                }
            }
            catch (Exception ex)
            {
                Logged(ex.ToString());
                return null;
            }
        }

        protected virtual void Serialize(Stream stream, ProtocolReq req)
        {
            MessagePackSerializer.Serialize(stream, req);
        }

        private ProtocolRes DecryptAndDeserialize(XXTea xxtea, byte[] buffer, int offset, int count)
        {
            try
            {
                byte[] decrypted = xxtea.Decrypt(buffer, offset, count);

                using (MemoryStream ms = new MemoryStream(decrypted, 0, decrypted.Length, true, true))
                {
                    return Deserialize(ms);
                }
            }
            catch (Exception ex)
            {
                Logged(ex.ToString());
                return null;
            }
        }

        protected virtual ProtocolRes Deserialize(Stream stream)
        {
            return MessagePackSerializer.Deserialize<ProtocolRes>(stream);
        }

        private void Process(ReqAndRes rar)
        {
            ProtocolRes response = rar.Res;

            if (response.Result == Result.MaintenanceNow)
            {
#if MDEBUG
                Logged("--------------------------\nMaintenanceNow received...\n--------------------------");
#endif
                MaintenanceStarted();
                return;
            }
            else if (response.ProtocolId == ProtocolId.Handshake)
            {
                HandshakeRes res = (HandshakeRes)response;

                if (res.ProtocolVersionOK)
                {
                    xxtea1.SetKey(new uint[] { res.Key1, res.Key2, res.Key3, res.Key4 });
                }

                Handshaked(res.ProtocolVersionOK);
            }
            else if (response.ProtocolId == ProtocolId.Handshake2)
            {
                Handshake2Res res = (Handshake2Res)response;

                if (res.ProtocolVersionCheck == VersionCheck.Ok)
                {
                    xxtea1.SetKey(new uint[] { res.Key1, res.Key2, res.Key3, res.Key4 });
                }

                Handshaked(res.ProtocolVersionCheck == VersionCheck.Ok);
            }
            else if (response.ProtocolId == ProtocolId.Auth)
            {
                AuthRes res = (AuthRes)response;
                if (res.Result == 0) OnAuthenticated(res.Suid, res.Hash);
            }
            else if (response.ProtocolId == ProtocolId.AuthApple)
            {
                AuthAppleRes res = (AuthAppleRes)response;
                if (res.Result == 0) OnAuthenticated(res.Suid, res.Hash);
            }
            else if (response.ProtocolId == ProtocolId.AuthGoogle)
            {
                AuthGoogleRes res = (AuthGoogleRes)response;
                if (res.Result == 0) OnAuthenticated(res.Suid, res.Hash);
            }
            else if (response.ProtocolId == ProtocolId.AuthFacebook)
            {
                AuthFacebookRes res = (AuthFacebookRes)response;
                if (res.Result == 0) OnAuthenticated(res.Suid, res.Hash);
            }
            else if (response.ProtocolId == ProtocolId.AuthSteam)
            {
                AuthSteamRes res = (AuthSteamRes)response;
                if (res.Result == 0) OnAuthenticated(res.Suid, res.Hash);
            }
            else if (response.ProtocolId == ProtocolId.AuthAppleId)
            {
                AuthAppleIdRes res = (AuthAppleIdRes)response;
                if (res.Result == 0) OnAuthenticated(res.Suid, res.Hash);
            }

            Processing(rar);
        }

        public void OnAuthenticated(long suid, uint hash)
        {
            Suid = suid;
            Hash = hash;

            uint[] keys = xxtea1.Key;
            xxtea2.SetKey(new uint[] { Hash ^ keys[0], Hash ^ keys[1], Hash ^ keys[2], Hash ^ keys[3] });

            Authenticated();
        }
    }
}

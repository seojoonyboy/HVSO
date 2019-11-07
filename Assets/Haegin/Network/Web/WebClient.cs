using HaeginGame;
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
#if UNITY_STANDALONE && USE_STEAM
using Steamworks;
#elif UNITY_ANDROID
using SA.Android.GMS.Auth;
using SA.Android.GMS.Games;
using SA.Android.Vending.Billing;
#elif UNITY_IOS
using SA.iOS.GameKit;
#endif

namespace Haegin
{
    public partial class WebClient : MonoBehaviour
    {
        public enum ErrorCode
        {
            SUCCESS,
            OTHER_DEVICE_LOGIN_ERROR,
            NETWORK_ERROR,
            RECEIPT_VERIFICATION_FAILED,
            RECEIPT_NEED_TO_REFRESH
        }
        public enum VersionCheckCode
        {
            LATEST,
            UPDATE_IF_YOU_WANT,
            UPDATE_REQUIRED
        }
        public enum AuthCode
        {
            SUCCESS,
            NEED_TO_LINK,
            NEED_TO_SELECT,
            NEED_TO_LOGOUT,
            NEED_TO_LOGOUT_CLEAR,
            FAILED,
            FAILED_AuthExpired,
            FAILED_CreationFailed,
            FAILED_NotExist,

            FAILED_Blocked,
            FAILED_Blocked_UnauthorizedPrograms,
            FAILED_Blocked_MisusingSystemErrorsAndBugs,
            FAILED_Blocked_Abusing,
            FAILED_Blocked_InvalidPurchase,
            FAILED_Blocked_InappropriateBehavior,

            FAILED_CrossCheckSignatureMismatched,
            FAILED_CrossCheckRetry,

            Cancel
        }

        public delegate void AliveNoticeExist(AliveRes res);
        public delegate void HandshakedResult(ErrorCode error, VersionCheckCode code, string versionInfo);
        public delegate void AuthResult(ErrorCode error, AuthCode code, AccountType accountType, TimeSpan blockRemainTime, long blockSuid);
        public delegate void LinkAuthResult(ErrorCode error, AuthCode code, string accountId, AccountType localAccountType, string localAccountName, byte[] accountInfo, TimeSpan blockRemainTime, long blockSuid);
        public delegate void TermsListResult(ErrorCode error, List<Terms> list);
        public delegate void TermsConfirmResult(ErrorCode error, bool result);
        public delegate void ConsumeGoogleReceiptResult(ErrorCode error, List<GoogleConsumedProduct> list, byte[] purchasedData);
        public delegate void ConsumeAppleReceiptResult(ErrorCode error, StoreKitTransaction transaction, byte[] purchasedData);
#if USE_ONESTORE_IAP
        public delegate void ConsumeOneStoreReceiptResult(ErrorCode error, List<OneStorePurchasedProduct> list, byte[] purchasedData);
#endif
        public delegate void SteamInitTransactionResult(ErrorCode error, bool isSucceeded, long orderId, long transId, string steamUrl, int errorCode, string errorDesc);
        public delegate void SteamFinalizeTransactionResult(ErrorCode error, bool isSucceeded, int errorCode, string errorDesc, byte[] purchasedData);
        public delegate void EventsListResult(ErrorCode error, List<EventItem> list);
        public delegate void SteamShopProductInfoResult(ErrorCode error, List<IAPProduct> list);
        public delegate void MaintenanceCheckResult(ErrorCode error, bool isMaintenance, string contents, DateTime startTime, DateTime endTime, string ServerUrl, string PatchUrl);
        public delegate void MaintenanceCheckV2Result(ErrorCode error, bool isMaintenance, string contents, DateTime startTime, DateTime endTime, string CommonUrl, string GameUrl, string PatchUrl);
        public delegate void GPrestoCrossCheckResult(bool result);
        public delegate void CouponResult(ErrorCode error, Result result, byte[] data);
        private HandshakedResult handshakedResult;
        private AuthResult authResult;
        private LinkAuthResult linkAuthResult;
        private TermsListResult termsListResult;
        private TermsConfirmResult termsConfirmResult;
        private ConsumeGoogleReceiptResult consumeGoogleReceiptResult;
        private ConsumeAppleReceiptResult consumeAppleReceiptResult;
#if USE_ONESTORE_IAP
        private ConsumeOneStoreReceiptResult consumeOneStoreReceiptResult;
#endif
        private EventsListResult eventsListResult;
        private SteamInitTransactionResult steamInitTransactionResult;
        private SteamFinalizeTransactionResult steamFinalizeTransactionResult;
        private SteamShopProductInfoResult steamShopProductInfoResult;
        private MaintenanceCheckResult maintenanceCheckResult;
        private MaintenanceCheckV2Result maintenanceCheckV2Result;
        private AliveNoticeExist aliveNoticeExist;
        private GPrestoCrossCheckResult gPrestoCrossCheckResult;
        private CouponResult couponResult;

        private float crossCheckSdataWaitingTime;
        private const float SDATA_WAITING_TIME_MAX = 2.0f;

        public byte RetryMax
        {
            get
            {
                return client.RetryMax;
            }
            set
            {
                client.RetryMax = value;
            }
        }

        public uint TimeOut
        {
            get
            {
                return client.TimeOut;
            }
            set
            {
                client.TimeOut = value;
            }
        }

        public long Suid
        {
            get
            {
                return client.Suid;
            }
        }

        public uint Hash
        {
            get
            {
                return client.Hash;
            }
        }

        public static WebClient GetInstance(string urlString = null, bool DontDestroy = true)
        {
            if (DontDestroy)
            {
                if (urlString == null)
                {
                    urlString = prevUrl;
                }
                else
                {
                    prevUrl = urlString;
                }
            }
            GameObject gameObject = GameObject.Find("HaeginWebClient" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(urlString)));
            if (gameObject == null)
            {
                gameObject = new GameObject("HaeginWebClient" + System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(urlString)));
                if (DontDestroy)
                    DontDestroyOnLoad(gameObject);
                gameObject.AddComponent<WebClient>();

                WebClient web = gameObject.GetComponent<WebClient>();
                web.client = new ProtoWebClient(urlString);
                web.client.Processing += web.OnProcessing;
                //web.client.RetryMax = 3;
            }
            return gameObject.GetComponent<WebClient>();
        }

        private WebClient() : base()
        {
        }

        private static string prevUrl = null;

        public ProtoWebClient client = null;

        public event Logged Logged
        {
            add { client.Logged += value; }
            remove { client.Logged -= value; }
        }
        public event Handshaked Handshaked
        {
            add { client.Handshaked += value; }
            remove { client.Handshaked -= value; }
        }
        public event Authenticated Authenticated
        {
            add { client.Authenticated += value; }
            remove { client.Authenticated -= value; }
        }
        public event RetryOccurred RetryOccurred
        {
            add { client.RetryOccurred += value; }
            remove { client.RetryOccurred -= value; }
        }
        public event RetryFailed RetryFailed
        {
            add { client.RetryFailed += value; }
            remove { client.RetryFailed -= value; }
        }
        public event ErrorOccurred ErrorOccurred
        {
            add { client.ErrorOccurred += value; }
            remove { client.ErrorOccurred -= value; }
        }
        public event MaintenanceStarted MaintenanceStarted
        {
            add { client.MaintenanceStarted += value; }
            remove { client.MaintenanceStarted -= value; }
        }
        public event Processing Processing;




        public void OnProcessing(ReqAndRes rar)
        {
            try
            {
                Protocol req = rar.Req.Protocol;
                ProtocolRes res = rar.Res;

                if (res.ProtocolId == ProtocolId.Error)
                {
                    switch (req.ProtocolId)
                    {
                        case ProtocolId.Handshake: Process((HandshakeReq)req, (ErrorRes)res); break;
                        case ProtocolId.Auth: Process((AuthReq)req, (ErrorRes)res); break;
#if UNITY_ANDROID
                        case ProtocolId.AuthGoogle: Process((AuthGoogleReq)req, (ErrorRes)res); break;
                        case ProtocolId.FcmRegister: break;
#if USE_ONESTORE_IAP
                        case ProtocolId.ConsumeOneStoreReceipt: Process((ConsumeOneStoreReceiptReq)req, (ErrorRes)res); break;
#endif
                        case ProtocolId.ConsumeGoogleReceipt: Process((ConsumeGoogleReceiptReq)req, (ErrorRes)res); break;
#endif
                        case ProtocolId.AuthFacebook: Process((AuthFacebookReq)req, (ErrorRes)res); break;
#if UNITY_IOS
                        case ProtocolId.AuthApple: Process((AuthAppleReq)req, (ErrorRes)res); break;
                        case ProtocolId.ApnsRegister: break;
                        case ProtocolId.ConsumeAppleReceipt: Process((ConsumeAppleReceiptReq)req, (ErrorRes)res); break;
#endif
#if UNITY_STANDALONE && USE_STEAM
                        case ProtocolId.AuthSteam: Process((AuthSteamReq)req, (ErrorRes)res); break;
                        case ProtocolId.SteamInitTransaction: Process((SteamInitTransactionReq)req, (ErrorRes)res); break;
                        case ProtocolId.SteamFinalizeTransaction:  Process((SteamFinalizeTransactionReq)req, (ErrorRes)res); break;
                        case ProtocolId.ShopProductList: Process((ShopProductListReq)req, (ErrorRes)res); break;
#endif
                        case ProtocolId.TermsList: Process((TermsListReq)req, (ErrorRes)res); break;
                        case ProtocolId.TermsConfirm: Process((TermsConfirmReq)req, (ErrorRes)res); break;
                        case ProtocolId.EventList: Process((EventListReq)req, (ErrorRes)res); break;
                        case ProtocolId.MaintenanceCheck: Process((MaintenanceCheckReq)req, (ErrorRes)res); break;
                        case ProtocolId.MaintenanceCheckV2: Process((MaintenanceCheckV2Req)req, (ErrorRes)res); break;
                        case ProtocolId.Coupon: Process((CouponReq)req, (ErrorRes)res); break;
                        default: Processing(rar); break;
                    }
                }
                else
                {
                    switch (req.ProtocolId)
                    {
                        case ProtocolId.Handshake: Process((HandshakeReq)req, (HandshakeRes)res); break;
                        case ProtocolId.Auth: Process((AuthReq)req, (AuthRes)res); break;
#if UNITY_IOS
                        case ProtocolId.AuthApple: Process((AuthAppleReq)req, (AuthAppleRes)res); break;
#endif
#if UNITY_ANDROID
                        case ProtocolId.AuthGoogle: Process((AuthGoogleReq)req, (AuthGoogleRes)res); break;
#endif
#if UNITY_STANDALONE && USE_STEAM
                        case ProtocolId.AuthSteam: Process((AuthSteamReq)req, (AuthSteamRes)res); break;
                        case ProtocolId.SteamInitTransaction: Process((SteamInitTransactionReq)req, (SteamInitTransactionRes) res); break;
                        case ProtocolId.SteamFinalizeTransaction: Process((SteamFinalizeTransactionReq)req, (SteamFinalizeTransactionRes)res); break;
                        case ProtocolId.ShopProductList: Process((ShopProductListReq)req, (ShopProductListRes)res); break;
#endif
                        case ProtocolId.AuthFacebook: Process((AuthFacebookReq)req, (AuthFacebookRes)res); break;
                        case ProtocolId.Alive: Process((AliveReq)req, (AliveRes)res); break;
#if UNITY_IOS
                        case ProtocolId.ApnsRegister: Process((ApnsRegisterReq)req, (ApnsRegisterRes)res); break;
#endif
#if UNITY_ANDROID
                        case ProtocolId.FcmRegister: Process((FcmRegisterReq)req, (FcmRegisterRes)res); break;
                        //case ProtocolId.FcmUnregister: Process((FcmUnregisterReq)req, (FcmUnregisterRes)res); break;
#endif
#if UNITY_IOS
                        case ProtocolId.ConsumeAppleReceipt: Process((ConsumeAppleReceiptReq)req, (ConsumeAppleReceiptRes)res); break;
#endif
#if UNITY_ANDROID
#if USE_ONESTORE_IAP
                        case ProtocolId.ConsumeOneStoreReceipt: Process((ConsumeOneStoreReceiptReq)req, (ConsumeOneStoreReceiptRes)res); break;
#endif
                        case ProtocolId.ConsumeGoogleReceipt: Process((ConsumeGoogleReceiptReq)req, (ConsumeGoogleReceiptRes)res); break;
#endif
                        case ProtocolId.TermsList: Process((TermsListReq)req, (TermsListRes)res); break;
                        case ProtocolId.TermsConfirm: Process((TermsConfirmReq)req, (TermsConfirmRes)res); break;
                        case ProtocolId.EventList: Process((EventListReq)req, (EventListRes)res); break;
                        case ProtocolId.MaintenanceCheck: Process((MaintenanceCheckReq)req, (MaintenanceCheckRes)res); break;
                        case ProtocolId.MaintenanceCheckV2: Process((MaintenanceCheckV2Req)req, (MaintenanceCheckV2Res)res); break;
                        case ProtocolId.Coupon: Process((CouponReq)req, (CouponRes)res); break;
                        default: Processing(rar); break;
                    }
                }
            }
            catch (Exception ex)
            {
#if MDEBUG
                Debug.Log(ex.ToString());
#endif
                client.CallErrorOccurred(ProtoWebClient.PWC_UnknownError);
            }
        }

        private int aliveDuration = 30;  // 기본 30초마다 Alive 전송
        bool bAliveStarted = false;
        Coroutine coroutineSendAlive = null;


        IEnumerator SendAlive()
        {
            WaitForSeconds delay = new WaitForSeconds(aliveDuration);
            RequestAlive();
            while (true)
            {
                yield return delay;
                RequestAlive();
            }
        }

        public void SetAliveConfig(int aDuration, AliveNoticeExist callback = null)
        {
            aliveDuration = aDuration;
            aliveNoticeExist = callback;
            if(bAliveStarted)
            {
                StopCoroutine(coroutineSendAlive);
                coroutineSendAlive = StartCoroutine(SendAlive());
            }
        }

        public void RequestAlive()
        {
            AliveReq req = new AliveReq();
            req.Language = TextManager.GetLanguageSetting();
            Request(req);
        }

        public void Process(AliveReq req, AliveRes res)
        {
#if MDEBUG
            Debug.Log("Reply Alive");
#endif
            if (aliveNoticeExist != null)
            {
                aliveNoticeExist(res);
            }
        }


        //
        //   더 이상 이 함수는 사용하지 마세요. protocolVersion는 내부적으로 protocol dll 안에서 서버로 보내게 됩니다.
        // 
        public void RequestHandshake(ushort[] protocolVersion, ushort[] clientVersion, string language, HandshakedResult callback)
        {
            handshakedResult = callback;
            HandshakeReq req = new HandshakeReq { ProtocolVersion = protocolVersion, ClientVersion = clientVersion, Language = language };
#if UNITY_IOS
            req.MarketType = MarketType.AppleStore;
#elif UNITY_ANDROID
#if USE_ONESTORE_IAP
            req.MarketType = MarketType.OneStore;
#else
            req.MarketType = MarketType.GooglePlay;
#endif
#else
            req.MarketType = MarketType.GooglePlay;
#endif
            Request(req);
        }

        public void RequestHandshake(ushort[] clientVersion, string language, HandshakedResult callback)
        {
            handshakedResult = callback;
            HandshakeReq req = new HandshakeReq { ClientVersion = clientVersion, Language = language };
#if UNITY_IOS
            req.MarketType = MarketType.AppleStore;
#elif UNITY_ANDROID
#if USE_ONESTORE_IAP
            req.MarketType = MarketType.OneStore;
#else
            req.MarketType = MarketType.GooglePlay;
#endif
#else
            req.MarketType = MarketType.GooglePlay;
#endif
            Request(req);
        }


        public void Process(HandshakeReq req, HandshakeRes res)
        {
            switch (res.Result)
            {
                case Result.OK:
                    if (res.ProtocolVersionOK && res.ClientVersionOK)
                    {
                            bool VersionMismatched = false;
                        for (int i = 0; i < req.ClientVersion.Length; i++)
                        {
                            if (res.ClientVersion[i] != req.ClientVersion[i])
                            {
                                VersionMismatched = true;
                                break;
                            }
                        }
                        if (VersionMismatched)
                            handshakedResult(ErrorCode.SUCCESS, VersionCheckCode.UPDATE_IF_YOU_WANT, res.ContentsForUpdate);
                        else
                            handshakedResult(ErrorCode.SUCCESS, VersionCheckCode.LATEST, res.ContentsForUpdate);
                    }
                    else
                    {
                        handshakedResult(ErrorCode.SUCCESS, VersionCheckCode.UPDATE_REQUIRED, res.ContentsForUpdate);
                    }
                    break;
                case Result.AuthenticationExpired:
                    handshakedResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, VersionCheckCode.LATEST, res.ContentsForUpdate);
                    break;
                default:
                    handshakedResult(ErrorCode.NETWORK_ERROR, VersionCheckCode.LATEST, res.ContentsForUpdate);
                    break;
            }
        }

        public void Process(HandshakeReq req, ErrorRes res)
        {
            switch (res.Result)
            {
                case Result.AuthenticationExpired:
                    handshakedResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, VersionCheckCode.LATEST, null);
                    break;
                default:
                    handshakedResult(ErrorCode.NETWORK_ERROR, VersionCheckCode.LATEST, null);
                    break;
            }
        }

        public void RequestEventsList(EventsListResult callback)
        {
            eventsListResult = callback;
            EventListReq req = new EventListReq();
            Request(req);
        }

        public void Process(EventListReq req, EventListRes res)
        {
            switch (res.Result)
            {
                case Result.OK:
                    eventsListResult(ErrorCode.SUCCESS, res.Events);
                    break;
                case Result.AuthenticationExpired:
                    eventsListResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, null);
                    break;
                default:
                    eventsListResult(ErrorCode.NETWORK_ERROR, null);
                    break;
            }
        }

        public void Process(EventListReq req, ErrorRes res)
        {
            switch (res.Result)
            {
                case Result.AuthenticationExpired:
                    eventsListResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, null);
                    break;
                default:
                    eventsListResult(ErrorCode.NETWORK_ERROR, null);
                    break;
            }
        }

        public void RequestAuth(AuthResult callback)
        {
            authResult = callback;
            string advertisingId = null;
            if (!Application.RequestAdvertisingIdentifierAsync((string adId, bool trackingEnabled, string error) =>
            {
                if (trackingEnabled)
                    advertisingId = adId;
                else
                    advertisingId = "";
                StartCoroutine(RequestAuthSub(advertisingId));
            }))
            {
                advertisingId = "";
                StartCoroutine(RequestAuthSub(advertisingId));
            }
        }

        public IEnumerator RequestAuthSub(string advertisingId)
        {
            string sdata = null;
#if !DO_NOT_USE_GPRESTO && !UNITY_EDITOR
#if UNITY_IOS || UNITY_ANDROID
            crossCheckSdataWaitingTime = 0.0f;
            WaitForSeconds delay = new WaitForSeconds(1.0f);
            while (string.IsNullOrEmpty(sdata = GPrestoApi.GetSData()) && crossCheckSdataWaitingTime <= SDATA_WAITING_TIME_MAX)
            {
                yield return delay;
            }
#else
            yield return null;
#endif
#else
            yield return null;
#endif
            if (string.IsNullOrEmpty(sdata)) sdata = "";
#if MDEBUG
            Debug.Log("GPresto SData : [" + sdata + "]");
#endif
            AuthReq req = new AuthReq();
            req.AccountPass = Account.GetAccountId();
#if !DO_NOT_USE_GPRESTO && !UNITY_EDITOR
#if UNITY_IOS || UNITY_ANDROID
            //추후 G-Presto 서버와 매칭을 위해 유저에 대한 정보를 게임 서버로 전송시 해당 함수를 호출하여 값을 게임 서버로 전송
            req.Duid = GPresto.Protector.Common.GPUtils.GPUUID();
#else
            req.Duid = advertisingId;
#endif
#else
            req.Duid = advertisingId;
#endif
            req.DeviceInfo = SystemInfo.operatingSystem + "|" + SystemInfo.deviceModel;
            req.Market = IAP.GetMarketType();
            req.ClientVersion = Application.version;
#if UNITY_IOS
            req.Os = OsType.iOS;
#elif UNITY_ANDROID
            req.Os = OsType.Android;
#endif
            req.GpKey = sdata;
            Request(req);
        }

        public void Process(AuthReq req, AuthRes res)
        {
            switch (res.Result)
            {
                case Result.OK:
                    Account.SetAccountId(res.AccountPass, Account.GameServiceAccountType);
                    if (authResult != null)
                    {
                        authResult(ErrorCode.SUCCESS, AuthCode.SUCCESS, res.AccountType, TimeSpan.Zero, 0);
                    }
                    if (bAliveStarted == false)
                    {
                        bAliveStarted = true;
                        coroutineSendAlive = StartCoroutine(SendAlive());
                    }
                    break;
                case Result.AuthenticationExpired:
                    if (authResult != null)
                    {
                        authResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, AuthCode.FAILED_AuthExpired, res.AccountType, TimeSpan.Zero, 0);
                    }
                    break;
                case Result.UserCreationFailed:
                    if (authResult != null)
                    {
                        authResult(ErrorCode.SUCCESS, AuthCode.FAILED_CreationFailed, res.AccountType, TimeSpan.Zero, 0);
                    }
                    break;
                case Result.UserNotExist:
                    if (authResult != null)
                    {
                        authResult(ErrorCode.SUCCESS, AuthCode.FAILED_NotExist, res.AccountType, TimeSpan.Zero, 0);
                    }
                    break;
                case Result.UserBlocked:
                    if (authResult != null)
                    {
                        switch(res.BlockType)
                        {
                            case BlockType.UnauthorizedProgram:
                                authResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_UnauthorizedPrograms, res.AccountType, res.BlockRemainTime, res.BlockedSuid);
                                break;
                            case BlockType.MisusingBug:
                                authResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_MisusingSystemErrorsAndBugs, res.AccountType, res.BlockRemainTime, res.BlockedSuid);
                                break;
                            case BlockType.Abusing:
                                authResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_Abusing, res.AccountType, res.BlockRemainTime, res.BlockedSuid);
                                break;
                            case BlockType.InvalidPurchase:
                                authResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_InvalidPurchase, res.AccountType, res.BlockRemainTime, res.BlockedSuid);
                                break;
                            case BlockType.BadManner:
                                authResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_InappropriateBehavior, res.AccountType, res.BlockRemainTime, res.BlockedSuid);
                                break;
                        }
                    }
                    break;
                case Result.AuthenticationCrossCheckFailed:
                    Account.SetAccountId(res.AccountPass, Account.GameServiceAccountType);
                    if (authResult != null)
                    {
                        authResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckSignatureMismatched, res.AccountType, TimeSpan.Zero, res.BlockedSuid);
                    }
                    break;
                case Result.AuthenticationCrossCheckRetry:
                    Account.SetAccountId(res.AccountPass, Account.GameServiceAccountType);
                    if (authResult != null)
                    {
                        authResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckRetry, res.AccountType, TimeSpan.Zero, res.BlockedSuid);
                    }
                    break;
                default:
                    if (authResult != null)
                    {
                        authResult(ErrorCode.SUCCESS, AuthCode.FAILED, res.AccountType, TimeSpan.Zero, 0);
                    }
                    break;
            }
        }

        public void Process(AuthReq req, ErrorRes res)
        {
            switch (res.Result)
            {
                case Result.AuthenticationExpired:
                    if (authResult != null)
                    {
                        authResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, AuthCode.FAILED_AuthExpired, AccountType.None, TimeSpan.Zero, 0);
                    }
                    break;
                case Result.UserCreationFailed:
                    if (authResult != null)
                    {
                        authResult(ErrorCode.NETWORK_ERROR, AuthCode.FAILED_CreationFailed, AccountType.None, TimeSpan.Zero, 0);
                    }
                    break;
                case Result.UserNotExist:
                    if (authResult != null)
                    {
                        authResult(ErrorCode.NETWORK_ERROR, AuthCode.FAILED_NotExist, AccountType.None, TimeSpan.Zero, 0);
                    }
                    break;
                case Result.AuthenticationCrossCheckFailed:
                    if (authResult != null)
                    {
                        authResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckSignatureMismatched, AccountType.None, TimeSpan.Zero, 0);
                    }
                    break;
                case Result.AuthenticationCrossCheckRetry:
                    if (authResult != null)
                    {
                        authResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckRetry, AccountType.None, TimeSpan.Zero, 0);
                    }
                    break;
                default:
                    if (authResult != null)
                    {
                        authResult(ErrorCode.NETWORK_ERROR, AuthCode.FAILED, AccountType.None, TimeSpan.Zero, 0);
                    }
                    break;
            }
        }

        public void RequestAuthGoogle(string token, string accountId, LinkOption linkOption, LinkAuthResult callback)
        {
            StartCoroutine(RequestAuthGoogleSub(token, accountId, linkOption, callback));
        }

        public IEnumerator RequestAuthGoogleSub(string token, string accountId, LinkOption linkOption, LinkAuthResult callback)
        {
            string sdata = null;
#if !DO_NOT_USE_GPRESTO && !UNITY_EDITOR
#if UNITY_IOS || UNITY_ANDROID
            crossCheckSdataWaitingTime = 0.0f;
            WaitForSeconds delay = new WaitForSeconds(1.0f);
            while (string.IsNullOrEmpty(sdata = GPrestoApi.GetSData()) && crossCheckSdataWaitingTime <= SDATA_WAITING_TIME_MAX)
            {
                yield return delay;
            }
#else
            yield return null;
#endif
#else
            yield return null;
#endif
            if (string.IsNullOrEmpty(sdata)) sdata = "";
#if MDEBUG
            Debug.Log("GPresto SData : [" + sdata + "]");
#endif
            linkAuthResult = callback;


#if UNITY_ANDROID
            AN_PlayersClient client = AN_Games.GetPlayersClient();
            client.GetCurrentPlayer((result) => {
                string name = "noname";
                if (result.IsSucceeded)
                {
                    AN_Player player = result.Data;
                    name = player.DisplayName;
                }
                AuthGoogleReq req = new AuthGoogleReq();
                req.AccountPass = accountId;
                req.Link = linkOption;
                req.AuthCode = token;
                req.Name = name;
                req.DeviceInfo = SystemInfo.operatingSystem + "|" + SystemInfo.deviceModel;
                req.Market = IAP.GetMarketType();
                req.ClientVersion = Application.version;
#if UNITY_IOS
                req.Os = OsType.iOS;
#elif UNITY_ANDROID
                req.Os = OsType.Android;
#endif
                req.GpKey = sdata;

#if MDEBUG
                Debug.Log("AuthGoogleReq -------------------------------\n" + 
                req.AccountPass + "\n" +
                req.Link + "\n" +
                req.AuthCode + "\n" +
                req.Name + "\n" +
                req.DeviceInfo + "\n" +
                req.Market + "\n" +
                req.ClientVersion + "\n" +
                req.Os + "\n" +
                req.GpKey + "\n" +
                "------------- -------------------------------");
#endif
                Request(req);
            });
#endif
        }

        public void Process(AuthGoogleReq req, AuthGoogleRes res)
        {
            switch (res.Result)
            {
                case Result.OK:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.SUCCESS, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    if (bAliveStarted == false)
                    {
                        bAliveStarted = true;
                        coroutineSendAlive = StartCoroutine(SendAlive());
                    }
                    break;
                case Result.AuthenticationNeedToLink:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_LINK, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationNeedToSelect:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_SELECT, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationNeedToLogout:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_LOGOUT, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationNeedToLogoutAndClear:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_LOGOUT_CLEAR, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationExpired:
                    linkAuthResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, AuthCode.FAILED_AuthExpired, null, AccountType.None, null, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.UserBlocked:
                    switch(res.BlockType)
                    {
                        case BlockType.UnauthorizedProgram:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_UnauthorizedPrograms, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.MisusingBug:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_MisusingSystemErrorsAndBugs, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.Abusing:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_Abusing, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.InvalidPurchase:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_InvalidPurchase, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.BadManner:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_InappropriateBehavior, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                    }
                    break;
                case Result.AuthenticationCrossCheckFailed:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckSignatureMismatched, null, AccountType.None, null, res.ExtraData, TimeSpan.Zero, res.BlockedSuid);
                    break;
                case Result.AuthenticationCrossCheckRetry:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckRetry, null, AccountType.None, null, res.ExtraData, TimeSpan.Zero, res.BlockedSuid);
                    break;
                default:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED, null, AccountType.None, null, res.ExtraData, TimeSpan.Zero, 0);
                    break;
            }
        }

        public void Process(AuthGoogleReq req, ErrorRes res)
        {
            switch (res.Result)
            {
                case Result.AuthenticationExpired:
                    linkAuthResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, AuthCode.FAILED_AuthExpired, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationCrossCheckFailed:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckSignatureMismatched, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationCrossCheckRetry:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckRetry, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
                default:
                    linkAuthResult(ErrorCode.NETWORK_ERROR, AuthCode.FAILED, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
            }
        }

        public void RequestAuthApple(string playerId, string playerAlias, string publicKeyUrl, string signature, long timestamp, string salt, string accountId, LinkOption linkOption, LinkAuthResult callback)
        {
            StartCoroutine(RequestAuthAppleSub(playerId, playerAlias, publicKeyUrl, signature, timestamp, salt, accountId, linkOption, callback));
        }

        public IEnumerator RequestAuthAppleSub(string playerId, string playerAlias, string publicKeyUrl, string signature, long timestamp, string salt, string accountId, LinkOption linkOption, LinkAuthResult callback)
        {
            string sdata = null;
#if !DO_NOT_USE_GPRESTO && !UNITY_EDITOR
#if UNITY_IOS || UNITY_ANDROID
            crossCheckSdataWaitingTime = 0.0f;
            WaitForSeconds delay = new WaitForSeconds(1.0f);
            while (string.IsNullOrEmpty(sdata = GPrestoApi.GetSData()) && crossCheckSdataWaitingTime <= SDATA_WAITING_TIME_MAX)
            {
                yield return delay;
            }
#else
            yield return null;
#endif
#else
            yield return null;
#endif

            if (string.IsNullOrEmpty(sdata)) sdata = "";
#if MDEBUG
            Debug.Log("GPresto SData : [" + sdata + "]");
#endif
            linkAuthResult = callback;
            AuthAppleReq req = new AuthAppleReq();
            req.AccountPass = accountId;
            req.Link = linkOption;
            req.PlayerId = playerId;
            req.PlayerAlias = playerAlias;
            req.Signature = new GameCenterSignature
            {
                PublicKeyUrl = publicKeyUrl,
                Signature = signature,
                Timestamp = timestamp,
                Salt = salt
            };
#if UNITY_IOS
            ISN_GKLocalPlayer player = ISN_GKLocalPlayer.LocalPlayer;
            if(player != null)
            {
                req.Name = player.Alias;
            }
            else
            {
                req.Name = "noname";
            }
#endif
            req.DeviceInfo = SystemInfo.operatingSystem + "|" + SystemInfo.deviceModel;
            req.Market = IAP.GetMarketType();
            req.ClientVersion = Application.version;
#if UNITY_IOS
            req.Os = OsType.iOS;
#elif UNITY_ANDROID
            req.Os = OsType.Android;
#endif
            req.GpKey = sdata;
            Request(req);
        }

        public void Process(AuthAppleReq req, AuthAppleRes res)
        {
            switch (res.Result)
            {
                case Result.OK:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.SUCCESS, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    if (bAliveStarted == false)
                    {
                        bAliveStarted = true;
                        coroutineSendAlive = StartCoroutine(SendAlive());
                    }
                    break;
                case Result.AuthenticationNeedToLink:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_LINK, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationNeedToSelect:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_SELECT, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationNeedToLogout:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_LOGOUT, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationNeedToLogoutAndClear:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_LOGOUT_CLEAR, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationExpired:
                    linkAuthResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, AuthCode.FAILED_AuthExpired, null, AccountType.None, null, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.UserBlocked:
                    switch (res.BlockType)
                    {
                        case BlockType.UnauthorizedProgram:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_UnauthorizedPrograms, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.MisusingBug:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_MisusingSystemErrorsAndBugs, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.Abusing:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_Abusing, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.InvalidPurchase:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_InvalidPurchase, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.BadManner:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_InappropriateBehavior, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                    }
                    break;
                case Result.AuthenticationCrossCheckFailed:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckSignatureMismatched, null, AccountType.None, null, null, TimeSpan.Zero, res.BlockedSuid);
                    break;
                case Result.AuthenticationCrossCheckRetry:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckRetry, null, AccountType.None, null, null, TimeSpan.Zero, res.BlockedSuid);
                    break;
                default:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED, null, AccountType.None, null, res.ExtraData, TimeSpan.Zero, 0);
                    break;
            }
        }

        public void Process(AuthAppleReq req, ErrorRes res)
        {
            switch (res.Result)
            {
                case Result.AuthenticationExpired:
                    linkAuthResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, AuthCode.FAILED_AuthExpired, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationCrossCheckFailed:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckSignatureMismatched, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationCrossCheckRetry:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckRetry, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
                default:
                    linkAuthResult(ErrorCode.NETWORK_ERROR, AuthCode.FAILED, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
            }
        }

        public void RequestFacebookAuth(long fbid, string token, string accountId, string name, LinkOption linkOption, LinkAuthResult callback)
        {
            StartCoroutine(RequestFacebookAuthSub(fbid, token, accountId, name, linkOption, callback));
        }

        public IEnumerator RequestFacebookAuthSub(long fbid, string token, string accountId, string name, LinkOption linkOption, LinkAuthResult callback)
        {
            string sdata = null;
#if !DO_NOT_USE_GPRESTO && !UNITY_EDITOR
#if UNITY_IOS || UNITY_ANDROID
            crossCheckSdataWaitingTime = 0.0f;
            WaitForSeconds delay = new WaitForSeconds(1.0f);
            while (string.IsNullOrEmpty(sdata = GPrestoApi.GetSData()) && crossCheckSdataWaitingTime <= SDATA_WAITING_TIME_MAX)
            {
                yield return delay;
            }
#else
            yield return null;
#endif
#else
            yield return null;
#endif
            if (string.IsNullOrEmpty(sdata)) sdata = "";
#if MDEBUG
            Debug.Log("GPresto SData : [" + sdata + "]");
#endif
            linkAuthResult = callback;
            AuthFacebookReq req = new AuthFacebookReq();
            req.AccountPass = accountId;
            req.Link = linkOption;
            req.UserId = fbid;
            req.AccessToken = token;
            req.Name = name;
            req.DeviceInfo = SystemInfo.operatingSystem + "|" + SystemInfo.deviceModel;
            req.Market = IAP.GetMarketType();
            req.ClientVersion = Application.version;
#if UNITY_IOS
            req.Os = OsType.iOS;
#elif UNITY_ANDROID
            req.Os = OsType.Android;
#endif
            req.GpKey = sdata;
            Request(req);
        }

        public void Process(AuthFacebookReq req, AuthFacebookRes res)
        {
            switch (res.Result)
            {
                case Result.OK:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.SUCCESS, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    if (bAliveStarted == false)
                    {
                        bAliveStarted = true;
                        coroutineSendAlive = StartCoroutine(SendAlive());
                    }
                    break;
                case Result.AuthenticationNeedToLink:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_LINK, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationNeedToSelect:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_SELECT, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationNeedToLogout:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_LOGOUT, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationNeedToLogoutAndClear:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_LOGOUT_CLEAR, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationExpired:
                    linkAuthResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, AuthCode.FAILED_AuthExpired, null, AccountType.None, null, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.UserBlocked:
                    switch (res.BlockType)
                    {
                        case BlockType.UnauthorizedProgram:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_UnauthorizedPrograms, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.MisusingBug:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_MisusingSystemErrorsAndBugs, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.Abusing:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_Abusing, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.InvalidPurchase:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_InvalidPurchase, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.BadManner:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_InappropriateBehavior, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                    }
                    break;
                case Result.AuthenticationCrossCheckFailed:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckSignatureMismatched, null, AccountType.None, null, null, TimeSpan.Zero, res.BlockedSuid);
                    break;
                case Result.AuthenticationCrossCheckRetry:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckRetry, null, AccountType.None, null, null, TimeSpan.Zero, res.BlockedSuid);
                    break;
                default:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED, null, AccountType.None, null, res.ExtraData, TimeSpan.Zero, 0);
                    break;
            }
        }

        public void Process(AuthFacebookReq req, ErrorRes res)
        {
            switch (res.Result)
            {
                case Result.AuthenticationExpired:
                    linkAuthResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, AuthCode.FAILED_AuthExpired, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationCrossCheckFailed:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckSignatureMismatched, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationCrossCheckRetry:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckRetry, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
                default:
                    linkAuthResult(ErrorCode.NETWORK_ERROR, AuthCode.FAILED, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
            }
        }

        public void RequestSteamAuth(ulong steamId, byte[] sessionTicket, string accountId, string name, LinkOption linkOption, LinkAuthResult callback)
        {
            linkAuthResult = callback;

            AuthSteamReq req = new AuthSteamReq();
            req.AccountPass = accountId;
            req.Link = linkOption;
            req.SteamId = steamId;
            req.SessionTicket = sessionTicket;
            req.Name = name;
            req.DeviceInfo = SystemInfo.operatingSystem + "|" + SystemInfo.deviceModel;
            req.Market = IAP.GetMarketType();
            req.ClientVersion = Application.version;
            switch (Application.platform)
            {
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    req.Os = OsType.OSX;
                    break;
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    req.Os = OsType.Windows;
                    break;
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    req.Os = OsType.Linux;
                    break;
                default:
                    req.Os = OsType.None;
                    break;
            }
            Request(req);
        }

        public void Process(AuthSteamReq req, AuthSteamRes res)
        {
            switch (res.Result)
            {
                case Result.OK:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.SUCCESS, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    if (bAliveStarted == false)
                    {
                        bAliveStarted = true;
                        coroutineSendAlive = StartCoroutine(SendAlive());
                    }
                    break;
                case Result.AuthenticationNeedToLink:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_LINK, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationNeedToSelect:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_SELECT, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationNeedToLogout:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_LOGOUT, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationNeedToLogoutAndClear:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.NEED_TO_LOGOUT_CLEAR, res.AccountPass, res.LocalAccountType, res.LocalAccountName, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationExpired:
                    linkAuthResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, AuthCode.FAILED_AuthExpired, null, AccountType.None, null, res.ExtraData, TimeSpan.Zero, 0);
                    break;
                case Result.UserBlocked:
                    switch (res.BlockType)
                    {
                        case BlockType.UnauthorizedProgram:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_UnauthorizedPrograms, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.MisusingBug:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_MisusingSystemErrorsAndBugs, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.Abusing:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_Abusing, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.InvalidPurchase:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_InvalidPurchase, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                        case BlockType.BadManner:
                            linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_Blocked_InappropriateBehavior, null, AccountType.None, null, res.ExtraData, res.BlockRemainTime, res.BlockedSuid);
                            break;
                    }
                    break;
                case Result.AuthenticationCrossCheckFailed:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckSignatureMismatched, null, AccountType.None, null, null, TimeSpan.Zero, res.BlockedSuid);
                    break;
                case Result.AuthenticationCrossCheckRetry:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckRetry, null, AccountType.None, null, null, TimeSpan.Zero, res.BlockedSuid);
                    break;
                default:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED, null, AccountType.None, null, res.ExtraData, TimeSpan.Zero, 0);
                    break;
            }
        }

        public void Process(AuthSteamReq req, ErrorRes res)
        {
            switch (res.Result)
            {
                case Result.AuthenticationExpired:
                    linkAuthResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, AuthCode.FAILED_AuthExpired, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationCrossCheckFailed:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckSignatureMismatched, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
                case Result.AuthenticationCrossCheckRetry:
                    linkAuthResult(ErrorCode.SUCCESS, AuthCode.FAILED_CrossCheckRetry, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
                default:
                    linkAuthResult(ErrorCode.NETWORK_ERROR, AuthCode.FAILED, null, AccountType.None, null, null, TimeSpan.Zero, 0);
                    break;
            }
        }

        public void RequestTermsList(TermsListResult callback)
        {
            termsListResult = callback;
            TermsListReq req = new TermsListReq();
            try
            {
                req.Language = (Language)System.Enum.Parse(typeof(Language), TextManager.GetLanguageSetting());
            }
            catch
            {
                req.Language = Language.English;
            }
            Request(req);
        }

        public void Process(TermsListReq req, TermsListRes res)
        {
            switch (res.Result)
            {
                case Result.OK:
                    termsListResult(ErrorCode.SUCCESS, res.List);
                    break;
                case Result.AuthenticationExpired:
                    termsListResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, null);
                    break;
                default:
                    termsListResult(ErrorCode.SUCCESS, null);
                    break;
            }
        }

        public void Process(TermsListReq req, ErrorRes res)
        {
            switch (res.Result)
            {
                case Result.AuthenticationExpired:
                    termsListResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, null);
                    break;
                default:
                    termsListResult(ErrorCode.NETWORK_ERROR, null);
                    break;
            }
        }

        public void RequestTermsConfirm(List<TermsKindVersion> confirms, TermsConfirmResult callback)
        {
            termsConfirmResult = callback;
            TermsConfirmReq req = new TermsConfirmReq();
            req.Confirms = confirms;
            Request(req);
        }

        public void Process(TermsConfirmReq req, TermsConfirmRes res)
        {
            switch (res.Result)
            {
                case Result.OK:
                    termsConfirmResult(ErrorCode.SUCCESS, true);
                    break;
                case Result.AuthenticationExpired:
                    termsConfirmResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, false);
                    break;
                default:
                    termsConfirmResult(ErrorCode.SUCCESS, false);
                    break;
            }
        }

        public void Process(TermsConfirmReq req, ErrorRes res)
        {
            switch (res.Result)
            {
                case Result.AuthenticationExpired:
                    termsConfirmResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, false);
                    break;
                default:
                    termsConfirmResult(ErrorCode.NETWORK_ERROR, false);
                    break;
            }
        }

#if UNITY_IOS
        public void RequestApnsRegister(string deviceToken)
        {
            ApnsRegisterReq req = new ApnsRegisterReq();
            req.DeviceToken = deviceToken;
            req.Language = TextManager.GetLanguageSetting();
            Request(req);
        }

        public void Process(ApnsRegisterReq req, ApnsRegisterRes res)
        {
#if MDEBUG
            Debug.Log("APNS Register " + res.Result);
#endif
        }

        public void RequestConsumeAppleReceipt(string receiptRaw, ConsumeAppleReceiptResult callback)
        {
            consumeAppleReceiptResult = callback;
            ConsumeAppleReceiptReq req = new ConsumeAppleReceiptReq();
            req.Receipt = receiptRaw;
            req.Language = TextManager.GetLanguageSetting();
            Request(req);
        }

        public void Process(ConsumeAppleReceiptReq req, ConsumeAppleReceiptRes res)
        {
#if MDEBUG
            Debug.Log("Process(ConsumeAppleReceiptReq req, ConsumeAppleReceiptRes res)");
#endif
            switch (res.Result)
            {
                case Result.OK:
                    {
                        bool bRetError = true;
                        if (res.Transactions != null)
                        {
                            foreach (var tr in res.Transactions)
                            {
                                if (tr != null && IAP.IsAvailableItem(tr.ProductId))
                                {
                                    bRetError = false;
                                    consumeAppleReceiptResult(ErrorCode.SUCCESS, tr, res.PurchasedData);
                                }
                            }
                        }
                        if (bRetError)
                        {
                            consumeAppleReceiptResult(ErrorCode.RECEIPT_VERIFICATION_FAILED, null, null);
                        }
                    }
                    break;
                case Result.AuthenticationExpired:
                    consumeAppleReceiptResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, null, null);
                    break;
                case Result.ValidatingReceiptNeedToRefresh:
#if MDEBUG
                    Debug.Log("------------------------------------------------------");
                    Debug.Log("ValidatingReceiptNeedToRefresh  [" + req.Receipt + "]");
                    Debug.Log("------------------------------------------------------");
#endif
                    consumeAppleReceiptResult(ErrorCode.RECEIPT_NEED_TO_REFRESH, null, null);
                    break;
                case Result.ValidatingReceiptFailed:
                    consumeAppleReceiptResult(ErrorCode.RECEIPT_VERIFICATION_FAILED, null, null);
                    break;
            }
        }

        public void Process(ConsumeAppleReceiptReq req, ErrorRes res)
        {
#if MDEBUG
            Debug.Log("Process(ConsumeAppleReceiptReq req, ErrorRes res)");
#endif
            switch (res.Result)
            {
                case Result.AuthenticationExpired:
                    consumeAppleReceiptResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, null, null);
                    break;
                default:
                    consumeAppleReceiptResult(ErrorCode.NETWORK_ERROR, null, null);
                    break;
            }
        }
#endif

#if UNITY_ANDROID
        public void RequestFcmRegister(string regId)
        {
            FcmRegisterReq req = new FcmRegisterReq();
            req.RegistrationId = regId;
            req.Language = TextManager.GetLanguageSetting();
            Request(req);
        }

        public void Process(FcmRegisterReq req, FcmRegisterRes res)
        {
#if MDEBUG
            Debug.Log("FCM Register " + res.Result);
#endif
        }

#if USE_ONESTORE_IAP
        public void RequestConsumeOneStoreReceipt(List<OneStorePurchasedProduct> purchases, ConsumeOneStoreReceiptResult callback)
        {
            consumeOneStoreReceiptResult = callback;
            ConsumeOneStoreReceiptReq req = new ConsumeOneStoreReceiptReq();
            req.Products = purchases;
            req.Language = TextManager.GetLanguageSetting();
            Request(req);
        }

        public void Process(ConsumeOneStoreReceiptReq req, ConsumeOneStoreReceiptRes res)
        {
            switch (res.Result)
            {
                case Result.OK:
                     consumeOneStoreReceiptResult(ErrorCode.SUCCESS, res.Products, res.PurchasedData);
                    break;
                case Result.AuthenticationExpired:
                    consumeOneStoreReceiptResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, null, null);
                    break;
                case Result.ValidatingReceiptFailed:
                    consumeOneStoreReceiptResult(ErrorCode.SUCCESS, null, null);
                    break;
                case Result.ValidatingReceiptUsed:
                    consumeOneStoreReceiptResult(ErrorCode.SUCCESS, null, null);
                    break;
            }
        }

        public void Process(ConsumeOneStoreReceiptReq req, ErrorRes res)
        {
            switch (res.Result)
            {
                case Result.AuthenticationExpired:
#if MDEBUG
                    Debug.Log("Process(ConsumeOneStoreReceiptReq req, ErrorRes res)   AuthenticationExpired");
#endif
                    consumeOneStoreReceiptResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, null, null);
                    break;
                default:
                    consumeOneStoreReceiptResult(ErrorCode.NETWORK_ERROR, null, null);
                    break;
            }
        }
#endif

        public void RequestConsumeGoogleReceipt(List<AN_Purchase> purchases, ConsumeGoogleReceiptResult callback)
        {
            consumeGoogleReceiptResult = callback;
            ConsumeGoogleReceiptReq req = new ConsumeGoogleReceiptReq();
            req.Products = new List<GooglePurchasedProduct>();
            foreach (AN_Purchase p in purchases)
            {
                GooglePurchasedProduct gpp = new GooglePurchasedProduct
                {
                    PackageName = p.PackageName,
                    OrderId = p.OrderId,
                    ProductId = p.ProductId,
                    Token = p.Token
                };
                req.Products.Add(gpp);
            }
            req.Language = TextManager.GetLanguageSetting();
            Request(req);
        }

        public void Process(ConsumeGoogleReceiptReq req, ConsumeGoogleReceiptRes res)
        {
            switch(res.Result) {
                case Result.OK:
                    consumeGoogleReceiptResult(ErrorCode.SUCCESS, res.Products, res.PurchasedData);
                    break;
                case Result.AuthenticationExpired:
                    consumeGoogleReceiptResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, null, null);
                    break;
                case Result.ValidatingReceiptFailed:
                    consumeGoogleReceiptResult(ErrorCode.SUCCESS, null, null);
                    break;
                case Result.ValidatingReceiptUsed:
                    consumeGoogleReceiptResult(ErrorCode.SUCCESS, null, null);
                    break;
            }
        }

        public void Process(ConsumeGoogleReceiptReq req, ErrorRes res)
        {
            switch(res.Result) {
                case Result.AuthenticationExpired:
                    Debug.Log("Process(ConsumeGoogleReceiptReq req, ErrorRes res)   AuthenticationExpired");
                    consumeGoogleReceiptResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, null, null);
                    break;
                default:
                    consumeGoogleReceiptResult(ErrorCode.NETWORK_ERROR, null, null);
                    break;
            }
        }
#endif

#if UNITY_STANDALONE && USE_STEAM
        public void RequestSteamInitTransaction(string productId, SteamInitTransactionResult callback)
        {
            steamInitTransactionResult = callback;
            SteamInitTransactionReq req = new SteamInitTransactionReq();
            req.SteamId = SteamUser.GetSteamID().m_SteamID;
            req.Sku = productId;
            Request(req);
        }

        public void Process(SteamInitTransactionReq req, SteamInitTransactionRes res)
        {
            switch (res.Result)
            {
                case Result.OK:
                    steamInitTransactionResult(ErrorCode.SUCCESS, res.IsSucceeded, res.OrderId, res.TransId, res.SteamUrl, res.ErrorCode, res.ErrorDesc);
                    break;
                default:
                    steamInitTransactionResult(ErrorCode.SUCCESS, false, res.OrderId, res.TransId, res.SteamUrl, res.ErrorCode, res.ErrorDesc);
                    break;
            }
        }

        public void Process(SteamInitTransactionReq req, ErrorRes res)
        {
            switch(res.Result)
            {
                case Result.AuthenticationExpired:
                    steamInitTransactionResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, false, 0, 0, null, -999999, "other device login error");
                    break;
                default:
                    steamInitTransactionResult(ErrorCode.NETWORK_ERROR, false, 0, 0, null, -999999, "error");
                    break;
            }
        }

        public void RequestSteamFinalizeTransaction(long orderId, SteamFinalizeTransactionResult callback)
        {
            steamFinalizeTransactionResult = callback;
            SteamFinalizeTransactionReq req = new SteamFinalizeTransactionReq();
            req.OrderId = orderId;
            Request(req);
        }

        public void Process(SteamFinalizeTransactionReq req, SteamFinalizeTransactionRes res)
        {
            switch(res.Result)
            {
                case Result.OK:
                    steamFinalizeTransactionResult(ErrorCode.SUCCESS, res.IsSucceeded, res.ErrorCode, res.ErrorDesc, res.PurchasedData);
                    break;
                default:
                    steamFinalizeTransactionResult(ErrorCode.SUCCESS, false, res.ErrorCode, res.ErrorDesc, res.PurchasedData);
                    break;
            }
        }

        public void Process(SteamFinalizeTransactionReq req, ErrorRes res)
        {
            switch(res.Result)
            {
                case Result.AuthenticationExpired:
                    steamFinalizeTransactionResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, false, -999999, "other deivce login error", null);
                    break;
                default:
                    steamFinalizeTransactionResult(ErrorCode.NETWORK_ERROR, false, -999999, "error", null);
                    break;
            }
        }

        public void RequestSteamShopProductInfo(string[] productIdentifiers, SteamShopProductInfoResult callback)
        {
            steamShopProductInfoResult = callback;
            ShopProductListReq req = new ShopProductListReq();
            req.Skus = new List<string>();
            for (int i = 0; i < productIdentifiers.Length; i++) 
            {
                req.Skus.Add(productIdentifiers[i]);
            }
            Request(req);
        }

        public void Process(ShopProductListReq req, ShopProductListRes res)
        {
            switch(res.Result)
            {
                case Result.OK:
                    {
                        List<IAPProduct> list = new List<IAPProduct>();
                        foreach(ShopProductInfo prod in res.Products)
                        {
                            IAPProduct product = new IAPProduct(prod.ProductId, prod.Title, prod.Price, prod.Description);
                            list.Add(product);
                        }
                        steamShopProductInfoResult(ErrorCode.SUCCESS, list);
                    }
                    break;
                default:
                    steamShopProductInfoResult(ErrorCode.NETWORK_ERROR, null);
                    break;
            }
        }

        public void Process(ShopProductListReq req, ErrorRes res)
        {
            switch (res.Result)
            {
                case Result.AuthenticationExpired:
                    steamShopProductInfoResult(ErrorCode.OTHER_DEVICE_LOGIN_ERROR, null);
                    break;
                default:
                    steamShopProductInfoResult(ErrorCode.NETWORK_ERROR, null);
                    break;
            }
        }
#endif

        public void RequestMaintenanceCheck(string ServerName, MaintenanceCheckResult callback)
        {
            maintenanceCheckResult = callback;
            MaintenanceCheckReq req = new MaintenanceCheckReq();
            req.Language = TextManager.GetLanguageSetting();
            req.ServerName = ServerName;
            Request(req);
        }

        public void Process(MaintenanceCheckReq req, MaintenanceCheckRes res)
        {
            if (res.Result == Result.OK)
                maintenanceCheckResult(ErrorCode.SUCCESS, res.IsMaintenance, res.Message, res.StartTime, res.EndTime, res.ServerUrl, res.PatchUrl);
            else
                maintenanceCheckResult(ErrorCode.NETWORK_ERROR, true, res.Message, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null, null);
            //Debug.Log(res.ServerUrl);
            //Debug.Log(res.PatchUrl);
            //Debug.Log(res.Message);
        }

        public void Process(MaintenanceCheckReq req, ErrorRes res)
        {
            maintenanceCheckResult(ErrorCode.NETWORK_ERROR, true, "contents", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null, null);
        }

        public void RequestMaintenanceCheckV2(string ServerName, MaintenanceCheckV2Result callback)
        {
            maintenanceCheckV2Result = callback;
            MaintenanceCheckV2Req req = new MaintenanceCheckV2Req();
            req.Language = TextManager.GetLanguageSetting();
            req.ServerName = ServerName;
            Request(req);
        }

        public void Process(MaintenanceCheckV2Req req, MaintenanceCheckV2Res res)
        {
            if (res.Result == Result.OK)
                maintenanceCheckV2Result(ErrorCode.SUCCESS, res.IsMaintenance, res.Message, res.StartTime, res.EndTime, res.CommonUrl, res.GameUrl, res.PatchUrl);
            else
                maintenanceCheckV2Result(ErrorCode.NETWORK_ERROR, true, res.Message, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null, null, null);
            //Debug.Log(res.ServerUrl);
            //Debug.Log(res.PatchUrl);
            //Debug.Log(res.Message);
        }

        public void Process(MaintenanceCheckV2Req req, ErrorRes res)
        {
            maintenanceCheckV2Result(ErrorCode.NETWORK_ERROR, true, "contents", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null, null, null);
        }

        void Awake()
        {
        }

        void Update()
        {
            crossCheckSdataWaitingTime += Time.deltaTime;
            client.Process();
        }

        public void Request(Protocol protocol)
        {
            client.Request(protocol);
        }

        public void RequestKeyCount(string key)
        {
            KeyCountReq req = new KeyCountReq();
            req.Key = key;
            Request(req);
        }


        public void RequestCoupon(string couponId, CouponResult callback)
        {
            couponResult = callback;
            CouponReq req = new CouponReq();
            req.Coupon = couponId;
            Request(req);
        }

        public void Process(CouponReq req, CouponRes res)
        {
            couponResult(ErrorCode.SUCCESS, res.Result, res.Data);
        }

        public void Process(CouponReq req, ErrorRes res)
        {
            couponResult(ErrorCode.NETWORK_ERROR, res.Result, null);
        }
    }
}

using HaeginGame;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;
using System.Text;
using System;
#if UNITY_IOS
using SA.Foundation.Templates;
using SA.iOS.GameKit;
#elif UNITY_ANDROID
using SA.Android.GMS.Common;
using SA.Android.GMS.Auth;
using SA.Android.GMS.Drive;
using SA.Android.GMS.Games;
using SA.Android.App;
using SA.Android.App.View;
#elif UNITY_STANDALONE && USE_STEAM
using Steamworks;
#endif
using System.Runtime.InteropServices;

namespace Haegin
{
    public class Account
    {
        public static string webClientOAuth2ClientId = "551232432184-233chdikqqqj6sqsj3rihs7cq6ij4fm0.apps.googleusercontent.com";

        public enum HaeginAccountType
        {
            Guest,
            GooglePlayGameService,
            AppleGameCenter,
            Facebook,
            Steam,
            AppleId
        }

        public enum DialogType
        {
            Link,
            LinkOrNew,  // iOS only
            Select,
            MustLogout, // iOS only
            Logout,
            CannotLogin,
            LoginFromIOSSetting
        }
        
        public enum SelectButton { YES, NO };
        public delegate void OnSelect(SelectButton selectButton);
        public delegate void OpenSelectDialog(DialogType type, AccountType localAccountType, string localAccountName, string authAccountName, byte[] accountInfo, OnSelect callback);
        public delegate void OnLoggedIn(bool result, WebClient.AuthCode code, TimeSpan blockRemainTime, long blockSuid);
        public delegate void OnIsRegisteredTester(bool result, string adsId);

        private static string advertisingId;
        private static string accountId = null;
        public static HaeginAccountType GameServiceAccountType = HaeginAccountType.Guest;
        public static bool isGuestAccount = true;

        private static OnLoggedIn _onLoggedIn;
        private static OpenSelectDialog _openSelectDialog;

        public static bool IsGuestAccount()
        {
            return isGuestAccount;
        }

        private static string Encrypt(string text, string key)
        {
            try
            {
                byte[] textdata = Encoding.UTF8.GetBytes(text);
                byte[] keydata = Encoding.UTF8.GetBytes(key);
                uint[] k = G.Util.ConvertEx.ToKey(G.Util.Base62.ToBase62(keydata));
                G.Util.XXTea xxtea = new G.Util.XXTea(k);
                return xxtea.EncryptString(Convert.ToBase64String(textdata));
            }
            catch
            {
                return null;
            }
        }

        private static string Decrypt(string text, string key)
        {
            try
            {
                byte[] keydata = Encoding.UTF8.GetBytes(key);
                uint[] k = G.Util.ConvertEx.ToKey(G.Util.Base62.ToBase62(keydata));
                G.Util.XXTea xxtea = new G.Util.XXTea(k);
                return Encoding.UTF8.GetString(Convert.FromBase64String(xxtea.DecryptString(text)));
            }
            catch
            {
                return null;
            }
        }

        public static void IsRegisteredTester(OnIsRegisteredTester callback)
        {
            bool SupportAdsId = Application.RequestAdvertisingIdentifierAsync((string adId, bool trackingEnabled, string error) =>
            {
                callback(trackingEnabled, adId);
            });
            if (!SupportAdsId)
            {
                callback(false, "not supported");
            }
        }

        private static string TypedName(AccountType t, string name)
        {
            switch (t)
            {
                case AccountType.GooglePlay:
                    return "GooglePlay " + name;
                case AccountType.AppleGameCenter:
                    return "GameCenter " + name;
                case AccountType.Steam:
                    return "Steam " + name;
                case AccountType.Facebook:
                    return "Facebook " + name;
                case AccountType.AppleId:
                    return "AppleID " + name;
            }
            return name;
        }

#if UNITY_IOS
        static bool IsGameCenterLoginCancelled = false;
#endif
        public static void Initialize(string oauth2)
        {
            webClientOAuth2ClientId = oauth2;

#if UNITY_ANDROID
            SA.Android.AN_Settings.Instance.GooglePlay = true;
            SA.Android.AN_Settings.Instance.Vending = true;
            SA.Android.AN_Settings.Instance.Licensing = false;
#elif UNITY_IOS && !UNITY_EDITOR
            SignInWithAppleManager.GetInstance();
#endif
            if (!Application.RequestAdvertisingIdentifierAsync((string adId, bool trackingEnabled, string error) =>
                {
                    if (trackingEnabled)
                        advertisingId = adId;
                    else
                        advertisingId = "";
                    InitializeSub();
                }))
            {
                advertisingId = "";
                InitializeSub();
            }
        }

        public static void InitializeSub()
        {
#if UNITY_ANDROID || UNITY_IOS
            // Facebook
            if (FB.IsInitialized)
            {
                FB.ActivateApp();
            }
            else
            {
                FB.Init(
                    onInitComplete: () =>
                    {
                        FB.ActivateApp();
                    },
                    onHideUnity: isGameShown => { Time.timeScale = isGameShown ? 1 : 0; }
                );
            }
#endif

#if UNITY_STANDALONE && USE_STEAM
            SteamManager.Initialize();
#endif
            LoadAccountInfo();
        }



        public static void LoadAccountInfo()
        {
            string accountTypeString = PlayerPrefs.GetString("GameServiceAccountType", HaeginAccountType.Guest.ToString());
            GameServiceAccountType = (HaeginAccountType)System.Enum.Parse(typeof(HaeginAccountType), accountTypeString);
#if UNITY_EDITOR
            accountId = PlayerPrefs.GetString(ProjectSettings.editorAccountIdKey + "1", ProjectSettings.accountIdToCreate);
#else
            string encryptedAccountId = PlayerPrefs.GetString("AccountId0", ProjectSettings.accountIdToCreate);
            string decryptedAccountId = null;
            if (!ProjectSettings.accountIdToCreate.Equals(encryptedAccountId)) 
            {
                decryptedAccountId = Decrypt(encryptedAccountId, "5swdcsrBHoBYjDWi5VXN72");
            }
            if (decryptedAccountId != null)
            {
                accountId = decryptedAccountId;
            }
            else 
            {
                encryptedAccountId = PlayerPrefs.GetString("AccountId1", ProjectSettings.accountIdToCreate);

                if (!ProjectSettings.accountIdToCreate.Equals(encryptedAccountId))
                {
                    decryptedAccountId = Decrypt(encryptedAccountId, UnityEngine.SystemInfo.deviceUniqueIdentifier);

                    if (decryptedAccountId != null)
                    {
                        accountId = decryptedAccountId;
                        SaveAccountInfo();
                    }
                    else
                    {
                    if (!string.IsNullOrEmpty(advertisingId))
                        {
                            encryptedAccountId = PlayerPrefs.GetString("AccountId2", ProjectSettings.accountIdToCreate);
                            if (!ProjectSettings.accountIdToCreate.Equals(encryptedAccountId))
                            {
                                decryptedAccountId = Decrypt(encryptedAccountId, advertisingId);
                                if (decryptedAccountId != null)
                                {
                                    accountId = decryptedAccountId;
                                    SaveAccountInfo();
                                }
                                else
                                {
                                    accountId = ProjectSettings.accountIdToCreate;
                                }
                            }
                            else
                            {
                                accountId = ProjectSettings.accountIdToCreate;
                            }
                        }
                        else
                        {
                            accountId = ProjectSettings.accountIdToCreate;
                        }
                    }
                }
                else
                {
                    accountId = ProjectSettings.accountIdToCreate;
                }
            }
#endif

#if MDEBUG
            Debug.Log("Load AccountId [" + accountId + "]");
#endif
        }

        public static bool IsLoggedInGameService()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return AN_GoogleSignIn.GetLastSignedInAccount() != null;
#elif UNITY_IOS && !UNITY_EDITOR
            if (ISN_GKLocalPlayer.LocalPlayer != null)
                return ISN_GKLocalPlayer.LocalPlayer.Authenticated;
            else
                return false;
#elif UNITY_STANDALONE && USE_STEAM && !UNITY_EDITOR
            return SteamUser.BLoggedOn();
#else
            return false;
#endif
        }

        public static void LogoutGameService()
        {
#if UNITY_ANDROID
            AN_GoogleSignInOptions gso = new AN_GoogleSignInOptions.Builder(AN_GoogleSignInOptions.DEFAULT_GAMES_SIGN_IN).Build();
            AN_GoogleSignInClient client = AN_GoogleSignIn.GetClient(gso);
            client.SignOut(() => {});
#elif UNITY_IOS
            Debug.Log("Can't logout gamecenter");
#elif UNITY_STANDALONE && USE_STEAM

#endif
        }

        public static bool IsLoggedInFacebook()
        {
            return FB.IsLoggedIn;
        }

        public static void LogoutFacebook()
        {
            if(FB.IsLoggedIn)
                FB.LogOut();
        }

        public static bool IsLoggedInAppleId()
        {
            return SignInWithAppleManager.GetInstance().IsLoggedIn();
        }

        public static bool IsSupportedAppleId()
        {
            return SignInWithAppleManager.GetInstance().IsSupported();
        }

        public static void LogoutAppleId()
        {
            SignInWithAppleManager.GetInstance().Logout();
        }

        public static void SaveAccountInfo()
        {
            PlayerPrefs.SetString("GameServiceAccountType", GameServiceAccountType.ToString());
#if UNITY_EDITOR
            PlayerPrefs.SetString(ProjectSettings.editorAccountIdKey + "1", accountId);
#else
            PlayerPrefs.SetString("AccountId0", Encrypt(accountId, "5swdcsrBHoBYjDWi5VXN72"));
            PlayerPrefs.SetString("AccountId1", Encrypt(accountId, UnityEngine.SystemInfo.deviceUniqueIdentifier));
            if (!string.IsNullOrEmpty(advertisingId))
            {
                PlayerPrefs.SetString("AccountId2", Encrypt(accountId, advertisingId));
            }
#endif
            PlayerPrefs.Save();

#if MDEBUG
            Debug.Log("Save AccountId [" + accountId + "]");
#endif
        }

        public static void SetAccountId(string aId, HaeginAccountType gameServiceAccountType)
        {
            GameServiceAccountType = gameServiceAccountType;
            accountId = aId;
#if USE_APPGUARD && UNITY_IOS
            AppGuardApi.SetAppGuardUserID(aId);
#endif
            SaveAccountInfo();
        }

//#if CHECK_FACEBOOK_LOGOUT_AFTER_SELECT
        public static void SetFBLoginSelect()
        {
            PlayerPrefs.SetInt("FBLSF", 1);
            PlayerPrefs.Save();
        }

        public static void ResetFBLoginSelect()
        {
            PlayerPrefs.SetInt("FBLSF", 0);
            PlayerPrefs.Save();
        }

        public static bool CheckIfFBLogoutNeeded()
        {
            if(IsLoggedInFacebook() && PlayerPrefs.GetInt("FBLSF", 0) == 1)
            {
                LogoutFacebook();
                ResetFBLoginSelect();
                return false;
            }
            return true;
        }
//#endif

        public static void SetAppleIdLoginSelect()
        {
            PlayerPrefs.SetInt("SIWALSF", 1);
            PlayerPrefs.Save();
        }

        public static void ResetAppleIdLoginSelect()
        {
            PlayerPrefs.SetInt("SIWALSF", 0);
            PlayerPrefs.Save();
        }

        public static bool CheckIfAppleIdLogoutNeeded()
        {
            if (IsLoggedInAppleId() && PlayerPrefs.GetInt("SIWALSF", 0) == 1)
            {
                LogoutAppleId();
                ResetAppleIdLoginSelect();
                return false;
            }
            return true;
        }


        public static string GetAccountId()
        {
            LoadAccountInfo();
            return accountId;
        }

        public static void LoginAccount(HaeginAccountType accountType, OpenSelectDialog openSelectDialog, OnLoggedIn onLoggedIn, List<string> perms = null, bool checkPermission = false)
        {
            switch (accountType)
            {
                case HaeginAccountType.Guest:
                    if (CheckIfFBLogoutNeeded() && FB.IsLoggedIn)
                    {
                        LoginAccountFacebook(openSelectDialog, onLoggedIn, perms, checkPermission);
                    }
                    else if(CheckIfAppleIdLogoutNeeded() && IsLoggedInAppleId())
                    {
                        LoginAccountAppleId(openSelectDialog, onLoggedIn);
                    }
                    else
                    {
                        LoginAccountGuest(onLoggedIn);
                    }
                    break;
#if UNITY_ANDROID
                case HaeginAccountType.GooglePlayGameService:
                    LoginAccountGooglePlayGameService(openSelectDialog, onLoggedIn);
                    break;
#endif
#if UNITY_IOS
                case HaeginAccountType.AppleGameCenter:
                    LoginAccountAppleGameCenter(openSelectDialog, onLoggedIn);
                    break;
#endif
                case HaeginAccountType.Facebook:
                    LoginAccountFacebook(openSelectDialog, onLoggedIn, perms, checkPermission);
                    break;
#if UNITY_STANDALONE && USE_STEAM
                case HaeginAccountType.Steam:
#if MDEBUG
                    Debug.Log("Trying Staem Login");
#endif
                    LoginAccountSteam(openSelectDialog, onLoggedIn);
                    break;
#endif
                case HaeginAccountType.AppleId:
                    LoginAccountAppleId(openSelectDialog, onLoggedIn);
                    break;
            }
        }

        public static void LoginAccountGuest(OnLoggedIn onLoggedIn)
        {
            WebClient.GetInstance().RequestAuth((WebClient.ErrorCode error, WebClient.AuthCode code, AccountType accountType, TimeSpan blockRemainTime, long blockSuid) =>
            {
                if (error == WebClient.ErrorCode.SUCCESS)
                {
                    isGuestAccount = (accountType == AccountType.None);
                    switch (code)
                    {
                        case WebClient.AuthCode.SUCCESS:
                            onLoggedIn(true, code, blockRemainTime, blockSuid);
                            break;
                        default:// WebClient.AuthCode.FAILED:
                            // 로그인 실패 처리
                            onLoggedIn(false, code, blockRemainTime, blockSuid);
                            break;
                    }
                }
                else
                {
                    // 네트워크 에러 처리
                    onLoggedIn(false, WebClient.AuthCode.FAILED, blockRemainTime, blockSuid);
                }
            });
        }

        public static void LoginAccountAppleId(OpenSelectDialog openSelectDialog, OnLoggedIn onLoggedIn, bool forceRelogin = false)
        {
            if(SignInWithAppleManager.GetInstance().IsSupported())
            {
                SignInWithAppleManager.GetInstance().Login((bool result, string identityToken, string authCode, string appleId, string name, string email) =>
                {
                    if (result)
                    {
#if MDEBUG
                        Debug.Log("-------------------------------------------------------------------");
                        Debug.Log("Unity AppleId : [" + appleId + "]");
                        Debug.Log("Unity Email : [" + email + "]");
                        Debug.Log("Unity IdentityToken : [" + identityToken + "]");
                        Debug.Log("Unity AuthCode : [" + authCode + "]");
                        Debug.Log("-------------------------------------------------------------------");
#endif
                        if(identityToken == null || authCode == null)
                        {
                            // 이 경우는 재로그인인 경우로 그냥 Guest 로그인 한다.
                            WebClient.GetInstance().RequestAuth((WebClient.ErrorCode error, WebClient.AuthCode code, AccountType accountType, TimeSpan blockRemainTime, long blockSuid) => {
                                if (error == WebClient.ErrorCode.SUCCESS)
                                {
                                    switch (code)
                                    {
                                        case WebClient.AuthCode.SUCCESS:
                                            onLoggedIn(true, code, blockRemainTime, blockSuid);
                                            break;
                                        default:// WebClient.AuthCode.FAILED:
                                            // 이 경우는 다른 단말기에서 select 상태에서 기존계정을 지운 경우입니다.
                                            // 이 경우는 애플로그인을 다시 하여, identityToken, authcode를 받아오게 합니다.
#if MDEBUG
                                            Debug.Log("GuestAppleId AuthCode : " + code);
#endif
                                            ThreadSafeDispatcher.Instance.Invoke(() => {
                                                LoginAccountAppleId(openSelectDialog, onLoggedIn, true);
                                            });
                                            break;
                                    }
                                }
                                else
                                {
                                    // 네트워크 에러 처리
                                    onLoggedIn(false, WebClient.AuthCode.FAILED, blockRemainTime, blockSuid);
                                }
                            });
                            return;
                        }

                        WebClient.GetInstance().RequestAppleIdAuth(appleId, email, identityToken, authCode, Account.GetAccountId(), name, LinkOption.None, (WebClient.ErrorCode error, WebClient.AuthCode code, string accountId1, AccountType localAccountType, string localAccountName, byte[] accountInfo, TimeSpan blockRemainTime, long blockSuid) =>
                        {
                            // 다시 쓰면 안되니까, 리셋한다.
                            // identityToken = null;
                            // authCode = null;

                            if (error == WebClient.ErrorCode.SUCCESS)
                            {
                                if (code == WebClient.AuthCode.SUCCESS)
                                {
                                    isGuestAccount = false;
                                    // AppleId에 연동된 계정으로 로그인 성공
                                    Account.SetAccountId(accountId1, Account.GameServiceAccountType);
                                    onLoggedIn(true, code, blockRemainTime, blockSuid);
                                }
                                else if (code == WebClient.AuthCode.NEED_TO_LINK)
                                {
                                    // 기존 소셜 계정과 새로운 소셜 계정에 연결이 필요한 경우
                                    // 구글에 연동된 예전 게임 데이타가 없음.
                                    openSelectDialog(DialogType.Link, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.AppleId, name), accountInfo, selectButton =>
                                    {
                                        if (selectButton == SelectButton.YES)
                                        {
                                            WebClient.GetInstance().RequestAppleIdAuth(appleId, email, identityToken, authCode, Account.GetAccountId(), name, LinkOption.Link, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                            {
                                                if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                                {
                                                    isGuestAccount = false;
                                                    Account.SetAccountId(accountId2, Account.GameServiceAccountType);
                                                    onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                }
                                                else
                                                {
                                                    onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                }
                                            });
                                        }
                                        else
                                        {
                                            // 로그인을 취소함.
                                            LogoutAppleId();
                                            onLoggedIn(false, WebClient.AuthCode.Cancel, TimeSpan.Zero, 0);
                                        }
                                    });
                                }
                                else if (code == WebClient.AuthCode.NEED_TO_SELECT)
                                {
                                    // Facebook에 연동된 예전 게임 데이타가 있음
                                    // 이 경우에는 사용자에게 어느 계정을 사용할 것인지 물어봐야함
                                    SetAppleIdLoginSelect();
                                    openSelectDialog(DialogType.Select, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Facebook, name), accountInfo, selectButton =>
                                    {
                                        ResetAppleIdLoginSelect();
                                        if (selectButton == SelectButton.YES)
                                        {
                                            WebClient.GetInstance().RequestAppleIdAuth(appleId, email, identityToken, authCode, Account.GetAccountId(), name, LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                            {
                                                if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                                {
                                                    isGuestAccount = false;
                                                    Account.SetAccountId(accountId2, Account.GameServiceAccountType);
                                                    onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                }
                                                else
                                                {
                                                    onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                }
                                            });
                                        }
                                        else
                                        {
                                            WebClient.GetInstance().RequestAppleIdAuth(appleId, email, identityToken, authCode, Account.GetAccountId(), name, LinkOption.Local, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                            {
                                                if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                                {
                                                    isGuestAccount = false;
                                                    Account.SetAccountId(accountId2, Account.GameServiceAccountType);
                                                    onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                }
                                                else
                                                {
                                                    onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                }
                                            });
                                        }
                                    });
                                }
                                else if (code == WebClient.AuthCode.NEED_TO_LOGOUT)
                                {
#if UNITY_ANDROID
                            // Android의 경우 
                            openSelectDialog(DialogType.Logout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.AppleId, name), accountInfo, selectButton =>
                            {
                                if (selectButton == SelectButton.YES)
                                {
                                    // Google Play 계정 로그아웃 
                                    if (IsLoggedInGameService())
                                    {
                                        LogoutGameService();
                                    }
                                    // Facebook 계정 로그아웃
                                    if(IsLoggedInFacebook())
                                    {
                                        LogoutFacebook();
                                    }
                                    WebClient.GetInstance().RequestAppleIdAuth(appleId, email, identityToken, authCode, Account.GetAccountId(), name, LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                    {
                                        if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                        {
                                            isGuestAccount = false;
                                            Account.SetAccountId(accountId2, Account.HaeginAccountType.Guest);
                                            onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                        }
                                        else
                                        {
                                            onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                        }
                                    });
                                }
                                else
                                {
                                    // AppleId 계정 로그아웃
                                    if (IsLoggedInAppleId())
                                    {
                                        LogoutAppleId();
                                    }
                                    onLoggedIn(false, code, blockRemainTime, blockSuid);
                                }
                            });
#elif UNITY_IOS
                                    if (IsLoggedInGameService())
                                    {
                                        // iOS의 경우 GameCenter 계정은 로그아웃 할 수 없으므로, 
                                        // AppleId 로그아웃하고, facebook 로그인 불가능하다고 알려줌 
                                        LogoutAppleId();
                                        openSelectDialog(DialogType.CannotLogin, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.AppleId, name), accountInfo, selectButton =>
                                        {
                                            onLoggedIn(false, code, blockRemainTime, blockSuid);
                                        });
                                    }
                                    else
                                    {
                                        openSelectDialog(DialogType.Logout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.AppleId, name), accountInfo, selectButton =>
                                        {
                                            if (selectButton == SelectButton.YES)
                                            {
                                                // Facebook 계정 로그아웃
                                                if (IsLoggedInFacebook())
                                                {
                                                    LogoutFacebook();
                                                }
                                                WebClient.GetInstance().RequestAppleIdAuth(appleId, email, identityToken, authCode, Account.GetAccountId(), name, LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                                {
                                                    isGuestAccount = false;
                                                    Account.SetAccountId(accountId2, Account.HaeginAccountType.Guest);
                                                    onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                });
                                            }
                                            else
                                            {
                                                // AppleId 계정 로그아웃
                                                if (IsLoggedInAppleId())
                                                {
                                                    LogoutAppleId();
                                                }
                                                onLoggedIn(false, code, blockRemainTime, blockSuid);
                                            }
                                        });
                                    }
#endif
                                }
                                else if (code == WebClient.AuthCode.NEED_TO_LOGOUT_CLEAR)
                                {
#if UNITY_ANDROID
                            // Android의 경우 
                            openSelectDialog(DialogType.Logout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.AppleId, name), accountInfo, selectButton =>
                            {
                                if (selectButton == SelectButton.YES)
                                {
                                    // Google Play 계정 로그아웃 
                                    if (IsLoggedInGameService())
                                    {
                                        LogoutGameService();
                                    }
                                    // Facebook 계정 로그아웃
                                    if (IsLoggedInFacebook())
                                    {
                                        LogoutFacebook();
                                    }
                                    SetAccountId(ProjectSettings.accountIdToCreate, Account.HaeginAccountType.Guest);
                                    WebClient.GetInstance().RequestAuth((WebClient.ErrorCode error3, WebClient.AuthCode code3, AccountType accountType3, TimeSpan blockRemainTime3, long blockSuid3) =>
                                    {
                                        if (error3 == WebClient.ErrorCode.SUCCESS)
                                        {
                                            isGuestAccount = (accountType3 == AccountType.None);

                                            switch (code3)
                                            {
                                                case WebClient.AuthCode.SUCCESS:
                                                    WebClient.GetInstance().RequestAppleIdAuth(appleId, email, identityToken, authCode, Account.GetAccountId(), name, LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                                    {
                                                        if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                                        {
                                                            isGuestAccount = false;
                                                            Account.SetAccountId(accountId2, Account.HaeginAccountType.Guest);
                                                            onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                        }
                                                        else
                                                        {
                                                            onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                        }
                                                    });
                                                    break;
                                                default://case WebClient.AuthCode.FAILED:
                                                    // 로그인 실패 처리
                                                    LogoutAppleId();
                                                    onLoggedIn(false, code3, blockRemainTime3, blockSuid3);
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            // 네트워크 에러 처리
                                            onLoggedIn(false, WebClient.AuthCode.FAILED, blockRemainTime3, blockSuid3);
                                        }
                                    });
                                }
                                else
                                {
                                    // AppleId 계정 로그아웃
                                    if (IsLoggedInAppleId())
                                    {
                                        LogoutAppleId();
                                    }
                                    onLoggedIn(false, code, blockRemainTime, blockSuid);
                                }
                            });
#elif UNITY_IOS
                                    if (IsLoggedInGameService())
                                    {
                                        // iOS의 경우 GameCenter 계정은 로그아웃 할 수 없으므로, 
                                        // AppleId 로그아웃하고, AppleId 로그인 불가능하다고 알려줌 
                                        LogoutAppleId();
                                        openSelectDialog(DialogType.CannotLogin, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.AppleId, name), accountInfo, selectButton =>
                                        {
                                            onLoggedIn(false, code, blockRemainTime, blockSuid);
                                        });
                                    }
                                    else
                                    {
                                        openSelectDialog(DialogType.Logout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.AppleId, name), accountInfo, selectButton =>
                                        {
                                            if (selectButton == SelectButton.YES)
                                            {
                                                // Facebook 계정 로그아웃
                                                if (IsLoggedInFacebook())
                                                {
                                                    LogoutFacebook();
                                                }
                                                SetAccountId(ProjectSettings.accountIdToCreate, Account.HaeginAccountType.Guest);
                                                WebClient.GetInstance().RequestAuth((WebClient.ErrorCode error3, WebClient.AuthCode code3, AccountType accountType3, TimeSpan blockRemainTime3, long blockSuid3) =>
                                                {
                                                    if (error3 == WebClient.ErrorCode.SUCCESS)
                                                    {
                                                        isGuestAccount = (accountType3 == AccountType.None);
                                                        switch (code3)
                                                        {
                                                            case WebClient.AuthCode.SUCCESS:
                                                                WebClient.GetInstance().RequestAppleIdAuth(appleId, email, identityToken, authCode, Account.GetAccountId(), name, LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                                                {
                                                                    isGuestAccount = false;
                                                                    Account.SetAccountId(accountId2, Account.HaeginAccountType.Guest);
                                                                    onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                                });
                                                                break;
                                                            default://case WebClient.AuthCode.FAILED:
                                                                // 로그인 실패 처리
                                                                // AppleId 계정 로그아웃
                                                                LogoutAppleId();
                                                                onLoggedIn(false, code3, blockRemainTime3, blockSuid3);
                                                                break;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        LogoutAppleId();
                                                        // 네트워크 에러 처리
                                                        onLoggedIn(false, WebClient.AuthCode.FAILED, blockRemainTime3, blockSuid3);
                                                    }
                                                });
                                            }
                                            else
                                            {
                                                // AppleId 계정 로그아웃
                                                LogoutAppleId();
                                                onLoggedIn(false, code, blockRemainTime, blockSuid);
                                            }
                                        });
                                    }
#endif
                                }
                                else
                                {
                                    LogoutAppleId();
                                    onLoggedIn(false, code, blockRemainTime, blockSuid);
                                }
                            }
                            else
                            {
                                LogoutAppleId();
                                onLoggedIn(false, code, blockRemainTime, blockSuid);
                            }
                        });
                    }
                    else
                    {
                        // AppleId 계정 로그아웃
                        if(!forceRelogin)
                        {
                            LogoutAppleId();
                        }
                        onLoggedIn(false, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                    }
                }, forceRelogin);
            }
            else
            {
                // AppleId 계정 로그아웃
                LogoutAppleId();
                onLoggedIn(false, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
            }
        }

#if UNITY_STANDALONE && USE_STEAM
        public static byte[] GetSteamAuthSessionTicket()
        {
            byte[] sessionTicketBuf = new byte[1024];
            uint pcbTicket = 0;
            HAuthTicket ticket = SteamUser.GetAuthSessionTicket(sessionTicketBuf, 1024, out pcbTicket);
            if (pcbTicket > 0)
            {
                byte[] sessionTicket = new byte[pcbTicket];
                for (int i = 0; i < pcbTicket; i++) sessionTicket[i] = sessionTicketBuf[i];
                return sessionTicket;
            }
            else
            {
                return null;
            }
        }

        public static void LoginAccountSteam(OpenSelectDialog openSelectDialog, OnLoggedIn onLoggedIn)
        {
            WebClient.GetInstance().RequestSteamAuth(SteamUser.GetSteamID().m_SteamID, GetSteamAuthSessionTicket(), Account.GetAccountId(), SteamFriends.GetPersonaName(), LinkOption.None, (WebClient.ErrorCode error, WebClient.AuthCode code, string accountId1, AccountType localAccountType, string localAccountName, byte[] accountInfo, TimeSpan blockRemainTime, long blockSuid) =>
            {
                if (error == WebClient.ErrorCode.SUCCESS)
                {
                    if (code == WebClient.AuthCode.SUCCESS)
                    {
                        isGuestAccount = false;
                        // Steam에 연동된 계정으로 로그인 성공
                        Account.SetAccountId(accountId1, Account.HaeginAccountType.Steam);
                        onLoggedIn(true, code, blockRemainTime, blockSuid);
                    }
                    else if (code == WebClient.AuthCode.NEED_TO_LINK)
                    {
                        // 기존 소셜 연동된 계정 상태에서 새롭게 게임 계정과 연동되지 않은 Steam 계정 로그인시
                        openSelectDialog(DialogType.LinkOrNew, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Steam, SteamFriends.GetPersonaName()), accountInfo, selectButton =>
                        {
                            if (selectButton == SelectButton.YES)
                            {
                                // 기존 소셜 계정과 Steam 계정을 연동
                                WebClient.GetInstance().RequestSteamAuth(SteamUser.GetSteamID().m_SteamID, GetSteamAuthSessionTicket(), Account.GetAccountId(), SteamFriends.GetPersonaName(), LinkOption.Link, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                {
                                    if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                    {
                                        isGuestAccount = false;
                                        Account.SetAccountId(accountId2, Account.HaeginAccountType.Steam);
                                        onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                    }
                                    else
                                    {
                                        onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                    }
                                });
                            }
                            else
                            {
                                // Steam 계정은 로그아웃 불가능하니, 싫다고 하면 기존 계정 로그아웃. Steam 계정에 연동되는 새 계정 만들어서 연동
                                if (localAccountType == AccountType.Facebook)
                                {
                                    FB.LogOut();
                                }
                                WebClient.GetInstance().RequestSteamAuth(SteamUser.GetSteamID().m_SteamID, GetSteamAuthSessionTicket(), Account.GetAccountId(), SteamFriends.GetPersonaName(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                {
                                    if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                    {
                                        isGuestAccount = false;
                                        Account.SetAccountId(accountId2, Account.HaeginAccountType.Steam);
                                        onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                    }
                                    else
                                    {
                                        onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                    }
                                });
                            }
                        });
                    }
                    else if (code == WebClient.AuthCode.NEED_TO_SELECT)
                    {
                        // Steam에 연동된 예전 게임 데이타가 있음
                        // 이 경우에는 사용자에게 어느 계정을 사용할 것인지 물어봐야함
                        openSelectDialog(DialogType.Select, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Steam, SteamFriends.GetPersonaName()), accountInfo, selectButton =>
                        {
                            if (selectButton == SelectButton.YES)
                            {
                                WebClient.GetInstance().RequestSteamAuth(SteamUser.GetSteamID().m_SteamID, GetSteamAuthSessionTicket(), Account.GetAccountId(), SteamFriends.GetPersonaName(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                {
                                    if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                    {
                                        isGuestAccount = false;
                                        Account.SetAccountId(accountId2, Account.HaeginAccountType.Steam);
                                        onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                    }
                                    else
                                    {
                                        onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                    }
                                });
                            }
                            else
                            {
                                WebClient.GetInstance().RequestSteamAuth(SteamUser.GetSteamID().m_SteamID, GetSteamAuthSessionTicket(), Account.GetAccountId(), SteamFriends.GetPersonaName(), LinkOption.Local, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                {
                                    if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                    {
                                        isGuestAccount = false;
                                        Account.SetAccountId(accountId2, Account.HaeginAccountType.Steam);
                                        onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                    }
                                    else
                                    {
                                        onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                    }
                                });
                            }
                        });
                    }
                    else if (code == WebClient.AuthCode.NEED_TO_LOGOUT)
                    {
                        openSelectDialog(DialogType.MustLogout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Steam, SteamFriends.GetPersonaName()), accountInfo, selectButton =>
                        {
                            // Steam 계정은 로그아웃 불가능하니, 기존 계정 로그아웃.
                            // Steam 연동된 계정 불러오기.
                            if (FB.IsLoggedIn)
                            {
                                FB.LogOut();
                            }
                            if (IsLoggedInAppleId())
                            {
                                LogoutAppleId();
                            }
                            WebClient.GetInstance().RequestSteamAuth(SteamUser.GetSteamID().m_SteamID, GetSteamAuthSessionTicket(), Account.GetAccountId(), SteamFriends.GetPersonaName(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                            {
                                if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                {
                                    isGuestAccount = false;
                                    Account.SetAccountId(accountId2, Account.HaeginAccountType.Steam);
                                    onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                }
                                else
                                {
                                    onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                }
                            });
                        });
                    }
                    else if (code == WebClient.AuthCode.NEED_TO_LOGOUT_CLEAR)
                    {
                        openSelectDialog(DialogType.MustLogout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Steam, SteamFriends.GetPersonaName()), accountInfo, selectButton =>
                        {
                            // Steam 계정은 로그아웃 불가능하니, 기존 계정 로그아웃.
                            // Steam 연동된 계정 불러오기.
                            if (FB.IsLoggedIn)
                            {
                                FB.LogOut();
                            }
                            if (IsLoggedInAppleId())
                            {
                                LogoutAppleId();
                            }
                            SetAccountId(ProjectSettings.accountIdToCreate, Account.HaeginAccountType.Guest);
                            WebClient.GetInstance().RequestAuth((WebClient.ErrorCode error3, WebClient.AuthCode code3, AccountType accountType3, TimeSpan blockRemainTime3, long blockSuid3) => {
                                if (error3 == WebClient.ErrorCode.SUCCESS)
                                {
                                    isGuestAccount = (accountType3 == AccountType.None);
                                    switch (code3)
                                    {
                                        case WebClient.AuthCode.SUCCESS:
                                            WebClient.GetInstance().RequestSteamAuth(SteamUser.GetSteamID().m_SteamID, GetSteamAuthSessionTicket(), Account.GetAccountId(), SteamFriends.GetPersonaName(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                            {
                                                isGuestAccount = false;
                                                Account.SetAccountId(accountId2, Account.HaeginAccountType.Steam);
                                                onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                            });
                                            break;
                                        default://case WebClient.AuthCode.FAILED:
                                            // 로그인 실패 처리
                                            onLoggedIn(false, code3, blockRemainTime3, blockSuid3);
                                            break;
                                     }
                                 }
                                 else
                                 {
                                      // 네트워크 에러 처리
                                      onLoggedIn(false, WebClient.AuthCode.FAILED, blockRemainTime3, blockSuid3);
                                 }
                            });
                        });
                    }
                    else
                    {
                        onLoggedIn(false, code, blockRemainTime, blockSuid);
                    }
                }
                else
                {
                    onLoggedIn(false, code, blockRemainTime, blockSuid);
                }
            });
        }
#endif
        static void LoginAccountFacebookSub(OpenSelectDialog openSelectDialog, OnLoggedIn onLoggedIn)
        {
            FB.API("/me?fields=name", HttpMethod.POST, (IGraphResult result2) =>
            {
                if (result2.Error != null)
                {
#if MDEBUG
                    Debug.Log(result2.Error);
#endif
                    FB.LogOut();
                    onLoggedIn(false, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                    return;
                }
                string name = null;
                try
                {
                    name = result2.ResultDictionary["name"].ToString();
                }
                catch (Exception e)
                {
#if MDEBUG
                    Debug.Log(e.Message);
                    Debug.Log(e.StackTrace);
#endif
                    FB.LogOut();
                    onLoggedIn(false, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                    return;
                }
                WebClient.GetInstance().RequestFacebookAuth(long.Parse(AccessToken.CurrentAccessToken.UserId), AccessToken.CurrentAccessToken.TokenString, Account.GetAccountId(), name, LinkOption.None, (WebClient.ErrorCode error, WebClient.AuthCode code, string accountId1, AccountType localAccountType, string localAccountName, byte[] accountInfo, TimeSpan blockRemainTime, long blockSuid) =>
                {
                    if (error == WebClient.ErrorCode.SUCCESS)
                    {
                        if (code == WebClient.AuthCode.SUCCESS)
                        {
                            isGuestAccount = false;
                            // Facebook에 연동된 계정으로 로그인 성공
                            Account.SetAccountId(accountId1, Account.GameServiceAccountType);
                            onLoggedIn(true, code, blockRemainTime, blockSuid);
                        }
                        else if (code == WebClient.AuthCode.NEED_TO_LINK)
                        {
                            // 기존 소셜 계정과 새로운 소셜 계정에 연결이 필요한 경우
                            // 구글에 연동된 예전 게임 데이타가 없음.
                            openSelectDialog(DialogType.Link, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Facebook, name), accountInfo, selectButton =>
                            {
                                if (selectButton == SelectButton.YES)
                                {
                                    WebClient.GetInstance().RequestFacebookAuth(long.Parse(AccessToken.CurrentAccessToken.UserId), AccessToken.CurrentAccessToken.TokenString, Account.GetAccountId(), name, LinkOption.Link, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                    {
                                        if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                        {
                                            isGuestAccount = false;
                                            Account.SetAccountId(accountId2, Account.GameServiceAccountType);
                                            onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                        }
                                        else
                                        {
                                            onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                        }
                                    });
                                }
                                else
                                {
                                    // 로그인을 취소한. 
                                    FB.LogOut();
                                    onLoggedIn(false, WebClient.AuthCode.Cancel, TimeSpan.Zero, 0);
                                }
                            });
                        }
                        else if (code == WebClient.AuthCode.NEED_TO_SELECT)
                        {
                            // Facebook에 연동된 예전 게임 데이타가 있음
                            // 이 경우에는 사용자에게 어느 계정을 사용할 것인지 물어봐야함
//#if CHECK_FACEBOOK_LOGOUT_AFTER_SELECT
                            SetFBLoginSelect();
//#endif
                            openSelectDialog(DialogType.Select, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Facebook, name), accountInfo, selectButton =>
                            {
//#if CHECK_FACEBOOK_LOGOUT_AFTER_SELECT
                                ResetFBLoginSelect();
//#endif
                                if (selectButton == SelectButton.YES)
                                {
                                    WebClient.GetInstance().RequestFacebookAuth(long.Parse(AccessToken.CurrentAccessToken.UserId), AccessToken.CurrentAccessToken.TokenString, Account.GetAccountId(), name, LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                    {
                                        if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                        {
                                            isGuestAccount = false;
                                            Account.SetAccountId(accountId2, Account.GameServiceAccountType);
                                            onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                        }
                                        else
                                        {
                                            onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                        }
                                    });
                                }
                                else
                                {
                                    WebClient.GetInstance().RequestFacebookAuth(long.Parse(AccessToken.CurrentAccessToken.UserId), AccessToken.CurrentAccessToken.TokenString, Account.GetAccountId(), name, LinkOption.Local, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                    {
                                        if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                        {
                                            isGuestAccount = false;
                                            Account.SetAccountId(accountId2, Account.GameServiceAccountType);
                                            onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                        }
                                        else
                                        {
                                            onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                        }
                                    });
                                }
                            });
                        }
                        else if (code == WebClient.AuthCode.NEED_TO_LOGOUT)
                        {
#if UNITY_ANDROID
                            // Android의 경우 
                            openSelectDialog(DialogType.Logout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Facebook, name), accountInfo, selectButton =>
                            {
                                if (selectButton == SelectButton.YES)
                                {
                                    // Google Play 계정 로그아웃 
                                    if (IsLoggedInGameService())
                                    {
                                        LogoutGameService();
                                    }
                                    if (IsLoggedInAppleId())
                                    {
                                        LogoutAppleId();
                                    }
                                    WebClient.GetInstance().RequestFacebookAuth(long.Parse(AccessToken.CurrentAccessToken.UserId), AccessToken.CurrentAccessToken.TokenString, Account.GetAccountId(), name, LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                    {
                                        if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                        {
                                            isGuestAccount = false;
                                            Account.SetAccountId(accountId2, Account.HaeginAccountType.Guest);
                                            onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                        }
                                        else
                                        {
                                            onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                        }
                                    });
                                }
                                else
                                {
                                    // Facebook 계정 로그아웃
                                    if (FB.IsLoggedIn)
                                    {
                                        FB.LogOut();
                                    }
                                    onLoggedIn(false, code, blockRemainTime, blockSuid);
                                }
                            });
#elif UNITY_IOS
                            if (IsLoggedInGameService()) 
                            {
                                // iOS의 경우 GameCenter 계정은 로그아웃 할 수 없으므로, 
                                // Facebook 로그아웃하고, facebook 로그인 불가능하다고 알려줌 
                                FB.LogOut();
                                openSelectDialog(DialogType.CannotLogin, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Facebook, name), accountInfo, selectButton =>
                                {
                                    onLoggedIn(false, code, blockRemainTime, blockSuid);
                                });
                            }
                            else {
                                openSelectDialog(DialogType.Logout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Facebook, name), accountInfo, selectButton =>
                                {
                                    if (selectButton == SelectButton.YES)
                                    {
                                        if (IsLoggedInAppleId())
                                        {
                                            LogoutAppleId();
                                        }
                                        WebClient.GetInstance().RequestFacebookAuth(long.Parse(AccessToken.CurrentAccessToken.UserId), AccessToken.CurrentAccessToken.TokenString, Account.GetAccountId(), name, LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                        {
                                            isGuestAccount = false;
                                            Account.SetAccountId(accountId2, Account.HaeginAccountType.Guest);
                                            onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                        });
                                    }
                                    else
                                    {
                                        // Facebook 계정 로그아웃
                                        if (FB.IsLoggedIn)
                                        {
                                            FB.LogOut();
                                        }
                                        onLoggedIn(false, code, blockRemainTime, blockSuid);
                                    }
                                });
                            }
#endif
                        }
                        else if (code == WebClient.AuthCode.NEED_TO_LOGOUT_CLEAR)
                        {
#if UNITY_ANDROID
                            // Android의 경우 
                            openSelectDialog(DialogType.Logout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Facebook, name), accountInfo, selectButton =>
                            {
                                if (selectButton == SelectButton.YES)
                                {
                                    // Google Play 계정 로그아웃 
                                    if (IsLoggedInGameService())
                                    {
                                        LogoutGameService();
                                    }
                                    if (IsLoggedInAppleId())
                                    {
                                        LogoutAppleId();
                                    }
                                    SetAccountId(ProjectSettings.accountIdToCreate, Account.HaeginAccountType.Guest);
                                    WebClient.GetInstance().RequestAuth((WebClient.ErrorCode error3, WebClient.AuthCode code3, AccountType accountType3, TimeSpan blockRemainTime3, long blockSuid3) =>
                                    {
                                        if (error3 == WebClient.ErrorCode.SUCCESS)
                                        {
                                            isGuestAccount = (accountType3 == AccountType.None);

                                            switch (code3)
                                            {
                                                case WebClient.AuthCode.SUCCESS:
                                                    WebClient.GetInstance().RequestFacebookAuth(long.Parse(AccessToken.CurrentAccessToken.UserId), AccessToken.CurrentAccessToken.TokenString, Account.GetAccountId(), name, LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                                    {
                                                        if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                                        {
                                                            isGuestAccount = false;
                                                            Account.SetAccountId(accountId2, Account.HaeginAccountType.Guest);
                                                            onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                        }
                                                        else
                                                        {
                                                            onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                        }
                                                    });
                                                    break;
                                                default://case WebClient.AuthCode.FAILED:
                                                    // 로그인 실패 처리
                                                    onLoggedIn(false, code3, blockRemainTime3, blockSuid3);
                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            // 네트워크 에러 처리
                                            onLoggedIn(false, WebClient.AuthCode.FAILED, blockRemainTime3, blockSuid3);
                                        }
                                    });
                                }
                                else
                                {
                                    // Facebook 계정 로그아웃
                                    if (FB.IsLoggedIn)
                                    {
                                        FB.LogOut();
                                    }
                                    onLoggedIn(false, code, blockRemainTime, blockSuid);
                                }
                            });
#elif UNITY_IOS
                            if (IsLoggedInGameService())
                            {
                                // iOS의 경우 GameCenter 계정은 로그아웃 할 수 없으므로, 
                                // Facebook 로그아웃하고, facebook 로그인 불가능하다고 알려줌 
                                FB.LogOut();
                                openSelectDialog(DialogType.CannotLogin, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Facebook, name), accountInfo, selectButton =>
                                {
                                    onLoggedIn(false, code, blockRemainTime, blockSuid);
                                });
                            }
                            else
                            {
                                openSelectDialog(DialogType.Logout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.Facebook, name), accountInfo, selectButton =>
                                {
                                    if (selectButton == SelectButton.YES)
                                    {
                                        if (IsLoggedInAppleId())
                                        {
                                            LogoutAppleId();
                                        }
                                        SetAccountId(ProjectSettings.accountIdToCreate, Account.HaeginAccountType.Guest);
                                        WebClient.GetInstance().RequestAuth((WebClient.ErrorCode error3, WebClient.AuthCode code3, AccountType accountType3, TimeSpan blockRemainTime3, long blockSuid3) =>
                                        {
                                            if (error3 == WebClient.ErrorCode.SUCCESS)
                                            {
                                                isGuestAccount = (accountType3 == AccountType.None);
                                                switch (code3)
                                                {
                                                    case WebClient.AuthCode.SUCCESS:
                                                        WebClient.GetInstance().RequestFacebookAuth(long.Parse(AccessToken.CurrentAccessToken.UserId), AccessToken.CurrentAccessToken.TokenString, Account.GetAccountId(), name, LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                                        {
                                                            isGuestAccount = false;
                                                            Account.SetAccountId(accountId2, Account.HaeginAccountType.Guest);
                                                            onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                        });
                                                        break;
                                                    default://case WebClient.AuthCode.FAILED:
                                                        // 로그인 실패 처리
                                                        onLoggedIn(false, code3, blockRemainTime3, blockSuid3);
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                // 네트워크 에러 처리
                                                onLoggedIn(false, WebClient.AuthCode.FAILED, blockRemainTime3, blockSuid3);
                                            }
                                        });
                                    }
                                    else
                                    {
                                        // Facebook 계정 로그아웃
                                        if (FB.IsLoggedIn)
                                        {
                                            FB.LogOut();
                                        }
                                        onLoggedIn(false, code, blockRemainTime, blockSuid);
                                    }
                                });
                            }
#endif
                        }
                        else
                        {
                            onLoggedIn(false, code, blockRemainTime, blockSuid);
                        }
                    }
                    else
                    {
                        onLoggedIn(false, code, blockRemainTime, blockSuid);
                    }
                });
            });
        }

        public static bool IsFBPermissionGranted(string permission)
        {
            if (FB.IsLoggedIn)
            {
                foreach (string p in AccessToken.CurrentAccessToken.Permissions)
                {
                    if (p.Equals(permission))
                    {
#if MDEBUG
                        Debug.Log("Granted FACEBOOK Permissions : " + permission);
#endif
                        return true;
                    }
                }
            }
#if MDEBUG
            Debug.Log("Not yet granted FACEBOOK Permissions : " + permission);
#endif
            return false;
        }


        public static void LoginAccountFacebook(OpenSelectDialog openSelectDialog, OnLoggedIn onLoggedIn, List<string> perms = null, bool checkPermission = false)
        {
            if(perms == null)
            {
                perms = new List<string> { "public_profile", "user_friends" };
            }

            bool isLoggedIn = false;
            if(FB.IsLoggedIn)
            {
                isLoggedIn = true;
                if(checkPermission)
                {
                    foreach (string p in perms)
                    {
                        if (!IsFBPermissionGranted(p))
                        {
                            isLoggedIn = false;
                            break;
                        }
                    }
                }
            }

            if (isLoggedIn)
            {
                LoginAccountFacebookSub(openSelectDialog, onLoggedIn);
            }
            else 
            {
                FB.LogInWithReadPermissions(perms, callback: result =>
                {
                    if (result.Cancelled)
                    {
                        onLoggedIn(false, WebClient.AuthCode.Cancel, TimeSpan.Zero, 0);
                    }
                    else if (FB.IsLoggedIn)
                    {
                        LoginAccountFacebookSub(openSelectDialog, onLoggedIn);
                    }
                    else
                    {
                        onLoggedIn(false, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                    }
                });
            }
        }

#if UNITY_IOS
        public static void LoginAccountAppleGameCenter(OpenSelectDialog openSelectDialog, OnLoggedIn onLoggedIn)
        {
            _onLoggedIn = onLoggedIn;
            _openSelectDialog = openSelectDialog;

            if(IsGameCenterLoginCancelled && !IsLoggedInGameService())
            {
                _openSelectDialog(DialogType.LoginFromIOSSetting, AccountType.AppleGameCenter, null, null, null, selectButton =>
                {
                    _onLoggedIn(false, WebClient.AuthCode.Cancel, TimeSpan.Zero, 0);
                });
                return;
            }


            if (ISN_GKLocalPlayer.LocalPlayer != null && ISN_GKLocalPlayer.LocalPlayer.Authenticated)
            {
                ISN_GKLocalPlayer player = ISN_GKLocalPlayer.LocalPlayer;
                player.GenerateIdentityVerificationSignatureWithCompletionHandler((signatureResult) => {
#if MDEBUG
                    if (signatureResult.IsSucceeded)
                    {
                        Debug.Log("signatureResult.PublicKeyUrl: " + signatureResult.PublicKeyUrl);
                        Debug.Log("signatureResult.Timestamp: " + signatureResult.Timestamp);
                        Debug.Log("signatureResult.Salt.Length: " + signatureResult.Salt.Length);
                        Debug.Log("signatureResult.Signature.Length: " + signatureResult.Signature.Length);
                    }
                    else
                    {
                        Debug.Log("IdentityVerificationSignature has failed: " + signatureResult.Error.FullMessage);
                    }
#endif
                    OnPlayerSignatureRetrieveResult(signatureResult);
                });
            }
            else
            {
                ISN_GKLocalPlayer.Authenticate((SA_Result result) => {
                    OnAuthFinished(result);
                });
            }

        }
#endif

#if UNITY_IOS
        static void OnAuthFinished(SA_Result res)
        {
            if (res.IsSucceeded)
            {
                ISN_GKLocalPlayer player = ISN_GKLocalPlayer.LocalPlayer;
                player.GenerateIdentityVerificationSignatureWithCompletionHandler((signatureResult) => {
#if MDEBUG
                    if (signatureResult.IsSucceeded)
                    {
                        Debug.Log("signatureResult.PublicKeyUrl: " + signatureResult.PublicKeyUrl);
                        Debug.Log("signatureResult.Timestamp: " + signatureResult.Timestamp);
                        Debug.Log("signatureResult.Salt.Length: " + signatureResult.Salt.Length);
                        Debug.Log("signatureResult.Signature.Length: " + signatureResult.Signature.Length);
                    }
                    else
                    {
                        Debug.Log("IdentityVerificationSignature has failed: " + signatureResult.Error.FullMessage);
                    }
#endif
                    OnPlayerSignatureRetrieveResult(signatureResult);
                });
            }
            else if(res.IsFailed) 
            {
#if MDEBUG
                Debug.Log(res.Error.Code);
                Debug.Log(res.Error.Message);
                Debug.Log("OnAuthFinished   failed");
#endif
                if(res.Error.Code == 2 || res.Error.Code == 6)
                {
                    // 아이폰 세팅에서 GameCenter 로그인을 해야하는 상황.
                    IsGameCenterLoginCancelled = true;
                    _openSelectDialog(DialogType.LoginFromIOSSetting, AccountType.AppleGameCenter, null, null, null, selectButton =>
                    {
                        _onLoggedIn(false, WebClient.AuthCode.Cancel, TimeSpan.Zero, 0);
                    });
                }
                else 
                {
                    _onLoggedIn(false, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                }
            }
        }

        static void OnPlayerSignatureRetrieveResult(ISN_GKIdentityVerificationSignatureResult result)
        {
            if (result.IsSucceeded)
            {
                ISN_GKLocalPlayer player = ISN_GKLocalPlayer.LocalPlayer;
                string playerId = player.PlayerID;
                string playerAlias = player.Alias;
                string publicKeyUrl = result.PublicKeyUrl;
                string signature = System.Convert.ToBase64String(result.Signature);
                long timestamp = result.Timestamp;
                string salt = System.Convert.ToBase64String(result.Salt);

                WebClient.GetInstance().RequestAuthApple(playerId, playerAlias, publicKeyUrl, signature, timestamp, salt, Account.GetAccountId(), LinkOption.None, (WebClient.ErrorCode error, WebClient.AuthCode code, string accountId1, AccountType localAccountType, string localAccountName, byte[] accountInfo, TimeSpan blockRemainTime, long blockSuid) =>
                {
                    if (error == WebClient.ErrorCode.SUCCESS)
                    {
                        if (code == WebClient.AuthCode.SUCCESS)
                        {
                            isGuestAccount = false;
                            // 게임센터에 연동된 계정으로 로그인 성공
                            Account.SetAccountId(accountId1, Account.HaeginAccountType.AppleGameCenter);
                             _onLoggedIn(true, code, blockRemainTime, blockSuid);
                        }
                        else if (code == WebClient.AuthCode.NEED_TO_LINK)
                        {
                            // 기존 소셜 연동된 계정 상태에서 새롭게 게임 계정과 연동되지 않은 GameCenter 계정 로그인시
                            _openSelectDialog(DialogType.LinkOrNew, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.AppleGameCenter, playerAlias), accountInfo, selectButton =>
                            {
                                if (selectButton == SelectButton.YES)
                                {
                                    // 기존 소셜 계정과 GameCenter 계정을 연동
                                    WebClient.GetInstance().RequestAuthApple(playerId, playerAlias, publicKeyUrl, signature, timestamp, salt, Account.GetAccountId(), LinkOption.Link, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                    {
                                        if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                        {
                                            isGuestAccount = false;
                                            Account.SetAccountId(accountId2, Account.HaeginAccountType.AppleGameCenter);
                                            _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                        }
                                        else
                                        {
                                            _onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                        }
                                    });
                                }
                                else
                                {
                                    // GameCenter 계정은 로그아웃 불가능하니, 싫다고 하면 기존 계정 로그아웃. GameCenter 계정에 연동되는 새 계정 만들어서 연동
                                    if(localAccountType == AccountType.Facebook) 
                                    {
                                        FB.LogOut();
                                    }
                                    WebClient.GetInstance().RequestAuthApple(playerId, playerAlias, publicKeyUrl, signature, timestamp, salt, Account.GetAccountId(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                    {
                                        if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                        {
                                            isGuestAccount = false;
                                            Account.SetAccountId(accountId2, Account.HaeginAccountType.AppleGameCenter);
                                            _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                        }
                                        else
                                        {
                                            _onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                        }
                                    });
                                }
                            });
                        }
                        else if(code == WebClient.AuthCode.NEED_TO_SELECT)
                        {
                            // Guest 계정 상태에서
                            // 게임센터에 연동된 예전 게임 데이타가 있음
                            // 이 경우에는 사용자에게 어느 계정을 사용할 것인지 물어봐야함
                            _openSelectDialog(DialogType.Select, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.AppleGameCenter, playerAlias), accountInfo, selectButton =>
                            {
                                if (selectButton == SelectButton.YES)
                                {
                                    // 기존 게스트 계정을 포기하고, 
                                    WebClient.GetInstance().RequestAuthApple(playerId, playerAlias, publicKeyUrl, signature, timestamp, salt, Account.GetAccountId(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                    {
                                        if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                        {
                                            isGuestAccount = false;
                                            Account.SetAccountId(accountId2, Account.HaeginAccountType.AppleGameCenter);
                                            _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                        }
                                        else
                                        {
                                            _onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                        }
                                    });
                                }
                                else
                                {
                                    WebClient.GetInstance().RequestAuthApple(playerId, playerAlias, publicKeyUrl, signature, timestamp, salt, Account.GetAccountId(), LinkOption.Local, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                    {
                                        if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                        {
                                            isGuestAccount = false;
                                            Account.SetAccountId(accountId2, Account.HaeginAccountType.AppleGameCenter);
                                            _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                        }
                                        else
                                        {
                                            _onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                        }
                                    });
                                }
                            });
                        }
                        else if(code == WebClient.AuthCode.NEED_TO_LOGOUT) 
                        {
                            _openSelectDialog(DialogType.MustLogout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.AppleGameCenter, playerAlias), accountInfo, selectButton =>
                            {
                                // GameCenter 계정은 로그아웃 불가능하니, 기존 계정 로그아웃.
                                // GameCenter에 연동된 계정 불러오기.
                                if (FB.IsLoggedIn)
                                {
                                    FB.LogOut();
                                }
                                if (IsLoggedInAppleId())
                                {
                                    LogoutAppleId();
                                }
                                WebClient.GetInstance().RequestAuthApple(playerId, playerAlias, publicKeyUrl, signature, timestamp, salt, Account.GetAccountId(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                {
                                    if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                    {
                                        isGuestAccount = false;
                                        Account.SetAccountId(accountId2, Account.HaeginAccountType.AppleGameCenter);
                                        _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                    }
                                    else
                                    {
                                        _onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                    }
                                });
                            });
                        }
                        else if(code == WebClient.AuthCode.NEED_TO_LOGOUT_CLEAR) 
                        {
                            _openSelectDialog(DialogType.MustLogout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.AppleGameCenter, playerAlias), accountInfo, selectButton =>
                            {
                                // GameCenter 계정은 로그아웃 불가능하니, 기존 계정 로그아웃.
                                // GameCenter에 연동된 계정 불러오기.
                                if (FB.IsLoggedIn)
                                {
                                    FB.LogOut();
                                }
                                if (IsLoggedInAppleId())
                                {
                                    LogoutAppleId();
                                }
                                SetAccountId(ProjectSettings.accountIdToCreate, Account.HaeginAccountType.Guest);
                                WebClient.GetInstance().RequestAuth((WebClient.ErrorCode error3, WebClient.AuthCode code3, AccountType accountType3, TimeSpan blockRemainTime3, long blockSuid3) => {
                                    if (error3 == WebClient.ErrorCode.SUCCESS)
                                    {
                                        isGuestAccount = (accountType3 == AccountType.None);
                                         switch (code3)
                                         {
                                              case WebClient.AuthCode.SUCCESS:
                                                  WebClient.GetInstance().RequestAuthApple(playerId, playerAlias, publicKeyUrl, signature, timestamp, salt, Account.GetAccountId(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                                  {
                                                      isGuestAccount = false;
                                                      Account.SetAccountId(accountId2, Account.HaeginAccountType.AppleGameCenter);
                                                      _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                  });
                                                  break;
                                              default://case WebClient.AuthCode.FAILED:
                                                  // 로그인 실패 처리
                                                  _onLoggedIn(false, code3, blockRemainTime3, blockSuid3);
                                                  break;
                                          }
                                     }
                                     else
                                     {
                                          // 네트워크 에러 처리
                                          _onLoggedIn(false, WebClient.AuthCode.FAILED, blockRemainTime3, blockSuid3);
                                     }
                                });
                            });
                        }
                        else
                        {
                            _onLoggedIn(false, code, blockRemainTime, blockSuid);
                        }
                    }
                    else
                    {
                        _onLoggedIn(false, code, blockRemainTime, blockSuid);
                    }
                });
            }
            else
            {
                _onLoggedIn(false, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
            }
        }

#elif UNITY_ANDROID


        static AN_GoogleSignInClient GetSignInClient()
        {
            AN_GoogleSignInOptions.Builder builder = new AN_GoogleSignInOptions.Builder(AN_GoogleSignInOptions.DEFAULT_GAMES_SIGN_IN);
            builder.RequestId();
            builder.RequestEmail();
            builder.RequestProfile();
            builder.RequestScope(new AN_Scope(AN_Scopes.GAMES));
            builder.RequestScope(new AN_Scope(AN_Scopes.GAMES_LITE));
            // Add the APPFOLDER scope for Snapshot support.
            //builder.RequestScope(AN_Drive.SCOPE_APPFOLDER);
            builder.RequestServerAuthCode(ProjectSettings.webClientOAuth2ClientId, false);
            //builder.RequestIdToken(ProjectSettings.webClientOAuth2ClientId);
            AN_GoogleSignInOptions gso = builder.Build();
            AN_GoogleSignInClient client = AN_GoogleSignIn.GetClient(gso);
            return client;
        }
        public delegate void OnServerAuthCode(string authCode);
        static void GetRefreshedServerAuthCode(OnServerAuthCode callback)
        {
            GPGSServerAuthCode.GetInstance().GetServerAuthCode(ProjectSettings.webClientOAuth2ClientId, (string authCode) => {
                callback(authCode);
            });
/*
            AN_GoogleSignInClient client = GetSignInClient();
            client.SilentSignIn((result) =>
            {
                if (result.IsSucceeded)
                {
                    if (result.IsSucceeded)
                    {
                        try
                        {
                            AN_GoogleSignInAccount account = result.Account;
                            string authCode = account.GetServerAuthCode();
#if MDEBUG
                            Debug.Log("ServerAuthCode = [" + authCode + "]");
                            //Debug.Log("IdToken = [" + account.GetIdToken() + "]");
#endif
                            callback(authCode);
                        }
                        catch
                        {
#if MDEBUG
                            Debug.Log("ServerAuthCode = []  1");
#endif
                            callback(null);
                        }
                    }
                    else
                    {
#if MDEBUG
                        Debug.Log("ServerAuthCode = []  2");
#endif
                        callback(null);
                    }
                }
                else
                {
#if MDEBUG
                    Debug.Log("ServerAuthCode = []  3");
#endif
                    callback(null);
                }
            });
*/            
        }
        public static void LoginAccountGooglePlayGameService(OpenSelectDialog openSelectDialog, OnLoggedIn onLoggedIn, bool forceRefresh = false)
        {
#if MDEBUG
            Debug.Log("LoginAccountGooglePlayGameService");
#endif
            _onLoggedIn = onLoggedIn;
            _openSelectDialog = openSelectDialog;

            AN_GoogleSignInClient client = GetSignInClient();

            client.SilentSignIn((result) => {
                if (result.IsSucceeded)
                {
                    ThreadSafeDispatcher.Instance.Invoke(() => {
                        OnConnectionResultReceived(result);
                    });
                }
                else
                {
                    // Player will need to sign-in explicitly using via UI
#if MDEBUG
                    Debug.Log("SilentSignIn Failed with code: " + result.Error.Code + " " + result.Error.Message);
                    Debug.Log("Starting the default Sign in flow");
#endif
                    //Starting the interactive sign-in
                    client.SignIn((signInResult) => {
#if MDEBUG
                        Debug.Log("Sign In StatusCode2: " + signInResult.StatusCode);
#endif
                        ThreadSafeDispatcher.Instance.Invoke(() => {
                            OnConnectionResultReceived(signInResult);
                        });
                    });


                }
            });
        }

        static void OnConnectionResultReceived(AN_GoogleSignInResult result)
        {
#if MDEBUG
            Debug.Log("OnConnectionResultReceived  " + result.IsSucceeded);
            if (!result.IsSucceeded)
            {
                Debug.Log("SignIn Failed with code: " + result.Error.Code + " " + result.Error.Message);
            }
#endif
            if (result.IsSucceeded)
            {
                var gamesClient = AN_Games.GetGamesClient();
                gamesClient.SetViewForPopups(AN_MainActivity.Instance);
                gamesClient.SetGravityForPopups(AN_Gravity.TOP | AN_Gravity.CENTER_HORIZONTAL);

                AN_GoogleSignInAccount account = result.Account;
                GetRefreshedServerAuthCode((string authCode2) => {
                    OnServerAuthCodeLoaded(result, authCode2, account);
                });
            }
            else
            {
                if(result.StatusCode == AN_CommonStatusCodes.CANCELED)
                {
                    LogoutGameService();
                    _onLoggedIn(false, WebClient.AuthCode.Cancel, TimeSpan.Zero, 0);
                }
                else if (result.StatusCode == AN_CommonStatusCodes.UNKNOWN_SING_ERROR)
                {
                    LogoutGameService();
                    _onLoggedIn(false, WebClient.AuthCode.Cancel, TimeSpan.Zero, 0);
                }
                else
                {
                    _onLoggedIn(false, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                }
            }
        }

        static void OnServerAuthCodeLoaded(AN_GoogleSignInResult result2, string authCode, AN_GoogleSignInAccount account)
        {
            if (!string.IsNullOrEmpty(authCode))
            {
                AN_PlayersClient playerClient = AN_Games.GetPlayersClient();
                playerClient.GetCurrentPlayer((currentUserResult) => {
                    string name = "noname";
                    if (currentUserResult.IsSucceeded)
                    {
                        AN_Player player = currentUserResult.Data;
                        name = player.DisplayName;
                    }

                    WebClient.GetInstance().RequestAuthGoogle(authCode, Account.GetAccountId(), LinkOption.None, (WebClient.ErrorCode error, WebClient.AuthCode code, string accountId1, AccountType localAccountType, string localAccountName, byte[] accountInfo, TimeSpan blockRemainTime, long blockSuid) =>
                    {
                        if (error == WebClient.ErrorCode.SUCCESS)
                        {
                            if (code == WebClient.AuthCode.SUCCESS)
                            {
                                isGuestAccount = false;
                                // 구글에 연동된 계정으로 로그인 성공
                                Account.SetAccountId(accountId1, Account.HaeginAccountType.GooglePlayGameService);
                                _onLoggedIn(true, code, blockRemainTime, blockSuid);
                            }
                            else if (code == WebClient.AuthCode.NEED_TO_LINK)
                            {
                                // 기존 소셜 계정과 새로운 소셜 계정에 연결이 필요한 경우
                                // 구글에 연동된 예전 게임 데이타가 없음.
                                _openSelectDialog(DialogType.Link, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.GooglePlay, name), accountInfo, selectButton =>
                                {
                                    if (selectButton == SelectButton.YES)
                                    {
                                        GetRefreshedServerAuthCode((string serverAuthCode) => {
                                            WebClient.GetInstance().RequestAuthGoogle(serverAuthCode, Account.GetAccountId(), LinkOption.Link, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                            {
                                                if(error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                                {
                                                    isGuestAccount = false;
                                                    Account.SetAccountId(accountId2, Account.HaeginAccountType.GooglePlayGameService);
                                                    _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                }
                                                else
                                                {
                                                    _onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                }
                                            });
                                        });
                                    }
                                    else
                                    {
                                        // 구글 로그인을 취소한다. 로그아웃
                                        LogoutGameService();
                                        _onLoggedIn(false, WebClient.AuthCode.Cancel, TimeSpan.Zero, 0);
                                    }
                                });
                            }
                            else if (code == WebClient.AuthCode.NEED_TO_SELECT)
                            {
                                // 구글에 연동된 예전 게임 데이타가 있음
                                // 이 경우에는 사용자에게 어느 계정을 사용할 것인지 물어봐야함
                                _openSelectDialog(DialogType.Select, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.GooglePlay, name), accountInfo, selectButton =>
                                {
                                    if (selectButton == SelectButton.YES)
                                    {
                                        GetRefreshedServerAuthCode((string serverAuthCode) => {
                                            WebClient.GetInstance().RequestAuthGoogle(serverAuthCode, Account.GetAccountId(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                            {
                                                if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                                {
                                                    isGuestAccount = false;
                                                    Account.SetAccountId(accountId2, Account.HaeginAccountType.GooglePlayGameService);
                                                    _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                }
                                                else
                                                {
                                                    _onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                }
                                            });
                                        });
                                    }
                                    else
                                    {
                                        GetRefreshedServerAuthCode((string serverAuthCode) => {
                                            WebClient.GetInstance().RequestAuthGoogle(serverAuthCode, Account.GetAccountId(), LinkOption.Local, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                            {
                                                if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                                {
                                                    isGuestAccount = false;
                                                    Account.SetAccountId(accountId2, Account.HaeginAccountType.GooglePlayGameService);
                                                    _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                }
                                                else
                                                {
                                                    _onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                }
                                            });
                                        });
                                    }
                                });
                            }
                            else if (code == WebClient.AuthCode.NEED_TO_LOGOUT)
                            {
#if SKIP_OPEN_NEED_TO_LOGOUT_DIALOG
                                if(GameServiceAccountType == Account.HaeginAccountType.GooglePlayGameService)
                                {
                                    // 이미 구글 연동 계정인데, 다른 구글 계정으로 연동한 경우에는 계정 선택없이 바로 새로 로그인한 계정으로 변경하자.
                                    _openSelectDialog(DialogType.MustLogout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.GooglePlay, name), accountInfo, selectButton =>
                                    {
                                        if (FB.IsLoggedIn)
                                        {
                                            FB.LogOut();
                                        }
                                        if (IsLoggedInAppleId())
                                        {
                                            LogoutAppleId();
                                        }
                                        GetRefreshedServerAuthCode((string serverAuthCode) => {
                                            WebClient.GetInstance().RequestAuthGoogle(serverAuthCode, Account.GetAccountId(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                            {
                                                if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                                {
                                                    isGuestAccount = false;
                                                    Account.SetAccountId(accountId2, Account.HaeginAccountType.GooglePlayGameService);
                                                    _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                }
                                                else
                                                {
                                                    _onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                }
                                            });
                                        });
                                    });
                                }
                                else
                                {
#endif
                                    _openSelectDialog(DialogType.Logout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.GooglePlay, name), accountInfo, selectButton =>
                                    {
                                        if (selectButton == SelectButton.YES)
                                        {
                                            if (FB.IsLoggedIn)
                                            {
                                                FB.LogOut();
                                            }
                                            if (IsLoggedInAppleId())
                                            {
                                                LogoutAppleId();
                                            }
                                            GetRefreshedServerAuthCode((string serverAuthCode) => {
                                                WebClient.GetInstance().RequestAuthGoogle(serverAuthCode, Account.GetAccountId(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                                {
                                                    if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                                    {
                                                        isGuestAccount = false;
                                                        Account.SetAccountId(accountId2, Account.HaeginAccountType.GooglePlayGameService);
                                                        _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                    }
                                                    else
                                                    {
                                                        _onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                    }
                                                });
                                            });
                                        }
                                        else
                                        {
                                            // Google Play 계정 로그아웃 
                                            if (IsLoggedInGameService())
                                            {
                                                LogoutGameService();
                                            }
                                            _onLoggedIn(false, code, blockRemainTime, blockSuid);
                                        }
                                    });
#if SKIP_OPEN_NEED_TO_LOGOUT_DIALOG
                                }
#endif
                            }
                            else if (code == WebClient.AuthCode.NEED_TO_LOGOUT_CLEAR)
                            {
#if SKIP_OPEN_NEED_TO_LOGOUT_DIALOG
                                if(GameServiceAccountType == Account.HaeginAccountType.GooglePlayGameService)
                                {
                                    // 이미 구글 연동 계정인데, 다른 구글 계정으로 연동한 경우에는 계정 선택없이 바로 새로 로그인한 계정으로 변경하자.
                                    _openSelectDialog(DialogType.MustLogout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.GooglePlay, name), accountInfo, selectButton =>
                                    {
                                        if (FB.IsLoggedIn)
                                        {
                                            FB.LogOut();
                                        }
                                        if (IsLoggedInAppleId())
                                        {
                                            LogoutAppleId();
                                        }

                                        SetAccountId(ProjectSettings.accountIdToCreate, Account.HaeginAccountType.Guest);
                                        WebClient.GetInstance().RequestAuth((WebClient.ErrorCode error3, WebClient.AuthCode code3, AccountType accountType3, TimeSpan blockRemainTime3, long blockSuid3) => {
                                            if (error3 == WebClient.ErrorCode.SUCCESS)
                                            {
                                                isGuestAccount = (accountType3 == AccountType.None);
                                                switch (code3)
                                                {
                                                    case WebClient.AuthCode.SUCCESS:
                                                        GetRefreshedServerAuthCode((string serverAuthCode) => {
                                                            WebClient.GetInstance().RequestAuthGoogle(serverAuthCode, Account.GetAccountId(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                                            {
                                                                if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                                                {
                                                                    isGuestAccount = false;
                                                                    Account.SetAccountId(accountId2, Account.HaeginAccountType.GooglePlayGameService);
                                                                    _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                                }
                                                                else
                                                                {
                                                                    _onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                                }
                                                            });
                                                        });
                                                        break;
                                                    default://case WebClient.AuthCode.FAILED:
                                                            // 로그인 실패 처리
                                                        _onLoggedIn(false, code3, blockRemainTime3, blockSuid3);
                                                        break;
                                                }
                                            }
                                            else
                                            {
                                                // 네트워크 에러 처리
                                                _onLoggedIn(false, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                            }
                                        });
                                    });
                                }
                                else
                                {
#endif
                                    _openSelectDialog(DialogType.Logout, localAccountType, TypedName(localAccountType, localAccountName), TypedName(AccountType.GooglePlay, name), accountInfo, selectButton =>
                                    {
                                        if (selectButton == SelectButton.YES)
                                        {
                                            if (FB.IsLoggedIn)
                                            {
                                                FB.LogOut();
                                            }
                                            if (IsLoggedInAppleId())
                                            {
                                                LogoutAppleId();
                                            }

                                            SetAccountId(ProjectSettings.accountIdToCreate, Account.HaeginAccountType.Guest);
                                            WebClient.GetInstance().RequestAuth((WebClient.ErrorCode error3, WebClient.AuthCode code3, AccountType accountType3, TimeSpan blockRemainTime3, long blockSuid3) => {
                                                if (error3 == WebClient.ErrorCode.SUCCESS)
                                                {
                                                    isGuestAccount = (accountType3 == AccountType.None);
                                                    switch (code3)
                                                    {
                                                        case WebClient.AuthCode.SUCCESS:
                                                            GetRefreshedServerAuthCode((string serverAuthCode) => {
                                                                WebClient.GetInstance().RequestAuthGoogle(serverAuthCode, Account.GetAccountId(), LinkOption.Auth, (WebClient.ErrorCode error2, WebClient.AuthCode code2, string accountId2, AccountType localAccountType2, string localAccountName2, byte[] accountInfo2, TimeSpan blockRemainTime2, long blockSuid2) =>
                                                                {
                                                                    if (error2 == WebClient.ErrorCode.SUCCESS && code2 == WebClient.AuthCode.SUCCESS)
                                                                    {
                                                                        isGuestAccount = false;
                                                                        Account.SetAccountId(accountId2, Account.HaeginAccountType.GooglePlayGameService);
                                                                        _onLoggedIn(true, code2, blockRemainTime2, blockSuid2);
                                                                    }
                                                                    else
                                                                    {
                                                                        _onLoggedIn(error2 == WebClient.ErrorCode.SUCCESS, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                                    }
                                                                });
                                                            });
                                                            break;
                                                        default://case WebClient.AuthCode.FAILED:
                                                                // 로그인 실패 처리
                                                            _onLoggedIn(false, code3, blockRemainTime3, blockSuid3);
                                                            break;
                                                    }
                                                }
                                                else
                                                {
                                                    // 네트워크 에러 처리
                                                    _onLoggedIn(false, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                                                }
                                            });
                                        }
                                        else
                                        {
                                            // Google Play 계정 로그아웃 
                                            if (IsLoggedInGameService())
                                            {
                                                LogoutGameService();
                                            }
                                            _onLoggedIn(false, code, blockRemainTime, blockSuid);
                                        }
                                    });
#if SKIP_OPEN_NEED_TO_LOGOUT_DIALOG
                                }
#endif
                            }
                            else
                            {
                                _onLoggedIn(false, code, blockRemainTime, blockSuid);
                            }
                        }
                        else
                        {
                            _onLoggedIn(false, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
                        }
                    });
                });
            }
            else
            {
                _onLoggedIn(false, WebClient.AuthCode.FAILED, TimeSpan.Zero, 0);
            }
        }
#endif
                            }
                        }

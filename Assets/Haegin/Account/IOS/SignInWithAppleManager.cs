﻿using AppleAuth.IOS;
using AppleAuth.IOS.Enums;
using AppleAuth.IOS.Interfaces;
using AppleAuth.IOS.NativeMessages;
using UnityEngine;

namespace Haegin
{
	public class SignInWithAppleManager : MonoBehaviour
	{
		private const string AppleUserIdKey = "AppleUserId";
        private const string AppleUserNameKey = "AppleUserName";
        private const string AppleUserEmailKey = "AppleUserEmail";
        private IAppleAuthManager _appleAuthManager;
		private OnDemandMessageHandlerScheduler _scheduler;

        public delegate void SIWACallback(bool result, string identityToken, string authCode, string appleId, string fullname, string email);

        public static SignInWithAppleManager GetInstance()
		{
			GameObject gameObject = GameObject.Find("HaeginSIWAManager");
			if (gameObject == null)
			{
				gameObject = new GameObject("HaeginSIWAManager");
    			DontDestroyOnLoad(gameObject);
				gameObject.AddComponent<SignInWithAppleManager>();

                gameObject.GetComponent<SignInWithAppleManager>().Initialize();
            }
			return gameObject.GetComponent<SignInWithAppleManager>();
		}

#if IN_TESTING   // 문제가 있어서 적용 중단.
        private async void Initialize()
#else
        private void Initialize()
#endif
        {
            Debug.Log("AppleId Initialize Begin");
            // Creates the Scheduler to execute the pending callbacks on demand
            this._scheduler = new OnDemandMessageHandlerScheduler();
            // Creates a default JSON deserializer, to transform JSON Native responses to C# instances
            var deserializer = new PayloadDeserializer();
            // Creates an Apple Authentication manager with the scheduler and the deserializer
            this._appleAuthManager = new AppleAuthManager(deserializer, this._scheduler);

            if (IsSupported())
            {
                this._appleAuthManager.SetCredentialsRevokedCallback(result =>
                {
                    PlayerPrefs.DeleteKey(AppleUserIdKey);
                });

#if IN_TESTING
                if (PlayerPrefs.HasKey(AppleUserIdKey))
                {
                    bool quit = false;
                    var storedAppleUserId = PlayerPrefs.GetString(AppleUserIdKey);
                    this._appleAuthManager.GetCredentialState(
                        storedAppleUserId,
                        state =>
                        {
                            switch (state)
                            {
                                case CredentialState.Authorized:
                                    quit = true;
                                    Debug.Log("AppleId check 1");
                                    break;
                                case CredentialState.Revoked:
                                    this._appleAuthManager.QuickLogin(
                                        credential =>
                                        {
                                            Debug.Log("AppleId check 2");
                                            quit = true;
                                        },
                                        error =>
                                        {
                                            Logout();
                                            Debug.Log("AppleId check 3");
                                            quit = true;
                                        });
                                    break;
                                default:
                                    Logout();
                                    Debug.Log("AppleId check 4");
                                    quit = true;
                                    break;
                            }
                        },
                        error =>
                        {
                            Logout();
                            Debug.Log("AppleId check 5");
                            quit = true;
                        });

                    await System.Threading.Tasks.Task.Run(() =>
                    {
                        while (!quit)
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    });
                }
#endif
            }
#if MDEBUG
            Debug.Log("AppleId Initialize Finished");
#endif
        }

		private void Update()
		{
			// Updates the scheduler to execute pending response callbacks
			// This ensures they are executed inside Unity's Update loop
			this._scheduler.Update();
		}

		public void Logout()
		{
            PlayerPrefs.DeleteKey(AppleUserIdKey);
            PlayerPrefs.DeleteKey(AppleUserNameKey);
            PlayerPrefs.DeleteKey(AppleUserEmailKey);
            PlayerPrefs.Save();
        }

        public bool IsSupported()
		{
#if UNITY_IOS
            return this._appleAuthManager.IsCurrentPlatformSupported;
#else
            return false;
#endif
        }

        public bool IsLoggedIn()
        {
            return IsSupported() && PlayerPrefs.HasKey(AppleUserIdKey);
        }

		public void Login(SIWACallback callback, bool forceRelogin = false)
		{
            if(!IsSupported())
            {
                callback(false, null, null, null, null, null);
            }

			// If we have stored a Apple User Id, attempt to get the credentials for it first
			if (PlayerPrefs.HasKey(AppleUserIdKey) && !forceRelogin)
			{
				var storedAppleUserId = PlayerPrefs.GetString(AppleUserIdKey);
                var storedAppleUserName = PlayerPrefs.GetString(AppleUserNameKey);
                var storedAppleUserEmail = PlayerPrefs.GetString(AppleUserEmailKey);
                this.CheckCredentialStatusForUserId(storedAppleUserId, storedAppleUserName, storedAppleUserEmail, callback);
			}
			// If we do not have a user ID, we just show the button to Sign In with Apple
			else
			{
				this.SignInWithApple(callback, forceRelogin);
			}
		}

		private void CheckCredentialStatusForUserId(string appleUserId, string appleUserName, string appleUserEmail, SIWACallback callback)
		{
			this._appleAuthManager.GetCredentialState(
				appleUserId,
				state =>
				{
					switch (state)
					{
						case CredentialState.Authorized:
                            callback(true, null, null, appleUserId, appleUserName, appleUserEmail);
							return;
						case CredentialState.Revoked:
							this.AttemptQuickLogin(callback);
							return;
						case CredentialState.NotFound:
                            Logout();
                            this.SignInWithApple(callback);
							return;
					}
				},
				error =>
				{
					this.SignInWithApple(callback);
				});
		}

        private void CallCallbackWithCredential(ICredential credential, SIWACallback callback)
        {
            string identityToken = null;
            string authCode = null;
            string fullname = "NA";
            string email = "NA";

            var appleIdCredential = credential as IAppleIDCredential;
            if (appleIdCredential != null)
            {
                if(appleIdCredential.IdentityToken != null)
                    identityToken = System.Text.Encoding.UTF8.GetString(appleIdCredential.IdentityToken);
                if(appleIdCredential.AuthorizationCode != null)
                    authCode = System.Text.Encoding.UTF8.GetString(appleIdCredential.AuthorizationCode);
                if (appleIdCredential.FullName != null)
                {
                    fullname = AppleAuth.IOS.Extensions.PersonNameExtensions.ToLocalizedString(appleIdCredential.FullName);
                    PlayerPrefs.SetString(AppleUserNameKey, fullname);
                }
                if (appleIdCredential.Email != null)
                {
                    email = appleIdCredential.Email;
                    PlayerPrefs.SetString(AppleUserEmailKey, email);
                }
                PlayerPrefs.SetString(AppleUserIdKey, credential.User);
                PlayerPrefs.Save();
            }
            callback(true, identityToken, authCode, credential.User, fullname, email);
        }

        private void AttemptQuickLogin(SIWACallback callback)
		{
			this._appleAuthManager.QuickLogin(
				credential =>
				{
                    CallCallbackWithCredential(credential, callback);
                },
				error =>
				{
                    Logout();
                    SignInWithApple(callback);
                });
		}

		private void SignInWithApple(SIWACallback callback, bool forceRelogin = false)
		{
			this._appleAuthManager.LoginWithAppleId(
				LoginOptions.IncludeEmail | LoginOptions.IncludeFullName,
				credential =>
				{
                    CallCallbackWithCredential(credential, callback);
                },
				error =>
				{
                    if(!forceRelogin)
                        Logout();
                    callback(false, null, null, null, null, null);
				});
		}
	}
}

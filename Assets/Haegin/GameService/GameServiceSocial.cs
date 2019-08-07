using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Facebook.Unity;

#if UNITY_ANDROID
using SA.Android.GMS.Games;
using SA.Android.GMS.Common.Images;
#elif UNITY_IOS
using SA.iOS.GameKit;
#elif UNITY_STANDALONE && USE_STEAM
using Steamworks;
#endif

namespace Haegin
{
    public enum PhotoLoad
    {
        None,
        Normal,
        HiRes
    };

    public enum GameServiceResult
    {
        LoginRequired,
        Succeeded,
        Failed
    };

    public enum SocialType
    {
        GameCenter,
        GoogleGameService,
        Facebook,
        Steam
    };

    public class SocialPlayerInfo
    {
        public string id;
        public string name;
        public Texture2D photo;
        public SocialType socialType;
    }

    public class GameServiceSocial
    {
        public delegate void OnLoadFriendsList(GameServiceResult result, List<SocialPlayerInfo> friendIds);
        public delegate void OnLoadPlayerInfo(GameServiceResult result, SocialPlayerInfo friend);

//        delegate void OnFriendsListLoaded(SA.Common.Models.Result result);

        public static bool IsLoggedIn()
        {
            return Account.IsLoggedInGameService();
        }

        public static void LoadMyInfo(PhotoLoad downloadPhoto, OnLoadPlayerInfo callback)
        {
            SocialPlayerInfo playerInfo = new SocialPlayerInfo();

#if UNITY_STANDALONE && USE_STEAM
            playerInfo.id = SteamUser.GetSteamID().ToString();
            playerInfo.socialType = SocialType.Steam;
            playerInfo.name = SteamFriends.GetPersonaName();
#else
            if (FB.IsLoggedIn)
            {
                FB.API("/me?fields=name", HttpMethod.POST, (IGraphResult result) =>
                {
                    playerInfo.id = AccessToken.CurrentAccessToken.UserId;
                    playerInfo.name = result.ResultDictionary["name"].ToString();
                    playerInfo.socialType = SocialType.Facebook;
                    LoadPlayerInfo(playerInfo, downloadPhoto, callback);
                });
                return;
            }
            if(IsLoggedIn()) 
            {
#if UNITY_ANDROID
                AN_PlayersClient client = AN_Games.GetPlayersClient();
                if(client != null)
                {
                    client.GetCurrentPlayer((result) => {
                        if (result.IsSucceeded)
                        {
                            AN_Player player = result.Data;
                            playerInfo.id = player.Id;
                            playerInfo.socialType = SocialType.GoogleGameService;
#if MDEBUG
                            Debug.Log("player.Id: " + player.Id);
                            Debug.Log("player.Title: " + player.Title);
                            Debug.Log("player.DisplayName: " + player.DisplayName);
                            Debug.Log("player.HiResImageUri: " + player.HiResImageUri);
                            Debug.Log("player.IconImageUri: " + player.IconImageUri);
                            Debug.Log("player.HasIconImage: " + player.HasIconImage);
                            Debug.Log("player.HasHiResImage: " + player.HasHiResImage);
#endif
                        }
                        else
                        {
#if MDEBUG
                            Debug.Log("Failed to load Current Player " + result.Error.FullMessage);
#endif
                        }
                    });
                }
#elif UNITY_IOS
                ISN_GKLocalPlayer player = ISN_GKLocalPlayer.LocalPlayer;
                if (player != null)
                {
                    playerInfo.id = player.PlayerID;
                }
                playerInfo.socialType = SocialType.GameCenter;
#endif
                LoadPlayerInfo(playerInfo, downloadPhoto, callback);
            }
#endif
        }

        public static void LoadFriendsList(OnLoadFriendsList callback)
        {
            if (IsLoggedIn() == false)
            {
                if (FB.IsLoggedIn)
                {
                    LoadFBFriendsList(GameServiceResult.LoginRequired, null, callback);
                }
                else
                {
                    callback(GameServiceResult.LoginRequired, null);
                }
                return;
            }
#if UNITY_ANDROID
            if (FB.IsLoggedIn)
            {
                LoadFBFriendsList(GameServiceResult.Succeeded, null, callback);
            }
            else
            {
                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    callback(GameServiceResult.Succeeded, null);
                });
            }
#elif UNITY_IOS
            if (FB.IsLoggedIn)
            {
                LoadFBFriendsList(GameServiceResult.Succeeded, null, callback);
            }
            else
            {
                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    callback(GameServiceResult.Succeeded, null);
                });
            }
#elif UNITY_STANDALONE && USE_STEAM
            int nCount = SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagAll);
            if(nCount > 0)
            {
                List<SocialPlayerInfo> friends = new List<SocialPlayerInfo>();
                for (int i = 0; i < nCount; i++)
                {
                    SocialPlayerInfo friend = new SocialPlayerInfo();
                    friend.id = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagAll).ToString();
                    friend.socialType = SocialType.Steam;
                    friends.Add(friend);
                }
                callback(GameServiceResult.Succeeded, friends);
            }
            else
            {
                callback(GameServiceResult.Succeeded, null);    
            }
#endif
        }

        static List<SocialPlayerInfo> ConvertSocialPlayerInfoList(List<string> ids)
        {
            if (ids == null)
            {
                return null;
            }
            List<SocialPlayerInfo> friends = new List<SocialPlayerInfo>();

            foreach (string id in ids)
            {
                SocialPlayerInfo friend = new SocialPlayerInfo();
                friend.id = id;
#if UNITY_ANDROID
                friend.socialType = SocialType.GoogleGameService;
#elif UNITY_IOS
                friend.socialType = SocialType.GameCenter;
#elif UNITY_STANDALONE && USE_STEAM
                friend.socialType = SocialType.Steam;
#endif
                friends.Add(friend);
            }
            return friends;
        }

        static void LoadFBFriendsList(GameServiceResult prevResult, List<SocialPlayerInfo> friends, OnLoadFriendsList callback)
        {
            if(FB.IsLoggedIn) {
                FB.API("/me/friends?fields=id,name", HttpMethod.GET, (IGraphResult result) => {
                    IList list = (IList)result.ResultDictionary["data"];
                    List<SocialPlayerInfo> newFriends = new List<SocialPlayerInfo>();
                    if(friends != null)
                    {
                        foreach(SocialPlayerInfo friend in friends) {
                            newFriends.Add(friend);
                        }    
                    }

                    if (list.Count > 0)
                    {
                        foreach (var dict in list)
                        {
                            SocialPlayerInfo friend = new SocialPlayerInfo();
                            friend.id = ((Dictionary<string, object>)dict)["id"].ToString();
                            friend.name = ((Dictionary<string, object>)dict)["name"].ToString();
                            friend.socialType = SocialType.Facebook;
                            newFriends.Add(friend);
                        }
                    }
                    callback(GameServiceResult.Succeeded, newFriends);
                });
            }
            else {
                callback(GameServiceResult.LoginRequired, friends);
            }
        }

        public static void LoadFBInvitableFriendsList(OnLoadFriendsList callback)
        {
            /*
            * 페이스북 캔버스 앱이 있어야 사용할 수 있음. 결론은 모바일 앱만 개발해서는 사용할 수 없음. T.T
            */
            if (FB.IsLoggedIn)
            {
                FB.API("/me/invitable_friends?fields=id,name,picture.width(128).height(128).type(normal)", HttpMethod.GET, (IGraphResult result) => {
                    IList list = (IList)result.ResultDictionary["data"];
                    List<SocialPlayerInfo> newFriends = new List<SocialPlayerInfo>();
                    if (list.Count > 0)
                    {
                        foreach (var dict in list)
                        {
                            SocialPlayerInfo friend = new SocialPlayerInfo();
                            friend.id = ((Dictionary<string, object>)dict)["id"].ToString();
                            friend.name = ((Dictionary<string, object>)dict)["name"].ToString();
                            friend.socialType = SocialType.Facebook;
                            newFriends.Add(friend);
                        }
                    }
                    callback(GameServiceResult.Succeeded, newFriends);
                });
            }
            else
            {
                callback(GameServiceResult.LoginRequired, null);
            }
        }

        public static void LoadPlayerInfo(SocialPlayerInfo playerInfo, PhotoLoad downloadPhoto, OnLoadPlayerInfo callback)
        {
            if (playerInfo.socialType == SocialType.Facebook)
            {
                if (FB.IsLoggedIn == false)
                {
                    callback(GameServiceResult.LoginRequired, playerInfo);
                    return;
                }
                if (playerInfo.photo == null)
                {
                    if (downloadPhoto == PhotoLoad.HiRes)
                    {
                        FB.API(playerInfo.id + "/picture?type=square&height=512&width=512", HttpMethod.GET, (IGraphResult picresult) =>
                        {
                            playerInfo.photo = picresult.Texture;
                            callback(GameServiceResult.Succeeded, playerInfo);
                        });
                    }
                    else if (downloadPhoto == PhotoLoad.Normal)
                    {
                        FB.API(playerInfo.id + "/picture?type=square&height=128&width=128", HttpMethod.GET, (IGraphResult picresult) =>
                        {
                            playerInfo.photo = picresult.Texture;
                            callback(GameServiceResult.Succeeded, playerInfo);
                        });
                    }
                    else
                    {
                        playerInfo.photo = null;
                        callback(GameServiceResult.Succeeded, playerInfo);
                    }
                }
                else
                {
                    callback(GameServiceResult.Succeeded, playerInfo);
                }
                return;
            }


            if (IsLoggedIn() == false)
            {
                callback(GameServiceResult.LoginRequired, playerInfo);
                return;
            }
#if UNITY_ANDROID
            AN_PlayersClient client = AN_Games.GetPlayersClient();
            client.GetCurrentPlayer((result) => {
                if (result.IsSucceeded)
                {
                    AN_Player player = result.Data;

                    if (player != null)
                    {
                        playerInfo.name = player.DisplayName;
                        if (downloadPhoto == PhotoLoad.HiRes && player.HasHiResImage)
                        {
                            var url = player.HiResImageUri;
                            AN_ImageManager manager = new AN_ImageManager();
                            manager.LoadImage(url, (imaheLoadResult) =>
                            {
                                if (imaheLoadResult.IsSucceeded)
                                {
                                    playerInfo.photo = imaheLoadResult.Image;
                                }
                                else
                                {
                                    //Or you may want to assing some default texture here
                                    playerInfo.photo = null;
                                }
                                ThreadSafeDispatcher.Instance.Invoke(() =>
                                {
                                    callback(GameServiceResult.Succeeded, playerInfo);
                                });
                            });
                        }
                        else if (downloadPhoto == PhotoLoad.Normal && player.HasIconImage)
                        {
                            var url = player.IconImageUri;
                            AN_ImageManager manager = new AN_ImageManager();
                            manager.LoadImage(url, (imaheLoadResult) =>
                            {
                                if (imaheLoadResult.IsSucceeded)
                                {
                                    playerInfo.photo = imaheLoadResult.Image;
                                }
                                else
                                {
                                    //Or you may want to assing some default texture here
                                    playerInfo.photo = null;
                                }
                                ThreadSafeDispatcher.Instance.Invoke(() =>
                                {
                                    callback(GameServiceResult.Succeeded, playerInfo);
                                });
                            });
                        }
                        else
                        {
                            playerInfo.photo = null;
                            callback(GameServiceResult.Succeeded, playerInfo);
                        }
                    }
                    else
                    {
                        callback(GameServiceResult.Failed, playerInfo);
                    }
                }
            });
#elif UNITY_IOS
            ISN_GKLocalPlayer player = ISN_GKLocalPlayer.LocalPlayer;
            if (player != null && player.PlayerID.Equals(playerInfo.id))
            {
                playerInfo.name = player.Alias;
                playerInfo.photo = null;
                callback(GameServiceResult.Succeeded, playerInfo);
            }
            else
            {
                callback(GameServiceResult.Failed, playerInfo);
            }
#elif UNITY_STANDALONE && USE_STEAM
            CSteamID steamID = new CSteamID(ulong.Parse(playerInfo.id));
            playerInfo.name = SteamFriends.GetFriendPersonaName(steamID);
            try {
                int photoId = SteamFriends.GetLargeFriendAvatar(steamID);
                int bufSize = 4 * 184 * 184 * sizeof(char);
                byte[] buf = new byte[bufSize];
                if (SteamUtils.GetImageRGBA(photoId, buf, bufSize))
                {
                    Texture2D org = new Texture2D(184, 184);
                    org.LoadRawTextureData(buf);
                    org.Apply();

                    playerInfo.photo = new Texture2D(184, 184);
                    int xN = org.width;
                    int yN = org.height;
                    for (int i = 0; i < xN; i++)
                    {
                        for (int j = 0; j < yN; j++)
                        {
                            playerInfo.photo.SetPixel(j, xN - i - 1, org.GetPixel(j, i));
                        }
                    }
                    playerInfo.photo.Apply();
                }
                else
                {
                    playerInfo.photo = null;
                }
            }
            catch
            {
                playerInfo.photo = null;
            }
            callback(GameServiceResult.Succeeded, playerInfo);
#endif
        }
    
        
    }
}


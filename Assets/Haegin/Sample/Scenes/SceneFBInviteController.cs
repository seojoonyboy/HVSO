using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Haegin;

/*
 * 페이스북 AppRequest를 통한 친구 초대 기능은 iOS/Android앱과 별도로 Facebook Canvas 앱을 별도로 개발해서 Facebook에 등록한 경우에만 가능합니다.
 * 필요한 경우 페이스북 서비스를 만들어야합니다.
 */
public class SceneFBInviteController : MonoBehaviour
{
    public Canvas canvas;
    public GameObject friendPanel;
    public GameObject systemDialog;
    public GameObject eulaText;

    private void Awake()
    {
        ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(OnSystemBackKey);
    }

    public void OnSystemBackKey()
    {
        UGUICommon.ShowYesNoDialog(systemDialog, eulaText, canvas, TextManager.GetString(TextManager.StringTag.Quit), TextManager.GetString(TextManager.StringTag.QuitConfirm), (UGUICommon.ButtonType buttonType) =>
        {
            if (buttonType == UGUICommon.ButtonType.Yes)
            {
                ThreadSafeDispatcher.ApplicationQuit();
            }
        });
    }

    void OnDestroy()
    {
        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
    }

    void Start()
    {
        GameServiceSocial.LoadFBInvitableFriendsList((GameServiceResult result, List<SocialPlayerInfo> friends) =>
        {
            if (friends != null && friends.Count > 0)
            {
                GameObject contentObject = GameObject.Find("FriendsContent");
                for (int i = 0; i < friends.Count; i++)
                {
                    GameObject textBlock = (GameObject)Object.Instantiate(friendPanel);
                    textBlock.transform.SetParent(contentObject.transform);
                    textBlock.transform.localRotation = Quaternion.identity;
                    textBlock.transform.localPosition = Vector3.zero;
                    textBlock.transform.localScale = Vector3.one;
                    textBlock.transform.GetComponentsInChildren<Text>()[0].text = friends[i].name + "(" + friends[i].id + ")";
                    textBlock.name = i.ToString();
                    textBlock.GetComponent<LayoutElement>().preferredWidth = 860;
                    textBlock.GetComponent<LayoutElement>().preferredHeight = 128;
                    if (friends[i].photo != null)
                    {
                        textBlock.transform.GetComponentsInChildren<RawImage>()[0].texture = friends[i].photo;
                    }
                }
            }
            else
            {
#if MDEBUG
                Debug.Log("no friends");
#endif
            }
        });
    }

    public void OnCloseButton()
    {
        SceneManager.LoadScene("SceneGameService", LoadSceneMode.Single);
    }

    public void OnInvite()
    {
    }
}

using UnityEngine;
using Haegin;
using UnityEngine.UI;

public class AccountDialogController : MonoBehaviour
{
    public Canvas canvas;
    public GameObject linkDialog;
    public GameObject selectDialog;
    public GameObject logoutDialog;

    public void OpenSelectDialog(Account.DialogType type, AccountType localAccountType, string localAccountName, string authAccountName, byte[] accountInfo, Account.OnSelect callback)
    {
        // 이 함수를 게임에 맞춰서 커스터마이징 하시면 됩니다.
        // accountInfo 에는 게임별로 커스터마이징된 계정에 대한 정보가 들어옵니다.    accountInfo를 디코딩하는 방법은 게임별로 상이합니다.    accountInfo는 null일 수 있습니다.
        // 해당 내용을 아래 텍스트 대신에 사용하면, 더 명확한 계정 데이터 선택을 유도할 수 있습니다.
        switch (type)
        {
            case Account.DialogType.Link:
                ThreadSafeDispatcher.Instance.Invoke(() =>
                {
                    GameObject SelectDialog = (GameObject)Instantiate(linkDialog);
                    SelectDialog.transform.SetParent(canvas.transform);
                    SelectDialog.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    SelectDialog.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                    //
                    //   Msg:  localAccountName 계정에 새로 로그인 한 authAccountName 계정을 연결하시겠습니까? 
                    // 
                    //   Yes:  예, 로그인하고 계정을 연동하겠습니다.  
                    //
                    //   No:   아니오, 로그인을 취소하겠습니다.
                    // 
                    GameObject.Find("MsgText").GetComponent<Text>().text = string.Format(TextManager.GetString(TextManager.StringTag.AccountLinkMsg2), localAccountName, authAccountName);

                    ThreadSafeDispatcher.OnSystemBackKey onSystemBack = () =>
                    {
#if MDEBUG
                        Debug.Log("Dialog System Back Key");
#endif
                    };
                    ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(onSystemBack);

                    GameObject.Find("Yes").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                        callback(Account.SelectButton.YES);
                        Destroy(SelectDialog);
                    });
                    GameObject.Find("No").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                        callback(Account.SelectButton.NO);
                        Destroy(SelectDialog);
                    });
                });
                break;
            case Account.DialogType.Select:
                {
                    GameObject SelectDialog = (GameObject)Instantiate(selectDialog);
                    SelectDialog.transform.SetParent(canvas.transform);
                    SelectDialog.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    SelectDialog.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                    //
                    //   Msg:  새로 로그인한 계정에 연동된 이전 게임 데이터가 있습니다. 새로 로그인한 계정의 데이터를 불러와서 사용하시겠습니까? 
                    // 
                    //   Yes:  현재 플레이 중이던 데이터를 포기하고, 새로 로그인한 계정의 예전 게임 데이터를 불러와서 사용
                    //
                    //   No:   새로 로그인한 계정의 예전 게임 데이터를 포기하고, 현재 데이터를 새로 로그인한 계정의 데이타로 연결해서 사용
                    // 
                    ThreadSafeDispatcher.OnSystemBackKey onSystemBack = () =>
                    {
#if MDEBUG
                        Debug.Log("Dialog System Back Key");
#endif
                    };
                    ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(onSystemBack);

                    GameObject.Find("Yes").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                        callback(Account.SelectButton.YES);
                        Destroy(SelectDialog);
                    });
                    GameObject.Find("No").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                        callback(Account.SelectButton.NO);
                        Destroy(SelectDialog);
                    });
                }
                break;
            case Account.DialogType.CannotLogin:
                {
                    GameObject SelectDialog = (GameObject)Instantiate(logoutDialog);
                    SelectDialog.transform.SetParent(canvas.transform);
                    SelectDialog.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    SelectDialog.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                    //
                    //   Msg:  이미 다른 게임 계정에 연결되어 있어서 로그인이 불가능합니다.
                    // 
                    //   Yes:  확인
                    //
                    ThreadSafeDispatcher.OnSystemBackKey onSystemBack = () =>
                    {
#if MDEBUG
                        Debug.Log("Dialog System Back Key");
#endif
                    };
                    ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(onSystemBack);

                    GameObject.Find("Ok").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                        callback(Account.SelectButton.YES);
                        Destroy(SelectDialog);
                    });
                }
                break;
            case Account.DialogType.Logout:
                {
#if UNITY_IOS
                    GameObject SelectDialog = (GameObject)Instantiate(logoutDialog);
                    SelectDialog.transform.SetParent(canvas.transform);
                    SelectDialog.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    SelectDialog.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                    //
                    //   Msg:  이미 다른 게임 계정에 연결된 계정으로 로그인을 시도하셨습니다. 현재 계정을 로그아웃하고 새로운 계정으로 로그인합니다.
                    // 
                    //   Yes:  확인
                    //
                    GameObject.Find("MsgText").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.AccountLogoutMsg2);

                    ThreadSafeDispatcher.OnSystemBackKey onSystemBack = () =>
                    {
#if MDEBUG
                        Debug.Log("Dialog System Back Key");
#endif
                    };
                    ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(onSystemBack);

                    GameObject.Find("Ok").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                        callback(Account.SelectButton.YES);
                        Destroy(SelectDialog);
                    });
#else
                    GameObject SelectDialog = (GameObject)Instantiate(linkDialog);
                    SelectDialog.transform.SetParent(canvas.transform);
                    SelectDialog.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    SelectDialog.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);

                    //
                    //   Msg:  이미 다른 게임 계정에 연결된 계정으로 로그인을 시도하셨습니다. 현재 계정을 로그아웃하고 새로운 계정으로 로그인 하시겠습니까?
                    // 
                    //   Yes:  예, 새로운 계정으로 로그인하겠습니다.  
                    //
                    //   No:   아니오, 로그인을 취소하겠습니다.
                    // 
                    GameObject.Find("MsgText").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.AccountLogoutMsg);
                    GameObject.Find("YesText").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.AccountLogout1);
                    GameObject.Find("NoText").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.AccountLink4);

                    ThreadSafeDispatcher.OnSystemBackKey onSystemBack = () =>
                    {
#if MDEBUG
                        Debug.Log("Dialog System Back Key");
#endif
                    };
                    ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(onSystemBack);

                    GameObject.Find("Yes").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                        callback(Account.SelectButton.YES);
                        Destroy(SelectDialog);
                    });
                    GameObject.Find("No").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                        callback(Account.SelectButton.NO);
                        Destroy(SelectDialog);
                    });
#endif
                }
                break;
            case Account.DialogType.LoginFromIOSSetting:
                {
                    GameObject SelectDialog = (GameObject)Instantiate(logoutDialog);
                    SelectDialog.transform.SetParent(canvas.transform);
                    SelectDialog.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 0);
                    SelectDialog.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);
                    //
                    //   Msg:  단말기 세팅에서 게임센터계정에 로그인하신 후 사용하실 수 있습니다.
                    // 
                    //   Yes:  확인
                    //
                    GameObject.Find("MsgText").GetComponent<Text>().text = TextManager.GetString(TextManager.StringTag.AccountLoginIOSSetting);
                    ThreadSafeDispatcher.OnSystemBackKey onSystemBack = () =>
                    {
#if MDEBUG
                        Debug.Log("Dialog System Back Key");
#endif
                    };
                    ThreadSafeDispatcher.Instance.PushSystemBackKeyListener(onSystemBack);

                    GameObject.Find("Ok").GetComponent<Button>().onClick.AddListener(() =>
                    {
                        ThreadSafeDispatcher.Instance.PopSystemBackKeyListener();
                        callback(Account.SelectButton.YES);
                        Destroy(SelectDialog);
                    });
                }
                break;
        }
    }
}

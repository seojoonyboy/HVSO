using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Spine;
using Spine.Unity;


public class MenuSceneController : MonoBehaviour {
    [SerializeField] Transform fixedCanvas;
    [SerializeField] HorizontalScrollSnap windowScrollSnap;
    [SerializeField] DeckSettingManager deckSettingManager;
    [SerializeField] Transform dictionaryMenu;
    [SerializeField] SkeletonGraphic battleSwordSkeleton;
    [SerializeField] TMPro.TextMeshProUGUI nicknameText;
    [SerializeField] GameObject battleReadyPanel;   //대전 준비 화면
    private SkeletonGraphic[] buttonSkeletons = new SkeletonGraphic[5];
    protected SkeletonGraphic selectedAnimation;
    private int currentPage;
    private bool buttonClicked;
    public MyDecksLoader decksLoader;
    

    private void Start() {
        deckSettingManager.AttachDecksLoader(ref decksLoader);
        //cardDictionaryManager.AttachDecksLoader(ref decksLoader);
        decksLoader.OnLoadFinished.AddListener(() => {
            nicknameText.text = AccountManager.Instance.NickName;
        });
        decksLoader.Load();
        AccountManager.Instance.OnCardLoadFinished.AddListener(() => SetCardNumbersPerDic());
        currentPage = 2;
        Transform buttonsParent = fixedCanvas.Find("Footer");
        //for (int i = 0; i < fixedCanvas.Find("Footer").childCount; i++)
        //    buttonSkeletons[i] = buttonsParent.GetChild(i).Find("ButtonImage").GetComponent<SkeletonGraphic>();
        //StartCoroutine(UpdateWindow());
        TouchEffecter.Instance.SetScript();
        if (AccountManager.Instance.dicInfo.inDic) {
            windowScrollSnap.StartingScreen = 0;
            AccountManager.Instance.dicInfo.inDic = false;
        }
    }

    /// <summary>
    /// PVP대전 버튼 클릭
    /// </summary>
    public void OnPVPClicked() {
        battleReadyPanel.SetActive(true);
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

    public void OnStoryClicked() {
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.MISSION_SELECT_SCENE);
        SoundManager.Instance.PlaySound(SoundType.FIRST_TURN);
    }

    public void ClickMenuButton(int pageNum) {
        buttonClicked = true;
        currentPage = pageNum;
        windowScrollSnap.GoToScreen(pageNum);
        //SetButtonAnimation(pageNum);
    }
    
    private void SetButtonAnimation(int pageNum, TrackEntry trackEntry = null) {
        TrackEntry entry;
        for (int i = 0; i < buttonSkeletons.Length; i++) {
            if (i == pageNum) {
                selectedAnimation = buttonSkeletons[i];
                entry = buttonSkeletons[i].AnimationState.SetAnimation(0, "TOUCH", false);
                entry.Complete += Idle;
            }
            else {
                entry = buttonSkeletons[i].AnimationState.SetAnimation(0, "NOANI", false);
            }
        }
    }


    public void Idle(TrackEntry trackEntry = null) {
        selectedAnimation.AnimationState.SetAnimation(0, "IDLE", true);
    }

    public void SetCardNumbersPerDic() {
        int humanTotalCards = 0;
        int orcTotalCards = 0;
        int myHumanCards = 0;
        int myOrcCards = 0;
        foreach (dataModules.CollectionCard card in AccountManager.Instance.allCards) {
            if (!card.isHeroCard) {
                if (card.camp == "human") {
                    humanTotalCards++;
                    if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) {
                        myHumanCards++;
                    }
                }
                else {
                    orcTotalCards++;
                    if (AccountManager.Instance.cardPackage.data.ContainsKey(card.id)) {
                        myOrcCards++;
                    }
                }
            }
        }
        dictionaryMenu.Find("HumanButton/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = myHumanCards.ToString() + "/" + humanTotalCards.ToString();
        dictionaryMenu.Find("HumanButton/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.checkHumanCard.Count > 0);
        dictionaryMenu.Find("OrcButton/CardNum").GetComponent<TMPro.TextMeshProUGUI>().text = myOrcCards.ToString() + "/" + orcTotalCards.ToString();
        dictionaryMenu.Find("OrcButton/NewCard").gameObject.SetActive(AccountManager.Instance.cardPackage.checkOrcCard.Count > 0);
    }

    IEnumerator UpdateWindow() {
        yield return new WaitForSeconds(1.0f);
        while (true) {
            if (!buttonClicked && currentPage != windowScrollSnap.CurrentPage) {
                currentPage = windowScrollSnap.CurrentPage;
                //SetButtonAnimation(currentPage);
            }
            else {
                if (currentPage == windowScrollSnap.CurrentPage)
                    buttonClicked = false;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void OpenDictionary(bool isHuman) {
        AccountManager.Instance.dicInfo.isHuman = isHuman;
        AccountManager.Instance.dicInfo.inDic = true;
        FBL_SceneManager.Instance.LoadScene(FBL_SceneManager.Scene.DICTIONARY_SCENE);
    }
}

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
    [SerializeField] CardDictionaryManager cardDictionaryManager;
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
        cardDictionaryManager.AttachDecksLoader(ref decksLoader);
        decksLoader.OnLoadFinished.AddListener(() => {
            nicknameText.text = AccountManager.Instance.NickName;
        });
        decksLoader.Load();

        currentPage = 2;
        Transform buttonsParent = fixedCanvas.Find("Footer");
        //for (int i = 0; i < fixedCanvas.Find("Footer").childCount; i++)
        //    buttonSkeletons[i] = buttonsParent.GetChild(i).Find("ButtonImage").GetComponent<SkeletonGraphic>();
        StartCoroutine(UpdateWindow());
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
}

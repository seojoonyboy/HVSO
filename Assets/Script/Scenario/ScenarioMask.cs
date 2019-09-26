using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UniRx;
using TMPro;

public class ScenarioMask : SerializedMonoBehaviour
{
    public static ScenarioMask Instance;

    public RectTransform topMask;
    public RectTransform leftMask;
    public RectTransform rightMask;
    public RectTransform bottonMask;

    public float rectWidthRadius = 0;
    public float rectHeightRadius = 0;

    public GameObject highlightCanvas;
    public bool onHighlight = false;

    public HighlightPingpong pingpongObject;
    public GameObject fieldGlow;
    public GameObject defaultGlow;
    public GameObject blockTurnBtn;

    public Dictionary<string, GameObject> targetObject;

    public GameObject outText;



    public void GetMaskHighlight(GameObject targetObject) {
        if (targetObject == null) return;
        ActiveMask();
        RectTransform targetTransform = targetObject.GetComponent<RectTransform>();

        if (targetTransform != null) {
            Vector3 targetPos = targetTransform.position;
            Vector3[] targetRectPos = new Vector3[4];
            targetTransform.GetWorldCorners(targetRectPos);

            // Vector3 leftTop = new Vector3(targetPos.x - targetRectPos[1].x, targetRectPos[1].y - targetPos.y, 0);

            float halfHeight = targetRectPos[1].y - targetPos.y;
            float halfWidth = targetPos.x - targetRectPos[1].x;

            Vector3 top = new Vector3(0, targetPos.y + halfHeight + rectHeightRadius, 0);
            Vector3 left = new Vector3(targetPos.x - halfWidth - rectWidthRadius, 0, 0);
            Vector3 right = new Vector3(targetPos.x + halfWidth + rectWidthRadius, 0, 0);
            Vector3 bottom = new Vector3(0, targetPos.y - halfHeight - rectHeightRadius, 0);
            topMask.position = top;
            leftMask.position = left;
            rightMask.position = right;
            bottonMask.position = bottom;
        }        
    }

    public void DisableMask() {
        topMask.transform.gameObject.SetActive(false);
        leftMask.transform.gameObject.SetActive(false);
        rightMask.transform.gameObject.SetActive(false);
        bottonMask.transform.gameObject.SetActive(false);
    }

    public void ActiveMask() {
        topMask.transform.gameObject.SetActive(true);
        leftMask.transform.gameObject.SetActive(true);
        rightMask.transform.gameObject.SetActive(true);
        bottonMask.transform.gameObject.SetActive(true);
    }

    public void BlockButton() {
        blockTurnBtn.SetActive(true);
    }

    public void UnblockButton() {
        blockTurnBtn.SetActive(false);
    }




    public GameObject GetMaskingObject(string main, string sub = null, string third = null) {

        GameObject maskObject = targetObject[main].gameObject;

        if(maskObject != null) {

            if (sub == null) {
                if(main == "hands") {
                    maskObject = PlayMangement.instance.player.playerUI;
                }
                return maskObject;
            }
            else {
                if (main == "muligunCard" || main == "muligun_card") {
                    switch (sub) {
                        case "left,top":
                            maskObject = maskObject.transform.GetChild(5).gameObject;
                            break;
                        case "right,top":
                            maskObject = maskObject.transform.GetChild(6).gameObject;
                            break;
                        case "left,bottom":
                            maskObject = maskObject.transform.GetChild(7).gameObject;
                            break;
                        case "right,bottom":
                            maskObject = maskObject.transform.GetChild(8).gameObject;
                            break;
                        default:
                            maskObject = maskObject.transform.GetChild(5).gameObject;
                            break;
                    }
                    maskObject = maskObject.transform.Find("ChangeButton").gameObject;

                }
                if (main == "hand_card") {
                    foreach (Transform cardSlot in maskObject.transform) {

                        if (cardSlot.childCount < 1)
                            continue;

                        if (cardSlot.GetChild(0).GetComponent<CardHandler>().cardID == sub) {
                            maskObject = cardSlot.gameObject;
                            break;
                        }
                    }
                    if(third != null) {
                        if (third == "mana")
                            maskObject = maskObject.transform.Find("Cost").gameObject;
                    }
                }
                if (main == "mana") {
                    if (sub == "my") {
                        maskObject = PlayMangement.instance.player.playerUI.transform.Find("PlayerResource/Cost").gameObject;
                    }
                }
                if (main == "field") {
                    switch (sub) {
                        case "1":
                            maskObject = maskObject.transform.GetChild(0).gameObject;
                            break;
                        case "2":
                            maskObject = maskObject.transform.GetChild(1).gameObject;
                            break;
                        case "3":
                            maskObject = maskObject.transform.GetChild(2).gameObject;
                            break;
                        case "4":
                            maskObject = maskObject.transform.GetChild(3).gameObject;
                            break;
                        case "5":
                            maskObject = maskObject.transform.GetChild(4).gameObject;
                            break;
                        default:
                            maskObject = maskObject.transform.GetChild(0).gameObject;
                            break;
                    }


                }
                if (main == "shield-gage") {
                    maskObject = (sub == "my") ? maskObject : null;
                }

                if (main == "shield-num") {
                    if (sub == "my")
                        maskObject = (PlayMangement.instance.player.isHuman == true) ? maskObject.transform.Find("HumanShield").gameObject : maskObject.transform.Find("OrcShield").gameObject;
                    else
                        maskObject = (PlayMangement.instance.enemyPlayer.isHuman == true) ? PlayMangement.instance.enemyPlayer.playerUI.transform.Find("PlayerHealth/ShieldUI/HumanShield").gameObject : PlayMangement.instance.enemyPlayer.playerUI.transform.Find("PlayerHealth/ShieldUI/OrcShield").gameObject;

                }
                if (main == "button") {
                    if (sub == "muligunEnd") {
                        maskObject = PlayMangement.instance.player.playerUI.transform.parent.Find("FirstDrawWindow").Find("FinishButton").gameObject;
                    }
                    if (sub == "endTurn") {
                        maskObject = (PlayMangement.instance.player.isHuman) ? maskObject.transform.Find("HumanButton").gameObject : maskObject.transform.Find("Orc").gameObject;
                    }
                }

                if (main == "shieldArrow") {
                    if (sub == "top")
                        maskObject = maskObject.transform.Find("Guide_up").gameObject;
                    else
                        maskObject = maskObject.transform.Find("Guide_down").gameObject;
                }

                if(main == "shieldCard") {
                    Transform left = maskObject.transform.Find("Left");
                    Transform right = maskObject.transform.Find("Right");

                    if (left.childCount > 1 && left.GetChild(1).gameObject.GetComponent<CardHandler>().cardData.id == sub)
                        maskObject = left.GetChild(1).gameObject;

                    if (right.childCount > 1 && right.GetChild(1).gameObject.GetComponent<CardHandler>().cardData.id == sub)
                        maskObject = right.GetChild(1).gameObject;
                }



                return maskObject;
            }
        }
        return null;
    }

    float defaultAlpha = 0.004f;
    //float defaultAlpha = 0.2f;

    public void MaskScreen() {
        ActiveMask();
        topMask.position = Vector3.zero;
        leftMask.position = Vector3.zero;
        rightMask.position = Vector3.zero;
        bottonMask.position = Vector3.zero;

        Color prevColor = topMask.transform.GetComponent<Image>().color;
        topMask.transform.GetComponent<Image>().color = new Color(prevColor.r, prevColor.g, prevColor.b, defaultAlpha);
        leftMask.transform.GetComponent<Image>().color = new Color(prevColor.r, prevColor.g, prevColor.b, defaultAlpha);
        rightMask.transform.GetComponent<Image>().color = new Color(prevColor.r, prevColor.g, prevColor.b, defaultAlpha);
        bottonMask.transform.GetComponent<Image>().color = new Color(prevColor.r, prevColor.g, prevColor.b, defaultAlpha);
    }


    public void OffMaskScreen() {
        Color prevColor = topMask.transform.GetComponent<Image>().color;
        topMask.transform.GetComponent<Image>().color = new Color(prevColor.r, prevColor.g, prevColor.b, defaultAlpha);
        leftMask.transform.GetComponent<Image>().color = new Color(prevColor.r, prevColor.g, prevColor.b, defaultAlpha);
        rightMask.transform.GetComponent<Image>().color = new Color(prevColor.r, prevColor.g, prevColor.b, defaultAlpha);
        bottonMask.transform.GetComponent<Image>().color = new Color(prevColor.r, prevColor.g, prevColor.b, defaultAlpha);
    }

    private void Awake() {
        Instance = this;
        ZeroMaskPos();
    }

    private void OnDestroy() {
        Instance = null;
    }

    private void ZeroMaskPos() {
        topMask.transform.position = Vector3.zero;
        leftMask.transform.position = Vector3.zero;
        rightMask.transform.position = Vector3.zero;
        bottonMask.transform.position = Vector3.zero;
    }


    // Start is called before the first frame update
    void Start()
    {
        if (topMask == null)
            topMask = gameObject.transform.Find("TopMask").GetComponent<RectTransform>();

        if (leftMask == null)
            leftMask = gameObject.transform.Find("LeftMask").GetComponent<RectTransform>();

        if (rightMask == null)
            rightMask = gameObject.transform.Find("RightMask").GetComponent<RectTransform>();

        if (bottonMask == null)
            bottonMask = gameObject.transform.Find("BottomMask").GetComponent<RectTransform>();




        Vector3[] temp = new Vector3[4];
        topMask.GetWorldCorners(temp);


        rectWidthRadius = topMask.transform.position.x - temp[0].x;
        rectHeightRadius = rectWidthRadius;      
    }

    public void SetHighlightImage(GameObject showObject) {
        GameObject glowObject = GetUnactiveGlow();
        RectTransform glowRect = glowObject.GetComponent<RectTransform>();
        if (glowObject == null) return;

        string targetName = showObject.name;
        GameObject targetObject = showObject;
        Image glowImage = glowObject.GetComponent<Image>();


        if (targetName.Contains("CardSlot") && PlayMangement.instance.isMulligan == false) {
            targetObject = showObject.transform.GetChild(0).gameObject;
            glowRect.gameObject.SetActive(true);
            glowRect.position = targetObject.transform.Find("Portrait").position;
            glowRect.sizeDelta = targetObject.transform.Find("Portrait").gameObject.GetComponent<RectTransform>().sizeDelta;
            glowImage.sprite = targetObject.transform.Find("Portrait").gameObject.GetComponent<Image>().sprite;

            glowRect.GetChild(0).gameObject.SetActive(true);
            glowRect.GetChild(0).position = targetObject.transform.Find("BackGround").position;
            glowRect.GetChild(0).GetComponent<RectTransform>().sizeDelta = targetObject.GetComponent<RectTransform>().sizeDelta;
            glowRect.GetChild(0).gameObject.GetComponent<Image>().sprite = targetObject.transform.Find("BackGround").gameObject.GetComponent<Image>().sprite;
        }
        else if (targetName.Contains("HeroCard")) {
            glowRect.gameObject.SetActive(true);
            glowRect.position = targetObject.transform.Find("Portrait").position;
            glowRect.sizeDelta = targetObject.transform.Find("Portrait").gameObject.GetComponent<RectTransform>().sizeDelta;
            glowImage.sprite = targetObject.transform.Find("Portrait").gameObject.GetComponent<Image>().sprite;
            

            glowRect.GetChild(0).gameObject.SetActive(true);
            glowRect.GetChild(0).position = targetObject.transform.Find("BackGround").position;
            glowRect.GetChild(0).GetComponent<RectTransform>().sizeDelta = targetObject.GetComponent<RectTransform>().sizeDelta;
            glowRect.GetChild(0).gameObject.GetComponent<Image>().sprite = targetObject.transform.Find("BackGround").gameObject.GetComponent<Image>().sprite;
            glowRect.localScale = targetObject.transform.localScale;
        }

        else if (targetName.Contains("Line_")) {
            targetObject = this.targetObject["fields"].transform.GetChild(targetObject.transform.GetSiblingIndex()).gameObject;
            glowRect.gameObject.SetActive(true);
            glowRect.position = targetObject.transform.position;
            glowRect.localScale = targetObject.transform.localScale;
            glowRect.sizeDelta = targetObject.GetComponent<RectTransform>().sizeDelta;
            glowImage.sprite = targetObject.GetComponent<Image>().sprite;
            glowRect.SetParent(fieldGlow.transform);
        }
        else if (targetName.Contains("turnUI_outLine")) {
            glowRect.gameObject.SetActive(true);
            glowRect.position = targetObject.transform.position;
            glowRect.localScale = targetObject.transform.localScale;
            glowRect.sizeDelta = targetObject.GetComponent<RectTransform>().sizeDelta;
            glowImage.sprite = targetObject.GetComponent<Image>().sprite;


            GameObject turnBtn = targetObject.transform.parent.Find("Image").gameObject;
            glowRect.GetChild(0).gameObject.SetActive(true);
            glowRect.GetChild(0).position = turnBtn.transform.position;
            glowRect.GetChild(0).GetComponent<RectTransform>().sizeDelta = turnBtn.GetComponent<RectTransform>().sizeDelta;
            glowRect.GetChild(0).gameObject.GetComponent<Image>().sprite = turnBtn.GetComponent<Image>().sprite;
        }
        else if (targetName.Contains("Guide_")) {
            glowRect.gameObject.SetActive(true);
            glowRect.localScale = targetObject.transform.localScale;
            glowRect.sizeDelta = targetObject.GetComponent<RectTransform>().sizeDelta;
            glowImage.sprite = targetObject.GetComponent<Image>().sprite;
            glowObject.GetComponent<Animation>().clip = glowObject.GetComponent<Animation>().GetClip("WhiteGlowAnimation");
            Observable.EveryUpdate().TakeWhile(_ => glowRect.gameObject.activeSelf == true).Subscribe(_ => glowRect.position = targetObject.transform.position);
        }
        else if (targetName.Contains("Player1_Panel") || targetName.Contains("Player2_Panel") || targetName.Contains("FieldUI")) {
            glowRect.gameObject.SetActive(true);
            glowRect.position = targetObject.transform.position;
            glowRect.localScale = targetObject.transform.localScale;

            Vector2 size = new Vector2(Camera.main.pixelWidth, targetObject.GetComponent<RectTransform>().sizeDelta.y);            
            glowRect.sizeDelta = size;           
            glowImage.sprite = (targetObject.GetComponent<Image>() != null) ? targetObject.GetComponent<Image>().sprite : defaultGlow.GetComponent<Image>().sprite;

        }
        else {
            glowRect.gameObject.SetActive(true);
            glowRect.position = targetObject.transform.position;
            glowRect.localScale = targetObject.transform.localScale;
            glowRect.sizeDelta = targetObject.GetComponent<RectTransform>().sizeDelta;
            Debug.Log(glowRect.sizeDelta);
            glowImage.sprite = (targetObject.GetComponent<Image>() != null) ? targetObject.GetComponent<Image>().sprite : defaultGlow.GetComponent<Image>().sprite;
        }      
        Animation glowAnimation = glowObject.GetComponent<Animation>();
        glowAnimation.Play();        
    }
    public void UnmaskHeroGuide() {
        Color color = Color.white;
        color.a = 1f;

        GetMaskingObject("shieldArrow", "top").GetComponent<Image>().color = color;
        GetMaskingObject("shieldArrow", "bottom").GetComponent<Image>().color = color;
    }


    public GameObject GetUnactiveGlow() {
        foreach(Transform child in highlightCanvas.transform) {
            if(child.gameObject.activeSelf == false) {
                return child.gameObject;
            }
        }

        

        return null;
    }

    public void StopHighlight(int num) {
        GameObject glowObject = highlightCanvas.transform.GetChild(num).gameObject;
        HighlightPingpong pingpong = glowObject.transform.Find("Laber").gameObject.GetComponent<HighlightPingpong>();
        pingpong.EndPingPong();
    }

    public void StopEveryHighlight() {
        foreach (Transform child in highlightCanvas.transform) {
            if (child.gameObject.activeSelf == false)
                continue;

            Animation glowAnimation = child.gameObject.GetComponent<Animation>();
            glowAnimation.Stop();
            glowAnimation.clip = glowAnimation.GetClip("glowAnimation");
            child.gameObject.GetComponent<Image>().color = Color.white;
            child.localScale = Vector3.one;
            child.position = Vector3.one * 100f;
            child.gameObject.SetActive(false);
            child.GetChild(0).gameObject.SetActive(false);
            
        }

        if(fieldGlow.transform.childCount > 0) {

            foreach(Transform child in fieldGlow.transform) {
                Animation glowAnimation = child.gameObject.GetComponent<Animation>();
                glowAnimation.Stop();
                child.SetParent(highlightCanvas.transform);
                child.SetAsLastSibling();
                child.gameObject.SetActive(false);
            }

        }

    }    

    public void ShowText(string word = "") {
        outText.SetActive(true);

        TextMeshProUGUI setText = outText.transform.Find("OutText").gameObject.GetComponent<TextMeshProUGUI>();
        

        if (string.IsNullOrEmpty(word) == false) {
            setText.text = word;
        }
        else {
            setText.text = "터치를 누르면 진행합니다.";
        }
    }

    public void HideText() {
        outText.SetActive(false);
    }


}

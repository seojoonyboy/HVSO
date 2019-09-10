using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;

public class ScenarioMask : SerializedMonoBehaviour
{
    public static ScenarioMask Instance;

    public RectTransform topMask;
    public RectTransform leftMask;
    public RectTransform rightMask;
    public RectTransform bottonMask;

    public float rectWidthRadius = 0;
    public float rectHeightRadius = 0;


    public Dictionary<string, GameObject> targetObject;



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
                }
                if (main == "hand_card") {
                    foreach (Transform cardSlot in maskObject.transform) {

                        if (cardSlot.childCount < 1)
                            continue;

                        if (cardSlot.GetChild(0).GetComponent<CardHandler>().cardID == "sub") {
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

                if (main == "shield_num_My") {
                    maskObject = (PlayMangement.instance.player.isHuman == true) ? maskObject.transform.Find("HumanSheild").gameObject : maskObject.transform.Find("OrcSheild").gameObject;
                }
                if (main == "button") {
                    if (sub == "muligunEnd") {
                        maskObject = PlayMangement.instance.player.playerUI.transform.Find("FirstDrawWindow/FinishButton").gameObject;
                    }
                    if (sub == "endTurn") {
                        maskObject = (PlayMangement.instance.player.isHuman) ? maskObject.transform.Find("HumanButton").gameObject : maskObject.transform.Find("Orc").gameObject;
                    }
                }

                return maskObject;
            }
        }
        return null;
    }




    private void Awake() {
        Instance = this;
        DisableMask();
    }

    private void OnDestroy() {
        Instance = null;
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

}

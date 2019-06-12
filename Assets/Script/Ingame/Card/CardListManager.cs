using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Spine;
using Spine.Unity;


public class CardListManager : MonoBehaviour
{
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform contentParent;
    [SerializeField] Transform standbyInfo;
    [SerializeField] GameObject infoPrefab;
    [SerializeField] GameObject firstInfoPrefab;
    [SerializeField] Transform mulliganInfoList;
    Animator animator;
    HorizontalScrollSnap hss;

    void Start()
    {
        transform.GetComponent<Image>().enabled = false;
        animator = transform.GetComponentInChildren<Animator>();
        animator.SetBool("Hide", true);
        hss = transform.GetComponentInChildren<HorizontalScrollSnap>();
    }

    public void AddCardInfo(CardData data, string id) {
        GameObject newcard = standbyInfo.GetChild(0).gameObject;
        SetCardInfo(newcard, data);
        hss.AddChild(newcard);
        GameObject unitSpine = newcard.transform.Find("Info/UnitImage").GetChild(0).gameObject;
        if (data.type == "unit") {
            unitSpine.GetComponent<SkeletonGraphic>().skeletonDataAsset = AccountManager.Instance.resource.cardPreveiwSkeleton[id].GetComponent<SkeletonGraphic>().skeletonDataAsset;
            unitSpine.GetComponent<SkeletonGraphic>().Initialize(true);
            unitSpine.SetActive(true);
        }
        else
            unitSpine.SetActive(false);
    }

    public void AddMulliganCardInfo(CardData data, string id, int changeNum = 100) {
        //GameObject newcard = Instantiate(firstInfoPrefab, mulliganInfoList);
        GameObject newcard;
        if (changeNum == 100) {
            newcard = standbyInfo.GetChild(0).gameObject;
            newcard.transform.SetParent(mulliganInfoList);
        }
        else
            newcard = mulliganInfoList.GetChild(changeNum).gameObject;
        SetCardInfo(newcard, data);
        GameObject unitSpine = newcard.transform.Find("Info/UnitImage").GetChild(0).gameObject;
        if (data.type == "unit") {
            unitSpine.GetComponent<SkeletonGraphic>().skeletonDataAsset = AccountManager.Instance.resource.cardPreveiwSkeleton[id].GetComponent<SkeletonGraphic>().skeletonDataAsset;
            unitSpine.GetComponent<SkeletonGraphic>().Initialize(true);
            unitSpine.SetActive(true);
        }
        else
            unitSpine.SetActive(false);
        newcard.transform.Find("Info/SimpleImage/Chain").gameObject.SetActive(false);
        newcard.transform.position = new Vector3(0, 0, 0);
        newcard.SetActive(false);
    }

    public void AddFeildUnitInfo(CardData data, int unitNum) {
        GameObject newcard = standbyInfo.GetChild(0).gameObject;
        newcard.name = unitNum.ToString() + "unit";
        newcard.transform.SetParent(transform.Find("FieldUnitInfo"));
        SetCardInfo(newcard, data);
        newcard.transform.Find("Info/UnitImage").GetChild(0).gameObject.SetActive(false);
    }


    public void SendMulliganInfo() {
        int i = 0;
        while(i < 4) {
            standbyInfo.GetChild(0).Find("Info/SimpleImage/Chain").gameObject.SetActive(true);

            mulliganInfoList.GetChild(0).gameObject.SetActive(true);
            hss.AddChild(mulliganInfoList.GetChild(0).gameObject);
            i++;
        }
    }

    public void OpenMulliganCardList(int cardnum) {
        if (PlayMangement.movingCard == null) {
            mulliganInfoList.gameObject.SetActive(true);
            mulliganInfoList.GetChild(cardnum).gameObject.SetActive(true);
        }
    }

    public void CloseMulliganCardList() {
        mulliganInfoList.gameObject.SetActive(false);
        for (int i = 0; i < 4; i++) {
            mulliganInfoList.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void RemoveCardInfo(int index) {
        GameObject remove;
        hss.RemoveChild(index, out remove);
        remove.transform.SetParent(standbyInfo);
        remove.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        remove.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        remove.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        remove.transform.localScale = new Vector3(1, 1, 1);
        remove.transform.localPosition = new Vector3(0, 0, 0);
        remove.SetActive(false);
    }

    public void RemoveUnitInfo(int index) {
        string objName = index.ToString() + "unit";
        Transform remove = transform.Find("FieldUnitInfo").Find(objName);
        remove.SetParent(standbyInfo);
        remove.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        remove.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        remove.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
        remove.localScale = new Vector3(1, 1, 1);
        remove.localPosition = new Vector3(0, 0, 0);
        remove.name = "CardInfoPage";
        remove.gameObject.SetActive(false);
    }

    public void OpenCardList(int cardnum) {
        PlayMangement.instance.infoOn = true;
        transform.GetComponent<Image>().enabled = true;
        animator.SetBool("Hide", false);
        hss.GoToScreen(cardnum);
    }

    public void SetCardInfo(GameObject obj, CardData data) {
        Transform info = obj.transform.GetChild(0);
        info.Find("Name/NameText").GetComponent<Text>().text = data.name;
        if (data.rarelity != "legend") {
            info.Find("Name").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites[data.rarelity + "_ribon"];
            info.Find("UnitDialogue").GetComponent<Image>().sprite = AccountManager.Instance.resource.infoSprites[data.rarelity + "_flag"];
        }
        if (data.hp != null)
            info.Find("HP/HpText").GetComponent<Text>().text = data.hp.ToString();
        else
            info.Find("HP").gameObject.SetActive(false);

        if (data.attack != null)
            info.Find("Attack/AttackText").GetComponent<Text>().text = data.attack.ToString();
        else
            info.Find("Attack").gameObject.SetActive(false);

        if (data.category_1 != null)
            info.Find("Category_1").GetComponent<Text>().text = data.category_1.ToString();
        else
            info.Find("Category_1").gameObject.SetActive(false);

        if (data.category_2 != null)
            info.Find("Category_2").GetComponent<Text>().text = data.category_2.ToString();
        else
            info.Find("Category_2").gameObject.SetActive(false);

        info.Find("Cost/CostText").GetComponent<Text>().text = data.cost.ToString();

        obj.transform.Find("Class1Button").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[data.class_1];
        obj.transform.Find("Class1Button").name = data.class_1;
        if (data.class_2 == null)
            obj.transform.Find("Class2Button").gameObject.SetActive(false);
        else {
            obj.transform.Find("Class2Button").GetComponent<Image>().sprite = AccountManager.Instance.resource.classImage[data.class_2];
            obj.transform.Find("Class2Button").name = data.class_2;
        }
        if(data.skills.Length != 0) {
            info.Find("SkillInfo").GetComponent<Text>().text = data.skills[0].desc;
        }
        obj.SetActive(true);
    }


    public void OpenUnitInfoWindow(Vector3 inputPos) {
        if (Input.GetMouseButtonDown(0)) {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(inputPos);

            LayerMask mask = (1 << LayerMask.NameToLayer("UnitInfo"));
            RaycastHit2D[] hits = Physics2D.RaycastAll(
                new Vector2(mousePos.x, mousePos.y),
                Vector2.zero,
                Mathf.Infinity,
                mask
            );

            if (hits != null) {
                foreach (RaycastHit2D hit in hits) {
                    GameObject selectedTarget = hit.collider.gameObject;
                    //if (selectedTarget.GetComponentInParent<ambush>() && !selectedTarget.GetComponentInParent<PlaceMonster>().isPlayer) return;
                    string objName = selectedTarget.GetComponentInParent<PlaceMonster>().myUnitNum.ToString() + "unit";
                    transform.Find("FieldUnitInfo").gameObject.SetActive(true);
                    transform.Find("FieldUnitInfo").Find(objName).gameObject.SetActive(true);
                    PlayMangement.instance.infoOn = true;
                }
            }
        }
    }

    public void CloseUnitInfoWindow() {
        transform.Find("FieldUnitInfo").gameObject.SetActive(false);
        for (int i = 0; i < transform.Find("FieldUnitInfo").childCount; i++) {
            transform.Find("FieldUnitInfo").GetChild(i).gameObject.SetActive(false);
        }
        PlayMangement.instance.infoOn = false;
    }

}

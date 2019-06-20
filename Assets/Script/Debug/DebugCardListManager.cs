using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Spine;
using Spine.Unity;

public class DebugCardListManager : CardListManager
{
    private void Start() {
        hss = transform.GetComponentInChildren<HorizontalScrollSnap>();
        
    }

    public override void AddCardInfo(CardData data, string id) {
        GameObject newcard = standbyInfo.GetChild(0).gameObject;
        SetCardInfo(newcard, data);
        hss.AddChild(newcard);
        GameObject unitSpine = newcard.transform.Find("Info/UnitImage").GetChild(0).gameObject;
        if (data.type == "unit") {
            unitSpine.GetComponent<SkeletonGraphic>().skeletonDataAsset = DebugManagement.instance.GetComponent<ResourceManager>().cardPreveiwSkeleton[id].GetComponent<SkeletonGraphic>().skeletonDataAsset;
            unitSpine.GetComponent<SkeletonGraphic>().Initialize(true);
            unitSpine.SetActive(true);
        }
        else
            unitSpine.SetActive(false);
    }

    public override void AddMulliganCardInfo(CardData data, string id, int changeNum = 100) {
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
            unitSpine.GetComponent<SkeletonGraphic>().skeletonDataAsset = DebugManagement.instance.GetComponent<ResourceManager>().cardPreveiwSkeleton[id].GetComponent<SkeletonGraphic>().skeletonDataAsset;
            unitSpine.GetComponent<SkeletonGraphic>().Initialize(true);
            unitSpine.SetActive(true);
        }
        else
            unitSpine.SetActive(false);
        newcard.transform.Find("Info/SimpleImage/Chain").gameObject.SetActive(false);
        newcard.transform.position = new Vector3(0, 0, 0);
        newcard.SetActive(false);
    }

    public override void SetCardInfo(GameObject obj, CardData data) {
        Transform info = obj.transform.GetChild(0);
        info.Find("Name/NameText").GetComponent<Text>().text = data.name;
        if (data.rarelity != "legend") {
            info.Find("Name").GetComponent<Image>().sprite = DebugManagement.instance.GetComponent<ResourceManager>().infoSprites[data.rarelity + "_ribon"];
            info.Find("UnitDialogue").GetComponent<Image>().sprite = DebugManagement.instance.GetComponent<ResourceManager>().infoSprites[data.rarelity + "_flag"];
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

        obj.transform.GetChild(1).GetComponent<Image>().sprite = DebugManagement.instance.GetComponent<ResourceManager>().classImage[data.class_1];
        obj.transform.GetChild(1).name = data.class_1;
        if (data.class_2 == null)
            obj.transform.GetChild(2).gameObject.SetActive(false);
        else {
            obj.transform.GetChild(2).GetComponent<Image>().sprite = DebugManagement.instance.GetComponent<ResourceManager>().classImage[data.class_2];
            obj.transform.GetChild(2).name = data.class_2;
        }
        if (data.skills.Length != 0) {
            info.Find("SkillInfo").GetComponent<Text>().text = data.skills[0].desc;
        }
        obj.SetActive(true);
    }


    public override void OpenUnitInfoWindow(Vector3 inputPos) {
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
                    if (selectedTarget.GetComponentInParent<ambush>() && !selectedTarget.GetComponentInParent<PlaceMonster>().isPlayer) return;
                    string objName = selectedTarget.GetComponentInParent<PlaceMonster>().myUnitNum.ToString() + "unit";
                    transform.Find("FieldUnitInfo").gameObject.SetActive(true);
                    transform.Find("FieldUnitInfo").Find(objName).gameObject.SetActive(true);
                    DebugManagement.instance.infoOn = true;
                }
            }
        }
    }

    public override void CloseUnitInfoWindow() {
        transform.Find("FieldUnitInfo").gameObject.SetActive(false);
        for (int i = 0; i < transform.Find("FieldUnitInfo").childCount; i++) {
            transform.Find("FieldUnitInfo").GetChild(i).gameObject.SetActive(false);
        }
        DebugManagement.instance.infoOn = false;
    }


}

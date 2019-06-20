using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugCardHandler : CardHandler {
    private void Awake() {
        csm = DebugManagement.instance.cardInfoCanvas.Find("CardInfoList").GetComponent<CardListManager>();
    }

    public override void DrawCard(string ID, int itemID = -1, bool first = false) {
        cardData = DebugData.Instance.cardData[ID];
        cardID = ID;

        if (DebugData.Instance.cardData.ContainsKey(cardID)) {
            cardData = DebugData.Instance.cardData[cardID];
            transform.Find("Portrait").GetComponent<Image>().sprite = DebugManagement.instance.GetComponent<ResourceManager>().cardPortraite[cardID];
            transform.Find("BackGround").GetComponent<Image>().sprite = DebugManagement.instance.GetComponent<ResourceManager>().cardBackground[cardData.type + "_" + cardData.rarelity];
            skeleton = DebugManagement.instance.GetComponent<ResourceManager>().cardSkeleton[cardID];
        }
        else
            Debug.Log("NoData");
        if (cardData.type == "unit") {
            transform.Find("Health").gameObject.SetActive(true);
            transform.Find("attack").gameObject.SetActive(true);
            transform.Find("Health").Find("Text").GetComponent<Text>().text = cardData.hp.ToString();
            transform.Find("attack").Find("Text").GetComponent<Text>().text = cardData.attack.ToString();
        }
        else {
            transform.Find("Health").gameObject.SetActive(false);
            transform.Find("attack").gameObject.SetActive(false);
        }
        transform.Find("Cost").Find("Text").GetComponent<Text>().text = cardData.cost.ToString();

    }

    public override void ActivateCard() {

        isDropable = true;
        if (cardData.cost <= DebugPlayer.activeCardMinCost)
            DebugPlayer.activeCardMinCost = cardData.cost;
        transform.Find("GlowEffect").GetComponent<Image>().enabled = true;
        transform.Find("GlowEffect").GetComponent<Image>().color = new Color(1, 1, 1);
        transform.Find("Portrait").GetComponent<Image>().color = Color.white;
        transform.Find("attack").GetComponent<Image>().color = Color.white;
        transform.Find("Health").GetComponent<Image>().color = Color.white;
        transform.Find("Cost").GetComponent<Image>().color = Color.white;

    }

    protected override void CheckLocation(bool off = false) {
        if (off) {
            pointOnFeild = false;
            if (cardData.type == "unit")
                DebugCardInfoOnDrag.instance.ActivePreviewUnit(false);
            return;
        }
        if (transform.localPosition.y > -350) {
            if (!pointOnFeild) {
                pointOnFeild = true;
                transform.localScale = new Vector3(0, 0, 0);
                if (cardData.type == "unit")
                    DebugCardInfoOnDrag.instance.ActivePreviewUnit(true);
            }
        }
        else {
            if (pointOnFeild) {
                pointOnFeild = false;
                transform.localScale = new Vector3(1, 1, 1);
                if (cardData.type == "unit")
                    DebugCardInfoOnDrag.instance.ActivePreviewUnit(false);
            }
        }
    }

    public override void CheckHighlight() {
        if (!highlighted) {
            highlightedSlot = CheckSlot();
            if (highlightedSlot != null) {
                highlighted = true;
                transform.Find("GlowEffect").GetComponent<Image>().color = new Color(163.0f / 255.0f, 236.0f / 255.0f, 27.0f / 255.0f);
                transform.Find("GlowEffect").localScale = new Vector3(1.05f, 1.05f, 1);
                DebugCardDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
            }
        }
        else {
            if (highlightedSlot != CheckSlot()) {
                highlighted = false;
                if (isDropable)
                    transform.Find("GlowEffect").GetComponent<Image>().color = new Color(1, 1, 1);
                else
                    transform.Find("GlowEffect").GetComponent<Image>().color = new Color(1, 1, 1);
                transform.Find("GlowEffect").localScale = new Vector3(1, 1, 1);
                DebugCardDropManager.Instance.HighLightSlot(highlightedSlot, highlighted);
                highlightedSlot = null;
            }
        }
    }

}

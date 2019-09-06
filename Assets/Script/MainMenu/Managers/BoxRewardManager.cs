using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BoxRewardManager : MonoBehaviour
{
    [SerializeField] Transform boxObject;
    // Start is called before the first frame update
    Transform hudCanvas;
    void Start()
    {
        hudCanvas = transform.parent;
    }

    public void SetBoxObj() {
        boxObject.Find("SupplyGauge/Value").GetComponent<Image>().fillAmount = (float)AccountManager.Instance.userResource.supply * 0.01f;
        boxObject.Find("SupplyGauge/ValueText").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.supply.ToString() + "/100";
        if (AccountManager.Instance.userResource.supplyBox > 0) {
            boxObject.Find("BoxImage/BoxValue").gameObject.SetActive(true);
            boxObject.Find("BoxImage/BoxValue/BoxNum").GetComponent<TMPro.TextMeshProUGUI>().text = AccountManager.Instance.userResource.supplyBox.ToString();
        }
        else
            boxObject.Find("BoxImage/BoxValue").gameObject.SetActive(false);
    }

    public void OpenBox() {
        transform.Find("ShowBox").gameObject.SetActive(true);
    }

    public void GetResult() {
        transform.Find("ShowBox/Text").gameObject.SetActive(false);
        StartCoroutine(ShowRewards());
    }

    IEnumerator ShowRewards() {
        Transform boxParent = transform.Find("OpenBox");
        boxParent.gameObject.SetActive(true);
        iTween.ScaleTo(boxParent.GetChild(0).gameObject, iTween.Hash("x", 1, "y", 1, "islocal", true, "time", 0.2f));
        yield return new WaitForSeconds(0.2f);
        iTween.ScaleTo(boxParent.GetChild(1).gameObject, iTween.Hash("x", 1, "y", 1, "islocal", true, "time", 0.2f));
        yield return new WaitForSeconds(0.2f);
        iTween.ScaleTo(boxParent.GetChild(2).gameObject, iTween.Hash("x", 1, "y", 1, "islocal", true, "time", 0.2f));
        yield return new WaitForSeconds(0.2f);
        iTween.ScaleTo(boxParent.GetChild(3).gameObject, iTween.Hash("x", 1, "y", 1, "islocal", true, "time", 0.2f));
        yield return new WaitForSeconds(0.5f);
        transform.Find("ExitButton").gameObject.SetActive(true);
    }

    public void ExitBoxOpen() {
        Transform boxParent = transform.Find("OpenBox");
        for (int i = 0; i < 4; i++) 
            boxParent.GetChild(i).localScale = Vector3.zero;
        boxParent.gameObject.SetActive(false);
        transform.Find("ShowBox").gameObject.SetActive(false);
        transform.Find("ExitButton").gameObject.SetActive(false);
        for (int i = 0; i < 3; i++)
            boxParent.GetChild(2).GetChild(i).gameObject.SetActive(false);
        SetBoxObj();
    }



    public void SetRewards(RewardClass[] rewardList) {
        Transform boxParent = transform.Find("OpenBox");
        boxParent.GetChild(0).GetChild(0).GetComponent<MenuCardHandler>().DrawCard(rewardList[0].item);
        if (rewardList[0].amount > 0) {
            boxParent.GetChild(0).GetChild(1).gameObject.SetActive(true);
            boxParent.GetChild(0).GetChild(1).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[0].amount.ToString();
        }
        boxParent.GetChild(1).GetChild(0).GetComponent<MenuCardHandler>().DrawCard(rewardList[1].item);
        if (rewardList[1].amount > 0) {
            boxParent.GetChild(1).GetChild(1).gameObject.SetActive(true);
            boxParent.GetChild(1).GetChild(1).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[1].amount.ToString();
        }

        boxParent.GetChild(2).Find(rewardList[2].item).gameObject.SetActive(true);
        boxParent.GetChild(2).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[2].amount.ToString();

        if (rewardList[3].type == "card"){
            boxParent.GetChild(3).Find("Card").GetChild(0).GetComponent<MenuCardHandler>().DrawCard(rewardList[3].item);
            if (rewardList[3].amount > 0) {
                boxParent.GetChild(3).Find("Card").GetChild(1).gameObject.SetActive(true);
                boxParent.GetChild(3).Find("Card").GetChild(1).Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[3].amount.ToString();
            }
        }
        else {
            boxParent.GetChild(3).Find("Resource").Find(rewardList[3].item).gameObject.SetActive(true);
            boxParent.GetChild(3).Find("Resource").Find("Value").GetComponent<TMPro.TextMeshProUGUI>().text = rewardList[3].amount.ToString();
        }
    }
}

public class RewardClass {
    public string item;
    public int amount;
    public string type;
}
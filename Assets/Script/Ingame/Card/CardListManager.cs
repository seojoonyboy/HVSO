using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class CardListManager : MonoBehaviour
{
    [SerializeField] GameObject cardPrefab;
    [SerializeField] Transform contentParent;
    [SerializeField] GameObject infoPrefab;
    [SerializeField] Transform mulliganList;
    Animator animator;
    HorizontalScrollSnap hss;
    // Start is called before the first frame update
    void Start()
    {
        transform.GetComponent<Image>().enabled = false;
        animator = transform.GetComponentInChildren<Animator>();
        animator.SetBool("Hide", true);
        hss = transform.GetComponentInChildren<HorizontalScrollSnap>();
    }

    public void AddCardInfo(CardData data, string id) {
        GameObject newcard =  Instantiate(cardPrefab);
        SetCardInfo(newcard, data);
        GameObject newcardInfo = Instantiate(infoPrefab, contentParent);
        SetCardClassInfo(newcardInfo, data);
        hss.AddChild(newcard);
        if (data.type == "unit") {
            GameObject unitImage = Instantiate(AccountManager.Instance.resource.cardSkeleton[id], newcard.transform.Find("Info/UnitImage"));
            Destroy(unitImage.GetComponent<UnitSpine>());
            unitImage.transform.localScale = new Vector3(500, 500, 0);
        }
    }

    public void AddMulliganCardInfo(CardData data, string id) {
        GameObject newcard = Instantiate(cardPrefab, mulliganList);
        SetCardInfo(newcard, data);
        GameObject newcardInfo = Instantiate(infoPrefab, contentParent);
        SetCardClassInfo(newcardInfo, data);
        if (data.type == "unit") {
            GameObject unitImage = Instantiate(AccountManager.Instance.resource.cardSkeleton[id], newcard.transform.Find("Info/UnitImage"));
            Destroy(unitImage.GetComponent<UnitSpine>());
            unitImage.transform.localScale = new Vector3(500, 500, 0);
        }
        newcard.SetActive(false);
    }

    public void OpenMulliganCardList(int cardnum) {
        mulliganList.GetChild(cardnum).gameObject.SetActive(true);
    }

    public void RemoveCardInfo(int index) {
        GameObject remove;
        hss.RemoveChild(index, out remove);
        Destroy(remove);
        Destroy(contentParent.GetChild(index).gameObject);
    }

    public void OpenCardList(int cardnum) {
        transform.GetComponent<Image>().enabled = true;
        animator.SetBool("Hide", false);
        hss.GoToScreen(cardnum);
    }

    public void SetCardInfo(GameObject obj, CardData data) {
        Transform info = obj.transform.GetChild(0);
        info.Find("Name/NameText").GetComponent<Text>().text = data.name;
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

        if (data.class_2 == null)
            obj.transform.Find("Class2Button").gameObject.SetActive(false);
    }

    public void SetCardClassInfo(GameObject obj, CardData data) {
        obj.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = data.class_1.ToString();
        if (data.class_2 != null)
            obj.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = data.class_1.ToString();
        else
            obj.transform.GetChild(1).gameObject.SetActive(false);
    }
}

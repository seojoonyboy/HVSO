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
    [SerializeField] GameObject firstInfoPrefab;
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
        hss.AddChild(newcard);
        if (data.type == "unit") {
            GameObject unitImage = Instantiate(AccountManager.Instance.resource.cardSkeleton[id], newcard.transform.Find("Info/UnitImage"));
            Destroy(unitImage.GetComponent<UnitSpine>());
            unitImage.transform.localScale = new Vector3(500, 500, 0);
        }
    }

    public void AddMulliganCardInfo(CardData data, string id) {
        GameObject newcard = Instantiate(firstInfoPrefab, mulliganList);
        SetCardInfo(newcard, data);
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

        obj.transform.Find("Class1Button").name = data.class_1;
        if (data.class_2 == null)
            obj.transform.Find("Class2Button").gameObject.SetActive(false);
        else
            obj.transform.Find("Class2Button").name = data.class_2;
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
                    GameObject selectedTarget = hit.collider.gameObject.GetComponentInParent<PlaceMonster>().gameObject;
                    string objName = hit.collider.gameObject.GetComponentInParent<PlaceMonster>().myUnitNum.ToString() + "unit";
                    transform.GetChild(1).Find(objName).gameObject.SetActive(true);
                }
            }
        }
    }

    public void SetFeildUnitInfo(CardData data, int unitNum) {
        GameObject info = Instantiate(firstInfoPrefab, transform.Find("FieldUnitInfo"));
        info.name = unitNum.ToString() + "unit";
        SetCardInfo(info, data);
        info.SetActive(false);
    }
}

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

    public void AddCardInfo(CardData data) {
        GameObject newcard =  Instantiate(cardPrefab);
        SetCardInfo(newcard, data);
        GameObject newcardInfo = Instantiate(infoPrefab, contentParent);
        SetCardClassInfo(newcardInfo, data);
        hss.AddChild(newcard);
    }

    public void OpenCardList(int cardnum) {
        transform.GetComponent<Image>().enabled = true;
        animator.SetBool("Hide", false);
        hss.GoToScreen(cardnum);
    }

    public void SetCardInfo(GameObject obj, CardData data) {
        obj.transform.GetChild(0).GetComponent<Text>().text = data.name;
        if (data.hp != null)
            obj.transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = data.hp.ToString();
        else
            obj.transform.GetChild(1).GetChild(1).gameObject.SetActive(false);

        if (data.attack != null)
            obj.transform.GetChild(1).GetChild(2).GetChild(0).GetComponent<Text>().text = data.attack.ToString();
        else
            obj.transform.GetChild(1).GetChild(2).gameObject.SetActive(false);

        obj.transform.GetChild(1).GetChild(3).GetChild(0).GetComponent<Text>().text = data.cost.ToString();

        if (data.class_2 != null)
            obj.transform.GetChild(3).gameObject.SetActive(false);
    }

    public void SetCardClassInfo(GameObject obj, CardData data) {
        obj.transform.GetChild(0).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = data.class_1.ToString();
        if (data.class_2 != null)
            obj.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>().text = data.class_1.ToString();
        else
            obj.transform.GetChild(1).gameObject.SetActive(false);
    }
}

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
        //hss.transform.GetComponent<RectTransform>().position = new Vector3(0, Screen.height * 0.05f, 0);
        //hss.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width * 0.8f, Screen.height * 0.375f);
    }

    public void AddCardInfo() {
        GameObject newcard =  Instantiate(cardPrefab);
        GameObject newcardInfo = Instantiate(infoPrefab, contentParent);
        hss.AddChild(newcard);
        //newcard.GetComponent<RectTransform>().localScale = new Vector3(Screen.width / 1080.0f, Screen.height / 1920.0f, 1);
    }

    public void OpenCardList(int cardnum) {
        transform.GetComponent<Image>().enabled = true;
        animator.SetBool("Hide", false);
        hss.GoToScreen(cardnum);
    }
}

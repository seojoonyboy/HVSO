using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public class CardListManager : MonoBehaviour
{
    [SerializeField] GameObject cardPrefab;
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


    public void OpenCardList(int cardnum) {
        transform.GetComponent<Image>().enabled = true;
        animator.SetBool("Hide", false);
        hss.GoToScreen(transform.GetSiblingIndex());
    }
}

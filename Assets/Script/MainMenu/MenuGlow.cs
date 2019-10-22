using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuGlow : MonoBehaviour
{
    public static MenuGlow Instance;
    public GameObject glowParent;

    private void Awake() {
        Instance = this;
    }

    private void OnDestroy() {
        Instance = null;
    }

    private GameObject GetUnglowObject() {
        foreach(Transform child in gameObject.transform) {
            if (child.gameObject.activeSelf == false)
                return child.gameObject;
        }
        return null;
    }

    public void StartGlow(GameObject targetObject) {
        if (targetObject.GetComponent<RectTransform>() == null) return;
        RectTransform targetRect = targetObject.GetComponent<RectTransform>();
        GameObject glowObject = GetUnglowObject();
        RectTransform glowRect = glowObject.GetComponent<RectTransform>();
        Image glowImage = glowObject.GetComponent<Image>();

        Animation glowAnimation = glowObject.GetComponent<Animation>();
        glowRect.position = targetRect.position;
        glowRect.sizeDelta = targetRect.sizeDelta;
        glowImage.sprite = targetObject.GetComponent<Image>().sprite;

        glowObject.SetActive(true);
        glowAnimation.Play();      

    }

    public void StopEveryGlow() {
        foreach (Transform child in gameObject.transform) {
            if (child.gameObject.activeSelf == false)
                continue;

            Animation glowAnimation = child.gameObject.GetComponent<Animation>();
            glowAnimation.Stop();
            glowAnimation.clip = glowAnimation.GetClip("glowAnimation");
            child.gameObject.GetComponent<Image>().color = Color.white;
            child.localScale = Vector3.one;
            child.position = Vector3.one * 100f;
            child.gameObject.SetActive(false);
            child.GetChild(0).gameObject.SetActive(false);
        }
    }



}

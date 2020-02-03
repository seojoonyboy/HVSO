using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderAssetController : MonoBehaviour
{
    public bool filled;
    public bool textOn;

    public delegate void Callback();
    private Callback callback = null;

    Transform target;
    TMPro.TextMeshProUGUI text;
    float startPos;
    float width;

    // Start is called before the first frame update
    void Awake() {
        target = transform.Find("Slider/Value");
        text = transform.Find("Slider/ValueText").GetComponent<TMPro.TextMeshProUGUI>();
        width = target.GetComponent<RectTransform>().rect.width;
        if (!filled) {
            target.GetComponent<Image>().type = Image.Type.Sliced;
            startPos = (width * -0.5f) * 3;
        }
        else {
            target.GetComponent<Image>().type = Image.Type.Filled;
        }
    }

    public void SetSliderAmount(int amount, int max) {
        if (!filled) {
            target.localPosition = new Vector3(startPos + (width * ((float)amount / (float)max)), 0, 0);
        }
        else {
            target.GetComponent<Image>().fillAmount = (float)amount / (float)max;
        }

        text.gameObject.SetActive(textOn);
        if (textOn) {
            text.text = amount.ToString() + "/" + max.ToString();
        }
        if (amount == max) callback();
    }

    public void SetCallBack(Callback call) {
        callback = call;
    }
}


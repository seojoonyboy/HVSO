using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
public class Fader : MonoBehaviour {
    public Image fadeOutUIImage;
    public float fadeSpeed = 1;

    public delegate void Callback();
    private Callback callback = null;

    public enum FadeDirection {
        In, //Alpha = 1
        Out // Alpha = 0
    }

    private void Awake() {
        fadeOutUIImage = GetComponent<Image>();
    }
    //void OnEnable() {
    //    StartCoroutine(Fade(FadeDirection.Out));
    //}

    public IEnumerator Fade(FadeDirection fadeDirection, Callback callback) {
        float alpha = (fadeDirection == FadeDirection.Out) ? 1 : 0;
        float fadeEndValue = (fadeDirection == FadeDirection.Out) ? 0 : 1;
        if (fadeDirection == FadeDirection.Out) {
            while (alpha >= fadeEndValue) {
                SetColorImage(ref alpha, fadeDirection);
                yield return null;
            }
            fadeOutUIImage.enabled = false;
        }
        else {
            fadeOutUIImage.enabled = true;
            while (alpha <= fadeEndValue) {
                SetColorImage(ref alpha, fadeDirection);
                yield return null;
            }
        }

        callback.Invoke();
    }

    public void StartFade(FadeDirection direction, Callback callback) {
        StartCoroutine(Fade(direction, callback));
        //callback.Invoke();
    }

    private void SetColorImage(ref float alpha, FadeDirection fadeDirection) {
        fadeOutUIImage.color = new Color(fadeOutUIImage.color.r, fadeOutUIImage.color.g, fadeOutUIImage.color.b, alpha);
        alpha += Time.deltaTime * 10 * fadeSpeed * ((fadeDirection == FadeDirection.Out) ? -1 : 1);
    }
}
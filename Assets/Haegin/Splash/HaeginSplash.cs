using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Haegin
{
    public class HaeginSplash : MonoBehaviour
    {
        public enum Orientations
        {
            Portrait,
            Landscape
        }
        public delegate void OnSpalshClosed();

        IEnumerator ShowHaeginSplashSub(Orientations ori, OnSpalshClosed callback)
        {
            if (ori == Orientations.Portrait)
            {
                gameObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1064, 1900);
                gameObject.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.0f;
            }
            else
            {
                gameObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1776, 1000);
                gameObject.GetComponent<CanvasScaler>().matchWidthOrHeight = 1.0f;
            }

            yield return new WaitForSeconds(2.5f);

            callback();
            Destroy(gameObject);
        }

        public void Setting(Orientations ori, OnSpalshClosed callback)
        {
            StartCoroutine(ShowHaeginSplashSub(ori, callback));
        }

        public static void ShowHaeginSplash(Orientations ori, OnSpalshClosed callback)
        {
            GameObject splash = Instantiate(Resources.Load<GameObject>("Splash/HaeginSplash"));
            splash.GetComponent<HaeginSplash>().Setting(ori, callback);
        }
    }
}

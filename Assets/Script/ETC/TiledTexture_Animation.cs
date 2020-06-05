using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TiledTexture_Animation : MonoBehaviour {
    float scrollSpeed = 0.1f;
    private RawImage _rawImage;
    private float uvHeight, uvWidth;

    private void OnEnable() {
        _rawImage = GetComponent<RawImage>();
        
        var uvRect = _rawImage.uvRect;
        uvHeight = uvRect.height;
        uvWidth = uvRect.width;
        
        StartCoroutine(UpdateTexture());
    }

    private void OnDestroy() {
        StopAllCoroutines();
    }

    IEnumerator UpdateTexture() {
        while (true) {
            float offset = -1 * Time.time * scrollSpeed;
            _rawImage.uvRect = new Rect(offset, 0, uvWidth, uvHeight);
            yield return new WaitForEndOfFrame();
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Haegin
{
    public class Share
    {
        static Texture2D duplicateTexture(Texture2D source, int w, int h)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
                        w,
                        h,
                        0,
                        RenderTextureFormat.Default,
                        RenderTextureReadWrite.Linear);

            Graphics.Blit(source, renderTex);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            Texture2D readableText = new Texture2D(w, h);
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);
            return readableText;
        }

        public static void OpenShareDialog(string dialogTitle, string text, Texture2D image = null)
        {
#if !UNITY_EDITOR
            if (image != null) 
            {
                Texture2D readableImage = duplicateTexture(image, image.width, image.height);
                string filePath = System.IO.Path.Combine(Application.temporaryCachePath, "sharedimg.png");
                System.IO.File.WriteAllBytes(filePath, readableImage.EncodeToPNG());
                new NativeShare().AddFile(filePath, "image/png").SetTitle(dialogTitle).SetText(text).Share();
                GameObject.Destroy(readableImage);
            }
            else
            {
                new NativeShare().SetTitle(dialogTitle).SetText(text).Share();
            }
#endif
            /*
                        //#if !UNITY_EDITOR
#if UNITY_IOS
                        if(image != null) 
                            IOSSocialManager.Instance.ShareMedia(text, duplicateTexture(image, image.width, image.height));
                        }   
                        else
                            IOSSocialManager.Instance.ShareMedia(text);
#elif UNITY_ANDROID
                        if (image != null)
                        {
                            AndroidNativeSettings.Instance.ImageFormat = AndroidCameraImageFormat.PNG;
                            AndroidNativeSettings.Instance.SaveCameraImageToGallery = false;
                            AndroidSocialGate.StartShareIntent(dialogTitle, text, duplicateTexture(image, image.width, image.height));
                        }
                        else
                            AndroidSocialGate.StartShareIntent(dialogTitle, text);
#endif
                        //#endif
            */
        }
    }
}

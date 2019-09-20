using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.Build;
#if UNITY_2018_2_OR_NEWER
using UnityEditor.Build.Reporting;
#endif

#if UNITY_ANDROID && UNITY_2018_2_OR_NEWER
public class CopyAndroidIconFiles : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        PlatformIcon[] icons = PlayerSettings.GetPlatformIcons(BuildTargetGroup.Android, UnityEditor.Android.AndroidPlatformIconKind.Legacy);

        foreach (PlatformIcon icon in icons)
        {
            switch (icon.width)
            {
                case 192:
                    SaveToPng(icon.GetTexture(), "Assets/Plugins/Android/AN_Res/res/drawable-xxxhdpi/app_icon.png", 192, 192);
                    SaveToPng(icon.GetTexture(), "Assets/Plugins/Android/AN_Res/res/drawable/app_icon.png", 192, 192);
                    break;
                case 144:
                    SaveToPng(icon.GetTexture(), "Assets/Plugins/Android/AN_Res/res/drawable-xxhdpi/app_icon.png", 144, 144);
                    break;
                case 96:
                    SaveToPng(icon.GetTexture(), "Assets/Plugins/Android/AN_Res/res/drawable-xhdpi/app_icon.png", 96, 96);
                    break;
                case 72:
                    SaveToPng(icon.GetTexture(), "Assets/Plugins/Android/AN_Res/res/drawable-hdpi/app_icon.png", 72, 72);
                    break;
                case 48:
                    SaveToPng(icon.GetTexture(), "Assets/Plugins/Android/AN_Res/res/drawable-mdpi/app_icon.png", 48, 48);
                    break;
            }
        }
    }

    Texture2D duplicateTexture(Texture2D source, int w, int h)
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

    void SaveToPng(Texture2D tex, string filename, int w, int h)
    {
        if (tex != null)
        {
            if (File.Exists(filename))
                File.Delete(filename);
            byte[] bytes = duplicateTexture(tex, w, h).EncodeToPNG();
            File.WriteAllBytes(filename, bytes);
        }
    }
}
#endif

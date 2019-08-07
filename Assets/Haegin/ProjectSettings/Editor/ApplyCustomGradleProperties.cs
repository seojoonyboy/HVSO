#if CUSTOM_GRADLE_PROPERTIES
using UnityEngine;
using UnityEditor.Android;
using System.IO;

public class AndroidPostBuildProcessor : IPostGenerateGradleAndroidProject
{
    public int callbackOrder
    {
        get
        {
            return 999;
        }
    }

    void IPostGenerateGradleAndroidProject.OnPostGenerateGradleAndroidProject(string path)
    {
        // Gradle.properties 을 커스터마이징 하려면 여기서 출력하시면 되요.
        Debug.Log("Bulid path : " + path);
        string gradlePropertiesFile = path + "/gradle.properties";
        if (File.Exists(gradlePropertiesFile))
        {
            File.Delete(gradlePropertiesFile);
        }
        StreamWriter writer = File.CreateText(gradlePropertiesFile);
        writer.WriteLine("org.gradle.jvmargs=-Xms128m -Xmx2048m -XX:+CMSClassUnloadingEnabled");
        writer.Flush();
        writer.Close();
    }
}
#endif
using UnityEditor;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
using UnityEditor.iOS.Xcode.Extensions;
#endif
using System.IO;
using UnityEditor.Callbacks;

public class CreateIOSLocalizeStrings : MonoBehaviour 
{
    private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
    {
        // Get the subdirectories for the specified directory.
        DirectoryInfo dir = new DirectoryInfo(sourceDirName);

        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException(
                "Source directory does not exist or could not be found: "
                + sourceDirName);
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        // If the destination directory doesn't exist, create it.
        if (!Directory.Exists(destDirName))
        {
            Directory.CreateDirectory(destDirName);
        }

        // Get the files in the directory and copy them to the new location.
        FileInfo[] files = dir.GetFiles();
        foreach (FileInfo file in files)
        {
            string temppath = Path.Combine(destDirName, file.Name);
            file.CopyTo(temppath, true);
        }

        // If copying subdirectories, copy them and their contents to new location.
        if (copySubDirs)
        {
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(destDirName, subdir.Name);
                DirectoryCopy(subdir.FullName, temppath, copySubDirs);
            }
        }
    }

    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target == BuildTarget.iOS)
        {
#if UNITY_IOS
            var localizePath = pathToBuiltProject;

            var projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);
            string targetGUID = proj.TargetGuidByName(PBXProject.GetUnityTargetName());

            // 로컬라이즈 디렉토리들을 지운다.
            DirectoryInfo[] dirs = new DirectoryInfo(localizePath).GetDirectories();
            foreach (DirectoryInfo subdir in dirs)
            {
                if (subdir.FullName.EndsWith(".lproj") == true)
                {
                    string guid = proj.FindFileGuidByProjectPath(subdir.Name);
                    Debug.Log(subdir.Name + "  ===>  " + guid);
                    if (guid != null) 
                    {
                        proj.RemoveFileFromBuild(targetGUID, guid);
                        proj.RemoveFile(guid);
                    }
                    Directory.Delete(subdir.FullName, true);
                }
            }

            // 파일 복사
            DirectoryCopy("IOSLocalize", localizePath, true);


            // 프로젝트 루트에 lproj 폴더를 넣어주는 것 만으로도 앱네임 로컬라이즈가 되네. --;
            for (int i = 0; i < (int)ProjectSettingsWindow.LocalizedName.Max; i++)
            {
                string folderName = null;
                if (string.IsNullOrEmpty(ProjectSettingsWindow.IOSLocalizedPostfix[i]))
                {
                    folderName = "en.lproj";
                }
                else
                {
                    folderName = ProjectSettingsWindow.IOSLocalizedPostfix[i] + ".lproj";
                }
                if (Directory.Exists(localizePath + "/" + folderName))
                {
                    proj.AddFileToBuild(targetGUID, proj.AddFile(folderName, folderName));
                }
            }
            proj.WriteToFile(projPath);
#endif
        }
    }
}

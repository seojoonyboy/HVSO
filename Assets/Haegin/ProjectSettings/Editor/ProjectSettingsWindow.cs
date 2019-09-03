using UnityEditor;
using UnityEngine;
using System.IO;
using System.Xml;

public class ProjectSettingsWindow : EditorWindow
{
    private const string xmlpath = "HaeginProjectSettings.xml";

    public enum LocalizedName
    {
        DefaultEn,
        Ko,
        Jp,
        ZhHant,
        ZhHans,
        Es,
        De,
        It,
        Fr,
        Pt,
        Id,
        Max
    };

    public static string[] IOSLocalizedPostfix = {
        "",
        "ko",
        "ja",
        "zh-Hant",
        "zh-Hans",
        "es",
        "de",
        "it",
        "fr",
        "pt",
        "id",
    };

    public static string[] AndroidLocalizedPostfix = {
        "",
        "ko",
        "ja",
        "zh-rTW",
        "zh-rCN",
        "es",
        "de",
        "it",
        "fr",
        "pt",
        "in", // id 
    };

    public static string[] AppNameTitle = {
        "App Name(default, 영어)",
        "App Name(한글)",
        "App Name(일본어)",
        "App Name(중국어 번체)",
        "App Name(중국어 간체)",
        "App Name(스페인어)",
        "App Name(독일어)",
        "App Name(이탈리아어)",
        "App Name(프랑스어)",
        "App Name(포르투갈어)",
        "App Name(인도네시아어)",
    };

    public static string[] SettingItemName = {
        "GoogleAppName",
        "GoogleAppNameKo",
        "GoogleAppNameJp",
        "GoogleAppNameZhHant",
        "GoogleAppNameZhHans",
        "GoogleAppNameEs",
        "GoogleAppNameDe",
        "GoogleAppNameIt",
        "GoogleAppNameFr",
        "GoogleAppNamePt",
        "GoogleAppNameId",
    };

    static string ProtocolName1 = "";
    static string ProtocolName2 = "HaeginGame";
    static string accountIdToCreate = "ckeag1C97qEweL1lnZTVf3";
    static string webClientOAuth2ClientId = "551232432184-233chdikqqqj6sqsj3rihs7cq6ij4fm0.apps.googleusercontent.com";
    static string FacebookAppID = "166749030757238";
    static string[] GoogleAppName = {
        "Module Sample",  // Default 영어
        "모듈 샘플",        // 한글
        "",               // 일본어
        "",               // 중국어 번체
        "",               // 중국어 간체
        "",               // 스페인어 
        "",               // 독일어 
        "",               // 이탈리아어 
        "",               // 프랑스어 
        "",               // 포르투갈어
        "",               // 인도네시아어
    };
    static string GoogleAppID = "551232432184";
    static string BundleID = "com.haegin.modulesample";
    static string base64EncodedPublicKey = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAhyY/WIjzgbHjmJpKtvc/0G/9bm8GS6/+Xpg78KHaY43jobSfzktzvI8wgQntmpVyDS79/1EPuBjciySmvlaFuDUii4LQeb6ohOwesf3wcwhX5OjYiS+PJCher4wxXwF/1LWOg+bR+g8x+5OSREvjT0GCsDt+oEFmAUdFSC23tcqvqF2J+1HeMKDPONmElSQymiTbg+f3rBJ5d9/lpWGtZMIvZB3HlHNsaxTPQmFoT1sBQ8vmEKBS4prx4vN2uf4T/WRl4S9JQD0No0n+7AgS5nRkjfFXMjbeU/bTioWc3C52SaZeld8meTza0TQ0fCekVZSgKnU3q7hUxUV9dhRgzwIDAQAB";
    static string toolsReplaceAdd = ""; //", android:roundIcon";
    static string EditorAccountIdKey = "AccountId";
    static string URLScheme = "hgmodsample";
    static string firebaseDynamicLink = "hgmodsample.page.link";
    static string AdMobAppId = "ca-app-pub-8910195275590924~5903100277";
    static string oneStoreBase64EncodedPublicKey = "";
    static bool UseAppsFlyer = false;
    static bool SkipPermissionsDialog = false;
    static bool UseIOSGoogleMobileAds7_24_0_OR_HIGHER = false;
    static bool UseOneStoreIAP = false;
    static string ZendeskHelpUrl = "https://help-homerunclash.haegin.kr/hc";
    static string ZendeskHelpAPPageID = "360033798014";
    static string ZendeskHelpSupportMail = "support@homerunclash.zendesk.com";

    Rect notifyIcon1;


    [MenuItem("Haegin/Project Settings")]
    public static void ShowWindow() 
    {
        ProjectSettingsWindow window = EditorWindow.GetWindow<ProjectSettingsWindow>(true, "Haegin Project Settings");
    } 

    [MenuItem("Haegin/Delete PlayerPrefs")]
    public static void DeletePlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    } 

    static ProjectSettingsWindow()
    {
        LoadSettings();
    }

    static void LoadSettings()
    {
        if (File.Exists(xmlpath)) 
        {
            XmlDocument xml = new XmlDocument();
            xml.Load(xmlpath);

            string text;
            XmlNode node = xml.SelectSingleNode("HaeginSettings");
            try
            {
                text = node["BaseProtocolNameSpace"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    ProtocolName1 = text;
            }
            catch { }
            try
            {
                text = node["GameProtocolNameSpace"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    ProtocolName2 = text;
            }
            catch { }
            try
            {
                text = node["AccountIdToCreate"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    accountIdToCreate = text;
            }
            catch { }
            try
            {
                text = node["Auth2ClientID"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    webClientOAuth2ClientId = text;
            }
            catch { }
            try
            {
                text = node["FacebookAppID"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    FacebookAppID = text;
            }
            catch { }
            for (int i = 0; i < (int)LocalizedName.Max; i++)
            {
                try
                {
                    text = node[SettingItemName[i]].InnerText;
                    if (!string.IsNullOrEmpty(text))
                        GoogleAppName[i] = text;
                }
                catch { }
            }
            try
            {
                text = node["GoogleAppID"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    GoogleAppID = text;
            }
            catch { }
            try
            {
                text = node["EditorAccountIdKey"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    EditorAccountIdKey = text;
            }
            catch { }
            try
            {
                text = node["URLScheme"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    URLScheme = text;
            }
            catch { }
            try
            {
                text = node["FirebaseDynamicLink"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    firebaseDynamicLink = text;
            }
            catch { }
            try
            {
                text = node["PackageName"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    BundleID = text;
            }
            catch { }
            try
            {
                text = node["ToolsReplace"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    toolsReplaceAdd = text;
            }
            catch { }
            try
            {
                text = node["GoogleBase64EncodedPublicKey"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    base64EncodedPublicKey = text;
            }
            catch { }
            try
            {
                text = node["AdMobAppId"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    AdMobAppId = text;
            }
            catch { }
            try
            {
                text = node["UseAppsFlyer"].InnerText;
                if (!string.IsNullOrEmpty(text))
                {
                    UseAppsFlyer = text.Equals("true", System.StringComparison.OrdinalIgnoreCase);
                }
            }
            catch { }
            try
            {
                text = node["SkipPermissionsDialog"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    SkipPermissionsDialog = text.Equals("true", System.StringComparison.OrdinalIgnoreCase);
            }
            catch { }
            try
            {
                text = node["UseIOSGoogleMobileAds7_24_0_OR_HIGHER"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    UseIOSGoogleMobileAds7_24_0_OR_HIGHER = text.Equals("true", System.StringComparison.OrdinalIgnoreCase);
            }
            catch { }
            try
            {
                text = node["UseOneStoreIAP"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    UseOneStoreIAP = text.Equals("true", System.StringComparison.OrdinalIgnoreCase);
            }
            catch { }
            try
            {
                text = node["OneStoreBase64EncodedPublicKey"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    oneStoreBase64EncodedPublicKey = text;
            }
            catch { }
            try
            {
                text = node["ZendeskHelpAPPageID"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    ZendeskHelpAPPageID = text;
            }
            catch { }
            try
            {
                text = node["ZendeskHelpUrl"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    ZendeskHelpUrl = text;
            }
            catch { }
            try
            {
                text = node["ZendeskHelpSupportMail"].InnerText;
                if (!string.IsNullOrEmpty(text))
                    ZendeskHelpSupportMail = text;
            }
            catch { }
        }
        else
        {
            Debug.Log("File Not Found");
        }
    }

    void SaveSettings()
    {
        if (File.Exists(xmlpath)) 
        {
            File.Delete(xmlpath);
        }
        string[] contents = new string[24 + (int)LocalizedName.Max];

        contents[0] = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";
        contents[1] = "<HaeginSettings>";
        contents[2] = "\t<BaseProtocolNameSpace>" + ProtocolName1 + "</BaseProtocolNameSpace>";
        contents[3] = "\t<GameProtocolNameSpace>" + ProtocolName2 + "</GameProtocolNameSpace>";
        contents[4] = "\t<AccountIdToCreate>" + accountIdToCreate + "</AccountIdToCreate>";
        contents[5] = "\t<Auth2ClientID>" + webClientOAuth2ClientId + "</Auth2ClientID>";
        contents[6] = "\t<FacebookAppID>" + FacebookAppID + "</FacebookAppID>";
        for (int i = 0; i < (int)LocalizedName.Max; i++)
        {
            contents[7 + i] = "\t<" + SettingItemName[i] + ">" + GoogleAppName[i] + "</" + SettingItemName[i] + ">";
        }
        contents[7 + (int)LocalizedName.Max] = "\t<GoogleAppID>" + GoogleAppID + "</GoogleAppID>";
        contents[8 + (int)LocalizedName.Max] = "\t<EditorAccountIdKey>" + EditorAccountIdKey + "</EditorAccountIdKey>";
        contents[9 + (int)LocalizedName.Max] = "\t<PackageName>" + BundleID + "</PackageName>";
        contents[10 + (int)LocalizedName.Max] = "\t<ToolsReplace>" + toolsReplaceAdd + "</ToolsReplace>";
        contents[11 + (int)LocalizedName.Max] = "\t<GoogleBase64EncodedPublicKey>" + base64EncodedPublicKey + "</GoogleBase64EncodedPublicKey>";
        contents[12 + (int)LocalizedName.Max] = "\t<URLScheme>" + URLScheme + "</URLScheme>";
        contents[13 + (int)LocalizedName.Max] = "\t<FirebaseDynamicLink>" + firebaseDynamicLink + "</FirebaseDynamicLink>";
        contents[14 + (int)LocalizedName.Max] = "\t<AdMobAppId>" + AdMobAppId + "</AdMobAppId>";
        contents[15 + (int)LocalizedName.Max] = "\t<UseAppsFlyer>" + UseAppsFlyer + "</UseAppsFlyer>";
        contents[16 + (int)LocalizedName.Max] = "\t<SkipPermissionsDialog>" + SkipPermissionsDialog + "</SkipPermissionsDialog>";
        contents[17 + (int)LocalizedName.Max] = "\t<UseIOSGoogleMobileAds7_24_0_OR_HIGHER>" + UseIOSGoogleMobileAds7_24_0_OR_HIGHER + "</UseIOSGoogleMobileAds7_24_0_OR_HIGHER>";
        contents[18 + (int)LocalizedName.Max] = "\t<UseOneStoreIAP>" + UseOneStoreIAP + "</UseOneStoreIAP>";
        contents[19 + (int)LocalizedName.Max] = "\t<OneStoreBase64EncodedPublicKey>" + oneStoreBase64EncodedPublicKey + "</OneStoreBase64EncodedPublicKey>";
        contents[20 + (int)LocalizedName.Max] = "\t<ZendeskHelpUrl>" + ZendeskHelpUrl + "</ZendeskHelpUrl>";
        contents[21 + (int)LocalizedName.Max] = "\t<ZendeskHelpAPPageID>" + ZendeskHelpAPPageID + "</ZendeskHelpAPPageID>";
        contents[22 + (int)LocalizedName.Max] = "\t<ZendeskHelpSupportMail>" + ZendeskHelpSupportMail + "</ZendeskHelpSupportMail>";
        contents[23 + (int)LocalizedName.Max] = "</HaeginSettings>";
        File.WriteAllLines(xmlpath, contents);
    }

    void OnGUI()
    {
        GUILayout.BeginVertical("box");
        GUILayout.Label("Basic Settings");
        GUILayout.BeginVertical("box");
        GUILayout.Label("Network Protocol Settings");
        ProtocolName1 = EditorGUILayout.TextField("Base Protocol Name Space", ProtocolName1);
        ProtocolName2 = EditorGUILayout.TextField("Game Protocol Name Space", ProtocolName2);
        accountIdToCreate = EditorGUILayout.TextField("Account ID to create", accountIdToCreate);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Label("Facebook Settings");
        FacebookAppID = EditorGUILayout.TextField("Facebook Application ID", FacebookAppID);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Label("Application Icon Name (게임내 해당 언어 번역이 없는 경우 빈칸으로 두세요)");
        for (int i = 0; i < (int)LocalizedName.Max; i++)
        {
            GoogleAppName[i] = EditorGUILayout.TextField(AppNameTitle[i], GoogleAppName[i]);
        }
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Label("Android Settings");
        GoogleAppID = EditorGUILayout.TextField("Google Play Service App ID", GoogleAppID);
        webClientOAuth2ClientId = EditorGUILayout.TextField("OAuth2 Client ID", webClientOAuth2ClientId);
        BundleID = EditorGUILayout.TextField("Package Name", BundleID);
        URLScheme = EditorGUILayout.TextField("URL Scheme", URLScheme);
        AdMobAppId = EditorGUILayout.TextField("AdMob App Id", AdMobAppId);
        base64EncodedPublicKey = EditorGUILayout.TextField("Google Base64 Encoded Public Key", base64EncodedPublicKey);
        toolsReplaceAdd = EditorGUILayout.TextField("Additional tools:replace item", toolsReplaceAdd);
        SkipPermissionsDialog = EditorGUILayout.Toggle("SkipPermissionsDialog", SkipPermissionsDialog);

        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Label("iOS Settings");
        UseIOSGoogleMobileAds7_24_0_OR_HIGHER = EditorGUILayout.Toggle("Use GoogleAds7.24.0 or higher", UseIOSGoogleMobileAds7_24_0_OR_HIGHER);
        firebaseDynamicLink = EditorGUILayout.TextField("Firebase Dynamic Link", firebaseDynamicLink);

        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Label("Editor Runtime Settings");
        EditorAccountIdKey = EditorGUILayout.TextField("PlayerPrefs AccountId Key", EditorAccountIdKey);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Label("AppsFlyer Settings");
        UseAppsFlyer = EditorGUILayout.Toggle("Use AppsFlyer", UseAppsFlyer);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Label("OneStore Settings");
        UseOneStoreIAP = EditorGUILayout.Toggle("UseOneStoreIAP", UseOneStoreIAP);
        oneStoreBase64EncodedPublicKey = EditorGUILayout.TextField("OneStore Base64 Encoded Public Key", oneStoreBase64EncodedPublicKey);
        GUILayout.EndVertical();

        GUILayout.BeginVertical("box");
        GUILayout.Label("Zendesk HelpCenter Settings");
        ZendeskHelpUrl = EditorGUILayout.TextField("Zendesk URL", ZendeskHelpUrl);
        ZendeskHelpAPPageID = EditorGUILayout.TextField("아이템 획득 확률 페이지 ID", ZendeskHelpAPPageID);
        ZendeskHelpSupportMail = EditorGUILayout.TextField("Support Mail", ZendeskHelpSupportMail);
        GUILayout.EndVertical();

        if (GUILayout.Button("Apply"))
        {
            ApplyProjectSettings();
        }
        GUILayout.EndVertical();
    }

    void ApplyProjectSettings()
    {
        SaveSettings();
        CreateStringsXML();
        CreateIOSLocalizedName();
        CreateProjectSettingsCS();
        RegenerateModuleSource();
		CreateBaseManifest();
        AssetDatabase.Refresh();
        Close();
    }

    void CreateBaseManifest()
    {
        CreateBaseManifestSub(UseOneStoreIAP, BundleID);
    }

    static void CreateBaseManifestFromBuildScript(bool overrideUseOneStoreIAP, string overrideBundleID)
    {
        LoadSettings();
        BundleID = overrideBundleID;
        UseOneStoreIAP = overrideUseOneStoreIAP;
        CreateBaseManifestSub(overrideUseOneStoreIAP, overrideBundleID);
    }

    static void CreateBaseManifestSub(bool overrideUseOneStoreIAP, string overrideBundleID)
    { 
		string[] filenames = new string[] {
            "AndroidManifest.xml"
        };
		string srcDir = "Assets/Haegin/ProjectSettings/Editor/src/";
        string dstDir = "Assets/Plugins/Android/";

        for (int i = 0; i < filenames.Length; i++)
        {
            string srcpath = srcDir + filenames[i] + ".txt";
            string dstpath = dstDir + filenames[i];

            string srcstr0 = File.ReadAllText(srcpath + "0");
            string srcstr1 = File.ReadAllText(srcpath + "1");
            string srcstr2;
            if(overrideUseOneStoreIAP == false)
            {
                srcstr2 = File.ReadAllText(srcpath + "2_1");
            }
            else
            {
                srcstr2 = File.ReadAllText(srcpath + "2_2");
            }
            string srcstr3 = File.ReadAllText(srcpath + "3");
            string srcstr4 = File.ReadAllText(srcpath + "4");
            string srcstr5 = File.ReadAllText(srcpath + "5");
            string srcstr6 = File.ReadAllText(srcpath + "6");
            string srcstr7_1 = File.ReadAllText(srcpath + "7_1");
            string srcstr7_2 = File.ReadAllText(srcpath + "7_2");

            if (File.Exists(dstpath))
            {
                File.Delete(dstpath);
            }
            File.AppendAllText(dstpath, srcstr0);
            File.AppendAllText(dstpath, overrideBundleID);
            File.AppendAllText(dstpath, srcstr1);
            File.AppendAllText(dstpath, toolsReplaceAdd);
            File.AppendAllText(dstpath, srcstr2.Replace("haeginsample", URLScheme));
            File.AppendAllText(dstpath, FacebookAppID);
            File.AppendAllText(dstpath, srcstr3);
			File.AppendAllText(dstpath, FacebookAppID);
            File.AppendAllText(dstpath, srcstr4);
            File.AppendAllText(dstpath, overrideBundleID);
            File.AppendAllText(dstpath, srcstr5);
            File.AppendAllText(dstpath, AdMobAppId);
            File.AppendAllText(dstpath, srcstr6);
            if(SkipPermissionsDialog)
            {
                File.AppendAllText(dstpath, "    <meta-data android:name=\"unityplayer.SkipPermissionsDialog\" android:value=\"true\" />\n");
            }
            if (UseAppsFlyer)
            {
                File.AppendAllText(dstpath, srcstr7_1);
            }
            else
            {
                File.AppendAllText(dstpath, srcstr7_2);
            }
        }
    }

    void CreateIOSLocalizedName()
    {
        for (int i = 0; i < (int)LocalizedName.Max; i++)
        {
            string folderName = null;
            if(string.IsNullOrEmpty(IOSLocalizedPostfix[i])) 
            {
                folderName = "en.lproj";
            }
            else 
            {
                folderName = IOSLocalizedPostfix[i] + ".lproj";
            }

            if (string.IsNullOrEmpty(GoogleAppName[i]))
            {
                if (Directory.Exists("IOSLocalize/" + folderName))
                {
                    Directory.Delete("IOSLocalize/" + folderName, true);
                }
            }
            else
            {
                if (!Directory.Exists("IOSLocalize/" + folderName))
                {
                    Directory.CreateDirectory("IOSLocalize/" + folderName);
                }
                string[] contents = new string[] {
                    "\"CFBundleDisplayName\" = \"" + GoogleAppName[i] + "\";"
                };
                string path = "IOSLocalize/" + folderName + "/InfoPlist.strings";
                if (File.Exists(path)) 
                {
                    File.Delete(path);
                }
                File.WriteAllLines(path, contents);
            }
        }
    }

    void CreateStringsXML()
    {
        for (int i = 0; i < (int)LocalizedName.Max; i++)
        {
            if(string.IsNullOrEmpty(GoogleAppName[i])) 
            {
                if (Directory.Exists("Assets/Plugins/Android/AN_Res/res/values-" + AndroidLocalizedPostfix[i]))
                {
                    Directory.Delete("Assets/Plugins/Android/AN_Res/res/values-" + AndroidLocalizedPostfix[i], true);
                }
            }
            else
            {
                string xmlPath = null;
                if (string.IsNullOrEmpty(AndroidLocalizedPostfix[i]))
                {
                    xmlPath = "Assets/Plugins/Android/AN_Res/res/values/strings.xml";
                }
                else
                {
                    xmlPath = "Assets/Plugins/Android/AN_Res/res/values-" + AndroidLocalizedPostfix[i] + "/strings.xml";
                    if (!Directory.Exists("Assets/Plugins/Android/AN_Res/res/values-" + AndroidLocalizedPostfix[i]))
                    {
                        Directory.CreateDirectory("Assets/Plugins/Android/AN_Res/res/values-" + AndroidLocalizedPostfix[i]);
                    }
                }
                if (File.Exists(xmlPath))
                {
                    File.Delete(xmlPath);
                }
                string[] contents = new string[] {
                    "<resources>",
                    "\t<string name=\"app_name\">" + GoogleAppName[i] + "</string>",
                    "\t<string name=\"app_id\">" + GoogleAppID + "</string>",
                    "\t<string name=\"fb_app_id\">" + FacebookAppID + "</string>",
                    "\t<string name=\"fb_provider_auth\">com.facebook.app.FacebookContentProvider" + FacebookAppID + "</string>",
                    "</resources>"
                };
                File.WriteAllLines(xmlPath, contents);
            }
        }
    }

    static void CreateProjectSettingsCS()
    {
        string csPath = "Assets/Haegin/ProjectSettings/ProjectSettings.cs";
        if (File.Exists(csPath))
        {
            File.Delete(csPath);
        }

        string[] contents = new string[] {
            "namespace Haegin",
            "{",
            "\tpublic class ProjectSettings",
            "\t{",
            "\t\tpublic static string accountIdToCreate = \"" + accountIdToCreate + "\";",
            "\t\tpublic static string webClientOAuth2ClientId = \"" + webClientOAuth2ClientId + "\";",
            "#if UNITY_EDITOR",
            "\t\tpublic static string editorAccountIdKey = \"" + EditorAccountIdKey + "\";",
            "#else",
            "\t\tpublic static string editorAccountIdKey = \"AccountId\";",
            "#endif",
            "\t\tpublic static string base64EncodedPublicKey = \"" + base64EncodedPublicKey + "\";",
            "\t\tpublic static string urlScheme = \"" + URLScheme + "://\";",
            "\t\tpublic static string firebaseDynamicLink = \"" + firebaseDynamicLink + "\";",
            "\t\tpublic static bool useAppsFlyer = " + UseAppsFlyer.ToString().ToLower() + ";",
            "\t\tpublic static bool UseIOSGoogleMobileAds7_24_0_OR_HIGHER = " + UseIOSGoogleMobileAds7_24_0_OR_HIGHER.ToString().ToLower() + ";",
            "\t\tpublic static string oneStoreBase64EncodedPublicKey = \"" + oneStoreBase64EncodedPublicKey + "\";",
            "\t\tpublic static string ZendeskHelpAPPageID = \"" + ZendeskHelpAPPageID + "\";",
            "\t}",
            "}"
        };
        File.WriteAllLines(csPath, contents);
    }

    void RegenerateModuleSource()
    {
        string[] filenames = new string[] {
            "Account/Account.cs",
            "EULA/EULA.cs",
            "Events/PromoEvents.cs",
            "IAP/IAP.cs",
            "Network/Web/WebClient.cs",
            "NetWork/Web/Source/ProtoWebClient.cs"
        };
        string srcDir = "Assets/Haegin/ProjectSettings/Editor/src/";
        string dstDir = "Assets/Haegin/";

        for (int i = 0; i < filenames.Length; i++)
        {
            string srcpath = srcDir + filenames[i] + ".txt";
            string dstpath = dstDir + filenames[i];

            string srcstr = File.ReadAllText(srcpath);

            if (File.Exists(dstpath))
            {
                File.Delete(dstpath);
            }
            string addlines = "";
            if (!string.IsNullOrEmpty(ProtocolName1))
            {
                addlines = addlines + "using " + ProtocolName1 + ";\n";
            }
            if (!string.IsNullOrEmpty(ProtocolName2))
            {
                addlines = addlines + "using " + ProtocolName2 + ";\n";
            }
            File.WriteAllText(dstpath, addlines);
            File.AppendAllText(dstpath, srcstr);
        }

        {
            string srcpath = srcDir + "Help/Help.cs.txt";
            string dstpath = dstDir + "Help/Help.cs";

            string srcstr = File.ReadAllText(srcpath);

            if (File.Exists(dstpath))
            {
                File.Delete(dstpath);
            }
            srcstr = srcstr.Replace("https://help-homerunclash.haegin.kr/hc", ZendeskHelpUrl);
            srcstr = srcstr.Replace("support@haegin.kr", ZendeskHelpSupportMail);
            File.AppendAllText(dstpath, srcstr);
        }

        filenames = new string[] {
            "Sample/Scenes/SceneStartController.cs"
        };

        for (int i = 0; i < filenames.Length; i++)
        {
            string srcpath = srcDir + filenames[i] + ".txt";
            string dstpath = dstDir + filenames[i];

            string srcstr1 = File.ReadAllText(srcpath + "1");
            string srcstr2 = File.ReadAllText(srcpath + "2");

            if (File.Exists(dstpath))
            {
                File.Delete(dstpath);
            }
            string addlines = "";
            if (!string.IsNullOrEmpty(ProtocolName1))
            {
                addlines = addlines + "using " + ProtocolName1 + ";\n";
            }
            if (!string.IsNullOrEmpty(ProtocolName2))
            {
                addlines = addlines + "using " + ProtocolName2 + ";\n";
            }
            File.WriteAllText(dstpath, addlines);


            string resolverLine;
            if (string.IsNullOrEmpty(ProtocolName1))
            {
                resolverLine = "MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(Haegin.Resolvers.HaeginResolver.Instance, MessagePack.Unity.UnityResolver.Instance, MessagePack.Resolvers.BuiltinResolver.Instance, MessagePack.Resolvers.AttributeFormatterResolver.Instance, MessagePack.Resolvers.PrimitiveObjectResolver.Instance);\n";
            }
            else
            {
                resolverLine = "MessagePack.Resolvers.CompositeResolver.RegisterAndSetAsDefault(" + ProtocolName1 + ".Resolvers." + ProtocolName1 + "Resolver.Instance, MessagePack.Unity.UnityResolver.Instance, MessagePack.Resolvers.BuiltinResolver.Instance, MessagePack.Resolvers.AttributeFormatterResolver.Instance, MessagePack.Resolvers.PrimitiveObjectResolver.Instance);\n";
            }

            File.AppendAllText(dstpath, srcstr1);
            File.AppendAllText(dstpath, resolverLine);
            File.AppendAllText(dstpath, srcstr2);
        }

        SetOneStoreSettings(UseOneStoreIAP, false);
    }

    public static void SetOneStoreSettings(bool UseOneStoreIAP, bool refresh = true)
    {
        string dstDir = "Assets/Haegin/";
        string srcDir = "Assets/Haegin/ProjectSettings/Editor/src/";

        if (UseOneStoreIAP)
        {
            if (Directory.Exists(dstDir + "IAP/OneStore"))
            {
                Directory.Delete(dstDir + "IAP/OneStore", true);
            }
            Directory.CreateDirectory(dstDir + "IAP/OneStore");
            Directory.CreateDirectory(dstDir + "IAP/OneStore/Android");

            string[] onestore_filenames = new string[] {
                "IAP/OneStore/IAPManager.cs",
                "IAP/OneStore/Onestore_PurchaseResponse.cs"
            };
            for (int i = 0; i < onestore_filenames.Length; i++)
                File.Copy(srcDir + onestore_filenames[i] + ".txt", dstDir + onestore_filenames[i]);

            File.Copy(srcDir + "IAP/OneStore/iap_plugin_v17.02.00_20181012.jarsrc", dstDir + "IAP/OneStore/Android/iap_plugin_v17.02.00_20181012.jar");
            File.Copy(srcDir + "IAP/OneStore/haeginonestore.aarsrc", dstDir + "IAP/OneStore/Android/haeginonestore.aar");

            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            if (!defines.Contains("USE_ONESTORE_IAP"))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, "USE_ONESTORE_IAP;" + defines);
            }
        }
        else
        {
            if (Directory.Exists(dstDir + "IAP/OneStore"))
            {
                Directory.Delete(dstDir + "IAP/OneStore", true);
            }

            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            if (defines.Contains("USE_ONESTORE_IAP"))
            {
                defines = defines.Replace("USE_ONESTORE_IAP;", "").Replace("USE_ONESTORE_IAP", "");
                PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);
            }
        }        
        CreateBaseManifestFromBuildScript(UseOneStoreIAP, PlayerSettings.applicationIdentifier);
        if (refresh)
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
    }
}

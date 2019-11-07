using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using SA.Android.Contacts;
#elif UNITY_IOS
using SA.iOS.Contacts;
#endif
using System.Runtime.InteropServices;

namespace Haegin
{
    public class ContactsRecord
    {
        public string Name;
        public string Phone;
    }

    public class Contacts
    {
        public enum ContactsResult
        {
            PermissionDenied,
            Error,
            Success
        };

        public static Dictionary<string, string> prefixTable = new Dictionary<string, string>()
        {
            { "AC", "+247" },
            { "AD", "+376" },
            { "AE", "+971" },
            { "AF", "+93"  },
            { "AG", "+1268" },
            { "AI", "+1264" },
            { "AL", "+355" },
            { "AM", "+374" },
            { "AO", "+244" },
            { "AR", "+54" },
            { "AS", "+1684" },
            { "AT", "+43" },
            { "AU", "+61" },
            { "AW", "+297" },
            { "AZ", "+994" },
            { "BA", "+387" },
            { "BB", "+1246" },
            { "BD", "+880" },
            { "BE", "+32" },
            { "BF", "+226" },
            { "BG", "+359" },
            { "BH", "+973" },
            { "BI", "+257" },
            { "BJ", "+229" },
            { "BM", "+1441" },
            { "BN", "+673" },
            { "BO", "+591" },
            { "BQ", "+599" },
            { "BR", "+55" },
            { "BS", "+1242" },
            { "BT", "+975" },
            { "BW", "+267" },
            { "BY", "+375" },
            { "BZ", "+501" },
            { "CA", "+1" },
            { "CD", "+243" },
            { "CF", "+236" },
            { "CG", "+242" },
            { "CH", "+41" },
            { "CI", "+225" },
            { "CK", "+682" },
            { "CL", "+56" },
            { "CM", "+237" },
            { "CN", "+86" },
            { "CO", "+57" },
            { "CR", "+506" },
            { "CU", "+53" },
            { "CV", "+238" },
            { "CW", "+599" },
            { "CY", "+357" },
            { "CZ", "+420" },
            { "DE", "+49" },
            { "DJ", "+253" },
            { "DK", "+45" },
            { "DM", "+1767" },
            { "DO", "+1809" },
            { "DZ", "+213" },
            { "EC", "+593" },
            { "EE", "+372" },
            { "EG", "+20" },
            { "ER", "+291" },
            { "ES", "+34" },
            { "ET", "+251" },
            { "FI", "+358" },
            { "FJ", "+679" },
            { "FM", "+691" },
            { "FO", "+298" },
            { "FR", "+33" },
            { "GA", "+241" },
            { "GB", "+44" },
            { "GD", "+1473" },
            { "GE", "+995" },
            { "GF", "+594" },
            { "GH", "+233" },
            { "GI", "+350" },
            { "GL", "+299" },
            { "GM", "+220" },
            { "GN", "+224" },
            { "GP", "+590" },
            { "GQ", "+240" },
            { "GR", "+30" },
            { "GT", "+502" },
            { "GU", "+1671" },
            { "GW", "+245" },
            { "GY", "+592" },
            { "HK", "+852" },
            { "HN", "+504" },
            { "HR", "+385" },
            { "HT", "+509" },
            { "HU", "+36" },
            { "ID", "+62" },
            { "IE", "+353" },
            { "IL", "+972" },
            { "IN", "+91" },
            { "IQ", "+964" },
            { "IR", "+98" },
            { "IS", "+354" },
            { "IT", "+39" },
            { "JM", "+1876" },
            { "JO", "+962" },
            { "JP", "+81" },
            { "KE", "+254" },
            { "KG", "+996" },
            { "KH", "+855" },
            { "KI", "+686" },
            { "KM", "+269" },
            { "KN", "+1869" },
            { "KR", "+82" },
            { "KW", "+965" },
            { "KY", "+1345" },
            { "KZ", "+7" },
            { "LA", "+856" },
            { "LB", "+961" },
            { "LC", "+1758" },
            { "LI", "+423" },
            { "LK", "+94" },
            { "LR", "+231" },
            { "LS", "+266" },
            { "LT", "+370" },
            { "LU", "+352" },
            { "LV", "+371" },
            { "LY", "+218" },
            { "MA", "+212" },
            { "MC", "+377" },
            { "MD", "+373" },
            { "ME", "+382" },
            { "MG", "+261" },
            { "MH", "+692" },
            { "MK", "+389" },
            { "ML", "+223" },
            { "MM", "+95" },
            { "MN", "+976" },
            { "MO", "+853" },
            { "MP", "+1670" },
            { "MQ", "+596" },
            { "MR", "+222" },
            { "MS", "+1664" },
            { "MT", "+356" },
            { "MU", "+230" },
            { "MV", "+960" },
            { "MW", "+265" },
            { "MX", "+52" },
            { "MY", "+60" },
            { "MZ", "+258" },
            { "NA", "+264" },
            { "NC", "+687" },
            { "NE", "+227" },
            { "NG", "+234" },
            { "NI", "+505" },
            { "NL", "+31" },
            { "NO", "+47" },
            { "NP", "+977" },
            { "NR", "+674" },
            { "NZ", "+64" },
            { "OM", "+968" },
            { "PA", "+507" },
            { "PE", "+51" },
            { "PF", "+689" },
            { "PG", "+675" },
            { "PH", "+63" },
            { "PK", "+92" },
            { "PL", "+48" },
            { "PM", "+508" },
            { "PR", "+1787" },
            { "PS", "+970" },
            { "PT", "+351" },
            { "PW", "+680" },
            { "PY", "+595" },
            { "QA", "+974" },
            { "RE", "+262" },
            { "RO", "+40" },
            { "RS", "+381" },
            { "RU", "+7" },
            { "RW", "+250" },
            { "SA", "+966" },
            { "SB", "+677" },
            { "SC", "+248" },
            { "SD", "+249" },
            { "SE", "+46" },
            { "SG", "+65" },
            { "SI", "+386" },
            { "SK", "+421" },
            { "SL", "+232" },
            { "SM", "+378" },
            { "SN", "+221" },
            { "SO", "+252" },
            { "SR", "+597" },
            { "SS", "+211" },
            { "ST", "+239" },
            { "SV", "+503" },
            { "SX", "+1721" },
            { "SY", "+963" },
            { "SZ", "+268" },
            { "TC", "+1649" },
            { "TD", "+235" },
            { "TG", "+228" },
            { "TH", "+66" },
            { "TJ", "+992" },
            { "TL", "+670" },
            { "TM", "+993" },
            { "TN", "+216" },
            { "TO", "+676" },
            { "TR", "+90" },
            { "TT", "+1868" },
            { "TW", "+886" },
            { "TZ", "+255" },
            { "UA", "+380" },
            { "UG", "+256" },
            { "US", "+1" },
            { "UY", "+598" },
            { "UZ", "+998" },
            { "VC", "+1784" },
            { "VE", "+58" },
            { "VG", "+1284" },
            { "VI", "+1340" },
            { "VN", "+84" },
            { "VU", "+678" },
            { "WS", "+685" },
            { "XK", "+383" },
            { "YE", "+967" },
            { "YT", "+269" },
            { "ZA", "+27" },
            { "ZM", "+260" },
            { "ZW", "+263" },
        };

        public static string GetPrefixFromCountryCode(string countryCode)
        {
            if (prefixTable.ContainsKey(countryCode))
            {
                return prefixTable[countryCode];
            }
            return "";
        }

        private static string defaultPrefix = null;
        public static string GetDefaultPrefix()
        {
            if (string.IsNullOrEmpty(defaultPrefix))
            {
                defaultPrefix = PlayerPrefs.GetString("DefaultPhonePrefix", "");
                if(string.IsNullOrEmpty(defaultPrefix))
                {
                    // get countryCode and convert to prefix
                    string cc = getCarrierCountryCode();
                    if(string.IsNullOrEmpty(cc))
                    {
                        // 이통사가 없는 경우에는 어떻게 하지?
                        // --;
                    }
                    defaultPrefix = GetPrefixFromCountryCode(cc);
                }
            }
            return defaultPrefix;
        }

        public static void SetDefaultPrefix(string prefix)
        {
            defaultPrefix = prefix;
            if (string.IsNullOrEmpty(defaultPrefix))
                defaultPrefix = "";
            PlayerPrefs.SetString("DefaultPhonePrefix", defaultPrefix);
            PlayerPrefs.Save();
        }
#if UNITY_EDITOR
        private static string getCarrierCountryCode()
        {
            return "KR";
        }
#elif UNITY_IOS
        #region NativeMethods
        [DllImport("__Internal")]
        private static extern string getCarrierCountryCode();
        #endregion
#elif UNITY_ANDROID
        private static string getCarrierCountryCode()
        {
            string ret;
            AndroidJNI.AttachCurrentThread();
            using (var pluginClass = new AndroidJavaClass("com.haegin.haeginmodule.download.BGDownloadPlugin"))
            {
                ret = pluginClass.CallStatic<string>("getCarrierCountryCode");
            }
            return ret;
        }
#endif

        static string NormalizePhoneNum(string phonenum)
        {
            string cc = phonenum.Substring(0, 2);
            if (cc != null) cc = cc.ToUpper();
            string prefix = GetPrefixFromCountryCode(cc);
            if (!string.IsNullOrEmpty(prefix))
            {
                // for iOS : remove countryCode 
                phonenum = phonenum.Substring(2);
            }
            else
            {
                prefix = GetDefaultPrefix();
            }
            phonenum = phonenum.Replace("(", "").Replace(")", "").Replace("-", "").Replace(" ", "");
            if(!string.IsNullOrEmpty(prefix))
            {
                if (phonenum.StartsWith("0", System.StringComparison.Ordinal))
                {
                    phonenum = phonenum.Substring(1);
                }
                if (phonenum.IndexOf('+') < 0)
                {
                    return prefix + phonenum;
                }
                else
                {
                    return phonenum;
                }
            }
            else
            {
                return phonenum;
            }
        }

        public delegate void OnContactsInfo(ContactsResult result, List<ContactsRecord> records);
        public static void GetContactsInfo(OnContactsInfo onContactsInfo)
        {
#if UNITY_IOS
            var status = ISN_CNContactStore.GetAuthorizationStatus(ISN_CNEntityType.Contacts);
            if(status == ISN_CNAuthorizationStatus.Authorized) {
#if MDEBUG
                Debug.Log("Contacts Permission granted");
#endif
                GetContactsInfoSub(onContactsInfo);
            }
            else {
                ISN_CNContactStore.RequestAccess(ISN_CNEntityType.Contacts, (result) => {
                    if (result.IsSucceeded) {
#if MDEBUG
                        Debug.Log("Contacts Permission granted");
#endif
                    }
                    else {
#if MDEBUG
                        Debug.Log("Contacts Permission denied");
#endif
                    }
                });
            }
#elif !UNITY_EDITOR && UNITY_ANDROID
            string permission = "android.permission.READ_CONTACTS";

            if (AndroidPermissionsManager.IsPermissionGranted(permission))
            {
#if MDEBUG
                Debug.Log("Contacts Permission granted");
#endif
                GetContactsInfoSub(onContactsInfo);
            }
            else
            {
                AndroidPermissionsManager.RequestPermission(permission, new AndroidPermissionCallback(
                    grantedPermission =>
                    {
#if MDEBUG
                        Debug.Log("Contacts Permission granted");
#endif
                        GetContactsInfoSub(onContactsInfo);
                    },
                    deniedPermission =>
                    {
#if MDEBUG
                        Debug.Log("Contacts Permission denied");
#endif
                        onContactsInfo(ContactsResult.PermissionDenied, null);
                    },
                    deniedPermissionAndDontAskAgain =>
                    {
#if MDEBUG
                        Debug.Log("Contacts Permission denied");
#endif
                        onContactsInfo(ContactsResult.PermissionDenied, null);
                    }
                ));
            }
#else
            onContactsInfo(ContactsResult.PermissionDenied, null);
#endif
        }

        public static void GetContactsInfoSub(OnContactsInfo onContactsInfo)
        {
#if UNITY_IOS
            ISN_CNContactStore.FetchPhoneContacts((result) => {
                if (result.IsSucceeded)
                {
                    List<ContactsRecord> records = new List<ContactsRecord>();
                    for (int i = 0; i < result.Contacts.Count; i++)
                    {
                        for(int j = 0; j < result.Contacts[i].Phones.Count; j++)
                        {
#if MDEBUG
                            Debug.Log("contact.Name: " + result.Contacts[i].GivenName + " " + result.Contacts[i].FamilyName);
                            Debug.Log("contact.Organization: " + result.Contacts[i].OrganizationName);
                            Debug.Log("contact.CountryCode: " + result.Contacts[i].Phones[j].CountryCode);
                            Debug.Log("contact.Phone: " + result.Contacts[i].Phones[j].FullNumber);
#endif
                            ContactsRecord rec = new ContactsRecord();
                            rec.Name = result.Contacts[i].GivenName + " " + result.Contacts[i].FamilyName;
                            rec.Phone = NormalizePhoneNum(result.Contacts[i].Phones[j].FullNumber);
                            records.Add(rec);
                        }
                    }
                    onContactsInfo(ContactsResult.Success, records);
                }
                else
                {
#if MDEBUG
                    Debug.Log("Error: " + result.Error.Message);
#endif
                    onContactsInfo(ContactsResult.Error, null);
                }
            });
#elif UNITY_ANDROID
            AN_ContactsContract.Retrieve((result) => {
                if(result.IsFailed) {
#if MDEBUG
                    Debug.Log("Filed:  " + result.Error.Message);
#endif
                    onContactsInfo(ContactsResult.Error, null);
                    return;
                }
#if MDEBUG
                Debug.Log("Loaded: " + result.Contacts.Count + " Contacts.");
#endif
                List<ContactsRecord> records = new List<ContactsRecord>();
                for(int i = 0; i < result.Contacts.Count; i++) {
#if MDEBUG
                    Debug.Log("contact.Id: " + result.Contacts[i].Id);
                    Debug.Log("contact.Name: " + result.Contacts[i].Name);
                    Debug.Log("contact.Note: " + result.Contacts[i].Note);
                    Debug.Log("contact.Organization: " + result.Contacts[i].Organization);
                    Debug.Log("contact.Phone: " + result.Contacts[i].Phone);
                    Debug.Log("contact.Photo: " + result.Contacts[i].Photo);
#endif
                    ContactsRecord rec = new ContactsRecord();
                    rec.Name = result.Contacts[i].Name;
                    rec.Phone = NormalizePhoneNum(result.Contacts[i].Phone);
                    records.Add(rec);
                }
                onContactsInfo(ContactsResult.Success, records);
            });                                
#endif
        }

    }
}

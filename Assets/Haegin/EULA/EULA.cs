using HaeginGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Haegin
{
    public class EULA : MonoBehaviour
    {
        public delegate void OnFinishedDelegate(bool isSuccess);
        public delegate void OnConfirmEULA(bool t1, bool t2, bool t3);
        public delegate void OnOpenEULADialog(string[] titles, string[] contents, bool[] isChecked, OnConfirmEULA onConfirm);

        public static void CheckEULA(OnOpenEULADialog onOpenDialog, OnFinishedDelegate onFinished)
        {
            WebClient webClient = WebClient.GetInstance();
            webClient.RequestTermsList((WebClient.ErrorCode error, List<Terms> list) =>
            {
                if (error == WebClient.ErrorCode.SUCCESS)
                {
#if MDEBUG
                    Debug.Log("List Count " + list.Count);
#endif
                    if (list == null || list.Count <= 0 || IsNotRequiredEULA(list))
                    {
                        onFinished(true);
                    }
                    else
                    {
                        string[] title = new string[list.Count];
                        string[] contents = new string[list.Count];
                        bool[] isChecked = new bool[list.Count];
                        int[] versions = new int[list.Count];
                        foreach (Terms term in list)
                        {
                            title[term.Kind - 1] = term.Title;
                            contents[term.Kind - 1] = term.Content;
                            isChecked[term.Kind - 1] = term.IsConfirmed;
                            versions[term.Kind - 1] = term.Version;
                        }

                        onOpenDialog(title, contents, isChecked, (bool t1, bool t2, bool t3) =>
                        {
                            List<TermsKindVersion> confirms = new List<TermsKindVersion>();
                            if (t1 == true)
                            {
                                TermsKindVersion confirm = new TermsKindVersion();
                                confirm.Kind = 1;
                                confirm.Version = versions[0];
                                confirms.Add(confirm);
                            }
                            if (t2 == true)
                            {
                                TermsKindVersion confirm = new TermsKindVersion();
                                confirm.Kind = 2;
                                confirm.Version = versions[1];
                                confirms.Add(confirm);
                            }
                            if (t3 == true)
                            {
                                TermsKindVersion confirm = new TermsKindVersion();
                                confirm.Kind = 3;
                                confirm.Version = versions[2];
                                confirms.Add(confirm);
                            }
                            webClient.RequestTermsConfirm(confirms, (WebClient.ErrorCode error2, bool result) => {
                                onFinished(result);
                            });
                        });
                    }
                }
                else
                {
                    onFinished(false);
                }
            });
        }

        public static bool IsNotRequiredEULA(List<Terms> list)
        {
            foreach (Terms term in list)
            {
                if (term.IsRequired && !term.IsConfirmed)
                {
                    return false;
                }
            }
            return true;
        }
    }
}

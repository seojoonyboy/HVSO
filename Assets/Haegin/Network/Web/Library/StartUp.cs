using System.Runtime.Serialization;
using System.Net.NetworkInformation;
using System.Collections;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Resolvers;
using UnityEngine;
using HaeginGame;

public class StartUp
{
    private static bool isRegistered = false;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        if (!isRegistered)
        {
#if MDEBUG
            Debug.Log("MessagePack2 Register Begin");
#endif
			StaticCompositeResolver.Instance.Register(
                Haegin.Resolvers.HaeginResolver.Instance,
				global::MessagePack.Unity.UnityResolver.Instance,
				global::MessagePack.Unity.Extension.UnityBlitResolver.Instance,
				global::MessagePack.Resolvers.StandardResolver.Instance
			);

			var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
			MessagePackSerializer.DefaultOptions = option;

            isRegistered = true;
#if MDEBUG
            Debug.Log("MessagePack2 Register End");
#endif
        }
    }

#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoadMethod]
    static void EditorInitialize()
    {
        Initialize();
    }
#endif
}

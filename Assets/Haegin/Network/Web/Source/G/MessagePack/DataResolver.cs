using MessagePack;
using MessagePack.Resolvers;

namespace G.MessagePack
{
	public class DataResolver
	{
		// static DataResolver()
		// {
		// 	Initialize();
		// }

		// public static void Initialize()
		// {
		// 	StaticCompositeResolver.Instance.Register(
		// 		global::MessagePack.Unity.UnityResolver.Instance,
		// 		global::MessagePack.Unity.Extension.UnityBlitWithPrimitiveArrayResolver.Instance,
		// 		global::MessagePack.Resolvers.StandardResolver.Instance
		// 	);

		// 	var option = MessagePackSerializerOptions.Standard.WithResolver(StaticCompositeResolver.Instance);
		// 	MessagePackSerializer.DefaultOptions = option;
		// }

		public static void Register(IFormatterResolver resolver)
		{
			// StaticCompositeResolver.Instance.Register(resolver);
			// Initialize();
		}
	}
}
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace G.Util
{
	public class AssemblyEx
	{
		public Assembly Assembly { get; private set; }

		public AssemblyEx(Type type)
		{
			Assembly = type.GetTypeInfo().Assembly;
		}

		public AssemblyEx(string typeName = null)
		{
			if (typeName == null)
			{
				Assembly = Assembly.GetEntryAssembly();
			}
			else
			{
				var type = Type.GetType(typeName);
				if (type != null)
					Assembly = type.GetTypeInfo().Assembly;
			}
		}

		public string Name
		{
			get { return Assembly.GetName().Name; }
		}

		public string FullName
		{
			get { return Assembly.FullName; }
		}

		public Stream GetStream(string resourceName)
		{
			return Assembly.GetManifestResourceStream(Name + "." + resourceName);
		}

		public string GetText(string resourceName)
		{
			string text = null;
			using (Stream stream = GetStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				text = reader.ReadToEnd();
			}
			return text;
		}

		public string[] GetLines(string resourceName)
		{
			List<string> lines = new List<string>();

			using (Stream stream = GetStream(resourceName))
			using (StreamReader reader = new StreamReader(stream))
			{
				while (true)
				{
					string line = reader.ReadLine();
					if (line == null) break;
					lines.Add(line);
				}
			}

			return lines.ToArray();
		}

		public byte[] GetData(string resourceName)
		{
			using (Stream source = GetStream(resourceName))
			using (MemoryStream target = new MemoryStream())
			{
				source.CopyTo(target);
				return target.ToArray();
			}
		}

		//public static CompilerResults Compile(IEnumerable<string> dlls, IEnumerable<string> codes)
		//{
		//	CompilerParameters parameters = new CompilerParameters
		//	{
		//		GenerateInMemory = true,
		//		IncludeDebugInformation = false
		//	};

		//	foreach (var d in dlls)
		//	{
		//		parameters.ReferencedAssemblies.Add(d);
		//	}

		//	var codeList = new List<string>();
		//	foreach (var c in codes)
		//	{
		//		codeList.Add(c);
		//	}

		//	var providerOptions = new Dictionary<string, string>();
		//	providerOptions.Add("CompilerVersion", "v4.7.2");

		//	var provider = new CSharpCodeProvider();
		//	var result = provider.CompileAssemblyFromFile(parameters, codeList.ToArray());

		//	return result;
		//}
	}
}

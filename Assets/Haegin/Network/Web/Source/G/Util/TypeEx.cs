using System;
using System.Collections.Generic;

namespace G.Util
{
	public class TypeEx
	{
		private static Dictionary<Type, string> typeMap;
		private static Dictionary<string, Type> typeMapReverse;
		private static HashSet<Type> unsignedSet;
		private static HashSet<Type> numericSet;

		static TypeEx()
		{
			typeMap = new Dictionary<Type, string>();
			typeMap[typeof(byte)] = "byte";
			typeMap[typeof(sbyte)] = "sbyte";
			typeMap[typeof(short)] = "short";
			typeMap[typeof(ushort)] = "ushort";
			typeMap[typeof(int)] = "int";
			typeMap[typeof(uint)] = "uint";
			typeMap[typeof(long)] = "long";
			typeMap[typeof(ulong)] = "ulong";
			typeMap[typeof(float)] = "float";
			typeMap[typeof(double)] = "double";
			typeMap[typeof(decimal)] = "decimal";
			typeMap[typeof(bool)] = "bool";
			typeMap[typeof(string)] = "string";
			typeMap[typeof(char)] = "char";
			typeMap[typeof(Guid)] = "Guid";
			typeMap[typeof(DateTime)] = "DateTime";
			typeMap[typeof(DateTimeOffset)] = "DateTimeOffset";
			typeMap[typeof(TimeSpan)] = "TimeSpan";
			typeMap[typeof(IntPtr)] = "IntPtr";
			typeMap[typeof(byte[])] = "byte[]";
			typeMap[typeof(byte?)] = "byte?";
			typeMap[typeof(sbyte?)] = "sbyte?";
			typeMap[typeof(short?)] = "short?";
			typeMap[typeof(ushort?)] = "ushort?";
			typeMap[typeof(int?)] = "int?";
			typeMap[typeof(uint?)] = "uint?";
			typeMap[typeof(long?)] = "long?";
			typeMap[typeof(ulong?)] = "ulong?";
			typeMap[typeof(float?)] = "float?";
			typeMap[typeof(double?)] = "double?";
			typeMap[typeof(decimal?)] = "decimal?";
			typeMap[typeof(bool?)] = "bool?";
			typeMap[typeof(char?)] = "char?";
			typeMap[typeof(Guid?)] = "Guid?";
			typeMap[typeof(DateTime?)] = "DateTime?";
			typeMap[typeof(DateTimeOffset?)] = "DateTimeOffset?";
			typeMap[typeof(TimeSpan?)] = "TimeSpan?";
			typeMap[typeof(UIntPtr)] = "UIntPtr";
			typeMap[typeof(void)] = "void";
			// typeMap[typeof(System.Data.Linq.Binary)] = "Binary";

			typeMapReverse = new Dictionary<string, Type>();
			foreach (var i in typeMap)
			{
				typeMapReverse[i.Value] = i.Key;
			}

			unsignedSet = new HashSet<Type>();
			unsignedSet.Add(typeof(byte));
			unsignedSet.Add(typeof(ushort));
			unsignedSet.Add(typeof(uint));
			unsignedSet.Add(typeof(ulong));
			unsignedSet.Add(typeof(byte?));
			unsignedSet.Add(typeof(ushort?));
			unsignedSet.Add(typeof(uint?));
			unsignedSet.Add(typeof(ulong?));
			unsignedSet.Add(typeof(UIntPtr));

			numericSet = new HashSet<Type>
			{
				typeof(byte),
				typeof(sbyte),
				typeof(short),
				typeof(ushort),
				typeof(int),
				typeof(uint),
				typeof(long),
				typeof(ulong),
				typeof(decimal),
				typeof(float),
				typeof(double)
			};
		}

		public static string ToCsType(Type type)
		{
			try
			{
				return typeMap[type];
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static string ToCsType(string type)
		{
			try
			{
				return typeMap[Type.GetType(type)];
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static Type GetType(string csType)
		{
			try
			{
				return typeMapReverse[csType];
			}
			catch (Exception)
			{
				return null;
			}
		}

		public static bool IsUnsigned(Type type)
		{
			return unsignedSet.Contains(type);
		}

		public static bool IsNumeric(Type type)
		{
			return numericSet.Contains(type);
		}
	}
}

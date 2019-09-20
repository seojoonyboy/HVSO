using System;
using System.Text;
using System.Collections.Generic;

namespace G.Util
{
	public class StringEx
	{
		public static string LineSingle(int length, string title = null)
		{
			return LineText('-', length, title);
		}

		public static string LineDouble(int length, string title = null)
		{
			return LineText('=', length, title);
		}

		public static string LineText(char ch, int length, string title = null)
		{
			if (String.IsNullOrEmpty(title))
				return new String(ch, length);

			int len = title.Length;
			if (len + 1 >= length)
				return title;
			else
				return String.Format("{0} {1}", title, new String(ch, length - len - 1));
		}

		public static string ToStringWithSeparator<T>(char separator, params T[] array)
		{
			if (array == null || array.Length == 0) return string.Empty;

			bool isFirst = true;
			StringBuilder sb = new StringBuilder();
			foreach (T t in array)
			{
				if (isFirst)
					isFirst = false;
				else
					sb.Append(separator);
				sb.Append(t);
			}
			return sb.ToString();
		}

		public static string ToStringWithSeparator<T>(char separator, List<T> list)
		{
			if (list == null) return string.Empty;
			return ToStringWithSeparator<T>(separator, list.ToArray());
		}

		public static string ToStringWithSeparator(char separator, IEnumerable<string> enums)
		{
			if (enums == null) return string.Empty;

			bool isFirst = true;
			StringBuilder sb = new StringBuilder();
			foreach (string e in enums)
			{
				if (isFirst)
					isFirst = false;
				else
					sb.Append(separator);
				sb.Append(e);
			}
			return sb.ToString();
		}

		public static List<T> FromStringWithSeparator<T>(char separator, string text, T defaultValue = default(T))
		{
			string[] tokens = text.Split(separator);
			List<T> list = new List<T>(tokens.Length);
			foreach (string token in tokens)
			{
				if (String.IsNullOrEmpty(token) || token.Trim().Length == 0)
					list.Add(defaultValue);
				else
					list.Add((T)Convert.ChangeType(token, typeof(T)));
			}
			return list;
		}

		public static string ToStringWithComma<T>(params T[] array)
		{
			return ToStringWithSeparator<T>(',', array);
		}

		public static string ToStringWithComma<T>(List<T> list)
		{
			return ToStringWithSeparator<T>(',', list);
		}

		public static string ToStringWithComma(IEnumerable<string> enums)
		{
			return ToStringWithSeparator(',', enums);
		}

		public static List<T> FromStringWithComma<T>(string text, T defaultValue = default(T))
		{
			return FromStringWithSeparator<T>(',', text, defaultValue);
		}

		public static string ToStringWithDot<T>(params T[] array)
		{
			return ToStringWithSeparator<T>('.', array);
		}

		public static string ToStringWithDot<T>(List<T> list)
		{
			return ToStringWithSeparator<T>('.', list);
		}

		public static string ToStringWithDot(IEquatable<string> enums)
		{
			return ToStringWithSeparator('.', enums);
		}

		public static List<T> FromStringWithDot<T>(string text, T defaultValue = default(T))
		{
			return FromStringWithSeparator<T>('.', text, defaultValue);
		}

		#region Camel & Pascal
		public static string ToCamelString(string text)
		{
			return ToCamelPascalString(text, false);
		}

		public static string ToPascalString(string text)
		{
			return ToCamelPascalString(text, true);
		}

		public static string ToCamelPascalString(string text, bool isPascal = false)
		{
			if (string.IsNullOrEmpty(text)) return text;

			text = ArrageCase(text);

			StringBuilder sb = new StringBuilder();

			bool isFirst = true;
			string[] tokens = SplitToCamelPascal(text);
			foreach (var t in tokens)
			{
				string lt = t.ToLower();

				if (isFirst)
				{
					isFirst = false;
					if (isPascal)
						sb.Append(ToUpperFirstCharacter(lt));
					else
						sb.Append(ToLowerFirstCharacter(lt));
				}
				else
					sb.Append(ToUpperFirstCharacter(lt));
			}

			return sb.ToString();
		}

		public static string ToLowerFirstCharacter(string text)
		{
			if (string.IsNullOrEmpty(text)) return text;
			if (!char.IsUpper(text[0])) return text;

			char[] chs = text.ToCharArray();
			chs[0] = char.ToLower(chs[0]);
			return new string(chs);
		}

		public static string ToUpperFirstCharacter(string text)
		{
			if (string.IsNullOrEmpty(text)) return text;
			if (!char.IsLower(text[0])) return text;

			char[] chs = text.ToCharArray();
			chs[0] = char.ToUpper(chs[0]);
			return new string(chs);
		}

		public static string[] SplitToCamelPascal(string text)
		{
			if (text == null) return null;
			if (text.Length == 0) return new string[] { "" };

			List<string> result = new List<string>();

			string[] tokens = text.Split('_', '-', ' ', '\t');
			foreach (var t in tokens)
			{
				string[] ss = _SplitToCamelPascal(t);
				foreach (var s in ss)
				{
					if (string.IsNullOrEmpty(s.Trim())) continue;
					result.Add(s);
				}
			}

			return result.ToArray();
		}

		private static string[] _SplitToCamelPascal(string text)
		{
			if (text == null) return null;
			if (text.Length == 0) return new string[] { "" };

			List<string> tokens = new List<string>();

			int kind = 0;       // 0:Start, 1:Upper, 2:Lower, 3:Etc
			int offset = 0;

			char[] chs = text.ToCharArray();
			for (int i = 0; i < chs.Length; i++)
			{
				char ch = chs[i];

				if (char.IsUpper(ch))
				{
					if (kind == 0) kind = 1;
					else if (kind != 1)
					{
						kind = 1;
						tokens.Add(text.Substring(offset, i - offset));
						offset = i;
					}
				}
				else if (char.IsLower(ch))
				{
					if (kind == 0 || kind == 1) kind = 2;
					else if (kind != 2)
					{
						kind = 2;
						tokens.Add(text.Substring(offset, i - offset));
						offset = i;
					}
				}
				else
				{
					if (kind == 0) kind = 3;
					else if (kind != 3)
					{
						kind = 3;
						tokens.Add(text.Substring(offset, i - offset));
						offset = i;
					}
				}
			}

			if (chs.Length != offset)
			{
				tokens.Add(text.Substring(offset, chs.Length - offset));
			}

			return tokens.ToArray();
		}

		public static string ArrageCase(string text)
		{
			text = text.Replace("ID", "Id").Replace("SN", "Sn").Replace("NO", "No").Replace("LV", "Lv");
			text = text.Replace("MIN", "Min").Replace("MAX", "Max").Replace("HP", "Hp").Replace("MP", "Mp");
			text = text.Replace("BGM", "Bgm").Replace("EXP", "Exp").Replace("NAME", "Name");
			return text;
		}
		#endregion
	}
}

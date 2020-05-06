using System;
using System.Text;

namespace G.Util
{
	public class CodeBuilder
	{
		protected StringBuilder sb = new StringBuilder();
		protected bool isNewLine = true;

		private int initTab;
		public int tab;

		public CodeBuilder(int initTab = 0)
		{
			this.initTab = initTab;
			tab = initTab;
		}

		public _Tab OpenTab()
		{
			return new _Tab(this);
		}

		public _Brace OpenBrace(bool terminatedWithSemiColon = false)
		{
			if (isNewLine)
			{
				sb.Append(GetTabs());
				sb.AppendLine("{");
			}
			else
			{
				sb.AppendLine(" {");
				isNewLine = true;
			}

			return new _Brace(this, terminatedWithSemiColon);
		}

		public string GetTabs()
		{
			return new String('\t', tab);
		}

		public void Clear()
		{
			tab = initTab;
			isNewLine = true;

			sb.Clear();
		}

		public CodeBuilder Append(string code)
		{
			if (isNewLine)
			{
				isNewLine = false;
				sb.Append(GetTabs());
			}

			sb.Append(code);
			return this;
		}

		public CodeBuilder AppendFormat(string format, params object[] args)
		{
			if (isNewLine)
			{
				isNewLine = false;
				sb.Append(GetTabs());
			}

			sb.AppendFormat(format, args);
			return this;
		}

		public CodeBuilder AppendLine(string code)
		{
			if (isNewLine)
				sb.Append(GetTabs());

			sb.AppendLine(code);

			isNewLine = true;

			return this;
		}

		public CodeBuilder AppendLine(string format, params object[] args)
		{
			return AppendLine(string.Format(format, args));
		}

		public CodeBuilder AppendLine()
		{
			sb.AppendLine();
			isNewLine = true;

			return this;
		}

		public override string ToString()
		{
			return sb.ToString();
		}
	}

	public class _Tab : IDisposable
	{
		private bool disposed = false;
		protected CodeBuilder codeBuilder;

		public _Tab(CodeBuilder codeBuilder)
		{
			this.codeBuilder = codeBuilder;
			codeBuilder.tab++;
		}

		~_Tab()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed) return;
			if (disposing) codeBuilder.tab--;
			disposed = true;
		}
	}

	public class _Brace : _Tab
	{
		private bool disposed = false;
		private bool terminatedWithSemiColon;

		public _Brace(CodeBuilder codeBuilder, bool terminatedWithSemiColon) : base(codeBuilder)
		{
			this.terminatedWithSemiColon = terminatedWithSemiColon;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposed) return;

			if (disposing)
			{
				codeBuilder.tab--;
				if (terminatedWithSemiColon)
					codeBuilder.AppendLine("};");
				else
					codeBuilder.AppendLine("}");
				codeBuilder.tab++;
			}

			disposed = true;

			base.Dispose(disposing);
		}
	}
}

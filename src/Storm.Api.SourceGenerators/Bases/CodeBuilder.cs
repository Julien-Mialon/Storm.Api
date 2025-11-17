using System.Text;

namespace Storm.Api.SourceGenerators.Bases;

public class CodeBuilder
{
	private readonly char _indentChar;
	private readonly StringBuilder _content = new StringBuilder();

	private int _indentLevel;
	private bool _isStartLine = true;

	public CodeBuilder(char indentChar = '\t', int startWithIndentLevel = 0)
	{
		_indentChar = indentChar;
		_indentLevel = startWithIndentLevel;
	}

	public string Code()
	{
		if (_isStartLine is false)
		{
			_content.AppendLine();
			_isStartLine = true;
		}
		return _content.ToString();
	}

	public override string ToString()
	{
		return Code();
	}

	public CodeBuilder Indent()
	{
		_indentLevel++;
		return this;
	}

	public CodeBuilder Unindent()
	{
		_indentLevel--;
		return this;
	}

	public CodeBuilder Add(string content)
	{
		if (_isStartLine)
		{
			_content.Append(_indentChar, _indentLevel);
		}

		_content.Append(content);
		_isStartLine = false;
		return this;
	}

	public CodeBuilder AddLine(string content)
	{
		if (_isStartLine)
		{
			_content.Append(_indentChar, _indentLevel);
		}

		_content.AppendLine(content);
		_isStartLine = true;
		return this;
	}

	public CodeBuilder AddLine()
	{
		_content.AppendLine();
		_isStartLine = true;
		return this;
	}

	public CodeBuilder EndLine()
	{
		_isStartLine = true;
		return this;
	}
}
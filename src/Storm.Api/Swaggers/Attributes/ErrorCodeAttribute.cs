using System;

namespace Storm.Api.Swaggers.Attributes
{
	[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public sealed class ErrorCodeAttribute : Attribute
	{
		public string ErrorCode { get; }
		public string Explanation { get; }

		public ErrorCodeAttribute(string errorCode)
		{
			ErrorCode = errorCode;
		}

		public ErrorCodeAttribute(string errorCode, string explanation) : this(errorCode)
		{
			Explanation = explanation;
		}
	}
}
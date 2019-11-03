using System;
using System.Collections.Generic;
using System.Text;

namespace Storm.Api.Core.Logs.Appenders
{
	public sealed class TimestampLogAppender : ILogAppender
	{
		private readonly Func<DateTime> _timestamp;
		private readonly string _fieldName;

		public TimestampLogAppender(string fieldName = "timestamp") : this(() => DateTime.UtcNow, fieldName)
		{
		}

		public TimestampLogAppender(Func<DateTime> timestamp, string fieldName = "timestamp")
		{
			_timestamp = timestamp;
			_fieldName = fieldName;
		}

		public void Append(IObjectWriter writer)
		{
			writer.WriteProperty(_fieldName, _timestamp());
		}
	}
}

using System;
using System.IO;

namespace Storm.Api.Core.Domains.Results
{
	internal class FileByteResult : FileResult
	{
		public byte[] Data { get; set; }


		public override bool IsRawData => true;
		public override bool IsStreamData => false;
		public override byte[] AsRawData() => Data;
		public override Stream AsStreamData() => throw new InvalidOperationException("Stream data not supported");
	}
}
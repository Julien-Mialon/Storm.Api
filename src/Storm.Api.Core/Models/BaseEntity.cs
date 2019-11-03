using System;
using ServiceStack.DataAnnotations;

namespace Storm.Api.Core.Models
{
	public abstract class BaseEntity
	{
		[PrimaryKey]
		[AutoIncrement]
		public long Id { get; set; }

		[Index]
		public Guid CollationId { get; set; }

		[Index]
		public DateTime EntityCreatedDate { get; set; }

		public DateTime? EntityUpdatedDate { get; set; }

		[Index]
		public bool IsDeleted { get; set; }

		public DateTime? EntityDeletedDate { get; set; }
	}
}
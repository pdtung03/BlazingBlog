using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazingBlog.Data.Entities
{
	public class BlogPost
	{
		public int Id { get; set; }

		[Required, MaxLength(100)]
		public string Title { get; set; }

		[MaxLength(125)]
		public string Slug { get; set; }

		[Required, MaxLength(100)]
		public string Image { get; set; }

		[Required, MaxLength(500)]
		public string Introduction { get; set; }

		[Required]
		public string Content { get; set; }

		public int CategoryId { get; set; }

		public string UserId { get; set; }

		public bool isPublished { get; set; }

		public int ViewCount { get; set; }

		public bool isFeatured { get; set; }

		public DateTime CreatedAt { get; set; }
		public DateTime? PublishedAt { get; set; }

		public virtual Category Category { get; set; }
		public virtual ApplicationUser User { get; set; }



	}
}

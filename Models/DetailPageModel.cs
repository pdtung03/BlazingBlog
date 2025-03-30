using BlazingBlog.Data.Entities;

namespace BlazingBlog.Models
{
    public record DetailPageModel(BlogPost BlogPost, BlogPost[] RelatedPosts)
    {
        public static DetailPageModel Empty() => new(default, []);

        public bool isEmpty => BlogPost is null;
    }
}

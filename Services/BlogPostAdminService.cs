using BlazingBlog.Data;
using BlazingBlog.Data.Entities;
using BlazingBlog.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazingBlog.Services
{
    public interface IBlogPostAdminService
    {
        Task<PagedResult<BlogPost>> GetBlogPostsAsync(int startIndex, int pageSize);
        Task<BlogPost?> GetBlogPostByIdAsync(int id);
        Task<BlogPost> SaveBlogPostAsync(BlogPost blogPost, string userId);
    }
    public class BlogPostAdminService : IBlogPostAdminService
    {
        private IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BlogPostAdminService(IDbContextFactory<ApplicationDbContext> contextFactory) 
        {
            _contextFactory = contextFactory;
        }

        private async Task<TResult> ExecuteOnContext<TResult>(Func<ApplicationDbContext, Task<TResult>> query)
        {
            using var context = _contextFactory.CreateDbContext();
            return await query.Invoke(context);
        }

        public async Task<BlogPost> GetBlogPostByIdAsync(int id) =>
            await ExecuteOnContext(async context =>
                await context.BlogPosts
                        .AsNoTracking()
                        .Include(b => b.Category)
                        .FirstOrDefaultAsync(b => b.Id == id)
            );

        public async Task<PagedResult<BlogPost>> GetBlogPostsAsync(int startIndex, int pageSize)
        {
           return await ExecuteOnContext(async context =>
           {

               var query = context.BlogPosts
                                   .AsNoTracking();

                   var count = await query.CountAsync();

                   var records = await query.Include(b => b.Category)
                                          .OrderByDescending(b => b.Id)
                                          .Skip(startIndex)
                                          .Take(pageSize)
                                          .ToArrayAsync();

               return new PagedResult<BlogPost>(records, count);
              
           });
        }
        
        private async Task<string> GenerateSlug(BlogPost blogPost)
        {
            return await ExecuteOnContext(async context =>
            {
                string originalSlug = blogPost.Title.ToSlug();
                string slug = originalSlug;
                int count = 1;

                while(await context.BlogPosts.AsNoTracking().AnyAsync(b => b.Slug == slug))
                {
                    slug = $"{originalSlug}-{count++}";
                }
                return slug;
                });
        }
        public Task<BlogPost> SaveBlogPostAsync(BlogPost blogPost,string userId)
        {
            return ExecuteOnContext(async context =>
            {
                if (blogPost.Id == 0)
                {
                    //new blog post
                    var duplicateTitle = await context.BlogPosts
                                                      .AsNoTracking().AnyAsync(b =>b.Title == blogPost.Title);
                    if(duplicateTitle)
                    {
                        throw new InvalidOperationException($"Blog post with this title {blogPost.Title} already exists");
                    }
                    blogPost.Slug = await GenerateSlug(blogPost);
                    blogPost.CreatedAt = DateTime.UtcNow;
                    blogPost.UserId = userId;

                    if(blogPost.isPublished)
                    {
                        blogPost.PublishedAt = DateTime.UtcNow;
                    }
                    context.BlogPosts.AddAsync(blogPost);

                }
                else
                {
                    //existing blogpost
                    var duplicateTitle = await context.BlogPosts
                                                     .AsNoTracking().AnyAsync(b => b.Title == blogPost.Title && b.Id != blogPost.Id);
                    if (duplicateTitle)
                    {
                        throw new InvalidOperationException($"Blog post with this title {blogPost.Title} already exists");
                    }

                    var dbBlog = await context.BlogPosts.FindAsync(blogPost.Id);

                    dbBlog.Title = blogPost.Title;
                    dbBlog.Image = blogPost.Image;
                    dbBlog.Content = blogPost.Content;
                    dbBlog.CategoryId = blogPost.CategoryId;
                    dbBlog.isPublished = blogPost.isPublished;
                    dbBlog.isFeatured = blogPost.isFeatured;
                    dbBlog.PublishedAt = blogPost.PublishedAt;
                    //dbBlog.Id = blogPost.Id;
                    //dbBlog.CreatedAt = DateTime.UtcNow;
                    //dbBlog.UserId = userId;
                    //dbBlog.Introduction = blogPost.Introduction;

                    if(blogPost.isPublished)
                    {
                        if(!dbBlog.isPublished)
                        {
                            blogPost.PublishedAt = DateTime.UtcNow;
                        }
                    }
                    else
                    {
                        blogPost.PublishedAt = null;
                    }

                }

                await context.SaveChangesAsync();
                return blogPost;
            });
        }
    }
}

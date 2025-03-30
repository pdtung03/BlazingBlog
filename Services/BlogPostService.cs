using BlazingBlog.Data;
using BlazingBlog.Data.Entities;
using BlazingBlog.Models;
using Microsoft.EntityFrameworkCore;

namespace BlazingBlog.Services
{
    public interface IBlogPostService
    {
        Task<DetailPageModel> GetBlogPostBySlugAsync(string slug);
        Task<BlogPost[]> GetFeaturedBlogPostsAsync(int count, int categoryId = 0);
        Task<BlogPost[]> GetPopularBlogPostsAsync(int count, int categoryId = 0);
        Task<BlogPost[]> GetRecentBlogPostsAsync(int count, int categoryId = 0);
		Task<BlogPost[]> GetBlogPostAsync(int pageIndex, int pageSize, int categoryId = 0);

	}

    public class BlogPostService : IBlogPostService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public BlogPostService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<Tresult> QueryOnContextAsync<Tresult>(Func<ApplicationDbContext, Task<Tresult>> query)
        {
            using var context = _contextFactory.CreateDbContext();
            return await query(context);
        }

        public async Task<BlogPost[]> GetFeaturedBlogPostsAsync(int count, int categoryId = 0)
        {
            return await QueryOnContextAsync(async context =>
            {
                var query = context.BlogPosts
                                   .AsNoTracking()
                                   .Include(p => p.Category)
                                   .Include(b => b.User)
                                   .Where(b => b.isPublished);
                if (categoryId > 0)
                {
                    query = query.Where(b => b.CategoryId == categoryId);
                }

                var records = await query.Where(b => b.isFeatured)
                            .OrderBy(_ => Guid.NewGuid())
                            .Take(count)
                            .ToArrayAsync();

                if(count > records.Length)
                {
                    var additionalRecords = await query.Where(b => !b.isFeatured)
							                            .OrderBy(_ => Guid.NewGuid())
							                            .Take(count - records.Length)
							                            .ToArrayAsync();
                    records = [.. records, .. additionalRecords];
				}
				return records;
			});
        }

        public async Task<BlogPost[]> GetPopularBlogPostsAsync(int count, int categoryId = 0)
        {
            return await QueryOnContextAsync(async context =>
            {
                var query = context.BlogPosts
                                   .AsNoTracking()
                                   .Include(p => p.Category)
                                   .Include(b => b.User)
                                   .Where(b => b.isPublished);
                if (categoryId > 0)
                {
                    query = query.Where(b => b.CategoryId == categoryId);
                }

                return await query.OrderByDescending(b => b.ViewCount)
                            .Take(count)
                            .ToArrayAsync();
            });
        }

        public async Task<BlogPost[]> GetRecentBlogPostsAsync(int count, int categoryId = 0) =>
			await GetPostsAsync(0, count, categoryId);

		public async Task<DetailPageModel> GetBlogPostBySlugAsync(string slug)
        {
            return await QueryOnContextAsync(async context =>
            {
                var blogPost = await context.BlogPosts.AsNoTracking()
                                            .Include(b => b.Category)
                                            .Include(b => b.User)
                                            .FirstOrDefaultAsync(b => b.Slug == slug && b.isPublished);

                if (blogPost is null)
                    return DetailPageModel.Empty();

                var relatedPosts = await context.BlogPosts
                                          .AsNoTracking()
                                          .Include(b => b.Category)
                                          .Include(b => b.User)
                                          .Where(b => b.CategoryId == blogPost.CategoryId && b.isPublished)
                                          .OrderBy(_ => Guid.NewGuid())
                                          .Take(4)
                                          .ToArrayAsync();

                return new DetailPageModel(blogPost, relatedPosts);
            });
        }

        public async Task<BlogPost[]> GetBlogPostAsync(int pageIndex, int pageSize, int categoryId = 0) =>
            await GetPostsAsync(pageIndex * pageSize, pageSize, categoryId);

		

        private async Task<BlogPost[]> GetPostsAsync(int skip,int take,int categoryId)
        {
			return await QueryOnContextAsync(async context =>
			{
				var query = context.BlogPosts
								   .AsNoTracking()
								   .Include(p => p.Category)
								   .Include(b => b.User)
								   .Where(b => b.isPublished);
				if (categoryId > 0)
				{
					query = query.Where(b => b.CategoryId == categoryId);
				}

				return await query.OrderByDescending(b => b.PublishedAt)
							.Skip(skip)
							.Take(take)
							.ToArrayAsync();
			});
		}


	}
}

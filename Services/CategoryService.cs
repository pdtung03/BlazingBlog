using BlazingBlog.Data;
using BlazingBlog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace BlazingBlog.Services
{
    public interface ICategoryService
    {
        Task<Category[]> GetCategoriesAsync();
        Task<Category?> GetCategoryBySlugAsync(string slug);
        Task<Category> SaveCategoriyAsync(Category category);
    }

    public class CategoryService : ICategoryService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public CategoryService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        private async Task<TResult> ExecuteOnContext<TResult>(Func<ApplicationDbContext, Task<TResult>> query)
        {
            using var context = _contextFactory.CreateDbContext();
            return await query.Invoke(context);
        }

        public async Task<Category[]> GetCategoriesAsync()
        {
            return await ExecuteOnContext(async context =>
            {
                var categories = await context.Categories
                                        .AsNoTracking()
                                        .ToArrayAsync();
                return categories;
            });

        }

        public async Task<Category> SaveCategoriyAsync(Category category)
        {
            return await ExecuteOnContext(async context =>
            {
                if (category.Id == 0)
                {
                    // Its a new category
                    if (await context.Categories
                                .AsNoTracking()
                                .AnyAsync(c => c.Name == category.Name))
                    {
                        throw new InvalidOperationException($"Category with the name {category.Name} already exists");
                    }
                    category.Slug = category.Name.ToSlug();
                    await context.Categories.AddAsync(category);
                    await context.SaveChangesAsync();
                }
                else
                {
                    // Its an existing category
                    if (await context.Categories
                                .AsNoTracking()
                                .AnyAsync(c => c.Name == category.Name && c.Id != category.Id))
                    {
                        throw
                        new InvalidOperationException($"Category with the name {category.Name} already exists");
                    }
                    var dbCategory = await context.Categories
                                                  .FindAsync(category.Id);

                    dbCategory.Name = category.Name;
                    dbCategory.ShowOnNavbar = category.ShowOnNavbar;

                    category.Slug = dbCategory.Slug;
                    //Its a new category
                }
                await context.SaveChangesAsync();
                return category;
            });


        }

        public async Task<Category?> GetCategoryBySlugAsync(string slug) =>
            await ExecuteOnContext(async context =>
                await context.Categories
                             .AsNoTracking()
                             .FirstOrDefaultAsync(c => c.Slug == slug)
            );
    }
}

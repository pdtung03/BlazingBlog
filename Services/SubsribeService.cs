using BlazingBlog.Data;
using BlazingBlog.Data.Entities;
using BlazingBlog.Models;
using Microsoft.EntityFrameworkCore;


namespace BlazingBlog.Services
{
	public interface ISubsribeService
	{
		Task<string?> SubsribeAsync(Subscriber subscriber);
		Task<PagedResult<Subscriber>> GetSubscribersAsync(int startIndex, int pageSize);

    }

	public class SubsribeService : ISubsribeService
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

		public SubsribeService(IDbContextFactory<ApplicationDbContext> contextFactory)
		{
			_contextFactory = contextFactory;
		}

		public async Task<string?> SubsribeAsync(Subscriber subscriber)
		{
			using var context = _contextFactory.CreateDbContext();
			var IsAlreadySubscribed = await context.Subscribers
												   .AsNoTracking()
												   .AnyAsync(s => s.Email == subscriber.Email);
			if (IsAlreadySubscribed)
			{
				return "You are already subscribed.";
			}
			subscriber.SubscribedOn = DateTime.Now;
			await context.Subscribers.AddAsync(subscriber);
			await context.SaveChangesAsync();
			return null;

		}

		public async Task<PagedResult<Subscriber>> GetSubscribersAsync(int startIndex, int pageSize)
		{
			using var context = _contextFactory.CreateDbContext();
			var query = context.Subscribers
                               .AsNoTracking()
                               .OrderByDescending(s => s.SubscribedOn);

			var totalRecords = await query.CountAsync();

			var records = await query.Skip(startIndex)
                                        .Take(pageSize)
                                        .ToArrayAsync();

			return new PagedResult<Subscriber>(records, totalRecords);
		}
	}
}

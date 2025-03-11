using BlazingBlog.Data;
using BlazingBlog.Data.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazingBlog.Services
{
	internal static class AdminAccount
	{
		public const string Name = "Phan Tung";
		public const string Email = "phanductung03@gmail.com";
		public const string Role = "Admin";
		public const string Password = "P@ssword123";
	}

	public interface ISeedService
	{
		Task SeedDataAsync();
	}

	public class SeedService : ISeedService
	{
		private readonly ApplicationDbContext _context;
		private readonly IUserStore<ApplicationUser> _userStore;
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;

		public SeedService(ApplicationDbContext context, IUserStore<ApplicationUser> userStore,
			UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
		{
			_context = context;
			_userStore = userStore;
			_userManager = userManager;
			_roleManager = roleManager;
		}
		public async Task SeedDataAsync()
		{
			//Seed AdminRole

			if (await _roleManager.FindByNameAsync(AdminAccount.Role) == null)
			{
				//This Admin role does not exist in the database
				//Lets create it 
				var adminRole = new IdentityRole(AdminAccount.Role);

				var result = await _roleManager.CreateAsync(adminRole);
				if (!result.Succeeded)
				{
					var errorsString = result.Errors.Select(e => e.Description);
					throw new Exception($"Error in creating Admin Role {Environment.NewLine}{string.Join(Environment.NewLine, errorsString)}");
				}
			}
			//Seed Admin User

			var adminUser = await _userManager.FindByEmailAsync(AdminAccount.Email);
			if (adminUser == null)
			{
				//This Admin role does not exist in the database
				//Lets create it 
				adminUser = new ApplicationUser();

				adminUser.Name = AdminAccount.Name;

				await _userStore.SetUserNameAsync(adminUser, AdminAccount.Name, CancellationToken.None);

				var result = await _userManager.CreateAsync(adminUser, AdminAccount.Password);
				if (!result.Succeeded)
				{
					var errorsString = result.Errors.Select(e => e.Description);
					throw new Exception($"Error in creating Admin User {Environment.NewLine}{string.Join(Environment.NewLine, errorsString)}");

				}
			}
			//Seed Categories

			if (!await _context.Categories.AsNoTracking().AnyAsync())
			{
				//There are no categories in the database
				//Lets create some categories
				_context.Categories.AddRangeAsync(Category.GetSeedCategories());
				await _context.SaveChangesAsync();
			}
		}
	}
}

using AuthService.Domain.Entities;
using AuthService.Domain.Interfaces;
using AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AuthService.Infrastructure.Repositories
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public UserRepository(ApplicationDbContext context, UserManager<ApplicationUser> userManager) : base(context)
        {
            _userManager = userManager;
            _dbContext = context;
        }

        public async Task<ApplicationUser> FindByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<(List<(ApplicationUser User, string Role)> Users, int TotalCount)> GetUsersAsync(string sortBy, string sortOrder, string emailFilter, string roleFilter, int page, int pageSize)
        {
            var query = _dbSet.AsQueryable();

            if (!string.IsNullOrEmpty(emailFilter))
            {
                query = query.Where(u => u.Email.Contains(emailFilter));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .Select(u => new
                {
                    User = u,
                    RoleId = _dbContext.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Select(ur => ur.RoleId)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var userList = new List<(ApplicationUser User, string Role)>();
            foreach (var userData in users)
            {
                var roleName = userData.RoleId != null
                    ? (await _dbContext.Roles.FirstOrDefaultAsync(r => r.Id == userData.RoleId))?.Name ?? "User"
                    : "User";
                if (!string.IsNullOrEmpty(roleFilter) && roleName != roleFilter)
                {
                    continue;
                }
                userList.Add((userData.User, roleName));
            }

            userList = sortBy switch
            {
                "email" => sortOrder == "desc" ? userList.OrderByDescending(u => u.User.Email).ToList() : userList.OrderBy(u => u.User.Email).ToList(),
                "role" => sortOrder == "desc" ? userList.OrderByDescending(u => u.Role).ToList() : userList.OrderBy(u => u.Role).ToList(),
                _ => userList.OrderBy(u => u.User.Email).ToList()
            };

            var pagedUsers = userList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (pagedUsers, totalCount);
        }

        public async Task UpdateRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return;

            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, role);
        }
    }
}
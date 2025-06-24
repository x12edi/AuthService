using AuthService.Domain.Entities;
using System.Linq.Expressions;

namespace AuthService.Domain.Interfaces
{
    public interface IUserRepository : IRepository<ApplicationUser>
    {
        Task<ApplicationUser> FindByEmailAsync(string email);
        Task<(List<(ApplicationUser User, string Role)> Users, int TotalCount)> GetUsersAsync(string sortBy, string sortOrder, string emailFilter, string roleFilter, int page, int pageSize);
        Task UpdateRoleAsync(string userId, string role);
    }
}
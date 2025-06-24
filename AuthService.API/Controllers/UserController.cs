using AuthService.Application.DTOs;
using AuthService.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            return Ok(new { UserId = userId, Email = email, Role = role });
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetUsers([FromQuery] string sortBy = "email", [FromQuery] string sortOrder = "asc", [FromQuery] string emailFilter = "", [FromQuery] string roleFilter = "", [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var (users, totalCount) = await _unitOfWork.Users.GetUsersAsync(sortBy, sortOrder, emailFilter, roleFilter, page, pageSize);
            var userDtos = users.Select(u => new UserDto
            {
                Id = u.User.Id,
                Email = u.User.Email,
                FirstName = u.User.FirstName,
                LastName = u.User.LastName,
                Role = u.Role
            }).ToList();
            return Ok(new { Users = userDtos, TotalCount = totalCount });
        }

        [HttpPut("role")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleDto roleDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (currentUserId == roleDto.UserId)
            {
                return BadRequest("Cannot change your own role.");
            }

            await _unitOfWork.Users.UpdateRoleAsync(roleDto.UserId, roleDto.Role);
            await _unitOfWork.CompleteAsync();
            return Ok();
        }
    }
}
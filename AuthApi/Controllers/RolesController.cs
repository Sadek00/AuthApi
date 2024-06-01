using AuthApi.Models;
using AuthApi.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Linq;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Authorize(Roles = "Owner")]
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpPost("CreateRole")]
        [SwaggerOperation(Summary = "Create a new role", Description = "Creates a new role with the specified name.")]
        [SwaggerResponse(200, "Role created successfully")]
        [SwaggerResponse(400, "Invalid role name or role already exists")]
        public async Task<ActionResult<string>> CreateRole([FromBody] CreateRoleDto createRoleDto)
        {
            if (string.IsNullOrEmpty(createRoleDto.RoleName))
            {
                return BadRequest("Role name is required");
            }

            if (await _roleManager.RoleExistsAsync(createRoleDto.RoleName))
            {
                return BadRequest("Role already exists");
            }

            var role = new IdentityRole(createRoleDto.RoleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                return Ok("Role created successfully");
            }

            return BadRequest("Role creation failed");
        }

        [HttpGet("GetRoles")]
        [SwaggerOperation(Summary = "Get all roles", Description = "Retrieves a list of all roles along with the total number of users in each role.")]
        [SwaggerResponse(200, "List of roles along with total users", typeof(RoleResponseDto[]))]
        public async Task<ActionResult<RoleResponseDto[]>> GetRoles()
        {
            var roles = await _roleManager.Roles
                .Select(r => new RoleResponseDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    TotalUsers = _userManager.GetUsersInRoleAsync(r.Name!).Result.Count
                })
                .ToArrayAsync();

            return Ok(roles);
        }

        [HttpDelete("DeleteRole/{id}")]
        [SwaggerOperation(Summary = "Delete a role", Description = "Deletes the role with the specified ID.")]
        [SwaggerResponse(200, "Role deleted successfully")]
        [SwaggerResponse(400, "Role deletion failed")]
        public async Task<ActionResult<string>> DeleteRole(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null)
            {
                return NotFound("Role not found");
            }

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                return Ok("Role deleted successfully");
            }

            return BadRequest("Role deletion failed");
        }

        [HttpPost("AssignRoleToUser")]
        [SwaggerOperation(Summary = "Assign role to user", Description = "Assigns a role to the user with the specified ID.")]
        [SwaggerResponse(200, "Role assigned successfully")]
        [SwaggerResponse(400, "Failed to assign role")]
        public async Task<ActionResult<string>> AssignRole([FromBody] RoleAssignDto roleAssignDto)
        {
            var user = await _userManager.FindByIdAsync(roleAssignDto.UserId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var role = await _roleManager.FindByIdAsync(roleAssignDto.RoleId);
            if (role == null)
            {
                return NotFound("Role not found");
            }

            var result = await _userManager.AddToRoleAsync(user, role.Name!);
            if (result.Succeeded)
            {
                return Ok("Role assigned successfully");
            }

            return BadRequest(result.Errors.FirstOrDefault()?.Description ?? "Failed to assign role");
        }
    }
}

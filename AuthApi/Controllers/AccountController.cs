using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthApi.Models;
using AuthApi.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<AppUser> userManager,
                                 RoleManager<IdentityRole> roleManager,
                                 IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        [SwaggerOperation(Summary = "Registers a new user.", Description = "Creates a new user with the specified details.")]
        [SwaggerResponse(200, "Account Created Successfully!", typeof(AuthResponseDto))]
        [SwaggerResponse(400, "Invalid input.", typeof(IEnumerable<IdentityError>))]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = new AppUser
            {
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                UserName = registerDto.Email
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roles = registerDto.Roles ?? new List<string> { "User" };
            foreach (var role in roles)
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return Ok(new AuthResponseDto { IsSuccess = true, Message = "Account Created Successfully!" });
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        [SwaggerOperation(Summary = "Logs in a user.", Description = "Authenticates a user and returns a JWT token.")]
        [SwaggerResponse(200, "Login Success.", typeof(AuthResponseDto))]
        [SwaggerResponse(401, "Unauthorized.", typeof(AuthResponseDto))]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "User not found with this email" });
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if (!result)
            {
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Invalid Password." });
            }

            var token = await GenerateTokenAsync(user);
            return Ok(new AuthResponseDto { Token = token, IsSuccess = true, Message = "Login Success." });
        }

        [HttpGet("UserDetails")]
        [SwaggerOperation(Summary = "Gets the details of the current user.", Description = "Retrieves the details of the currently authenticated user.")]
        [SwaggerResponse(200, "User details retrieved successfully.", typeof(UserDetailDto))]
        [SwaggerResponse(401, "Unauthorized.", typeof(AuthResponseDto))]
        [SwaggerResponse(404, "User not found.", typeof(AuthResponseDto))]
        public async Task<ActionResult<UserDetailDto>> GetUserDetail()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
            {
                return Unauthorized(new AuthResponseDto { IsSuccess = false, Message = "Unauthorized" });
            }

            var user = await _userManager.FindByIdAsync(currentUserId);
            if (user == null)
            {
                return NotFound(new AuthResponseDto { IsSuccess = false, Message = "User not found" });
            }

            var roles = await _userManager.GetRolesAsync(user);
            var userDetails = new UserDetailDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                Roles = roles.ToArray(),
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                AccessFailedCount = user.AccessFailedCount
            };

            return Ok(userDetails);
        }

        [HttpGet("GetAllUsers")]
        [SwaggerOperation(Summary = "Gets the details of all users.", Description = "Retrieves a list of details for all users.")]
        [SwaggerResponse(200, "User details retrieved successfully.", typeof(IEnumerable<UserDetailDto>))]
        public async Task<ActionResult<IEnumerable<UserDetailDto>>> GetUsers()
        {
            var users = await _userManager.Users
                .Select(u => new UserDetailDto
                {
                    Id = u.Id,
                    Email = u.Email ?? string.Empty,
                    FirstName = u.FirstName ?? string.Empty,
                    LastName = u.LastName ?? string.Empty,
                    Roles = _userManager.GetRolesAsync(u).Result.ToArray()
                })
                .ToListAsync();

            return Ok(users);
        }

        private async Task<string> GenerateTokenAsync(AppUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["JWTSetting:securityKey"]
                                               ?? throw new ArgumentNullException("JWTSetting:securityKey"));

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(JwtRegisteredClaimNames.Name, (user.FirstName ?? string.Empty) + " " + (user.LastName ?? string.Empty)),
                new(JwtRegisteredClaimNames.NameId, user.Id ?? string.Empty),
                new(JwtRegisteredClaimNames.Aud, _configuration["JWTSetting:validAudience"] ?? string.Empty),
                new(JwtRegisteredClaimNames.Iss, _configuration["JWTSetting:validIssuer"] ?? string.Empty)
            };

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}

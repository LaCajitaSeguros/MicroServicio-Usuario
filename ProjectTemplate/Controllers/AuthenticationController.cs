using Application.Auth;
using Autenticacion.Configuration;
using Domain.DTOs;
using Infraestructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Autenticacion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        //Maneja la autenticación de usuarios
        private readonly UserManager<IdentityUser> _userManager;
        private readonly JwtConfig _jwtConfig;
        private readonly AppDbContext _dbContext;

        public AuthenticationController(UserManager<IdentityUser> userManager,IOptions<JwtConfig> jwtConfig, AppDbContext dbContext)
        {
            _userManager = userManager;
            _jwtConfig = jwtConfig.Value;
            _dbContext = dbContext;
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Registrer([FromBody] UserRegistrationRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var emailExists = await _userManager.FindByEmailAsync(requestDto.EmailAddress);
            if (emailExists != null)
            {
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
            {
                "Email already exists"
            }
                });
            }

            var user = new IdentityUser()
            {
                Email = requestDto.EmailAddress,
                UserName = requestDto.EmailAddress
            };

            var isCreate = await _userManager.CreateAsync(user, requestDto.Password);

            if (isCreate.Succeeded)
            {
                var additionalData = new UserAdditionalData
                {
                    UserId = user.Id,
                    Name = requestDto.Name,
                    LastName = requestDto.LastName,
                    Dni = requestDto.Dni
                };

                _dbContext.UsersAdditionalData.Add(additionalData);
                await _dbContext.SaveChangesAsync();

                var token = GenerationToken(user);
                return Ok(new AuthResult()
                {
                    Result = true,
                    Token = token
                });
            }
            else
            {
                var errors = new List<string>();
                foreach (var err in isCreate.Errors)
                    errors.Add(err.Description);

                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = errors
                });
            }

            return BadRequest(new AuthResult()
            {
                Result = false,
                Errors = new List<string>()
        {
            "Unable to create user"
            }
                });
            }



        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto request) {
            if (!ModelState.IsValid) return BadRequest();

            //Check if the user with the email exists
            var existingUser = await _userManager.FindByEmailAsync(request.EmailAddress);

            if(existingUser==null)
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Invalid authentication request"
                    }
                });

            var checkUserAndPass= await _userManager.CheckPasswordAsync(existingUser, request.Password);
            //Si la contaseña no es correcta, devolver un error
            if (!checkUserAndPass)
                return BadRequest(new AuthResult()
                {
                    Result = false,
                    Errors = new List<string>()
                    {
                        "Invalid authentication request"
                    }
                });

            var token = GenerationToken(existingUser);
            return Ok(new AuthResult()
            {
                Result = true,
                Token = token
            });
        }


        private string GenerationToken(IdentityUser user) 
        { 
            
            var JwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())

                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = JwtTokenHandler.CreateToken(tokenDescriptor);

            return JwtTokenHandler.WriteToken(token);

        }


    }
}

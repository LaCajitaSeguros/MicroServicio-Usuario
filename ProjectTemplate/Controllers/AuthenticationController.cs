using Application.Auth;
using Application.Service;
using Domain.DTOs;
using Domain.Entities;
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
        private readonly IUserService userService;

        public AuthenticationController(IUserService userService)
        {
            this.userService = userService;

        }


        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto requestDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await userService.RegisterAsync(requestDto);
            return result.Result ? Ok(result) : BadRequest(result);
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto request)
        {
            var result = await userService.LoginAsync(request);
            if (result == null || !ModelState.IsValid)
            {
                return BadRequest("Contraseña o usuario invalido");
            }

            return Ok(result);
        }


        //[HttpPost("Register")]
        //public async Task<IActionResult> Registrer([FromBody] UserRegistrationRequestDto requestDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }

        //    var emailExists = await _userManager.FindByEmailAsync(requestDto.EmailAddress);
        //    if (emailExists != null)
        //    {
        //        return BadRequest(new AuthResult()
        //        {
        //            Result = false,
        //            Errors = new List<string>()
        //    {
        //        "Email already exists"
        //    }
        //        });
        //    }

        //    var user = new IdentityUser()
        //    {
        //        Email = requestDto.EmailAddress,
        //        UserName = requestDto.EmailAddress
        //    };

        //    var isCreate = await _userManager.CreateAsync(user, requestDto.Password);

        //    if (isCreate.Succeeded)
        //    {
        //        var userDto = new User
        //        {
        //            UserId = user.Id,
        //            Name = requestDto.Name,
        //            LastName = requestDto.LastName,
        //            Dni = requestDto.Dni
        //        };

        //        _dbContext.User.Add(userDto);
        //        await _dbContext.SaveChangesAsync();

        //        var token = GenerationToken(user);
        //        return Ok(new AuthResult()
        //        {
        //            Result = true,
        //            Token = token
        //        });
        //    }
        //    else
        //    {
        //        var errors = new List<string>();
        //        foreach (var err in isCreate.Errors)
        //            errors.Add(err.Description);

        //        return BadRequest(new AuthResult()
        //        {
        //            Result = false,
        //            Errors = errors
        //        });
        //    }

        ////    return BadRequest(new AuthResult()
        ////    {
        ////        Result = false,
        ////        Errors = new List<string>()
        ////{
        ////    "Unable to create user"
        ////    }
        ////        });
        //    }




    }
}

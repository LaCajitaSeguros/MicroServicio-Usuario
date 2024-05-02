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
    }
}

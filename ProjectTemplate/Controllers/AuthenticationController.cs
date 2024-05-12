using Application.Auth;
using Application.Service;
using Domain.DTOs;
using Domain.Entities;
using Infraestructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using XAct.Users;

namespace Autenticacion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : Controller
    {
        //Maneja la autenticación de usuarios
        private readonly IUserService userService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AuthenticationController(IUserService userService, UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
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

        if (result.Result)
        {
            // Obtén el usuario registrado del resultado
            IdentityUser user = await _userManager.FindByEmailAsync(requestDto.EmailAddress);
        
            // Envía el correo de verificación
            await SendVerificationEmail(user);

            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }

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

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(code))
                return BadRequest(new AuthResult
                {
                    Result = false,
                    Errors = new List<string> { "Invalid email confirmation url" }
                });

            var result = await userService.ConfirmEmailAsync(userId, code);

            if (result.Succeeded)
            {
                return Ok("Thanks Your email has been confirmed");
            }
            else
            {
                var status = "Error confirming your email";
                return BadRequest(status);
            }
       
        }

        private async Task SendVerificationEmail(IdentityUser user)
        {
            var verificationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var varificationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(verificationToken));

            //example: https://localhost:8080/api/Authentication/VerifyEmail?userId=prueba&code=prueba
            var callBackUrl = $@"{Request.Scheme}://{Request.Host}{Url.Action("ConfirmEmail", controller: "Authentication",
                               new { userId = user.Id, code = varificationCode })}";

            var emailBody = $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'" +
                $">clicking here</a>";

            await _emailSender.SendEmailAsync(user.Email, "Confirm your email", emailBody);

        }


    }
}

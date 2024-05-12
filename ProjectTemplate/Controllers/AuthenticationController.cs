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
        private readonly IUserService _userService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public AuthenticationController(IUserService userService, UserManager<IdentityUser> userManager, IEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            this._userService = userService;

        }


    [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserRegistrationRequestDto requestDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var result = await _userService.RegisterAsync(requestDto);

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
            var result = await _userService.LoginAsync(request);
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

            var result = await _userService.ConfirmEmailAsync(userId, code);

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
            var verificationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(verificationToken));
            
            // URL de confirmación de correo electrónico con el código de verificación
            var callBackUrl = $@"{Request.Scheme}://{Request.Host}{Url.Action("ConfirmEmail", controller: "Authentication",
                                    new { userId = user.Id, code = verificationCode })}";

            // URL del logotipo de Cajita Seguros
            var imageUrl = "https://www.rua-asistencia.com.py/wp-content/uploads/sites/18/2021/09/1630702216674-1080x628.jpg";

            // Cuerpo del correo electrónico con la imagen incrustada y el enlace de confirmación
            var emailBody = $@"
                <p>¡Bienvenido/a a Cajita Seguros!</p>
                <p>Por favor, confirma tu cuenta haciendo clic en el siguiente botón:</p>
                <p><a href='{HtmlEncoder.Default.Encode(callBackUrl)}'><button style='background-color: #4CAF50; /* Green */
                border: none;
                color: white;
                padding: 15px 32px;
                text-align: center;
                text-decoration: none;
                display: inline-block;
                font-size: 16px;'>Confirmar tu cuenta</button></a></p>
                <p>También puedes escanear el siguiente código QR:</p>
                <p><img src='{imageUrl}' alt='Cajita Seguros Logo'></p>
                <p>Gracias por unirte a Cajita Seguros.</p>";

            // Envía el correo electrónico de confirmación
            await _emailSender.SendEmailAsync(user.Email, "Confirmar tu cuenta en Cajita Seguros", emailBody);
        }


    }
}

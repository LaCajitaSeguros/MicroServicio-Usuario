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
                // Si el usuario no se registro por algun error no se envia el correo y retorna null
                if (user == null)
                {
                    return BadRequest(result);

                }
                 //Envía el correo de verificación
                //await SendVerificationEmail(user);

                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }

        }

        [HttpPost("VerifyRegistration")]
        public async Task<IActionResult> VerifyRegistration([FromBody] string code)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Solicitud inválida.");
            }

            var (isSuccess, errorMessage) = await userService.VerifyCodeAsync(code);

            if (isSuccess)
            {
                return Ok(new { message = "Código verificado correctamente. Registro completo." });
            }
            else
            {
                return BadRequest(new { message = errorMessage });
            }
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] UserLoginRequestDto request)
        {
            var result = await userService.LoginAsync(request);
            if (result == null || result.Token==null || !ModelState.IsValid)
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

        //private async Task SendVerificationEmail(IdentityUser user)
        //{
        //    var verificationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //    var verificationCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(verificationToken));

        //    // URL de confirmación de correo electrónico con el código de verificación
            
        //    //falta cambiar la url para que apunte al post de Codeverification


        //    var callBackUrl = $@"{Request.Scheme}://{Request.Host}{Url.Action("ConfirmEmail", controller: "Authentication",
        //                            new { userId = user.Id, code = verificationCode })}";

        //    // URL del logotipo de Cajita Seguros
        //    var imageUrl = "https://www.rua-asistencia.com.py/wp-content/uploads/sites/18/2021/09/1630702216674-1080x628.jpg";

        //    // Cuerpo del correo electrónico con la imagen incrustada y el enlace de confirmación
        //    var emailBody = $@"
        //        <p>¡Bienvenido/a a Cajita Seguros!</p>
        //        <p>Por favor, confirma tu cuenta haciendo clic en el siguiente botón:</p>
        //        <p><a href='{HtmlEncoder.Default.Encode(callBackUrl)}'><button style='background-color: #4CAF50; /* Green */
        //        border: none;
        //        color: white;
        //        padding: 15px 32px;
        //        text-align: center;
        //        text-decoration: none;
        //        display: inline-block;
        //        font-size: 16px;'>Confirmar tu cuenta</button></a></p>
        //        <p>También puedes escanear el siguiente código QR:</p>
        //        <p><img src='{imageUrl}' alt='Cajita Seguros Logo'></p>
        //        <p>Gracias por unirte a Cajita Seguros.</p>";

        //    // Envía el correo electrónico de confirmación
        //    await _emailSender.SendEmailAsync(user.Email, "Confirmar tu cuenta en Cajita Seguros", emailBody);
        //}

        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Solicitud inválida.");
            }

            var (isSuccess, errorMessage) = await userService.GenerateAndSendVerificationCodeAsync(request.EmailAddress);

            if (isSuccess)
            {
                return Ok("Se ha enviado un código de verificación a su correo electrónico.");
            }
            else
            {
                return BadRequest(errorMessage);
            }
        }

        [HttpPost("VerifyAndResetPassword")]
        public async Task<IActionResult> VerifyAndResetPassword([FromBody] ResetPasswordRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Solicitud inválida.");
            }

            var (isSuccess, errorMessage) = await userService.VerifyAndResetPasswordAsync(request.EmailAddress, request.VerificationCode, request.NewPassword);

            if (isSuccess)
            {
                return Ok("Su contraseña ha sido restablecida correctamente.");
            }
            else
            {
                return BadRequest(errorMessage);
            }
        }


        [HttpPost("CheckEmail")]
        public async Task<IActionResult> CheckEmail([FromBody] CheckEmailRequestDto requestDto)
        {
            var user = await _userManager.FindByEmailAsync(requestDto.EmailAddress);
            if (user != null)
            {
                var id = userService.GetEmail(requestDto.EmailAddress);
                return Ok(new { exists = true, userId = id.Result });
            }
            else
            {
                return Ok(new { exists = false });
            }
        }

    }

}

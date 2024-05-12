using Application.Auth;
using Domain.DTOs;
using Domain.Entities;
using Infraestructure.Repository;
using Application.Validation;
using Microsoft.AspNetCore.Identity;
using XAct;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Drawing.Text;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Web;
using System.Security.Policy;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Application.Service.ServiceImpl
{
    public class UserServiceImpl : IUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IUserRepository _userRepository;
        private readonly IValidation _validation;
        private readonly IEmailSender _emailSender;

        public UserServiceImpl(UserManager<IdentityUser> userManager, IUserRepository userRepository, IValidation validation, IEmailSender emailSender)
        {
            _userManager = userManager;
            _userRepository = userRepository;
            _validation = validation;
            _emailSender = emailSender;
        }

        public async Task<AuthResult> RegisterAsync(UserRegistrationRequestDto requestDto)
        {
            if (await _userManager.FindByEmailAsync(requestDto.EmailAddress) != null)
            {
                return new AuthResult { Result = false
                    , Errors = new List<string> { "Email already exists" } };
            }

            var user = new IdentityUser
            {
                Email = requestDto.EmailAddress,
                UserName = requestDto.EmailAddress
            };

            var result = await _userManager.CreateAsync(user, requestDto.Password);

            if (result.Succeeded)
            {
                var userDto = new User
                {
                    UserId = user.Id,
                    Name = requestDto.Name,
                    LastName = requestDto.LastName,
                    Dni = requestDto.Dni,
                    EmailAddress = requestDto.EmailAddress,
                    Password = _validation.HashPassword(requestDto.Password)
                };

                await _userRepository.AddUserAsync(userDto);

                //var token = _validation.GenerationToken(user);
                return new AuthResult { Result = true };
            }
            else
            {
                var errors = new List<string>();
                foreach (var err in result.Errors)
                    errors.Add(err.Description);

                return new AuthResult { Result = true, Errors = errors };
            }
        }

        public async Task<AuthResult?> LoginAsync(UserLoginRequestDto request)
        {
            // Verificar si el usuario con el correo electrónico existe
            var existingUser = await _userManager.FindByEmailAsync(request.EmailAddress);
            if (existingUser == null)
            {
                return null;
            }

            if (!existingUser.EmailConfirmed)
                return (new AuthResult
                {
                    Result = false,
                    Errors = new List<string> { "Email needs to be confirmed" }
                });

            // Verificar si la contraseña es correcta
            var checkUserAndPass = await _userManager.CheckPasswordAsync(existingUser, request.Password);
            if (!checkUserAndPass)
            {
                return null;
            }

            // Generar token de autenticación
            var token = _validation.GenerationToken(existingUser);
            return new AuthResult { Result = true, Token = token };
        }


        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // You might want to handle this differently, maybe return a different result or log it
                throw new InvalidOperationException($"Unable to load user with Id '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            var result = await _userManager.ConfirmEmailAsync(user, code);
            return result;
        }


        public async Task<bool> ForgotPasswordAsync(string emailAddress, string callback)
        {
            //var user = await _userManager.FindByEmailAsync(emailAddress);
            //if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            //{
            //    // Usuario no encontrado o correo electrónico no confirmado
            //    return false;
            //}
            //var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            //var callbackUrl = $"https://yourapp.com/reset-password?email={HttpUtility.UrlEncode(emailAddress)}&token={HttpUtility.UrlEncode(resetToken)}";

            ////var callBackUrl2 = $@"{Request.Scheme}://{Request.Host}{Url.Action("ConfirmEmail", controller: "Authentication",
            ////                      new { userId = user.Id, code = verificationCode })}";



            //// Construir el cuerpo del correo electrónico
            //var emailBody = $"Para restablecer su contraseña, haga clic en el siguiente enlace: <a href='{callbackUrl}'>Restablecer contraseña</a>";
            //// Enviar el correo electrónico
            //await _emailSender.SendEmailAsync(emailAddress, "Restablecer contraseña", emailBody);

            //return true;


            var user = await _userManager.FindByEmailAsync(emailAddress);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Usuario no encontrado o correo electrónico no confirmado
                return false;
            }
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetUrl = $"{callback}?email={HttpUtility.UrlEncode(emailAddress)}&token={HttpUtility.UrlEncode(resetToken)}";

            // Construir el cuerpo del correo electrónico
            var emailBody = $"Para restablecer su contraseña, haga clic en el siguiente enlace: <a href='{resetUrl}'>Restablecer contraseña</a>";
            // Enviar el correo electrónico
            await _emailSender.SendEmailAsync(emailAddress, "Restablecer contraseña", emailBody);

            return true;


        }

    }
}

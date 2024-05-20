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
        private static Dictionary<string, (string Code, DateTime Expiration)> verificationCodes = new Dictionary<string, (string Code, DateTime Expiration)>();
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
           
            if (requestDto.Password != requestDto.ConfirmPassword)
            {
                return new AuthResult
                {
                    Result = false,
                    Errors = new List<string> { "Passwords do not match" }
                };
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

        //con token

        //public async Task<bool> SendVerificationTokenAsync(string emailAddress)
        //{
        //    var user = await _userManager.FindByEmailAsync(emailAddress);
        //    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        //    {
        //        // Usuario no encontrado o correo electrónico no confirmado
        //        return false;
        //    }

        //    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        //    var emailBody = $"Su código de verificación para restablecer la contraseña es: {token}";
        //    await _emailSender.SendEmailAsync(emailAddress, "Código de Verificación", emailBody);

        //    return true;
        //}

        //public async Task<bool> VerifyAndResetPasswordAsync(string emailAddress, string token, string newPassword)
        //{
        //    var user = await _userManager.FindByEmailAsync(emailAddress);
        //    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        //    {
        //        // Usuario no encontrado o correo electrónico no confirmado
        //        return false;
        //    }

        //    var resetResult = await _userManager.ResetPasswordAsync(user, token, newPassword);
        //    if (!resetResult.Succeeded)
        //    {
        //        return false;
        //    }

        //    return true;
        //}


        //con codigo sin errores especificos
        ////public static string GenerateVerificationCode(string email)
        ////{
        ////    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        ////    var random = new Random();
        ////    string code = new string(Enumerable.Repeat(chars, 6)
        ////        .Select(s => s[random.Next(s.Length)]).ToArray());

        ////    verificationCodes[email] = (code, DateTime.Now.AddMinutes(15));

        ////    return code;
        ////}

        ////public static bool VerifyCode(string email, string code)
        ////{
        ////    if (verificationCodes.TryGetValue(email, out var value))
        ////    {
        ////        if (value.Expiration > DateTime.Now && value.Code == code)
        ////        {
        ////            verificationCodes.Remove(email);
        ////            return true;
        ////        }
        ////    }
        ////    return false;
        ////}

        ////public async Task<bool> GenerateAndSendVerificationCodeAsync(string emailAddress)
        ////{
        ////    var user = await _userManager.FindByEmailAsync(emailAddress);
        ////    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        ////    {
        ////        return false;
        ////    }

        ////    var code = GenerateVerificationCode(emailAddress);

        ////    if (!string.IsNullOrEmpty(code))
        ////    {
        ////        var emailBody = $"Su código de verificación para restablecer la contraseña es: {code}";
        ////        await _emailSender.SendEmailAsync(emailAddress, "Código de Verificación", emailBody);

        ////        return true;
        ////    }
        ////    else
        ////    {
        ////        return false;
        ////    }
        ////}

        ////public async Task<bool> VerifyAndResetPasswordAsync(string emailAddress, string code, string newPassword)
        ////{
        ////    var user = await _userManager.FindByEmailAsync(emailAddress);
        ////    if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
        ////    {
        ////        return false;
        ////    }

        ////    if (!VerifyCode(emailAddress, code))
        ////    {
        ////        return false; 
        ////    }

        ////    var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        ////    var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
        ////    if (!resetResult.Succeeded)
        ////    {
        ////        return false;
        ////    }

        ////    return true;
        ////}
        ///
        public static string GenerateVerificationCode(string email)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            string code = new string(Enumerable.Repeat(chars, 6)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            verificationCodes[email] = (code, DateTime.Now.AddMinutes(15));

            return code;
        }

        public static bool VerifyCode(string email, string code)
        {
            if (verificationCodes.TryGetValue(email, out var value))
            {
                if (value.Expiration > DateTime.Now && value.Code == code)
                {
                    verificationCodes.Remove(email);
                    return true;
                }
            }
            return false;
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> GenerateAndSendVerificationCodeAsync(string emailAddress)
        {
            var user = await _userManager.FindByEmailAsync(emailAddress);
            if (user == null)
            {
                return (false, "El correo electrónico no está registrado.");
            }
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return (false, "El correo electrónico no está confirmado.");
            }

            var code = GenerateVerificationCode(emailAddress);

            if (!string.IsNullOrEmpty(code))
            {
                var emailBody = $"Su código de verificación para restablecer la contraseña es: {code}";
                await _emailSender.SendEmailAsync(emailAddress, "Código de Verificación", emailBody);

                return (true,"");
            }
            else
            {
                return (false, "No se pudo generar el código de verificación.");
            }
        }

        public async Task<(bool IsSuccess, string ErrorMessage)> VerifyAndResetPasswordAsync(string emailAddress, string code, string newPassword)
        {
            var user = await _userManager.FindByEmailAsync(emailAddress);
            if (user == null)
            {
                return (false, "El correo electrónico no está registrado.");
            }
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return (false, "El correo electrónico no está confirmado.");
            }

            if (!VerifyCode(emailAddress, code))
            {
                return (false, "El código de verificación es incorrecto o ha expirado.");
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, newPassword);
            if (!resetResult.Succeeded)
            {
                return (false, "No se pudo restablecer la contraseña. Por favor, asegúrese de que la nueva contraseña cumpla con los requisitos.");
            }

            return (true, "");
        }
    }

}

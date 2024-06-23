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
using Autenticacion.Controllers;

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


            var verificationCode = GenerateVerificationCode(requestDto.EmailAddress);

            var emailBody = $@"
            <p>Estimado/a,</p>
            <p>¡Bienvenido/a a La Cajita Seguros!</p>
            <p>Nos complace tenerlo/a con nosotros. Su código de verificación para el registro es: <strong>{verificationCode}</strong></p>
            <p>Por favor, ingrese este código en la página de registro para completarlo.</p>
            <p>Si tiene alguna pregunta o necesita asistencia, no dude en contactarnos.</p>
            <p>Saludos cordiales,<br/>El equipo de La Cajita Seguros</p>";

            await _emailSender.SendEmailAsync(requestDto.EmailAddress, "Bienvenido a CajitaSeguros - Código de Verificación", emailBody);



            var verifyCodeRequestDto = new VerifyCodeRequestDto
            {
                EmailAddress = requestDto.EmailAddress,
                Code = verificationCode,
                Password = requestDto.Password,
                Name = requestDto.Name,
                LastName = requestDto.LastName,
                Dni = requestDto.Dni
            };

            var verifyResult = await VerifyCodeAndCompleteRegistrationAsync(verifyCodeRequestDto);

            if (!verifyResult.Result)
            {
                return new AuthResult { Result = false, Errors = verifyResult.Errors };
            }

            return new AuthResult { Result = true, Message = "Verification code sent to email." 
                , UserId=verifyResult.UserId,Name=verifyResult.Name,LastName=verifyResult.LastName};




        }


        public async Task<AuthResult> VerifyCodeAndCompleteRegistrationAsync(VerifyCodeRequestDto verifyCodeRequestDto)
        
        {


            if (!VerifyCode(verifyCodeRequestDto.EmailAddress, verifyCodeRequestDto.Code))
            {
                return new AuthResult
                {
                    Result = false,
                    Errors = new List<string> { "Invalid verification code or it has expired." }
                };
            }

            var user = new IdentityUser
            {
                Email = verifyCodeRequestDto.EmailAddress,
                UserName = verifyCodeRequestDto.EmailAddress
            };

            var result = await _userManager.CreateAsync(user, verifyCodeRequestDto.Password);

            if (result.Succeeded)
            {
                var userDto = new User
                {
                    UserId = user.Id,
                    Name = verifyCodeRequestDto.Name,
                    LastName = verifyCodeRequestDto.LastName,
                    Dni = verifyCodeRequestDto.Dni,
                    EmailAddress = verifyCodeRequestDto.EmailAddress,
                    Password = _validation.HashPassword(verifyCodeRequestDto.Password),
                    Code = verifyCodeRequestDto.Code
                };

                await _userRepository.AddUserAsync(userDto);

                return new AuthResult { Result = true, UserId = userDto.UserId,Name=userDto.Name,LastName=userDto.LastName};
            }
            else
            {
                var errors = new List<string>();
                foreach (var err in result.Errors)
                    errors.Add(err.Description);

                return new AuthResult { Result = false, Errors = errors };
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
                    Errors = new List<string> { "Se debe confirmar el email" }
                });

            // Verificar si la contraseña es correcta
            var checkUserAndPass = await _userManager.CheckPasswordAsync(existingUser, request.Password);
            if (!checkUserAndPass)
            {
                return null;
            }

            // Generar token de autenticación
            var token = _validation.GenerationToken(existingUser);
            var id = await _userRepository.GetByMailAsync(request.EmailAddress);
            var userName = await _userRepository.GetUserNameAsync(id);
            return new AuthResult { Result = true, Token = token, UserId = id, Name = userName };
        }

        public async Task<string> GetEmail(string email)
        {
            var id = await _userRepository.GetByMailAsync(email);
            return id;
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



        public async Task<(bool isSuccess, string? errorMessage)> VerifyCodeAsync(string code)
        {
            var verificationEntry = await _userRepository.GetByCodeAsync(code);
            
            if (verificationEntry == null)
            {
                return (false, "El código es incorrecto o ha expirado.");
            }
            // Marca el usuario como verificado, si tienes una propiedad para eso.
            var user = await _userManager.FindByEmailAsync(verificationEntry.EmailAddress);
            if (user != null)
            {
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }

            return (true, null);
        }
    }

}

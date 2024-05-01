using Application.Auth;
using Domain.DTOs;
using Domain.Entities;
using Infraestructure.Repository;
using Application.Validation;
using Microsoft.AspNetCore.Identity;
using XAct;

namespace Application.Service.ServiceImpl
{
    public class UserServiceImpl : IUserService
    {
            private readonly UserManager<IdentityUser> _userManager;
            private readonly IUserRepository _userRepository;
            private readonly IValidation _validation;

        public UserServiceImpl(UserManager<IdentityUser> userManager, IUserRepository userRepository, IValidation validation)
            {
                _userManager = userManager;
                _userRepository = userRepository;
                _validation = validation;

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

                    var token = _validation.GenerationToken(user);
                    return new AuthResult { Result = true, Token = token };
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

    }
}

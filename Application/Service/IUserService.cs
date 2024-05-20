using Application.Auth;
using Domain.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public interface IUserService
    {

        Task<AuthResult> RegisterAsync(UserRegistrationRequestDto requestDto);
        Task<AuthResult?> LoginAsync(UserLoginRequestDto request);
        Task<IdentityResult> ConfirmEmailAsync(string userId, string code);
        //token
        //Task<bool> VerifyAndResetPasswordAsync(string emailAddress, string token, string newPassword);
        //Task<bool> SendVerificationTokenAsync(string emailAddress);
        //codigo
        //Task<bool> GenerateAndSendVerificationCodeAsync(string emailAddress);
        //Task<bool> VerifyAndResetPasswordAsync(string emailAddress, string code, string newPassword);

        Task<(bool IsSuccess, string ErrorMessage)> GenerateAndSendVerificationCodeAsync(string emailAddress);
        Task<(bool IsSuccess, string ErrorMessage)> VerifyAndResetPasswordAsync(string emailAddress, string code, string newPassword);



    }
}

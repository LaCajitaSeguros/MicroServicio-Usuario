﻿using Application.Auth;
using Domain.DTOs;
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
    }
}

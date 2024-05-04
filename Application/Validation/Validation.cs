using Autenticacion.Configuration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using XSystem.Security.Cryptography;

namespace Application.Validation
{
    public interface IValidation
    {

        string GenerationToken(IdentityUser user);
        string HashPassword(string password);


    }
    public class Validation : IValidation
    {

        private readonly JwtConfig _jwtConfig;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailSender _emailSender;

        public Validation(IOptions<JwtConfig> jwtConfig, UserManager<IdentityUser> userManager,
            IEmailSender email)
        {
            _jwtConfig = jwtConfig.Value;
            _userManager = userManager;
            _emailSender = email;

        }

        public string GenerationToken(IdentityUser user)
        {

            var JwtTokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(_jwtConfig.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", user.Id),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTime.Now.ToUniversalTime().ToString())

                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = JwtTokenHandler.CreateToken(tokenDescriptor);

            return JwtTokenHandler.WriteToken(token);

        }
        public string HashPassword(string password)
        {
            using (var algorithm = new SHA256Managed())
            {
                var hashBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
                var hashedPassword = Convert.ToBase64String(hashBytes);
                return hashedPassword;
            }

        }
    }
}

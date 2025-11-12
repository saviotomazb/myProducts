using Microsoft.IdentityModel.Tokens;
using myProducts.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net.NetworkInformation;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Serilog;
using Log = Serilog.Log;

namespace myProducts.Services
{
    public static class TokenService
    {

        //Gera um JWT contendo as informações do usuário (claims) e assinado com chave simétrica (HMAC-SHA256).
        public static string GenerateJwtToken(IConfiguration configuration, string username, int userId, string fullName)
        {
            var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key não foi configurada.");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("FullName", fullName)
            };

            var token = new JwtSecurityToken(
                issuer: configuration["Jwt:Issuer"],
                audience: configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
                );

            Log.ForContext("SourceContext", "myProducts.Services.TokenService").Information("Gerando JWT para usuário {UserId} ({Username})", userId, username);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //Gera um Refresh Token criptograficamente seguro (64 bytes aleatórios codificados em Base64).
        public static string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            Log.ForContext("SourceContext", "myProducts.Services.TokenService").Information("Gerando refresh token.");
            return Convert.ToBase64String(randomBytes);
        }

        // Calcula o hash SHA-256 de um refresh token para armazenamento seguro no banco.
        // (Evita salvar o refresh token em texto puro, seguindo boas práticas de segurança).
        public static byte[] ComputeHash(string token)
        {
            using var sha256 = SHA256.Create();
            return sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
        }
    }
}
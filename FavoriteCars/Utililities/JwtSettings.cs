using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FavoriteCars.Utililities
{
    public class JwtSettings
    {
        public string Key { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }

        public string GenerateJwt(string username)
        {
            // Retrieve the secret key from the JWT settings.
            var secretKey = Key;

            // Convert the secret key from string to a symmetric security key.
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            // Create signing credentials using the symmetric security key and HMAC SHA256 algorithm.
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define the claims (or payload) for the JWT.
            // The 'sub' claim denotes the subject (typically user id or username).
            // The 'jti' claim provides a unique identifier for the token.
            var claims = new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Create a JWT with specified issuer, audience, claims, expiration time, and signing credentials.
            var token = new JwtSecurityToken(
                issuer: Issuer,
                audience: Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Set the token to expire in one hour.
                signingCredentials: credentials
            );

            // Instantiate a JWT token handler.
            var tokenHandler = new JwtSecurityTokenHandler();

            // Serialize the JWT into a string format.
            var serializedToken = tokenHandler.WriteToken(token);
            return serializedToken;
        }

        public string? GetUsername(string token)
        {
            // Instantiate a JWT token handler.
            var tokenHandler = new JwtSecurityTokenHandler();

            if (!tokenHandler.CanReadToken(token))
            {
                return null;
            }

            var jwtToken = tokenHandler.ReadJwtToken(token);

            var subClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub);

            return subClaim?.Value;
        }
    }
}

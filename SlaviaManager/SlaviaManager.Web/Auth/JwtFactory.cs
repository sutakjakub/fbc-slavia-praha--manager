using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SlaviaManager.Web.Data;
using SlaviaManager.Web.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SlaviaManager.Web.Auth
{
    public class JwtFactory : IJwtFactory
    {
        private readonly JwtIssuerOptions _jwtOptions;
        private readonly UserManager<AppUserEntity> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public JwtFactory(IOptions<JwtIssuerOptions> jwtOptions, UserManager<AppUserEntity> userManager, RoleManager<IdentityRole> roleManager)
        {
            _jwtOptions = jwtOptions.Value;
            ThrowIfInvalidOptions(_jwtOptions);
            this._userManager = userManager;
            this._roleManager = roleManager;
        }

        public async Task<string> GenerateEncodedToken(string userName, ClaimsIdentity identity)
        {
            var claims = new List<Claim>();
            //basic claims
            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, userName));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, await _jwtOptions.JtiGenerator()));
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(_jwtOptions.IssuedAt).ToString(), ClaimValueTypes.Integer64));
            claims.Add(identity.FindFirst(JwtConstants.Strings.JwtClaimIdentifiers.Id));

            var user = await _userManager.FindByNameAsync(userName);
            var userClaims = await _userManager.GetClaimsAsync(user);
            var userRoles = await _userManager.GetRolesAsync(user);
            claims.AddRange(userClaims);
            foreach (var userRole in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, userRole));
                var role = await _roleManager.FindByNameAsync(userRole);
                if (role != null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (Claim roleClaim in roleClaims)
                    {
                        claims.Add(roleClaim);
                    }
                }
            }

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                notBefore: _jwtOptions.NotBefore,
                expires: _jwtOptions.Expiration,
                signingCredentials: _jwtOptions.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            return encodedJwt;
        }

        public async Task<ClaimsIdentity> GenerateClaimsIdentity(string userName, string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var roles = await _userManager.GetRolesAsync(user);

            List<Claim> claimsResult = new List<Claim>();
            claimsResult.Add(new Claim(JwtConstants.Strings.JwtClaimIdentifiers.Id, id));
            //claimsResult.Add(new Claim(JwtConstants.Strings.JwtClaimIdentifiers.Rol, JwtConstants.Strings.JwtClaims.ApiAccess));

            IList<Claim> claims;
            foreach (var role in roles)
            {
                if (!claimsResult.Any(p => p.Value == role))
                {
                    claims = await _roleManager.GetClaimsAsync(new IdentityRole(role));
                    foreach (var claim in claims)
                    {
                        if (!claimsResult.Any(p => p.Value == claim.Type))
                        {
                            claimsResult.Add(new Claim(JwtConstants.Strings.JwtClaimIdentifiers.Rol, claim.Type));
                        }
                    }

                    //claimsResult.Add(new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", role));
                    claimsResult.Add(new Claim(JwtConstants.Strings.JwtClaimIdentifiers.Rol, role));
                }
            }

            return new ClaimsIdentity(new GenericIdentity(userName, "Token"), claimsResult);
        }

        /// <returns>Date converted to seconds since Unix epoch (Jan 1, 1970, midnight UTC).</returns>
        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);

        private static void ThrowIfInvalidOptions(JwtIssuerOptions options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));

            if (options.ValidFor <= TimeSpan.Zero)
            {
                throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(JwtIssuerOptions.ValidFor));
            }

            if (options.SigningCredentials == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.SigningCredentials));
            }

            if (options.JtiGenerator == null)
            {
                throw new ArgumentNullException(nameof(JwtIssuerOptions.JtiGenerator));
            }
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SlaviaManager.Web.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SlaviaManager.Web.Data
{
    public static class SeedData
    {
        public static List<string> Roles { get { return new List<string>() { "Administrator", "Management", "Coach", "Player", "Parent" }; } }

        public static List<AppUserEntity> Users
        {
            get
            {
                return new List<AppUserEntity>()
                {
                    new AppUserEntity() {
                        Email = "administrator@slaviamanager.cz",
                        FirstName = "Administrator name",
                        LastName = "Administrator last name",
                        Gender = "Male",
                        UserName = "administrator"
                    },
                    new AppUserEntity() {
                        Email = "coach@slaviamanager.cz",
                        FirstName = "Coach name",
                        LastName = "Coach last name",
                        Gender = "Male",
                        UserName = "coach"
                    },
                    new AppUserEntity() {
                        Email = "management@slaviamanager.cz",
                        FirstName = "Management name",
                        LastName = "Management last name",
                        Gender = "Male",
                        UserName = "management"
                    },
                    new AppUserEntity() {
                        Email = "player@slaviamanager.cz",
                        FirstName = "Player name",
                        LastName = "Player last name",
                        Gender = "Male",
                        UserName = "player"
                    },
                    new AppUserEntity() {
                        Email = "parent@slaviamanager.cz",
                        FirstName = "Parent name",
                        LastName = "Parent last name",
                        Gender = "Male",
                        UserName = "parent"
                    },
                };
            }
        }

        public static List<KeyValuePair<string, string>> ClaimsToRole
        {
            get
            {
                var list = new List<KeyValuePair<string, string>>();

                list.Add(new KeyValuePair<string, string>("EditUserPermissions", "Administrator"));
                list.Add(new KeyValuePair<string, string>("ReadOnlyUserPermissions", "Administrator"));
                list.Add(new KeyValuePair<string, string>("ReadOnlyUserPermissions", "Management"));
                list.Add(new KeyValuePair<string, string>("AcceptNewUser", "Management"));
                list.Add(new KeyValuePair<string, string>("AcceptNewUser", "Administrator"));

                return list;
            }
        }


        public static async Task AddRoles(RoleManager<IdentityRole> roleManager)
        {
            if (roleManager == null) throw new ArgumentNullException(nameof(roleManager));

            if (roleManager.Roles.Count() == Roles.Count) return;
            foreach (var role in Roles)
            {
                await AddRole(roleManager, role);
            }
        }

        public static async Task AddRole(RoleManager<IdentityRole> roleManager, string role)
        {
            if (roleManager == null) throw new ArgumentNullException(nameof(roleManager));
            if (string.IsNullOrWhiteSpace(role)) throw new ArgumentNullException(nameof(role));

            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        public static async Task AddUsers(RoleManager<IdentityRole> roleManager, UserManager<AppUserEntity> userManager)
        {
            if (userManager == null) throw new ArgumentNullException(nameof(userManager));
            if (roleManager == null) throw new ArgumentNullException(nameof(roleManager));

            if (userManager.Users.Count() == Users.Count) return;
            foreach (var user in Users)
            {
                await AddUser(roleManager, userManager, user);
            }
        }

        public static async Task AddUser(RoleManager<IdentityRole> roleManager, UserManager<AppUserEntity> userManager, AppUserEntity user)
        {
            if (roleManager == null) throw new ArgumentNullException(nameof(roleManager));
            if (userManager == null) throw new ArgumentNullException(nameof(userManager));
            if (user == null) throw new ArgumentNullException(nameof(user));

            if (!userManager.Users.Any(p => p.UserName == user.UserName))
            {
                await userManager.CreateAsync(user, "password");
                if (await roleManager.RoleExistsAsync(user.UserName))
                {
                    await userManager.AddToRoleAsync(user, user.UserName);
                }
            }
        }

        public static async Task AddClaims(RoleManager<IdentityRole> roleManager)
        {
            if (roleManager == null) throw new ArgumentNullException(nameof(roleManager));

            foreach (var claim in ClaimsToRole)
            {
                await AddClaim(roleManager, claim);
            }
        }

        public static async Task AddClaim(RoleManager<IdentityRole> roleManager, KeyValuePair<string, string> claimToRole)
        {
            if (roleManager == null) throw new ArgumentNullException(nameof(roleManager));

            var identityRole = await roleManager.FindByNameAsync(claimToRole.Value);
            if (identityRole != null)
            {
                var claims = await roleManager.GetClaimsAsync(identityRole);
                if (!claims.Any(p => p.Value == claimToRole.Key))
                {
                    await roleManager.AddClaimAsync(identityRole, new Claim(claimToRole.Key, ""));
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SlaviaManager.Web.Auth
{
    public static class CustomClaims
    {
        public const string EditUserPermissions = "EditUserPermissions";
        public const string ReadOnlyUserPermissions = "ReadOnlyUserPermissions";
        public const string AcceptNewUser = "AcceptNewUser";
    }
}

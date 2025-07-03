using Enums.User;
using System.Data;

namespace Forms.Services
{
    public class AuthServices
    {
        private IHttpContextAccessor _contextAccessor;

        public const string AUTH_TYPE_KEY = "AuthType";
        public const string CLAIM_TYPE_ID = "Id";
        public const string CLAIM_TYPE_NAME = "Name";
        public const string CLAIM_TYPE_ROLE = "Role";

        public AuthServices(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public bool IsAuthenticated()
        {
            return GetId() is not null;
        }
        
        public int? GetId()
        {
            var idstr = GetClaimValue(CLAIM_TYPE_ID);
            if(idstr == null)
            {
                return null;
            }

            return int.Parse(idstr);
        }

        public string? GetName()
        {
            var namestr = GetClaimValue(CLAIM_TYPE_NAME);
            if (namestr == null)
            {
                return null;
            }

            return namestr;
        }

        public Role GetRole()
        {
            var rolestr = GetClaimValue(CLAIM_TYPE_ROLE);
            if (rolestr == null)
            {
                throw new Exception("Guest cant hhas role");
            }
            var roleint = int.Parse(rolestr);
            var role = (Role)roleint;

            return role;
        }

        public bool IsAdmin()
        {
            return GetRole() == Role.Admin;
        }

        private string? GetClaimValue(string type)
        {
            return _contextAccessor.HttpContext!.User.Claims.FirstOrDefault(x => x.Type == type)?.Value;
        }
    }
}

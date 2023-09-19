using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using TrackingBugs.Models;

namespace TrackingBugs.Extensions
{
    public class BTUserClaimsPrincipleFactory : UserClaimsPrincipalFactory<BTUser, IdentityRole>

    {
        public BTUserClaimsPrincipleFactory(UserManager<BTUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IOptions<IdentityOptions> optionsAccessor) : 
            base(userManager, roleManager, optionsAccessor)
        {
        }
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BTUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            return identity;
        }
    }
}

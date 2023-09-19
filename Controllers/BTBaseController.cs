using Microsoft.AspNetCore.Mvc;
using TrackingBugs.Extensions;

namespace TrackingBugs.Controllers
{
    [Controller]
    public abstract class BTBaseController : Controller
    {
        protected int _companyId => User.Identity!.GetCompanyId();
    }
}

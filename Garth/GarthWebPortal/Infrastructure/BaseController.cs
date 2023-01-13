using Microsoft.AspNetCore.Mvc;

namespace GarthWebPortal
{
    [Route("[controller]/[action]", Name = "[controller]_[action]")]
    public abstract class BaseController : Controller
    {
    }
}

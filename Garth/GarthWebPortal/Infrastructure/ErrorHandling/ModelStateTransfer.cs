using Microsoft.AspNetCore.Mvc.Filters;

namespace GarthWebPortal.ErrorHandling
{
    public abstract class ModelStateTransfer : ActionFilterAttribute
    {
        protected const string Key = nameof(ModelStateTransfer);
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TrainingSystem.MVC.Controllers
{
    public class BaseController : Controller
    {
        protected int? UserId => HttpContext.Session.GetInt32("UserId");
        protected int? RoleId => HttpContext.Session.GetInt32("RoleId");

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (UserId == null)
            {
                context.Result = RedirectToAction("Login", "Account");
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
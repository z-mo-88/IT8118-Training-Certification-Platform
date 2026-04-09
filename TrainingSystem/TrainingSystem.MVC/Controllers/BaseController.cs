using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TrainingSystem.MVC.Controllers
{
    public class BaseController : Controller
    {
        protected int? UserId => HttpContext.Session.GetInt32("UserId");
        protected int? RoleId => HttpContext.Session.GetInt32("RoleId");

        protected bool IsTrainee => RoleId == 1;
        protected bool IsInstructor => RoleId == 2;
        protected bool IsCoordinator => RoleId == 3;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            // Check if logged in
            if (UserId == null)
            {
                context.Result = RedirectToAction("Login", "Account");
                return;
            }

            base.OnActionExecuting(context);
        }

        protected IActionResult AuthorizeRole(params int[] roles)
        {
            if (RoleId == null || !roles.Contains(RoleId.Value))
            {
                return RedirectToAction("Login", "Account");
            }

            return null;
        }
    }
}
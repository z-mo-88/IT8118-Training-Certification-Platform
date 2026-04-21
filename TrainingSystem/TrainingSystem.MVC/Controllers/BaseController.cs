using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace TrainingSystem.MVC.Controllers
{
    public class BaseController : Controller
    {
        protected int? UserId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        protected int? RoleId =>
            int.TryParse(User.FindFirstValue(ClaimTypes.Role), out var role) ? role : null;

        protected bool IsTrainee => RoleId == 1;
        protected bool IsInstructor => RoleId == 2;
        protected bool IsCoordinator => RoleId == 3;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var controller = context.RouteData.Values["controller"]?.ToString();

            
            if (controller == "Account")
            {
                base.OnActionExecuting(context);
                return;
            }

            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                context.Result = RedirectToAction("Login", "Account");
                return;
            }

            base.OnActionExecuting(context);
        }

        protected IActionResult? AuthorizeRole(params int[] roles)
        {
            if (RoleId == null || !roles.Contains(RoleId.Value))
            {
                return RedirectToAction("Login", "Account");
            }

            return null;
        }
    }
}
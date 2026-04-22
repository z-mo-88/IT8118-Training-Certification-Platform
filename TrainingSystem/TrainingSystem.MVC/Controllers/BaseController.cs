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
            var path = context.HttpContext.Request.Path.Value;

            if (path.StartsWith("/Account/Login"))
            {
                base.OnActionExecuting(context);
                return;
            }

            //  RESTORE SESSION FROM COOKIE
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                var userIdCookie = Request.Cookies["UserId"];
                var roleIdCookie = Request.Cookies["RoleId"];
                var userNameCookie = Request.Cookies["UserName"];

                if (userIdCookie != null && roleIdCookie != null)
                {
                    HttpContext.Session.SetInt32("UserId", int.Parse(userIdCookie));
                    HttpContext.Session.SetInt32("RoleId", int.Parse(roleIdCookie));

                    if (userNameCookie != null)
                        HttpContext.Session.SetString("UserName", userNameCookie);
                }
            }

            if (HttpContext.Session.GetInt32("UserId") == null)
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
                return RedirectToAction("AccessDenied", "Account");
            }

            return null;
        }
    }
}
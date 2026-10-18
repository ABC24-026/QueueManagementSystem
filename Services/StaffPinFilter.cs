using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QueueManagementSystem.Services
{
    public class StaffPinFilter : IActionFilter
    {
        private readonly string _pin;

        public StaffPinFilter(IConfiguration config)
        {
            _pin = config["Staff:Pin"] ?? "1234";
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            string supplied = context.HttpContext.Request.Headers["X-Staff-Pin"].ToString();

            if (supplied != _pin)
            {
                context.Result = new UnauthorizedObjectResult(new { error = "Invalid staff PIN." });
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // nothing needed here
        }
    }
}

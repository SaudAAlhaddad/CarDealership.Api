using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CarDealership.Api.Filters;

// This filter runs before controller actions
// Its job is to clean up user input automatically 
// trims extra spaces from all string fields
// converts email to lowercase
public class TrimAndNormalizeFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is null) continue;

            var props = arg.GetType().GetProperties()
                .Where(p => p.CanRead && p.CanWrite && p.PropertyType == typeof(string));

            foreach (var p in props)
            {
                var val = (string?)p.GetValue(arg);
                if (val is null) continue;

                var trimmed = val.Trim();

                // Normalize known fields
                if (string.Equals(p.Name, "Email", StringComparison.OrdinalIgnoreCase))
                    trimmed = trimmed.ToLowerInvariant();

                p.SetValue(arg, trimmed);
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}

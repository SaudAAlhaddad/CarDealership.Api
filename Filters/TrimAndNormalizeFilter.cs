using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CarDealership.Api.Filters;

/// <summary>
/// Global action filter that automatically sanitizes user input
/// - Trims whitespace from all string properties
/// - Converts email fields to lowercase for consistency
/// Runs before controller actions execute
/// </summary>
public class TrimAndNormalizeFilter : IActionFilter
{
    /// <summary>
    /// Executes before controller action - sanitizes input data
    /// </summary>
    public void OnActionExecuting(ActionExecutingContext context)
    {
        // Process all action arguments (DTOs, parameters, etc.)
        foreach (var actionArgument in context.ActionArguments.Values)
        {
            if (actionArgument is null) continue;

            // Find all string properties that can be read and written
            var stringProperties = actionArgument.GetType().GetProperties()
                .Where(property => property.CanRead && property.CanWrite && property.PropertyType == typeof(string));

            foreach (var stringProperty in stringProperties)
            {
                var currentValue = (string?)stringProperty.GetValue(actionArgument);
                if (currentValue is null) continue;

                // Trim whitespace from string values
                var sanitizedValue = currentValue.Trim();

                // Normalize email fields to lowercase for consistency
                if (string.Equals(stringProperty.Name, "Email", StringComparison.OrdinalIgnoreCase))
                    sanitizedValue = sanitizedValue.ToLowerInvariant();

                stringProperty.SetValue(actionArgument, sanitizedValue);
            }
        }
    }

    /// <summary>
    /// Executes after controller action - no cleanup needed
    /// </summary>
    public void OnActionExecuted(ActionExecutedContext context) { }
}

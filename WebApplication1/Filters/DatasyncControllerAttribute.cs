using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NetTopologySuite.IO.Converters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace WebApplication1.Filters;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class DatasyncControllerAttribute : ResultFilterAttribute
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        AllowTrailingCommas = true,
        Converters =
            {
                new JsonStringEnumConverter(),
                new GeoJsonConverterFactory()
                // In real version, we also have special converters for DateTimeOffset, DateTime, and TimeOnly
            },
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
        IgnoreReadOnlyFields = true,
        IgnoreReadOnlyProperties = false,
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public override void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult result)
        {
            context.Result = new JsonResult(result.Value, SerializerOptions) { StatusCode = result.StatusCode };
        }
        base.OnResultExecuting(context);
    }
}

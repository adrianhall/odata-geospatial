using Microsoft.AspNetCore.OData;
using WebApplication1.Models;
using WebApplication1.TestData;

#pragma warning disable RCS1021 // Convert lambda expression body to expression body

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Adds an IList<Country> to the service container.
builder.Services
    .AddSingleton<IList<Country>>(CountryData.GetCountries().ToList());

// Adds the basic MVC services, including the OData capabilities.
// Note that we DON'T add route components for OData - it's encoded in the
// table controller.
builder.Services
    .AddControllers()
    .AddOData(options =>
    {
        options.Count().Filter().OrderBy().Expand().SetMaxTop(1000).Select();
    });

WebApplication app = builder.Build();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program
{
}

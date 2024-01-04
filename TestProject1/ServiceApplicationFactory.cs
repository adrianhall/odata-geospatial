using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using WebApplication1.Models;

namespace TestProject1;

[ExcludeFromCodeCoverage]
public class ServiceApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
    }

    internal Country? GetCountryById(string id)
    {
        using IServiceScope scope = Services.CreateScope();
        IList<Country> countries = scope.ServiceProvider.GetRequiredService<IList<Country>>();
        return countries.FirstOrDefault(c => c.Id == id);
    }
}

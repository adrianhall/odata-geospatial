using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Collections.Concurrent;

namespace WebApplication1.Models;

internal static class ModelCache
{
    private static readonly Lazy<ConcurrentDictionary<Type, IEdmModel>> cache = new(() => new ConcurrentDictionary<Type, IEdmModel>());

    internal static IEdmModel GetEdmModel(Type type)
    {
        if (!cache.Value.TryGetValue(type, out IEdmModel? model))
        {
            model = BuildEdmModel(type);
            cache.Value.TryAdd(type, model);
        }
        return model;
    }

    internal static IEdmModel BuildEdmModel(Type type)
    {
        var modelBuilder = new ODataConventionModelBuilder();
        modelBuilder.EnableLowerCamelCase();
        modelBuilder.AddEntityType(type);
        return modelBuilder.GetEdmModel();
    }
}

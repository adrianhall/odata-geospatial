using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.Spatial;
using NetTopologySuite.Geometries;
using System.Collections.Concurrent;
using System.Xml.Linq;

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

        if (type == typeof(Country))
        {
            var country = modelBuilder.StructuralTypes.First(t => t.ClrType == typeof(Country));
            country.AddProperty(typeof(Country).GetProperty("EdmLocation")).Name = "Location"; // make sure the edm property name same as the NetTopology property name
        }

        return modelBuilder.GetEdmModel();
    }
}

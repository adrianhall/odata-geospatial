using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using Microsoft.Spatial;
using NetTopologySuite.Geometries;
using NetTopologySuite;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.OData.ModelBuilder.Capabilities.V1;

namespace WebApplication1.Models;

internal class MyFilterBinder : FilterBinder
{
    internal const string GeoDistanceFunctionName = "geo.distance";

    private static readonly MethodInfo distanceMethodDb = typeof(NetTopologySuite.Geometries.Geometry).GetMethod("Distance");

    public override Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        switch (node.Name)
        {
            case GeoDistanceFunctionName:
                return BindGeoDistance(node, context);

            default:
                return base.BindSingleValueFunctionCallNode(node, context);
        }
    }

    public Expression BindGeoDistance(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        Expression[] arguments = BindArguments(node.Parameters, context);

     //   GetPointExpressions(arguments, null, out MemberExpression memberExpression, out ConstantExpression constantExpression);
        var ex = Expression.Call(arguments[0], distanceMethodDb, arguments[1]);

        return ex;
    }

    public override Expression BindPropertyAccessQueryNode(SingleValuePropertyAccessNode propertyAccessNode, QueryBinderContext context)
    {
        if (string.Equals(propertyAccessNode.Property.Name, "location", StringComparison.OrdinalIgnoreCase))
        {
            Expression source = context.CurrentParameter;

            return Expression.Property(source, "Location");
        }

        return base.BindPropertyAccessQueryNode(propertyAccessNode, context);
    }

    public override Expression BindConvertNode(ConvertNode convertNode, QueryBinderContext context)
    {
        if (convertNode.Source.Kind == QueryNodeKind.SingleValuePropertyAccess)
        {
            return BindPropertyAccessQueryNode((SingleValuePropertyAccessNode)convertNode.Source, context);
        }

        return base.BindConvertNode(convertNode, context);
    }

    public override Expression BindConstantNode(ConstantNode constantNode, QueryBinderContext context)
    {
        if (constantNode.TypeReference.FullName() == "Edm.GeographyPoint" && constantNode.Value is GeographyPoint geoPoint)
        {
            Point point = CreatePoint(geoPoint.Latitude, geoPoint.Longitude);
            return Expression.Constant(point);
        }

        return base.BindConstantNode(constantNode, context);
    }

    private static Point CreatePoint(double latitude, double longitude)
    {
        // 4326 is most common coordinate system used by GPS/Maps
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

        // see https://docs.microsoft.com/en-us/ef/core/modeling/spatial
        // Longitude and Latitude
        var newLocation = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

        return newLocation;
    }
}

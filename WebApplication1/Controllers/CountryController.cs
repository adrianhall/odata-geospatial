using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Query.Wrapper;
using Microsoft.OData;
using Microsoft.OData.Edm;
using WebApplication1.Filters;
using WebApplication1.Models;

namespace WebApplication1.Controllers;

[Route("/api/country")]
[DatasyncController]
public class CountryController : ControllerBase
{
    public CountryController(IList<Country> countries)
    {
        Queryable = countries.AsQueryable();
        EdmModel = ModelCache.GetEdmModel(typeof(Country));
    }

    IQueryable<Country> Queryable { get; }

    IEdmModel EdmModel { get; }

    // In the real table controller, this is an async method - so async away if you want!
    [HttpGet]
    public IActionResult Query()
    {
        IQueryable<Country> dataset = Queryable; // Retrieved from a repository normally.

        ODataValidationSettings validationSettings = new() { MaxTop = 100 };    // Customer can set MaxTop on a per-table basis.
        ODataQuerySettings querySettings = new() { PageSize = 10, EnsureStableOrdering = true }; // Customer can set PageSize on a per-table basis.
        ODataQueryContext queryContext = new(EdmModel, typeof(Country), new Microsoft.OData.UriParser.ODataPath());
        ODataQueryOptions<Country> queryOptions = new(queryContext, Request);

        try
        {
            queryOptions.Validate(validationSettings);
        }
        catch (ODataException validationException)
        {
            return BadRequest(validationException.Message);
        }

        // For the results in PagedResult, we need three bits of information:
        //  #1: The results for this page.
        //  #2: If $count=true, the total number of results.
        //  #3: The next page link, if there is one.  It isn't generated if there are no more results (so we need the total number of results anyhow)

        // #1: The results
        IEnumerable<object> results = (IEnumerable<object>)queryOptions.ApplyTo(dataset, querySettings);
        int resultCount = results.Count();
        PagedResult pagedResult = results is IEnumerable<ISelectExpandWrapper> wrapper ? new(wrapper.Select(x => x.ToDictionary())) : new(results);

        // #2: The total number of results.  Would love to get both values from one query, but I don't know how.
        IQueryable<Country> query = (IQueryable<Country>?)queryOptions.Filter?.ApplyTo(dataset, new ODataQuerySettings()) ?? dataset;
        int totalCount = query.Count();
        if (queryOptions.Count?.Value == true)
        {
            pagedResult.Count = totalCount;
        }

        // #3: The next page link, which is based on the current query values +/- the result count, bounded by the total count.
        int skip = (queryOptions.Skip?.Value ?? 0) + resultCount;
        if (queryOptions.Top != null)
        {
            int top = queryOptions.Top.Value - resultCount;
            pagedResult.NextLink = skip >= totalCount || top <= 0 ? null : CreateNextLink(skip, top);
        }
        else
        {
            pagedResult.NextLink = skip >= totalCount ? null : CreateNextLink(skip);
        }

        return Ok(pagedResult);
    }

    private string CreateNextLink(int skip = 0, int top = 0)
    {
        var builder = new UriBuilder(Request.GetDisplayUrl());
        List<string> query = string.IsNullOrEmpty(builder.Query) ? new() : builder.Query.TrimStart('?').Split('&').Where(q => !q.StartsWith("$skip=") && !q.StartsWith("$top=")).ToList();
        if (skip > 0)
        {
            query.Add($"$skip={skip}");
        }
        if (top > 0)
        {
            query.Add($"$top={top}");
        }
        return string.Join('&', query).TrimStart('&');
    }
}

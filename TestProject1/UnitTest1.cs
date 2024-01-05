using FluentAssertions;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using NetTopologySuite.IO.Converters;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApplication1.Models;
using Xunit.Abstractions;

#nullable disable

namespace TestProject1;

[ExcludeFromCodeCoverage]
public class UnitTest1 : IClassFixture<ServiceApplicationFactory>
{
    private readonly ServiceApplicationFactory _factory;
    private readonly HttpClient _client;
    private const string _baseUrl = "/api/country";

    // Copy of the serializer optons from DatasyncControllerAttribute.cs
    private static readonly JsonSerializerOptions serializerOptions = new(JsonSerializerDefaults.Web)
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

    private readonly ITestOutputHelper _output;

    public UnitTest1(ServiceApplicationFactory factory, ITestOutputHelper output)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _output = output;
    }

    [Fact]
    public async Task Test1()
    {
        // A basic test to see if the service is running and OData queries generally work.
        HttpResponseMessage response = await _client.GetAsync($"{_baseUrl}?$top=1&$count=true");
        string content = await response.Content.ReadAsStringAsync();
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        Page<Country> page = JsonSerializer.Deserialize<Page<Country>>(content, serializerOptions);

        page.Should().NotBeNull();
        page.Count.Should().Be(250);
        page.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Test2()
    {
        // This is a GeoJSON test for equality of the OData query results.
        HttpResponseMessage response = await _client.GetAsync($"{_baseUrl}?$filter=location eq geography'POINT(-97 38)'");
        string content = await response.Content.ReadAsStringAsync();
        _output.WriteLine(content);

        // Put a breakpoint here and inspect the content.  Content will contain the error message thrown by the OData validation.
        response.Should().HaveStatusCode(HttpStatusCode.OK);
        Page<Country> page = JsonSerializer.Deserialize<Page<Country>>(content, serializerOptions);

        page.Should().NotBeNull();
        page.Items.Should().HaveCount(1);
        page.Items[0].IsoCode.Should().Be("US");
    }

    /// <summary>
    /// The deserialized content from a paging operation.
    /// </summary>
    /// <typeparam name="T">The type of entity being transmitted</typeparam>
    public class Page<T> where T : class
    {
        public T[] Items { get; set; }
        public long? Count { get; set; }
        public string NextLink { get; set; }
    }
}
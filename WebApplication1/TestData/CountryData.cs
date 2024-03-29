﻿using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebApplication1.Models;

#nullable disable

namespace WebApplication1.TestData;

public static class CountryData
{
    /// <summary>
    /// Retrieve the country data form the embedded resource.
    /// </summary>
    /// <returns>A list of <see cref="Country"/> objects for the dataset.</returns>
    public static IEnumerable<Country> GetCountries()
    {
        Assembly asm = Assembly.GetExecutingAssembly();
        using Stream s = asm.GetManifestResourceStream($"{asm.GetName().Name}.TestData.countries.json");
        using StreamReader sr = new(s);
        string json = sr.ReadToEnd().Replace("\r\n", "\n").TrimEnd();
        return JsonSerializer.Deserialize<List<CountryJsonData>>(json).Select(c => new Country(c));
    }
}

public class CountryJsonData
{
    [JsonPropertyName("name")] public CountryNameJsonData Name { get; set; }
    [JsonPropertyName("tld")] public IEnumerable<string> TopLevelDomains { get; set; }
    [JsonPropertyName("cca2")] public string IsoCode { get; set; }
    [JsonPropertyName("ccn3")] public string NumericCode { get; set; }
    [JsonPropertyName("cca3")] public string Iso3Code { get; set; }
    [JsonPropertyName("cioc")] public string OlympicCode { get; set; }
    [JsonPropertyName("independent")] public bool Independent { get; set; }
    [JsonPropertyName("status")] public string Status { get; set; }
    [JsonPropertyName("unMember")] public bool UnMember { get; set; }
    [JsonPropertyName("currencies")] public IDictionary<string, CurrencyJsonData> Currencies { get; set; }
    [JsonPropertyName("idd")] public IddJsonData Idd { get; set; }
    [JsonPropertyName("capital")] public IEnumerable<string> Capital { get; set; }
    [JsonPropertyName("altSpellings")] public IEnumerable<string> AlternateSpellings { get; set; }
    [JsonPropertyName("region")] public string Region { get; set; }
    [JsonPropertyName("subregion")] public string Subregion { get; set; }
    [JsonPropertyName("languages")] public IDictionary<string, string> Languages { get; set; }
    [JsonPropertyName("translations")] public IDictionary<string, TranslationJsonData> Translations { get; set; }
    [JsonPropertyName("latlng")] public IEnumerable<double> LatitudeLongitude { get; set; }
    [JsonPropertyName("landlocked")] public bool Landlocked { get; set; }
    [JsonPropertyName("borders")] public IEnumerable<string> Borders { get; set; }
    [JsonPropertyName("area")] public double Area { get; set; }
    [JsonPropertyName("flag")] public string Flag { get; set; }
    [JsonPropertyName("demonyms")] public IDictionary<string, DemonymsJsonData> Demonyms { get; set; }
    [JsonPropertyName("callingCodes")] public IEnumerable<string> CallingCodes { get; set; }

    public class OfficialCommonJsonData
    {
        [JsonPropertyName("official")] public string Official { get; set; }
        [JsonPropertyName("common")] public string Common { get; set; }
    }

    public class CountryNameJsonData : OfficialCommonJsonData
    {
        [JsonPropertyName("native")] public IDictionary<string, OfficialCommonJsonData> Native { get; set; }
    }

    public class CurrencyJsonData
    {
        [JsonPropertyName("name")] public string Name { get; set; }
        [JsonPropertyName("symbol")] public string Symbol { get; set; }
    }

    public class IddJsonData
    {
        [JsonPropertyName("root")] public string Root { get; set; }
        [JsonPropertyName("suffixes")] public IEnumerable<string> Suffixes { get; set; }
    }

    public class TranslationJsonData : OfficialCommonJsonData
    {
    }

    public class DemonymsJsonData
    {
        [JsonPropertyName("f")] public string Feminine { get; set; }
        [JsonPropertyName("m")] public string Masculine { get; set; }
    }
}

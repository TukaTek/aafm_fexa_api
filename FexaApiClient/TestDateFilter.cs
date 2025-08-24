using System;
using Fexa.ApiClient.Models;

class TestDateFilter
{
    static void Main()
    {
        var visitParams = new VisitQueryParameters
        {
            Start = 0,
            Limit = 20,
            ScheduledDateFrom = new DateTime(2025, 8, 14),
            ScheduledDateTo = new DateTime(2025, 8, 15)
        };
        
        var queryDict = visitParams.ToDictionary();
        var queryString = string.Join("&", queryDict.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"));
        
        Console.WriteLine("Generated query string for date range filter:");
        Console.WriteLine($"/api/ev1/visits?{queryString}");
        Console.WriteLine();
        Console.WriteLine("Query parameters:");
        foreach(var kvp in queryDict)
        {
            Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
        }
    }
}
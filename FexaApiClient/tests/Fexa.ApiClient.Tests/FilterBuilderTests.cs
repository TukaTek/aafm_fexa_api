using Xunit;
using FluentAssertions;
using Fexa.ApiClient.Models;
using System.Text.Json;

namespace Fexa.ApiClient.Tests;

public class FilterBuilderTests
{
    [Fact]
    public void Where_SingleFilter_CreatesCorrectJson()
    {
        // Arrange & Act
        var filters = FilterBuilder.Create()
            .Where("workorders.id", 1)
            .Build();
        
        var json = FilterBuilder.Create()
            .Where("workorders.id", 1)
            .ToJson();
        
        // Assert
        filters.Should().HaveCount(1);
        filters[0].Property.Should().Be("workorders.id");
        filters[0].Value.Should().Be(1);
        filters[0].Operator.Should().BeNull();
        
        json.Should().Be("[{\"property\":\"workorders.id\",\"value\":1}]");
    }
    
    [Fact]
    public void Where_MultipleFilters_CreatesCorrectJson()
    {
        // Arrange & Act
        var json = FilterBuilder.Create()
            .Where("workorders.id", 1)
            .Where("vendors.id", 25)
            .ToJson();
        
        // Assert
        json.Should().Be("[{\"property\":\"workorders.id\",\"value\":1},{\"property\":\"vendors.id\",\"value\":25}]");
    }
    
    [Fact]
    public void WhereIn_CreatesCorrectJsonWithOperator()
    {
        // Arrange & Act
        var json = FilterBuilder.Create()
            .WhereIn("workorders.id", 116, 117)
            .ToJson();
        
        // Assert
        json.Should().Be("[{\"property\":\"workorders.id\",\"value\":[116,117],\"operator\":\"in\"}]");
    }
    
    [Fact]
    public void WhereNotIn_CreatesCorrectJsonWithOperator()
    {
        // Arrange & Act
        var filters = FilterBuilder.Create()
            .WhereNotIn("status", "cancelled", "rejected")
            .Build();
        
        // Assert
        filters.Should().HaveCount(1);
        filters[0].Property.Should().Be("status");
        filters[0].Operator.Should().Be("not in");
        (filters[0].Value as object[]).Should().BeEquivalentTo(new[] { "cancelled", "rejected" });
    }
    
    [Fact]
    public void WhereBetween_CreatesCorrectJsonWithOperator()
    {
        // Arrange & Act
        var json = FilterBuilder.Create()
            .WhereBetween("amount", 100, 500)
            .ToJson();
        
        // Assert
        json.Should().Be("[{\"property\":\"amount\",\"value\":[100,500],\"operator\":\"between\"}]");
    }
    
    [Fact]
    public void WhereDateBetween_FormatsDatesProperly()
    {
        // Arrange
        var startDate = new DateTime(2023, 1, 1);
        var endDate = new DateTime(2023, 12, 31);
        
        // Act
        var json = FilterBuilder.Create()
            .WhereDateBetween("created_at", startDate, endDate)
            .ToJson();
        
        // Assert
        json.Should().Be("[{\"property\":\"created_at\",\"value\":[\"2023-01-01\",\"2023-12-31\"],\"operator\":\"between\"}]");
    }
    
    [Fact]
    public void ToUrlEncoded_EncodesJsonProperly()
    {
        // Arrange & Act
        var encoded = FilterBuilder.Create()
            .Where("workorders.id", 1)
            .ToUrlEncoded();
        
        // Assert
        encoded.Should().Contain("%5b"); // [
        encoded.Should().Contain("%7b"); // {
        encoded.Should().Contain("%22property%22"); // "property"
        encoded.Should().Contain("%22workorders.id%22"); // "workorders.id"
    }
    
    [Fact]
    public void ConvenienceMethods_WorkCorrectly()
    {
        // Arrange & Act
        var builder = FilterBuilder.Create()
            .WhereWorkOrderId(123)
            .WhereVendorIds(1, 2, 3);
        
        var filters = builder.Build();
        
        // Assert
        filters.Should().HaveCount(2);
        filters[0].Property.Should().Be("workorders.id");
        filters[0].Value.Should().Be(123);
        filters[1].Property.Should().Be("vendors.id");
        filters[1].Operator.Should().Be("in");
    }
}
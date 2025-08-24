using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Fexa.ApiClient.Services;
using Fexa.ApiClient.Models;
using Fexa.ApiClient.Exceptions;

namespace Fexa.ApiClient.Tests;

public class UserServiceTests
{
    private readonly Mock<IFexaApiService> _mockApiService;
    private readonly Mock<ILogger<UserService>> _mockLogger;
    private readonly UserService _userService;
    
    public UserServiceTests()
    {
        _mockApiService = new Mock<IFexaApiService>();
        _mockLogger = new Mock<ILogger<UserService>>();
        _userService = new UserService(_mockApiService.Object, _mockLogger.Object);
    }
    
    [Fact]
    public async Task GetUserAsync_WithValidUserId_ReturnsUser()
    {
        // Arrange
        var userId = "123";
        var expectedUser = new User
        {
            Id = userId,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };
        
        var response = new BaseResponse<User>
        {
            Success = true,
            Data = expectedUser
        };
        
        _mockApiService
            .Setup(x => x.GetAsync<BaseResponse<User>>(
                It.Is<string>(s => s == $"/api/users/{userId}"), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        // Act
        var result = await _userService.GetUserAsync(userId);
        
        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(userId);
        result.Email.Should().Be(expectedUser.Email);
        _mockApiService.Verify(x => x.GetAsync<BaseResponse<User>>(
            It.Is<string>(s => s == $"/api/users/{userId}"), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetUserAsync_WithEmptyUserId_ThrowsArgumentException()
    {
        // Arrange
        var userId = "";
        
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _userService.GetUserAsync(userId));
    }
    
    [Fact]
    public async Task CreateUserAsync_WithValidRequest_ReturnsCreatedUser()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Email = "newuser@example.com",
            FirstName = "New",
            LastName = "User"
        };
        
        var expectedUser = new User
        {
            Id = "456",
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName
        };
        
        var response = new BaseResponse<User>
        {
            Success = true,
            Data = expectedUser
        };
        
        _mockApiService
            .Setup(x => x.PostAsync<BaseResponse<User>>(
                It.Is<string>(s => s == "/api/users"), 
                It.IsAny<CreateUserRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        // Act
        var result = await _userService.CreateUserAsync(request);
        
        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(request.Email);
        result.FirstName.Should().Be(request.FirstName);
        result.LastName.Should().Be(request.LastName);
    }
    
    [Fact]
    public async Task DeleteUserAsync_WithValidUserId_CallsApiService()
    {
        // Arrange
        var userId = "789";
        var response = new BaseResponse<object>
        {
            Success = true
        };
        
        _mockApiService
            .Setup(x => x.DeleteAsync<BaseResponse<object>>(
                It.Is<string>(s => s == $"/api/users/{userId}"), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        // Act
        await _userService.DeleteUserAsync(userId);
        
        // Assert
        _mockApiService.Verify(x => x.DeleteAsync<BaseResponse<object>>(
            It.Is<string>(s => s == $"/api/users/{userId}"), 
            It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task GetUsersAsync_WithPagination_ReturnsPagedResponse()
    {
        // Arrange
        var parameters = new QueryParameters
        {
            Page = 1,
            PageSize = 10
        };
        
        var expectedResponse = new PagedResponse<User>
        {
            Success = true,
            Data = new List<User>
            {
                new User { Id = "1", Email = "user1@example.com" },
                new User { Id = "2", Email = "user2@example.com" }
            },
            Page = 1,
            PageSize = 10,
            TotalCount = 2,
            TotalPages = 1
        };
        
        _mockApiService
            .Setup(x => x.GetAsync<PagedResponse<User>>(
                It.Is<string>(s => s.Contains("start=0") && s.Contains("limit=10")), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);
        
        // Act
        var result = await _userService.GetUsersAsync(parameters);
        
        // Assert
        result.Should().NotBeNull();
        result.Data.Should().HaveCount(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
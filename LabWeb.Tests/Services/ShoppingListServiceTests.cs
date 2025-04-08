using System.Linq.Expressions;
using FluentAssertions;
using LabWeb.Models.Entities;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services;
using Moq;

namespace LabWeb.Tests.Services;

public class ShoppingListServiceTests
{
    private readonly Mock<IShoppingListRepository> _mockRepository;
    private readonly ShoppingListService _shoppingListService;

    public ShoppingListServiceTests()
    {
        _mockRepository = new Mock<IShoppingListRepository>();
        _shoppingListService = new ShoppingListService(_mockRepository.Object);
    }

    [Fact]
    public void GetShoppingListByUserId_ShouldReturnList_WhenDataExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var shoppingListEntities = new List<ShoppingList>
        {
            new ShoppingList { Id = Guid.NewGuid(), Name = "Groceries", UserId = userId },
            new ShoppingList { Id = Guid.NewGuid(), Name = "Electronics", UserId = userId }
        };

        _mockRepository.Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<ShoppingList, bool>>>(), null, false))
            .Returns(shoppingListEntities.AsQueryable());

        // Act
        var result = _shoppingListService.GetShoppingListByUserId(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(x => x.Name == "Groceries");
        result.Should().Contain(x => x.Name == "Electronics");
    }

    [Fact]
    public void GetShoppingListByUserId_ShouldReturnEmptyList_WhenNoDataExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<ShoppingList, bool>>>(), null, false))
            .Returns(new List<ShoppingList>().AsQueryable());

        // Act
        var result = _shoppingListService.GetShoppingListByUserId(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public void GetShoppingListByUserId_ShouldThrowException_WhenRepositoryFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _mockRepository.Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<ShoppingList, bool>>>(), null, false))
            .Throws(new Exception("Database error"));

        // Act & Assert
        Func<Task> act = async () => _shoppingListService.GetShoppingListByUserId(userId);
        act.Should().ThrowAsync<Exception>().WithMessage("Database error");
    }

    [Theory]
    [InlineData("11111111-1111-1111-1111-111111111111")]
    [InlineData("22222222-2222-2222-2222-222222222222")]
    [InlineData("33333333-3333-3333-3333-333333333333")]
    public void GetShoppingListByUserId_ShouldReturnCorrectData_ForVariousUserIds(Guid userId)
    {
        // Arrange
        var shoppingListEntities = new List<ShoppingList>
        {
            new ShoppingList { Id = Guid.NewGuid(), Name = "Groceries", UserId = userId },
            new ShoppingList { Id = Guid.NewGuid(), Name = "Clothing", UserId = userId }
        };

        // Convert List to IQueryable
        _mockRepository.Setup(repo => repo.GetWhere(It.IsAny<Expression<Func<ShoppingList, bool>>>(), null, false))
            .Returns(shoppingListEntities.AsQueryable());

        // Act
        var result = _shoppingListService.GetShoppingListByUserId(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2); 
        result.Should().Contain(x => x.Name == "Groceries");
        result.Should().Contain(x => x.Name == "Clothing");
    }
}
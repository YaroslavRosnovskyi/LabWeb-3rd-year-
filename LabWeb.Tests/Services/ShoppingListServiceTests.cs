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

    [Fact]
    public async Task FindByIdAsync_ShouldReturnDifferentResultsOnConsecutiveCalls()
    {
        // Arrange
        var id = Guid.NewGuid();
        var firstEntity = new ShoppingList { Id = id, Name = "FirstList" };
        var secondEntity = new ShoppingList { Id = id, Name = "SecondList" };

        _mockRepository.SetupSequence(x => x.GetByIdAsync(id))
            .ReturnsAsync(firstEntity)
            .ReturnsAsync(secondEntity)
            .ReturnsAsync((ShoppingList?)null);

        // Act
        var result1 = await _shoppingListService.FindByIdAsync(id);
        var result2 = await _shoppingListService.FindByIdAsync(id);
        var result3 = await _shoppingListService.FindByIdAsync(id);

        // Assert
        result1.Should().NotBeNull();
        result1!.Name.Should().Be("FirstList");

        result2.Should().NotBeNull();
        result2!.Name.Should().Be("SecondList");

        result3.Should().BeNull();

        _mockRepository.Verify(x => x.GetByIdAsync(id), Times.Exactly(3));
    }

    [Fact]
    public void ShoppingListComplexMockScenarios_Test()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();

        var mockSequence = new MockSequence();

        _mockRepository.Setup(repo => repo.GetWhere(
                It.Is<Expression<Func<ShoppingList, bool>>>(expr =>
                    ExpressionContainsValue(expr, userId1)), null, false))
            .Returns(new List<ShoppingList>
            {
                new ShoppingList { Id = Guid.NewGuid(), Name = "User1List", UserId = userId1 }
            }.AsQueryable());

        _mockRepository.Setup(repo => repo.GetWhere(
                It.Is<Expression<Func<ShoppingList, bool>>>(expr =>
                    ExpressionContainsValue(expr, userId2)), null, false))
            .Throws(new InvalidOperationException("User is blocked"));

        var setupSequence = _mockRepository.SetupSequence(repo => repo.GetWhere(
            It.Is<Expression<Func<ShoppingList, bool>>>(expr =>
                ExpressionContainsValue(expr, userId3)), null, false));

        setupSequence.Returns(new List<ShoppingList>().AsQueryable());
        setupSequence.Returns(new List<ShoppingList>
        {
            new ShoppingList { Id = Guid.NewGuid(), Name = "FirstCall", UserId = userId3 }
        }.AsQueryable());
        setupSequence.Returns(new List<ShoppingList>
        {
            new ShoppingList { Id = Guid.NewGuid(), Name = "SecondCall", UserId = userId3 },
            new ShoppingList { Id = Guid.NewGuid(), Name = "ThirdCall", UserId = userId3 }
        }.AsQueryable());

        _mockRepository.Setup(repo => repo.SaveChangesAsync())
            .Returns(Task.CompletedTask)
            .Verifiable();

        _mockRepository.Setup(repo => repo.Post(It.IsAny<ShoppingList>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result1 = _shoppingListService.GetShoppingListByUserId(userId1);
        result1.Should().HaveCount(1);
        result1.First().Name.Should().Be("User1List");

        Func<Task> act = async () => _shoppingListService.GetShoppingListByUserId(userId2);
        act.Should().ThrowAsync<InvalidOperationException>().WithMessage("User is blocked");

        var result3First = _shoppingListService.GetShoppingListByUserId(userId3);
        result3First.Should().BeEmpty();

        var result3Second = _shoppingListService.GetShoppingListByUserId(userId3);
        result3Second.Should().HaveCount(1);
        result3Second.First().Name.Should().Be("FirstCall");
        var result3Third = _shoppingListService.GetShoppingListByUserId(userId3);
        result3Third.Should().HaveCount(2);
        result3Third.Should().Contain(x => x.Name == "SecondCall");
        result3Third.Should().Contain(x => x.Name == "ThirdCall");

        _mockRepository.Verify(repo => repo.GetWhere(It.IsAny<Expression<Func<ShoppingList, bool>>>(), null, false), Times.Exactly(5));

        _mockRepository.VerifyAll();
    }

    private bool ExpressionContainsValue(Expression<Func<ShoppingList, bool>> expression, Guid value)
    {
        return expression.ToString().Contains(value.ToString());
    }
}
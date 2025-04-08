using FluentAssertions;
using LabWeb.DTOs.ItemDTO;
using LabWeb.Models.Entities;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services;
using MockQueryable;
using Moq;

namespace LabWeb.Tests.Services
{
    public class ItemServiceTests
    {
        private readonly Mock<IItemRepository> _repoMock;
        private readonly ItemService _service;

        public ItemServiceTests()
        {
            _repoMock = new Mock<IItemRepository>();
            _service = new ItemService(_repoMock.Object);
        }

        public List<Item> GetSampleItems()
        {
            return new List<Item>
            {
                new Item
                {
                    Id = Guid.NewGuid(),
                    Name = "Item1",
                    Quantity = 10,
                    Notes = "Description1",
                    Price = 10.99m,
                    ShoppingListId = Guid.NewGuid(),
                    ItemCategoryId = Guid.NewGuid()
                },
                new Item
                {
                    Id = Guid.NewGuid(),
                    Name = "Item2",
                    Quantity = 5,
                    Notes = "Description2",
                    Price = 20.99m,
                    ShoppingListId = Guid.NewGuid(),
                    ItemCategoryId = Guid.NewGuid()
                }
            };
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllMappedEntities()
        {
            // Arrange
            var entities = GetSampleItems().BuildMock();
            _repoMock.Setup(r => r.GetAll(null)).Returns(entities.AsQueryable());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllBeOfType<ItemResponse>();
            result.Select(x => x.Name).Should().Contain(new[] { "Item1", "Item2" });
            result.Select(x => x.Quantity).Should().Contain(new[] { 10, 5 });
            result.Select(x => x.Price).Should().Contain(new[] { 10.99m, 20.99m });
        }

        [Fact]
        public async Task FindByIdAsync_ShouldReturnCorrectEntity()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new Item
            {
                Id = id,
                Name = "Item1",
                Quantity = 10,
                Notes = "Description1",
                Price = 10.99m,
                ShoppingListId = Guid.NewGuid(),
                ItemCategoryId = Guid.NewGuid()
            };
            _repoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entity);

            // Act
            var result = await _service.FindByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Name.Should().Be("Item1");
            result.Quantity.Should().Be(10);
            result.Price.Should().Be(10.99m);
            result.Notes.Should().Be("Description1");
        }

        [Fact]
        public async Task Insert_ShouldAddAndReturnEntity()
        {
            // Arrange
            var request = new ItemRequest
            {
                Name = "NewItem",
                Quantity = 10,
                Notes = "New Description",
                Price = 15.99m,
                ShoppingListId = Guid.NewGuid(),
                ItemCategoryId = Guid.NewGuid()
            };
            _repoMock.Setup(x => x.Post(It.IsAny<Item>())).Returns(Task.CompletedTask);
            _repoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.Insert(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("NewItem");
            result.Quantity.Should().Be(10);
            result.Price.Should().Be(15.99m);
            result.Notes.Should().Be("New Description");
        }

        [Fact]
        public async Task Update_ShouldUpdateAndReturnEntity()
        {
            // Arrange
            var entityDto = new ItemResponse
            {
                Id = Guid.NewGuid(),
                Name = "UpdatedItem",
                Quantity = 15,
                Notes = "Updated Description",
                Price = 25.99m,
                ShoppingListId = Guid.NewGuid(),
                ItemCategoryId = Guid.NewGuid()
            };
            var entity = new Item
            {
                Id = entityDto.Id,
                Name = entityDto.Name,
                Quantity = entityDto.Quantity,
                Notes = entityDto.Notes,
                Price = entityDto.Price,
                ShoppingListId = entityDto.ShoppingListId,
                ItemCategoryId = entityDto.ItemCategoryId
            };

            _repoMock.Setup(x => x.Update(It.IsAny<Item>()));
            _repoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.Update(entityDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("UpdatedItem");
            result.Quantity.Should().Be(15);
            result.Price.Should().Be(25.99m);
            result.Notes.Should().Be("Updated Description"); 
            _repoMock.Verify(x => x.Update(It.Is<Item>(e => e.Name == "UpdatedItem" && e.Quantity == 15)), Times.Once); 
            _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once); 
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteAndSave()
        {
            // Arrange
            var entityDto = new ItemResponse { Id = Guid.NewGuid(), Name = "ItemToDelete" };
            _repoMock.Setup(x => x.Delete(It.IsAny<Item>()));
            _repoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var act = async () => await _service.DeleteAsync(entityDto);

            // Assert
            await act.Should().NotThrowAsync();
            _repoMock.Verify(x => x.Delete(It.IsAny<Item>()), Times.Once);
            _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var entityDto = new ItemResponse { Id = Guid.NewGuid(), Name = "InvalidItem" };
            var entity = new Item { Id = entityDto.Id, Name = entityDto.Name };
            _repoMock.Setup(x => x.Update(It.IsAny<Item>()));
            _repoMock.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception("Save failed"));

            // Act & Assert
            var act = async () => await _service.Update(entityDto);
            await act.Should().ThrowAsync<Exception>().WithMessage("Save failed");
        }
    }
}

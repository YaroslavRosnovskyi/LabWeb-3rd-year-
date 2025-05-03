using FluentAssertions;
using LabWeb.DTOs.ItemCategoryDTO;
using LabWeb.Models.Entities;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services;
using MockQueryable;
using Moq;

namespace LabWeb.Tests.Services
{
    public class ItemCategoryServiceTests
    {
        private readonly Mock<IItemCategoryRepository> _repoMock;
        private readonly ItemCategoryService _service;

        public ItemCategoryServiceTests()
        {
            _repoMock = new Mock<IItemCategoryRepository>();
            _service = new ItemCategoryService(_repoMock.Object);
        }

        public List<ItemCategory> GetSampleItemCategories()
        {
            return new List<ItemCategory>
            {
                new ItemCategory
                {
                    Id = Guid.NewGuid(),
                    Name = "Category1"
                },
                new ItemCategory
                {
                    Id = Guid.NewGuid(),
                    Name = "Category2"
                }
            };
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllMappedEntities()
        {
            // Arrange
            var entities = GetSampleItemCategories().BuildMock();
            _repoMock.Setup(r => r.GetAll(null)).Returns(entities.AsQueryable());

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllBeOfType<ItemCategoryResponse>();
            result.Select(x => x.Name).Should().Contain(new[] { "Category1", "Category2" });
            result.Should().OnlyContain(x => !string.IsNullOrEmpty(x.Name));
        }

        [Fact]
        public async Task FindByIdAsync_ShouldReturnCorrectEntity()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new ItemCategory
            {
                Id = id,
                Name = "Category1"
            };
            _repoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entity);

            // Act
            var result = await _service.FindByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Name.Should().Be("Category1");
        }

        [Fact]
        public async Task Insert_ShouldAddAndReturnEntity()
        {
            // Arrange
            var request = new ItemCategoryRequest
            {
                Name = "NewCategory"
            };
            _repoMock.Setup(x => x.Post(It.IsAny<ItemCategory>())).Returns(Task.CompletedTask);
            _repoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.Insert(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("NewCategory");
        }

        [Fact]
        public async Task Update_ShouldUpdateAndReturnEntity()
        {
            // Arrange
            var entityDto = new ItemCategoryResponse
            {
                Id = Guid.NewGuid(),
                Name = "UpdatedCategory"
            };
            var entity = new ItemCategory
            {
                Id = entityDto.Id,
                Name = entityDto.Name
            };

            _repoMock.Setup(x => x.Update(It.IsAny<ItemCategory>()));
            _repoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.Update(entityDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("UpdatedCategory");
            _repoMock.Verify(x => x.Update(It.Is<ItemCategory>(e => e.Name == "UpdatedCategory")), Times.Once);
            _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteAndSave()
        {
            // Arrange
            var entityDto = new ItemCategoryResponse { Id = Guid.NewGuid(), Name = "CategoryToDelete" };
            _repoMock.Setup(x => x.Delete(It.IsAny<ItemCategory>()));
            _repoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var act = async () => await _service.DeleteAsync(entityDto);

            // Assert
            await act.Should().NotThrowAsync();
            _repoMock.Verify(x => x.Delete(It.IsAny<ItemCategory>()), Times.Once);
            _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldThrowException_WhenRepositoryFails()
        {
            // Arrange
            var entityDto = new ItemCategoryResponse { Id = Guid.NewGuid(), Name = "InvalidCategory" };
            var entity = new ItemCategory { Id = entityDto.Id, Name = entityDto.Name };
            _repoMock.Setup(x => x.Update(It.IsAny<ItemCategory>()));
            _repoMock.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception("Save failed"));

            // Act & Assert
            var act = async () => await _service.Update(entityDto);
            await act.Should().ThrowAsync<Exception>().WithMessage("Save failed");
        }

        [Fact]
        public async Task FindByIdAsync_ShouldReturnDifferentResultsOnConsecutiveCalls()
        {
            // Arrange
            var id = Guid.NewGuid();
            var firstEntity = new ItemCategory { Id = id, Name = "FirstCategory" };
            var secondEntity = new ItemCategory { Id = id, Name = "SecondCategory" };

            _repoMock.SetupSequence(x => x.GetByIdAsync(id))
                .ReturnsAsync(firstEntity)
                .ReturnsAsync(secondEntity)
                .ReturnsAsync((ItemCategory?)null);

            // Act
            var result1 = await _service.FindByIdAsync(id);
            var result2 = await _service.FindByIdAsync(id);
            var result3 = await _service.FindByIdAsync(id);

            // Assert
            result1.Should().NotBeNull();
            result1!.Name.Should().Be("FirstCategory");

            result2.Should().NotBeNull();
            result2!.Name.Should().Be("SecondCategory");

            result3.Should().BeNull();

            _repoMock.Verify(x => x.GetByIdAsync(id), Times.Exactly(3));
        }
    }
}

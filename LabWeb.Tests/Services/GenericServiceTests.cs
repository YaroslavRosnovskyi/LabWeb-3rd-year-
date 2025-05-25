using FluentAssertions;
using LabWeb.DTOs.Interfaces;
using LabWeb.Models;
using LabWeb.Repositories.Interfaces;
using LabWeb.Services;
using MockQueryable;
using Moq;

namespace LabWeb.Tests.Services
{
    public class FakeEntity : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
    }

    public class FakeRequest : IRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class FakeResponse : IResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class FakeService : GenericService<FakeEntity, FakeRequest, FakeResponse>
    {
        public FakeService(IGenericRepository<FakeEntity> repo) : base(repo) { }
    }

    public class GenericServiceTests
    {
        private readonly Mock<IGenericRepository<FakeEntity>> _repoMock;
        private readonly FakeService _service;

        public GenericServiceTests()
        {
            _repoMock = new Mock<IGenericRepository<FakeEntity>>();
            _service = new FakeService(_repoMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllMappedEntities()
        {
            // Arrange
            var entities = new List<FakeEntity>
            {
                new FakeEntity { Id = Guid.NewGuid(), Name = "Test1" },
                new FakeEntity { Id = Guid.NewGuid(), Name = "Test2" }
            };

            var mockQueryable = entities.AsQueryable().BuildMock();

            _repoMock.Setup(x => x.GetAll(null))
                .Returns(mockQueryable);

            // Act
            var result = await _service.GetAllAsync();

            // Assert
            result.Should().HaveCount(2);
            result.Should().AllBeOfType<FakeResponse>();
            result.Select(x => x.Name).Should().Contain(new[] { "Test1", "Test2" });
            result.Should().OnlyContain(x => x.Id != Guid.Empty);
        }

        [Fact]
        public async Task GetAllPaginatedAsync_ShouldReturnCorrectPagination()
        {
            // Arrange
            var entities = new List<FakeEntity> {
                new FakeEntity { Id = Guid.NewGuid(), Name = "Paginated" },
            };
            _repoMock
                .Setup(r => r.GetAllPaginated(0, 1, null))
                .ReturnsAsync(entities);


            // Act
            var result = await _service.GetAllPaginatedAsync(0, 1);

            // Assert
            result.Should().NotBeNull();
            result.Entities.Should().HaveCount(1);
            result.Entities.First().Name.Should().Be("Paginated");
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task FindByIdAsync_ShouldReturnCorrectEntity()
        {
            // Arrange
            var id = Guid.NewGuid();
            var entity = new FakeEntity { Id = id, Name = "Found" };

            _repoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(entity);

            // Act
            var result = await _service.FindByIdAsync(id);

            // Assert
            result.Should().NotBeNull();
            result!.Id.Should().Be(id);
            result.Name.Should().Be("Found");
        }

        [Fact]
        public async Task Insert_ShouldAddAndReturnEntity()
        {
            // Arrange
            var request = new FakeRequest { Name = "New" };
            _repoMock.Setup(x => x.Post(It.IsAny<FakeEntity>())).Returns(Task.CompletedTask);
            _repoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.Insert(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("New");
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallDeleteAndSave()
        {
            // Arrange
            var response = new FakeResponse { Id = Guid.NewGuid(), Name = "ToDelete" };

            _repoMock.Setup(x => x.Delete(It.IsAny<FakeEntity>()));
            _repoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var act = async () => await _service.DeleteAsync(response);

            // Assert
            await act.Should().NotThrowAsync();
            _repoMock.Verify(x => x.Delete(It.IsAny<FakeEntity>()), Times.Once);
            _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("invalid")]
        public async Task FindByIdAsync_ShouldReturnNull_WhenEntityNotFound(string? name)
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((FakeEntity?)null);

            // Act
            var result = await _service.FindByIdAsync(id);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Update_ShouldCallRepositoryUpdateAndReturnUpdatedEntity()
        {
            // Arrange
            var entityDto = new FakeResponse { Id = Guid.NewGuid(), Name = "UpdatedName" };
            var entity = new FakeEntity { Id = entityDto.Id, Name = entityDto.Name };

            _repoMock.Setup(x => x.Update(It.IsAny<FakeEntity>()));
            _repoMock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _service.Update(entityDto);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("UpdatedName");
            _repoMock.Verify(x => x.Update(It.Is<FakeEntity>(e => e.Name == "UpdatedName" && e.Id == entityDto.Id)), Times.Once);
            _repoMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task Update_ShouldThrowException_WhenUpdateFails()
        {
            // Arrange
            var entityDto = new FakeResponse { Id = Guid.NewGuid(), Name = "UpdatedName" };
            var entity = new FakeEntity { Id = entityDto.Id, Name = entityDto.Name };

            _repoMock.Setup(x => x.Update(It.IsAny<FakeEntity>()));
            _repoMock.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception("Update failed"));

            // Act & Assert
            var act = async () => await _service.Update(entityDto);
            await act.Should().ThrowAsync<Exception>().WithMessage("Update failed");
        }

        [Fact]
        public async Task FindByIdAsync_ShouldReturnDifferentResultsOnConsecutiveCalls()
        {
            // Arrange
            var id = Guid.NewGuid();
            var firstEntity = new FakeEntity { Id = id, Name = "FirstCall" };
            var secondEntity = new FakeEntity { Id = id, Name = "SecondCall" };

            _repoMock.SetupSequence(x => x.GetByIdAsync(id))
                .ReturnsAsync(firstEntity)
                .ReturnsAsync(secondEntity)
                .ReturnsAsync((FakeEntity?)null);

            // Act
            var result1 = await _service.FindByIdAsync(id);
            var result2 = await _service.FindByIdAsync(id);
            var result3 = await _service.FindByIdAsync(id);

            // Assert
            result1.Should().NotBeNull();
            result1!.Name.Should().Be("FirstCall");

            result2.Should().NotBeNull();
            result2!.Name.Should().Be("SecondCall");

            result3.Should().BeNull();

            _repoMock.Verify(x => x.GetByIdAsync(id), Times.Exactly(3));
        }
    }
}

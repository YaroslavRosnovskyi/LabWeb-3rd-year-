using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using LabWeb.Models.IdentityModels;
using LabWeb.Services;
using LabWeb.Services.Interfaces.AzureInterfaces;
using LabWeb.SettingOptions;
using Microsoft.Extensions.Options;
using Moq;

namespace LabWeb.Tests.Services
{
    public class TokenServiceTests
    {
        private readonly TokenService _tokenService;
        private readonly Mock<IBlobStorageService> _blobStorageServiceMock;
        private readonly JwtSettings _jwtSettings;

        public TokenServiceTests()
        {
            _blobStorageServiceMock = new Mock<IBlobStorageService>();
            _jwtSettings = new JwtSettings
            {
                Secret = "supersecretkey1234567890EXTRASECRET1234",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpirationMinutes = 60
            };
            var options = Options.Create(_jwtSettings);
            _tokenService = new TokenService(options, _blobStorageServiceMock.Object);
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_ShouldGenerateValidToken_WithCorrectClaims()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "TestUser",
                ImageName = "test.png"
            };

            _blobStorageServiceMock.Setup(x => x.GetBlobUrl(user.ImageName))
                .ReturnsAsync("https://blobstorage/test.png");

            // Act
            var token = await _tokenService.GenerateJwtTokenAsync(user);

            // Assert
            token.Should().NotBeNullOrEmpty();

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == user.UserName);
            jwt.Claims.Should().Contain(c => c.Type == "ImageName" && c.Value == "https://blobstorage/test.png");
            jwt.Issuer.Should().Be(_jwtSettings.Issuer);
            jwt.Audiences.Should().Contain(_jwtSettings.Audience);
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_ShouldUseDefaultImage_WhenImageIsNull()
        {
            // Arrange
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "TestUser",
                ImageName = null
            };

            _blobStorageServiceMock.Setup(x => x.GetBlobUrl(null))
                .ReturnsAsync((string?)null);

            // Act
            var token = await _tokenService.GenerateJwtTokenAsync(user);

            // Assert
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            jwt.Claims.Should().Contain(c => c.Type == "ImageName" && c.Value == "Default.jpg");
        }

        [Fact]
        public async Task GenerateJwtTokenAsync_ShouldThrowException_WhenSecretIsInvalid()
        {
            // Arrange
            var invalidOptions = Options.Create(new JwtSettings
            {
                Secret = "",
                Issuer = "TestIssuer",
                Audience = "TestAudience",
                ExpirationMinutes = 60
            });
            var service = new TokenService(invalidOptions, _blobStorageServiceMock.Object);
            var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "User" };

            // Act
            Func<Task> act = async () => await service.GenerateJwtTokenAsync(user);

            // Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("*key length is zero*");
        }

        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("testImage.png", true)]
        public async Task GenerateJwtTokenAsync_ImageUrlTests(string? imageName, bool expectUrl)
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "TestUser", ImageName = imageName };
            var expectedUrl = expectUrl ? "https://blobstorage/testImage.png" : null;

            _blobStorageServiceMock.Setup(x => x.GetBlobUrl(imageName))
                .ReturnsAsync(expectedUrl);

            // Act
            var token = await _tokenService.GenerateJwtTokenAsync(user);
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Assert
            var imageClaim = jwt.Claims.First(c => c.Type == "ImageName").Value;
            if (expectUrl)
                imageClaim.Should().Be(expectedUrl);
            else
                imageClaim.Should().Be("Default.jpg");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabWeb.Context;
using LabWeb.DTOs;
using LabWeb.DTOs.ServiceBusDTO;
using LabWeb.DTOs.ShoppingListDTO;
using LabWeb.Services.Interfaces;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LabWeb.Models.IdentityModels;
using LabWeb.Services.Interfaces.AzureInterfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;
using Elastic.Clients.Elasticsearch.Security;

namespace LabWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IAzureBusSenderService _azureBusSenderService;
        private readonly IShoppingListService _shoppingListService;


        public UsersController(
            UserManager<ApplicationUser> userManager, 
            IBlobStorageService blobStorageService,
            SignInManager<ApplicationUser> signInManager, 
            ITokenService tokenService, 
            IAzureBusSenderService azureBusSenderService, 
            IShoppingListService shoppingListService)
        {
            _userManager = userManager;
            _blobStorageService = blobStorageService;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _azureBusSenderService = azureBusSenderService;
            _shoppingListService = shoppingListService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (model.Password != model.ConfirmPassword)
                return BadRequest("Passwords do not match");

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                ImageName = "Default.jpg"
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var message = new Message(user.Email!, "Registration", "Registration was successful");
                await _azureBusSenderService.Send(message);

                await _signInManager.SignInAsync(user, isPersistent: false);

                var token = await _tokenService.GenerateJwtTokenAsync(user);
                return Ok(new { Token = token, UserId = user.Id });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var token = await _tokenService.GenerateJwtTokenAsync(user);
                return Ok(new { Token = token, UserId = user.Id });
            }

            return Unauthorized("Invalid email or password");
        }

        [HttpGet("external-login/{provider}")]
        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Users", new { ReturnUrl = returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return Challenge(properties, provider);
        }

        [HttpGet("external-login-callback")]
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            if (remoteError != null)
            {
                return BadRequest($"Error from external provider: {remoteError}");
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return Unauthorized("Error loading external login information.");
            }

            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                var token = await _tokenService.GenerateJwtTokenAsync(user);
                return Ok(new { Token = token, UserId = user.Id });
            }

            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            if (email == null)
            {
                return BadRequest("Email claim not received from external provider.");
            }

            var applicationUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                ImageName = "Default.jpg"
            };

            var createResult = await _userManager.CreateAsync(applicationUser);
            if (!createResult.Succeeded)
            {
                return BadRequest(createResult.Errors);
            }

            var addLoginResult = await _userManager.AddLoginAsync(applicationUser, info);
            if (!addLoginResult.Succeeded)
            {
                return BadRequest(addLoginResult.Errors);
            }

            var message = new Message(applicationUser.Email!, "Registration", "Registration was successful");
            await _azureBusSenderService.Send(message);

            await _signInManager.SignInAsync(applicationUser, isPersistent: false);

            var jwtToken = await _tokenService.GenerateJwtTokenAsync(applicationUser);

            return Ok(new { Token = jwtToken, UserId = applicationUser.Id });
        }


        [Authorize]
        [HttpPost("uploadImage/{id}")]
        public async Task<IActionResult> UploadImage([FromRoute] Guid id, IFormFile formFile)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());

            if (user == null)
            {
                NotFound("User not found");
            }

            var blobName = await _blobStorageService.UploadBlob(formFile, user.Id.ToString(), user.ImageName);

            user.ImageName = blobName;

            await _userManager.UpdateAsync(user);

            return Ok($"Image upload was successful, blob name: {blobName}");
        }

        [HttpGet("shopping-list/{id}")]
        public async Task<ActionResult<List<ShoppingListResponse>>> GetShoppingListByUserId([FromRoute] Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return BadRequest("User Not Found");
            }

            var shoppingListResponse =  _shoppingListService.GetShoppingListByUserId(user.Id);

            return Ok(shoppingListResponse);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }

            await _blobStorageService.RemoveBlob(user.ImageName);
            await _userManager.DeleteAsync(user);

            return NoContent();
        }
    }
} 
        

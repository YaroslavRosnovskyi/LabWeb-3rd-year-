using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabWeb.Context;
using LabWeb.DTOs;
using LabWeb.DTOs.ShoppingListDTO;
using LabWeb.Models;
using LabWeb.Services.Interfaces;
using LabWeb.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

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


        public UsersController(UserManager<ApplicationUser> userManager, IBlobStorageService blobStorageService,
            SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IAzureBusSenderService azureBusSenderService, IShoppingListService shoppingListService)
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

        // POST: api/auth/login
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
                // Generate JWT Token
                var token = await _tokenService.GenerateJwtTokenAsync(user);
                return Ok(new { Token = token, UserId = user.Id });
            }

            return Unauthorized("Invalid email or password");
        }



        [Authorize]
        [HttpPost("uploadImage/{userName}")]
        public async Task<ActionResult<UserDto>> UploadImage([FromRoute] string userName, IFormFile formFile)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                NotFound("User not found");
            }

            var blobName = await _blobStorageService.UploadBlob(formFile, user.Id.ToString(), user.ImageName);

            user.ImageName = blobName;

            await _userManager.UpdateAsync(user);

            return Ok(blobName);
        }

        [HttpGet("shopping-list/{id}")]
        public async Task<ActionResult<List<ShoppingListResponse>>> GetShoppingListByUserId(Guid id)
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
        public async Task<IActionResult> DeleteUser(Guid id)
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
        

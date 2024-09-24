using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabWeb.Context;
using LabWeb.DTOs;
using LabWeb.Models;
using LabWeb.Services.Interfaces;
using LabWeb.Services;
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
        private readonly IEmailMessageSender _emailSender;
        private readonly IAzureBusSenderService _azureBusSenderService;


        public UsersController(UserManager<ApplicationUser> userManager, IBlobStorageService blobStorageService, SignInManager<ApplicationUser> signInManager, ITokenService tokenService, IEmailMessageSender emailSender, IAzureBusSenderService azureBusSenderService)
        {
            _userManager = userManager;
            _blobStorageService = blobStorageService;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _emailSender = emailSender;
            _azureBusSenderService = azureBusSenderService;
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
                ImageName = "Default.jpg" // Or handle image as needed
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            var message = new Message(user.Email! , "Account Confirmation", "ff");
                await _azureBusSenderService.Send(message);
            

            if (result.Succeeded)
            {
                

                // Optionally sign in the user automatically after registration
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Generate JWT Token
                var token = await _tokenService.GenerateJwtTokenAsync(user);
                return Ok(new { Token = token });
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
                return Ok(new { Token = token });
            }

            return Unauthorized("Invalid email or password");
        }

        // GET: api/Users
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        //{
        //    return await _userService.GetAllAsync();
        //}

        //[HttpGet]
        //public async Task<ActionResult<PaginatedResponse<UserDto>>> GetPaginatedItems([FromQuery] int skip = 0, [FromQuery] int limit = 10)
        //{
        //    var paginatedEntities = await _userManager.GetAllPaginatedAsync(skip, limit);

        //    foreach (var paginatedEntity in paginatedEntities.MappedEntities)
        //    {
        //        paginatedEntity.ImageName = await _blobStorageService.GetBlobUrl(paginatedEntity.ImageName);
        //    }

        //    paginatedEntities.NextLink = CreateNewLink(skip, limit, paginatedEntities.MappedEntities.Count());


        //    return paginatedEntities;
        //}

        // GET: api/Users/5
        //[HttpGet("{id}")]
        //public async Task<ActionResult<UserDto>> GetUser(Guid id)
        //{
        //    var user = await _userManager.FindByIdAsync(id);

        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    user.ImageName = await _blobStorageService.GetBlobUrl(user.ImageName);

        //    return user;
        //}

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("{id}")]
        //public async Task<IActionResult> PutUser(Guid id, UserDto user)
        //{
        //    if (id != user.Id)
        //    {
        //        return BadRequest();
        //    }

        //    try
        //    {
        //        await _userManager.Update(user);
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!await UserExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPost]
        //public async Task<ActionResult<UserDto>> PostUser(UserDto user)
        //{
        //    var userDto = await _userManager.Insert(user);

        //    userDto.ImageName = await _blobStorageService.GetBlobUrl(userDto.ImageName);

        //    return CreatedAtAction("GetUser", new { id = userDto.Id }, userDto);
        //}

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

        // DELETE: api/Users/5
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteUser(Guid id)
        //{
        //    var user = await _userManager.FindByIdAsync(id);
        //    if (user == null)
        //    {
        //        return NotFound();
        //    }

        //    await _blobStorageService.RemoveBlob(user.ImageName);
        //    await _userManager.DeleteAsync(user);

        //    return NoContent();
        //}

        //private async Task<bool> UserExists(Guid id)
        //{
        //    return await _userManager.FindByIdAsync(id) != null;
        //}

        //private string? CreateNewLink(int skip, int limit, int totalCount)
        //{
        //    string? nextLink = String.Empty;
        //    if (limit <= totalCount)
        //    {
        //        nextLink = Url.Action(nameof(GetPaginatedItems), new { skip = skip + limit, limit });
        //    }
        //    return nextLink;
        //}
    }
}

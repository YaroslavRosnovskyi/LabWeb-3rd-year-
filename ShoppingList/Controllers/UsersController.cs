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

namespace LabWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IBlobStorageService _blobStorageService;

        public UsersController(IUserService userService, IBlobStorageService blobStorageService)
        {
            _userService = userService;
            _blobStorageService = blobStorageService;
        }

        // GET: api/Users
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        //{
        //    return await _userService.GetAllAsync();
        //}

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<UserDto>>> GetPaginatedItems([FromQuery] int skip = 0, [FromQuery] int limit = 10)
        {
            var paginatedEntities = await _userService.GetAllPaginatedAsync(skip, limit);

            foreach (var paginatedEntity in paginatedEntities.MappedEntities)
            {
                paginatedEntity.ImageName = await _blobStorageService.GetBlobUrl(paginatedEntity.ImageName);
            }

            paginatedEntities.NextLink = CreateNewLink(skip, limit, paginatedEntities.MappedEntities.Count());


            return paginatedEntities;
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(Guid id)
        {
            var user = await _userService.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            user.ImageName = await _blobStorageService.GetBlobUrl(user.ImageName);

            return user;
        }

        // PUT: api/Users/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(Guid id, UserDto user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            try
            {
                await _userService.Update(user);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<UserDto>> PostUser(UserDto user)
        {
            var userDto = await _userService.Insert(user);

            userDto.ImageName = await _blobStorageService.GetBlobUrl(userDto.ImageName);

            return CreatedAtAction("GetUser", new { id = userDto.Id }, userDto);
        }

        [HttpPost("uploadImage/{id}")]
        public async Task<ActionResult<UserDto>> UploadImage([FromRoute] Guid id, IFormFile formFile)
        {
            var user = await _userService.FindByIdAsync(id);

            if (user == null)
            {
                NotFound("User not found");
            }

            var blobName = await _blobStorageService.UploadBlob(formFile, user.Id.ToString(), user.ImageName);

            user.ImageName = blobName;

            await _userService.Update(user);

            return Ok(blobName);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var user = await _userService.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            await _blobStorageService.RemoveBlob(user.ImageName);
            await _userService.DeleteAsync(user);

            return NoContent();
        }

        private async Task<bool> UserExists(Guid id)
        {
            return await _userService.FindByIdAsync(id) != null;
        }

        private string? CreateNewLink(int skip, int limit, int totalCount)
        {
            string? nextLink = String.Empty;
            if (limit <= totalCount)
            {
                nextLink = Url.Action(nameof(GetPaginatedItems), new { skip = skip + limit, limit });
            }
            return nextLink;
        }
    }
}

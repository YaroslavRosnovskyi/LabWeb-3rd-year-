using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabWeb.Context;
using LabWeb.DTOs;
using LabWeb.DTOs.ItemDTO;
using LabWeb.Models;
using LabWeb.Services;
using LabWeb.Services.Interfaces;

namespace LabWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;
        private readonly IElasticService _elasticService;

        public ItemsController(IItemService itemService, IElasticService elasticService)
        {
            _itemService = itemService;
            _elasticService = elasticService;
        }

        // GET: api/Items
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<ItemDto>>> GetItems()
        //{
        //    return await _itemService.GetAllAsync();
        //}
        [HttpGet("search")]
        public async Task<ActionResult<List<ItemDto>>> SearchItems([FromQuery] string query, [FromQuery] int skip = 0, [FromQuery] int limit = 10)
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest("Query parameter cannot be empty.");
            }

            var items = await _elasticService.Search(query, skip, limit);

            return Ok(items);
        }


        [HttpPost("create-index")]
        public async Task<IActionResult> CreateIndex(string indexName)
        {
            await _elasticService.CreateIndexIfNotExists(indexName);
            return Ok($"Index {indexName} was created");
        }


        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<ItemResponse>>> GetPaginatedItems([FromQuery] int skip = 0, [FromQuery] int limit = 10)
        {
            var paginatedEntities = await _itemService.GetAllPaginatedAsync(skip, limit);

            await _elasticService.AddOrUpdateBulk(paginatedEntities.MappedEntities, "items");

            string? nextLink = String.Empty;
            if (limit <= paginatedEntities.MappedEntities.Count())
            {
                nextLink = Url.Action(nameof(GetPaginatedItems), new { skip = skip + limit, limit });
            }
            paginatedEntities.NextLink = nextLink;
            

            return paginatedEntities;
        }

        // GET: api/Items/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemResponse>> GetItem(Guid id)
        {
            var item = await _itemService.FindByIdAsync(id);

            if (item == null)
            {
                return NotFound();
            }

            return item;
        }

        // PUT: api/Items/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItem(Guid id, ItemResponse item)
        {
            if (id != item.Id)
            {
                return BadRequest();
            }

            try
            {
                await _itemService.Update(item);
                await _elasticService.AddOrUpdate(item);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ItemExists(id))
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

        // POST: api/Items
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ItemResponse>> PostItem(ItemRequest item)
        {
            var itemDto = await _itemService.Insert(item);
            await _elasticService.AddOrUpdate(itemDto);

            return CreatedAtAction("GetItem", new { id = itemDto.Id }, itemDto);
        }

        // DELETE: api/Items/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            var item = await _itemService.FindByIdAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            await  _itemService.DeleteAsync(item);
            await _elasticService.Remove(item.Id.ToString());

            return NoContent();
        }

        private async Task<bool> ItemExists(Guid id)
        {
            return await _itemService.FindByIdAsync(id) != null;
        }
    }
}

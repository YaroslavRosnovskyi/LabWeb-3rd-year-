using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LabWeb.Context;
using LabWeb.DTOs.ItemCategoryDTO;
using LabWeb.DTOs.ItemDTO;
using LabWeb.Models;
using LabWeb.Services.Interfaces;

namespace LabWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemCategoriesController : ControllerBase
    {
        private readonly IItemCategoryService _itemCategoryService;

        public ItemCategoriesController(IItemCategoryService itemCategoryService)
        {
            _itemCategoryService = itemCategoryService;
        }

        // GET: api/ItemCategories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemCategoryResponse>>> GetItemCategories()
        {
            return await _itemCategoryService.GetAllAsync();
        }

        // GET: api/ItemCategories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemCategoryResponse>> GetItemCategory(Guid id)
        {
            var itemCategory = await _itemCategoryService.FindByIdAsync(id);

            if (itemCategory == null)
            {
                return NotFound();
            }

            return itemCategory;
        }

        // PUT: api/ItemCategories/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutItemCategory(Guid id, ItemCategoryResponse itemCategory)
        {
            if (id != itemCategory.Id)
            {
                return BadRequest();
            }

            try
            {
                await _itemCategoryService.Update(itemCategory);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ItemCategoryExists(id))
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

        // POST: api/ItemCategories
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ItemCategoryResponse>> PostItemCategory(ItemCategoryRequest itemCategory)
        {
            var itemCategoryResponse = await _itemCategoryService.Insert(itemCategory);

            return CreatedAtAction("GetItemCategory", new { id = itemCategoryResponse.Id }, itemCategoryResponse);
        }

        // DELETE: api/ItemCategories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItemCategory(Guid id)
        {
            var itemCategory = await _itemCategoryService.FindByIdAsync(id);
            if (itemCategory == null)
            {
                return NotFound();
            }

            await _itemCategoryService.DeleteAsync(itemCategory);

            return NoContent();
        }

        private async Task<bool> ItemCategoryExists(Guid id)
        {
            return await _itemCategoryService.FindByIdAsync(id) != null;
        }
    }
}

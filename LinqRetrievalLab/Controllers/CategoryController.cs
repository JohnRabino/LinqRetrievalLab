using LinqRetrievalLab.Models.Data;
using LinqRetrievalLab.Models.Domain;
using LinqRetrievalLab.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinqRetrievalLab.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly AppDbContext _db;

        public CategoryController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _db.Category
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _db.Category
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Name
                })
                .SingleOrDefaultAsync();

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(CategoryDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Category data is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Category name is required.");
            }

            var category = new Category
            {
                Name = dto.Name.Trim()
            };

            _db.Category.Add(category);

            await _db.SaveChangesAsync();

            return Ok(category);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditCategory(
            int id,
            CategoryDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Category data is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Category name is required.");
            }

            var category = await _db.Category.FindAsync(id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            category.Name = dto.Name.Trim();

            await _db.SaveChangesAsync();

            return Ok(category);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _db.Category.FindAsync(id);

            if (category == null)
            {
                return NotFound("Category not found.");
            }

            _db.Category.Remove(category);

            await _db.SaveChangesAsync();

            return Ok("Category deleted successfully.");
        }
    }
}
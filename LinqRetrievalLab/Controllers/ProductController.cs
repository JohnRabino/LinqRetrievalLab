using LinqRetrievalLab.Models.Data;
using LinqRetrievalLab.Models.Domain;
using LinqRetrievalLab.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinqRetrievalLab.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly AppDbContext _db;

        public ProductController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            int page = 1,
            int size = 10)
        {
            if (page < 1)
            {
                return BadRequest("Page must be at least 1.");
            }

            if (size < 1 || size > 100)
            {
                return BadRequest("Size must be between 1 and 100.");
            }

            var products = await _db.Product
                .AsNoTracking()
                .OrderBy(p => p.Id)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Stock,
                    p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _db.Product
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Stock,
                    p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .SingleOrDefaultAsync();

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            return Ok(product);
        }

        [HttpGet("category/{categoryId:int}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var categoryExists = await _db.Category
                .AsNoTracking()
                .AnyAsync(c => c.Id == categoryId);

            if (!categoryExists)
            {
                return NotFound("Category not found.");
            }

            var products = await _db.Product
                .AsNoTracking()
                .Where(p => p.CategoryId == categoryId)
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Stock,
                    p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Search name is required.");
            }

            var products = await _db.Product
                .AsNoTracking()
                .Where(p => p.Name.Contains(name))
                .OrderBy(p => p.Name)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Stock,
                    p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("price")]
        public async Task<IActionResult> GetProductsByPrice(
            decimal min,
            decimal max)
        {
            if (min < 0 || max < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            if (min > max)
            {
                return BadRequest(
                    "Minimum price cannot be greater than maximum price.");
            }

            var products = await _db.Product
                .AsNoTracking()
                .Where(p => p.Price >= min && p.Price <= max)
                .OrderBy(p => p.Price)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Price,
                    p.Stock,
                    p.CategoryId,
                    CategoryName = p.Category.Name
                })
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            var totalProducts = await _db.Product
                .AsNoTracking()
                .CountAsync();

            var anyExpensive = await _db.Product
                .AsNoTracking()
                .AnyAsync(p => p.Price > 1000m);

            var totalStock = await _db.Product
                .AsNoTracking()
                .SumAsync(p => p.Stock);

            var averagePrice = await _db.Product
                .AsNoTracking()
                .AverageAsync(p => p.Price);

            return Ok(new
            {
                TotalProducts = totalProducts,
                AnyProductOver1000 = anyExpensive,
                TotalStock = totalStock,
                AveragePrice = averagePrice
            });
        }

        [HttpGet("find/{id:int}")]
        public async Task<IActionResult> FindProduct(int id)
        {
            var product = await _db.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            return Ok(product);
        }

        [HttpGet("unique/{id:int}")]
        public async Task<IActionResult> GetUniqueProduct(int id)
        {
            var product = await _db.Product
                .AsNoTracking()
                .Where(p => p.Id == id)
                .SingleOrDefaultAsync();

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(
            ProductDto dto,
            int categoryId)
        {
            if (dto == null)
            {
                return BadRequest("Product data is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Product name is required.");
            }

            if (dto.Price < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            if (dto.StockQuantity < 0)
            {
                return BadRequest("Stock cannot be negative.");
            }

            var categoryExists = await _db.Category
                .AsNoTracking()
                .AnyAsync(c => c.Id == categoryId);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Price = dto.Price,
                Stock = dto.StockQuantity,
                CategoryId = categoryId
            };

            _db.Product.Add(product);

            await _db.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.Id },
                product);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> EditProduct(
            int id,
            ProductDto dto,
            int categoryId)
        {
            if (dto == null)
            {
                return BadRequest("Product data is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.Name))
            {
                return BadRequest("Product name is required.");
            }

            if (dto.Price < 0)
            {
                return BadRequest("Price cannot be negative.");
            }

            if (dto.StockQuantity < 0)
            {
                return BadRequest("Stock cannot be negative.");
            }

            var product = await _db.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            var categoryExists = await _db.Category
                .AsNoTracking()
                .AnyAsync(c => c.Id == categoryId);

            if (!categoryExists)
            {
                return BadRequest("Category does not exist.");
            }

            product.Name = dto.Name.Trim();
            product.Price = dto.Price;
            product.Stock = dto.StockQuantity;
            product.CategoryId = categoryId;

            await _db.SaveChangesAsync();

            return Ok(product);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _db.Product.FindAsync(id);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            _db.Product.Remove(product);

            await _db.SaveChangesAsync();

            return Ok("Product deleted successfully.");
        }
    }
}
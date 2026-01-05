using EshopProducts.Data;
using EshopProducts.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EshopProducts.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/products")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsDbContext _db;

        public ProductsController(ProductsDbContext db) => _db = db;

        // ---------- v1 ----------
        /// <summary>
        /// Returns all products (no pagination).
        /// </summary>
        [HttpGet]
        [MapToApiVersion("1.0")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            return await _db.Products
                .AsNoTracking()
                .ToListAsync();
        }

        // ---------- v2 ----------
        /// <summary>
        /// Returns products with pagination.
        /// </summary>
        [HttpGet]
        [MapToApiVersion("2.0")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllV2(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var totalItems = await _db.Products.CountAsync();

            var products = await _db.Products
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            Response.Headers["X-Total-Count"] = totalItems.ToString();
            Response.Headers["X-Page-Number"] = page.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();

            return Ok(products);
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<Product>> Get(long id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            return product;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Create(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get),
                new { id = product.Id, version = HttpContext.GetRequestedApiVersion()!.ToString() },
                product);
        }

        [HttpPatch("{id:long}/description")]
        public async Task<ActionResult> PatchDescription(
            long id,
            [FromBody] string? description)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Description = description;
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}

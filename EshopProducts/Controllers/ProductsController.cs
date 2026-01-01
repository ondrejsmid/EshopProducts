using EshopProducts.Data;
using EshopProducts.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Xml.Linq;

namespace EshopProducts.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductsDbContext _db;

        public ProductsController(ProductsDbContext db) => _db = db;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            return await _db.Products.AsNoTracking().ToListAsync();
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult<Product>> Get(long id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            return product;
        }

        [HttpGet("v2")]
        public async Task<ActionResult<IEnumerable<Product>>> GetAllV2(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var totalItems = await _db.Products.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var products = await _db.Products
                .AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Optional: return pagination info in headers
            Response.Headers.Add("X-Total-Count", totalItems.ToString());
            Response.Headers.Add("X-Total-Pages", totalPages.ToString());
            Response.Headers.Add("X-Page-Number", page.ToString());
            Response.Headers.Add("X-Page-Size", pageSize.ToString());

            return Ok(products);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> Create(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
        }

        [HttpPatch("{id:long}/description")]
        public async Task<ActionResult> PatchDescription(long id, [FromBody] string? description)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.Description = description;
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}

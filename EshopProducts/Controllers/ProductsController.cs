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
        /// Returns all products (no pagination). Version 1.0.
        /// </summary>
        /// <returns>List of all products.</returns>
        [HttpGet]
        [MapToApiVersion("1.0")]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            return await _db.Products
                .AsNoTracking()
                .ToListAsync();
        }

        // ---------- v2 ----------
        /// <summary>
        /// Returns products with pagination. Version 2.0.
        /// </summary>
        /// <param name="page">Page number (default = 1).</param>
        /// <param name="pageSize">Page size (default = 10).</param>
        /// <returns>Paged list of products with pagination headers.</returns>
        [HttpGet]
        [MapToApiVersion("2.0")]
        [ProducesResponseType(typeof(IEnumerable<Product>), StatusCodes.Status200OK)]
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

        /// <summary>
        /// Returns a single product by ID.
        /// </summary>
        /// <param name="id">Product ID</param>
        /// <returns>The requested product, or 404 if not found.</returns>
        [HttpGet("{id:long}")]
        [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Product>> Get(long id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();
            return product;
        }

        /// <summary>
        /// Creates a new product.
        /// </summary>
        /// <param name="product">Product object to create</param>
        /// <returns>The created product.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(Product), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Product>> Create(Product product)
        {
            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(Get),
                new { id = product.Id, version = HttpContext.GetRequestedApiVersion()!.ToString() },
                product);
        }

        /// <summary>
        /// Updates the description of a product.
        /// </summary>
        /// <param name="id">ID of the product to update</param>
        /// <param name="description">New description value</param>
        /// <returns>No content if updated, 404 if not found</returns>
        [HttpPatch("{id:long}/description")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

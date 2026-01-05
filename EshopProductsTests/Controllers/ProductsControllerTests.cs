using EshopProducts.Controllers;
using EshopProducts.Data;
using EshopProducts.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EshopProducts.Tests
{
    public class ProductsControllerTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ProductsDbContext _db;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<ProductsDbContext>()
                .UseSqlite(_connection)
                .Options;

            _db = new ProductsDbContext(options);
            _db.Database.EnsureCreated();

            _controller = new ProductsController(_db);
        }

        public void Dispose()
        {
            _db.Dispose();
            _connection.Dispose();
        }

        [Fact]
        public async Task GetAll_NoProducts_ReturnsEmptyList()
        {
            // Arrange: DB is empty

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value!);
            Assert.Empty(products);
        }

        [Fact]
        public async Task GetAll_WithProducts_ReturnsAllProducts()
        {
            // Arrange
            var p1 = new Product { Id = 1, Name = "Prod1", ImgUri = "img1", Price = 5, Description = "desc1" };
            var p2 = new Product { Id = 2, Name = "Prod2", ImgUri = "img2", Price = 10, Description = "desc2" };
            _db.Products.AddRange(p1, p2);
            await _db.SaveChangesAsync();

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<ActionResult<IEnumerable<Product>>>(result);
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value!);
            Assert.Equal(2, products.Count());
            Assert.Contains(products, p => p.Id == 1 && p.Name == "Prod1");
            Assert.Contains(products, p => p.Id == 2 && p.Name == "Prod2");
        }

        [Fact]
        public async Task GetAllV2_NoProducts_ReturnsEmptyListAndHeaders()
        {
            // Arrange: assign HttpContext
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            // Act
            var result = await _controller.GetAllV2();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value!);
            Assert.Empty(products);

            Assert.Equal("0", _controller.Response.Headers["X-Total-Count"]);
            Assert.Equal("1", _controller.Response.Headers["X-Page-Number"]);
            Assert.Equal("10", _controller.Response.Headers["X-Page-Size"]);
        }

        [Fact]
        public async Task GetAllV2_8Products_Page2_PageSize3_ReturnsCorrectPage()
        {
            for (int i = 1; i <= 8; i++)
                _db.Products.Add(new Product { Id = i, Name = $"P{i}", ImgUri = $"img{i}", Price = i, Description = $"d{i}" });
            await _db.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.GetAllV2(page: 2, pageSize: 3);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value!);
            Assert.Equal(3, products.Count());
            Assert.Contains(products, p => p.Id == 4);
            Assert.Contains(products, p => p.Id == 5);
            Assert.Contains(products, p => p.Id == 6);

            Assert.Equal("8", _controller.Response.Headers["X-Total-Count"]);
            Assert.Equal("2", _controller.Response.Headers["X-Page-Number"]);
            Assert.Equal("3", _controller.Response.Headers["X-Page-Size"]);
        }

        [Fact]
        public async Task GetAllV2_8Products_DefaultPage_PageSize3_ReturnsFirstPage()
        {
            for (int i = 1; i <= 8; i++)
                _db.Products.Add(new Product { Id = i, Name = $"P{i}", ImgUri = $"img{i}", Price = i, Description = $"d{i}" });
            await _db.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.GetAllV2(pageSize: 3);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value!);
            Assert.Equal(3, products.Count());
            Assert.Contains(products, p => p.Id == 1);
            Assert.Contains(products, p => p.Id == 2);
            Assert.Contains(products, p => p.Id == 3);

            Assert.Equal("8", _controller.Response.Headers["X-Total-Count"]);
            Assert.Equal("1", _controller.Response.Headers["X-Page-Number"]);
            Assert.Equal("3", _controller.Response.Headers["X-Page-Size"]);
        }

        [Fact]
        public async Task GetAllV2_12Products_Page2_DefaultPageSize_ReturnsCorrectPage()
        {
            for (int i = 1; i <= 12; i++)
                _db.Products.Add(new Product { Id = i, Name = $"P{i}", ImgUri = $"img{i}", Price = i, Description = $"d{i}" });
            await _db.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.GetAllV2(page: 2);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value!);
            Assert.Equal(2, products.Count());
            Assert.Contains(products, p => p.Id == 11);
            Assert.Contains(products, p => p.Id == 12);

            Assert.Equal("12", _controller.Response.Headers["X-Total-Count"]);
            Assert.Equal("2", _controller.Response.Headers["X-Page-Number"]);
            Assert.Equal("10", _controller.Response.Headers["X-Page-Size"]);
        }

        [Fact]
        public async Task GetAllV2_12Products_DefaultPageAndPageSize_ReturnsFirstPage()
        {
            for (int i = 1; i <= 12; i++)
                _db.Products.Add(new Product { Id = i, Name = $"P{i}", ImgUri = $"img{i}", Price = i, Description = $"d{i}" });
            await _db.SaveChangesAsync();

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var result = await _controller.GetAllV2();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<Product>>(okResult.Value!);
            Assert.Equal(10, products.Count());
            Assert.Contains(products, p => p.Id == 1);
            Assert.Contains(products, p => p.Id == 10);

            Assert.Equal("12", _controller.Response.Headers["X-Total-Count"]);
            Assert.Equal("1", _controller.Response.Headers["X-Page-Number"]);
            Assert.Equal("10", _controller.Response.Headers["X-Page-Size"]);
        }

        [Fact]
        public async Task PatchDescription_ProductExists_UpdatesDescription_ReturnsNoContent()
        {
            // Arrange
            var product = new Product
            {
                Id = 1,
                Name = "Test",
                ImgUri = "img",
                Price = 10,
                Description = "old"
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            // Act
            var result = await _controller.PatchDescription(1, "new description");

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updated = await _db.Products.FindAsync(1L);
            Assert.Equal("new description", updated!.Description);
        }

        [Fact]
        public async Task PatchDescription_ProductDoesNotExist_ReturnsNotFound()
        {
            // Act
            var result = await _controller.PatchDescription(999, "doesnt matter");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task PatchDescription_NullDescription_SetsDescriptionToNull()
        {
            // Arrange
            var product = new Product
            {
                Id = 2,
                Name = "Test",
                ImgUri = "img",
                Price = 10,
                Description = "existing"
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            // Act
            var result = await _controller.PatchDescription(2, null);

            // Assert
            Assert.IsType<NoContentResult>(result);

            var updated = await _db.Products.FindAsync(2L);
            Assert.Null(updated!.Description);
        }
    }
}

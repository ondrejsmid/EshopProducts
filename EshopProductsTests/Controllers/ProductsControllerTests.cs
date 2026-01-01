using EshopProducts.Controllers;
using EshopProducts.Data;
using EshopProducts.Models;
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

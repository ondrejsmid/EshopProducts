namespace EshopProducts.Models
{
    public class Product
    {
        public required long Id { get; set; }
        public required string Name { get; set; }
        public required string ImgUri { get; set; }
        public required decimal Price { get; set; }
        public string? Description { get; set; }
    }
}

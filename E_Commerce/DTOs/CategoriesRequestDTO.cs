namespace E_Commerce.DTOs
{
    public class CategoriesRequestDTO
    {
        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }

        public IFormFile? Image { get; set; }
    }
}

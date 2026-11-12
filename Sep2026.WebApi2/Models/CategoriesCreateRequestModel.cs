namespace Sep2026.WebApi2.Models
{
    public class CategoriesCreateRequestModel
    {
        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }
    }

    public class CategoriesUpsertRequestModel
    {
        public string CategoryName { get; set; } = null!;

        public string? Description { get; set; }
    }
}


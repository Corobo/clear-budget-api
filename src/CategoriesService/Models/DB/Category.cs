namespace CategoriesService.Models.DB
{

    public class Category
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Color { get; set; } = "#000000";

        /// <summary>
        /// If null → global category. Otherwise → user-specific.
        /// Note: this is an external reference (not a FK).
        /// </summary>
        public Guid? UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

}

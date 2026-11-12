using System;

namespace BookStore.DTOs
{
    public class ProductReviewDto
    {
        public string ReviewerName { get; set; } = string.Empty;
        public byte Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public string StarsText => new string('★', Rating) + new string('☆', Math.Max(0, 5 - Rating));
    }
}

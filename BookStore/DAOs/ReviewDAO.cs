using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using BookStore.Models;

namespace BookStore.DAOs
{
    public class ReviewDAO
    {
        private static ReviewDAO? _instance;
        public static ReviewDAO Instance => _instance ??= new ReviewDAO();

        // Kiểm tra điều kiện đánh giá
        public bool CanUserReview(int accountId, int orderDetailId, out string errorMessage)
        {
            errorMessage = string.Empty;
            using var context = new DbSe2038BookymartFContext();

            var detail = context.TbOrderDetails
                .Include(d => d.Order)
                .FirstOrDefault(d => d.OrderDetailId == orderDetailId);

            if (detail == null)
            {
                errorMessage = "Không tìm thấy thông tin dòng sản phẩm trong đơn hàng.";
                return false;
            }

            if (detail.Order == null || detail.Order.AccountId != accountId)
            {
                errorMessage = "Bạn không có quyền đánh giá đơn hàng của người khác.";
                return false;
            }

            if (detail.Order.Status != "DELIVERED")
            {
                errorMessage = "Chỉ có thể đánh giá sản phẩm từ đơn hàng đã giao thành công (DELIVERED).";
                return false;
            }

            if (context.TbReviews.Any(r => r.OrderDetailId == orderDetailId))
            {
                errorMessage = "Dòng sản phẩm này trong đơn hàng đã được đánh giá trước đó.";
                return false;
            }

            return true;
        }

        // Thêm đánh giá mới
        public void AddReview(int accountId, int orderDetailId, byte rating, string? comment)
        {
            using var context = new DbSe2038BookymartFContext();
            var detail = context.TbOrderDetails.FirstOrDefault(d => d.OrderDetailId == orderDetailId);
            if (detail == null) throw new Exception("Không tìm thấy dòng đơn hàng.");

            var review = new TbReview
            {
                ProductId = detail.ProductId,
                AccountId = accountId,
                OrderDetailId = orderDetailId,
                Rating = rating,
                Comment = comment,
                Status = "VISIBLE",
                CreatedAt = DateTime.Now
            };

            context.TbReviews.Add(review);
            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = accountId,
                Action = $"Đánh giá {rating} sao cho sản phẩm ID {detail.ProductId}",
                CreatedAt = DateTime.Now
            });

            context.SaveChanges();
        }

        // Thống kê điểm trung bình
        public (double AvgRating, int TotalReviews) GetProductRatingStats(int productId)
        {
            using var context = new DbSe2038BookymartFContext();
            var reviews = context.TbReviews.Where(r => r.ProductId == productId && r.Status == "VISIBLE");

            int count = reviews.Count();
            if (count == 0) return (0.0, 0);

            return (Math.Round(reviews.Average(r => (double)r.Rating), 1), count);
        }
    }
}
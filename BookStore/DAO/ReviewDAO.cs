using System;
using System.Collections.Generic;
using System.Linq;
using BookStore.DTOs;
using BookStore.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DAO
{
    public class ReviewDAO
    {
        private static ReviewDAO? _instance;
        public static ReviewDAO Instance => _instance ??= new ReviewDAO();

        // Lấy tên sản phẩm theo dòng đơn hàng để hiển thị trên ReviewWindow
        public string? GetOrderDetailProductName(int orderDetailId)
        {
            using var context = new DbSe2038BookymartFContext();
            return context.TbOrderDetails.AsNoTracking()
                .Where(d => d.OrderDetailId == orderDetailId)
                .Select(d => d.ProductNameSnapshot)
                .FirstOrDefault();
        }

        // Kiểm tra điều kiện đánh giá (CUS-09 & constraint 2.4)
        public bool CanUserReview(int accountId, int orderDetailId, out string errorMessage)
        {
            errorMessage = string.Empty;
            using var context = new DbSe2038BookymartFContext();

            var detail = context.TbOrderDetails
                .AsNoTracking()
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

        // Thêm đánh giá mới (CUS-09)
        public string? AddReview(int accountId, int orderDetailId, byte rating, string? comment)
        {
            if (rating < 1 || rating > 5)
                return "Số sao đánh giá phải từ 1 đến 5.";

            string? trimmedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
            if (trimmedComment != null && trimmedComment.Length > 1000)
                return "Nội dung đánh giá tối đa 1000 ký tự.";

            if (!CanUserReview(accountId, orderDetailId, out string err))
                return err;

            using var context = new DbSe2038BookymartFContext();
            var detail = context.TbOrderDetails.FirstOrDefault(d => d.OrderDetailId == orderDetailId);
            if (detail == null) return "Không tìm thấy dòng đơn hàng.";

            context.TbReviews.Add(new TbReview
            {
                ProductId = detail.ProductId,
                AccountId = accountId,
                OrderDetailId = orderDetailId,
                Rating = rating,
                Comment = trimmedComment,
                Status = "VISIBLE",
                CreatedAt = DateTime.Now
            });

            OrderLog.Add(context, $"Đánh giá {rating} sao cho sản phẩm ID {detail.ProductId} (OrderDetail ID {orderDetailId})", accountId);
            context.SaveChanges();
            return null;
        }

        // Lấy danh sách đánh giá hiển thị của sản phẩm (GUE-03 / CUS-09)
        public List<ProductReviewDto> GetProductReviews(int productId)
        {
            using var context = new DbSe2038BookymartFContext();
            return context.TbReviews
                .AsNoTracking()
                .Where(r => r.ProductId == productId && r.Status == "VISIBLE")
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ProductReviewDto
                {
                    ReviewerName = r.Account.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment ?? string.Empty,
                    CreatedAt = r.CreatedAt
                })
                .ToList();
        }

        // Thống kê điểm trung bình của 1 sản phẩm
        public (double AvgRating, int TotalReviews) GetProductRatingStats(int productId)
        {
            using var context = new DbSe2038BookymartFContext();
            var reviews = context.TbReviews.AsNoTracking()
                .Where(r => r.ProductId == productId && r.Status == "VISIBLE");

            int count = reviews.Count();
            if (count == 0) return (0.0, 0);

            return (Math.Round(reviews.Average(r => (double)r.Rating), 1), count);
        }

        // Thống kê điểm trung bình hàng loạt theo danh sách ProductId (tránh query N+1 ở HomeWindow)
        public Dictionary<int, (double AvgRating, int TotalReviews)> GetProductRatingStatsBatch(IEnumerable<int> productIds)
        {
            var ids = productIds.Distinct().ToList();
            if (ids.Count == 0) return new Dictionary<int, (double, int)>();

            using var context = new DbSe2038BookymartFContext();
            return context.TbReviews
                .AsNoTracking()
                .Where(r => ids.Contains(r.ProductId) && r.Status == "VISIBLE")
                .GroupBy(r => r.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,
                    Avg = g.Average(r => (double)r.Rating),
                    Count = g.Count()
                })
                .ToDictionary(x => x.ProductId, x => (Math.Round(x.Avg, 1), x.Count));
        }
    }
}

using BookStore.DTOs;
using BookStore.Models;
using BookStore.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BookStore.DAOs
{
    public class ReportDAO
    {
        private static ReportDAO? _instance;
        public static ReportDAO Instance => _instance ??= new ReportDAO();

        public RevenueReportDto GetRevenueReport(DateTime fromDate, DateTime toDate)
        {
            using var context = new DbSe2038BookymartFContext();

            var deliveredOrders = context.TbOrders
                .Where(o => o.Status == "DELIVERED" && o.CreatedAt >= fromDate && o.CreatedAt <= toDate)
                .ToList();

            var orderIds = deliveredOrders.Select(o => o.OrderId).ToList();

            var orderDetails = context.TbOrderDetails
                .Where(d => orderIds.Contains(d.OrderId))
                .ToList();

            decimal totalRevenue = orderDetails.Sum(d => d.Quantity * d.UnitPrice);
            decimal totalCost = orderDetails.Sum(d => d.Quantity * d.CostPriceSnapshot);

            return new RevenueReportDto
            {
                TotalOrders = deliveredOrders.Count,
                TotalRevenue = totalRevenue,
                TotalCost = totalCost,
                GrossProfit = totalRevenue - totalCost
            };
        }

        public List<TopSellingProductDto> GetTopSellingProducts(int topCount = 5)
        {
            using var context = new DbSe2038BookymartFContext();

            return context.TbOrderDetails
                .Include(d => d.Order)
                .Where(d => d.Order != null && d.Order.Status == "DELIVERED")
                .GroupBy(d => new { d.ProductId, d.ProductNameSnapshot })
                .Select(g => new TopSellingProductDto
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductNameSnapshot,
                    TotalQuantitySold = g.Sum(x => x.Quantity),
                    TotalRevenueGenerated = g.Sum(x => x.Quantity * x.UnitPrice)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(topCount)
                .ToList();
        }

        public List<TbProduct> GetLowStockProducts(int threshold = 10)
        {
            using var context = new DbSe2038BookymartFContext();
            return context.TbProducts
                .Include(p => p.Category)
                .Where(p => p.Status == "ACTIVE" && p.StockQuantity <= threshold)
                .OrderBy(p => p.StockQuantity)
                .ToList();
        }

        public void ExportProductsToCsv(string filePath)
        {
            using var context = new DbSe2038BookymartFContext();
            var products = context.TbProducts.Include(p => p.Category).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("ID,SKU,TenSanPham,DanhMuc,Loai,GiaBan,GiaVon,TonKho,TrangThai");

            foreach (var p in products)
            {
                string categoryName = p.Category != null ? p.Category.Name : "";
                string nameEscaped = $"\"{p.Name.Replace("\"", "\"\"")}\"";
                sb.AppendLine($"{p.ProductId},{p.Sku},{nameEscaped},{categoryName},{p.ProductType},{p.Price},{p.UnitCost},{p.StockQuantity},{p.Status}");
            }

            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);

            context.TbActivityLogs.Add(new TbActivityLog
            {
                AccountId = GlobalSession.AccountId,
                Action = $"Xuất CSV danh sách sản phẩm ra tệp: {filePath}",
                CreatedAt = DateTime.Now
            });
            context.SaveChanges();
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BookStore.DTOs;
using BookStore.Models;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DAO
{
    public class ReportDAO
    {
        private static ReportDAO? _instance;
        public static ReportDAO Instance => _instance ??= new ReportDAO();

        public RevenueReportDto GetRevenueReport(DateTime fromDate, DateTime toDate)
        {
            DateTime start = fromDate.Date;
            DateTime end = toDate.Date.AddDays(1).AddSeconds(-1);

            using var context = new DbSe2038BookymartFContext();

            var deliveredOrders = context.TbOrders
                .AsNoTracking()
                .Where(o => o.Status == "DELIVERED" && o.CreatedAt >= start && o.CreatedAt <= end)
                .Select(o => new { o.OrderId, o.DiscountAmount })
                .ToList();

            var orderIds = deliveredOrders.Select(o => o.OrderId).ToList();

            var totals = context.TbOrderDetails
                .AsNoTracking()
                .Where(d => orderIds.Contains(d.OrderId))
                .Select(d => new
                {
                    Sales = d.Quantity * d.UnitPrice,
                    Cost = d.Quantity * d.CostPriceSnapshot
                })
                .ToList();

            decimal grossSales = totals.Sum(x => x.Sales);
            decimal totalDiscount = deliveredOrders.Sum(o => o.DiscountAmount);
            decimal totalRevenue = Math.Max(0m, grossSales - totalDiscount);
            decimal totalCost = totals.Sum(x => x.Cost);

            return new RevenueReportDto
            {
                TotalOrders = deliveredOrders.Count,
                TotalRevenue = totalRevenue,
                TotalCost = totalCost,
                GrossProfit = totalRevenue - totalCost
            };
        }

        public List<TopSellingProductDto> GetTopSellingProducts(int topCount = 5)
            => GetTopSellingProducts(null, null, topCount);

        public List<TopSellingProductDto> GetTopSellingProducts(DateTime? fromDate, DateTime? toDate, int topCount = 5)
        {
            using var context = new DbSe2038BookymartFContext();

            var query = context.TbOrderDetails
                .AsNoTracking()
                .Where(d => d.Order.Status == "DELIVERED");

            if (fromDate.HasValue)
            {
                DateTime start = fromDate.Value.Date;
                query = query.Where(d => d.Order.CreatedAt >= start);
            }

            if (toDate.HasValue)
            {
                DateTime end = toDate.Value.Date.AddDays(1).AddSeconds(-1);
                query = query.Where(d => d.Order.CreatedAt <= end);
            }

            return query
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
                .AsNoTracking()
                .Include(p => p.Category)
                .Where(p => p.Status == "ACTIVE" && p.StockQuantity <= threshold)
                .OrderBy(p => p.StockQuantity)
                .ToList();
        }

        public void ExportProductsToCsv(string filePath)
        {
            using var context = new DbSe2038BookymartFContext();
            var products = context.TbProducts.AsNoTracking().Include(p => p.Category).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("ID,SKU,TenSanPham,DanhMuc,Loai,GiaBan,GiaVon,TonKho,CanNangGram,TrangThai");

            foreach (var p in products)
            {
                string categoryName = EscapeCsv(p.Category?.Name ?? string.Empty);
                string nameEscaped = EscapeCsv(p.Name);
                string skuEscaped = EscapeCsv(p.Sku);
                sb.AppendLine($"{p.ProductId},{skuEscaped},{nameEscaped},{categoryName},{p.ProductType},{p.Price:0},{p.UnitCost:0},{p.StockQuantity},{p.WeightGrams ?? 0},{p.Status}");
            }

            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));

            OrderLog.Add(context, $"Xuất CSV danh sách sản phẩm ra tệp: {filePath}");
            context.SaveChanges();
        }

        private static string EscapeCsv(string value)
            => $"\"{value.Replace("\"", "\"\"")}\"";
    }
}

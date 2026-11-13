using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BookStore.Controllers;
using BookStore.DAO;
using BookStore.Helpers;
using BookStore.Models;

namespace BookStore.Tests
{
    public class StreamIoTests
    {
        [Fact]
        public void BuildProductsCsv_MemoryStreamWithUtf8Bom_RoundTripsVietnameseAndEscapedQuotes()
        {
            var products = new List<TbProduct>
            {
                new()
                {
                    ProductId = 10,
                    Sku = "BK-\"010\"",
                    Name = "Sách \"Đắc Nhân Tâm\", Bản Đặc Biệt",
                    Category = new TbCategory { CategoryId = 1, Name = "Văn học & Kỹ năng" },
                    ProductType = "BOOK",
                    Price = 110000m,
                    UnitCost = 65000m,
                    StockQuantity = 25,
                    WeightGrams = 420,
                    Status = "ACTIVE"
                },
                new()
                {
                    ProductId = 11,
                    Sku = "ST-011",
                    Name = "Bút bi Thiên Long",
                    Category = null!,
                    ProductType = "STATIONERY",
                    Price = 5500m,
                    UnitCost = 3000m,
                    StockQuantity = 100,
                    WeightGrams = null,
                    Status = "INACTIVE"
                }
            };

            string csv = ReportDAO.BuildProductsCsv(products);

            using var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true), leaveOpen: true))
            {
                writer.Write(csv);
            }

            byte[] rawBytes = memoryStream.ToArray();
            Assert.True(rawBytes.Length >= 3);
            Assert.Equal(0xEF, rawBytes[0]);
            Assert.Equal(0xBB, rawBytes[1]);
            Assert.Equal(0xBF, rawBytes[2]);

            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream, Encoding.UTF8);
            string? header = reader.ReadLine();
            string? firstRow = reader.ReadLine();
            string? secondRow = reader.ReadLine();
            string? end = reader.ReadLine();

            Assert.Equal("ID,SKU,TenSanPham,DanhMuc,Loai,GiaBan,GiaVon,TonKho,CanNangGram,TrangThai", header);
            Assert.Equal("10,\"BK-\"\"010\"\"\",\"Sách \"\"Đắc Nhân Tâm\"\", Bản Đặc Biệt\",\"Văn học & Kỹ năng\",BOOK,110000,65000,25,420,ACTIVE", firstRow);
            Assert.Equal("11,\"ST-011\",\"Bút bi Thiên Long\",\"\",STATIONERY,5500,3000,100,0,INACTIVE", secondRow);
            Assert.Null(end);
        }

        [Fact]
        public void BuildVouchersCsv_FileStreamRoundTrip_PreservesUtf8BomAndAllColumns()
        {
            var vouchers = new List<TbVoucher>
            {
                new()
                {
                    VoucherId = 1,
                    Code = "SALE20",
                    DiscountType = "PERCENT",
                    DiscountValue = 20m,
                    MaxDiscountCap = 50000m,
                    MinOrderValue = 100000m,
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 12, 31),
                    MaxUsageLimit = 200,
                    PerCustomerLimit = 2,
                    Status = "ACTIVE"
                },
                new()
                {
                    VoucherId = 2,
                    Code = "FLAT\"30",
                    DiscountType = "FIXED",
                    DiscountValue = 30000m,
                    MaxDiscountCap = null,
                    MinOrderValue = 150000m,
                    StartDate = new DateTime(2026, 2, 1),
                    EndDate = new DateTime(2026, 3, 1),
                    MaxUsageLimit = 50,
                    PerCustomerLimit = 1,
                    Status = "INACTIVE"
                }
            };

            string tempFile = Path.Combine(Path.GetTempPath(), $"bookymart_vouchers_{Guid.NewGuid():N}.csv");
            try
            {
                string csv = VoucherController.BuildVouchersCsv(vouchers);
                using (var writeStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(writeStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)))
                {
                    writer.Write(csv);
                }

                byte[] bom = new byte[3];
                using (var inspectStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    int bytesRead = inspectStream.Read(bom, 0, bom.Length);
                    Assert.Equal(3, bytesRead);
                }
                Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bom);

                using var readStream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var reader = new StreamReader(readStream, Encoding.UTF8);
                Assert.Equal("ID,MaCode,LoaiGiam,MucGiam,GiamToiDa,DonToiThieu,NgayBatDau,NgayKetThuc,TongLuotDung,LuotMoiKhach,TrangThai", reader.ReadLine());
                Assert.Equal("1,\"SALE20\",PERCENT,20,50000,100000,2026-01-01,2026-12-31,200,2,ACTIVE", reader.ReadLine());
                Assert.Equal("2,\"FLAT\"\"30\",FIXED,30000,,150000,2026-02-01,2026-03-01,50,1,INACTIVE", reader.ReadLine());
                Assert.Null(reader.ReadLine());
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void ImageHelper_SaveProductImageAndResolve_CopiesExactBinaryStream()
        {
            byte[] samplePngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x01, 0x02, 0x03, 0x04];
            string sourceFile = Path.Combine(Path.GetTempPath(), $"bookymart_img_{Guid.NewGuid():N}.png");
            string? resolvedDestination = null;

            try
            {
                using (var fs = new FileStream(sourceFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    fs.Write(samplePngBytes, 0, samplePngBytes.Length);
                }

                string relativeUrl = ImageHelper.SaveProductImage(sourceFile, 777);
                Assert.StartsWith("/Images/Products/prod_777_", relativeUrl);
                Assert.EndsWith(".png", relativeUrl);

                resolvedDestination = ImageHelper.ResolveImagePath(relativeUrl);
                Assert.NotNull(resolvedDestination);
                Assert.True(File.Exists(resolvedDestination));

                using var readBack = new FileStream(resolvedDestination!, FileMode.Open, FileAccess.Read, FileShare.Read);
                using var ms = new MemoryStream();
                readBack.CopyTo(ms);
                Assert.Equal(samplePngBytes, ms.ToArray());
            }
            finally
            {
                if (File.Exists(sourceFile)) File.Delete(sourceFile);
                if (!string.IsNullOrEmpty(resolvedDestination) && File.Exists(resolvedDestination))
                    File.Delete(resolvedDestination);
            }
        }

        [Fact]
        public void ImageHelper_SaveProductImage_ReturnsFallbackWhenSourceMissing_AndResolveHandlesEdgeInputs()
        {
            const string missingPath = @"C:\nonexistent\missing_book_cover.jpg";
            Assert.Equal(missingPath, ImageHelper.SaveProductImage(missingPath, 1));
            Assert.Null(ImageHelper.ResolveImagePath(null));
            Assert.Null(ImageHelper.ResolveImagePath("   "));
            Assert.Equal("https://cdn.example.com/cover.jpg", ImageHelper.ResolveImagePath("https://cdn.example.com/cover.jpg"));
        }
    }
}

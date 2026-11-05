using BookStore.Models;
using BookStore.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.DAO
{
    /// <summary>
    /// Vòng đời đơn hàng: CUS-06/07/08, STF-01 đến STF-05, MNG-04.
    /// Mọi lần đổi trạng thái đều kiểm tra lại OrderStatus.ALLOWED_TRANSITIONS tại đây,
    /// độc lập với việc giao diện đã ẩn nút. Đây là nơi duy nhất (cùng CheckoutDAO) đổi tồn kho
    /// khi đặt, hủy hoặc hoàn hàng.
    /// </summary>
    public static class OrderDAO
    {
        /// <summary>Giá trị lọc riêng cho danh sách của Staff: đơn có yêu cầu hoàn hàng đang chờ.</summary>
        public const string FilterReturnRequested = "RETURN_REQUESTED";

        // ===== ĐỌC DỮ LIỆU =====

        public static List<OrderRow> GetMyOrders(string? status)
        {
            int accountId = SessionGuard.CurrentAccountId;
            using var context = new DbSe2038BookymartFContext();

            var query = context.TbOrders.AsNoTracking().Where(o => o.AccountId == accountId);
            if (!string.IsNullOrEmpty(status)) query = query.Where(o => o.Status == status);

            return Project(query.OrderByDescending(o => o.OrderId)).ToList();
        }

        public static List<OrderRow> GetOrdersForStaff(string? filter, string? keyword)
        {
            if (!SessionGuard.HasRole(SessionGuard.RoleStaff)) return new List<OrderRow>();

            using var context = new DbSe2038BookymartFContext();
            var query = context.TbOrders.AsNoTracking().AsQueryable();

            if (filter == FilterReturnRequested)
            {
                query = query.Where(o => o.ReturnStatus == ReturnState.Requested);
            }
            else if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(o => o.Status == filter);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string text = keyword.Trim();
                query = query.Where(o => o.OrderCode.Contains(text)
                                         || o.ReceiverName.Contains(text)
                                         || o.ReceiverPhone.Contains(text));
            }

            return Project(query.OrderByDescending(o => o.OrderId)).ToList();
        }

        public static List<OrderRow> GetRefundPending()
        {
            if (!SessionGuard.HasRole(SessionGuard.RoleManager)) return new List<OrderRow>();

            using var context = new DbSe2038BookymartFContext();
            var query = context.TbOrders.AsNoTracking()
                .Where(o => o.PaymentStatus == PaymentState.RefundPending)
                .OrderBy(o => o.OrderId);
            return Project(query).ToList();
        }

        /// <summary>Khách chỉ xem được đơn của mình; Staff và Manager xem được mọi đơn.</summary>
        public static OrderDetailData? GetOrderDetail(int orderId)
        {
            using var context = new DbSe2038BookymartFContext();

            var order = Project(context.TbOrders.AsNoTracking().Where(o => o.OrderId == orderId)).FirstOrDefault();
            if (order == null) return null;

            bool isInternal = SessionGuard.HasRole(SessionGuard.RoleStaff, SessionGuard.RoleManager);
            if (!isInternal && order.AccountId != SessionGuard.CurrentAccountId) return null;

            var lines = context.TbOrderDetails.AsNoTracking()
                .Where(d => d.OrderId == orderId)
                .Select(d => new OrderLineRow
                {
                    OrderDetailId = d.OrderDetailId,
                    ProductId = d.ProductId,
                    ProductName = d.ProductNameSnapshot,
                    Quantity = d.Quantity,
                    UnitPrice = d.UnitPrice,
                    HasReview = d.TbReview != null
                })
                .ToList();

            bool canReview = SessionGuard.IsCustomer && order.Status == OrderStatus.Delivered;
            foreach (var line in lines)
            {
                line.CanReview = canReview && !line.HasReview;
            }

            return new OrderDetailData { Order = order, Lines = lines };
        }

        private static IQueryable<OrderRow> Project(IQueryable<TbOrder> query)
        {
            return query.Select(o => new OrderRow
            {
                OrderId = o.OrderId,
                OrderCode = o.OrderCode,
                AccountId = o.AccountId,
                CustomerName = o.Account.FullName,
                CreatedAt = o.CreatedAt,
                Status = o.Status,
                PaymentMethod = o.PaymentMethod,
                PaymentStatus = o.PaymentStatus,
                ReceiverName = o.ReceiverName,
                ReceiverPhone = o.ReceiverPhone,
                ShippingAddress = o.ShippingAddress,
                Subtotal = o.TbOrderDetails.Sum(d => d.Quantity * d.UnitPrice),
                DiscountAmount = o.DiscountAmount,
                ShippingFee = o.ShippingFee,
                VoucherCode = o.Voucher != null ? o.Voucher.Code : null,
                CancelReason = o.CancelReason,
                ReturnStatus = o.ReturnStatus,
                ReturnReason = o.ReturnReason,
                ReturnRequestedAt = o.ReturnRequestedAt,
                PhoneConfirmStatus = o.PhoneConfirmStatus,
                PhoneAttemptCount = o.PhoneAttemptCount
            });
        }

        // ===== KHÁCH HÀNG =====

        /// <summary>CUS-07: khách chỉ tự hủy khi đơn đang PENDING (mục 2.4).</summary>
        public static string? CancelByCustomer(int orderId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return "Vui lòng nhập lý do hủy đơn.";

            return Execute(orderId, new[] { SessionGuard.RoleCustomer }, (context, order) =>
            {
                if (order.AccountId != SessionGuard.CurrentAccountId) return "Không tìm thấy đơn hàng.";
                if (order.Status == OrderStatus.PendingPayment)
                {
                    return "Đơn VietQR chưa được xác nhận thanh toán nên chưa thể tự hủy. Vui lòng liên hệ cửa hàng.";
                }
                if (order.Status != OrderStatus.Pending)
                {
                    return "Đơn hàng đã được xử lý nên không thể tự hủy.";
                }

                return ApplyCancel(context, order, reason.Trim(), $"Khách hủy đơn {order.OrderCode}");
            });
        }

        /// <summary>CUS-08: yêu cầu hoàn hàng sau khi đã nhận hàng. Không giới hạn thời gian, mỗi đơn một lần.</summary>
        public static string? RequestReturn(int orderId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return "Vui lòng nhập lý do hoàn hàng.";

            return Execute(orderId, new[] { SessionGuard.RoleCustomer }, (context, order) =>
            {
                if (order.AccountId != SessionGuard.CurrentAccountId) return "Không tìm thấy đơn hàng.";
                if (order.Status != OrderStatus.Delivered) return "Chỉ đơn đã giao mới được yêu cầu hoàn hàng.";
                if (order.ReturnStatus != null) return "Đơn hàng này đã có yêu cầu hoàn hàng.";

                order.ReturnStatus = ReturnState.Requested;
                order.ReturnReason = reason.Trim();
                order.ReturnRequestedAt = DateTime.Now;
                OrderLog.Add(context, $"Khách yêu cầu hoàn hàng đơn {order.OrderCode}");
                return null;
            });
        }

        // ===== NHÂN VIÊN =====

        /// <summary>STF-02: xác nhận đã nhận tiền VietQR sau khi đối chiếu sao kê.</summary>
        public static string? ConfirmPayment(int orderId)
        {
            return ExecuteStaff(orderId, (context, order) =>
            {
                if (order.PaymentMethod != PaymentMethods.OnlineQr) return "Đơn này không thanh toán bằng VietQR.";

                string? error = Transition(order, OrderStatus.Pending);
                if (error != null) return error;

                order.PaymentStatus = PaymentState.Paid;
                OrderLog.Add(context, $"Staff xác nhận thanh toán VietQR đơn {order.OrderCode}");
                return null;
            });
        }

        /// <summary>STF-01: xác nhận đơn, chuyển sang xử lý.</summary>
        public static string? ConfirmOrder(int orderId)
        {
            return ExecuteStaff(orderId, (context, order) =>
            {
                if (order.Status != OrderStatus.Pending) return "Chỉ xác nhận được đơn đang chờ xác nhận.";

                string? error = Transition(order, OrderStatus.Processing);
                if (error != null) return error;

                OrderLog.Add(context, $"Staff xác nhận đơn {order.OrderCode}");
                return null;
            });
        }

        /// <summary>STF-03: ghi nhận đã gọi được khách.</summary>
        public static string? ConfirmPhone(int orderId)
        {
            return ExecuteStaff(orderId, (context, order) =>
            {
                if (order.Status != OrderStatus.Processing) return "Chỉ gọi xác nhận khi đơn đang xử lý.";

                order.PhoneConfirmStatus = PhoneState.Confirmed;
                order.PhoneAttemptCount = 0;
                OrderLog.Add(context, $"Staff gọi xác nhận thành công đơn {order.OrderCode}");
                return null;
            });
        }

        /// <summary>STF-03: không liên hệ được khách. Đủ 3 lần liên tiếp thì tự động hủy đơn.</summary>
        public static string? MarkPhoneNotReached(int orderId)
        {
            return ExecuteStaff(orderId, (context, order) =>
            {
                if (order.Status != OrderStatus.Processing) return "Chỉ gọi xác nhận khi đơn đang xử lý.";

                order.PhoneConfirmStatus = PhoneState.NotConfirmed;
                order.PhoneAttemptCount += 1;
                OrderLog.Add(context, $"Staff không liên hệ được khách đơn {order.OrderCode} (lần {order.PhoneAttemptCount})");

                if (order.PhoneAttemptCount >= PhoneState.MaxFailedAttempts)
                {
                    return ApplyCancel(context, order,
                        $"Không liên hệ được khách sau {PhoneState.MaxFailedAttempts} lần gọi",
                        $"Hệ thống tự hủy đơn {order.OrderCode}");
                }
                return null;
            });
        }

        /// <summary>STF-01/03: chỉ được giao khi đã gọi xác nhận khách.</summary>
        public static string? ShipOrder(int orderId)
        {
            return ExecuteStaff(orderId, (context, order) =>
            {
                if (order.PhoneConfirmStatus != PhoneState.Confirmed)
                {
                    return "Cần gọi xác nhận khách trước khi chuyển sang đang giao.";
                }

                string? error = Transition(order, OrderStatus.Shipped);
                if (error != null) return error;

                OrderLog.Add(context, $"Staff chuyển giao đơn {order.OrderCode}");
                return null;
            });
        }

        /// <summary>Giao thành công. Đơn COD được tính là đã thu tiền tại thời điểm này.</summary>
        public static string? MarkDelivered(int orderId)
        {
            return ExecuteStaff(orderId, (context, order) =>
            {
                string? error = Transition(order, OrderStatus.Delivered);
                if (error != null) return error;

                if (order.PaymentMethod == PaymentMethods.Cod)
                {
                    order.PaymentStatus = PaymentState.Paid;
                }

                OrderLog.Add(context, $"Staff xác nhận đã giao đơn {order.OrderCode}");
                return null;
            });
        }

        /// <summary>STF-04: giao thất bại hoặc khách từ chối nhận, chuyển sang tạm giữ.</summary>
        public static string? MarkDeliveryFailed(int orderId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return "Vui lòng nhập lý do tạm giữ.";

            return ExecuteStaff(orderId, (context, order) =>
            {
                string? error = Transition(order, OrderStatus.OnHold);
                if (error != null) return error;

                OrderLog.Add(context, $"Staff tạm giữ đơn {order.OrderCode}: {reason.Trim()}");
                return null;
            });
        }

        /// <summary>Giao lại đơn đang tạm giữ: phải gọi xác nhận khách lại từ đầu.</summary>
        public static string? ResumeOrder(int orderId)
        {
            return ExecuteStaff(orderId, (context, order) =>
            {
                if (order.Status != OrderStatus.OnHold) return "Chỉ xử lý lại được đơn đang tạm giữ.";

                string? error = Transition(order, OrderStatus.Processing);
                if (error != null) return error;

                order.PhoneConfirmStatus = PhoneState.Pending;
                OrderLog.Add(context, $"Staff xử lý lại đơn {order.OrderCode}");
                return null;
            });
        }

        public static string? CancelByStaff(int orderId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return "Vui lòng nhập lý do hủy đơn.";

            return ExecuteStaff(orderId, (context, order) =>
                ApplyCancel(context, order, reason.Trim(), $"Staff hủy đơn {order.OrderCode}"));
        }

        /// <summary>STF-05: đã nhận lại hàng: hoàn kho và chuyển sang chờ Manager hoàn tiền. Không trả lại lượt voucher.</summary>
        public static string? ConfirmReturnReceived(int orderId)
        {
            return ExecuteStaff(orderId, (context, order) =>
            {
                if (order.ReturnStatus != ReturnState.Requested) return "Đơn này không có yêu cầu hoàn hàng đang chờ.";

                order.ReturnStatus = ReturnState.Completed;
                if (order.PaymentStatus == PaymentState.Paid)
                {
                    order.PaymentStatus = PaymentState.RefundPending;
                }
                RestoreStock(context, order.OrderId);
                OrderLog.Add(context, $"Staff xác nhận đã nhận lại hàng đơn {order.OrderCode}");
                return null;
            });
        }

        /// <summary>STF-05: từ chối yêu cầu hoàn hàng.</summary>
        public static string? RejectReturn(int orderId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return "Vui lòng nhập lý do từ chối.";

            return ExecuteStaff(orderId, (context, order) =>
            {
                if (order.ReturnStatus != ReturnState.Requested) return "Đơn này không có yêu cầu hoàn hàng đang chờ.";

                order.ReturnStatus = ReturnState.Rejected;
                OrderLog.Add(context, $"Staff từ chối hoàn hàng đơn {order.OrderCode}: {reason.Trim()}");
                return null;
            });
        }

        // ===== QUẢN LÝ =====

        /// <summary>MNG-04: xác nhận đã chuyển khoản hoàn tiền thật cho khách.</summary>
        public static string? ConfirmRefund(int orderId)
        {
            return Execute(orderId, new[] { SessionGuard.RoleManager }, (context, order) =>
            {
                if (order.PaymentStatus != PaymentState.RefundPending) return "Đơn này không ở trạng thái chờ hoàn tiền.";

                bool stepOneDone = order.Status == OrderStatus.Cancelled || order.ReturnStatus == ReturnState.Completed;
                if (!stepOneDone) return "Staff chưa xác nhận đã nhận lại hàng nên chưa thể hoàn tiền.";

                order.PaymentStatus = PaymentState.Refunded;
                OrderLog.Add(context, $"Manager xác nhận hoàn tiền đơn {order.OrderCode}");
                return null;
            });
        }

        // ===== HÀM NỘI BỘ =====

        private static string? ExecuteStaff(int orderId, Func<DbSe2038BookymartFContext, TbOrder, string?> work)
        {
            return Execute(orderId, new[] { SessionGuard.RoleStaff }, work);
        }

        /// <summary>Khóa dòng đơn hàng trong transaction để hai người không xử lý cùng một đơn cùng lúc.</summary>
        private static string? Execute(int orderId, string[] roles, Func<DbSe2038BookymartFContext, TbOrder, string?> work)
        {
            if (!SessionGuard.HasRole(roles)) return "Tài khoản của bạn không có quyền thực hiện thao tác này.";

            using var context = new DbSe2038BookymartFContext();
            using var transaction = context.Database.BeginTransaction();

            var order = context.TbOrders
                .FromSqlInterpolated($"SELECT * FROM tbOrder WITH (UPDLOCK, ROWLOCK) WHERE order_id = {orderId}")
                .FirstOrDefault();
            if (order == null) return "Không tìm thấy đơn hàng.";

            string? error = work(context, order);
            if (error != null) return error;

            context.SaveChanges();
            transaction.Commit();
            return null;
        }

        private static string? Transition(TbOrder order, string newStatus)
        {
            if (!OrderStatus.CanTransition(order.Status, newStatus))
            {
                return $"Đơn {order.OrderCode} đang “{OrderStatus.ToText(order.Status)}”, "
                     + $"không thể chuyển sang “{OrderStatus.ToText(newStatus)}”.";
            }

            order.Status = newStatus;
            return null;
        }

        /// <summary>Hủy đơn: hoàn kho, trả lại lượt voucher và chuyển sang chờ hoàn tiền nếu khách đã trả tiền.</summary>
        private static string? ApplyCancel(DbSe2038BookymartFContext context, TbOrder order, string reason, string logAction)
        {
            string? error = Transition(order, OrderStatus.Cancelled);
            if (error != null) return error;

            order.CancelReason = reason.Length > 255 ? reason.Substring(0, 255) : reason;
            if (order.PaymentStatus == PaymentState.Paid)
            {
                order.PaymentStatus = PaymentState.RefundPending;
            }

            RestoreStock(context, order.OrderId);

            int orderId = order.OrderId;
            context.TbVoucherUsages.Where(u => u.OrderId == orderId).ExecuteDelete();

            OrderLog.Add(context, $"{logAction}: {reason}");
            return null;
        }

        private static void RestoreStock(DbSe2038BookymartFContext context, int orderId)
        {
            var details = context.TbOrderDetails.AsNoTracking()
                .Where(d => d.OrderId == orderId)
                .OrderBy(d => d.ProductId)
                .Select(d => new { d.ProductId, d.Quantity })
                .ToList();

            foreach (var detail in details)
            {
                int productId = detail.ProductId;
                int quantity = detail.Quantity;
                context.TbProducts.Where(p => p.ProductId == productId)
                    .ExecuteUpdate(s => s.SetProperty(p => p.StockQuantity, p => p.StockQuantity + quantity));
            }
        }
    }
}

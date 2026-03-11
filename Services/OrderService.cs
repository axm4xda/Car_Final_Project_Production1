using Car_Project.Data;
using Car_Project.Models;
using Car_Project.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Car_Project.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService         _cartService;

        public OrderService(ApplicationDbContext context, ICartService cartService)
        {
            _context     = context;
            _cartService = cartService;
        }

        public async Task<Order> PlaceOrderAsync(Order order, string sessionId)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            var cartItems = await _cartService.GetCartAsync(sessionId);
            if (!cartItems.Any())
                throw new InvalidOperationException("Səbət boşdur.");

            // Sifariş elementlərini səbətdən yarat
            var orderItems = cartItems.Select(ci => new OrderItem
            {
                ProductId  = ci.ProductId,
                Quantity   = ci.Quantity,
                UnitPrice  = ci.Product.Price,
                TotalPrice = ci.Product.Price * ci.Quantity,
                CreatedDate = DateTime.UtcNow
            }).ToList();

            order.Items      = orderItems;
            order.SubTotal   = orderItems.Sum(i => i.TotalPrice);
            order.Total      = order.SubTotal + order.ShippingCost - order.Discount;
            order.Status     = OrderStatus.Pending;
            order.CreatedDate = DateTime.UtcNow;

            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();

            // Stok azalt
            foreach (var item in cartItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product != null)
                    product.Stock = Math.Max(0, product.Stock - item.Quantity);
            }

            await _context.SaveChangesAsync();

            // Səbəti təmizlə
            await _cartService.ClearCartAsync(sessionId);

            return order;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IList<Order>> GetAllAdminAsync()
        {
            return await _context.Orders
                .AsNoTracking()
                .Include(o => o.Payment)
                .OrderByDescending(o => o.CreatedDate)
                .ToListAsync();
        }

        public async Task<(IList<Order> Items, int TotalCount)> GetPagedAdminAsync(
            int page, int pageSize = 15)
        {
            if (page < 1) page = 1;

            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Payment)
                .OrderByDescending(o => o.CreatedDate);

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task UpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId)
                ?? throw new KeyNotFoundException($"Id={orderId} olan sifariş tapılmadı.");

            order.Status = newStatus;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var order = await _context.Orders.FindAsync(id)
                ?? throw new KeyNotFoundException($"Id={id} olan sifariş tapılmadı.");

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetPendingCountAsync() =>
            await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);

        public async Task<List<(string ProductName, int Quantity, decimal UnitPrice)>> GetItemsForEmailAsync(int orderId)
        {
            return await _context.OrderItems
                .AsNoTracking()
                .Where(i => i.OrderId == orderId)
                .Include(i => i.Product)
                .Select(i => ValueTuple.Create(
                    i.Product != null ? i.Product.Name : "Product",
                    i.Quantity,
                    i.UnitPrice))
                .ToListAsync();
        }
    }
}

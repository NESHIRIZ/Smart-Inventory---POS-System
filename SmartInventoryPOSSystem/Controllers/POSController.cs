using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryPOSSystem.Data;
using SmartInventoryPOSSystem.Helpers;
using SmartInventoryPOSSystem.Models;
using SmartInventoryPOSSystem.ViewModels;

namespace SmartInventoryPOSSystem.Controllers;

[Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.Cashier)]
public class POSController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public POSController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var products = await _context.Products
            .Include(p => p.Category)
            .Where(p => string.IsNullOrWhiteSpace(search) || p.Name.Contains(search) || p.SKU.Contains(search))
            .OrderBy(p => p.Name)
            .ToListAsync();

        var cart = GetCart();
        return View(new POSViewModel
        {
            Products = products,
            CartItems = cart,
            SearchTerm = search ?? string.Empty,
            Customers = await _context.Customers.OrderBy(c => c.FullName).ToListAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(int productId)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product is null)
        {
            return NotFound();
        }

        var cart = GetCart();
        var cartItem = cart.FirstOrDefault(i => i.ProductId == productId);
        if (cartItem is null)
        {
            cart.Add(new POSCartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = 1
            });
        }
        else
        {
            cartItem.Quantity += 1;
        }

        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RemoveFromCart(int productId)
    {
        var cart = GetCart();
        var existing = cart.FirstOrDefault(i => i.ProductId == productId);
        if (existing != null)
        {
            cart.Remove(existing);
            SaveCart(cart);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(int? customerId, decimal discount, decimal paidAmount)
    {
        var cart = GetCart();
        if (cart.Count == 0)
        {
            TempData["Error"] = "Cart is empty.";
            return RedirectToAction(nameof(Index));
        }

        var totalAmount = cart.Sum(i => i.UnitPrice * i.Quantity);
        discount = Math.Max(0, discount);
        if (discount > totalAmount)
        {
            discount = totalAmount;
        }

        if (paidAmount < totalAmount - discount)
        {
            TempData["Error"] = "Paid amount must be equal to or greater than the final total.";
            return RedirectToAction(nameof(Index));
        }

        var userId = _userManager.GetUserId(User);
        var sale = new Sale
        {
            UserId = userId ?? string.Empty,
            CustomerId = customerId > 0 ? customerId : null,
            Discount = discount,
            PaidAmount = paidAmount,
            SaleDate = DateTime.UtcNow,
            ReceiptNumber = $"RCPT-{DateTime.UtcNow:yyyyMMddHHmmss}",
            TotalAmount = totalAmount - discount
        };

        _context.Sales.Add(sale);
        await _context.SaveChangesAsync();

        foreach (var item in cart)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product is null)
            {
                continue;
            }

            if (product.StockQuantity < item.Quantity) {
                TempData["Error"] = $"Insufficient stock for {product.Name}.";
                return RedirectToAction(nameof(Index));
            }

            product.StockQuantity -= item.Quantity;
            _context.SaleItems.Add(new SaleItem
            {
                SaleId = sale.Id,
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.UnitPrice * item.Quantity
            });

            _context.StockMovements.Add(new StockMovement
            {
                ProductId = item.ProductId,
                QuantityChange = -item.Quantity,
                MovementType = "Sale",
                Description = "Stock reduced after point of sale checkout.",
                Reference = sale.ReceiptNumber,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        HttpContext.Session.Remove("POS.Cart");
        TempData["Success"] = "Sale completed successfully.";
        return RedirectToAction(nameof(Index));
    }

    private List<POSCartItemViewModel> GetCart()
    {
        var cartJson = HttpContext.Session.GetString("POS.Cart");
        if (string.IsNullOrWhiteSpace(cartJson))
        {
            return new List<POSCartItemViewModel>();
        }

        return System.Text.Json.JsonSerializer.Deserialize<List<POSCartItemViewModel>>(cartJson) ?? new List<POSCartItemViewModel>();
    }

    private void SaveCart(List<POSCartItemViewModel> cart)
    {
        HttpContext.Session.SetString("POS.Cart", System.Text.Json.JsonSerializer.Serialize(cart));
    }
}

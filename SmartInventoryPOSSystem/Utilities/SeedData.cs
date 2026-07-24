using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using SmartInventoryPOSSystem.Data;
using SmartInventoryPOSSystem.Helpers;
using SmartInventoryPOSSystem.Models;

namespace SmartInventoryPOSSystem.Utilities;

public static class SeedData
{
    public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles = [RoleConstants.Admin, RoleConstants.Cashier];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string adminEmail = "admin@inventory.com";
        const string adminPassword = "Admin@123";

        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "System Administrator",
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);
            if (!createResult.Succeeded)
            {
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(adminUser, RoleConstants.Admin))
        {
            await userManager.AddToRoleAsync(adminUser, RoleConstants.Admin);
        }

        var allUsers = userManager.Users.ToList();
        foreach (var user in allUsers)
        {
            var userRoles = await userManager.GetRolesAsync(user);
            if (!userRoles.Any())
            {
                await userManager.AddToRoleAsync(user, RoleConstants.Admin);
            }
        }
    }

    public static async Task SeedSampleDataAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                new Category { Name = "Electronics", Description = "High-end electronics and accessories." },
                new Category { Name = "Office Supplies", Description = "Everyday office essentials." },
                new Category { Name = "Food & Beverage", Description = "Consumable goods and snacks." }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Suppliers.Any())
        {
            context.Suppliers.AddRange(
                new Supplier { Name = "Nexa Distribution", Email = "sales@nexa.com", PhoneNumber = "(555) 012-3456", Address = "123 Commerce Blvd" },
                new Supplier { Name = "Global Supplies Co.", Email = "info@globalsupplies.com", PhoneNumber = "(555) 987-6543", Address = "456 Enterprise Ave" },
                new Supplier { Name = "Fresh Market", Email = "orders@freshmarket.com", PhoneNumber = "(555) 555-0100", Address = "789 Market Street" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Customers.Any())
        {
            context.Customers.AddRange(
                new Customer { FullName = "John Doe", Email = "john.doe@example.com", PhoneNumber = "(555) 111-2222", Address = "10 Park Lane" },
                new Customer { FullName = "Acme Corp", Email = "purchasing@acmecorp.com", PhoneNumber = "(555) 333-4444", Address = "99 Industrial Parkway" },
                new Customer { FullName = "Tech Retailers", Email = "orders@techretailers.com", PhoneNumber = "(555) 666-7777", Address = "22 Commerce Road" }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Products.Any())
        {
            var electronicsCategory = context.Categories.First(c => c.Name == "Electronics");
            var officeCategory = context.Categories.First(c => c.Name == "Office Supplies");
            var foodCategory = context.Categories.First(c => c.Name == "Food & Beverage");

            var nexaSupplier = context.Suppliers.First(s => s.Name == "Nexa Distribution");
            var globalSupplier = context.Suppliers.First(s => s.Name == "Global Supplies Co.");
            var freshSupplier = context.Suppliers.First(s => s.Name == "Fresh Market");

            context.Products.AddRange(
                new Product { Name = "Wireless Barcode Scanner", SKU = "WB-200", Barcode = "100200300400", CategoryId = electronicsCategory.Id, SupplierId = nexaSupplier.Id, Price = 149.99m, CostPrice = 85.00m, StockQuantity = 24, Description = "Fast and reliable scanner for retail and inventory." },
                new Product { Name = "Thermal Receipt Printer", SKU = "TRP-350", Barcode = "200300400500", CategoryId = electronicsCategory.Id, SupplierId = globalSupplier.Id, Price = 249.99m, CostPrice = 160.00m, StockQuantity = 12, Description = "High-speed receipt printing for busy checkout lanes." },
                new Product { Name = "Office Label Rolls", SKU = "LBL-100", Barcode = "300400500600", CategoryId = officeCategory.Id, SupplierId = globalSupplier.Id, Price = 12.50m, CostPrice = 4.25m, StockQuantity = 120, Description = "Durable label rolls for all common label printers." },
                new Product { Name = "Coffee Pods", SKU = "CFP-010", Barcode = "400500600700", CategoryId = foodCategory.Id, SupplierId = freshSupplier.Id, Price = 9.99m, CostPrice = 4.50m, StockQuantity = 74, Description = "Premium espresso pods for staff and guests." }
            );
            await context.SaveChangesAsync();
        }

        if (!context.Sales.Any() && context.Products.Any())
        {
            var customer = context.Customers.First();
            var product1 = context.Products.First(p => p.SKU == "WB-200");
            var product2 = context.Products.First(p => p.SKU == "LBL-100");

            var sale = new Sale
            {
                UserId = string.Empty,
                CustomerId = customer.Id,
                Discount = 5.00m,
                PaidAmount = 157.49m,
                SaleDate = DateTime.UtcNow.AddDays(-1),
                ReceiptNumber = $"RCPT-{DateTime.UtcNow:yyyyMMddHHmmss}",
                TotalAmount = 157.49m
            };

            context.Sales.Add(sale);
            await context.SaveChangesAsync();

            var saleItem1 = new SaleItem
            {
                SaleId = sale.Id,
                ProductId = product1.Id,
                Quantity = 1,
                UnitPrice = product1.Price,
                TotalPrice = product1.Price
            };

            var saleItem2 = new SaleItem
            {
                SaleId = sale.Id,
                ProductId = product2.Id,
                Quantity = 1,
                UnitPrice = product2.Price,
                TotalPrice = product2.Price
            };

            product1.StockQuantity -= 1;
            product2.StockQuantity -= 1;

            context.SaleItems.AddRange(saleItem1, saleItem2);
            context.StockMovements.AddRange(
                new StockMovement { ProductId = product1.Id, QuantityChange = -1, MovementType = "Sale", Description = "Seed data sale", Reference = sale.ReceiptNumber, CreatedAt = DateTime.UtcNow.AddDays(-1) },
                new StockMovement { ProductId = product2.Id, QuantityChange = -1, MovementType = "Sale", Description = "Seed data sale", Reference = sale.ReceiptNumber, CreatedAt = DateTime.UtcNow.AddDays(-1) }
            );

            await context.SaveChangesAsync();
        }
    }
}

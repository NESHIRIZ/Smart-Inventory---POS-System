using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SmartInventoryPOSSystem.Data;
using SmartInventoryPOSSystem.Helpers;
using SmartInventoryPOSSystem.Interfaces;
using SmartInventoryPOSSystem.Models;
using SmartInventoryPOSSystem.ViewModels;

namespace SmartInventoryPOSSystem.Controllers;

[Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.Cashier)]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ApplicationDbContext _context;

    public ProductsController(IProductService productService, ApplicationDbContext context)
    {
        _productService = productService;
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var products = await _productService.SearchAsync(search ?? string.Empty);

        var model = new ProductListViewModel
        {
            SearchTerm = search,
            Products = products.Select(p => new ProductListItemViewModel
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Barcode = p.Barcode,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                CategoryName = p.Category?.Name ?? "-",
                SupplierName = p.Supplier?.Name ?? "-"
            })
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = await BuildProductFormViewModelAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (!await _productService.IsSkuUniqueAsync(model.SKU))
        {
            ModelState.AddModelError(nameof(ProductFormViewModel.SKU), "SKU must be unique.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(model);
            return View(model);
        }

        var product = new Product
        {
            Name = model.Name,
            SKU = model.SKU,
            Barcode = model.Barcode,
            Description = model.Description,
            Price = model.Price,
            CostPrice = model.CostPrice,
            StockQuantity = model.StockQuantity,
            ImageUrl = model.ImageUrl,
            CategoryId = model.CategoryId,
            SupplierId = model.SupplierId,
            CreatedAt = DateTime.UtcNow
        };

        await _productService.CreateAsync(product);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var product = await _productService.GetByIdAsync(id);
        if (product is null)
        {
            return NotFound();
        }

        var model = new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            SKU = product.SKU,
            Barcode = product.Barcode,
            Description = product.Description,
            Price = product.Price,
            CostPrice = product.CostPrice,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId,
            SupplierId = product.SupplierId
        };

        await PopulateSelectListsAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductFormViewModel model)
    {
        if (!await _productService.IsSkuUniqueAsync(model.SKU, model.Id))
        {
            ModelState.AddModelError(nameof(ProductFormViewModel.SKU), "SKU must be unique.");
        }

        if (!ModelState.IsValid)
        {
            await PopulateSelectListsAsync(model);
            return View(model);
        }

        var product = await _productService.GetByIdAsync(model.Id);
        if (product is null)
        {
            return NotFound();
        }

        product.Name = model.Name;
        product.SKU = model.SKU;
        product.Barcode = model.Barcode;
        product.Description = model.Description;
        product.Price = model.Price;
        product.CostPrice = model.CostPrice;
        product.StockQuantity = model.StockQuantity;
        product.ImageUrl = model.ImageUrl;
        product.CategoryId = model.CategoryId;
        product.SupplierId = model.SupplierId;

        await _productService.UpdateAsync(product);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _productService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private async Task<ProductFormViewModel> BuildProductFormViewModelAsync()
    {
        var model = new ProductFormViewModel();
        await PopulateSelectListsAsync(model);
        return model;
    }

    private async Task PopulateSelectListsAsync(ProductFormViewModel model)
    {
        model.Categories = await _context.Categories
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
            .ToListAsync();

        model.Suppliers = await _context.Suppliers
            .OrderBy(s => s.Name)
            .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
            .ToListAsync();
    }
}

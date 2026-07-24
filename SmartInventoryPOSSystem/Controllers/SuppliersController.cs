using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryPOSSystem.Data;
using SmartInventoryPOSSystem.Helpers;
using SmartInventoryPOSSystem.Models;
using SmartInventoryPOSSystem.ViewModels;

namespace SmartInventoryPOSSystem.Controllers;

[Authorize(Roles = RoleConstants.Admin)]
public class SuppliersController : Controller
{
    private readonly ApplicationDbContext _context;

    public SuppliersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Suppliers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(s => s.Name.Contains(search) || s.Email!.Contains(search));
        }

        var suppliers = await query.OrderBy(s => s.Name).ToListAsync();
        return View(suppliers);
    }

    public IActionResult Create() => View(new SupplierViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.Suppliers.Add(new Supplier
        {
            Name = model.Name,
            PhoneNumber = model.PhoneNumber,
            Email = model.Email,
            Address = model.Address,
            ContactDetails = model.ContactDetails,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null)
        {
            return NotFound();
        }

        return View(new SupplierViewModel
        {
            Id = supplier.Id,
            Name = supplier.Name,
            PhoneNumber = supplier.PhoneNumber,
            Email = supplier.Email,
            Address = supplier.Address,
            ContactDetails = supplier.ContactDetails
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SupplierViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var supplier = await _context.Suppliers.FindAsync(model.Id);
        if (supplier is null)
        {
            return NotFound();
        }

        supplier.Name = model.Name;
        supplier.PhoneNumber = model.PhoneNumber;
        supplier.Email = model.Email;
        supplier.Address = model.Address;
        supplier.ContactDetails = model.ContactDetails;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var supplier = await _context.Suppliers.FindAsync(id);
        if (supplier is null)
        {
            return NotFound();
        }

        _context.Suppliers.Remove(supplier);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

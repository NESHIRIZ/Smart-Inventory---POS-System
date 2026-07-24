using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartInventoryPOSSystem.Data;
using SmartInventoryPOSSystem.Helpers;
using SmartInventoryPOSSystem.Models;
using SmartInventoryPOSSystem.ViewModels;

namespace SmartInventoryPOSSystem.Controllers;

[Authorize(Roles = RoleConstants.Admin + "," + RoleConstants.Cashier)]
public class CustomersController : Controller
{
    private readonly ApplicationDbContext _context;

    public CustomersController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var query = _context.Customers.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.FullName.Contains(search) || c.PhoneNumber!.Contains(search));
        }

        var customers = await query.OrderBy(c => c.FullName).ToListAsync();
        return View(customers);
    }

    public IActionResult Create() => View(new CustomerViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CustomerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        _context.Customers.Add(new Customer
        {
            FullName = model.FullName,
            PhoneNumber = model.PhoneNumber,
            Email = model.Email,
            Address = model.Address,
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        return View(new CustomerViewModel
        {
            Id = customer.Id,
            FullName = customer.FullName,
            PhoneNumber = customer.PhoneNumber,
            Email = customer.Email,
            Address = customer.Address
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CustomerViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var customer = await _context.Customers.FindAsync(model.Id);
        if (customer is null)
        {
            return NotFound();
        }

        customer.FullName = model.FullName;
        customer.PhoneNumber = model.PhoneNumber;
        customer.Email = model.Email;
        customer.Address = model.Address;
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer is null)
        {
            return NotFound();
        }

        _context.Customers.Remove(customer);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OzSapkaTShirt.Data;
using OzSapkaTShirt.Models;

namespace OzSapkaTShirt.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : Controller
    {
        public class ReportModel
        {
            public DateTime? Start { get; set; }
            public DateTime? End { get; set; }
            public string? UserId { get; set; }
            public long? ProductId { get; set; }
        }
        private readonly ApplicationContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index()
        {
            ViewData["Customers"] = new SelectList(_userManager.Users, "Id", "Name");
            ViewData["Products"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // GET: Admin/Orders/Details/5
        public async Task<IActionResult> Report([Bind("Start,End,UserId,ProductId")] ReportModel reportModel)
        {
            IQueryable<Order> orders = _context.Orders;
            if (reportModel.Start != null)
            {
                orders = orders.Where(o => o.OrderDate >= reportModel.Start.Value);
            }
            if (reportModel.End != null)
            {
                orders = orders.Where(o => o.OrderDate <= reportModel.End.Value);
            }
            if (reportModel.UserId != null)
            {
                orders = orders.Where(o => o.UserId == reportModel.UserId);
            }
            //if (reportModel.ProductId != null)
            //{
            //    orders = orders.Where(o => o.UserId == reportModel.UserId);
            //}
            orders = orders.Include(o => o.OrderProducts).ThenInclude(op => op.Product);
            return View(orders.ToList());
        }

        // GET: Admin/Orders/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Admin/Orders/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,OrderDate,TotalPrice,Status")] Order order)
        {
            if (ModelState.IsValid)
            {
                _context.Add(order);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // POST: Admin/Orders/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, [Bind("Id,UserId,OrderDate,TotalPrice,Status")] Order order)
        {
            if (id != order.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(order);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderExists(order.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || _context.Orders == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // POST: Admin/Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (_context.Orders == null)
            {
                return Problem("Entity set 'ApplicationContext.Orders'  is null.");
            }
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                _context.Orders.Remove(order);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderExists(long id)
        {
          return (_context.Orders?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

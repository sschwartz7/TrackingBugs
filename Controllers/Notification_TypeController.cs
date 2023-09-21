using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TrackingBugs.Data;
using TrackingBugs.Models;

namespace TrackingBugs.Controllers
{
    [Authorize]
    public class Notification_TypeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public Notification_TypeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Notification_Type
        public async Task<IActionResult> Index()
        {
              return _context.NotificationTypes != null ? 
                          View(await _context.NotificationTypes.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Notification_Type'  is null.");
        }

        // GET: Notification_Type/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.NotificationTypes == null)
            {
                return NotFound();
            }

            var notification_Type = await _context.NotificationTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification_Type == null)
            {
                return NotFound();
            }

            return View(notification_Type);
        }

        // GET: Notification_Type/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Notification_Type/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] NotificationType notification_Type)
        {
            if (ModelState.IsValid)
            {
                _context.Add(notification_Type);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(notification_Type);
        }

        // GET: Notification_Type/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.NotificationTypes == null)
            {
                return NotFound();
            }

            var notification_Type = await _context.NotificationTypes.FindAsync(id);
            if (notification_Type == null)
            {
                return NotFound();
            }
            return View(notification_Type);
        }

        // POST: Notification_Type/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] NotificationType notification_Type)
        {
            if (id != notification_Type.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(notification_Type);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!Notification_TypeExists(notification_Type.Id))
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
            return View(notification_Type);
        }

        // GET: Notification_Type/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.NotificationTypes == null)
            {
                return NotFound();
            }

            var notification_Type = await _context.NotificationTypes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (notification_Type == null)
            {
                return NotFound();
            }

            return View(notification_Type);
        }

        // POST: Notification_Type/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.NotificationTypes == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Notification_Type'  is null.");
            }
            var notification_Type = await _context.NotificationTypes.FindAsync(id);
            if (notification_Type != null)
            {
                _context.NotificationTypes.Remove(notification_Type);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool Notification_TypeExists(int id)
        {
          return (_context.NotificationTypes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}

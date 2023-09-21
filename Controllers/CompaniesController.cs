using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TrackingBugs.Data;
using TrackingBugs.Extensions;
using TrackingBugs.Models;
using TrackingBugs.Models.ViewModels;
using TrackingBugs.Services;
using TrackingBugs.Services.Interfaces;

namespace TrackingBugs.Controllers
{
    public class CompaniesController : BTBaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyService _companyService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRoleService _roleService;



        public CompaniesController(ApplicationDbContext context, IBTCompanyService companyService, UserManager<BTUser> userManager, IBTRoleService roleService)
        {
            _context = context;
            _companyService = companyService;
            _userManager = userManager;
            _roleService = roleService;
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
              return _context.Companies != null ? 
                          View(await _context.Companies.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Company'  is null.");
        }

        // GET: Companies/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            Company company = await _companyService.GetCompanyInfoAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageFileData,ImageFileType")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles ="Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageFileData,ImageFileType")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Companies == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Company'  is null.");
            }
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
          return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUserRoles()
        {
            // 1 - Add an instance of the ViewModel as a List (model)
            List<ManageUserRolesViewModel> model = new List<ManageUserRolesViewModel>();
            // 3 - Get all company users
            IEnumerable<BTUser> members = await _companyService.GetMembersAsync(_companyId);
            // 4 - Loop over the users to populate the ViewModel
            //      - instantiate single ViewModel
            //      - use _rolesService
            //      - Create multiselect
            //      - viewmodel to model
            string? btUserId = _userManager.GetUserId(User);

            foreach(BTUser member in members)
            {
                if(string.Compare(btUserId, member.Id)!= 0)
                {
                    ManageUserRolesViewModel viewModel = new();
                    IEnumerable<string>? currentRoles = await _roleService.GetUserRolesAsync(member);

                    viewModel.BTUser = member;
                    viewModel.Roles = new MultiSelectList(await _roleService.GetRolesAsync(),"Name", "Name", currentRoles);

                    model.Add(viewModel);
                }
            }

            // 5 - Return the model to the View
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
        {
            // 1- Get the company Id
            // 2 - Instantiate the BTUser
            BTUser? bTUser = (await _companyService.GetMembersAsync(_companyId)).FirstOrDefault(m => m.Id == viewModel.BTUser?.Id);
            // 3 - Get Roles for the User
            IEnumerable<string>? currentRoles = await _roleService.GetUserRolesAsync(bTUser);
            // 4 - Get Selected Role(s) for the User
            string? selectedRole = viewModel.SelectedRoles!.FirstOrDefault();
            // 5 - Remove current role(s) and Add new role
            if (!string.IsNullOrEmpty(selectedRole))
            {
                if(await _roleService.RemoveUserFromRolesAsync(bTUser, currentRoles))
                {
                    await _roleService.AddUserToRoleAsync(bTUser, selectedRole);
                }
            }
            // 6 - Navigate
            return RedirectToAction(nameof(ManageUserRoles));
        }
    }
}

using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using TrackingBugs.Data;
using TrackingBugs.Models;
using TrackingBugs.Services.Interfaces;

namespace TrackingBugs.Services
{
    public class BTCompanyService : IBTCompanyService

    {
        private readonly ApplicationDbContext _context;

        public BTCompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Company> GetCompanyInfoAsync(int? companyId)
        {
            Company? result = new();
            try
            {
                if (companyId == null) { return result; }
                result = await _context.Companies
                    .Include(c => c.Members)
                    .Include(c => c.Invites)
                    .Include(c => c.Projects)
                    .ThenInclude(p => p.Tickets)
                    .FirstOrDefaultAsync(c => c.Id == companyId);
                return result!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Invite>> GetInvitesAsync(int? companyId)
        {
            List<Invite>? result = new();
            try
            {
                if (companyId == null) { return result; }
                result = await _context.Invites.Where(u => u.CompanyId == companyId).ToListAsync();
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetMembersAsync(int? companyId)
        {
            List<BTUser>? result = new ();
            try
            {
                if(companyId == null) { return result; }
                result = await _context.Users.Where(u => u.CompanyId == companyId).ToListAsync();
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Project>> GetProjectsAsync(int? companyId)
        {
            List<Project>? result = new();
            try
            {
                if (companyId == null) { return result; }
                result = await _context.Projects.Where(u => u.CompanyId == companyId).ToListAsync();
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

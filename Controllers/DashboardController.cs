using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Electroscann_ai.Data;
using Electroscann_ai.Models;
using ElectroScanAI.Models.Enums;
using ElectroScanAI.Models.Entities;

namespace Electroscann_ai.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly ElectroscannDbContext _context;

        public DashboardController(ElectroscannDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalElectricians = await _context.Electricians.CountAsync(),
                TotalCompanies = await _context.Companies.CountAsync(),
                TotalJobs = await _context.Jobs.CountAsync(),
                TotalRevenue = (decimal)await _context.Payments.Where(p => p.Status == PaymentStatus.Paid).Select(p => (double)p.Amount).SumAsync(),
                PendingApplications = await _context.JobApplications.Where(ja => ja.Status == "Pending").CountAsync()
            };
            return View("index", viewModel);
        }

        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Include(u => u.ElectricianProfile)
                .Include(u => u.CompanyProfile)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            return View("Users", users);
        }

        public async Task<IActionResult> Electricians()
        {
            var electricians = await _context.Electricians
                .Include(e => e.User)
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();
            return View("Electricians", electricians);
        }

        public async Task<IActionResult> Companies()
        {
            var companies = await _context.Companies
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
            return View("companies", companies);
        }

        public async Task<IActionResult> Jobs()
        {
            var jobs = await _context.Jobs
                .Include(j => j.Company)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
            return View("jobs", jobs);
        }

        public async Task<IActionResult> Subscriptions() // FIXED: Added this action
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.User)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
            return View("subscriptions", subscriptions);
        }

        public async Task<IActionResult> AIScans()
        {
            var scans = await _context.AIScans
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(50)
                .ToListAsync();
            return View("ai_scans", scans);
        }

        public async Task<IActionResult> Revenue()
        {
            var revenue = await _context.Payments
                .Where(p => p.Status == PaymentStatus.Paid)
                .GroupBy(p => new { p.PaymentDate.Year, p.PaymentDate.Month })
                .Select(g => new MonthlyRevenue
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = (decimal)g.Sum(p => (double)p.Amount)
                })
                .OrderByDescending(r => r.Year)
                .ThenByDescending(r => r.Month)
                .ToListAsync();
            return View("revenue", revenue);
        }

        public async Task<IActionResult> Messages() // FIXED: Spelling
        {
            var messages = await _context.ContactMessages
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return View("messages", messages);
        }

        public IActionResult Settings()
        {
            return View("settings");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDto model)
        {
            if (model == null) return Json(new { success = false, message = "Invalid data." });

            var user = await _context.Users.FindAsync(model.Id);
            if (user == null) return Json(new { success = false, message = "User not found." });

            user.FullName = model.FullName ?? user.FullName;
            user.Email = model.Email ?? user.Email;
            if (Enum.TryParse<UserRole>(model.Role, out var parsedRole))
            {
                user.Role = parsedRole;
            }
            user.IsActive = model.Status?.Equals("Active", StringComparison.OrdinalIgnoreCase) ?? user.IsActive;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) return Json(new { success = false, message = "User not found." });

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        public class UpdateUserDto
        {
            public int Id { get; set; }
            public string FullName { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
        }

        public class AdminDashboardViewModel
        {
            public int TotalUsers { get; set; }
            public int TotalElectricians { get; set; }
            public int TotalCompanies { get; set; }
            public int TotalJobs { get; set; }
            public decimal TotalRevenue { get; set; }
            public int PendingApplications { get; set; }
        }

        public class MonthlyRevenue
        {
            public int Year { get; set; }
            public int Month { get; set; }
            public decimal Amount { get; set; }
        }
    }
}

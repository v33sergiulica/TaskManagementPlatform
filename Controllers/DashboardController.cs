using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementPlatform.Data;
using TaskManagementPlatform.Models;
using TaskManagementPlatform.Models.ViewModels;

namespace TaskManagementPlatform.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var viewModel = new DashboardViewModel();

            viewModel.MyProjects = await _context.Projects
                .Include(p => p.Organizer)
                .Include(p => p.Members)
                .Where(p => p.OrganizerId == user.Id || p.Members.Any(m => m.UserId == user.Id))
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            viewModel.MyTasks = await _context.Tasks
                .Include(t => t.Project)
                .Where(t => t.AssignedToUserId == user.Id)
                .OrderBy(t => t.EndDate)
                .ToListAsync();

            var threeDaysFromNow = DateTime.Now.AddDays(3);
            viewModel.UpcomingDeadlines = viewModel.MyTasks
                .Where(t => t.Status != Models.TaskStatus.Completed && 
                            t.EndDate > DateTime.Now && 
                            t.EndDate <= threeDaysFromNow)
                .ToList();

            return View(viewModel);
        }
    }
}

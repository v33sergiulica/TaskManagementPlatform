using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementPlatform.Data;
using TaskManagementPlatform.Models;

namespace TaskManagementPlatform.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TaskManagementPlatform.Services.IOpenAIService _openAIService;

        public ProjectController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, TaskManagementPlatform.Services.IOpenAIService openAIService)
        {
            _context = context;
            _userManager = userManager;
            _openAIService = openAIService;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizer,Administrator")]
        public async Task<IActionResult> GenerateSummary(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == projectId);

            if (project == null) return NotFound();

            var result = await _openAIService.GenerateProjectSummaryAsync(project);
            if (result.Success)
            {
                project.AISummary = result.Content;
                project.AISummaryGeneratedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["Success"] = "AI Summary generated successfully!";
            }
            else
            {
                TempData["Error"] = "AI Error: " + result.ErrorMessage;
            }

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var project = await _context.Projects
                .Include(p => p.Organizer)
                .Include(p => p.Members)
                .ThenInclude(pm => pm.User)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var isMember = project.Members.Any(pm => pm.UserId == currentUser.Id);
            var isOrganizer = project.OrganizerId == currentUser.Id;
            var isAdmin = await _userManager.IsInRoleAsync(currentUser, "Administrator");

            if (!isMember && !isOrganizer && !isAdmin)
            {
                return Forbid();
            }

            return View(project);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Organizer,Administrator")]
        public async Task<IActionResult> AddMember(int projectId, string email)
        {
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            if (project.OrganizerId != currentUser.Id && !User.IsInRole("Administrator"))
            {
                return Forbid();
            }

            var userToAdd = await _userManager.FindByEmailAsync(email);
            if (userToAdd == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(Details), new { id = projectId });
            }

            var exists = await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userToAdd.Id);
            if (!exists)
            {
                var member = new ProjectMember { ProjectId = projectId, UserId = userToAdd.Id };
                _context.ProjectMembers.Add(member);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Member added successfully.";
            }
            else
            {
                TempData["Error"] = "User is already a member.";
            }

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        public async Task<IActionResult> Index()
        {
            var projects = await _context.Projects.Include(p => p.Organizer).ToListAsync();
            return View(projects);
        }

        [Authorize]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Project project)
        {
            ModelState.Remove("Organizer");
            ModelState.Remove("OrganizerId");
            ModelState.Remove("Tasks");

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                project.OrganizerId = user.Id;
                project.CreatedDate = DateTime.Now;
                
                if (!await _userManager.IsInRoleAsync(user, "Organizer"))
                {
                    await _userManager.AddToRoleAsync(user, "Organizer");
                }

                _context.Add(project);
                await _context.SaveChangesAsync();

                var member = new ProjectMember
                {
                    ProjectId = project.Id,
                    UserId = user.Id
                };
                _context.ProjectMembers.Add(member);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            return View(project);
        }
    }
}

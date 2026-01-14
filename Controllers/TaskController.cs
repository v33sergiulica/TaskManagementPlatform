using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TaskManagementPlatform.Data;
using TaskManagementPlatform.Models;

namespace TaskManagementPlatform.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TaskController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task<bool> UserHasAccessToProject(int projectId)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return false;
            
            if (await _userManager.IsInRoleAsync(currentUser, "Administrator")) return true;

            var isMember = await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == currentUser.Id);
            if (isMember) return true;

            var project = await _context.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId);
            return project != null && project.OrganizerId == currentUser.Id;
        }

        [Authorize]
        public async Task<IActionResult> Create(int? projectId)
        {
            if (projectId == null) return NotFound();

            if (!await UserHasAccessToProject(projectId.Value)) return Forbid();

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            var members = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == projectId)
                .Include(pm => pm.User)
                .Select(pm => pm.User)
                .ToListAsync();

            ViewBag.Members = new SelectList(members, "Id", "UserName");
            ViewBag.ProjectTitle = project.Title;

            var task = new Models.Task
            {
                ProjectId = projectId.Value,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(1)
            };
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Models.Task task)
        {
            ModelState.Remove("Project");
            ModelState.Remove("AssignedTo");

            if (!await UserHasAccessToProject(task.ProjectId)) return Forbid();

            if (task.EndDate <= task.StartDate)
            {
                ModelState.AddModelError("EndDate", "End Date must be greater than Start Date.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Project", new { id = task.ProjectId });
            }

            var project = await _context.Projects.FindAsync(task.ProjectId);
            ViewBag.ProjectTitle = project?.Title;

            var members = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == task.ProjectId)
                .Include(pm => pm.User)
                .Select(pm => pm.User)
                .ToListAsync();
            ViewBag.Members = new SelectList(members, "Id", "UserName", task.AssignedToUserId);
            
            return View(task);
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return NotFound();

            if (!await UserHasAccessToProject(task.ProjectId)) return Forbid();

            var members = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == task.ProjectId)
                .Include(pm => pm.User)
                .Select(pm => pm.User)
                .ToListAsync();
            ViewBag.Members = new SelectList(members, "Id", "UserName", task.AssignedToUserId);

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Models.Task task)
        {
            if (id != task.Id) return NotFound();

            if (!await UserHasAccessToProject(task.ProjectId)) return Forbid();

            ModelState.Remove("Project");
            ModelState.Remove("AssignedTo");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(task);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Tasks.Any(e => e.Id == task.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction("Details", "Project", new { id = task.ProjectId });
            }
            
            var members = await _context.ProjectMembers
                .Where(pm => pm.ProjectId == task.ProjectId)
                .Include(pm => pm.User)
                .Select(pm => pm.User)
                .ToListAsync();
            ViewBag.Members = new SelectList(members, "Id", "UserName", task.AssignedToUserId);

            return View(task);
        }

        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var task = await _context.Tasks
                .Include(t => t.Project)
                .Include(t => t.AssignedTo)
                .Include(t => t.Comments)
                .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (task == null) return NotFound();

            if (!await UserHasAccessToProject(task.ProjectId)) return Forbid();

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> AddComment(int taskId, string content)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return NotFound();

            if (!await UserHasAccessToProject(task.ProjectId)) return Forbid();

            if (!string.IsNullOrWhiteSpace(content))
            {
                var user = await _userManager.GetUserAsync(User);
                var comment = new Comment
                {
                    TaskId = taskId,
                    UserId = user.Id,
                    Content = content,
                    CreatedDate = DateTime.Now
                };
                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = taskId });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                var projectId = task.ProjectId;
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Project", new { id = projectId });
            }
            return RedirectToAction("Index", "Project");
        }
    }
}

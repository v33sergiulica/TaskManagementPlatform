using TaskManagementPlatform.Models;

namespace TaskManagementPlatform.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<Project> MyProjects { get; set; } = new List<Project>();
        public List<Task> MyTasks { get; set; } = new List<Task>();
        public List<Task> UpcomingDeadlines { get; set; } = new List<Task>();
    }
}

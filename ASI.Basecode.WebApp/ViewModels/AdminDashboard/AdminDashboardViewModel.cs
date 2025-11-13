namespace ASI.Basecode.WebApp.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int StudentCount { get; set; }
        public int TeacherCount { get; set; }
        public int AdminCount { get; set; }

        public int TotalClasses { get; set; }
        public int ActiveClasses { get; set; }
        public int InactiveClasses { get; set; }
    }
}
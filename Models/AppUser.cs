namespace Real_Time_Stock_Price_Monitor.Models
{
    public class AppUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;   
        public string DisplayName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;        
        public string PasswordHash { get; set; } = string.Empty;
    }
}

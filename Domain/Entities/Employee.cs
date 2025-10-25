namespace Domain.Entities
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? FatherName { get; set; }
        public string? Post { get; set; }
        public string? Status { get; set; }
        public DateTime LastEditTime { get; set; }
        public bool IsDeleted { get; set; }
        // ссылки 
        public string? UserId { get; set; }
        public User? User { get; set; }
        public List<EmployeeRole> EmployeeRoles { get; set; } = new();
    }
}
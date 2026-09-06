namespace EcomCourse.Application.Authentication.DTOs
{
    public class ApplicationUserDto
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public Guid CustomerId { get; set; }
        public string? Password { get; set; }
    }
}

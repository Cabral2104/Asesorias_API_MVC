namespace Asesorias_API_MVC.Models.Dtos
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
        public string PhoneNumber { get; set; }
    }
}

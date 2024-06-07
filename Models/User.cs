namespace OptimazedCvStorage.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string Username { get; set; }
       // public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }

        public PersonalInfo PersonalInfo { get; set; }
        public ICollection<Education> Educations { get; set; }
        public ICollection<WorkExperience> WorkExperiences { get; set; }
        public ICollection<Skill> Skills { get; set; }
        public ICollection<Certification> Certifications { get; set; }
    }
}

namespace OptimazedCvStorage.Models
{
    public class WorkExperience
{
    public int WorkExperienceID { get; set; }
    public int UserID { get; set; }
    public string CompanyName { get; set; }
    public string Position { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Responsibilities { get; set; }

    public User User { get; set; }
}
}

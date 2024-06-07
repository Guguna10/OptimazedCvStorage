namespace OptimazedCvStorage.Models
{
    public class Education
    {
        public int EducationID { get; set; }
        public int UserID { get; set; }
        public string InstitutionName { get; set; }
        public string Degree { get; set; }
        public string FieldOfStudy { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public User User { get; set; }
    }
}

namespace OptimazedCvStorage.Models
{
    public class Certification
    {
        public int CertificationID { get; set; }
        public int UserID { get; set; }
        public string CertificationName { get; set; }
        public string IssuingOrganization { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime ExpiryDate { get; set; }

        public User User { get; set; }
    } 
}

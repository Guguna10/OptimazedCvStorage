namespace OptimazedCvStorage.DTOs
{
    public class FullCvDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; } // Nullable DateTime
        public string SkillName { get; set; }
        public string SkillLevel { get; set; }
        public string InstitutionName { get; set; }
        public string Degree { get; set; }
        public string FieldOfStudy { get; set; }
        public DateTime? StartDate { get; set; } // Nullable DateTime
        public DateTime? EndDate { get; set; } // Nullable DateTime
        public string CertificationName { get; set; }
        public string IssuingOrganization { get; set; }
        public DateTime? IssueDate { get; set; } // Nullable DateTime
        public DateTime? ExpiryDate { get; set; } // Nullable DateTime
        public string CompanyName { get; set; }
        public string Position { get; set; }
        public string Responsibilities { get; set; }
    }

}

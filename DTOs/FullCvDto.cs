namespace OptimazedCvStorage.DTOs
{
    public class FullCvDto
    {
        public int UserID { get; set; } // Added UserID
        public int PersonalInfoID { get; set; } // Added PersonalInfoID
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public List<EducationDto> Educations { get; set; }
        public List<CertificationDto> Certifications { get; set; }
        public List<SkillDto> Skills { get; set; }
        public List<WorkExperienceDto> WorkExperiences { get; set; }
    }

    public class EducationDto
    {
        public int EducationID { get; set; } // Added EducationID
        public string InstitutionName { get; set; }
        public string Degree { get; set; }
        public string FieldOfStudy { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CertificationDto
    {
        public int CertificationID { get; set; } // Added CertificationID
        public string CertificationName { get; set; }
        public string IssuingOrganization { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }

    public class SkillDto
    {
        public int SkillID { get; set; } // Added SkillID
        public string SkillName { get; set; }
        public string SkillLevel { get; set; }
    }

    public class WorkExperienceDto
    {
        public int WorkExperienceID { get; set; } // Added WorkExperienceID
        public string CompanyName { get; set; }
        public string Position { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Responsibilities { get; set; }
    }
}
public class FullCvDto
{
    public int UserID { get; set; } // Added UserID
    public int PersonalInfoID { get; set; } // Added PersonalInfoID
    public string Username { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public List<EducationDto> Educations { get; set; }
    public List<CertificationDto> Certifications { get; set; }
    public List<SkillDto> Skills { get; set; }
    public List<WorkExperienceDto> WorkExperiences { get; set; }
}

public class EducationDto
{
    public int EducationID { get; set; } // Added EducationID
    public string InstitutionName { get; set; }
    public string Degree { get; set; }
    public string FieldOfStudy { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class CertificationDto
{
    public int CertificationID { get; set; } // Added CertificationID
    public string CertificationName { get; set; }
    public string IssuingOrganization { get; set; }
    public DateTime? IssueDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class SkillDto
{
    public int SkillID { get; set; } // Added SkillID
    public string SkillName { get; set; }
    public string SkillLevel { get; set; }
}

public class WorkExperienceDto
{
    public int WorkExperienceID { get; set; } // Added WorkExperienceID
    public string CompanyName { get; set; }
    public string Position { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Responsibilities { get; set; }
}
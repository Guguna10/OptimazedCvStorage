using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using OptimazedCvStorage.Models;
using System.Data;
using OptimazedCvStorage.DTOs;


using Microsoft.EntityFrameworkCore;
using OptimazedCvStorage.Data;

namespace OptimazedCvStorage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CVContext _context;

        public UsersController(CVContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<JsonResult> GetAllCvs()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                var allCvs = new List<FullCvDto>();

                foreach (var user in users)
                {
                    var personalInfo = await _context.PersonalInfo.FirstOrDefaultAsync(pi => pi.UserID == user.UserID);
                    var educations = await _context.Education.Where(e => e.UserID == user.UserID).ToListAsync();
                    var certifications = await _context.Certifications.Where(c => c.UserID == user.UserID).ToListAsync();
                    var skills = await _context.Skills.Where(s => s.UserID == user.UserID).ToListAsync();
                    var workExperiences = await _context.WorkExperience.Where(we => we.UserID == user.UserID).ToListAsync();

                    var fullCvDto = new FullCvDto
                    {
                        UserID = user.UserID,
                        PersonalInfoID = personalInfo.PersonalInfoID,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = personalInfo?.FullName,
                        Address = personalInfo?.Address,
                        PhoneNumber = personalInfo?.PhoneNumber,
                        DateOfBirth = personalInfo?.DateOfBirth,
                        Educations = educations.Select(e => new EducationDto
                        {
                            EducationID = e.EducationID,
                            InstitutionName = e.InstitutionName,
                            Degree = e.Degree,
                            FieldOfStudy = e.FieldOfStudy,
                            StartDate = e.StartDate,
                            EndDate = e.EndDate
                        }).ToList(),
                        Certifications = certifications.Select(c => new CertificationDto
                        {
                            CertificationID = c.CertificationID,
                            CertificationName = c.CertificationName,
                            IssuingOrganization = c.IssuingOrganization,
                            IssueDate = c.IssueDate,
                            ExpiryDate = c.ExpiryDate
                        }).ToList(),
                        Skills = skills.Select(s => new SkillDto
                        {
                            SkillID = s.SkillID,
                            SkillName = s.SkillName,
                            SkillLevel = s.SkillLevel
                        }).ToList(),
                        WorkExperiences = workExperiences.Select(we => new WorkExperienceDto
                        {
                            WorkExperienceID = we.WorkExperienceID,
                            CompanyName = we.CompanyName,
                            Position = we.Position,
                            StartDate = we.StartDate,
                            EndDate = we.EndDate,
                            Responsibilities = we.Responsibilities
                        }).ToList()
                    };

                    allCvs.Add(fullCvDto);
                }

                return new JsonResult(allCvs);
            }
            catch (Exception ex)
            {
                return new JsonResult("Error: " + ex.Message);
            }
        }


        [HttpPost]
        public async Task<JsonResult> Post(FullCvDto fullCvDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = new User
                {
                    Username = fullCvDto.Username,
                    Email = fullCvDto.Email,
                    CreatedAt = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                var personalInfo = new PersonalInfo
                {
                    UserID = user.UserID,
                    FullName = fullCvDto.FullName,
                    Address = fullCvDto.Address,
                    PhoneNumber = fullCvDto.PhoneNumber,
                    DateOfBirth = fullCvDto.DateOfBirth ?? DateTime.MinValue // Handle nullable DateTime
                };

                _context.PersonalInfo.Add(personalInfo);

                foreach (var educationDto in fullCvDto.Educations)
                {
                    var education = new Education
                    {
                        UserID = user.UserID,
                        InstitutionName = educationDto.InstitutionName,
                        Degree = educationDto.Degree,
                        FieldOfStudy = educationDto.FieldOfStudy,
                        StartDate = educationDto.StartDate ?? DateTime.MinValue,
                        EndDate = educationDto.EndDate ?? DateTime.MinValue
                    };

                    _context.Education.Add(education);
                }

                foreach (var certificationDto in fullCvDto.Certifications)
                {
                    var certification = new Certification
                    {
                        UserID = user.UserID,
                        CertificationName = certificationDto.CertificationName,
                        IssuingOrganization = certificationDto.IssuingOrganization,
                        IssueDate = certificationDto.IssueDate ?? DateTime.MinValue,
                        ExpiryDate = certificationDto.ExpiryDate ?? DateTime.MinValue
                    };

                    _context.Certifications.Add(certification);
                }

                foreach (var skillDto in fullCvDto.Skills)
                {
                    var skill = new Skill
                    {
                        UserID = user.UserID,
                        SkillName = skillDto.SkillName,
                        SkillLevel = skillDto.SkillLevel
                    };

                    _context.Skills.Add(skill);
                }

                foreach (var workExperienceDto in fullCvDto.WorkExperiences)
                {
                    var workExperience = new WorkExperience
                    {
                        UserID = user.UserID,
                        CompanyName = workExperienceDto.CompanyName,
                        Position = workExperienceDto.Position,
                        StartDate = workExperienceDto.StartDate ?? DateTime.MinValue,
                        EndDate = workExperienceDto.EndDate ?? DateTime.MinValue,
                        Responsibilities = workExperienceDto.Responsibilities
                    };

                    _context.WorkExperience.Add(workExperience);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new JsonResult("Added Successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new JsonResult("Error: " + ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<JsonResult> Update(int id, FullCvDto fullCvDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return new JsonResult("User not found");
                }

                // Update user properties
                user.Username = fullCvDto.Username;
                user.Email = fullCvDto.Email;

                // Update personal info
                var personalInfo = await _context.PersonalInfo.FirstOrDefaultAsync(pi => pi.UserID == id);
                if (personalInfo != null)
                {
                    personalInfo.FullName = fullCvDto.FullName;
                    personalInfo.Address = fullCvDto.Address;
                    personalInfo.PhoneNumber = fullCvDto.PhoneNumber;
                    personalInfo.DateOfBirth = fullCvDto.DateOfBirth ?? DateTime.MinValue; // Handle nullable DateTime
                }

                // Remove existing related entities
                _context.Education.RemoveRange(_context.Education.Where(e => e.UserID == id));
                _context.Certifications.RemoveRange(_context.Certifications.Where(c => c.UserID == id));
                _context.Skills.RemoveRange(_context.Skills.Where(s => s.UserID == id));
                _context.WorkExperience.RemoveRange(_context.WorkExperience.Where(w => w.UserID == id));

                // Add new related entities
                foreach (var educationDto in fullCvDto.Educations)
                {
                    var education = new Education
                    {
                        UserID = id,
                        InstitutionName = educationDto.InstitutionName,
                        Degree = educationDto.Degree,
                        FieldOfStudy = educationDto.FieldOfStudy,
                        StartDate = educationDto.StartDate ?? DateTime.MinValue,
                        EndDate = educationDto.EndDate ?? DateTime.MinValue
                    };
                    _context.Education.Add(education);
                }

                foreach (var certificationDto in fullCvDto.Certifications)
                {
                    var certification = new Certification
                    {
                        UserID = id,
                        CertificationName = certificationDto.CertificationName,
                        IssuingOrganization = certificationDto.IssuingOrganization,
                        IssueDate = certificationDto.IssueDate ?? DateTime.MinValue,
                        ExpiryDate = certificationDto.ExpiryDate ?? DateTime.MinValue
                    };
                    _context.Certifications.Add(certification);
                }

                foreach (var skillDto in fullCvDto.Skills)
                {
                    var skill = new Skill
                    {
                        UserID = id,
                        SkillName = skillDto.SkillName,
                        SkillLevel = skillDto.SkillLevel
                    };
                    _context.Skills.Add(skill);
                }

                foreach (var workExperienceDto in fullCvDto.WorkExperiences)
                {
                    var workExperience = new WorkExperience
                    {
                        UserID = id,
                        CompanyName = workExperienceDto.CompanyName,
                        Position = workExperienceDto.Position,
                        StartDate = workExperienceDto.StartDate ?? DateTime.MinValue,
                        EndDate = workExperienceDto.EndDate ?? DateTime.MinValue,
                        Responsibilities = workExperienceDto.Responsibilities
                    };
                    _context.WorkExperience.Add(workExperience);
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new JsonResult("Updated Successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new JsonResult("Error: " + ex.Message);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    return NotFound("User not found.");
                }

                // Delete records from related tables
                _context.PersonalInfo.RemoveRange(_context.PersonalInfo.Where(pi => pi.UserID == id));
                _context.Skills.RemoveRange(_context.Skills.Where(s => s.UserID == id));
                _context.Education.RemoveRange(_context.Education.Where(e => e.UserID == id));
                _context.Certifications.RemoveRange(_context.Certifications.Where(c => c.UserID == id));
                _context.WorkExperience.RemoveRange(_context.WorkExperience.Where(w => w.UserID == id));

                // Delete the user
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return Ok("User deleted successfully.");
            }
            catch (Exception)
            {

                // Return an error response
                return StatusCode(500, "An error occurred while deleting the user. Please try again later.");
            }
        }



        [HttpDelete("deleteAll")]
        public async Task<JsonResult> DeleteAll()
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Delete all related entities before deleting users
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM WorkExperience");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Certifications");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Education");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM Skills");
                await _context.Database.ExecuteSqlRawAsync("DELETE FROM PersonalInfo");

                // Retrieve all users
                var users = await _context.Users.ToListAsync();

                if (!users.Any())
                {
                    return new JsonResult("No users found") { StatusCode = 404 };
                }

                // Now delete users
                _context.Users.RemoveRange(users);
                await _context.SaveChangesAsync();

                // Reset identity columns
                await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Users AUTO_INCREMENT = 1;");
                await _context.Database.ExecuteSqlRawAsync("ALTER TABLE PersonalInfo AUTO_INCREMENT = 1;");
                await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Skills AUTO_INCREMENT = 1;");
                await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Education AUTO_INCREMENT = 1;");
                await _context.Database.ExecuteSqlRawAsync("ALTER TABLE Certifications AUTO_INCREMENT = 1;");
                await _context.Database.ExecuteSqlRawAsync("ALTER TABLE WorkExperience AUTO_INCREMENT = 1;");

                await transaction.CommitAsync();

                return new JsonResult("All users and their related information deleted and IDs reset successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new JsonResult("Error: " + ex.Message);
            }
        }
    }
}
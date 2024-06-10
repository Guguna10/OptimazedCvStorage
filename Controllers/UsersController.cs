using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OptimazedCvStorage.Data;
using OptimazedCvStorage.DTOs;
using OptimazedCvStorage.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptimazedCvStorage.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CVContext _context;
        private readonly IConnection _rabbitMqConnection; // RabbitMQ connection
        private readonly string _queueName = "cv_processing_queue";

        public UsersController(CVContext context, IConnection rabbitMqConnection)
        {
            _context = context;
            _rabbitMqConnection = rabbitMqConnection;
        }

        [HttpGet]
        public async Task<JsonResult> GetAllCvs()
        {
            try
            {
                var allCvs = await _context.Users
                    .Include(u => u.PersonalInfo)
                    .Include(u => u.Educations)
                    .Include(u => u.Certifications)
                    .Include(u => u.Skills)
                    .Include(u => u.WorkExperiences)
                    .Select(user => new FullCvDto
                    {
                        UserID = user.UserID,
                        PersonalInfoID = user.PersonalInfo.PersonalInfoID,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.PersonalInfo.FullName,
                        Address = user.PersonalInfo.Address,
                        PhoneNumber = user.PersonalInfo.PhoneNumber,
                        DateOfBirth = user.PersonalInfo.DateOfBirth,
                        Educations = user.Educations.Select(e => new EducationDto
                        {
                            EducationID = e.EducationID,
                            InstitutionName = e.InstitutionName,
                            Degree = e.Degree,
                            FieldOfStudy = e.FieldOfStudy,
                            StartDate = e.StartDate,
                            EndDate = e.EndDate
                        }).ToList(),
                        Certifications = user.Certifications.Select(c => new CertificationDto
                        {
                            CertificationID = c.CertificationID,
                            CertificationName = c.CertificationName,
                            IssuingOrganization = c.IssuingOrganization,
                            IssueDate = c.IssueDate,
                            ExpiryDate = c.ExpiryDate
                        }).ToList(),
                        Skills = user.Skills.Select(s => new SkillDto
                        {
                            SkillID = s.SkillID,
                            SkillName = s.SkillName,
                            SkillLevel = s.SkillLevel
                        }).ToList(),
                        WorkExperiences = user.WorkExperiences.Select(we => new WorkExperienceDto
                        {
                            WorkExperienceID = we.WorkExperienceID,
                            CompanyName = we.CompanyName,
                            Position = we.Position,
                            StartDate = we.StartDate,
                            EndDate = we.EndDate,
                            Responsibilities = we.Responsibilities
                        }).ToList()
                    })
                    .ToListAsync();

                return new JsonResult(allCvs);
            }
            catch (Exception ex)
            {
                return new JsonResult("Error: " + ex.Message);
            }
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetCvById(int id)
        {
            try
            {
                var cv = await _context.Users
                    .Include(u => u.PersonalInfo)
                    .Include(u => u.Educations)
                    .Include(u => u.Certifications)
                    .Include(u => u.Skills)
                    .Include(u => u.WorkExperiences)
                    .FirstOrDefaultAsync(u => u.UserID == id);

                if (cv == null)
                {
                    return NotFound($"CV with ID {id} not found.");
                }

                return Ok(cv);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while retrieving CV: {ex.Message}");
            }
        }


        [HttpPost("storeinqueue")]
        public async Task<JsonResult> StoreCvInQueue([FromBody] List<FullCvDto> fullCvDtos)
        {
            try
            {
                foreach (var fullCvDto in fullCvDtos)
                {
                    // Enqueue each CV for processing
                    EnqueueCvForProcessing(fullCvDto);
                }

                return new JsonResult("CVs enqueued successfully for processing.");
            }
            catch (Exception ex)
            {
                return new JsonResult("Error: " + ex.Message);
            }
        }

        private void EnqueueCvForProcessing(FullCvDto fullCvDto)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" }; 
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "cv_processing_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var cvDtoJson = JsonConvert.SerializeObject(fullCvDto);
                var body = Encoding.UTF8.GetBytes(cvDtoJson);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                channel.BasicPublish(exchange: "",
                                     routingKey: "cv_processing_queue",
                                     basicProperties: properties,
                                     body: body);
            }
        }


        [HttpPost("storeindatabase")]
        public async Task<JsonResult> StoreInDatabaseFromQueue()
        {
            try
            {
                if (DequeueCvFromQueue(out FullCvDto fullCvDto))
                {
                    await ProcessAndStoreCv(fullCvDto);
                    return new JsonResult("CV stored successfully.");
                }
                else
                {
                    return new JsonResult("No CVs in the queue.");
                }
            }
            catch (Exception ex)
            {
                return new JsonResult("Error: " + ex.Message);
            }
        }

        private bool DequeueCvFromQueue(out FullCvDto fullCvDto)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" }; 
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "cv_processing_queue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                BasicGetResult result = channel.BasicGet("cv_processing_queue", true);

                if (result != null)
                {
                    var body = result.Body.ToArray();
                    var cvDtoJson = Encoding.UTF8.GetString(body);
                    fullCvDto = JsonConvert.DeserializeObject<FullCvDto>(cvDtoJson);
                    return true;
                }
                else
                {
                    fullCvDto = null;
                    return false;
                }
            }
        }

        private async Task ProcessAndStoreCv(FullCvDto fullCvDto)
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                // Create a new user
                var user = new User
                {
                    Username = fullCvDto.Username,
                    Email = fullCvDto.Email,
                    CreatedAt = DateTime.Now
                };

                // Add the user to the database
                _context.Users.Add(user);
                await _context.SaveChangesAsync(); // Save changes immediately to get the UserID

                // Add personal information
                var personalInfo = new PersonalInfo
                {
                    UserID = user.UserID,
                    FullName = fullCvDto.FullName,
                    Address = fullCvDto.Address,
                    PhoneNumber = fullCvDto.PhoneNumber,
                    DateOfBirth = fullCvDto.DateOfBirth ?? DateTime.MinValue
                };
                _context.PersonalInfo.Add(personalInfo);

                // Add educations
                _context.Education.AddRange(fullCvDto.Educations.Select(e => new Education
                {
                    UserID = user.UserID,
                    InstitutionName = e.InstitutionName,
                    Degree = e.Degree,
                    FieldOfStudy = e.FieldOfStudy,
                    StartDate = e.StartDate ?? DateTime.MinValue,
                    EndDate = e.EndDate ?? DateTime.MinValue
                }));

                // Add certifications
                _context.Certifications.AddRange(fullCvDto.Certifications.Select(c => new Certification
                {
                    UserID = user.UserID,
                    CertificationName = c.CertificationName,
                    IssuingOrganization = c.IssuingOrganization,
                    IssueDate = c.IssueDate ?? DateTime.MinValue,
                    ExpiryDate = c.ExpiryDate ?? DateTime.MinValue
                }));

                // Add skills
                _context.Skills.AddRange(fullCvDto.Skills.Select(s => new Skill
                {
                    UserID = user.UserID,
                    SkillName = s.SkillName,
                    SkillLevel = s.SkillLevel
                }));

                // Add work experiences
                _context.WorkExperience.AddRange(fullCvDto.WorkExperiences.Select(we => new WorkExperience
                {
                    UserID = user.UserID,
                    CompanyName = we.CompanyName,
                    Position = we.Position,
                    StartDate = we.StartDate ?? DateTime.MinValue,
                    EndDate = we.EndDate ?? DateTime.MinValue,
                    Responsibilities = we.Responsibilities
                }));

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Commit transaction
                transaction.Commit();
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

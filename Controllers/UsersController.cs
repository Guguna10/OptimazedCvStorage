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
        public async Task<IActionResult> Get()
        {
            try
            {
                var users = await _context.Users
                                          .Include(u => u.PersonalInfo)
                                          .Select(u => new
                                          {
                                              u.UserID,
                                              u.Username,
                                              u.Email,
                                              u.CreatedAt,
                                              PersonalInfo = u.PersonalInfo == null ? null : new
                                              {
                                                  u.PersonalInfo.FullName,
                                                  u.PersonalInfo.Address,
                                                  u.PersonalInfo.PhoneNumber,
                                                  u.PersonalInfo.DateOfBirth
                                              }
                                          })
                                          .ToListAsync();

                return new JsonResult(users);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework here)
                Console.WriteLine(ex.Message);

                // Return a 500 status code with the exception message
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                await _context.SaveChangesAsync();

                var education = new Education
                {
                    UserID = user.UserID,
                    InstitutionName = fullCvDto.InstitutionName,
                    Degree = fullCvDto.Degree,
                    FieldOfStudy = fullCvDto.FieldOfStudy,
                    StartDate = fullCvDto.StartDate ?? DateTime.MinValue, // Handle nullable DateTime
                    EndDate = fullCvDto.EndDate ?? DateTime.MinValue // Handle nullable DateTime
                };

                _context.Education.Add(education);
                await _context.SaveChangesAsync();

                var certification = new Certification
                {
                    UserID = user.UserID,
                    CertificationName = fullCvDto.CertificationName,
                    IssuingOrganization = fullCvDto.IssuingOrganization,
                    IssueDate = fullCvDto.IssueDate ?? DateTime.MinValue, // Handle nullable DateTime
                    ExpiryDate = fullCvDto.ExpiryDate ?? DateTime.MinValue // Handle nullable DateTime
                };

                _context.Certifications.Add(certification);
                await _context.SaveChangesAsync();

                var skill = new Skill
                {
                    UserID = user.UserID,
                    SkillName = fullCvDto.SkillName,
                    SkillLevel = fullCvDto.SkillLevel
                };

                _context.Skills.Add(skill);
                await _context.SaveChangesAsync();

                var workExperience = new WorkExperience
                {
                    UserID = user.UserID,
                    CompanyName = fullCvDto.CompanyName,
                    Position = fullCvDto.Position,
                    StartDate = fullCvDto.StartDate ?? DateTime.MinValue, // Handle nullable DateTime
                    EndDate = fullCvDto.EndDate ?? DateTime.MinValue, // Handle nullable DateTime
                    Responsibilities = fullCvDto.Responsibilities,
                };

                _context.WorkExperience.Add(workExperience);
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



        /*[HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, UserWithPersonalInfoDto userDto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.Include(u => u.PersonalInfo).FirstOrDefaultAsync(u => u.UserID == id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                user.Username = userDto.Username;
                user.Email = userDto.Email;
                user.PersonalInfo.FullName = userDto.FullName;
                user.PersonalInfo.Address = userDto.Address;
                user.PersonalInfo.PhoneNumber = userDto.PhoneNumber;
                user.PersonalInfo.DateOfBirth = userDto.DateOfBirth;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok("Updated Successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Users.Include(u => u.PersonalInfo).FirstOrDefaultAsync(u => u.UserID == id);
                if (user == null)
                {
                    return NotFound("User not found");
                }

                if (user.PersonalInfo != null)
                {
                    _context.PersonalInfo.Remove(user.PersonalInfo);
                }
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok("Deleted Successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        */
    }
}
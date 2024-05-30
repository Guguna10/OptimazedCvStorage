using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using OptimazedCvStorage.Models;
using System.Data;

namespace OptimazedCvStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactInformationController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public ContactInformationController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public JsonResult Get()
        {
            string query = @"select PersonId, PersonName, PersonPhoneNumber, PersonEmail, PersonLastName  from
                                optimazedcvstorage.contactinformation
                ";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("OptimazedCvStorage");
            MySqlDataReader myReader;
            using(MySqlConnection mycon = new MySqlConnection(sqlDataSource))
            {
                mycon.Open();
                using (MySqlCommand myCommand = new MySqlCommand(query, mycon))
                {
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    mycon.Close();
                }   
            }

            return new JsonResult(table);
        }

        [HttpPost]
        public JsonResult Post(ContactInformation Contacts)
        {
            string query = @"insert into optimazedcvstorage.contactinformation (PersonName, PersonPhoneNumber, PersonEmail, PersonLastName) values
                                                                                    (@PersonName, @PersonPhoneNumber, @PersonEmail, @PersonLastName)
                ";
            DataTable table = new DataTable();
            string sqlDataSource = _configuration.GetConnectionString("OptimazedCvStorage");
            MySqlDataReader myReader;
            using (MySqlConnection mycon = new MySqlConnection(sqlDataSource))
            {
                mycon.Open();
                using (MySqlCommand myCommand = new MySqlCommand(query, mycon))
                {
                    myCommand.Parameters.AddWithValue("@PersonName", Contacts.PersonName);
                    myCommand.Parameters.AddWithValue("@PersonPhoneNumber",Contacts.PersonPhoneNumber);
                    myCommand.Parameters.AddWithValue("@PersonEmail", Contacts.PersonEmail);
                    myCommand.Parameters.AddWithValue("@PersonLastName", Contacts.PersonLastName);
                    myReader = myCommand.ExecuteReader();
                    table.Load(myReader);

                    myReader.Close();
                    mycon.Close();
                }
            }

            return new JsonResult("Added Successfully");
        }


        [HttpPut]
        public JsonResult Put(ContactInformation Contacts)
        {
            string query = @"UPDATE optimazedcvstorage.contactinformation 
                     SET PersonName = @PersonName, 
                         PersonPhoneNumber = @PersonPhoneNumber, 
                         PersonLastName = @PersonLastName, 
                         PersonEmail = @PersonEmail
                     WHERE PersonId = @PersonId";

            string sqlDataSource = _configuration.GetConnectionString("OptimazedCvStorage");

            using (MySqlConnection mycon = new MySqlConnection(sqlDataSource))
            {
                using (MySqlCommand myCommand = new MySqlCommand(query, mycon))
                {
                    myCommand.Parameters.AddWithValue("@PersonId", Contacts.PersonId);
                    myCommand.Parameters.AddWithValue("@PersonName", Contacts.PersonName);
                    myCommand.Parameters.AddWithValue("@PersonPhoneNumber", Contacts.PersonPhoneNumber);
                    myCommand.Parameters.AddWithValue("@PersonEmail", Contacts.PersonEmail);
                    myCommand.Parameters.AddWithValue("@PersonLastName", Contacts.PersonLastName);

                    mycon.Open();
                    int rowsAffected = myCommand.ExecuteNonQuery();
                    mycon.Close();

                    if (rowsAffected > 0)
                    {
                        return new JsonResult("Updated Successfully");
                    }
                    else
                    {
                        return new JsonResult("Update Failed");
                    }
                }
            }
        }


    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.AspNetCore.Mvc;
using APBD_5.Services;
using APBD_5.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace APBD_5.Controllers
{
    [ApiController]
    public class EnrollmentController : ControllerBase
    {

        SqlServerDbService dbService;

        public EnrollmentController(SqlServerDbService dbService)
        {
            this.dbService = dbService;
        }

        [Route("api/ennrollments")]
        [HttpPost]
        public IActionResult EnrollStudent(StudentRequest studentRequest)
        {
            int idStudies;
            var command = new SqlCommand() { CommandText = "select s.IdStudy from Studies s where s.Name=@studies" };
            command.Parameters.AddWithValue("studies", studentRequest.Studies);
            var queryResult = dbService.ExecuteSelect(command);
            if (queryResult.Count == 0)
            {
                return BadRequest("No studies");
            }
            else
            {
                idStudies = (int) queryResult[0][0];
            }

            command = new SqlCommand() { CommandText = "select * from Enrollment e JOIN Student s ON e.IdEnrollment=s.IdEnrollment where e.Semester=1 and e.IdStudy=@idStudy and IndexNumber=@indexNumber" };
            command.Parameters.AddWithValue("idStudies", idStudies);
            command.Parameters.AddWithValue("indexNumber", studentRequest.IndexNumber);
            queryResult = dbService.ExecuteSelect(command);
            if (queryResult.Count == 0)
            {
                command = new SqlCommand() { CommandText = "SELECT MAX(IdEnrollment) FROM Enrollment" };
                int idEnrollment = ((int)dbService.ExecuteSelect(command)[0][0]) + 1;
                var transaction = dbService.GetConnection().BeginTransaction();

                command = new SqlCommand() { CommandText = "INSERT INTO Enrollment(IdEnrollment, StartDate, IdStudy, Semester) VALUES(@idEnrollment, @startDate, @idStudy, @semester)" };
                DateTime startDate = DateTime.Now;

                command.Parameters.AddWithValue("idEnrollment", idEnrollment);
                command.Parameters.AddWithValue("startDate", SqlDateTime.Parse(startDate.ToString("yyyy-MM-dd")));
                command.Parameters.AddWithValue("idStudies", idStudies);
                command.Parameters.AddWithValue("semester", 1);
                dbService.ExecuteInsert(command);

                command = new SqlCommand() { CommandText = "INSERT INTO dbo.Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) VALUES (@indexNumber, @firstName, @lastName, @birthDate, @idEnrollment)" };
                command.Parameters.AddWithValue("indexNumber", studentRequest.IndexNumber);
                command.Parameters.AddWithValue("firstName", studentRequest.FirstName);
                command.Parameters.AddWithValue("lastName", studentRequest.LastName);
                command.Parameters.AddWithValue("birthDate", studentRequest.Birthdate);
                command.Parameters.AddWithValue("idEnrollment", idEnrollment);
                dbService.ExecuteInsert(command);

                Enrollment enrollment = new Enrollment();
                enrollment.IdEnrollment = idEnrollment;
                enrollment.Semester = 1;
                enrollment.IdStudy = idStudies;
                enrollment.StartDate = startDate;

                transaction.Commit();
                return Created("", enrollment);
            }
            else
            {
                return BadRequest("Index unavailable");
            }
        }
       
        [Route("api/enrollments/promotions")]
        [HttpPost]
        public IActionResult StudentPromotions(PromotionRequest promotionRequest)
        {
            var command = new SqlCommand() { CommandText = "SELECT s.IdStudy FROM Studies s WHERE s.Name = @studyName" };
            command.Parameters.AddWithValue("studyName", promotionRequest.Studies);
            var queryResult = dbService.ExecuteSelect(command);
            int idStudy;

            if (queryResult.Count == 0)
            {
                return BadRequest("not studies found");
            }
            else
            {
                idStudy = (int)queryResult[0][0];
            }

            command = new SqlCommand() { CommandText = "SELECT * FROM Enrollment e WHERE e.Semester = @semester and e.IdStudy = @idStudy" };
            command.Parameters.AddWithValue("semester", promotionRequest.Semester);
            command.Parameters.AddWithValue("idStudy", idStudy);

            queryResult = dbService.ExecuteSelect(command);
            if (queryResult.Count == 0)
            {
                return NotFound("No entries");
            }
            else
            {
                command = new SqlCommand()
                {
                    CommandText = "procedurePromoteStudents",
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("semester", promotionRequest.Semester);
                command.Parameters.AddWithValue("idStudy", idStudy);
                dbService.ExecuteInsert(command);
                return Ok();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Http;
using BangazonAPI.Models;
using System.Linq;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingProgramController : ControllerBase
    {
        private readonly IConfiguration _config;

        public TrainingProgramController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, StartDate, EndDate, MaxAttendees FROM TrainingProgram
                        WHERE 1=1";

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<TrainingProgram> trainingPrograms = new List<TrainingProgram>();

                    while (reader.Read())
                    {
                        TrainingProgram trainingProgram = new TrainingProgram
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                            EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                            MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees")),
                            Employees = new List<Employee>()
                        };

                        trainingPrograms.Add(trainingProgram);
                    }
                    reader.Close();

                    return Ok(trainingPrograms);
                }
            }
        }

        [HttpGet("{id}", Name = "GetTrainingProgram")]
        public async Task<IActionResult> Get([FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT tp.Id as TrainingProgramID, tp.Name, tp.StartDate, tp.EndDate, tp.MaxAttendees, et.Id as EmployeeTrainingId, et.EmployeeId, et.TrainingProgramId, e.Id AS EmployeeId, e.FirstName, e.LastName, e.DepartmentId, e.Email, e.IsSupervisor, e.ComputerId FROM TrainingProgram tp
                        LEFT JOIN EmployeeTraining et ON et.TrainingProgramId = tp.Id
                        LEFT JOIN Employee e ON et.EmployeeId = e.Id
                        WHERE tp.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();

                    TrainingProgram trainingProgram = null;

                    while (reader.Read())
                    {
                        if (trainingProgram == null)
                        {
                            trainingProgram = new TrainingProgram
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("TrainingProgramID")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                StartDate = reader.GetDateTime(reader.GetOrdinal("StartDate")),
                                EndDate = reader.GetDateTime(reader.GetOrdinal("EndDate")),
                                MaxAttendees = reader.GetInt32(reader.GetOrdinal("MaxAttendees")),
                                Employees = new List<Employee>()
                            };
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("EmployeeId")))
                        {
                            Employee employeeToAdd = new Employee()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                                DepartmentId = reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                                Email = reader.GetString(reader.GetOrdinal("Email")),
                                IsSupervisor = reader.GetBoolean(reader.GetOrdinal("IsSupervisor"))
                            };

                            if (!reader.IsDBNull(reader.GetOrdinal("ComputerId")))
                            {
                                employeeToAdd.ComputerId = reader.GetInt32(reader.GetOrdinal("ComputerId"));
                            }
                            trainingProgram.Employees.Add(employeeToAdd);
                        }

                    }
                    reader.Close();
                    return Ok(trainingProgram);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TrainingProgram trainingProgram)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO TrainingProgram (Name, StartDate, EndDate, MaxAttendees)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @startDate, @endDate, @maxAttendees)";
                    cmd.Parameters.Add(new SqlParameter("@name", trainingProgram.Name));
                    cmd.Parameters.Add(new SqlParameter("@startDate", trainingProgram.StartDate));
                    cmd.Parameters.Add(new SqlParameter("@endDate", trainingProgram.EndDate));
                    cmd.Parameters.Add(new SqlParameter("@maxAttendees", trainingProgram.MaxAttendees));

                    int newId = (int)cmd.ExecuteScalar();
                    trainingProgram.Id = newId;
                    return CreatedAtRoute("GetTrainingProgram", new { id = newId }, trainingProgram);
                }
            }
        }

        [HttpPost]
        [Route("{id}/employees")]
        public async Task<IActionResult> Post([FromBody] Employee employee, [FromRoute] int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO EmployeeTraining (EmployeeId, TrainingProgramId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@employeeId, @trainingProgramID)";
                    cmd.Parameters.Add(new SqlParameter("@employeeId", employee.Id));
                    cmd.Parameters.Add(new SqlParameter("@trainingProgramID", id));

                    cmd.ExecuteNonQuery();
                    return RedirectToRoute("GetTrainingProgram", new { id = id });
                }
            }
        }
    }
}
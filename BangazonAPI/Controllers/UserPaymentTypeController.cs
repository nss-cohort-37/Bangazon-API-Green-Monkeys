using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using BangazonAPI.Models;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserPaymentTypesController : ControllerBase
    {
        private readonly IConfiguration _config;
        public UserPaymentTypesController(IConfiguration config)
        {
            _config = config;
        }
        //computed property
        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }

        //api/userPaymentTypes? customerId = { customer id }
        public async Task<IActionResult> Get([FromQuery] int? customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT u.Id, u.AcctNumber, u.Active, u.CustomerId, u.PaymentTypeId
                    FROM UserPaymentType u
                    WHERE u.customerId = @customerId";

                    cmd.Parameters.Add(new SqlParameter("@customerid", customerId));

                    SqlDataReader reader = cmd.ExecuteReader();

                    List<UserPaymentType> userPaymentTypes = new List<UserPaymentType>();

                    while (reader.Read())
                    {
                        UserPaymentType userPaymentType = new UserPaymentType
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            AccountNumber = reader.GetString(reader.GetOrdinal("AcctNumber")),
                            Active = reader.GetBoolean(reader.GetOrdinal("Active")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            PaymentTypeId = reader.GetInt32(reader.GetOrdinal("PaymentTypeId"))
                            

                        };

                        userPaymentTypes.Add(userPaymentType);
                    }
                    reader.Close();

                    return Ok(userPaymentTypes);
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] UserPaymentType userPaymentType)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO UserPaymentType (AcctNumber, Active, CustomerId, PaymentTypeId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@AcctNumber, @Active, @CustomerId, @PaymentTypeId)";
                    cmd.Parameters.Add(new SqlParameter("@AcctNumber", userPaymentType.AccountNumber));
                    cmd.Parameters.Add(new SqlParameter("@Active", userPaymentType.Active));
                    cmd.Parameters.Add(new SqlParameter("@CustomerId", userPaymentType.CustomerId));
                    cmd.Parameters.Add(new SqlParameter("@PaymentTypeId", userPaymentType.PaymentTypeId));

                    int newId = (int)cmd.ExecuteScalar();
                    userPaymentType.Id = newId;
                    return CreatedAtRoute( new { id = newId }, userPaymentType);
                }
            }
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] UserPaymentType userPaymentType)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE UserPaymentType
                                            SET AcctNumber = @AcctNumber, Active = @Active, CustomerId = @CustomerId, PaymentTypeId = @PaymentTypeId
                                            WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@AcctNumber", userPaymentType.AccountNumber));
                        cmd.Parameters.Add(new SqlParameter("@Active", userPaymentType.Active));
                        cmd.Parameters.Add(new SqlParameter("@CustomerId", userPaymentType.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@PaymentTypeId", userPaymentType.PaymentTypeId));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!UserPaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE UserPaymentType
                                            SET Active = @active
                                            WHERE Id = @id";

                        cmd.Parameters.Add(new SqlParameter("@Active", false));
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!UserPaymentTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool UserPaymentTypeExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, AcctNumber, Active, CustomerId, PaymentTypeId
                        FROM UserPaymentType
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }




    }

}
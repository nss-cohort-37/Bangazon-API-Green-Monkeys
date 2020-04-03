using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BangazonAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace BangazonAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueReportController : ControllerBase
    {

        private readonly IConfiguration _config;
        public RevenueReportController(IConfiguration config)
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT p.ProductTypeId, pt.[Name] AS 'Product Type Name', SUM(p.Price) as 'Total Price'
                        FROM[Order] o
                        INNER JOIN UserPaymentType u ON o.UserPaymentTypeId = u.Id
                        LEFT JOIN[OrderProduct] op ON o.Id = op.OrderId
                        LEFT JOIN Product p ON op.ProductId = p.Id
                        LEFT JOIN ProductType pt ON p.ProductTypeId = pt.Id
                        GROUP BY p.ProductTypeId, pt.[Name]";
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<RevenueReport> revenueReports = new List<RevenueReport>();

                    while (reader.Read())
                    {
                        RevenueReport revenueReport = new RevenueReport
                        {
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            ProductType = reader.GetString(reader.GetOrdinal("Product Type Name")),
                            TotalRevenue = reader.GetDecimal(reader.GetOrdinal("Total Price"))
                        };

                        revenueReports.Add(revenueReport);
                    }
                    reader.Close();

                    return Ok(revenueReports);
                }
            }
        }
    }
}
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
    public class OrdersController : ControllerBase
    {
        private readonly IConfiguration _config;

        public OrdersController(IConfiguration config)
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

        public async Task<IActionResult> Get([FromQuery] int? customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                    SELECT o.Id, o.CustomerId, o.UserPaymentTypeId, p.CustomerId, p.Title, p.ProductTypeId, p.Price, p.DateAdded, p.Description
                    FROM [Order] o 
                    LEFT JOIN  Product p 
                    ON p.CustomerId = o.customerId
                    WHERE o.customerId = @customerId";

                    cmd.Parameters.Add(new SqlParameter("@customerid", customerId));

                    SqlDataReader reader = cmd.ExecuteReader();

                    Order order = null; 

                    List<Product> products = new List<Product>();

                    while (reader.Read())
                    {
                        if (order == null)
                        {
                            order = new Order
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                          
                            products = new List<Product>()
                            };

                            if (!reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")))
                            {
                                order.UserPaymentTypeId = reader.GetInt32(reader.GetOrdinal("UserPaymentTypeId"));
                            }
                            else
                            {
                                order.UserPaymentTypeId = null;
                            }
                        }
                        order.products.Add(new Product()
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                            ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                            CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                            Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Description = reader.GetString(reader.GetOrdinal("Description"))

                        });

                    }
                    reader.Close();

                    return Ok(order);
                }
            }
        }
    }
}
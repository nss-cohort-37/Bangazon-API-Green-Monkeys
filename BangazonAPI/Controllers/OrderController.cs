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

        //api/orders?customerId={customerId}&cart=true

        public async Task<IActionResult> Get(
            [FromQuery] int? customerId, 
            [FromQuery] string cart)
        {
            if (cart == "true" && customerId != null)
            {
                var orders = GetAllOrdersByCustomerIdCartTrue(customerId);
                return Ok(orders);
            }
           else if (customerId != null)
            {
                var orders = GetAllOrdersByCustomerId(customerId);
                return Ok(orders); 

            } 
            else { return null;  } 
        }

        private List<Order> GetAllOrdersByCustomerId([FromQuery] int? customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                  SELECT o.Id, o.CustomerId, o.UserPaymentTypeId,op.ProductId, p.Title, p.ProductTypeId, p.Price, p.DateAdded, p.Description
                    FROM [OrderProduct] op
                    LEFT JOIN [Order] o 
                    ON o.Id = op.OrderId
                    LEFT JOIN  Product p 
                    ON p.Id= op.ProductId
                    WHERE o.customerId = @customerId";

                    cmd.Parameters.Add(new SqlParameter("@customerid", customerId));


                    SqlDataReader reader = cmd.ExecuteReader();

                 
                    List<Order> orders = new List<Order>();

                    Order order = null;

                    while (reader.Read())
                    {
                      if(order == null)
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
                     orders.Add(order); 
                    reader.Close();


                    return orders;
                }
            }
        }
        private Order GetAllOrdersByCustomerIdCartTrue([FromQuery] int? customerId)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                   SELECT o.Id, o.CustomerId, o.UserPaymentTypeId,op.ProductId, p.Title, p.ProductTypeId, p.Price, p.DateAdded, p.Description
                    FROM [OrderProduct] op
                    LEFT JOIN [Order] o 
                    ON o.Id = op.OrderId
                    LEFT JOIN  Product p 
                    ON p.Id= op.ProductId";

                    cmd.Parameters.Add(new SqlParameter("@customerid", customerId));


                    SqlDataReader reader = cmd.ExecuteReader();

                    Order order = null;

                    List<Product> products = new List<Product>();

                    while (reader.Read())
                    {
                        if(reader.IsDBNull(reader.GetOrdinal("UserPaymentTypeId")) == true)
                        {

                            if (order == null )
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

                    }
                    reader.Close();

                    return order;
                }
            }
        }


        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<IActionResult> Get([FromRoute] int Id)
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
                    WHERE o.Id = @id";

                    cmd.Parameters.Add(new SqlParameter("@id", Id));
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

        //Add a product to shopping cart; check to see if user has cart and if not make a cart--which is an order with UserPayment is null 

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CustomerProduct customerProduct)
        {
            var cart = GetAllOrdersByCustomerIdCartTrue(customerProduct.CustomerId); 
            // if cart is null then make cart which is an order 

            // take cart and insert in orderproduct 

            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    
                 
                    if(cart == null)
                    {
                        cmd.CommandText = @"INSERT INTO [Order] (CustomerId, UserPaymentTypeId)
                                            OUTPUT INSERTED.Id
                                            VALUES ( @CustomerId, @UserPaymentTypeId)";

                        cmd.Parameters.Add(new SqlParameter("@CustomerId", customerProduct.CustomerId));
                        cmd.Parameters.Add(new SqlParameter("@UserPaymentTypeId", DBNull.Value));

                        int newId = (int)cmd.ExecuteScalar();
                        cart.Id = newId;
                        return CreatedAtRoute(new { id = newId }, customerProduct);

                        //return Ok(newId); 
                    }

                    //insert into orderproducts; we need the Id made on line 274
                    //orderId associated with the customerId from the customerProduct objec and UserPaymentNotNull



                    cmd.CommandText = @"INSERT INTO OrderProduct ( OrderId, ProductId)
                                            OUTPUT INSERTED.Id
                                            VALUES ( @OrderId, @ProductId)";


                        cmd.Parameters.Add(new SqlParameter("@OrderId", cart.Id ));
                        cmd.Parameters.Add(new SqlParameter("@ProductId", customerProduct.ProductId));




                    int newerId = (int)cmd.ExecuteScalar();
                   customerProduct.Id = newerId;
                    return CreatedAtRoute(new { id = newerId },customerProduct);

                }
            }
        }

        //[HttpPut("{id}")]
        //public async Task<IActionResult> Put([FromRoute] int id, [FromBody] UserPaymentType userPaymentType)
        //{
        //    try
        //    {
        //        using (SqlConnection conn = Connection)
        //        {
        //            conn.Open();
        //            using (SqlCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.CommandText = @"UPDATE Order
        //                                    SET CustomerId = @CustomerId, UserPaymentTypeId = @UserPaymentTypeId
        //                                    WHERE Id = @id";
                      
        //                cmd.Parameters.Add(new SqlParameter("@CustomerId", userPaymentType.CustomerId));
        //                cmd.Parameters.Add(new SqlParameter("@UserPaymentTypeId", userPaymentType.UserPaymentTypeId));
        //                cmd.Parameters.Add(new SqlParameter("@id", id));

        //                int rowsAffected = cmd.ExecuteNonQuery();
        //                if (rowsAffected > 0)
        //                {
        //                    return new StatusCodeResult(StatusCodes.Status204NoContent);
        //                }
        //                throw new Exception("No rows affected");
        //            }
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        if (!UserPaymentTypeExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}






    }
}
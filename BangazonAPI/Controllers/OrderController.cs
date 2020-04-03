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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="cart"></param>
        /// <returns></returns>
        [HttpGet]
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
                  SELECT o.Id AS OrderId, o.CustomerId, o.UserPaymentTypeId, op.ProductId, p.Title, p.ProductTypeId, p.Price, p.DateAdded, p.Description, p.Id AS ProductId
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

                    //order.Id != reader.GetInt32(reader.GetOrdinal("OrderId"))
                    while (reader.Read())
                    {
                        int idValue = reader.GetInt32(reader.GetOrdinal("OrderId"));
                        var existingOrder = orders.FirstOrDefault(o => o.Id == idValue);

                        if (existingOrder == null)
                        {

                            order = new Order
                            {
                                Id = idValue,
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


                            order.products.Add(new Product()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
                                DateAdded = reader.GetDateTime(reader.GetOrdinal("DateAdded")),
                                ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
                                CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                                Price = reader.GetDecimal(reader.GetOrdinal("Price")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                Description = reader.GetString(reader.GetOrdinal("Description"))

                            });

                            orders.Add(order);

                        }
                        else
                        {
                            existingOrder.products.Add(new Product()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
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
                   SELECT o.Id AS OrderId, o.CustomerId, o.UserPaymentTypeId, op.ProductId, p.Title, p.ProductTypeId, p.Price, p.DateAdded, p.Description, p.Id AS ProductId
                    FROM [OrderProduct] op
                    LEFT JOIN [Order] o 
                    ON o.Id = op.OrderId
                    LEFT JOIN  Product p 
                    ON p.Id= op.ProductId
                    WHERE o.customerId = @customerId";

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
                                    Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
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
                                Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("{id}", Name = "GetOrder")]
        public async Task<IActionResult> Get([FromRoute] int Id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                   SELECT o.Id AS OrderId, o.CustomerId, o.UserPaymentTypeId, op.ProductId, p.Title, p.ProductTypeId, p.Price, p.DateAdded, p.Description, p.Id AS ProductId
                    FROM [OrderProduct] op
                    LEFT JOIN [Order] o 
                    ON o.Id = op.OrderId
                    LEFT JOIN  Product p 
                    ON p.Id= op.ProductId
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
                                Id = reader.GetInt32(reader.GetOrdinal("OrderId")),
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
                            Id = reader.GetInt32(reader.GetOrdinal("ProductId")),
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="customerProduct"></param>
        /// <returns></returns>
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
                        cart = new Order()
                        {
                            Id= newId, 
                            CustomerId= customerProduct.CustomerId
                        };
                       

                       
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
                    return CreatedAtRoute(new { id = cart.Id }, new Order { CustomerId = customerProduct.CustomerId, Id = cart.Id });

                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Order order)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"UPDATE [Order]
                                            SET UserPaymentTypeId = @UserPaymentTypeId, CustomerId=CustomerId
                                            WHERE Id = @id";

                        
                        cmd.Parameters.Add(new SqlParameter("@UserPaymentTypeId", order.UserPaymentTypeId));
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
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        [HttpDelete("{orderId}/products/{productId}")]
        public async Task<IActionResult> Delete([FromRoute] int orderId, [FromRoute] int productId )
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM [OrderProduct] WHERE OrderId = @orderId AND ProductId = @productId";

                        cmd.Parameters.Add(new SqlParameter("@orderId", orderId));
                        cmd.Parameters.Add(new SqlParameter("@productId", productId));

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
                if (!OrderExists(orderId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        private bool OrderExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, CustomerId, UserPaymentTypeId
                        FROM [Order]
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }





    }
}
using E_Commerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly MyDbContext _Db;

        public OrderController(MyDbContext db)
        {

            _Db = db;
        }
        [HttpGet("GetAllOrders")]
        public IActionResult GetAllOrders()
        {
            var orders = _Db.Orders.ToList();
            return Ok(orders);
        }
    }
}

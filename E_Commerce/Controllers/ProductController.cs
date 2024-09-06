using E_Commerce.DTOs;
using E_Commerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {

        private readonly MyDbContext _Db;

        public ProductController(MyDbContext db)
        {

            _Db = db;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetProduct()
        {
            var products = _Db.Products
                .Join(_Db.Categories,
                    product => product.CategoryId,
                    category => category.Id,
                    (product, category) => new
                    {
                        id = product.Id,
                        productName = product.ProductName,
                        description = product.Description,
                        price = product.Price,
                        stockQuantity = product.StockQuantity,
                        image = product.Image,
                        discount = product.Discount,
                        categoryName = category.CategoryName
                    })
                .GroupJoin(_Db.Reviews,
                    product => product.id,
                    review => review.ProductId,
                    (product, reviews) => new
                    {
                        product.id,
                        product.productName,
                        product.description,
                        product.price,
                        product.stockQuantity,
                        product.image,
                        product.discount,
                        product.categoryName,
                        reviews = reviews.DefaultIfEmpty()  // Ensures the left join
                    })
                .SelectMany(p => p.reviews,
                    (p, review) => new
                    {
                        id = p.id,
                        productName = p.productName,
                        description = p.description,
                        price = p.price,
                        stockQuantity = p.stockQuantity,
                        image = p.image,
                        discount = p.discount,
                        categoryName = p.categoryName,
                        reviewRate = review != null ? review.Rating : (decimal?)null,
                        comment = review != null ? review.Comment : null,
                        status = review != null ? review.Status : null,
                    })
                .ToList();

            return Ok(products);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetProductById(int id)
        {
            var product = _Db.Products
                .Where(p => p.Id == id)
                .Join(_Db.Categories,
                    product => product.CategoryId,
                    category => category.Id,
                    (product, category) => new
                    {
                        id = product.Id,
                        productName = product.ProductName,
                        description = product.Description,
                        price = product.Price,
                        stockQuantity = product.StockQuantity,
                        image = product.Image,
                        discount = product.Discount,
                        categoryName = category.CategoryName
                    })
                .GroupJoin(_Db.Reviews,
                    product => product.id,
                    review => review.ProductId,
                    (product, reviews) => new
                    {
                        product.id,
                        product.productName,
                        product.description,
                        product.price,
                        product.stockQuantity,
                        product.image,
                        product.discount,
                        product.categoryName,
                        reviews = reviews.DefaultIfEmpty()
                    })
                .SelectMany(p => p.reviews,
                    (p, review) => new
                    {
                        id = p.id,
                        productName = p.productName,
                        description = p.description,
                        price = p.price,
                        stockQuantity = p.stockQuantity,
                        image = p.image,
                        discount = p.discount,
                        categoryName = p.categoryName,
                        reviewRate = review != null ? review.Rating : (decimal?)null,
                        comment = review != null ? review.Comment : null,
                        status = review != null ? review.Status : null,
                    })
                .FirstOrDefault();

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult AddReview([FromBody] ProductRequistDTO dto)
        {
            if (dto == null || dto.ProductId <= 0 || dto.UserId <= 0 || string.IsNullOrWhiteSpace(dto.Comment) || dto.Rating <= 0)
            {
                return BadRequest("Invalid review data.");
            }

            var productExists = _Db.Products.Any(p => p.Id == dto.ProductId);
            if (!productExists)
            {
                return NotFound("Product not found.");
            }

            var userExists = _Db.Users.Any(u => u.Id == dto.UserId);
            if (!userExists)
            {
                return NotFound("User not found.");
            }

            // Create the review
            var review = new Review
            {
                UserId = dto.UserId,
                ProductId = dto.ProductId,
                Comment = dto.Comment,
                Rating = dto.Rating,
            };

            _Db.Reviews.Add(review);
            _Db.SaveChanges();

            return Ok(review);
        }


        [HttpPut("/Admin/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateReviewStatus(int id, [FromBody] ProductRequestAdminDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Status))
            {
                return BadRequest("Invalid Status data.");
            }

            var review = _Db.Reviews.FirstOrDefault(r => r.Id == id);
            if (review == null)
            {
                return NotFound();
            }

            review.Status = dto.Status;

            _Db.Reviews.Update(review);
            _Db.SaveChanges();

            return Ok(review);
        }

        [HttpGet("/Reviews")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetApprovedReviews()
        {
            var approvedReviews = _Db.Reviews.Join(_Db.Users,
                review => review.UserId,
                user => user.Id,
                (review, user) =>
                new
                {
                    id = review.Id,
                    rating = review.Rating,
                    comment = review.Comment,
                    status = review.Status,
                    product = review.Product.ProductName,
                    user = user.Username

                }).Where(r => r.status == "Approved").ToList();
            return Ok(approvedReviews);
        }



        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteReview(int id)
        {
            var review = _Db.Reviews.FirstOrDefault(a => a.Id == id);
            if (review == null)
            {
                return NotFound();
            }

            _Db.Reviews.Remove(review);
            _Db.SaveChanges();

            return Ok(review);
        }



        [HttpGet("average-rating/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetAverageRating(int id)
        {

            var productExists = _Db.Products.Any(p => p.Id == id);
            if (!productExists)
            {
                return NotFound("Product not found.");
            }


            var reviews = _Db.Reviews.Where(r => r.ProductId == id);

            if (!reviews.Any())
            {
                return Ok(new
                {
                    ProductId = id,
                    AverageRating = (decimal?)null
                });
            }

            var averageRating = reviews.Average(r => r.Rating);

            return Ok(new
            {
                ProductId = id,
                AverageRating = averageRating
            });
        }

        [HttpGet("share/facebook/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult ShareToFacebook(int productId)
        {
            var product = _Db.Products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
            {
                return NotFound("Product not found.");
            }
            string productUrl = $"{Request.Scheme}://{Request.Host}/product/{productId}";
            string facebookShareUrl = $"https://www.facebook.com/sharer/sharer.php?u={Uri.EscapeDataString(productUrl)}";

            return Redirect(facebookShareUrl);
        }





    }
}
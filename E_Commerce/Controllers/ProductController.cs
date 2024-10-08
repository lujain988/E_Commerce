﻿using E_Commerce.DTOs;
using E_Commerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            if (dto == null || dto.ProductId <= 0 || dto.UserId <= 0 || string.IsNullOrWhiteSpace(dto.Comment))
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


        [HttpGet("/Reviews/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetApprovedReviews(int productId)
        {
            var approvedReviews = _Db.Reviews
      .Join(_Db.Users,
          review => review.UserId,
          user => user.Id,
          (review, user) => new { review, user })
      .Join(_Db.Products,
          combined => combined.review.ProductId,
          product => product.Id,
          (combined, product) => new
          {
              id = combined.review.Id,
              user = combined.user.Username,
              productName = product.ProductName,
              categoryName = product.Category.CategoryName,
              comment = combined.review.Comment,
              rating = combined.review.Rating,
              status = combined.review.Status,
              productId = combined.review.ProductId

          })
      .Where(r => r.status == "Approved" && r.productId == productId)
      .ToList();


            return Ok(approvedReviews);
        }



        [HttpPost("/SubmitReview")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SubmitReview([FromBody] ReviewDto reviewDto)
        {
            // Check if the user has already rated this product
            var existingReview = _Db.Reviews
                .FirstOrDefault(r => r.UserId == reviewDto.UserId && r.ProductId == reviewDto.ProductId);

            if (existingReview != null)
            {
                return BadRequest("User has already submitted a review for this product.");
            }

            var newReview = new Review
            {
                ProductId = reviewDto.ProductId,
                UserId = reviewDto.UserId,
                Rating = reviewDto.Rating,

            };

            _Db.Reviews.Add(newReview);
            _Db.SaveChanges();

            return CreatedAtAction(nameof(GetApprovedReviews), new { productId = newReview.ProductId }, newReview);
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

        [HttpGet("/getCategories/")]
        public IActionResult GetCategories()
        {
            var categories = _Db.Categories
                .Select(c => new
                {
                    categoryName = c.CategoryName
                })
                .Distinct()
                .ToList();

            return Ok(categories);
        }
        [HttpGet("/filterOnCategory/")]
        public IActionResult Filter(string category)
        {
            if (string.IsNullOrEmpty(category) || category == "All" || category == "*")
            {
                // Return all products when category is "All" or "*"
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
                    .ToList();

                return Ok(products);
            }

            var filteredProducts = _Db.Products
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
                .Where(p => p.categoryName == category)
                .ToList();

            return Ok(filteredProducts);
        }

        [HttpGet("/PriceFilter/")]
        public IActionResult GetPrice([FromQuery] decimal minPrice = 0, [FromQuery] decimal maxPrice = decimal.MaxValue)
        {

            var products = _Db.Products
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .ToList();
            return Ok(products);


        }


        [HttpGet("CountOfAllProduct")]
        public IActionResult GCountOfAllProduct()
        {

            var products = _Db.Products.ToList().Count;
            return Ok(products);


        }

        [HttpPut("{id}/apply-discount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ApplyDiscount(int id, [FromBody] decimal discount)
        {
            // Check if discount is valid
            if (discount < 0 || discount > 100)
            {
                return BadRequest("Discount must be between 0 and 100.");
            }

            // Find the product by its ID
            var product = _Db.Products.FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Apply the discount
            product.Discount = discount;


            _Db.SaveChanges();

            return Ok(new
            {
                message = "Discount applied successfully",
                productId = product.Id,
                discount = product.Discount
            });
        }

        [HttpGet("discounted")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetDiscountedProducts()
        {
            var discountedProducts = _Db.Products
                .Where(p => p.Discount > 0) // Filter products with a discount greater than 0
                .Select(product => new
                {
                    id = product.Id,
                    productName = product.ProductName,
                    description = product.Description,
                    price = product.Price,
                    stockQuantity = product.StockQuantity,
                    image = product.Image,
                    discount = product.Discount,
                    categoryName = product.Category.CategoryName,
                    reviews = product.Reviews.Select(review => new
                    {
                        reviewRate = review.Rating,
                        comment = review.Comment,
                        status = review.Status
                    }).ToList()
                })
                .ToList();
            if (discountedProducts.Count == 0)
                return NotFound();


            return Ok(discountedProducts);
        }


        [HttpGet("/GetAllReview/")]
        public IActionResult GetAllReview()
        {
            var reviews = _Db.Reviews
                .Join(_Db.Users,
                    review => review.UserId,
                    user => user.Id,
                    (review, user) => new { review, user })
                .Join(_Db.Products,
                    combined => combined.review.ProductId,
                    product => product.Id,
                    (combined, product) => new
                    {
                        id = combined.review.Id,
                        user = combined.user.Username,
                        productName = product.ProductName,
                        categoryName = product.Category.CategoryName,
                        comment = combined.review.Comment,
                        rating = combined.review.Rating,
                        status = combined.review.Status,
                        productId = combined.review.ProductId
                    })
                .OrderBy(r =>
                  
                    r.status == "Pending" ? 0 :
                    r.status == "Approved" ? 1 :
                    r.status == "Declined" ? 2 :
                    3 
                )
                .ToList();

            return Ok(reviews);
        }






    }
    }

using E_Commerce.DTOs;
using E_Commerce.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace E_Commerce.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly MyDbContext _db;

        public CategoriesController(MyDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetAllCategory")]
        public IActionResult GetAllCategory()
        {

            var categories = _db.Categories.ToList();
            return Ok(categories);


        }
        [HttpGet("CountAllCategory")]
        public IActionResult CountAllCategory()
        {

            var categories = _db.Categories.ToList().Count;
            return Ok(categories);


        }


        [Route("GetCategoryById/{id}")]
        [HttpGet]
        public IActionResult GetCategoryById(int id)
        {
            if (id <= 0)
                return BadRequest();
            var category = _db.Categories.Where(c => c.Id == id).FirstOrDefault();
            return Ok(category);
        }

        [HttpPost("AddCategories")]
        public IActionResult AddCategories([FromForm] CategoriesRequestDTO category)

        {
            var ImagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(ImagesFolder))
            {
                Directory.CreateDirectory(ImagesFolder);
            }
            var imageFile = Path.Combine(ImagesFolder, category.Image.FileName);
            using (var stream = new FileStream(imageFile, FileMode.Create))
            {
                category.Image.CopyToAsync(stream);
            }

            var c = new Category
            {
                CategoryName = category.CategoryName,
                Description = category.Description,
                Image = category.Image.FileName,
            };

            _db.Categories.Add(c);
            _db.SaveChanges();
            return Ok(c);
        }


        [HttpPut("UpdateCategory/{id}")]
        public IActionResult UpdateCategory(int id, [FromForm] CategoriesRequestDTO category)
        {
            var ImagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            var imageFile = Path.Combine(ImagesFolder, category.Image.FileName);
            using (var stream = new FileStream(imageFile, FileMode.Create))
            {
                category.Image.CopyToAsync(stream);
            }
            var c = _db.Categories.FirstOrDefault(l => l.Id == id);
            c.CategoryName = category.CategoryName;
            c.Description = category.Description;
            c.Image = category.Image.FileName;
            //c.CategoryImage = category.CategoryImage.FileName ?? c.CategoryImage;

            _db.Categories.Update(c);
            _db.SaveChanges();
            return Ok(c);

        }



        [HttpDelete("DeleteCategory/{id}")]
        public IActionResult Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var deletedProduct = _db.Products.Where(p => p.CategoryId == id).ToList();
            _db.RemoveRange(deletedProduct);
            _db.SaveChanges();

            var deleteCategory = _db.Categories.FirstOrDefault(x => x.Id == id);

            _db.Categories.Remove(deleteCategory);
            _db.SaveChanges();
            return NoContent();

        }

    }
}

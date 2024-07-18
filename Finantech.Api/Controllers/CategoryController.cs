using Finantech.Decorators;
using Finantech.DTOs.Category;
using Finantech.Enums;
using Finantech.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ExtractTokenInfo]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost("CreateCategory")]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var category = await _categoryService.CreateCategoryAsync(request, userId);

                return Created("", category);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        [HttpGet("GetAllCategories/{type?}")]
        public async Task<IActionResult> GetAllCategories(BillTypeEnum? type)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync(userId, type);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPatch("UpdateCategory")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                var categoryUpdated = await _categoryService.UpdateCategoryAsync(request, userId);

                return Ok(categoryUpdated);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("DeleteCategory/{categoryId}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int categoryId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            try
            {
                await _categoryService.DeleteCategoryAsync(categoryId, userId);

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}

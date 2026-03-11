using ControleCerto.Decorators;
using ControleCerto.DTOs.Category;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Extensions;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/categories")]
    [Authorize]
    [ExtractTokenInfo]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _categoryService.CreateCategoryAsync(request, userId);

            if (result.IsSuccess)
            {
                return Created($"/api/categories/{result.Value.Id}", result.Value);
            }
            else 
            {
                return result.HandleReturnResult();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories([FromQuery] BillTypeEnum? type)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _categoryService.GetAllCategoriesAsync(userId, type);

            return result.HandleReturnResult();
        }

        [HttpPatch("{categoryId:int}")]
        public async Task<IActionResult> UpdateCategory([FromRoute] int categoryId, [FromBody] UpdateCategoryRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            request.Id = categoryId;
            ModelState.Clear();
            TryValidateModel(request);

            var result = await _categoryService.UpdateCategoryAsync(request, userId);

            return result.HandleReturnResult();
        }

        [HttpDelete("{categoryId:int}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int categoryId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _categoryService.DeleteCategoryAsync(categoryId, userId);

            return result.HandleReturnResult();
        }


        [HttpPatch("{categoryId:long}/limit")]
        public async Task<IActionResult> UpdateCategoryLimit([FromRoute] long categoryId, [FromBody] UpdateCategoryLimitRequest data)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            data.CategoryId = categoryId;

            var result = await _categoryService.UpdateCategoryLimitAsync(data, userId);

            return result.HandleReturnResult();
        }

        [HttpGet("{categoryId:long}/limit")]
        public async Task<IActionResult> GetLimitInfo(long categoryId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _categoryService.GetLimitInfo(categoryId, userId);

            return result.HandleReturnResult();
        }

    }
}

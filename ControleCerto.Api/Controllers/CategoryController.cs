﻿using ControleCerto.Decorators;
using ControleCerto.DTOs.Category;
using ControleCerto.Enums;
using ControleCerto.Errors;
using ControleCerto.Extensions;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ControleCerto.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
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

            var result = await _categoryService.CreateCategoryAsync(request, userId);

            if (result.IsSuccess)
            {
                return Created("Category/GetAllCategories", result.Value);
            }
            else 
            {
                return result.HandleReturnResult();
            }
        }

        [HttpGet("GetAllCategories/{type?}")]
        public async Task<IActionResult> GetAllCategories(BillTypeEnum? type)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _categoryService.GetAllCategoriesAsync(userId, type);

            return result.HandleReturnResult();
        }

        [HttpPatch("UpdateCategory")]
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryRequest request)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _categoryService.UpdateCategoryAsync(request, userId);

            return result.HandleReturnResult();
        }

        [HttpDelete("DeleteCategory/{categoryId}")]
        public async Task<IActionResult> DeleteCategory([FromRoute] int categoryId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _categoryService.DeleteCategoryAsync(categoryId, userId);

            return result.HandleReturnResult();
        }


        [HttpPatch("UpdateCategoryLimit")]
        public async Task<IActionResult> UpdateCategoryLimit([FromBody] UpdateCategoryLimitRequest data)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _categoryService.UpdateCategoryLimitAsync(data, userId);

            return result.HandleReturnResult();
        }

        [HttpGet("GetLimitInfo/{categoryId}")]
        public async Task<IActionResult> GetLimitInfo(long categoryId)
        {
            int userId = (int)(HttpContext.Items["UserId"] as int?)!;

            var result = await _categoryService.GetLimitInfo(categoryId, userId);

            return result.HandleReturnResult();
        }

    }
}

﻿using Finantech.DTOs.User;
using Finantech.Models.Entities;
using Finantech.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace Finantech.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateUser([FromBody] CreateUserRequest data)
        {
            try 
            {
                var userInfo = await _userService.CreateUserAync(data);
                return CreatedAtAction("Auth/Authenticate", new { id = userInfo.Id }, userInfo);

            } catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            
        }
    }
}
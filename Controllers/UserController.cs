using MesDataCollection.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MesDataCollection.Controllers
{
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserRepository _userService;
        private readonly ILogger<UserController> _logger;

        public UserController( ILogger<UserController> logger, UserRepository userService)
        {
            _userService = userService;
            _logger = logger;
        }


        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> login(string passwort)
        {
            try
            {
                var user = await _userService.login(passwort);
                if (user != null)
                {
                    return Ok(user);
                }
                return StatusCode(400);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> getUser()
        {
            try
            {
                var users = await _userService.getUser();
                if (users == null || users.Count == 0)
                {
                    return Ok(new List<UserModel>());
                }

                Dictionary<string, string> typeMapping = new Dictionary<string, string>
                {
                  { "1", "看板用户" },
                  { "2", "管理员(管理生产计划)" },
                  { "3", "超级管理员（管理用户，管理生产计划）" }
                };

                List<UserModel> processedUsers = new List<UserModel>();
                foreach (var user in users)
                {
                    if (typeMapping.TryGetValue(user.type, out string typeName))
                    {
                        UserModel newUser = new UserModel
                        {
                            id = user.id,
                            password = user.password,
                            type = typeName,
                             create_date=user.create_date,
                             
                        };
                        processedUsers.Add(newUser);
                    }
                    else
                    {
                        processedUsers.Add(user);
                    }
                }
                
                return Ok(processedUsers);
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> UpdateUser(string id, string passwort)
        {
            try
            {
                await _userService.UpdateUser(id, passwort);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpGet]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> deleteUser(string id)
        {
            try
            {
                await _userService.deleteUser(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }

        [HttpPost]
        [Route("api/[controller]/[action]")]
        public async Task<IActionResult> addUser(UserModel  user)
        {
            try
            {
                await _userService.addUser(user);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex);
            }
        }


    }
}

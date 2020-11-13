using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Threading.Tasks;
using AccessBankTask.DTOs;
using AccessBankTask.Helpers;
using AccessBankTask.Models;
using AccessBankTask.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AccessBankTask.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        //private fields
        private readonly UserManager<User> _userManager;
        private readonly ILogger<AuthController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly ILogActivityRepository _logActivityRepository;

        public AuthController(IConfiguration configuration, SignInManager<User> signInManager, ILogActivityRepository logActivityRepository,
            UserManager<User> userManager, ILogger<AuthController> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _signInManager = signInManager;
            _userManager = userManager;
            _config = configuration;
            _logActivityRepository = logActivityRepository;
        }

        //register user
        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] AddUserDto model)
        {
            var userToRegister = _userManager.Users.FirstOrDefault(x => x.Email == model.Email);
            if (userToRegister != null)
                return BadRequest("Email already exist");

            var user = new User
            {
                UserName = model.Email,
                LastName = model.LastName,
                FirstName = model.FirstName,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
                return BadRequest(ModelState);
            }

            var currentIp = Utility.GetLocalIPAddress();
            var response = new LogActivity
            {
                UserId = user.Id,
                DeviceIp = currentIp,
                LoginTime = DateTime.Now
            };

            await _logActivityRepository.AddLogActivity(response);

            return Ok(user);

        }

        //login a user
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                var currentIp = Utility.GetLocalIPAddress();
                var isSignedIn = _signInManager.IsSignedIn(User);

                var user = _userManager.Users.FirstOrDefault(x => x.Email == model.Email);
                var userId = user.Id;

                //var ipAddress = await _logActivityRepository.GetIpAddress(userId, currentIp);

                var activityResponse = await _logActivityRepository.GetLogActivity(userId);

                if (isSignedIn && activityResponse.DeviceIp == currentIp && activityResponse.IsActive == true) return BadRequest("Already Signed In");

                // if the user has previously logged in on current device but logged out
                if (activityResponse.DeviceIp == currentIp && activityResponse.IsActive == false)
                {

                    activityResponse.LoginTime = DateTime.Now;
                    activityResponse.IsActive = true;

                    await _logActivityRepository.UpdateLogActivity(activityResponse);
                }
                else if (activityResponse == null)
                {
                    //this means the person is not on the log activity table and wants to login for the first time 
                    var response = new LogActivity
                    {
                        UserId = userId,
                        DeviceIp = currentIp,
                        LoginTime = DateTime.Now
                    };

                    await _logActivityRepository.AddLogActivity(response);
                }
                else
                {
                    // var IpAddressBool = await _logActivityRepository.GetIpAddress(activityResponse);
                    if (activityResponse.DeviceIp != currentIp)
                    {
                        return BadRequest("You are already logged in on another device. Do you want to logout?");
                        /*var response = new LogActivity
                        {
                            Id = activityResponse.Id,
                            IsActive = false,
                            LogoutTime = DateTime.Now
                        };

                        await _logActivityRepository.UpdateLogActivity(response);

                        var newResponse = new LogActivity
                        {
                            UserId = userId,
                            DeviceIp = currentIp,
                            LoginTime = DateTime.Now
                        };

                        await _logActivityRepository.AddLogActivity(newResponse);*/
                    }
                }
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    var getToken = JwtTokenConfig.GetToken(user, _config);
                    return Ok(new { message = "Logged in succesfully", token = getToken });
                }
                return Unauthorized("Incorrect username or Password");
                
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("An error occured");
            }

        }

        //login a user with a new device
        //[HttpPost("loginnewdevice")]
        //public async Task<IActionResult> LoginWithNewDevice([FromBody] LoginDto model)
        //{
        //    try
        //    {
        //        var currentIp = Utility.GetLocalIPAddress();
        //        var user = _userManager.Users.FirstOrDefault(x => x.Email == model.Email);
        //        var userId = user.Id;
        //        var activityResponse = await _logActivityRepository.GetLogActivity(userId);
        //        //set the isActive of the user in the log activity table to false
        //        //then login and the device ip newly

        //        if (activityResponse.DeviceIp != currentIp)
        //        {
        //            var response = new LogActivity
        //            {
        //                Id = activityResponse.Id,
        //                IsActive = false,
        //                LogoutTime = DateTime.Now
        //            };

        //            await _logActivityRepository.UpdateLogActivity(response);

        //            var newResponse = new LogActivity
        //            {
        //                UserId = userId,
        //                DeviceIp = currentIp,
        //                LoginTime = DateTime.Now
        //            };

        //            await _logActivityRepository.AddLogActivity(newResponse);
        //        }
        //        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
        //        if (result.Succeeded)
        //        {
        //            var getToken = JwtTokenConfig.GetToken(user, _config);
        //            return Ok(new { message = "Logged in succesfully with new device", token = getToken });
        //        }
        //        return Unauthorized("Incorrect username or Password");
        //    }
        //    catch (Exception e)
        //    {

        //        _logger.LogError(e.Message);
        //        return BadRequest("Error occured");
        //    }
        //}
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            //var userId = Utility.GetUserIdFromToken(HttpContext);
            var user = await _userManager.GetUserAsync(User);
            var activityResponse = await _logActivityRepository.GetActiveLogActivity(user.Id);

            activityResponse.IsActive = false;
            activityResponse.LogoutTime = DateTime.Now;

            await _logActivityRepository.UpdateLogActivity(activityResponse);
            await _signInManager.SignOutAsync();
            return Ok("Logged out successfully");
        }

    }
}

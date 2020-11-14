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
        [Obsolete]
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
            Console.WriteLine(currentIp);
            var response = new LogActivity
            {
                UserId = user.Id,
                DeviceIp = currentIp
            };

            await _logActivityRepository.AddLogActivity(response);

            return Ok(user);

        }

        //login a user
        [AllowAnonymous]
        [HttpPost("login")]
        [Obsolete]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            try
            {
                // get device ip
                var currentIp = Utility.getMac();
                Console.WriteLine(currentIp);
                //var isSignedIn = _signInManager.IsSignedIn(User);

                var user = _userManager.Users.FirstOrDefault(x => x.Email == model.Email);
                if (user == null) return BadRequest("User does not exist");

                var activityResponse = await _logActivityRepository.GetLogActivity(user.Id);
                if (activityResponse == null) return BadRequest("No registered user");

                if (activityResponse.DeviceIp == currentIp && activityResponse.IsActive) return BadRequest("Already Signed In");

                // if the user has previously logged in on current device but logged out
                if (activityResponse.DeviceIp == currentIp && !activityResponse.IsActive)
                {
                    activityResponse.LoginTime = DateTime.Now;
                    activityResponse.IsActive = true;

                    await _logActivityRepository.UpdateLogActivity(activityResponse);
                }
                else
                {
                    return BadRequest("You are already logged in on another device. Do you want to logout?");
                }
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    var getToken = JwtTokenConfig.GetToken(user, _config);
                    return Ok(new { message = "Logged in succesfully", token = getToken });
                }
                return Unauthorized("Incorrect username or Password");





                /*// get device ip
                var currentIp = Utility.GetLocalIPAddress();
                Console.WriteLine(currentIp);
                //var isSignedIn = _signInManager.IsSignedIn(User);

                var user = _userManager.Users.FirstOrDefault(x => x.Email == model.Email);
                if (user == null) return BadRequest("User does not exist"); 

                var activityResponse = await _logActivityRepository.GetLogActivity(user.Id);
                if (activityResponse == null) return BadRequest("No registered user");

                if (activityResponse.DeviceIp == currentIp && activityResponse.IsActive) return BadRequest("Already Signed In");

                // if the user has previously logged in on current device but logged out
                if (activityResponse.DeviceIp == currentIp && !activityResponse.IsActive)
                {
                    activityResponse.LoginTime = DateTime.Now;
                    activityResponse.IsActive = true;

                    await _logActivityRepository.UpdateLogActivity(activityResponse);
                }
                else
                {
                    return BadRequest("You are already logged in on another device. Do you want to logout?");
                }
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);
                if (result.Succeeded)
                {
                    var getToken = JwtTokenConfig.GetToken(user, _config);
                    return Ok(new { message = "Logged in succesfully", token = getToken });
                }
                return Unauthorized("Incorrect username or Password");*/
                
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return BadRequest("An error occured");
            }

        }

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
            return Ok(new { message = "Logged out succesfully"});
        }

    }
}

// Controllers/OtpController.cs
using Microsoft.AspNetCore.Mvc;
using EmailOtpApp.Services;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore;
using Microsoft.AspNetCore.Http.HttpResults;

namespace EmailOtpApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OtpController : ControllerBase
    {
        private static EmailOtpModule _otpModule = new EmailOtpModule();

        [HttpGet("generate")]
        public IActionResult GenerateOTP() 
        {
            //return StatusCode(_otpModule.GenerateRandomOtp());
            return Ok(_otpModule.GenerateRandomOtp());
        }

        [HttpPost("send")]
        public IActionResult SendOtp([FromBody] string email)
        {
            _otpModule.Start();
            var result = _otpModule.GenerateOtpEmail(email);
            //_otpModule.Close();
            return Ok(result);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerificationRequest request)
        {
            var result = await _otpModule.CheckOtpAsync(request.Otp);
            //_otpModule.Close();
            return StatusCode(result);
        }
    }

    public class OtpVerificationRequest
    {
        public string Otp { get; set; }
    }
}

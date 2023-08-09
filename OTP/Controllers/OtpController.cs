using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace OTP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly TwilioSettings _twilioSettings;

        public OtpController(IOptions<TwilioSettings> twilioSettings)
        {
            _twilioSettings = twilioSettings.Value;
        }

        [HttpPost("send")]
        public IActionResult SendOtp(string phoneNumber)
        {
            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);

            var parameters = new List<KeyValuePair<string, string>>
             {
                new KeyValuePair<string, string>("To", phoneNumber),
                new KeyValuePair<string, string>("Channel", "sms")
            };

            var content = new FormUrlEncodedContent(parameters);

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_twilioSettings.AccountSid}:{_twilioSettings.AuthToken}"));
            var authHeader = new AuthenticationHeaderValue("Basic", credentials);

            using (var httpClient = new HttpClient())
            {

                httpClient.DefaultRequestHeaders.Authorization = authHeader;

                var response = httpClient.PostAsync("https://verify.twilio.com/v2/Services/VAd7fb0faec1034450ff201315d4635cc5/Verifications", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    return Ok("OTP sent successfully");
                }
                else
                {
                    return BadRequest("Failed to send OTP");
                }
            }
        }
        [HttpPost("verify")]
        public IActionResult VerifyOtp(string phoneNumber, string otpCode)
        {
            TwilioClient.Init(_twilioSettings.AccountSid, _twilioSettings.AuthToken);

            var parameters = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("To", phoneNumber),
                new KeyValuePair<string, string>("Code", otpCode)
            };

            var content = new FormUrlEncodedContent(parameters);

            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_twilioSettings.AccountSid}:{_twilioSettings.AuthToken}"));
            var authHeader = new AuthenticationHeaderValue("Basic", credentials);

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = authHeader;

                var response = httpClient.PostAsync("https://verify.twilio.com/v2/Services/VAd7fb0faec1034450ff201315d4635cc5/VerificationCheck", content).Result;

                if (response.IsSuccessStatusCode)
                {
                    return Ok("OTP verification successful");
                }
                else
                {
                    return BadRequest("OTP verification failed");
                }
            }
        }
    }
}

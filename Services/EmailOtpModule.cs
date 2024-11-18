// Services/EmailOtpModule.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailOtpApp.Services
{
    public class EmailOtpModule
    {
        private string _currentOtp;
        private DateTime _otpSentTime;
        private const int OtpValidityDuration = 60; // in seconds
        private const int MaxAttempts = 10;
        private Message emailMessage; 

        public void Start()
        {
            _currentOtp = null;
            _otpSentTime = DateTime.MinValue;
            emailMessage = new Message("", "", StatusCode.STATUS_OTP_FAIL,"");

        }

        public void Close()
        {
            _currentOtp = null;
            _otpSentTime = DateTime.MinValue;
            emailMessage = null;
        }

        public Message GenerateOtpEmail(string userEmail)
        {
            if (!IsValidEmail(userEmail))
            {
                emailMessage.emailAddress = userEmail;
                emailMessage.emailBody = "Invalid Email";
                emailMessage.statusCode = StatusCode.STATUS_EMAIL_INVALID;
                return emailMessage;
            }

            _currentOtp = GenerateRandomOtp();
            _otpSentTime = DateTime.Now;
            string emailBody = $"Your OTP Code is {_currentOtp}. The code is valid for 1minute.";
            //string emailBody = $"Your OTP Code is {_currentOtp}. The code is valid for {OtpValidityDuration} seconds.";
            emailMessage.emailAddress = userEmail;
            emailMessage.emailBody = emailBody;
            emailMessage.otp = _currentOtp;

            try
            {
                SendEmail(userEmail, emailBody);
                emailMessage.statusCode = StatusCode.STATUS_EMAIL_OK;
                return emailMessage;
            }
            catch (Exception) 
            {
                emailMessage.statusCode =  StatusCode.STATUS_EMAIL_FAIL;
                return emailMessage;
            }
        }

        public async Task<int> CheckOtpAsync(string userInput)
        {
            for (int attempts = 0; attempts < MaxAttempts; attempts++)
            {
                if ((DateTime.Now - _otpSentTime).TotalSeconds > OtpValidityDuration)
                {
                    this.Close();
                    return StatusCode.STATUS_OTP_TIMEOUT;
                    
                }

                if (userInput == _currentOtp)
                {
                    return StatusCode.STATUS_OTP_OK;
                }
            }
            return StatusCode.STATUS_OTP_FAIL;
        }

        private bool IsValidEmail(string email)
        {
            return email.EndsWith(".dso.org.sg", StringComparison.OrdinalIgnoreCase);
        }

        public string GenerateRandomOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // Generates a random 6-digit number
        }

        private void SendEmail(string emailAddress, string emailBody)
        {
            using (var message = new MailMessage("noreply@dso.org.sg", emailAddress))
            {
                message.Subject = "Your OTP Code";
                message.Body = emailBody;
                using (var client = new SmtpClient("smtp.dso.org.sg"))
                {
                    //SMTP Configuration 
                    //client.Send(message);
                }
            }
        }
    }

    public static class StatusCode
    {
        public const int STATUS_EMAIL_OK = 200;
        public const int STATUS_EMAIL_FAIL = 400;
        public const int STATUS_EMAIL_INVALID = 401;
        public const int STATUS_OTP_OK = 200;
        public const int STATUS_OTP_FAIL = 403;
        public const int STATUS_OTP_TIMEOUT = 408;
    }

    public class Message
    {
        public string emailAddress { get; set; }
        public string emailBody { get; set; }
        public int statusCode {get; set;}
        public string otp { get; set; }
        public Message(string emailAddress, string emailBody, int statusCode, string otp) {
            this.emailAddress = emailAddress;
            this.emailBody = emailBody;
            this.statusCode = statusCode;
            this.otp = otp;
        }
    }
}


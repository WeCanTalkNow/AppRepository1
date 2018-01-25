//using System;
//using System.Configuration;
//using System.Web.Mvc;
//using Twilio;
//using Twilio.Types;
//using Twilio.AspNet.Mvc;
//using Twilio.Rest.Api.V2010.Account;


//namespace CallingService.Voice.Controllers
//{
//    public class OutGoingCallController : Controller
//    {
//        /// <summary>
//        /// Makecall is a sampl eillustration for making an outbound call using the 
//        /// This does not required to be hosted. Can be run directly 
//        /// since we are using localhost to make an outbound call
//        /// </summary>
//        /// <returns></returns>
//        public ActionResult MakeCall()
//        {
//            var accountId = ConfigurationManager.AppSettings["TwilioAccountSid"];
//            var accountToken = ConfigurationManager.AppSettings["TwilioAccountToken"];

//            var fromPhoneNumber = new PhoneNumber("+14159157316");
//            var toPhoneNumber = new PhoneNumber("+919818500936");

//            TwilioClient.Init(accountId, accountToken);

//            var ccOptions = new CreateCallOptions(toPhoneNumber, fromPhoneNumber);
            
//            var call = CallResource.Create(       
//                toPhoneNumber,
//                fromPhoneNumber,                
//                // The URL Twilio will request when the call is answered
//                url: new Uri("http://demo.twilio.com/welcome/voice/")
//            );
//            Response.Write(string.Format($"Started call: {call.StartTime.ToString()}"));
//            Response.Write(call.StartTime.ToString());
//            return Content(call.Status.ToString());
            
//        }
//    }
//}
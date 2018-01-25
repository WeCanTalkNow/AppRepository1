using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Twilio.TwiML;
using Twilio.AspNet.Mvc;

using System.Web.Mvc;
using CallngService.DataModel.Contexts;

namespace CallingService.SMS.Controllers
{
    public class AvailabilityController : TwilioController
    {
        [HttpPost]
        // GET: CheckAvailability
        public ActionResult Index()
        {
            var response = new MessagingResponse();
            var strResponse = string.Empty;
            
            strResponse = EvaluateMessage(smsMessageContents);
            
            GenerateREsponse(ref response,smsFromPhonenumber, strResponse);

            return TwiML(response);
        }

        private void GenerateREsponse(ref MessagingResponse response, string smsFromPhonenumber, string strResponse)
        {
            var message = new Message();

            response.Message(strResponse, smsFromPhonenumber, twilioPhoneNumber);
            //response.Message(smsResponse, "919818500936", twilioPhoneNumber);
            //response.Message()
        }

        private string EvaluateMessage(string smsMessageContents)
        {
            string strResponse;
            switch (smsMessageContents.ToUpper().Trim())
            {
                case "STATUS":   // Return current status
                    strResponse = returnStatus();
                    break;

                case "STATUS 1": // Update Status to Available
                    strResponse = updateStatus(1);
                    break;

                case "STATUS 0": // Update Status to not available
                    strResponse = updateStatus(0);
                    break;

                default:
                    strResponse =  "Invalid Command. Keywords - Status for retreving status, Status 0 for updating status to not available, status 1 for updating status to available";
                    break;
            }
            return strResponse;
        }
        

        private string updateStatus(int v)
        {
            throw new NotImplementedException();
        }

        private string returnStatus()
        {
            var retVal = String.Empty;
            // Check this code. It is crashing

            using (var context = new UserContext())
            {
                var user =
                            context
                            .User
                                .Where(u => u.TwilioPhoneNumber == twilioPhoneNumber.ToString().Substring(1)).FirstOrDefault();
                retVal = Convert.ToBoolean(user.AvailableStatus) ? "Available" : "Not Available";
            }
             return retVal;

        }

        public string smsMessageContents
        {
            get { return (!string.IsNullOrEmpty(Request.Params["Body"])) ? Request.Params["Body"].ToString(): string.Empty; }
        }

        public string twilioPhoneNumber
        {
            get { return (!string.IsNullOrEmpty(Request.Params["To"])) ? Request.Params["To"].ToString() : string.Empty; }
        }

        public string smsFromPhonenumber
        {
            get { return (!string.IsNullOrEmpty(Request.Params["From"])) ? Request.Params["From"].ToString() : string.Empty; }
        }

        //    return Content(
        //string.Format("<Response><Sms>{</Sms></Response>",
        //Request.Params["Body"]));
}   
}
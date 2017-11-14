using System;
using System.Linq;
using Twilio.TwiML;
using Twilio.AspNet.Mvc;
using System.Web.Mvc;
using TwilioConference.DataServices;
using Twilio.Rest.Api.V2010.Account;
using Twilio;

namespace CallingService.SMS.Controllers
{
    public class AvailabilityController : TwilioController
    {
        TwilioConferenceServices twilioConferenceServices;

        public AvailabilityController()
        {
            twilioConferenceServices = new TwilioConferenceServices();            
        }



        [HttpPost]
        public ActionResult Check()
        {
            var response = new MessagingResponse();
            var message = new Message();
            var strResponse = string.Empty;

            strResponse = EvaluateMessage(smsMessageContents);
            response.Message(strResponse, smsFromPhonenumber, twilioPhoneNumber);

            return TwiML(response);
        }

        #region Methods

        private string EvaluateMessage(string smsMessageContents)
        {
            string strResponse;
            switch (smsMessageContents.ToUpper().Trim())
            {
                case "STATUS":   // Return current status
                    strResponse =  twilioConferenceServices.returnStatus(twilioPhoneNumber);
                    break;

                case "STATUS 1": // Update Status to Available
                    strResponse = twilioConferenceServices.updateStatus(1,twilioPhoneNumber);
                    break;

                case "STATUS 0": // Update Status to not available
                    strResponse = twilioConferenceServices.updateStatus(0, twilioPhoneNumber);
                    break;

                default:
                    strResponse = "Invalid Command. Keywords are Status = retrieving status, Status 0 =  updating status to not available, status 1 =  updating status to available";
                    break;
            }
            return strResponse;
        }

         #endregion

        #region Properties

        public string smsMessageContents
        {
            get { return (!string.IsNullOrEmpty(Request.Params["Body"])) ? Request.Params["Body"].ToString() : string.Empty; }
        }

        public string twilioPhoneNumber
        {
            get { return (!string.IsNullOrEmpty(Request.Params["To"])) ? Request.Params["To"].ToString() : string.Empty; }
        }

        public string smsFromPhonenumber
        {
            get { return (!string.IsNullOrEmpty(Request.Params["From"])) ? Request.Params["From"].ToString() : string.Empty; }
        } 
        
        #endregion
    }

}
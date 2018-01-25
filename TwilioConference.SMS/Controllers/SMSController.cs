using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio.AspNet.Mvc;
using Twilio.AspNet.Common;
using Twilio.TwiML;
using CallingService.DataModel.Queries;
using CallingService.DataModel.Services;
using CallingService.DataModel.Classes;



namespace CallingService.Voice.Controllers
{
    public class SMSController : TwilioController
    {
        // GET: SMS

        ConferenceService conferenceServices;


        [HttpPost]
        public ActionResult Check(SmsRequest request)
        {
            Response.ContentType = "text/xml";
            var response = new MessagingResponse();
            //var message = new Message();
            var strResponse = string.Empty;
            var smsMC = request.Body.ToString();
            
            //response.Message(EvaluateMessage(request.Body.ToString().Substring(1).ToUpper().Trim()), smsFromPhonenumber, twilioPhoneNumber);
            response.Message(EvaluateMessage(request.Body.ToString().ToUpper().Trim()), smsFromPhonenumber, twilioPhoneNumber);
            //response.Message("Test message", smsFromPhonenumber, twilioPhoneNumber);
            return TwiML(response);

        }

        public SMSController()
        {
            conferenceServices = new ConferenceService();
            //conferenceServices.LogMessage("In constructor");

        }

        private string EvaluateMessage(string sMc)
        {
            var strResponse="";
            try
            {
                //conferenceServices.LogMessage(string.Format("Evaluate message {0} {1}", twilioPhoneNumber, sMc));

                //conferenceServices.LogMessage("about to enter switch");

                //conferenceServices.LogMessage(sMc);

                //if (string.Equals(sMc.ToUpper().Trim(), "STATUS"))
                //    conferenceServices.LogMessage("OK Check");

                //if (sMc.Trim().ToUpperInvariant() == "STATUS")
                //    conferenceServices.LogMessage("OK Check");

                

                switch  (sMc)
                {
                    case "STATUS":   // Return current status
//                        conferenceServices.LogMessage("Entering here STATUS  ");
                        strResponse = conferenceServices.returnStatus(twilioPhoneNumber);
                        break;

                    case "STATUS 1": // Update Status to Available
//                        conferenceServices.LogMessage("Entering here STATUS 1 ");
                        strResponse = conferenceServices.updateStatus(1, twilioPhoneNumber);
                        break;

                    case "STATUS 0": // Update Status to not available
//                        conferenceServices.LogMessage("Entering here STATUS 0 ");
                        strResponse = conferenceServices.updateStatus(0, twilioPhoneNumber);
                        break;

                    default:
                        strResponse = "Invalid Command. Keywords are Status = retrieving status, Status 0 =  updating status to not available, status 1 =  updating status to available";
                        break;
                }

                conferenceServices.LogMessage(sMc, smsFromPhonenumber, twilioPhoneNumber, strResponse);

            }
            catch (Exception ex)
            {
                conferenceServices.ErrorMessage(string.Format("|Error Message - {0}| 1.Source {1} | 2.Trace {2} |3.Inner Exception {3} |",
                   ex.Message,
                   ex.Source,
                   ex.StackTrace,
                   ex.InnerException));
                throw;

            }
            return strResponse;
        }

        //[System.Web.Mvc.HttpPost]
        //public ActionResult Index()
        //{
        //    return View();
        //}

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


    }
}
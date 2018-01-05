using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Web.Mvc;
using Twilio;
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.Types;
using TwilioConference.DataServices;
using TwilioConference.DataServices.Entities;
using TwilioConference.Models;
using NodaTime;
using Twilio.TwiML.Messaging;

namespace TwilioConference.Controllers
{
    public class TwilioSMSController : TwilioController
    {

        TwilioConferenceServices conferenceServices;
        string _smsMessageContents = "";

        public TwilioSMSController()
        {
            conferenceServices = new TwilioConferenceServices();
            conferenceServices.LogMessage("In constructor");

        }

        [HttpPost]
        public ActionResult CheckBak()
        {
            var response = new MessagingResponse();
            var message = new Message();
            var strResponse = string.Empty;
            Response.ContentType = "text/xml";
            conferenceServices.LogMessage(string.Format("In check {0}",twilioPhoneNumber));
            conferenceServices.LogMessage(string.Format("smsMessageContents is {0}", smsMessageContents));
            strResponse = EvaluateMessage(smsMessageContents);
            if (smsMessageContents == "?Status") conferenceServices.LogMessage("Okay!!!");
            response.Message(strResponse, smsFromPhonenumber, twilioPhoneNumber);
            conferenceServices.LogMessage(string.Format("In Check {0},{1},{2}",strResponse,smsFromPhonenumber,twilioPhoneNumber));
            return TwiML(response);
        }


        [HttpPost]
        public ActionResult Check(SmsRequest request)
        {
            var response = new MessagingResponse();
            var message = new Message();
            var strResponse = string.Empty;
            
            strResponse = EvaluateMessage(smsMessageContents);
            response.Message(strResponse, smsFromPhonenumber, twilioPhoneNumber);

            return TwiML(response);
        }
        //internal string smsMessageContents
        //{
        //    get
        //    {
        //        _smsMessageContents = _smsMessageContents ??
        //             Request.Params["Body"].ToString();
        //        return _smsMessageContents;
        //    }
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


        private string EvaluateMessage(string sMc)
        {
            string strResponse;
            try
            {
                conferenceServices.LogMessage(string.Format("Evaluate message {0} {1}", twilioPhoneNumber, sMc));

                conferenceServices.LogMessage("about to enter switch");

                conferenceServices.LogMessage(sMc);

                if (string.Equals(sMc.ToUpper().Trim(), "STATUS"))
                    conferenceServices.LogMessage("OK Check");

                if (sMc.Trim().ToUpperInvariant() == "STATUS")
                    conferenceServices.LogMessage("OK Check");

                conferenceServices.LogMessage(sMc.ToUpper().Trim());

                switch (sMc.ToUpper().Trim())
                {
                    case "STATUS" :   // Return current status
                        conferenceServices.LogMessage("Entering here");
                        strResponse = conferenceServices.returnStatus(twilioPhoneNumber);
                        break;

                    case "STATUS 1" : // Update Status to Available
                        strResponse = conferenceServices.updateStatus(1, twilioPhoneNumber);
                        break;

                    case "STATUS 0" : // Update Status to not available
                        strResponse = conferenceServices.updateStatus(0, twilioPhoneNumber);
                        break;

                    default:
                        strResponse = "Invalid Command. Keywords are Status = retrieving status, Status 0 =  updating status to not available, status 1 =  updating status to available";
                        break;
                }

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


        

    }
}

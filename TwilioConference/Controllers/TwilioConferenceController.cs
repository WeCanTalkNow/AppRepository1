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

namespace TwilioConference.Controllers
{
    public class TwilioConferenceController : TwilioController
    {
        static readonly string TWILIO_ACCOUNT_SID = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_SID"];
        static readonly string TWILIO_ACCOUNT_TOKEN = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_TOKEN"];
        static readonly string TWILIO_NUMBER = ConfigurationManager.AppSettings["TWILIO_NUMBER"];

        //To get the location the assembly normally resides on disk or the install directory
        static string strTargetTimeZoneID = "";
        static string strTwilioPhoneNumber = "14159186634";
        static Boolean AVAILABILITY_CHECK_DONE = false;
        static string WEBROOT_PATH = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        static string WEB_BIN_ROOT = System.IO.Path.GetDirectoryName(WEBROOT_PATH);
        static string WEB_JOBS_DIRECTORY = System.IO.Path.GetFullPath("D:\\home\\site\\wwwroot\\app_data\\jobs\\triggered\\TwilioConferenceTimer\\Webjob");
        //D:\home\site\wwwroot\app_data\jobs\triggered\TwilioConferenceTimer
        //D:\home\site\wwwroot\App_Data\jobs\triggered\TwilioConferenceTimer\Webjob

        static string TIMER_EXE = Path.Combine(WEB_JOBS_DIRECTORY, "TwilioConference.Timer.exe");

        TwilioConferenceServices conferenceServices;
        //private CallResource _crCurrentCall;
        //private string _strCallSID;
        //private string _strCallerID;


        public TwilioConferenceController()
        {
            TwilioClient.Init(TWILIO_ACCOUNT_SID, TWILIO_ACCOUNT_TOKEN);
            conferenceServices = new TwilioConferenceServices();
        }

        // Step 1. Person1 calls Twilio number 1 (connects to /Connect api)
        // Step 2. Twilio number 1 creates random conference call name
        // 3. Join Person 1 to this random conference call
        // 4. Next call Person2 from Twilio number 2
        // 5. When person 2 answers join person 2 to random conference call
        // 6. Schedule 9 minute join and say timer
        // 7. At 9 minutes call into the conference from another twilio number(bot) (should this be twilio number 3? or is reuse of 1 or 2 possible)
        // 8. Bot lets all conference participants know that it will be hanging up th conference in 1 minute
        // 9. Hangup Conference at 10 minutes

        // POST: Conference/Connect
        [System.Web.Mvc.HttpPost]
        public ActionResult Connect(VoiceRequest request)  // Step 1.
        {
            var response = new VoiceResponse();
            string from = request.From;
            string strCallServiceUserName = "";
            string strUserDialToPhoneNumber = "";                       
            Boolean IsUserAvailableToTakeCalls = true;
             
            if (!AVAILABILITY_CHECK_DONE)
            {
                try
                {
                    conferenceServices.LogMessage("In availability check");
                    IsUserAvailableToTakeCalls = conferenceServices.
                            CheckAvailabilityAndFetchDetails(
                                ref strCallServiceUserName,
                                        ref strUserDialToPhoneNumber,
                                            ref strTargetTimeZoneID,
                                                    strTwilioPhoneNumber);
                    conferenceServices.LogMessage("Availability check done");
                    //conferenceServices.LogMessage(string.Format("strTwilioPhoneNumber {0} strCallFromPhoneNumber, {1} strCallServiceUserName {2} IsUserAvailableToTakeCalls {3} strUserDialToPhoneNumber {4}",
                    //                                             strTwilioPhoneNumber, strCallFromPhoneNumber, strCallServiceUserName, IsUserAvailableToTakeCalls, strUserDialToPhoneNumber));

                    AVAILABILITY_CHECK_DONE = true;
                    if (!IsUserAvailableToTakeCalls)
                    {
                        response.Say(string.Concat(strCallServiceUserName,
                                      "is not available to take calls at this time. ",
                                       "Please check back with this user about their available call times"));
                        response.Hangup();
                        return TwiML(response);
                    }
                }
                catch (Exception ex)
                {
                    //conferenceServices.ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
                    //    ex.Message, ex.Source, ex.StackTrace, ex.InnerException));
                }
            }





            if (from.Contains("4159186649"))
            {
                conferenceServices.LogMessage("Connected from TWILIO NUMBER controller");
                string conferenceName = "mango";

                try
                {
                   conferenceName = conferenceServices.GetMostRecentConferenceNameFromNumber();
                }
                catch(Exception ex)
                {
                    conferenceServices.LogMessage(ex.Message);
                }
                
                var dial = new Dial();
                dial.Conference(conferenceName);
                response.Dial(dial);

                return new TwiMLResult(response);
            }
            else
            {
                // On first call the control flow should be here
                conferenceServices.LogMessage("Connected from " + from);
                string phone1 = from; // This is phone1 the person that calls the twilo number
                string phone2 = "+911142345253"; //You would get this from the database in advance. This is phone 2 your known number.
                
                string conferenceName = GetRandomConferenceName(); // Step 2.
                
                TwilioConferenceCall conferenceRecord =
                    conferenceServices
                    .CreateTwilioConferenceRecord(phone1, phone2, conferenceName, request.CallSid);

                response.Say("You are about to join a conference call");
                response.Say("We are going to conference you in with :");

                foreach (var digit in conferenceRecord.Phone2)
                {
                    response.Say(digit.ToString());
                }
                
                // Connect phone1 to conference // 3.
                var dial = new Dial();
                dial.Conference(conferenceName
                    , waitUrl: "http://twimlets.com/holdmusic?Bucket=com.twilio.music.ambient"
                    , statusCallbackEvent: "start end join"
                    , statusCallback: string.Format("http://callingserviceconference.azurewebsites.net/twilioconference/HandleConferenceStatusCallback?id={0}", conferenceRecord.Id)
                    , statusCallbackMethod: "POST"
                    , startConferenceOnEnter: true
                    , endConferenceOnExit: true);
                response.Dial(dial);
            }

            return TwiML(response);
        }

        [System.Web.Mvc.HttpPost]
        public TwiMLResult HandleConferenceStatusCallback(TConferenceRequest request)
        {
            //Uncomment below to print full
            //request body

            //var req = Request.InputStream;
            //var json = new StreamReader(req).ReadToEnd();
            //conferenceServices.LogMessage(json);
            
            var response = new VoiceResponse();
            
            string conferenceRecordId = Request.QueryString["id"];
            int id = Int32.Parse(conferenceRecordId);

            TwilioConferenceCall conferenceRecord = conferenceServices.GetConferenceRecord(id);

            var conferenceStatus = request.StatusCallbackEvent;
            //Check to see if caller 1 joined
            if (conferenceStatus == "participant-join"
                && request.CallSid == conferenceRecord.PhoneCall1SID) // 4.
            {
                // The first caller has joined
                // kick off job to call 2nd caller
                Connect(conferenceRecord.Phone2, conferenceRecord.ConferenceName, id);
            }
            if (conferenceStatus == "conference-start") //Caller 2 has joined start the timers
            {
                conferenceRecord.ConferenceSID = request.ConferenceSid;
                conferenceServices.UpdateConferenceSid(conferenceRecord);
                try
                {
                    conferenceServices.UpdateCallStartTime(id);
                    conferenceServices.LogMessage(string.Format("{0} - {1}"
                        , conferenceStatus
                        , request.ConferenceSid), id);

                    //Start the timer for the 9minute message
                    try
                    {
                        conferenceServices.LogMessage("Manually invoke the timer", id);
                        conferenceServices.LogMessage(TIMER_EXE, id);
                        conferenceServices.LogMessage("conference record id " + conferenceRecordId, id);
                        ProcessStartInfo processStartInfo = new ProcessStartInfo(TIMER_EXE, conferenceRecordId.ToString());
                        Process.Start(processStartInfo);

                    }
                    catch (Exception ex)
                    {
                        conferenceServices.LogMessage(ex.Message, id);
                    }
                }
                catch (Exception ex)
                {
                    conferenceServices.LogMessage(ex.Message, id);
                }
                finally
                {
                    conferenceServices.LogMessage(string.Format("{0} - {1} - Got this far"
                    , conferenceStatus
                    , request.ConferenceSid), id);
                }
            }
            else
            {
                conferenceServices.LogMessage(string.Format("{0} - {1}"
                    , conferenceStatus
                    , request.ConferenceSid), id);
            }

            return new TwiMLResult();
        }

        //[System.Web.Mvc.HttpPost]
        public ActionResult ConnectTwilioBot(VoiceRequest request)
        {
            var response = new VoiceResponse();
            conferenceServices.LogMessage("ConnectFromTwilio Bot Called "+ DateTime.Now.ToShortTimeString());
            Response.ContentType = "text/xml";
            //response.Pause(3);
            response.Say("Sorry to interrupt guys but just wanted to let you know that your conference would be ending in 1 minute");
            response.Hangup();            
            return new TwiMLResult(response);
        }

        string Connect(string phoneNumber, string conferenceName, int conferenceRecordId)
        {
            var call = CallResource.Create(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(TWILIO_NUMBER),
                url: new Uri(string.Format("http://callingserviceconference.azurewebsites.net/twilioconference/ConferenceInPerson2?conferenceName={0}&id={1}" // 5.
                , conferenceName, conferenceRecordId)));//new System.Uri("/response.xml", System.UriKind.Relative));

            return call.Sid;
        }


        public TwiMLResult ConferenceInPerson2(VoiceRequest request)
        {
            string conferenceName = Request.QueryString["conferenceName"];
            int id = Int32.Parse(Request.QueryString["id"]);

            TwilioConferenceCall conferenceRecord = conferenceServices.GetConferenceRecord(id);

            var response = new VoiceResponse();
            //response.Pause(5);
            response.Say("You are about to join a conference call");
            response.Say("We are going to conference you in with :");

            foreach (var digit in conferenceRecord.Phone1)
            {
                response.Say(digit.ToString());
            }

            var dial = new Dial();
            dial.Conference(conferenceName);
            response.Dial(dial);

            return new TwiMLResult(response);
        }

        //internal string strCallerId
        //{
        //    get
        //    {
        //        _strCallerID = _strCallerID ??
        //                (!string.IsNullOrEmpty(Request.Params["Called"])
        //                ? Request.Params["Called"].ToString().Substring(1)
        //                : string.Empty);
        //        return _strCallerID;
        //    }

        //}


        //internal string strCallSID
        //{
        //    get
        //    {
        //        _strCallSID = _strCallSID ??
        //                (!string.IsNullOrEmpty(Request.Params["CallSid"])
        //                ? Request.Params["CallSid"].ToString()
        //                : string.Empty);

        //        return _strCallSID;
        //    }
        //}


        //internal CallResource crCurrentCall
        //{
        //    get
        //    {
        //        _crCurrentCall = _crCurrentCall ??
        //             CallResource.Fetch(strCallSID);
        //        return _crCurrentCall;
        //    }
        //}


        string GetRandomConferenceName()
        {
            Random rnd = new Random();

            string[] fruits = new string[] { "apple"
                , "mango"
                , "papaya"
                , "banana"
                , "guava"
                , "pineapple"
                , "orange" };
            return fruits[rnd.Next(0, fruits.Length)] + DateTime.Now.Ticks;
        }

    }
}

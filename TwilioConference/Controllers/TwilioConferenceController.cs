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

namespace TwilioConference.Controllers
{
    public class TwilioConferenceController : TwilioController
    {
        // For API authorization
        static readonly string TWILIO_ACCOUNT_SID = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_SID"];
        static readonly string TWILIO_ACCOUNT_TOKEN = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_TOKEN"];

        // This is the TWILIO phone number used to initiate the conference
        static readonly string SERVICE_USER_TWILIO_PHONE_NUMBER = ConfigurationManager.AppSettings["SERVICE_USER_TWILIO_PHONE_NUMBER"];

        // This is the TWILIO phone number used as a BOT to play the message
        static readonly string TWILIO_BOT_NUMBER = ConfigurationManager.AppSettings["TWILIO_BOT_NUMBER"];

        // This is the Number of the registered user on which the conference call will be received
        string SERVICE_USER_CONFERENCE_WITH_NUMBER = "";  // TO get this from the DB

        static readonly string WEBROOT_PATH = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        static readonly string WEB_BIN_ROOT = System.IO.Path.GetDirectoryName(WEBROOT_PATH);
        static readonly string WEB_JOBS_DIRECTORY = System.IO.Path.GetFullPath("D:\\home\\site\\wwwroot\\app_data\\jobs\\triggered\\TwilioConferenceTimer\\Webjob");
        static readonly string PHONE_PREFIX = "1"; // Keep this at 1

        //To get the location the assembly normally resides on disk or the install directory
        static readonly string TIMER_EXE = Path.Combine(WEB_JOBS_DIRECTORY, "TwilioConference.Timer.exe");
        const string strUTCTimeZoneID = "Etc/UTC";

        private CallResource _crCurrentCall;        
        private string _strCallSID;        
        static string strTargetTimeZoneID = "";        
        private Boolean AVAILABILITY_CHECK_DONE = false;
        private string strCallServiceUserName = "";

        TwilioConferenceServices conferenceServices;

        public TwilioConferenceController()
        {
            TwilioClient.Init(TWILIO_ACCOUNT_SID, TWILIO_ACCOUNT_TOKEN);
            conferenceServices = new TwilioConferenceServices();
            conferenceServices.LogMessage("In contructor");
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
            Boolean IsUserAvailableToTakeCalls = true;
             
            if (!AVAILABILITY_CHECK_DONE)
            {
                try
                {
                    conferenceServices.LogMessage("In availability check");
                    IsUserAvailableToTakeCalls = conferenceServices.
                            CheckAvailabilityAndFetchDetails(
                                ref strCallServiceUserName,
                                        ref SERVICE_USER_CONFERENCE_WITH_NUMBER,
                                            ref strTargetTimeZoneID,
                                                    SERVICE_USER_TWILIO_PHONE_NUMBER);  
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
            
            if (from.Contains(TWILIO_BOT_NUMBER))
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

                conferenceServices.LogMessage("All checks done");
                LocalDateTime callStartTime = new
                            LocalDateTime(crCurrentCall.StartTime.Value.Year,
                                                crCurrentCall.StartTime.Value.Month,
                                                    crCurrentCall.StartTime.Value.Day,
                                                            crCurrentCall.StartTime.Value.Hour,
                                                                crCurrentCall.StartTime.Value.Minute,
                                                                        crCurrentCall.StartTime.Value.Second);

                ZonedDateTime utcCallStartTime = callStartTime.InZoneLeniently(dtzsourceDateTime);
                DateTimeZone tzTargetDateTime = DateTimeZoneProviders.Tzdb[strTargetTimeZoneID];
                ZonedDateTime targetCallStartTime = utcCallStartTime.WithZone(tzTargetDateTime);

                int intMinutesToPause = 0;
                int intSecondsToPause = 0;
                var strHourMessage = string.Empty;

                if (!CallStartTimeMeetsRequirements(
                    targetCallStartTime.LocalDateTime.Minute,
                        targetCallStartTime.LocalDateTime.Second,
                            ref intMinutesToPause,
                                ref intSecondsToPause,
                                    ref strHourMessage))
                {
                    response.Say("You've reached the Tackle Time line of ");
                    response.Say(strCallServiceUserName);
                    response.Say("You will be connected in");
                    response.Say(intMinutesToPause.ToString());
                    response.Say("minutes and");
                    response.Say(intSecondsToPause.ToString());
                    response.Say(" seconds");
                    response.Say(strHourMessage);
                    response.Pause(1);
                    response.Say("Please hold ");
                    //voiceResponse.Pause((intMinutesToPause * 60) + intSecondsToPause);
                }

                // On first call the control flow should be here
                conferenceServices.LogMessage("Connected from " + from);
                string phone1 = from; // This is phone1 the person that calls the twilo number
                string phone2 = string.Format("+{0}",SERVICE_USER_CONFERENCE_WITH_NUMBER); // Number on which  Service User prefers to take calls - Get from DB
                
                string conferenceName = GetRandomConferenceName(); // Step 2.
                
                TwilioConferenceCall conferenceRecord =
                    conferenceServices
                    .CreateTwilioConferenceRecord(phone1, phone2, conferenceName, request.CallSid);

                response.Pause(1);
                response.Say("Connecting Now");

                
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

        public DateTimeZone dtzsourceDateTime
        {
            get { return DateTimeZoneProviders.Tzdb[strUTCTimeZoneID]; }
        }


        //[System.Web.Mvc.HttpPost]
        public ActionResult ConnectTwilioBot(VoiceRequest request)
        {
            var response = new VoiceResponse();
            conferenceServices.LogMessage("ConnectFromTwilio Bot Called "+ DateTime.Now.ToShortTimeString());
            Response.ContentType = "text/xml";
            //response.Pause(3);
            response.Say("This conference call will be endng in 1 minute");
            response.Hangup();            
            return new TwiMLResult(response);
        }

        string Connect(string phoneNumber, string conferenceName, int conferenceRecordId)
        {
            var call = CallResource.Create(
                to: new PhoneNumber(phoneNumber),
                from: new PhoneNumber(
                    string.Format(PHONE_PREFIX+"{0}", SERVICE_USER_TWILIO_PHONE_NUMBER)),
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

        private bool CallStartTimeMeetsRequirements
                              (int zdtCallStartMinute,
                                   int zdtCallStartSecond,
                                        ref int intMinutesToPause,
                                            ref int intSecondsToPause,
                                              ref string strHourMessage)
        {
            var retVal = false;

            var blnCallStartTimeAtTimeSlot =
                ((zdtCallStartMinute % 10) == 0)          // The start minute is either exactly at  00 / 10 / 20 / 30 / 40 / 50 past the hour
                ||                                        // OR
                ((zdtCallStartMinute - 1) % 10) == 0;     // The start minute is within a minute of 00 / 10 / 20 / 30 / 40 / 50 past the hour

            switch (blnCallStartTimeAtTimeSlot)
            {
                case true:
                    retVal = true;
                    break;

                case false:
                    // Find total absolute seconds of call start time     
                    var intCallStartTotalSeconds =
                        zdtCallStartSecond             //Second the call started
                          + (zdtCallStartMinute * 60); //Minute the call started

                    // Fid time slot end
                    var div = zdtCallStartMinute / 10;
                    var timeslotend = (10 * div) + 10;

                    switch (timeslotend)
                    {
                        case 60:
                            strHourMessage = "at the Top of the hour";
                            break;
                        case 10:
                            strHourMessage = "at ten minutes past the hour";
                            break;
                        case 20:
                            strHourMessage = "at twenty minutes past the hour";
                            break;
                        case 30:
                            strHourMessage = "at thirty minutes past the hour";
                            break;
                        case 40:
                            strHourMessage = "at fourty minutes past the hour";
                            break;
                        case 50:
                            strHourMessage = "at fifty minutes past the hour";
                            break;
                    }


                    // Find total absolute seconds of call slot end time
                    var intCallSlotEndTotalSeconds = timeslotend * 60;

                    // Cover to minutes & seconds
                    intMinutesToPause = (intCallSlotEndTotalSeconds - intCallStartTotalSeconds) / 60;
                    intSecondsToPause = (intCallSlotEndTotalSeconds - intCallStartTotalSeconds) % 60;
                    break;
            }
            return retVal;
        }

        //[HttpPost]
        //public ActionResult Check()
        //{
        //    var response = new MessagingResponse();
        //    var message = new Message();
        //    var strResponse = string.Empty;
        //    conferenceServices.LogMessage(string.Format("In check {0}",twilioPhoneNumber));

        //    strResponse = EvaluateMessage(smsMessageContents);
        //    response.Message(strResponse, smsFromPhonenumber, twilioPhoneNumber);
        //    conferenceServices.LogMessage(string.Format("In Check {0},{1},{2}",strResponse,smsFromPhonenumber,twilioPhoneNumber));
        //    return TwiML(response);
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


        //private string EvaluateMessage(string smsMessageContents)
        //{
        //    string strResponse;
        //    conferenceServices.LogMessage(string.Format("Evaluate message {0} {1}", twilioPhoneNumber, smsMessageContents));
        //    switch (smsMessageContents.ToUpper().Trim())
        //    {
        //        case "STATUS":   // Return current status
        //            strResponse = conferenceServices.returnStatus(twilioPhoneNumber);
        //            break;

        //        case "STATUS 1": // Update Status to Available
        //            strResponse = conferenceServices.updateStatus(1, twilioPhoneNumber);
        //            break;

        //        case "STATUS 0": // Update Status to not available
        //            strResponse = conferenceServices.updateStatus(0, twilioPhoneNumber);
        //            break;

        //        default:
        //            strResponse = "Invalid Command. Keywords are Status = retrieving status, Status 0 =  updating status to not available, status 1 =  updating status to available";
        //            break;
        //    }
        //    return strResponse;
        //}


        internal CallResource crCurrentCall
        {
            get
            {
                _crCurrentCall = _crCurrentCall ??
                     CallResource.Fetch(strCallSID);
                return _crCurrentCall;
            }
        }

        internal string strCallSID
        {
            get
            {
                _strCallSID = _strCallSID ??
                        (!string.IsNullOrEmpty(Request.Params["CallSid"])
                        ? Request.Params["CallSid"].ToString()
                        : string.Empty);

                return _strCallSID;
            }
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

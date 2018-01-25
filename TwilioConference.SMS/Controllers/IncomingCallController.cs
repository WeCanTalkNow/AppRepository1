//using System;
//using System.Configuration;
//using System.Web.Mvc;
//using System.IO;
//using System.Diagnostics;
//using NodaTime;
//using Twilio.TwiML;
//using Twilio.AspNet.Mvc;
//using Twilio.AspNet.Common;
//using Twilio.Rest.Api.V2010.Account;
//using Twilio.Types;
//using Twilio;
//using CallingService.DataModel.Queries;
//using CallingService.DataModel.Services;
//using CallingService.DataModel.Classes;
//using CallingService.Voice.Models;


//namespace CallingService.Voice.Controllers
//{
//    public class IncomingCallController : TwilioController
//    {

//        #region Variables
//        private CallResource _crCurrentCall;
//        const string strUTCTimeZoneID = "Etc/UTC";
//        private string _strCallSID;
//        private string _strTwilioPhoneNumber;
//        private ConferenceService conferenceServices;
        

//        private static readonly string TWILIO_ACCOUNT_SID = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_SID"];
//        private static readonly string TWILIO_ACCOUNT_TOKEN = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_TOKEN"];
//        private static Boolean AVAILABILITY_CHECK_DONE = false;

//        //To get the location the assembly normally resides on disk or the install directory
//        static string WEBROOT_PATH = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
//        static string WEB_BIN_ROOT = System.IO.Path.GetDirectoryName(WEBROOT_PATH);
//        static string WEB_JOBS_DIRECTORY = System.IO.Path.GetFullPath("D:\\home\\site\\wwwroot\\app_data\\jobs\\triggered\\TwilioConferenceTimer\\Webjob");

//        static string TIMER_EXE = Path.Combine(WEB_JOBS_DIRECTORY, "TwilioConference.Timer.exe");

//        // Twilio Number used to conference in with message
//        // Note: To keep this in configuration file
//        private static readonly string TWILIO_MESSAGE_NUMBER = ConfigurationManager.AppSettings["TWILIO_MESSAGE_NUMBER"];
//        #endregion

//        public IncomingCallController()
//        {
//            TwilioClient.Init(TWILIO_ACCOUNT_SID, TWILIO_ACCOUNT_TOKEN);
//            conferenceServices = new ConferenceService();
//            conferenceServices.LogMessage("In class constructor");
//        }

//        #region Functions and Methods

//        /// <summary>
//        /// This is the default functionality 
//        /// Acheived before the conference feature was built
//        /// The pause part of the Twiml (Line 143 or so) is commented out
//        /// Please uncomment for production and comment for testing
//        /// </summary>
//        /// <param name="request"></param>
//        /// <returns></returns>
//        [HttpGet]
//        //public ActionResult TransferCallBak(VoiceRequest request)
//        public ActionResult TransferCallBak()
//        {

//            //    // Step 1  Determine if User is available
//            //    //         Update User Availability Check flag - to prevent checking again when 
//            //    //         this routine is called by Twilio_message_number
//            //    // Step 2  If user is not available, then play error message
//            //    //         control flow should go out of this method i.e. return
//            //    // Step 3  Assuming that user is available, then 
//            //    //         check call start time
//            //    //         If Call start time is within defined parameters
//            //    //         then put connect the conference
//            //    //         Else Pause for calculated period of time
//            //    // Step 4  Check if incoming call is from TWILIO_MESSAGE_PHONE
//            //    //         if Yes, Dial into conferencename
//            //    //         if No, then get a new conference name and dial into conference
//            //    // 

//            conferenceServices.LogMessage("started");
//            var voiceResponse = new VoiceResponse();
//            var strCallServiceUserName = string.Empty;
//            var strUserDialToPhoneNumber = string.Empty;
//            var strTargetTimeZoneID = string.Empty;
//            var IsUserAvailableToTakeCalls = true;
////            string strCallerID = request.From;
//            string strCallerID = "9818500936";
//            conferenceServices.LogMessage("upto here");
//            Response.ContentType = contentType;

//            try
//            {
//                IsUserAvailableToTakeCalls = VoiceQueries.
//                                               CheckAvailabilityAndFetchDetails(ref strCallServiceUserName,
//                                                                                    ref strUserDialToPhoneNumber,
//                                                                                            ref strTargetTimeZoneID,
//                                                                                                strTwilioPhoneNumber);

//                conferenceServices.LogMessage(string.Format("strTwilioPhoneNumber {0} strCallerId, {1} strCallServiceUserName {2} IsUserAvailableToTakeCalls {3} strUserDialToPhoneNumber {4}",
//                                                             strTwilioPhoneNumber, strCallerID, strCallServiceUserName, IsUserAvailableToTakeCalls, strUserDialToPhoneNumber));

//                switch (IsUserAvailableToTakeCalls)
//                {
//                    case false:
//                        voiceResponse.Say(string.Concat(strCallServiceUserName,
//                            "is not available to take calls at this time. ",
//                            "Please check back with this user about their available call times"));
//                        voiceResponse.Hangup();
//                        break;

//                    case true:
//                        LocalDateTime callStartTime = new
//                            LocalDateTime(crCurrentCall.StartTime.Value.Year,
//                                             crCurrentCall.StartTime.Value.Month,
//                                                    crCurrentCall.StartTime.Value.Day,
//                                                          crCurrentCall.StartTime.Value.Hour,
//                                                                crCurrentCall.StartTime.Value.Minute,
//                                                                      crCurrentCall.StartTime.Value.Second);


//                        ZonedDateTime utcCallStartTime = callStartTime.InZoneLeniently(dtzsourceDateTime);
//                        DateTimeZone tzTargetDateTime = DateTimeZoneProviders.Tzdb[strTargetTimeZoneID];
//                        ZonedDateTime targetCallStartTime = utcCallStartTime.WithZone(tzTargetDateTime);

//                        int intMinutesToPause = 0;
//                        int intSecondsToPause = 0;
//                        var strHourMessage = string.Empty;

//                        if (!CallStartTimeMeetsRequirements(
//                            targetCallStartTime.LocalDateTime.Minute,
//                                targetCallStartTime.LocalDateTime.Second,
//                                    ref intMinutesToPause,
//                                        ref intSecondsToPause,
//                                            ref strHourMessage))
//                        {
//                            voiceResponse.Say("You've reached the Tackle Time line of ");
//                            voiceResponse.Say(strCallServiceUserName);
//                            voiceResponse.Say("You will be connected in");
//                            voiceResponse.Say(intMinutesToPause.ToString());
//                            voiceResponse.Say("minutes and");
//                            voiceResponse.Say(intSecondsToPause.ToString());
//                            voiceResponse.Say(" seconds");
//                            voiceResponse.Say(strHourMessage);
//                            voiceResponse.Say("Please hold ");
//                            // voiceResponse.Pause((intMinutesToPause * 60) + intSecondsToPause);
//                        }

//                        voiceResponse.Say(string.Concat("Please wait, transferring your call to  ", strCallServiceUserName));
//                        voiceResponse.Dial(strUserDialToPhoneNumber, null, null, null, null, null, strCallerID);
//                        break;

//                    default:
//                        break;
//                }

//            }
//            catch (Exception ex)
//            {
//                conferenceServices.ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
//    ex.Message, ex.Source, ex.StackTrace, ex.InnerException));
//            }


//            return TwiML(voiceResponse);
//        }
        

//        [System.Web.Mvc.HttpPost]
//        public ActionResult ConferenceHandler(VoiceRequest request)
//        {

//            //    // Step 1  Determine if User is available
//            //    //         Update User Availability Check flag - to prevent checking again when 
//            //    //         this routine is called by Twilio_message_number
//            //    // Step 2  If user is not available, then play error message
//            //    //         control flow should go out of this method i.e. return
//            //    // Step 3  Assuming that user is available, then 
//            //    //         check call start time
//            //    //         If Call start time is within defined parameters
//            //    //         then put connect the conference
//            //    //         Else Pause for calculated period of time
//            //    // Step 4  Check if incoming call is from TWILIO_MESSAGE_PHONE
//            //    //         if Yes, Dial into conferencename
//            //    //         if No, then get a new conference name and dial into conference
//            //    // 

//            var voiceResponse = new VoiceResponse();
//            var dial = new Dial();

//            var strCallServiceUserName = string.Empty;
//            var strUserDialToPhoneNumber = string.Empty;
//            string strCallFromPhoneNumber = request.From;

//            var strTargetTimeZoneID = string.Empty;
//            var IsUserAvailableToTakeCalls = true;
//            var conferenceName = string.Empty;

//            conferenceServices.LogMessage("Upto here");
//            Response.ContentType = contentType;

//            if (!AVAILABILITY_CHECK_DONE)
//            {
//                try
//                {
//                    conferenceServices.LogMessage("In availability check");
//                    IsUserAvailableToTakeCalls = VoiceQueries.
//                            CheckAvailabilityAndFetchDetails(
//                                ref strCallServiceUserName,
//                                        ref strUserDialToPhoneNumber,
//                                            ref strTargetTimeZoneID,
//                                                    strTwilioPhoneNumber);

//                    conferenceServices.LogMessage(string.Format("strTwilioPhoneNumber {0} strCallFromPhoneNumber, {1} strCallServiceUserName {2} IsUserAvailableToTakeCalls {3} strUserDialToPhoneNumber {4}",
//                                                                 strTwilioPhoneNumber, strCallFromPhoneNumber, strCallServiceUserName, IsUserAvailableToTakeCalls, strUserDialToPhoneNumber));

//                    AVAILABILITY_CHECK_DONE = true;
//                    if (!IsUserAvailableToTakeCalls)
//                    {
//                        voiceResponse.Say(string.Concat(strCallServiceUserName,
//                                      "is not available to take calls at this time. ",
//                                       "Please check back with this user about their available call times"));
//                        voiceResponse.Hangup();
//                        return TwiML(voiceResponse);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    conferenceServices.ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
//                        ex.Message, ex.Source, ex.StackTrace, ex.InnerException));
//                }
//            }

//            if (strCallFromPhoneNumber.Contains(TWILIO_MESSAGE_NUMBER))
//                {                    
//                    try
//                    {
//                        conferenceServices.LogMessage("Connected from TWILIO MESSAGE PHONE");
//                        conferenceName = conferenceServices.GetMostRecentConferenceNameFromNumber(strTwilioPhoneNumber);
//                    }
//                    catch (Exception ex)
//                    {
//                        conferenceServices.LogMessage(ex.Message);
//                    }

//                    dial.Conference(conferenceName);
//                    voiceResponse.Dial(dial);

//                    return new TwiMLResult(voiceResponse);                
//               }
//                else
//                {
//                    conferenceServices.LogMessage("All checks done");
//                    LocalDateTime callStartTime = new
//                                LocalDateTime(crCurrentCall.StartTime.Value.Year,
//                                                    crCurrentCall.StartTime.Value.Month,
//                                                        crCurrentCall.StartTime.Value.Day,
//                                                                crCurrentCall.StartTime.Value.Hour,
//                                                                    crCurrentCall.StartTime.Value.Minute,
//                                                                            crCurrentCall.StartTime.Value.Second);

//                    ZonedDateTime utcCallStartTime = callStartTime.InZoneLeniently(dtzsourceDateTime);
//                    DateTimeZone tzTargetDateTime = DateTimeZoneProviders.Tzdb[strTargetTimeZoneID];
//                    ZonedDateTime targetCallStartTime = utcCallStartTime.WithZone(tzTargetDateTime);

//                    int intMinutesToPause = 0;
//                    int intSecondsToPause = 0;
//                    var strHourMessage = string.Empty;

//                    if (!CallStartTimeMeetsRequirements(
//                        targetCallStartTime.LocalDateTime.Minute,
//                            targetCallStartTime.LocalDateTime.Second,
//                                ref intMinutesToPause,
//                                    ref intSecondsToPause,
//                                        ref strHourMessage))
//                    {
//                        voiceResponse.Say("You've reached the Tackle Time line of ");
//                        voiceResponse.Say(strCallServiceUserName);
//                        voiceResponse.Say("You will be connected in");
//                        voiceResponse.Say(intMinutesToPause.ToString());
//                        voiceResponse.Say("minutes and");
//                        voiceResponse.Say(intSecondsToPause.ToString());
//                        voiceResponse.Say(" seconds");
//                        voiceResponse.Say(strHourMessage);
//                        voiceResponse.Say("Please hold ");
//                        //voiceResponse.Pause((intMinutesToPause * 60) + intSecondsToPause);
//                    }

//                    conferenceName = conferenceServices.GetRandomConferenceName(); // Step 2.

//                    ConferenceCall conferenceRecord =
//                    conferenceServices
//                   .CreateTwilioConferenceRecord(strCallFromPhoneNumber, strUserDialToPhoneNumber, conferenceName, request.CallSid);


//                    voiceResponse.Say("You are about to join a conference call");
//                    voiceResponse.Say("We are going to conference you in with :");

//                    foreach (var digit in conferenceRecord.ConferenceWithPhoneNumber)
//                    {
//                        voiceResponse.Say(digit.ToString());
//                    }

//                    // Connect phone1 to conference // Step 3.
//                    dial = new Dial();
                
//                    dial.Conference(
//                        name: conferenceName
//                        , waitUrl: "http://twimlets.com/holdmusic?Bucket=com.twilio.music.ambient"
//                        , statusCallbackEvent: "start end join"
//                        , statusCallback: string.Format("http://callingserviceconferenceapp.azurewebsites.net/IncomingCall/HandleConferenceStatusCallback?id={0}", conferenceRecord.Id)
//                        , statusCallbackMethod: "POST"
//                        , startConferenceOnEnter: true
//                        , endConferenceOnExit: true
//                        , trim: "trim-silence"
//                        );
//                    voiceResponse.Dial(dial);
//                    conferenceServices.LogMessage("Step 3 Dialed Conference Name " + conferenceName);
//                }

//            return TwiML(voiceResponse);
//        }

//        [System.Web.Mvc.HttpPost]
//        public TwiMLResult HandleConferenceStatusCallback(TConferenceRequest request)
//        {
//            //Uncomment below to print full
//            //request body
//            //var req = Request.InputStream;
//            //var json = new StreamReader(req).ReadToEnd();
//            //conferenceServices.LogMessage(json);

//            var response = new VoiceResponse();

//            string conferenceRecordId = Request.QueryString["id"];
//            int id = Int32.Parse(conferenceRecordId);

//            ConferenceCall conferenceRecord = conferenceServices.GetConferenceRecord(id);

//            var conferenceStatus = request.StatusCallbackEvent;

//            switch (conferenceStatus)
//            {
//                case "participant-join":
//                    if (request.CallSid == conferenceRecord.PhoneCall1SID)
//                    {
//                        conferenceServices.LogMessage("Step 4 " + "Dialing Person 2 " + conferenceRecord.ConferenceWithPhoneNumber);
//                        Connect(conferenceRecord.ConferenceWithPhoneNumber, conferenceRecord.ConferenceName, id);
//                    }
//                    break;
//                case "conference-start":
//                    {
//                        conferenceRecord.ConferenceSID = request.ConferenceSid;
//                        conferenceServices.UpdateConferenceSid(conferenceRecord);
//                        try
//                        {
//                            conferenceServices.UpdateCallStartTime(id);
//                            try
//                            {
//                                ProcessStartInfo processStartInfo = new ProcessStartInfo(TIMER_EXE, conferenceRecordId.ToString());
//                                Process.Start(processStartInfo);
//                                conferenceServices.LogMessage("Step 6 " + "Scheduled Timer started" + conferenceRecordId, id);
//                            }
//                            catch (Exception ex)
//                            {
//                                conferenceServices.LogMessage(ex.Message, id);
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            conferenceServices.LogMessage(ex.Message, id);
//                        }
//                    }
//                    break;
//                case "conference-end":
//                    // To write routine to make call inactive
//                    // This might be required to delete all calls that are inactive
//                    conferenceServices.UpdateActiveStatus(id, false);
//                    break;
//            }
//            conferenceServices.LogMessage(string.Format("{0} - {1} - {2}", conferenceStatus, request.ConferenceSid, request.CallSid), id);
//            return new TwiMLResult();
//        }


//        public ActionResult ConnectTwilioBot(VoiceRequest request)
//        {
//            var response = new VoiceResponse();
//            conferenceServices.LogMessage("Step 8 ConnectFromTwilio Bot Called " + DateTime.Now.ToShortTimeString());
//            Response.ContentType = "text/xml";
//            //response.Pause(3);            
//            response.Say("Sorry to interrupt guys but just wanted to let you know that your conference would be ending in 1 minute");
//            response.Hangup();
//            return new TwiMLResult(response);
//        }

//        void Connect(string phoneConferenceWith, string conferenceName, int conferenceRecordId)
//        {
//            //Step 5
//            var call = CallResource.Create(
//                        to: new PhoneNumber(phoneConferenceWith),
//                        from: new PhoneNumber(string.Format("+1{0}", TWILIO_MESSAGE_NUMBER)),
//                        url: new
//                             Uri(string.Format("http://callingserviceconferenceapp.azurewebsites.net/IncomingCall/ConferenceInPerson2?conferenceName={0}&id={1}"
//                             , conferenceName,
//                conferenceRecordId)));
//            conferenceServices.LogMessage("Step 5 " + "Dialed " + phoneConferenceWith);
//            return;
//        }

//        public TwiMLResult ConferenceInPerson2(VoiceRequest request)
//        {
//            string conferenceName = Request.QueryString["conferenceName"];
//            int id = Int32.Parse(Request.QueryString["id"]);

//            ConferenceCall conferenceRecord = conferenceServices.GetConferenceRecord(id);

//            var response = new VoiceResponse();
//            //response.Pause(5);
//            response.Say("You are about to join a conference call");
//            response.Say("We are going to conference you in with :");

//            foreach (var digit in conferenceRecord.ConferenceWithPhoneNumber)
//            {
//                response.Say(digit.ToString());
//            }

//            var dial = new Dial();
//            dial.Conference(conferenceName);
//            response.Dial(dial);

//            return new TwiMLResult(response);
//        }




//        [HttpPost]
//        public ActionResult GetCallStartTimeusingIana()
//        {
//            const string destTimeZone1 = "Asia/Calcutta";
//            const string destTimeZone2 = "America/Los_Angeles";
//            var response = new VoiceResponse();
//            Response.ContentType = contentType;
//            var accountId = ConfigurationManager.AppSettings["TwilioAccountSid"];
//            var accountToken = ConfigurationManager.AppSettings["TwilioAccountToken"];

//            TwilioClient.Init(accountId, accountToken);

//            LocalDateTime callStartTime = new
//                LocalDateTime(crCurrentCall.StartTime.Value.Year,
//                                 crCurrentCall.StartTime.Value.Month,
//                                        crCurrentCall.StartTime.Value.Day,
//                                              crCurrentCall.StartTime.Value.Hour,
//                                                    crCurrentCall.StartTime.Value.Minute,
//                                                          crCurrentCall.StartTime.Value.Second);

//            DateTimeZone tzsourceDateTime = DateTimeZoneProviders.Tzdb["Etc/UTC"];
//            ZonedDateTime utcCallStartTime = callStartTime.InZoneLeniently(tzsourceDateTime);

//            DateTimeZone tzTargetDateTime = DateTimeZoneProviders.Tzdb[destTimeZone1];
//            ZonedDateTime targetCallStartTime = utcCallStartTime.WithZone(tzTargetDateTime);

//            DateTimeZone tzTargetDateTime1 = DateTimeZoneProviders.Tzdb[destTimeZone2];
//            ZonedDateTime targetCallStartTime1 = utcCallStartTime.WithZone(tzTargetDateTime1);

//            response.Say(string.Concat("UTC Time is", utcCallStartTime.LocalDateTime.ToString()));
//            response.Say(string.Concat("India Time is ", targetCallStartTime.LocalDateTime.ToString()));
//            response.Say(string.Concat("San Francisco Time is", targetCallStartTime1.LocalDateTime.ToString()));

//            return TwiML(response);
//        }

//        [HttpPost]
//        public ActionResult GetCallStartTimeUsingBcl()
//        {
//            var response = new VoiceResponse();
//            Response.ContentType = contentType;
//            var accountId = ConfigurationManager.AppSettings["TwilioAccountSid"];
//            var accountToken = ConfigurationManager.AppSettings["TwilioAccountToken"];

//            TwilioClient.Init(accountId, accountToken);

//            var currentCall = CallResource.Fetch(strCallSID);

//            LocalDateTime callStartTime = new LocalDateTime(currentCall.StartTime.Value.Year,
//                                                     currentCall.StartTime.Value.Month,
//                                                        currentCall.StartTime.Value.Day,
//                                                          currentCall.StartTime.Value.Hour,
//                                                             currentCall.StartTime.Value.Minute,
//                                                                currentCall.StartTime.Value.Second);


//            DateTimeZone tzsourceDateTime = DateTimeZoneProviders.Bcl["UTC"];
//            ZonedDateTime utcCallStartTime = callStartTime.InZoneLeniently(tzsourceDateTime);

//            DateTimeZone tzTargetDateTime = DateTimeZoneProviders.Bcl["India Standard Time"];
//            ZonedDateTime targetCallStartTime = utcCallStartTime.WithZone(tzTargetDateTime);

//            DateTimeZone tzTargetDateTime1 = DateTimeZoneProviders.Bcl["Pacific Standard Time"];
//            ZonedDateTime targetCallStartTime1 = utcCallStartTime.WithZone(tzTargetDateTime1);

            
//            response.Say(string.Concat("UTC Time is", utcCallStartTime.LocalDateTime.ToString()));
//            response.Say(string.Concat("India Time is ", targetCallStartTime.LocalDateTime.ToString()));
//            response.Say(string.Concat("San Francisco Time is", targetCallStartTime1.LocalDateTime.ToString()));

//            //response.Say(callstartime.ToLocalTime().ToLongTimeString());

//            ////var currentCall = CallResource.Fetch(CallSID);

//            //////if (!string.IsNullOrEmpty(CallSID))
//            //////  response.Dial("+919818500936", null, null, null, null, null, CallerId);

//            ////if (!string.IsNullOrEmpty(CallSID))
//            ////    if (currentCall.StartTime.HasValue)
//            ////    {
//            ////        response.Sms(string.Concat(CallSID.ToString(),
//            ////                                currentCall.StartTime.Value.ToLongTimeString()),
//            ////                                   "+919818500936",
//            ////                                      "+14159157316");
//            ////    }


//            //if (!string.IsNullOrEmpty(CallSID))
//            //  response.Dial("+919818500936", null, null, null, null, null, CallerId);

//            return TwiML(response);
//        }

//        private void GenerateResponse(VoiceResponse response, string takeTimeUserName, bool availableToTakeCalls)
//        {
//            if (!availableToTakeCalls)
//                response.Say(string.Concat(takeTimeUserName, "is not available to take calls at this time. Please check back with this user about their available call times"));
//            else
//                response.Say(string.Concat("Please wait, transferring your call to  ", takeTimeUserName));
//        }


//        private bool CallStartTimeMeetsRequirementsBak(int zdtTargetCallStartTimeMinute, ref short intMinutesToPause)
//        {
//            var retVal = false;

//            if (
//                ((zdtTargetCallStartTimeMinute) % 10) == 0
//                || ((zdtTargetCallStartTimeMinute - 1) % 10) == 0
//                )
//            {
//                retVal = true;
//            }
//            else
//            {
//                // Find number of minutes to pause
//                var mod = zdtTargetCallStartTimeMinute % 10;
//                var div = zdtTargetCallStartTimeMinute / 10;

//                var timeSlotend = (10 * div) + 10;
//                intMinutesToPause = (short)(timeSlotend - zdtTargetCallStartTimeMinute);
//            }

//            return retVal;
//        }

//        private bool CallStartTimeMeetsRequirements
//                          (int zdtCallStartMinute,
//                               int zdtCallStartSecond,
//                                    ref int intMinutesToPause,
//                                        ref int intSecondsToPause,
//                                          ref string strHourMessage)
//        {
//            var retVal = false;

//            var blnCallStartTimeAtTimeSlot =
//                ((zdtCallStartMinute % 10) == 0)          // The start minute is either exactly at  00 / 10 / 20 / 30 / 40 / 50 past the hour
//                ||                                        // OR
//                ((zdtCallStartMinute - 1) % 10) == 0;     // The start minute is within a minute of 00 / 10 / 20 / 30 / 40 / 50 past the hour

//            switch (blnCallStartTimeAtTimeSlot)
//            {
//                case true:
//                    retVal = true;
//                    break;

//                case false:
//                    // Find total absolute seconds of call start time     
//                    var intCallStartTotalSeconds =
//                        zdtCallStartSecond             //Second the call started
//                          + (zdtCallStartMinute * 60); //Minute the call started

//                    // Fid time slot end
//                    var div = zdtCallStartMinute / 10;
//                    var timeslotend = (10 * div) + 10;

//                    switch (timeslotend)
//                    {
//                        case 60:
//                            strHourMessage = "at the Top of the hour";
//                            break;
//                        case 10:
//                            strHourMessage = "at ten minutes past the hour";
//                            break;
//                        case 20:
//                            strHourMessage = "at twenty minutes past the hour";
//                            break;
//                        case 30:
//                            strHourMessage = "at thirty minutes past the hour";
//                            break;
//                        case 40:
//                            strHourMessage = "at fourty minutes past the hour";
//                            break;
//                        case 50:
//                            strHourMessage = "at fifty minutes past the hour";
//                            break;
//                    }


//                    // Find total absolute seconds of call slot end time
//                    var intCallSlotEndTotalSeconds = timeslotend * 60;

//                    // Cover to minutes & seconds
//                    intMinutesToPause = (intCallSlotEndTotalSeconds - intCallStartTotalSeconds) / 60;
//                    intSecondsToPause = (intCallSlotEndTotalSeconds - intCallStartTotalSeconds) % 60;
//                    break;
//            }
//            return retVal;
//        }

//        #endregion

//        #region Properties
//        internal string contentType
//        {
//            get { return "text/xml"; }
//        }

//        internal string strTwilioPhoneNumber
//        {
//            get
//            {
//                _strTwilioPhoneNumber = _strTwilioPhoneNumber ??
//                        (!string.IsNullOrEmpty(Request.Params["Called"])
//                        ? Request.Params["Called"].ToString().Substring(1)
//                        : string.Empty);
//                return _strTwilioPhoneNumber;
//            }

//        }

//        internal string strCallSID
//        {
//            get
//            {
//                _strCallSID = _strCallSID ??
//                        (!string.IsNullOrEmpty(Request.Params["CallSid"])
//                        ? Request.Params["CallSid"].ToString()
//                        : string.Empty);

//                return _strCallSID;
//            }
//        }

//        public DateTimeZone dtzsourceDateTime
//        {
//            get { return DateTimeZoneProviders.Tzdb[strUTCTimeZoneID]; }
//        }

//        internal CallResource crCurrentCall
//        {
//            get
//            {
//                _crCurrentCall = _crCurrentCall ??
//                     CallResource.Fetch(strCallSID);
//                return _crCurrentCall;
//            }
//        }

//        #endregion
//    }
//    }
﻿using System;
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

        // 1. This is the TWILIO phone number used as a BOT to play the message
        // Obtained from web config
        static readonly string TWILIO_BOT_NUMBER = ConfigurationManager.AppSettings["TWILIO_BOT_NUMBER"];

        // 2. This is the Twiio number that has been used to initiate the conference
        // Captured on call pickup and then stored to conference table
        string SERVICE_USER_TWILIO_PHONE_NUMBER = "";

        // 3. This is the Number of the registered user on which the conference call will be received
        // Obtained from user table based on 2 above
        string SERVICE_USER_CONFERENCE_WITH_NUMBER = "";  

        static readonly string WEBROOT_PATH = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
        static readonly string WEB_BIN_ROOT = System.IO.Path.GetDirectoryName(WEBROOT_PATH);
        // Do not change path
        static readonly string WEB_JOBS_DIRECTORY = System.IO.Path.GetFullPath("D:\\home\\site\\wwwroot\\app_data\\jobs\\triggered\\TwilioConferenceTimer\\Webjob");        
        //To get the location the assembly normally resides on disk or the install directory
        static readonly string TIMER_EXE = Path.Combine(WEB_JOBS_DIRECTORY, "TwilioConference.Timer.exe");

        const string strUTCTimeZoneID = "Etc/UTC";
        string strTargetTimeZoneID = "";

        Boolean AVAILABILITY_CHECK_DONE = false;
        private string strCallServiceUserName = "";
        private CallResource _crCurrentCall;        
        private string _strCallSID;
        double messageIntervalinSeconds;
        double hangupIntervalinSeconds;
        double warningIntervalinSeconds;

        private TwilioConferenceServices conferenceServices = new TwilioConferenceServices();

        public TwilioConferenceController()
        {
            //TwilioClient.Init(TWILIO_ACCOUNT_SID, TWILIO_ACCOUNT_TOKEN);
            //conferenceServices = new TwilioConferenceServices();
            //conferenceServices.LogMessage("In contructor");
        }

        //    // Step 1  Determine if User is available
        //    //         Update User Availability Check flag - to prevent checking again when 
        //    //         this routine is called by Twilio_message_number
        //    // Step 2  If user is not available, then play error message
        //    //         control flow should go out of this method i.e. return
        //    // Step 3  Assuming that user is available, then 
        //    //         check call start time
        //    //         If Call start time is within defined parameters
        //    //         then put connect the conference
        //    //         Else Pause for calculated period of time
        //    // Step 4  Check if incoming call is from TWILIO_MESSAGE_PHONE
        //    //         if Yes, Dial into conferencename
        //    //         if No, then get a new conference name and dial into conference


        // POST: Conference/Connect
        [System.Web.Mvc.HttpPost]
        public ActionResult Connect(VoiceRequest request)  // Step 1.
        {
            var response = new VoiceResponse();
            string from = request.From;
            SERVICE_USER_TWILIO_PHONE_NUMBER = request.To;

            Boolean IsUserAvailableToTakeCalls = true;
            if (!AVAILABILITY_CHECK_DONE)
            {
                try
                {
                    TwilioClient.Init(TWILIO_ACCOUNT_SID, TWILIO_ACCOUNT_TOKEN);
                    IsUserAvailableToTakeCalls = conferenceServices.
                    CheckAvailabilityAndFetchDetails(
                        SERVICE_USER_TWILIO_PHONE_NUMBER.Substring(2),
                            ref strCallServiceUserName,
                                ref SERVICE_USER_CONFERENCE_WITH_NUMBER,
                                    ref strTargetTimeZoneID);

                    AVAILABILITY_CHECK_DONE = true;
                    if (!IsUserAvailableToTakeCalls)
                    {
                        response.Say(string.Concat(strCallServiceUserName,
                                      "is not available to take calls at this time. ",
                                       "Please check back with this user about their available call times"));
                        response.Hangup();
                        return TwiML(response);
                    }

                    conferenceServices.LogMessage(string.Concat("Step 1 Availability Check ",
                        string.Format("|Twilio Phone Number {0}| User Name {1} |Conference Number {2}| Time Zone {3}| Is Available? {4}|"
                          , SERVICE_USER_TWILIO_PHONE_NUMBER
                            , strCallServiceUserName
                               , SERVICE_USER_CONFERENCE_WITH_NUMBER
                                  , strTargetTimeZoneID
                                    , IsUserAvailableToTakeCalls)));
                }
                catch (Exception ex)
                {
                    conferenceServices.ErrorMessage(string.Format("|Error Message - {0}| 1.Source {1} | 2.Trace {2} |3.Inner Exception {3} |",
                       ex.Message, 
                         ex.Source, 
                           ex.StackTrace, 
                             ex.InnerException));
                }
            }
            
            if (from.Contains(TWILIO_BOT_NUMBER))
            {
                string conferenceName = "mango";
                var conferenceid = 0;
                try
                {                   
                   conferenceName = conferenceServices.GetMostRecentConferenceNameFromNumber(ref conferenceid);
                    conferenceServices.LogMessage(string.Concat("Step 5 Connecting Bot to conference ",
                        string.Format("|Twilio Bot Number {0}| Conference Name {1}|"
                          , TWILIO_BOT_NUMBER
                            , conferenceName)), conferenceid);

                }
                catch (Exception ex)
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
                //var req = Request.InputStream;
                //var json = new StreamReader(req).ReadToEnd();
                //conferenceServices.LogMessage(json);

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

                Boolean callStartAtTimeSlot = 
                    CallStartAtTimeSlot(
                        targetCallStartTime.LocalDateTime.Minute,
                            targetCallStartTime.LocalDateTime.Second,
                                ref intMinutesToPause,
                                    ref intSecondsToPause,
                                        ref strHourMessage,
                                          ref messageIntervalinSeconds,
                                            ref hangupIntervalinSeconds,
                                            ref warningIntervalinSeconds);


                response.Say("You've reached the Tackle Time line of ");
                response.Say(strCallServiceUserName);

                if (!callStartAtTimeSlot)
                    {
                    response.Say("You will be connected in");
                    response.Say(intMinutesToPause.ToString());
                    response.Say("minutes and");
                    response.Say(intSecondsToPause.ToString());
                    response.Say(" seconds");
                    response.Say(strHourMessage);
                    response.Pause(1);
                    response.Say("Please hold ");
                    //Pause a number of seconds
                    //response.Pause((intMinutesToPause * 60) + intSecondsToPause);
                }


                // This is phone of the person that calls the twilo number
                string phoneFrom = from;
                // Number on which  Service User prefers to take calls - Get from DB     
                string phoneTo = string.Format("+{0}",SERVICE_USER_CONFERENCE_WITH_NUMBER); 
                // Get a random conference name
                string conferenceName = conferenceServices.GetRandomConferenceName(); 

                TwilioConferenceCall conferenceRecord =
                    conferenceServices
                    .CreateTwilioConferenceRecord(phoneFrom, phoneTo, SERVICE_USER_TWILIO_PHONE_NUMBER, conferenceName, request.CallSid,
                          hangupIntervalinSeconds,messageIntervalinSeconds, warningIntervalinSeconds);

                conferenceServices.LogMessage(string.Concat("|Step 2 Connecting Caller |",
                    string.Format("|Connect From {0} |Connect To {1} |Conference Number {2} |Call Start Time Meet Req {3} |Conference Name {4} |Call SID {5}|"
                      , phoneFrom
                        , phoneTo
                           , SERVICE_USER_CONFERENCE_WITH_NUMBER
                              , callStartAtTimeSlot
                                , conferenceName
                                   , request.CallSid)),conferenceRecord.Id);

                conferenceServices.LogMessage(string.Concat("|checking call time values |",
                                    string.Format("|targetCallStartTime {0} |" +
                                      "| targetCallStartTime.LocalDateTime.Minute {1} |" +
                                         " |targetCallStartTime.LocalDateTime.Second {2} |" +
                                            "| utcCallStartTime {3} |" +
                                               "| tzTargetDateTime {4} |" +
                                                   "| Call SID {5}|" +
                                                      "| Minutes to pause {6}|" +
                                                         "|Seconds to pause {7}|" +
                                                           "|messageIntervalinSeconds {8} |" +
                                                              "|hangupIntervalinSeconds {9}| " +
                                                                 "|warningIntervalinSeconds {9}| "
                                      , targetCallStartTime
                                        , targetCallStartTime.LocalDateTime.Minute
                                           , targetCallStartTime.LocalDateTime.Second
                                              , utcCallStartTime
                                                , tzTargetDateTime
                                                   , request.CallSid
                                                      , intMinutesToPause
                                                        , intSecondsToPause
                                                          , messageIntervalinSeconds
                                                            , hangupIntervalinSeconds
                                                               , warningIntervalinSeconds)),conferenceRecord.Id);


                response.Pause(1);
                response.Say("Connecting Now");

                //foreach (var digit in conferenceRecord.PhoneTo)
                //{
                //    response.Say(digit.ToString());
                //}

                // Connect phone1 to conference // 3.
                var dial = new Dial();

                dial.Conference(conferenceName
                    , waitUrl: "http://callingservicetest.azurewebsites.net//twilioconference/HoldMusic"
                    , statusCallbackEvent: "start end join"
                    , statusCallback: string.Format("http://callingservicetest.azurewebsites.net//twilioconference/HandleConferenceStatusCallback?id={0}", conferenceRecord.Id)
                    , statusCallbackMethod: "POST"
                    , startConferenceOnEnter: true
                    , endConferenceOnExit: true);
                response.Dial(dial);
            }

            return TwiML(response);
        }


        //[System.Web.Mvc.HttpPostGet]
        public TwiMLResult HoldMusic()
        {
            var response = new VoiceResponse();

            string holdMusicUri = "http://com.twilio.music.ambient.s3.amazonaws.com/aerosolspray_-_Living_Taciturn.mp3";
            response.Play(holdMusicUri);

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

            switch (conferenceStatus)
            {
                case "participant-join":
                    if (request.CallSid == conferenceRecord.PhoneCall1SID)
                    {
                        conferenceServices.LogMessage("Step 3 " + "Dialing Participant " + conferenceRecord.PhoneTo,id);
                        ConnectParticipant(conferenceRecord.PhoneTo, conferenceRecord.TwilioPhoneNumber, conferenceRecord.ConferenceName, id);
                    }
                    break;
                case "conference-start":
                    {
                        conferenceRecord.ConferenceSID = request.ConferenceSid;
                        conferenceServices.UpdateConferenceSid(conferenceRecord);
                        try
                        {
                            conferenceServices.UpdateCallStartTime(id);
                            try
                            {
                                conferenceServices.LogMessage(string.Concat("|Step 4 Starting Scheduled Timer|",
                                    string.Format("|Exe Name & Path {0} |" +
                                                    "|Conference ID {1} |" +
                                                       "|TwilioPhone  {2} |" +
                                                           "|Call SID  {3} |" +
                                                       "|messageIntervalinSeconds {4}|" +
                                                             "|hangupIntervalinSeconds {5}|" +
                                                                "|warningIntervalinSeconds {6}|" 
                                      , TIMER_EXE
                                        , conferenceRecordId
                                           , conferenceRecord.TwilioPhoneNumber
                                                   , request.CallSid,
                                                       conferenceRecord.messageIntervalInSeconds,
                                                         conferenceRecord.hangupIntervalInSeconds,
                                                           conferenceRecord.warningIntervalInSeconds)),id);

                                string[] args = { conferenceRecordId.ToString(), conferenceRecord.TwilioPhoneNumber,
                                    conferenceRecord.messageIntervalInSeconds.ToString(),
                                      conferenceRecord.hangupIntervalInSeconds.ToString(),
                                        conferenceRecord.warningIntervalInSeconds.ToString()};

                                ProcessStartInfo processStartInfo = new ProcessStartInfo();
                                processStartInfo.FileName = TIMER_EXE;
                                processStartInfo.Arguments = string.Join(" ", args);
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
                    }
                    break;
                case "conference-end":
                    // To write routine to make call inactive
                    // This might be required to delete all calls that are inactive
                    conferenceServices.UpdateActiveStatus(id, false);
                    break;
            }
            conferenceServices.LogMessage(string.Format("{0} - {1} - {2}", conferenceStatus, request.ConferenceSid, request.CallSid), id);
            return new TwiMLResult();
        }


        public DateTimeZone dtzsourceDateTime
        {
            get { return DateTimeZoneProviders.Tzdb[strUTCTimeZoneID]; }
        }

        //[System.Web.Mvc.HttpPost]
        public ActionResult ConnectTwilioBotMessage(VoiceRequest request)
        {
            int conferenceRecordId = int.Parse(Request.QueryString["id"]);
            conferenceServices.LogMessage("Playing Message "+ DateTime.Now.ToShortTimeString(),conferenceRecordId);
            var response = new VoiceResponse();                        
            Response.ContentType = "text/xml";            
            response.Say("This conference call will be ending in 1 minute");            
            response.Hangup();            
            return new TwiMLResult(response);
        }

        public ActionResult ConnectTwilioBotWarning(VoiceRequest request)
        {
            int conferenceRecordId = int.Parse(Request.QueryString["id"]);
            conferenceServices.LogMessage("Playing Warning " + DateTime.Now.ToShortTimeString(), conferenceRecordId);
            var response = new VoiceResponse();
            Response.ContentType = "text/xml";            
            response.Say("hanging up shortly");
            response.Hangup();
            return new TwiMLResult(response);
        }



        void ConnectParticipant(string phoneNumber,string TwilioPhoneNumber, string conferenceName, int conferenceRecordId)
        {

            var call = CallResource.Create(
                to: new PhoneNumber(
                    phoneNumber),
                from: new PhoneNumber(
                    TwilioPhoneNumber),
                url: new Uri(string.Format("http://callingservicetest.azurewebsites.net//twilioconference/ConferenceInPerson2?conferenceName={0}&id={1}" // 5.
                , conferenceName, conferenceRecordId)));

            return;
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

            foreach (var digit in conferenceRecord.PhoneFrom)
            {
                response.Say(digit.ToString());
            }

            var dial = new Dial();
            dial.Conference(conferenceName);
            response.Dial(dial);

            return new TwiMLResult(response);
        }



        private bool CallStartAtTimeSlot
                              (int zdtCallStartMinute,
                                   int zdtCallStartSecond,
                                        ref int intMinutesToPause,
                                            ref int intSecondsToPause,
                                              ref string strHourMessage,
                                                ref double messageIntervalinSeconds,
                                                    ref double hangupIntervalinSeconds,
                                                      ref double warningIntervalinSeconds)
        {

            // Assume Call start time is 19:47:34
            // zdtCall Start start minute is 47
            // zdtCall Start start second is 34
            var retVal = false;
            
            var blnCallStartTimeAtTimeSlot =
                ((zdtCallStartMinute % 10) == 0)          // The start minute is either exactly at  00 / 10 / 20 / 30 / 40 / 50 past the hour
                ||                                        // OR
                ((zdtCallStartMinute - 1) % 10) == 0;     // The start minute is within a minute of 00 / 10 / 20 / 30 / 40 / 50 past the hour

            // Keep both message & hangup interval at default values of 8 & 9 minutes
//            messageIntervalinSeconds = (8 * 60);
//            hangupIntervalinSeconds = (9 * 60);

            messageIntervalinSeconds = 30;
            hangupIntervalinSeconds = 80;
            warningIntervalinSeconds = 70;

            // Find total absolute seconds of call start time     
            // If call start minute is 47 and call start seconds is 34
            // intCallStartTotalSeconds = 2854
            var intCallStartTotalSeconds =
                zdtCallStartSecond             //Second the call started
                  + (zdtCallStartMinute * 60); //Minute the call started

            // Fid time slot end
            // IF call start minute is 47 then div = 4
            var div = zdtCallStartMinute / 10;
            // time slot end is 50 
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
            // In current case this value is 3000 (60 * 50)
            var intCallSlotEndTotalSeconds = timeslotend * 60;

            // Convert to minutes & seconds
            // intMinutesToPause = (3000 - 2854) / 60 = 2 minutes
            intMinutesToPause = (intCallSlotEndTotalSeconds - intCallStartTotalSeconds) / 60;
            // intSecondsToPause = (3000 - 2854) % 60 = 26 seconds
            intSecondsToPause = (intCallSlotEndTotalSeconds - intCallStartTotalSeconds) % 60;

            if ((blnCallStartTimeAtTimeSlot) && (intSecondsToPause > 0))
            {
                //// Total 8 minutes into message, convert to seconds and adjust for 26 seconds into call
                //messageIntervalinSeconds = (8 * 60) - (60 - intSecondsToPause);
                //// Total 9 minutes into message, convert to seconds and adjust for 26 seconds into call
                //hangupIntervalinSeconds = (9 * 60) - (60 - intSecondsToPause);

                // Total 8 minutes into message, convert to seconds and adjust for 26 seconds into call
                messageIntervalinSeconds = (1 * 60) - (60 - intSecondsToPause);
                // Total 9 minutes into message, convert to seconds and adjust for 26 seconds into call
                hangupIntervalinSeconds = (2 * 60) - (60 - intSecondsToPause);
                warningIntervalinSeconds = hangupIntervalinSeconds - 10;

            }

            retVal = blnCallStartTimeAtTimeSlot;
            return retVal;
        }


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



    }
}

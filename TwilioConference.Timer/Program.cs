using Quartz;
using Quartz.Impl;
using System;
using System.Configuration;
using TwilioConference.DataServices;

namespace TwilioConference.Timer
{
    class Program
    {
        static bool secondParticipantConnected;
        static double messageInterval;
        static double warningInterval;
        static double hangupInterval;
        static int id;
        static string SERVICE_USER_TWILIO_PHONE_NUMBER;
        static string conferenceName;
        static string conferenceSid;
        static string TWILIO_ACCOUNT_SID;
        static string TWILIO_ACCOUNT_TOKEN;
        static string TWILIO_BOT_NUMBER;
        static ISchedulerFactory sChedFactory;
        static IScheduler sChedule;
        static void Main(string[] args)
        {
            TwilioConferenceServices conferenceServices = new TwilioConferenceServices();
            try
            {
                secondParticipantConnected = bool.Parse(args[0]);
                id = int.Parse(args[1]);
                SERVICE_USER_TWILIO_PHONE_NUMBER = args[2];
                messageInterval = double.Parse(args[3]);
                warningInterval = double.Parse(args[4]);
                hangupInterval = double.Parse(args[5]);

                conferenceServices.LogMessage("Step 7 Entered Scheduled Timer at " + DateTime.Now.ToString(), 7, id);
                TWILIO_ACCOUNT_SID = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_SID"];
                TWILIO_ACCOUNT_TOKEN = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_TOKEN"];

                TWILIO_BOT_NUMBER = ConfigurationManager.AppSettings["TWILIO_BOT_NUMBER"];

                conferenceSid = conferenceServices.GetConferenceRecord(id).ConferenceSID;

                // construct a scheduler factory
                sChedFactory = new StdSchedulerFactory();

                // get a scheduler
                sChedule = sChedFactory.GetScheduler();
                sChedule.Start();

                conferenceName = new TwilioConferenceServices().GetConferenceRecord(id).ConferenceName;

                switch (secondParticipantConnected)
                {
                    case true:
                        DefineMessageJob(messageInterval, conferenceServices, id);
                        DefineWarningJob(warningInterval, conferenceServices, id);
                        DefineHangupJob(hangupInterval, conferenceServices, id);
                        break;


                    case false:
                        DefineHangupJob(hangupInterval, conferenceServices, id);
                        break;


                    default:
                        break;
                }


                conferenceServices.LogMessage(string.Format("Step 7 Successfuly completed Scheduled Timer - " +
                     "|Twilio Phone Number-{0}| " +
                     "|Bot Number-{1}| " +
                     "|Conference Name-{2}| " +
                     "|Conference SID-{3} |" +
                     "|ID-{4} |" +
                     "|Number of seconds to message-{5}|" +
                     "|Number of seconds to warning-{6}|" +
                     "|Number of seconds to hangup-{7}|",
                      SERVICE_USER_TWILIO_PHONE_NUMBER,
                      TWILIO_BOT_NUMBER,
                      conferenceName,
                      conferenceSid,
                      id,
                      messageInterval,
                      warningInterval,
                      hangupInterval)
                      , 7, id);
            }
            catch (ArgumentException ex)
            {
                conferenceServices.ErrorMessage(string.Format(
                                   "|Error Message - {0}" +
                                   "| 1.Source {1} " +
                                   "| 2.Trace {2} " +
                                   "| 3.Inner Exception {3} |",
                                   ex.Message,
                                   ex.Source,
                                   ex.StackTrace,
                                   ex.InnerException));
                throw;
            }
        }

        private static void DefineMessageJob(double messageInterval, TwilioConferenceServices conferenceServices, int id)
        {
            //Message offset depending on ticks elapsed since call
            DateTimeOffset messageOffset = DateTime.Now.AddSeconds(messageInterval);
            conferenceServices.LogMessage(string.Format("Step 7 Message timer will execute at :{0}", messageOffset), 7, id);

            // Create Message Job
            IJobDetail messageNotificationJobDetail =
                JobBuilder.Create<MessageJob>()
                .WithIdentity("MessageJob", "TwilioGroup")
                .UsingJobData("conferenceSid", conferenceSid)
                .UsingJobData("twilloAccountSid", TWILIO_ACCOUNT_SID)
                .UsingJobData("twilloAccountToken", TWILIO_ACCOUNT_TOKEN)
                .UsingJobData("id", id)
                .UsingJobData("conferenceName", conferenceName)
                .UsingJobData("serviceUserTwilioPhoneNumber", SERVICE_USER_TWILIO_PHONE_NUMBER.Substring(2))
                .UsingJobData("twilioBotNumber", TWILIO_BOT_NUMBER)
                .Build();

            ITrigger messageTrigger = TriggerBuilder.Create()
                .StartAt(messageOffset)
                .Build();

            sChedule.ScheduleJob(messageNotificationJobDetail, messageTrigger);

        }

        private static void DefineWarningJob(double warningInterval, TwilioConferenceServices conferenceServices, int id)
        {
            //Warning offset depending on ticks elapsed since call
            DateTimeOffset warningOffset = DateTime.Now.AddSeconds(warningInterval);
            conferenceServices.LogMessage(string.Format("Step 7 Warning timer will execute at :{0}", warningOffset), 7, id);

            // Create Warning Job
            IJobDetail warningNotificationJobDetail =
                JobBuilder.Create<CallEndWarningJob>()
                .WithIdentity("CallEndWarningJob", "TwilioGroup")
                .UsingJobData("conferenceSid", conferenceSid)
                .UsingJobData("twilloAccountSid", TWILIO_ACCOUNT_SID)
                .UsingJobData("twilloAccountToken", TWILIO_ACCOUNT_TOKEN)
                .UsingJobData("id", id)
                .UsingJobData("conferenceName", conferenceName)
                .UsingJobData("serviceUserTwilioPhoneNumber", SERVICE_USER_TWILIO_PHONE_NUMBER.Substring(2))
                .UsingJobData("twilioBotNumber", TWILIO_BOT_NUMBER)
                .Build();

            ITrigger warningTrigger = TriggerBuilder.Create()
                .StartAt(warningOffset)
                .Build();

            sChedule.ScheduleJob(warningNotificationJobDetail, warningTrigger);

        }

        private static void DefineHangupJob(double hangupInterval, TwilioConferenceServices conferenceServices, int id)
        {
            //Hangup offset depending on ticks elapsed since call
            DateTimeOffset hangUpOffset = DateTime.Now.AddSeconds(secondParticipantConnected ?  hangupInterval :0);


            conferenceServices.LogMessage(string.Format("Step 7 Hangup timer will execute at :{0}", hangUpOffset), 7, id);

            // Create Hangup Job
            IJobDetail hangUpJobDetail = JobBuilder.Create<HangUpJob>()
             .WithIdentity("HangUpJob", "TwilioGroup")
             .UsingJobData("twilloAccountSid", TWILIO_ACCOUNT_SID)
             .UsingJobData("twilloAccountToken", TWILIO_ACCOUNT_TOKEN)
             .UsingJobData("conferenceSid", conferenceSid)
             .UsingJobData("id", id)
             .UsingJobData("conferenceName", conferenceName)
             .Build();

            ITrigger hangUpTrigger = TriggerBuilder.Create()
                .StartAt(hangUpOffset)
                .Build();

            sChedule.ScheduleJob(hangUpJobDetail, hangUpTrigger);


        }



    }


}

using Quartz;
using Quartz.Impl;
using System;
using System.Configuration;
using TwilioConference.DataServices;

namespace TwilioConference.Timer
{
    class Program
    {
        static void Main(string[] args)
        {
            TwilioConferenceServices conferenceServices = new TwilioConferenceServices();
            try
            {
            //Pass in the ConferenceRecordId
            int id = int.Parse(args[0]);
            string SERVICE_USER_TWILIO_PHONE_NUMBER = args[1];
            var messageInterval = double.Parse(args[2]);
            var hangupInterval = double.Parse(args[3]);

            conferenceServices.LogMessage("Entered Scheduled Timer at " + DateTime.Now.ToString(), id);
            string TWILIO_ACCOUNT_SID = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_SID"];
            string TWILIO_ACCOUNT_TOKEN = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_TOKEN"];

            //string SERVICE_USER_TWILIO_PHONE_NUMBER = ConfigurationManager.AppSettings["SERVICE_USER_TWILIO_PHONE_NUMBER"];
            string TWILIO_BOT_NUMBER = ConfigurationManager.AppSettings["TWILIO_BOT_NUMBER"];

            //string callSid = "CAbe07f2cc9faea5a9a7e832db7e4fa239";
            string conferenceSid = conferenceServices.GetConferenceRecord(id).ConferenceSID;
            conferenceServices.LogMessage("Conference id is: "+conferenceSid + " TwilioConference.Timer", id);

            //Message offset depending on ticks elapsed since call
            //DateTimeOffset messageOffset = DateTime.Now.AddMinutes(.5);
            DateTimeOffset messageOffset = DateTime.Now.AddSeconds(messageInterval);
            conferenceServices.LogMessage(string.Format("Message timer will execute at :{0}",messageOffset), id);

            //Hangup offset depending on ticks elapsed since call
            //DateTimeOffset hangUpOffset = DateTime.Now.AddMinutes(1);
            DateTimeOffset hangUpOffset = DateTime.Now.AddSeconds(hangupInterval);
            conferenceServices.LogMessage(string.Format("Hangup timer will execute at :{0}", hangUpOffset), id);

            // construct a scheduler factory
            ISchedulerFactory schedFact = new StdSchedulerFactory();

                // get a scheduler
                IScheduler sched = schedFact.GetScheduler();
                sched.Start();

                var conferenceName = new TwilioConferenceServices().GetConferenceRecord(id).ConferenceName;

                IJobDetail messageNotificationJobDetail =
                    JobBuilder.Create<MessageJob>()
                    .WithIdentity("MessageJob", "TwilioGroup")
                    .UsingJobData("callSid", conferenceSid)
                    .UsingJobData("twilloAccountSid", TWILIO_ACCOUNT_SID)
                    .UsingJobData("twilloAccountToken", TWILIO_ACCOUNT_TOKEN)
                    .UsingJobData("id", id)
                    .UsingJobData("conferenceName", conferenceName)
                    .UsingJobData("serviceUserTwilioPhoneNumber",  SERVICE_USER_TWILIO_PHONE_NUMBER.Substring(2))
                    .UsingJobData("twilioBotNumber", TWILIO_BOT_NUMBER)
                    .Build();

                ITrigger messageTrigger = TriggerBuilder.Create()
                    .StartAt(messageOffset)
                    .Build();

                sched.ScheduleJob(messageNotificationJobDetail, messageTrigger);

                IJobDetail hangUpJobDetail = JobBuilder.Create<HangUpJob>()
                 .WithIdentity("HangUpJob", "TwilioGroup")
                 .UsingJobData("twilloAccountSid", TWILIO_ACCOUNT_SID)
                 .UsingJobData("twilloAccountToken", TWILIO_ACCOUNT_TOKEN)
                 .UsingJobData("callSid", conferenceSid)
                 .UsingJobData("id", id)
                 .UsingJobData("conferenceName", conferenceName)
                 .Build();

                ITrigger hangUpTrigger = TriggerBuilder.Create()
                    .StartAt(hangUpOffset)
                    .Build();

                sched.ScheduleJob(hangUpJobDetail, hangUpTrigger);

                conferenceServices.LogMessage(string.Format("Successfuly completed Scheduled Timer - "
                    + " Twilio Phone Number-{0} " 
                      +  "Bot Number-{1} " 
                        +  "Conference Name-{2} "
                          + "Conference SID-{3} "
                              + "ID-{4} "
                                + "Number of seconds to message-{5}"
                                    + "Number of seconds to hangup-{6}",
                    SERVICE_USER_TWILIO_PHONE_NUMBER, 
                      TWILIO_BOT_NUMBER, 
                        conferenceName, 
                          conferenceSid, 
                            id,
                             messageInterval,
                              hangupInterval
                                ),id);
            }
            catch (ArgumentException ex)
            {

                conferenceServices.ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
 ex.Message, ex.Source, ex.StackTrace, ex.InnerException));

            }
        }
    }
}

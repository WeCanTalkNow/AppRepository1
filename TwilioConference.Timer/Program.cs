using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            conferenceServices.LogMessage("Entered Scheduled Timer at " + DateTime.Now.ToString(), id);
            string TWILIO_ACCOUNT_SID = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_SID"];
            string TWILIO_ACCOUNT_TOKEN = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_TOKEN"];

            //string SERVICE_USER_TWILIO_PHONE_NUMBER = ConfigurationManager.AppSettings["SERVICE_USER_TWILIO_PHONE_NUMBER"];
            string TWILIO_BOT_NUMBER = ConfigurationManager.AppSettings["TWILIO_BOT_NUMBER"];

            //string callSid = "CAbe07f2cc9faea5a9a7e832db7e4fa239";
            string conferenceSid = conferenceServices.GetConferenceRecord(id).ConferenceSID;
            conferenceServices.LogMessage("Conference id is: "+conferenceSid + " TwilioConference.Timer", id);

            //Get 9 minutes from when conference id was passed in
            DateTimeOffset messageOffset = DateTime.Now.AddMinutes(.5);
            conferenceServices.LogMessage(string.Format("9 minute timer will execute at :{0}",messageOffset), id);
            //Get 10 minutes from when conference id was passed in
            DateTimeOffset hangUpOffset = DateTime.Now.AddMinutes(1);
            conferenceServices.LogMessage(string.Format("10 minute timer will execute at :{0}", hangUpOffset), id);

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
                    + " Twilio Phone Number-{0} Bot Number-{1} Conference Name-{2} Conference SID-{3} ID-{4} ", 
                    SERVICE_USER_TWILIO_PHONE_NUMBER, 
                      TWILIO_BOT_NUMBER, 
                        conferenceName, 
                          conferenceSid, 
                            id),id);
            }
            catch (ArgumentException ex)
            {

                conferenceServices.ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
 ex.Message, ex.Source, ex.StackTrace, ex.InnerException));

            }
        }
    }
}

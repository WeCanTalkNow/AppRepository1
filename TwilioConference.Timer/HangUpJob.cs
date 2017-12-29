using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using TwilioConference.DataServices;

namespace TwilioConference.Timer
{
    public class HangUpJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            TwilioConferenceServices conferenceServices = new TwilioConferenceServices();

            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string twilloAccountSid = dataMap.GetString("twilloAccountSid");
            string twilloAccountToken = dataMap.GetString("twilloAccountToken");
            string conferenceSid = dataMap.GetString("conferenceSid");
            int id = dataMap.GetInt("id");
            string conferenceName = dataMap.GetString("conferenceName");

            //Make rest request to /HangUpAt10Minutes endpoint
            TwilioClient.Init(twilloAccountSid, twilloAccountToken);

            conferenceServices.LogMessage(string.Format("Step 10 Hangup timer begin: {0}", conferenceSid),10, id);

            ConferenceResource.Update(conferenceSid,
                                status: ConferenceResource.UpdateStatusEnum.Completed);
            conferenceServices.LogMessage(string.Format("Step 10 Hangup timer end: {0}", conferenceSid),10, id);

            Thread.Sleep(2000);

            Environment.Exit(0);
        }

    }
}

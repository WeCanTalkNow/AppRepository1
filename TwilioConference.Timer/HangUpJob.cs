﻿using Quartz;
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
            string callSid = dataMap.GetString("callSid");
            int id = dataMap.GetInt("id");
            string conferenceName = dataMap.GetString("conferenceName");

            //Make rest request to /HangUpAt10Minutes endpoint
            TwilioClient.Init(twilloAccountSid, twilloAccountToken);

            conferenceServices.LogMessage(string.Format("10 minute timer Begin: {0}", callSid), id);

            ConferenceResource.Update(callSid,
                                status: ConferenceResource.UpdateStatusEnum.Completed);

            conferenceServices.LogMessage(string.Format("10 minute timer End: {0}", callSid), id);

            Thread.Sleep(2000);

            Environment.Exit(0);
        }

    }
}

using Quartz;
using System;
using System.IO;
using System.Net;
using System.Text;
using TwilioConference.DataServices;

namespace TwilioConference.Timer
{
    public class CallEndWarningJob : IJob
    {
      public void Execute(IJobExecutionContext context)
        {
            TwilioConferenceServices conferenceServices = new TwilioConferenceServices();
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            string twilloAccountSid = dataMap.GetString("twilloAccountSid");
            string twilloAccountToken = dataMap.GetString("twilloAccountToken");
            string conferenceSid = dataMap.GetString("conferenceSid");

            string SERVICE_USER_TWILIO_PHONE_NUMBER = dataMap.GetString("serviceUserTwilioPhoneNumber");
            string TWILIO_BOT_NUMBER = dataMap.GetString("twilioBotNumber");

            int id = dataMap.GetInt("id");
            string conferenceName = dataMap.GetString("conferenceName");
            //string dataMapValues =
            //    string.Format("1. {0}|2. {1}|3. {2}|4. {3}|5. {4}|6. {5}||7. {6}|"
            //   , twilloAccountSid
            //   , twilloAccountToken
            //   , callSid
            //   , id
            //   , conferenceName
            //   , SERVICE_USER_TWILIO_PHONE_NUMBER
            //   , TWILIO_BOT_NUMBER);
            //conferenceServices.LogMessage(dataMapValues, id);
            conferenceServices.LogMessage(string.Format("Step 9 Warning Job  begin: {0}", conferenceSid),9, id);
            string connectUrl = string.Format("http://callingservicetest.azurewebsites.net//twilioconference/ConnectTwilioBotWarning?id={0}",id);
            try
            {

                string postUrl =
                    string.Format("https://api.twilio.com/2010-04-01/Accounts/{0}/Calls.json", twilloAccountSid);
                WebRequest myReq = WebRequest.Create(postUrl);
                string credentials = string.Format("{0}:{1}", twilloAccountSid, twilloAccountToken);
                myReq.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
                string formencodeddata = string.Format("To=+1{0}&From=+1{1}&Url={2}"
                    , SERVICE_USER_TWILIO_PHONE_NUMBER
                    , TWILIO_BOT_NUMBER
                    , connectUrl);
                byte[] formbytes = System.Text.ASCIIEncoding.Default.GetBytes(formencodeddata);
                myReq.Method = "POST";
                myReq.ContentType = "application/x-www-form-urlencoded";
                //conferenceServices.LogMessage("credentials " + credentials.ToString(), 9, id);
                //conferenceServices.LogMessage("get request stream " + myReq.GetRequestStream(), 9, id);
                //conferenceServices.LogMessage("formencodeddata " + formencodeddata.ToString(), 9, id);

                using (Stream postStream = myReq.GetRequestStream())
                {
                    postStream.Write(formbytes, 0, formbytes.Length);
                }
                conferenceServices.LogMessage(string.Format("Step 9 Warning Job  End: {0}", conferenceSid),9, id);
                WebResponse wr = myReq.GetResponse();
                Stream receiveStream = wr.GetResponseStream();
                StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
                //string content = reader.ReadToEnd();
                //conferenceServices.LogMessage(content, id);
                reader.Close();
                reader.Dispose();
                receiveStream.Close();
                receiveStream.Dispose();                
                wr.Close();
                wr.Dispose();

                //CallResource.CreateAsync(
                //    to: new PhoneNumber(Constants.TWILIO_CONFERENCE_NUMBER)
                //    , from: new PhoneNumber("4159656328")
                //    , url: new Uri(connectUrl)
                //    , method: HttpMethod.Post).Wait();
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
    }
}

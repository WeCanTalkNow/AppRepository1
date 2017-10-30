using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using TwilioConference.Controllers;

namespace TwilioConference.IntegrationTest
{
    /// <summary>
    /// Summary description for TwilioConference_Test
    /// </summary>
    [TestClass]
    public class TwilioConference_Test
    {
        static readonly string TWILIO_ACCOUNT_SID = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_SID"];
        static readonly string TWILIO_ACCOUNT_TOKEN = ConfigurationManager.AppSettings["TWILIO_ACCOUNT_TOKEN"];
        static readonly string TWILIO_NUMBER = ConfigurationManager.AppSettings["TWILIO_NUMBER"];

        public TwilioConference_Test()
        {
            //Init Twilio
            TwilioClient.Init(TWILIO_ACCOUNT_SID, TWILIO_ACCOUNT_TOKEN);
        }

        [TestMethod]
        public void Can_Send_Regular_Call()
        {
            TwilioConferenceController controller = new TwilioConferenceController();
            PhoneNumber to = new PhoneNumber("3014373223");
            PhoneNumber from = new PhoneNumber(TWILIO_NUMBER);

            try
            {
                var call = CallResource.Create(
                    to
                    , from
                    //url: new Uri("http://demo.twilio.com/welcome/voice/"));
                    , url: new Uri("http://192.168.1.16:45455/api/twilio"));
                //,url: new Uri("http://localhost:63529/api/twilio"));
            }
            catch (Exception ex)
            {

            }
        }

        [TestMethod]
        public void Can_Connect_To_Conference_Call()
        {

        }

        [TestMethod]
        public void Can_Conference_Another_User_In()
        {

        }

        [TestMethod]
        public void Can_Set_HangUpMessage_Timer()
        {

        }

        [TestMethod]
        public void Can_Trigger_HangUp_From_Timer()
        {

        }
    }
}

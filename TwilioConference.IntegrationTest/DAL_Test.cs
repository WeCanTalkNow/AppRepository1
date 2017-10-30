using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TwilioConference.DataServices;

namespace TwilioConference.IntegrationTest
{
    [TestClass]
    public class DAL_Test
    {
        [TestMethod]
        public void Can_Create_Twilio_DB()
        {
            using (var context = new TwilloDbContext())
            {
                try
                {
                    context.Database.Initialize(force: true);
                }
                catch(Exception ex)
                {
                    Assert.Fail(ex.Message);
                }
            }
        }
    }
}

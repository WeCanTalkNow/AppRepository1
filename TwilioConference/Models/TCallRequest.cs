using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwilioConference.Models
{
    public class TCallRequest
    {
        public string CallSid { get; set; }
        public string From { get; set; }
        public string CallStatus { get; set; }
        public DateTime  Timestamp { get; set; }
    }
}
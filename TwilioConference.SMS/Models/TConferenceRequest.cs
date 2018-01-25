using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallingService.Voice.Models
{
    public class TConferenceRequest
    {
        //public int Id { get; set; }
        public string ConferenceSid { get; set; }
        public string CallSid { get; set; }
        public string StatusCallbackEvent { get; set; }

    }
}
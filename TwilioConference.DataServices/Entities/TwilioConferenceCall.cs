using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioConference.DataServices.Entities
{
    public class TwilioConferenceCall
    {
        public int Id { get; set; }
        public string ConferenceSID { get; set; }
        public string PhoneCall1SID { get; set; }
        public DateTime? ConferenceStartTime { get; set; }
        public DateTime? CallStartTime { get; set; }
        public bool CallIsActive { get; set; }
        public string PhoneFrom { get; set; }
        public string PhoneTo { get; set; }
        public string TwilioPhoneNumber { get; set; }
        public SystemStatus SystemStatus {get;set;}
        public string ConferenceName { get; set; }
        public double messageIntervalInSeconds { get; set; }
        public double hangupIntervalInSeconds { get; set; }
        public double warningIntervalInSeconds { get; set; }
    }
}

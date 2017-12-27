using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioConference.DataServices.Entities
{
    public class LogMessage
    {

        // Adding a comment line
        public int Id { get; set; }
        public int Seq { get; set; }
        public int? ConferenceRecordId { get; set; }
        public string Message { get; set; }
        public DateTime? LogTime { get; set; }
    }
}

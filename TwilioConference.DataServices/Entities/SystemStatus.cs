using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioConference.DataServices.Entities
{
    public enum SystemStatus
    {
        RECORD_CREATED,          // 0
        CONFERENCE_START,        // 1
        CONFERENCE_COMPLETED,    // 2   TO BE USED ONLY WHEN A CONFERENCE RUNS ITS FULL LENGTH
        CONFERENC_END_PREMATURE, // 3   TO BE USED ONLY WHEN A PARTICIPANT LEAVES BEFORE THE CONFERENCE ENDS
        DELETE_PENDING,          // 4    
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioConference.DataServices.Entities
{
    public enum SystemStatus : int
    {
        RECORD_CREATED,           // 0
        CONFERENCE_START,         // 1
        ALL_PARTICIPANTS_JOINED,  // 2   TO BE UPDATED WHEN SECOND PARTICIPANT JOINS THE CALL
        CONFERENCE_COMPLETED,     // 3   TO BE USED ONLY WHEN A CONFERENCE RUNS ITS FULL LENGTH
        CONFERENCE_END_PREMATURE,  // 4   TO BE USED ONLY WHEN A PARTICIPANT LEAVES BEFORE THE CONFERENCE ENDS
        DELETE_PENDING,           // 5    
    }
}

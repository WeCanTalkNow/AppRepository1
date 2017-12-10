using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioConference.DataServices.Entities
{
    public enum SystemStatus : int
    {
        RECORD_CREATED,             // 0
        DALING_PARTICIPANT,         // 1   TO BE UPDATED WHEN CALL IS PICKED UP AND DIALING BEGINS
        CONFERENCE_IN_PROGRESS,     // 2   TO BE UPDATED WHEN SECOND PARTICIPANT JOINS THE CALL
        CONFERENCE_COMPLETED,       // 3   TO BE USED ONLY WHEN A CONFERENCE RUNS ITS FULL LENGTH OR IS TERMINATED BEFORE THE FULL DURATION 
        ARCHIVE,                    // 4   TO BE USED ONLY WHEN A PARTICIPANT LEAVES BEFORE THE CONFERENCE ENDS
        DELETE_PENDING,             // 5    
    }
}

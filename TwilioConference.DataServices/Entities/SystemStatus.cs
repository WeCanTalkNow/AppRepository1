using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioConference.DataServices.Entities
{
    // Due to certain reasons beyond the scope of discussion
    // The status for CONFERENCE_ABORTED or CONFERENCE_END_PREMATURE (or similar terminolgy)
    // Could not be implemented in terms of status.
    // The only way to determine if a conference was ended prematurely (AS OF 17-12-2017) is to check the log files for that conference
    // if the log contains "This call has been ended prematurely" that means the conference ended by the user terminating the conference
    // If needed we can ceate a flag in TwilioConference Table just for premature end of conference call
    public enum SystemStatus : int
    {
        RECORD_CREATED,             // 0
        DALING_PARTICIPANT,         // 1   TO BE UPDATED WHEN CALL IS PICKED UP AND DIALING BEGINS
        CONFERENCE_IN_PROGRESS,     // 2   TO BE UPDATED WHEN SECOND PARTICIPANT JOINS THE CALL
        CONFERENCE_COMPLETED,       // 3   TO BE USED ONLY WHEN A CONFERENCE RUNS ITS FULL LENGTH OR IS TERMINATED BEFORE THE FULL DURATION 
        ARCHIVE,                    // 4   TO BE USED TO DENOTE RECORDS THAT NEED TO BE ARCHIVED. TO CREATE A JOB TO FLAG SUCH CALLS.
        DELETE_PENDING,             // 5    
    }
}

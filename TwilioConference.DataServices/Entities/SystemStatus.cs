using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwilioConference.DataServices.Entities
{
    public enum SystemStatus
    {
        ACTIVE,
        CANCELED,
        SEND_CALL_INITIATED,
        CONNECT_PERSON_2_INITIATED,
        PERSON_2_PICKED_UP,
        SENT_9MINUTE_MESSAGE,
        COMPLETED,
        DELETE_PENDING
    }
}

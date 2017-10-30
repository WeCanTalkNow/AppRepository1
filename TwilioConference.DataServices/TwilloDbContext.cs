using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwilioConference.DataServices.Entities;

namespace TwilioConference.DataServices
{
    public class TwilloDbContext:DbContext
    {
        public TwilloDbContext():base("name=TwilioDbContext")
        {

        }

        public DbSet<TwilioConferenceCall> TwilioConferenceCalls { get; set; }
        public DbSet<LogMessage> LogMessages { get; set; }
    }
}

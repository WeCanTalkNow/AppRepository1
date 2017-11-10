using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwilioConference.DataServices.Entities;

namespace TwilioConference.DataServices
{
    public class TwilioConferenceServices
    {
        public TwilioConferenceServices(bool log = false)
        {
            if (log == true)
            {
                try
                {
                    LogMessage("TwilioServices entered at "+DateTime.Now.ToString());
                }
                catch (Exception ex)
                {
                    LogMessage(ex.Message);
                }
            }
        }

        public TwilioConferenceCall CreateTwilioConferenceRecord(string phone1, string phone2)
        {
            using (var _dbContext = new TwilloDbContext())
            {
                TwilioConferenceCall callRecord = new TwilioConferenceCall();
                callRecord.Phone1 = phone1;
                callRecord.Phone2 = phone2;
                callRecord.SystemStatus = SystemStatus.ACTIVE;
                callRecord.CallIsActive = true;


                _dbContext.TwilioConferenceCalls.Add(callRecord);
                _dbContext.SaveChanges();

                return callRecord;
            }
        }

        public void UpdateConferenceSid(TwilioConferenceCall conference)
        {
            using (var _dbContext = new TwilloDbContext())
            {
                var found = _dbContext.TwilioConferenceCalls.Find(conference.Id);
                found.ConferenceSID = conference.ConferenceSID;
                found.SystemStatus = SystemStatus.SEND_CALL_INITIATED;
                _dbContext.SaveChanges();
            }
        }

        public void UpdateCall1Sid(int id, string phone1CallSid)
        {
            using (var _dbContext = new TwilloDbContext())
            {
                var found = _dbContext.TwilioConferenceCalls.Find(id);
                found.PhoneCall1SID = phone1CallSid;
                found.SystemStatus = SystemStatus.CONNECT_PERSON_2_INITIATED;
                _dbContext.SaveChanges();
            }
        }

        public TwilioConferenceCall CreateTwilioConferenceRecord(string phone1, string phone2, string conferenceName, string phoneCall1Sid)
        {
            using (var _dbContext = new TwilloDbContext())
            {
                TwilioConferenceCall callRecord = new TwilioConferenceCall();
                callRecord.Phone1 = phone1;
                callRecord.Phone2 = phone2;
                callRecord.ConferenceName = conferenceName;
                callRecord.PhoneCall1SID = phoneCall1Sid;
                callRecord.SystemStatus = SystemStatus.ACTIVE;
                callRecord.CallIsActive = true;


                _dbContext.TwilioConferenceCalls.Add(callRecord);
                _dbContext.SaveChanges();

                return callRecord;
            }
        }

        public string GetMostRecentConferenceNameFromNumber(string from = null)
        {
            //Note an intelligent method is 
            //needed to connect the conferencename to number 2 and 1
            //from is a stand in

            string conferenceName = "mango";

            using (var _dbContext = new TwilloDbContext())
            {
                TwilioConferenceCall found = _dbContext.TwilioConferenceCalls.ToList().LastOrDefault();

                if(found != null)
                {
                    conferenceName = found.ConferenceName;
                }
            }

            return conferenceName;
        }

        public TwilioConferenceCall GetConferenceBySid(string callSid)
        {
            TwilioConferenceCall found = null;

            using (var _dbContext = new TwilloDbContext())
            {
                found = _dbContext.TwilioConferenceCalls.FirstOrDefault(x => x.ConferenceSID == callSid);
            }

            return found;
        }

        public TwilioConferenceCall GetConferenceRecord(int id)
        {
            TwilioConferenceCall found = null;

            using (var _dbContext = new TwilloDbContext())
            {
                found = _dbContext.TwilioConferenceCalls.Find(id);
            }

            return found;
        }

        public void UpdateSystemStatus(int id, SystemStatus status)
        {
            using (var _dbContext = new TwilloDbContext())
            {
                var found = _dbContext.TwilioConferenceCalls.Find(id);
                found.SystemStatus = status;
                _dbContext.SaveChanges();
            }
        }

        public void UpdateActiveStatus(int id, bool v)
        {
            using (var _dbContext = new TwilloDbContext())
            {
                var found = _dbContext.TwilioConferenceCalls.Find(id);
                found.CallIsActive = v;
                _dbContext.SaveChanges();
            }
        }

        public void LogMessage(string message, int id = 0)
        {
            using (var _dbContext = new TwilloDbContext())
            {
                _dbContext.LogMessages.Add(new LogMessage()
                {
                    LogTime = DateTime.Now,
                    ConferenceRecordId = id,
                    Message = message
                });
                _dbContext.SaveChanges();
            }
        }


        public bool CheckAvailabilityAndFetchDetails(ref string struserName,
            ref string strConferenceWithPhoneNumber, ref string strTimeZoneID, 
                 string strTwilioPhoneNumber)
        {
            var retVal = true;

            try
            {
                using (var _dbContext = new TwilloDbContext())
                {
                    var user =
                               _dbContext
                                .User
                                 .Where(u => u.TwilioPhoneNumber == strTwilioPhoneNumber)
                                  .FirstOrDefault();

                    struserName = user.UserFullName;
                    strConferenceWithPhoneNumber = string.Format("+{0}", user.DialToPhoneNumber);
                    strTimeZoneID = user.IANATimeZone;
                    retVal = Convert.ToBoolean(user.AvailableStatus);
                }

            }
            catch (Exception ex)
            {

            }
            return retVal;
        }


        public void UpdateCallStartTime(int id)
        {
            using (var _dbContext = new TwilloDbContext())
            {
                var found = _dbContext.TwilioConferenceCalls.Find(id);
                found.CallStartTime = DateTime.Now;
                _dbContext.SaveChanges();
            }
        }
    }
}

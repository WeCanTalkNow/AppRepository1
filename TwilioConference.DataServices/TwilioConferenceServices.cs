using System;
using System.Linq;
using TwilioConference.DataServices.Entities;
using System.Configuration;

namespace TwilioConference.DataServices
{
    public class TwilioConferenceServices
    {
        //Boolean Logging_enabled = bool.Parse(ConfigurationManager.AppSettings["LOGGING_ENABLED"]);
        
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
                    ErrorMessage(string.Format("|Error Message - {0}| 1.Source {1} | 2.Trace {2} |3.Inner Exception {3} |",
                       ex.Message,
                         ex.Source,
                           ex.StackTrace,
                             ex.InnerException));
                    throw;

                }
            }
        }

        public void ErrorMessage(string message, int id = 0)
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

        //public TwilioConferenceCall CreateTwilioConferenceRecord(string phone1, string phone2)
        //{
        //    using (var _dbContext = new TwilloDbContext())
        //    {
        //        TwilioConferenceCall callRecord = new TwilioConferenceCall();
        //        callRecord.Phone1 = phone1;
        //        callRecord.Phone2 = phone2;
        //        callRecord.SystemStatus = SystemStatus.ACTIVE;
        //        callRecord.CallIsActive = true;


        //        _dbContext.TwilioConferenceCalls.Add(callRecord);
        //        _dbContext.SaveChanges();

        //        return callRecord;
        //    }
        //}

        public void UpdateConference(TwilioConferenceCall conference)
        {
            try
            {
                using (var _dbContext = new TwilloDbContext())
                {
                    var found = _dbContext.TwilioConferenceCalls.Find(conference.Id);
                    found.ConferenceSID = conference.ConferenceSID;
                    found.SystemStatus = SystemStatus.CONFERENCE_START;
                    _dbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format("|Error Message - {0}| 1.Source {1} | 2.Trace {2} |3.Inner Exception {3} |",
                   ex.Message,
                     ex.Source,
                       ex.StackTrace,
                         ex.InnerException));
                throw;
            }
        }

        public void UpdateConference(int id, string Call2Sid, SystemStatus systemStatus)
        {
            try
            {
                //SystemStatus sysStat = (SystemStatus)systemStatus;
                using (var _dbContext = new TwilloDbContext())
                {
                    var found = _dbContext.TwilioConferenceCalls.Find(id);
                    if (found != null)
                    {
                        found.PhoneCall2SID = Call2Sid;
                        found.SystemStatus = systemStatus;
                    }

                    _dbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format("|Error Message - {0}| 1.Source {1} | 2.Trace {2} |3.Inner Exception {3} |",
                   ex.Message,
                     ex.Source,
                       ex.StackTrace,
                         ex.InnerException));
                throw;
            }
        }

        //public void UpdateCall1Sid(int id, string phone1CallSid)
        //{
        //    try
        //    {
        //        using (var _dbContext = new TwilloDbContext())
        //        {
        //            var found = _dbContext.TwilioConferenceCalls.Find(id);
        //            found.PhoneCall1SID = phone1CallSid;
        //            found.SystemStatus = SystemStatus.CONNECT_PERSON_2_INITIATED;
        //            _dbContext.SaveChanges();
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMessage(string.Format("|Error Message - {0}| 1.Source {1} | 2.Trace {2} |3.Inner Exception {3} |",
        //           ex.Message,
        //             ex.Source,
        //               ex.StackTrace,
        //                 ex.InnerException));
        //        throw;
        //    }
        //}

        public TwilioConferenceCall CreateTwilioConferenceRecord(string phoneFrom, 
            string phoneTo, string twilioPhoneNumber, string conferenceName, 
               string phoneCall1Sid, double hangupIntervalinSeconds, 
                   double messageIntervalinSeconds, double warningIntervalInSeconds,DateTime callStartTime)
        {
            using (var _dbContext = new TwilloDbContext())
            {
                TwilioConferenceCall callRecord = new TwilioConferenceCall();
                try
                {
                    callRecord.PhoneFrom = phoneFrom;
                    callRecord.CallStartTime = callStartTime;
                    callRecord.PhoneTo = phoneTo;
                    callRecord.TwilioPhoneNumber = twilioPhoneNumber;
                    callRecord.ConferenceName = conferenceName;
                    callRecord.PhoneCall1SID = phoneCall1Sid;
                    callRecord.CallIsActive = true;
                    callRecord.hangupIntervalInSeconds = hangupIntervalinSeconds;
                    callRecord.messageIntervalInSeconds = messageIntervalinSeconds;
                    callRecord.warningIntervalInSeconds = warningIntervalInSeconds;
                    callRecord.SystemStatus = SystemStatus.RECORD_CREATED;
                    _dbContext.TwilioConferenceCalls.Add(callRecord);
                    _dbContext.SaveChanges();

                }
                catch (Exception ex)
                {
                    ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
                        ex.Message, ex.Source, ex.StackTrace, ex.InnerException));
                    throw;
                }
                return callRecord;
            }
        }

        public string GetRandomConferenceName()
        {
            Random rnd = new Random();

            string[] greekAlpha = new string[] { "apple"
                , "alpha"
                , "beta"
                , "gamma"
                , "delta"
                , "omega"
                , "sigma" };
            return greekAlpha[rnd.Next(0, greekAlpha.Length)] + DateTime.Now.Ticks;
        }

        //public string GetMostRecentConferenceNameFromNumber(string twilioPhonenumber)
        //{

        //    string conferenceName = "mango";

        //    try
        //    {
        //        using (var _dbContext = new TwilloDbContext())
        //        {
        //            // Note: This has to return a value
        //            // This is the most recent conference started via a 
        //            // specific Twilio number and which is Active
        //            // Only worry right now is to ensure that 
        //            // periodic maintenace of the conference calls takes place every now and then
        //            // ensuring that not too many records in the conference table.
        //            TwilioConferenceCall found = _dbContext.TwilioConferenceCalls
        //              .Where(c => c.CallIsActive
        //                        &&  c.ConferenceSID != null
        //                          &&    c.TwilioPhoneNumber == string.Format("+{0}", twilioPhonenumber))
        //              .OrderByDescending(c => c.CallStartTime).SingleOrDefault();

        //            conferenceName = found.ConferenceName;

        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
        //            ex.Message, ex.Source, ex.StackTrace, ex.InnerException));
        //    }
        //    return conferenceName;
        //}

        public string GetMostRecentConferenceNameFromNumber(ref int conFerenceId, string twilioPhonenumber)
        {
            //Note an intelligent method is 
            //needed to connect the conferencename to number 2 and 1
            //from is a stand in

            string conferenceName = "mango";

            try
            {
                using (var _dbContext = new TwilloDbContext())
                {
                    TwilioConferenceCall found = _dbContext.TwilioConferenceCalls
                      .Where(c => c.CallIsActive
                                && c.ConferenceSID != null
                                  && c.TwilioPhoneNumber == string.Format("+{0}", twilioPhonenumber))
                      .OrderByDescending(c => c.CallStartTime).SingleOrDefault();

                    if (found != null)
                    {
                        conferenceName = found.ConferenceName;
                        conFerenceId = found.Id;
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
                    ex.Message, ex.Source, ex.StackTrace, ex.InnerException));
            }
            return conferenceName;
        }

        public TwilioConferenceCall GetConferenceBySid(string callSid)
        {
            TwilioConferenceCall found = null;

            try
            {
                using (var _dbContext = new TwilloDbContext())
                {
                    found = _dbContext.TwilioConferenceCalls.FirstOrDefault(x => x.ConferenceSID == callSid);
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
                    ex.Message, ex.Source, ex.StackTrace, ex.InnerException));
            }
            return found;
        }

        public TwilioConferenceCall GetConferenceRecord(int id)
        {
            TwilioConferenceCall found = null;

            try
            {
                using (var _dbContext = new TwilloDbContext())
                {
                    found = _dbContext.TwilioConferenceCalls.Find(id);
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
                    ex.Message, ex.Source, ex.StackTrace, ex.InnerException));

                throw;
            }
            return found;
        }

        public void UpdateSystemStatus(int id, SystemStatus status)
        {
            try
            {
                using (var _dbContext = new TwilloDbContext())
                {
                    var found = _dbContext.TwilioConferenceCalls.Find(id);
                    found.SystemStatus = status;
                    _dbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
                    ex.Message, ex.Source, ex.StackTrace, ex.InnerException));

                throw;
            }
        }

        public void UpdateActiveStatus(int id, bool v)
        {
            try
            {
                using (var _dbContext = new TwilloDbContext())
                {
                    var found = _dbContext.TwilioConferenceCalls.Find(id);
                    found.CallIsActive = v;
                    found.SystemStatus = SystemStatus.CONFERENCE_COMPLETED;
                    _dbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
                    ex.Message, ex.Source, ex.StackTrace, ex.InnerException));
            }
        }

        public void LogMessage(string message, int id = 0)
        {
            //if (!Logging_enabled) return;
            
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


        public bool CheckAvailabilityAndFetchDetails(string Service_User_Twilio_Phone_Number,
                                    ref string struserName,
                                        ref string Service_User_Conference_With_Number, 
                                            ref string strTimeZoneID)
        {
            var retVal = true;

            try
            {
                using (var _dbContext = new TwilloDbContext())
                {
                    var user =
                               _dbContext
                                .User
                                 .Where(u => u.Service_User_Twilio_Phone_Number == Service_User_Twilio_Phone_Number)
                                  .FirstOrDefault();

                    struserName = user.UserFullName;
                    Service_User_Conference_With_Number = user.Service_User_Conference_With_Number;
                    strTimeZoneID = user.IANATimeZone;
                    retVal = Convert.ToBoolean(user.AvailableStatus);
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
                    ex.Message, ex.Source, ex.StackTrace, ex.InnerException));
            }
            return retVal;
        }

        public  string returnStatus(string twilioPhoneNumber)
        {
            var retVal = String.Empty;

            try
            {
                using (var context = new TwilloDbContext())
                {
                    var user =
                                context
                                .User
                                    .Where(u => u.Service_User_Twilio_Phone_Number == twilioPhoneNumber.ToString().Substring(2)).FirstOrDefault();

                    retVal = Convert.ToBoolean(user.AvailableStatus) ? "Available" : "Not Available";
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
                    ex.Message, ex.Source, ex.StackTrace, ex.InnerException));
                throw;
            }
            return retVal;
        }

        public  string updateStatus(Int16 requiredStatus, string twilioPhoneNumber)
        {
            var retVal = string.Empty;

            try
            {
                using (var context = new TwilloDbContext())
                {
                    var user =
                                context
                                .User
                                    .Where(u => u.Service_User_Twilio_Phone_Number == twilioPhoneNumber.ToString().Substring(2)).FirstOrDefault();

                    switch (Convert.ToBoolean(requiredStatus))
                    {
                        case true:
                            {
                                if ((user.AvailableStatus) == true)
                                    retVal = "Status is already Available";
                                // Do nothing
                                else
                                {
                                    user.AvailableStatus = true;
                                    context.SaveChanges();
                                    retVal = "Status is now set to Available";
                                }
                            }
                            break;
                        case false:
                            {
                                if ((user.AvailableStatus) == false)
                                    retVal = "Status is already Not Available";
                                // Do nothing
                                else
                                {
                                    user.AvailableStatus = false;
                                    context.SaveChanges();
                                    retVal = "Status is now set to Not Available";
                                }
                            }
                            break;
                        default:
                            break;
                    }

                }

            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
                    ex.Message, ex.Source, ex.StackTrace, ex.InnerException));
                throw;
            }
            return retVal;
        }




        public void UpdateConferenceStartTime(int id,DateTime conferenceStartTime)
        {
            try
            {
                using (var _dbContext = new TwilloDbContext())
                {
                    var found = _dbContext.TwilioConferenceCalls.Find(id);
                    found.ConferenceStartTime = conferenceStartTime;
                    _dbContext.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                ErrorMessage(string.Format("Error Message - {0} 1.Source {1}  2.Trace {2} 3.Inner Exception {3} ",
                    ex.Message, ex.Source, ex.StackTrace, ex.InnerException));
                throw;
            }
        }
    }
}

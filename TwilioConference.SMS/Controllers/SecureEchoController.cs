using System.Diagnostics;
using System.Net;
using System.Web.Mvc;

namespace SmsEcho.Controllers
{
    // http://twiliosmsecho.azurewebsites.net/secureecho
    public class SecureEchoController : Controller
    {
        [HttpPost]
        public ActionResult Index()
        {
            Debug.Assert(Request.Url != null, "Request.Url != null");

            var validator = new Twilio.TwilioRequestValidator();

            if (validator.IsValid(Request.Url.AbsoluteUri, Request.Headers["X-Twilio-Signature"], Request.Form))
            {
                return Content(
                    string.Format("<Response><Sms>{0}</Sms></Response>",
                    Request.Params["Body"]));
            }

            Response.SuppressFormsAuthenticationRedirect = true;
            return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Twilio authentication failed");
        }
    }

}

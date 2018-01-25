    using System.Web.Mvc;

namespace SmsEcho.Controllers
{
    // http://twiliosmsecho.azurewebsites.net/echo
    public class EchoController : Controller
    {
        [HttpPost]
        public ActionResult Index()
        {
            return Content(
                string.Format("<Response><Sms>{0}</Sms></Response>",
                Request.Params["Body"]));
        }
    }
}

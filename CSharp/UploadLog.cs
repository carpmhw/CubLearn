curl -i -X POST -H "Content-Type: multipart/form-data" -H "bkKey: 9527" "http://localhost:49278/Home/Get" -F "file=@C:\Users\Public\Pictures\Sample Pictures\Desert.jpg"

[AuthHeaderKey]
public ActionResult Get(HttpPostedFileBase file)
{
	if (file != null && file.ContentLength > 0)
	{
		try
		{
			string path = Path.Combine(Server.MapPath("~/App_Data"),
									   Path.GetFileName(file.FileName));
			file.SaveAs(path);
		}
		catch (Exception ex)
		{
			var actionName = ControllerContext.RouteData.Values["action"].ToString();

			Logger logger = LogManager.GetLogger(GetType().FullName);
			logger.LogExt(LogLevel.Info, ex.Message, actionName);

			return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
		}

	   
	}

	return new HttpStatusCodeResult(HttpStatusCode.OK);
}

using System.Web.Mvc;

namespace WebAppMVC.Infrastructure
{
    public class AuthHeaderKeyAttribute: AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            var key = filterContext.HttpContext.Request.Headers.GetValues("bkKey");
            if (key == null || key.Length == 0)
            {
                base.HandleUnauthorizedRequest(filterContext);
                return;
            }

            if (key[0] != "9527")
            {
                base.HandleUnauthorizedRequest(filterContext);
                return;
            }

        }
    }
}
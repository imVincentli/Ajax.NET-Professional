/*
 * MS	06-04-11	added use of IHttpAsyncHandler when configured with AjaxMethod attribute
 * MS	06-05-09	fixed response if type could not be loaded
 * MS	06-05-22	added possibility to have one file for prototype,core instead of two
 *					use default HttpSessionRequirement.ReadWrite if not configured, ajaxNet/ajaxSettings/oldStyle/sessionStateDefaultNone
 *					improved performance by saving GetCustomAttributes type array
 * MS	06-05-30	added ms.ashx
 * MS	06-06-07	added check for new urlNamespaceMappings/allowListOnly attribute
 * MS	06-06-11	removed WebEvent because of SecurityPermissions not available in medium trust environments
 * 
 */
using System;
using System.IO;
using System.Web;
using System.Web.Caching;
#if(NET20)
using System.Web.Management;
#endif

namespace AjaxPro
{
	public class AjaxHandlerFactory : IHttpHandlerFactory
	{
		#region IHttpHandlerFactory Members

		public void ReleaseHandler(IHttpHandler handler)
		{
			// TODO:  Add AjaxHandlerFactory.ReleaseHandler implementation
		}

		public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
		{
			// First of all we want to check what a request is running. There are three different
			// requests that are made to this handler:
			//		1) GET core,prototype,converter.ashx which will include the common AJAX communication
			//		2) GET typename,assemblyname.ashx which will return the AJAX wrapper JavaScript code
			//		3) POST typename,assemblyname.ashx which will invoke a method.
			// The first two requests will return the JavaScript code or a HTTP 304 (not changed).

			string filename = Path.GetFileNameWithoutExtension(context.Request.Path);
			Type t = null;

			Exception typeException = null;
			bool isInTypesList = false;

			try
			{
				if (Utility.Settings != null && Utility.Settings.UrlNamespaceMappings.Contains(filename))
				{
					isInTypesList = true;
					t = Type.GetType(Utility.Settings.UrlNamespaceMappings[filename].ToString(), true);
				}

				if (t == null)
					t = Type.GetType(filename, true);
			}
			catch (Exception ex)
			{
				typeException = ex;
			}

			switch(requestType)
			{
				case "GET":		// get the JavaScript files

					switch(filename.ToLower())
					{
						case "prototype":
							return new EmbeddedJavaScriptHandler("prototype");

						case "core":
							return new EmbeddedJavaScriptHandler("core");

						case "ms":
							return new EmbeddedJavaScriptHandler("ms");

						case "prototype-core":
						case "core-prototype":
							return new EmbeddedJavaScriptHandler("prototype,core");

						case "converter":
							return new ConverterJavaScriptHandler();

						default:

							if (typeException != null)
							{
#if(WEBEVENT)
								string errorText = string.Format(Constant.AjaxID + " Error", context.User.Identity.Name);

								Management.WebAjaxErrorEvent ev = new Management.WebAjaxErrorEvent(errorText, WebEventCodes.WebExtendedBase + 201, typeException);
								ev.Raise();
#endif
								return null;
							}

							if (Utility.Settings.OnlyAllowTypesInList == true && isInTypesList == false)
								return null;

							return new TypeJavaScriptHandler(t);
					}

				case "POST":	// invoke the method

					if (Utility.Settings.OnlyAllowTypesInList == true && isInTypesList == false)
						return null;

					IAjaxProcessor[] p = new IAjaxProcessor[2];
					
					p[0] = new XmlHttpRequestProcessor(context, t);
					p[1] = new IFrameProcessor(context, t);

					for(int i=0; i<p.Length; i++)
					{
						if(p[i].CanHandleRequest)
						{
							if (typeException != null)
							{
#if(WEBEVENT)
								string errorText = string.Format(Constant.AjaxID + " Error", context.User.Identity.Name);

								Management.WebAjaxErrorEvent ev = new Management.WebAjaxErrorEvent(errorText, WebEventCodes.WebExtendedBase + 200, typeException);
								ev.Raise();
#endif
								p[i].SerializeObject(new NotSupportedException("This method is either not marked with an AjaxMethod or is not available."));
								return null;
							}

							AjaxMethodAttribute[] ma = (AjaxMethodAttribute[])p[i].AjaxMethod.GetCustomAttributes(typeof(AjaxMethodAttribute), true);

							bool useAsync = false;
							HttpSessionStateRequirement sessionReq = HttpSessionStateRequirement.ReadWrite;

							if (Utility.Settings.OldStyle.Contains("sessionStateDefaultNone"))
								sessionReq = HttpSessionStateRequirement.None;

							if(ma.Length > 0)
							{
								useAsync = ma[0].UseAsyncProcessing;

								if(ma[0].RequireSessionState != HttpSessionStateRequirement.UseDefault)
									sessionReq = ma[0].RequireSessionState;
							}

							switch (sessionReq)
							{
								case HttpSessionStateRequirement.Read:
									if (!useAsync)
										return new AjaxSyncHttpHandlerSessionReadOnly(p[i]);
									else
										return new AjaxAsyncHttpHandlerSessionReadOnly(p[i]);

								case HttpSessionStateRequirement.ReadWrite:
									if (!useAsync)
										return new AjaxSyncHttpHandlerSession(p[i]);
									else
										return new AjaxAsyncHttpHandlerSession(p[i]);

								case HttpSessionStateRequirement.None:
									if (!useAsync)
										return new AjaxSyncHttpHandler(p[i]);
									else
										return new AjaxAsyncHttpHandler(p[i]);

								default:
									if (!useAsync)
										return new AjaxSyncHttpHandlerSession(p[i]);
									else
										return new AjaxAsyncHttpHandlerSession(p[i]);
							}
						}
					}
					break;
			}

			return null;
		}

		#endregion
	}
}
﻿using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Web;

namespace ProviderPortal {
	public class Global : System.Web.HttpApplication {

		protected void Application_BeginRequest(Object sender, EventArgs e) {
			/*
			 * The URLRewriter was taken from http://www.codeproject.com/aspnet/URLRewriter.asp and modified slightly.
			 * It will read the config section called 'urlrewrites' from web.config and process each rule 
			 * The rules are set of url transformations defined using regular expressions with support for substitutions (the ability to extract regex-matched portions of a string).
			 * There is only one rule currenty defined. It rewrites urls like: user/john ->user.aspx?username=john
			 */
			// System.Diagnostics.Debugger.Launch();
			Trace.TraceInformation("Processing {0} on {1} ", Request.HttpMethod, Request.Url);
			if (Request.QueryString.Count > 0)
				Trace.TraceInformation("Querystring follows: {0}", ToString(Request.QueryString));
			if (Request.Form.Count > 0)
				Trace.TraceInformation("Posted form follows: {0}", ToString(Request.Form));

			URLRewriter.Process();
		}

		protected void Application_AuthenticateRequest(Object sender, EventArgs e) {
			Trace.TraceInformation("Is Forms Authenticated = {0}", HttpContext.Current.User != null);
		}


		protected void Application_EndRequest(Object sender, EventArgs e) {
		}

		protected void Application_Error(Object sender, EventArgs e) {
			Trace.TraceError("An unhandled exception was raised. Details follow: {0}",
				HttpContext.Current.Server.GetLastError());
		}

		public static string ToString(NameValueCollection collection) {
			using (StringWriter sw = new StringWriter()) {
				foreach (string key in collection.Keys) {
					sw.WriteLine("{0} = '{1}'", key, collection[key]);
				}
				return sw.ToString();
			}
		}
	}
}
using NarfoxGameTools.Extensions;
using System;
using System.Collections.Generic;
using System.Net;

namespace NarfoxGameTools.Telemetry
{
    /// <summary>
    /// Provides cross-platform Google Analytics tracking using the
    /// Google Measurement Protocol as defined here:
    /// https://developers.google.com/analytics/devguides/collection/protocol/v1/
    /// </summary>
    public class GoogleAnalytics
    {
        /// <summary>
        /// Singleton pattern accessor
        /// </summary>
        /// <value>The instance.</value>
        public static GoogleAnalytics Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GoogleAnalytics();
                    initialized = false;
                }
                return instance;
            }
        }

        /// <summary>
        /// Optional application name. Can include OS or platform
        /// to filter analytics by platform. For example:
        /// 'GameName PC' or 'GameName Mac'
        /// </summary>
        public string AppName { get; set; }

        /// <summary>
        /// Optional build version of teh application
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// Optional screen resolution the app is
        /// running at. Providing this can provide
        /// data on most popular resolutions for testing
        /// </summary>
        public string Resolution { get; set; }

        /// <summary>
        /// The version of Google Analytics API to use.
        /// Currently only 1 is supported by Google.
        /// </summary>
        public int GAVersion { get; set; } = 1;

        /// <summary>
        /// The unique user identifier, used like a web cookie.
        /// If none exists, a guid is automatically created on first get.
        /// </summary>
        public string ClientId
        {
            get
            {
                if (string.IsNullOrWhiteSpace(clientId))
                {
                    clientId = Guid.NewGuid().ToString("N");
                }

                return clientId;
            }
            set
            {
                clientId = value;
            }
        }

        /// <summary>
        ///  Google analytics tracking code, usually starts with "UA-"
        /// </summary>
        public string TrackingId
        {
            get
            {
                return trackingId;
            }
        }

        private const string gaUrl = "http://www.google-analytics.com/collect";

        private static GoogleAnalytics instance;
        private static bool initialized;
        private WebClient client;
        private string trackingId;
        private string clientId;

        /// <summary>
        /// Private constructor to enforce singleton pattern
        /// </summary>
        private GoogleAnalytics()
        {

        }

        /// <summary>
        /// Initialize this class with a tracking ID and optional version
        /// </summary>
        /// <param name="trackingId">Google Analytics Tracking Code</param>
        public void Initialize(string trackingId, string appName = null, string appVersion = null, string resolution = null)
        {
            client = new WebClient();
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            this.trackingId = trackingId;
            this.AppName = appName;
            this.AppVersion = appVersion;
            this.Resolution = resolution;

            initialized = true;
        }

        /// <summary>
        /// Disposes of the web client, uninitializes and nullifies the instance
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="T:Masteroid.GoogleAnalytics"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="T:Masteroid.GoogleAnalytics"/> in an unusable state. After
        /// calling <see cref="Dispose"/>, you must release all references to the <see cref="T:Masteroid.GoogleAnalytics"/> so
        /// the garbage collector can reclaim the memory that the <see cref="T:Masteroid.GoogleAnalytics"/> was occupying.</remarks>
        public void Dispose()
        {
            client.Dispose();
            client = null;
            initialized = false;
            instance = null;
        }

        /// <summary>
        /// Helper encodes string for URLs
        /// </summary>
        /// <returns>Encoded string</returns>
        private string Encode(string str)
        {
            return System.Net.WebUtility.UrlEncode(str);
        }

        /// <summary>
		/// Fire an event tracking request to google
		/// </summary>
		/// <returns>The response from Google</returns>
		/// <param name="category">Event category</param>
		/// <param name="action">Event action</param>
		/// <param name="label">Optional label</param>
		/// <param name="label">Optional value</param>
		public void TrackEvent(string category, string action, string label = null, int? eventValue = null, string screenName = null)
        {
            var reqParams = new Dictionary<string, string>();

            // build custom event parameters
            reqParams.Add("t", "event");
            reqParams.Add("ec", category);
            reqParams.Add("ea", action);

            if (label != null)
            {
                reqParams.Add("el", label);
            }

            if (eventValue != null)
            {
                reqParams.Add("ev", eventValue.ToString());
            }

            if (screenName != null)
            {
                reqParams.Add("cd", screenName);
            }

            SendTrackingRequest(reqParams);
        }

        /// <summary>
        /// Fires screen tracking only request to google
        /// To fire screen and event tracking request, use the TrackEvent method and pass a screen name
        /// Some useful info is here: https://stackoverflow.com/questions/30414559/google-analytics-screenname-for-events
        /// </summary>
        /// <param name="screenName">The name of the screen</param>
        /// <returns>The response from Google</returns>
        public void TrackScreenView(string screenName)
        {
            var reqParams = new Dictionary<string, string>();

            // build custom params
            reqParams.Add("t", "screenview");
            reqParams.Add("cd", screenName);

            SendTrackingRequest(reqParams);
        }

        /// <summary>
        /// The base method that actually broadcasts a tracking
        /// request. Automatically sends common information
        /// </summary>
        /// <param name="customParameters">A dictionary of key/value pairs to be used as request payload</param>
        /// <returns>Byte array response from Google</returns>
        public void SendTrackingRequest(Dictionary<string, string> customParameters)
        {
            // EARLY OUT: don't track in DEBUG mode. This pollutes game stats!
#if DEBUG
            return;
#endif

            if (!initialized || string.IsNullOrWhiteSpace(TrackingId))
            {
                throw new Exception("Google Analytics has not been initialized or tracking code is invalid.");
            }

            // build common request parameters
            var rp = new Dictionary<string, string>();

            // build custom event parameters
            rp.Add("v", GAVersion.ToString());
            rp.Add("tid", TrackingId);
            rp.Add("cid", clientId);

            if (!string.IsNullOrWhiteSpace(AppName))
            {
                rp.Add("an", AppName);
            }

            if (!string.IsNullOrWhiteSpace(AppVersion))
            {
                rp.Add("av", AppVersion);
            }

            if (!string.IsNullOrWhiteSpace(Resolution))
            {
                rp.Add("sr", Resolution);
            }

            // merge custom params, overriding existing keys
            foreach (var kvp in customParameters)
            {
                if (rp.ContainsKey(kvp.Key))
                {
                    rp[kvp.Key] = kvp.Value;
                }
                else
                {
                    rp.Add(kvp.Key, kvp.Value);
                }
            }

            try
            {

                var nvc = rp.ToNameValueCollection();
                Task.Factory.StartNew(() => client.UploadValues(gaUrl, "post", nvc));
            }
            catch
            {
                // TODO: catch failures and either batch or log or somehow handle failed calls
                int m = 4;
            }
        }
    }
}

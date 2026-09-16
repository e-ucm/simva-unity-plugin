using System;

namespace Simva
{
    // Device authorization info (RFC 8628), mirrors
    // js-tracker OAuth2DeviceAuthorization + onDeviceAuthorizationInfo payload.
    // The auth backend supplies this; this UI only renders it.
    [Serializable]
    public class DeviceAuthInfo
    {
        public string device_code;
        public string user_code;
        public string verification_uri;
        public string verification_uri_complete;
        public int interval;
        public int expires_in;

        public string CompleteUrl
        {
            get { return !string.IsNullOrEmpty(verification_uri_complete) ? verification_uri_complete : verification_uri; }
        }

        public string ManualUrl
        {
            get { return !string.IsNullOrEmpty(verification_uri) ? verification_uri : verification_uri_complete; }
        }
    }
}

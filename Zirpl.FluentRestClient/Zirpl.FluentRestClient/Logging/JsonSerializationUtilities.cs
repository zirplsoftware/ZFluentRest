using Newtonsoft.Json;

namespace Zirpl.FluentRestClient.Logging
{
    /// <summary>
    /// Provides utility methods to convert objects to JSON
    /// </summary>
    public static class JsonSerializationUtilities
    {
        /// <summary>
        /// Converts the specified object to loggable JSON (formatted, ignoring reference loops, does not preserve references)
        /// 
        /// Use like this to avoid heavy JSON serialization operations if the log event won't be logged due to levels/logger name:
        /// <code>
        /// this.GetLog().Debug(formatHandler("Here is the data: {0}", myData.ToLoggableJson()));
        /// </code>
        /// </summary>
        public static string ToLoggableJson(this object data)
        {
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.None
            };

            var json = JsonConvert.SerializeObject(data, Formatting.Indented, settings);
            json = json.MaskAllSensitiveJsonValues();

            return json;
        }

        public static string MaskAllSensitiveJsonValues(this string json, bool indented = true)
        {
            json = json.MaskSensitiveJsonValues(indented, "password",
                //"confirmPassword", this will already be caught by the one above
                "authorizationToken");

            return json;
        }

        public static string MaskSensitiveJsonValues(this string json, bool indented = true, params string[] keySuffixes)
        {
            foreach (var key in keySuffixes)
            {
                json = MaskSensitiveData(json, key + "\":\"", '"');
                json = MaskSensitiveData(json, key + "\": \"", '"');
            }

            return json;
        }

        private static string MaskSensitiveData(string data, string preValueToken, char endValueNextChar)
        {
            // this method can be used for XML as well as long as the proper values are passed in
#if DEBUG
            return data;
#endif
            if (!string.IsNullOrWhiteSpace(data))
            {
                // this likely has a password in it... strip it out
                int indexOfNextToken = 0;
                while (indexOfNextToken < data.Length
                       && indexOfNextToken != -1)
                {
                    indexOfNextToken = data.IndexOf(preValueToken, indexOfNextToken, StringComparison.InvariantCultureIgnoreCase);
                    if (indexOfNextToken > -1)
                    {
                        var indexOfPasswordStart = indexOfNextToken + preValueToken.Length;
                        var indexOfStartOfNextItem = data.IndexOf(endValueNextChar.ToString(), indexOfPasswordStart,
                            StringComparison.InvariantCultureIgnoreCase);
                        int lengthOfPassword = indexOfStartOfNextItem == -1
                            ? data.Length - indexOfPasswordStart
                            : indexOfStartOfNextItem - indexOfPasswordStart;
                        if (lengthOfPassword > 0)
                        {
                            var fixedData = data.Substring(0, indexOfPasswordStart)
                                            + "[REMOVED_FOR_SECURITY]";
                            if (data.Length > indexOfPasswordStart + lengthOfPassword)
                            {
                                fixedData += data.Substring(indexOfPasswordStart + lengthOfPassword);
                            }

                            data = fixedData;
                        }
                    }

                    if (indexOfNextToken != -1)
                    {
                        indexOfNextToken += 1;
                    }
                }
            }
            return data;
        }
    }
}
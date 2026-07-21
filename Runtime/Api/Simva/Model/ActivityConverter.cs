using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Simva;

namespace Simva.Model
{
    public class ActivityConverter : JsonConverter<Activity>
    {
        public override Activity ReadJson(JsonReader reader, Type objectType, Activity existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var activity = new Activity();

            if (ApiClient.HasSimlets)
            {
                activity.Id = obj["activity_id"]?.ToString();
                activity.Name = obj["activity_name"]?.ToString();
                activity.Type = obj["activity_type"]?.ToString();
                activity.Details = new ActivityDetails
                {
                    TraceStorage =  obj["activity_trace_storage"]?.Value<bool>() ?? false,
                    Backup = obj["game_backup"]?.Value<bool>() ?? false,
                    ScormXapiByGame = obj["game_scorm_xapi"]?.Value<bool>() ?? false,
                    Uri = obj["game_url"]?.ToString()
                };
            } else {
                activity.Id = obj["_id"]?.ToString();
                activity.Name = obj["name"]?.ToString();
                activity.Type = obj["type"]?.ToString();
                if (obj["details"] != null && obj["details"].Type != JTokenType.Null)
                {
                    activity.Details = obj["details"].ToObject<ActivityDetails>();
                }
                else
                {
                    activity.Details = new ActivityDetails
                    {
                        TraceStorage = false,
                        Backup = false
                    };
                }
            }

            return activity;
        }

        public override void WriteJson(JsonWriter writer, Activity value, JsonSerializer serializer)
        {
            if (ApiClient.HasSimlets)
            {
                var obj = new JObject
                {
                    ["activity_id"] = value.Id,
                    ["activity_name"] = value.Name,
                    ["activity_type"] = value.Type,
                };
                if (value.Details != null)
                {
                    if(value.Details.TraceStorage) obj["activity_trace_storage"] = true;
                    if(value.Details.Backup) obj["game_backup"] = true;
                    if (value.Details.ScormXapiByGame) obj["game_scorm_xapi"] = true;
                    if (!string.IsNullOrEmpty(value.Details.Uri)) obj["game_url"] = value.Details.Uri;
                }
                obj.WriteTo(writer);
            }
            else
            {
                var obj = new JObject
                {
                    ["_id"] = value.Id,
                    ["name"] = value.Name,
                    ["type"] = value.Type
                };
                if (value.CopySurvey != null) obj["copysurvey"] = value.CopySurvey;
                if (value.Details != null) obj["details"] = JToken.FromObject(value.Details);
                obj.WriteTo(writer);
            }
        }
    }
}

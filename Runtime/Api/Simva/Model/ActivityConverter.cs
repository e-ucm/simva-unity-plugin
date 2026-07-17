using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Simva.Model
{
    public class ActivityConverter : JsonConverter<Activity>
    {
        public override Activity ReadJson(JsonReader reader, Type objectType, Activity existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var activity = new Activity();

            if (obj["_id"] != null)
            {
                activity.Id = obj["_id"]?.ToString();
                activity.Name = obj["name"]?.ToString();
                activity.Type = obj["type"]?.ToString();
                activity.Owners = obj["owners"]?.ToObject<List<string>>();
                activity.Study = obj["study"]?.ToString() ?? obj["simlet"]?.ToString();
                activity.Test = obj["session"]?.ToString() ?? obj["test"]?.ToString();
                activity.CopySurvey = obj["copysurvey"]?.ToString();
                if (obj["details"] != null && obj["details"].Type != JTokenType.Null)
                {
                    activity.Details = obj["details"].ToObject<ActivityDetails>();
                }
                activity.TraceStorage = obj["trace_storage"]?.Value<bool>() ?? false;
                activity.Backup = obj["backup"]?.Value<bool>() ?? false;
                if (activity.Details == null) activity.Details = new ActivityDetails();
            }
            else
            {
                activity.Id = obj["activity_id"]?.ToString();
                activity.Name = obj["activity_name"]?.ToString();
                activity.Type = obj["activity_type"]?.ToString();
                activity.TraceStorage = obj["activity_trace_storage"]?.Value<bool>() ?? false;
                activity.Backup = obj["game_backup"]?.Value<bool>() ?? false;
                activity.Details = new ActivityDetails
                {
                    ScormXapiByGame = obj["game_scorm_xapi"]?.Value<bool>() ?? false,
                    Uri = obj["game_url"]?.ToString()
                };
            }

            return activity;
        }

        public override void WriteJson(JsonWriter writer, Activity value, JsonSerializer serializer)
        {
            var obj = new JObject
            {
                ["_id"] = value.Id,
                ["name"] = value.Name,
                ["type"] = value.Type,
                ["trace_storage"] = value.TraceStorage,
                ["backup"] = value.Backup
            };
            if (value.Owners != null) obj["owners"] = JArray.FromObject(value.Owners);
            if (value.Study != null) obj["study"] = value.Study;
            if (value.Test != null) obj["test"] = value.Test;
            if (value.CopySurvey != null) obj["copysurvey"] = value.CopySurvey;
            if (value.Details != null) obj["details"] = JToken.FromObject(value.Details);
            obj.WriteTo(writer);
        }
    }
}

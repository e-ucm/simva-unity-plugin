using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Simva.Model
{
    public class ScheduleConverter : JsonConverter<Schedule>
    {
        public override Schedule ReadJson(JsonReader reader, Type objectType, Schedule existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            var schedule = new Schedule();

            if (obj["study"] != null)
            {
                schedule.Study = obj["study"]?.ToString();
                schedule.StudyName = obj["studyName"]?.ToString();
            }
            else
            {
                schedule.Study = obj["simlet"]?.ToString();
            }

            schedule.Next = obj["next"]?.ToString();
            schedule.Url = obj["url"]?.ToString();

            if (obj["activities"] != null)
            {
                schedule.Activities = obj["activities"].ToObject<Dictionary<string, Activity>>(serializer);
            }

            return schedule;
        }

        public override void WriteJson(JsonWriter writer, Schedule value, JsonSerializer serializer)
        {
            var obj = new JObject
            {
                ["next"] = value.Next,
                ["url"] = value.Url
            };
            if (value.StudyName != null) obj["studyName"] = value.StudyName;
            if (value.Study != null) obj["study"] = value.Study;
            if (value.Activities != null)
            {
                var activitiesObj = new JObject();
                foreach (var kv in value.Activities)
                {
                    var activityObj = new JObject
                    {
                        ["_id"] = kv.Value.Id,
                        ["name"] = kv.Value.Name,
                        ["type"] = kv.Value.Type,
                        ["trace_storage"] = kv.Value.TraceStorage,
                        ["backup"] = kv.Value.Backup
                    };
                    if (kv.Value.Details != null)
                    {
                        activityObj["details"] = JToken.FromObject(kv.Value.Details);
                    }
                    activitiesObj[kv.Key] = activityObj;
                }
                obj["activities"] = activitiesObj;
            }
            obj.WriteTo(writer);
        }
    }
}

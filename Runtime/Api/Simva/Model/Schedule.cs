using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Simva.Model {

  /// <summary>
  /// 
  /// </summary>
  [DataContract]
  public class Schedule {
    /// <summary>
    /// The next activity ID
    /// </summary>
    /// <value>The next activity ID</value>
    [DataMember(Name="next", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "next")]
    public string Next { get; set; }

    /// <summary>
    /// The scheduler url
    /// </summary>
    /// <value>The scheduler url</value>
    [DataMember(Name="url", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "url")]
    public string Url { get; set; }

    /// <summary>
    /// The next activity ID
    /// </summary>
    /// <value>The next activity ID</value>
    [DataMember(Name = "activities", EmitDefaultValue = false)]
    [JsonProperty(PropertyName = "activities")]
    public Dictionary<string, Activity> Activities { get; set; }

    /// <summary>
    /// Gets or Sets Study
    /// </summary>
    [DataMember(Name="study", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "study")]
    public string Study { get; set; }

    /// <summary>
    /// Gets or Sets Study
    /// </summary>
    [DataMember(Name="studyName", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "studyName")]
    public string StudyName { get; set; }


    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append("class InlineResponse200 {\n");
      sb.Append("  Next: ").Append(Next).Append("\n");
      sb.Append("  Url: ").Append(Url).Append("\n");
      sb.Append("  Study: ").Append(Study).Append("\n");
      sb.Append("  StudyName: ").Append(StudyName).Append("\n");
      sb.Append("}\n");
      return sb.ToString();
    }

    /// <summary>
    /// Get the JSON string presentation of the object
    /// </summary>
    /// <returns>JSON string presentation of the object</returns>
    public string ToJson() {
      return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

}
}

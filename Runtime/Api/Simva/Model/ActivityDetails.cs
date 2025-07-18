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
  public class ActivityDetails {
    /// <summary>
    /// Gets or Sets 
    /// </summary>
    [DataMember(Name = "uri", EmitDefaultValue = false)]
    [JsonProperty(PropertyName = "uri")]
    public String Uri { get; set; }

    /// <summary>
    /// Gets or Sets 
    /// </summary>
    [DataMember(Name = "user_managed", EmitDefaultValue = false)]
    [JsonProperty(PropertyName = "user_managed")]
    public string UserManaged { get; set; }

    /// <summary>
    /// Gets or Sets Type
    /// </summary>
    [DataMember(Name = "backup", EmitDefaultValue = false)]
    [JsonProperty(PropertyName = "backup")]
    public bool Backup { get; set; }

    /// <summary>
    /// Gets or Sets Type
    /// </summary>
    [DataMember(Name = "trace_storage", EmitDefaultValue = false)]
    [JsonProperty(PropertyName = "trace_storage")]
    public bool TraceStorage { get; set; }
    
    
    /// <summary>
    /// Gets or Sets Type
    /// </summary>
    [DataMember(Name = "scorm_xapi_by_game", EmitDefaultValue = false)]
    [JsonProperty(PropertyName = "scorm_xapi_by_game")]
    public bool ScormXapiByGame { get; set; }

    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append("class ActivityDetails {\n");
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

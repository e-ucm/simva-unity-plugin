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
  public class User {
    /// <summary>
    /// Gets or Sets Email
    /// </summary>
    [DataMember(Name="email", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "email")]
    public string Email { get; set; }

    /// <summary>
    /// Gets or Sets ExternalEntity
    /// </summary>
    [DataMember(Name="external_entity", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "external_entity")]
    public List<UserExternalEntity> ExternalEntity { get; set; }

    /// <summary>
    /// Gets or Sets Password
    /// </summary>
    [DataMember(Name="password", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "password")]
    public string Password { get; set; }

    /// <summary>
    /// Gets or Sets Role
    /// </summary>
    [DataMember(Name="role", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "role")]
    public string Role { get; set; }

      /// <summary>
    /// Gets or Sets Role
    /// </summary>
    [DataMember(Name="token", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "token")]
    public string Token { get; set; }

    /// <summary>
    /// Gets or Sets Username
    /// </summary>
    [DataMember(Name="groupid", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "groupid")]
    public string Groupid { get; set; }

    /// <summary>
    /// Gets or Sets Username
    /// </summary>
    [DataMember(Name="username", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "username")]
    public string Username { get; set; }

    /// <summary>
    /// Gets or Sets isToken
    /// </summary>
    [DataMember(Name="isToken", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "isToken")]
    public bool IsToken { get; set; }

    /// <summary>
    /// Gets or Sets useNewGeneration
    /// </summary>
    [DataMember(Name="useNewGeneration", EmitDefaultValue=false)]
    [JsonProperty(PropertyName = "useNewGeneration")]
    public bool UseNewGeneration { get; set; }

    /// <summary>
    /// Get the string presentation of the object
    /// </summary>
    /// <returns>String presentation of the object</returns>
    public override string ToString()  {
      var sb = new StringBuilder();
      sb.Append("class User {\n");
      sb.Append("  Email: ").Append(Email).Append("\n");
      sb.Append("  ExternalEntity: ").Append(ExternalEntity).Append("\n");
      sb.Append("  Password: ").Append(Password).Append("\n");
      sb.Append("  Role: ").Append(Role).Append("\n");
      sb.Append("  Username: ").Append(Username).Append("\n");
      sb.Append("  IsToken: ").Append(IsToken).Append("\n");
      sb.Append("  Token: ").Append(Token).Append("\n");
      sb.Append("  Groupid: ").Append(Groupid).Append("\n");
      sb.Append("  UseNewGeneration: ").Append(UseNewGeneration).Append("\n");
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

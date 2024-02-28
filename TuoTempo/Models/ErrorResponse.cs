using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;

namespace TuoTempo.Models
{
    public class ErrorResponse
    {
        [JsonProperty("result")]
        public string Result { get; set; }

        [JsonProperty("error_code")]
        public string ErrorCode { get; set; }

        [JsonProperty("required_params")]
        public List<string> RequiredParams { get; set; }

        [JsonProperty("error_msg")]
        public string ErrorMessage { get; set; }

        [JsonProperty("debug_msg")]
        public string DebugMessage { get; set; }


        public ErrorResponse()
        {
                RequiredParams = new List<string>();
        }
        public ErrorResponse(string errorCode, List<string> requiredParams, string errorMessage, string debugMessage)
        {
            Result = "ERROR";
            ErrorCode = errorCode;
            RequiredParams = requiredParams ?? new List<string>();
            ErrorMessage = errorMessage;
            DebugMessage = debugMessage;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
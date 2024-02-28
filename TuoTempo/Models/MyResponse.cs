using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TuoTempo.Models
{

    public class MyResponse
    {
        public string result { get; set; }

        [JsonProperty("return")]
        public object returnObject { get; set; }
    }
}
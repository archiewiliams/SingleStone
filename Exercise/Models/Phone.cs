using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace Exercise.Models
{
    public class Phone
    {
        [IgnoreDataMember]
        public int phoneid { get; set; }
        public string number { get; set; }
        public string type { get; set; }
    }
}
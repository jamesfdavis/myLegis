using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace info.Models.DTO
{
    public class Person
    {

        public Int32 ID { get; set; }
        public string Copy { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string LegisUrl { get; set; }
        public string PhotoUrl { get; set; }
        public string WikiUrl { get; set; }

    }
}
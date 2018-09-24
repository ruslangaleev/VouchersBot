using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TravelerBot.MVC.Data.Models
{
    public class OneTimePassword
    {
        public string Password { get; set; }

        public DateTime LifeTime { get; set; }
    }
}
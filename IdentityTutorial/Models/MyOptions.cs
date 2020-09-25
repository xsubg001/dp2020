using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityTutorial.Models
{
    public class MyOptions
    {
        public MyOptions()
        {
            Option1 = "Value set in constructor";
        }

        public string Option1 { get; set; }
        public int Option2 { get; set; } = 5;
    }
}

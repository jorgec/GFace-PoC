using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFace
{
    class PersonResponse
    {
        public String personId { get; set; }
        public String name { get; set; }
        public String userData { get; set; }
        public List<String> persistedFaceIds { get; set; }
    }
}

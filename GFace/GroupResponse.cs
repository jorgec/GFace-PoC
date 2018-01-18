using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFace
{
    class GroupResponse
    {
        [JsonProperty("groups")]
        public String[][] groups { get; set; }
        [JsonProperty("messyGroup")]
        public Object[] messyGroup { get; set; }
    }
}

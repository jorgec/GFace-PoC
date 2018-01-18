using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GFace
{
    class PersonIdentifyRequest
    {
        public String[] faceIds { get; set; }
        public String personGroupId;
        public String confidenceThreshold;
    }
}

# GFace-PoC - DEPRECATED
## GFace Proof of Capability

OpenCV for initial image processing and face extraction, Microsoft Cognitive Services for face identification, grouping, and validation.

### Log Results description:
- Detected - total number of faces detected from live camera
- Undetected - deprecated
- Identified - total number of faces that have been identified (pre-trained); repeats are counted
- Repeats - total number of repeat identifications (person identified more than once)
- Unkown - total number of faces that are not in the trained database
- Unique - number of unique persons identified
- Messy - number of persons that cannot be properly identified or grouped

### App.config
- interval - in ms; determines FPS. A value of 1000 means that 1 frame will be captured every 1 second. 41 will be 24 FPS (default: 3000)
- exposure - camera exposure compensation (default: 0)
- main_cascade - haar_cascade used by OpenCV to detect faces
- api_key - Microsoft Face API key
- api_url - API base endpoint
- api_person_group - trained person group
- local_storage - local path where faces are stored as PNG
- db_server, db_user, db_password, db_db - database connectivity

### NuGet Packages
- EasyHttp.1.7.0
- JsonFx.2.0.1209.2802
- Microsoft.AspNet.WebApi.Client.5.2.3
- Microsot.Bcl.1.1.10
- Microsoft.Cbl.Build.1.0.14
- Microsoft.Net.Http.2.2.29
- Microsoft.ProjectOxford.Common.1.0.324
- Microsoft.ProjectOxford.Face.1.3.0
- MySql.Data.6.9.10
- Netwonsoft.Json.9.0.1
- OpenCvSharp3-AnyCPU.3.3.1.20171117
- ServiceStack.Common.5.0.2
- ServiceStack.Interfaces.5.0.2
- ServiceStack.OrmLite.5.0.2
- ServiceStack.OrmLite.MySql.5.0.2
- ServiceStack.Text.5.0.2

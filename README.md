# GFace-PoC
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

# GFace-PoC
## GFace Proof of Capability

OpenCV for initial image processing and face extraction, Microsoft Cognitive Services for face identification, grouping, and validation.

Log Results description:
- Detected - total number of faces detected from live camera
- Undetected - deprecated
- Identified - total number of faces that have been identified (pre-trained); repeats are counted
- Repeats - total number of repeat identifications (person identified more than once)
- Unkown - total number of faces that are not in the trained database
- Unique - number of unique persons identified
- Messy - number of persons that cannot be properly identified or grouped

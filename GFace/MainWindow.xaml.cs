using EasyHttp.Http;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GFace
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        // settings
        private AppSettingsReader settingsReader;

        // camera
        private CameraState cs;
        private VideoCapture camera;
        private Timer timer;
        private Mat mat;

        // opencv
        private CascadeClassifier haarCascade;
        private OpenCvSharp.Face.LBPHFaceRecognizer recognizer;

        // image
        private Bitmap imgSource;
        private List<Mat> detectedFaces;

        // counters
        private int totalFaceCount = 0;
        private int totalIdentifiedCount = 0;
        private int totalUniquesCount = 0;
        private int totalMessyCount = 0;
        private int totalUnknownCount = 0;
        private int totalRepeats = 0;
        private Dictionary<String, int> identifiedNames;

        // storage
        private String local_storage;

        // faceAPI
        /// <summary>
        /// Faces to group
        /// </summary>
        /// 

        private List<String> _faceIds;

        private Face[] Faces;

        FaceServiceClient faceServiceClient;

        private static HttpClient http;
        private String api_key;
        private String api_url;
        private String api_person_group;

        // Status Bar
        private void _(String t)
        {
            StatusText.Text = t;
        }     

        public MainWindow()
        {            
            InitializeComponent();
            http = new HttpClient();            

            LblDetected.Content = totalFaceCount;

            _faceIds = new List<string>();

            settingsReader = new AppSettingsReader();
            local_storage = settingsReader.GetValue("local_storage", typeof(string)).ToString();
            api_key = settingsReader.GetValue("api_key", typeof(string)).ToString();
            api_url = settingsReader.GetValue("api_url", typeof(string)).ToString();
            api_person_group = settingsReader.GetValue("api_person_group", typeof(string)).ToString();
            faceServiceClient = new FaceServiceClient(api_key, api_url);

            detectedFaces = new List<Mat>();
            identifiedNames = new Dictionary<string, int>();
            recognizer = OpenCvSharp.Face.LBPHFaceRecognizer.Create(1, 8, 8, 8, 123);
            recognizer.Dispose();
            
            haarCascade = new CascadeClassifier(settingsReader.GetValue("main_cascade", typeof(string)).ToString());
            _("Initializing...");
            InitializeCamera();
        }

        private void InitializeCamera()
        {
            cs = new CameraState();
            cs.Interval = Convert.ToInt32(settingsReader.GetValue("interval", typeof(string)));
            cs.Exposure = Convert.ToInt32(settingsReader.GetValue("exposure", typeof(string)));
            _(String.Format("Camera initialized at {0} FPS and {1} EXP", 1000.00/cs.Interval, cs.Exposure));
            cs.IsRenderingFaces = false;
            cs.IsRunning = false;
        }

        private void CameraOn()
        {
            camera = new VideoCapture();
            camera.Open(0);
            timer = new Timer(GrabImage, 0, 0, cs.Interval);
            cs.IsRunning = true;
        }

        private void CameraOff()
        {
            this.Dispatcher.Invoke(() =>
            {
                camera.Dispose();
                timer.Dispose();
                cs.IsRunning = false;
            });
        }

        private void BtnRecord_Click(object sender, RoutedEventArgs e)
        {            
            if (cs.IsRunning)
            {
                CameraOff();
            }
            else
            {
                CameraOn();
                _("Recording...");
            }
        }

        private void RenderFaceInfo()
        {
            var srcImage = mat;
            _(String.Format("Loading image..."));
            Cv2.WaitKey(1);

            var grayImage = new Mat();
            Cv2.CvtColor(srcImage, grayImage, ColorConversionCodes.BGRA2GRAY);
            Cv2.EqualizeHist(grayImage, grayImage);
            var cascade = haarCascade;

            Int32[] rejectLevels = new Int32[100];
            Double[] levelWeights = new Double[100];

            var faces = cascade.DetectMultiScale(
                    grayImage,
                    out rejectLevels,
                    out levelWeights,
                    1.1,
                    3,
                    HaarDetectionType.DoCannyPruning | HaarDetectionType.DoRoughSearch | HaarDetectionType.ScaleImage,
                    new OpenCvSharp.Size(50, 50),
                    new OpenCvSharp.Size(500, 500),
                    true
                );


            if (faces.Length > 0)
            {
                _("Face found! Extracting...");
                System.Drawing.Rectangle[] boxes = new System.Drawing.Rectangle[10];
                Bitmap faceBitmap = new Bitmap(mat.ToBitmap());
                int facesCount = faces.Length;

                for(int i = 0; i < facesCount; i++)
                {
                    totalFaceCount++;
                    LblDetected.Content = totalFaceCount;
                    var faceRect = faces[i];
                    boxes[i] = new System.Drawing.Rectangle(faceRect.X, faceRect.Y, faceRect.Width, faceRect.Height);
                    using(var g = Graphics.FromImage(faceBitmap))
                    {
                        g.DrawString(String.Format("Weight: {0}", levelWeights[i]), new Font("Tahoma", 8), System.Drawing.Brushes.White, new RectangleF(faceRect.X, faceRect.Y, faceRect.Width, 30));
                    }
                    detectedFaces.Add(new Mat(srcImage, faceRect));
                }
               
                using(var g = Graphics.FromImage(faceBitmap))
                {
                    g.DrawRectangles(Pens.Red, boxes);
                }
                CamProcessed.Source = faceBitmap.ToBitmapSource();
                
            }
        }

        private void GrabImage(object state)
        {
            if (cs.IsRunning)
            {
                this.Dispatcher.Invoke(() =>
                {
                    cs.FrameCount++;
                    _(String.Format("Frame #{0} captured at {1} FPS", cs.FrameCount, 1000.00 / cs.Interval));

                    try
                    {
                        mat = new Mat();
                        camera.Read(mat);
                        imgSource = BitmapConverter.ToBitmap(mat);
                        this.Dispatcher.Invoke(() =>
                        {
                            CamRaw.Source = imgSource.ToBitmapSource();
                            if (cs.IsRenderingFaces)
                            {
                                RenderFaceInfo();
                            }

                        });

                    }
                    catch (ObjectDisposedException e)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            _("Camera closed. " + e.Message);
                        });
                    }
                });
            }
        }

        private void BtnDetectFaces_Click(object sender, RoutedEventArgs e)
        {
            if( cs.IsRenderingFaces)
            {
                cs.IsRenderingFaces = false;
            }
            else
            {
                cs.IsRenderingFaces = true;
            }
        }

        private void BtnSaveFaces_Click(object sender, RoutedEventArgs e)
        {
            if(detectedFaces.Count > 0)
            {
                int fileCount = (from file in System.IO.Directory.EnumerateFiles(@local_storage, "*", System.IO.SearchOption.AllDirectories) select file).Count() + 1;
                int numDetected = detectedFaces.Count;

                foreach(var f in detectedFaces)
                {
                    String path = String.Format(@"{0}\{1}.png", local_storage, fileCount);
                    fileCount++;
                    f.SaveImage(path);
                }
            }
            else
            {
                MessageBox.Show("No faces in memory");
            }
        }

        private void BtnClearLocalCache_Click(object sender, RoutedEventArgs e)
        {
            detectedFaces.Clear();
        }

        private async void RemoteDetectFaces()
        {
            String[] imageFiles = Directory.GetFiles(local_storage, "*", SearchOption.TopDirectoryOnly);
            foreach(var imageFile in imageFiles)
            {
                _("Calling service... " + imageFile);
                try
                {
                    using (Stream imageFileStream = File.OpenRead(imageFile))
                    {
                        Faces = await faceServiceClient.DetectAsync(imageFileStream, true, true, new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Glasses, FaceAttributeType.HeadPose, FaceAttributeType.FacialHair, FaceAttributeType.Emotion, FaceAttributeType.Hair, FaceAttributeType.Makeup, FaceAttributeType.Occlusion, FaceAttributeType.Accessories, FaceAttributeType.Noise, FaceAttributeType.Exposure, FaceAttributeType.Blur });
                        var faceIds = Faces.Select(f => f.FaceId).ToArray();
                        foreach(var f in faceIds)
                        {
                            _faceIds.Add(f.ToString());
                        }
                    }
                    _("Done!");
                    

                }
                catch (FaceAPIException e)
                {
                    _(e.ErrorMessage);
                }
            }
            
        }
        
        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            GetPersonsInGroup(api_person_group);

        }

        private void UpdateIdentifiedCount(int n)
        {
            int identifiedCount = identifiedNames.Count;
            LblUnique.Content = identifiedCount;
        }        

        // httpclient
        private void GetPersonsInGroup(String group)
        {
            try
            {
                http.Request.RawHeaders.Add("Ocp-Apim-Subscription-Key", api_key);
            }catch(System.ArgumentException e)
            {
                Console.Write(e);
            }
            http.Request.Accept = "application/json";
            

            String url = String.Format("{0}/persongroups/{1}/persons", api_url, group);

            _("Getting persons in group...");

            HttpResponse response = http.Get(url);

            var responseArray = response.DynamicBody;
            try
            {
                LblStoredNamesCount.Content = responseArray.Length;
            }catch(Exception e)
            {
                LblStoredNamesCount.Content = 0;
            }
            _("Done.");
        }

        private GroupResponse GroupPersons()
        {
            try
            {
                http.Request.RawHeaders.Add("Ocp-Apim-Subscription-Key", api_key);
            }
            catch (System.ArgumentException e)
            {
                Console.Write(e);
            }
            http.Request.Accept = "application/json";

            String url = String.Format("{0}/group", api_url);

            FaceGroupRequest faceGroup = new FaceGroupRequest();
            faceGroup.faceIds = _faceIds.ToArray();
            
            var response = http.Post(url, faceGroup, "application/json");
            GroupResponse groupResponse = new GroupResponse();
            groupResponse.groups = response.DynamicBody.groups;
            groupResponse.messyGroup = response.DynamicBody.messyGroup;

            return groupResponse;

        }

        private async void RemoteIdentifyFaces()
        {
            if (_faceIds.Count > 0)
            {
                Guid[] faceGuIds = new Guid[_faceIds.Count];
                for(int i = 0; i < _faceIds.Count; i++)
                {
                    faceGuIds[i] = Guid.Parse(_faceIds[i]);
                }

                _("Attempting to identify...");
                try
                {
                    String groupId = settingsReader.GetValue("api_person_group", typeof(string)).ToString();
                    var results = await faceServiceClient.IdentifyAsync(groupId, faceGuIds);
                    foreach (var identifyResult in results)
                    {
                        _faceIds.Add(identifyResult.FaceId.ToString());
                        if (identifyResult.Candidates.Length != 0)
                        {
                            _(String.Format("{0} identified faces", identifyResult.Candidates.Length));
                            totalIdentifiedCount = totalIdentifiedCount + identifyResult.Candidates.Length;
                            LblIdentified.Content = totalIdentifiedCount;
                            var candidateId = identifyResult.Candidates[0].PersonId;
                            //identifyResult.
                            try
                            {
                                _("Trying to identify person...");
                                var person = await faceServiceClient.GetPersonAsync(groupId, candidateId);

                                // user identificated: person.name is the associated name
                                Console.WriteLine(String.Format("****************** {0} identified ******************", person.Name));
                                if (identifiedNames.ContainsKey(person.Name))
                                {
                                    int repeats = identifiedNames[person.Name];
                                    identifiedNames[person.Name] = repeats + 1;
                                    totalRepeats++;
                                    LblRepeats.Content = totalRepeats;
                                }
                                else
                                {
                                    identifiedNames.Add(person.Name, 1);
                                }
                            }
                            catch (FaceAPIException eGetPersonAsync)
                            {
                                _(eGetPersonAsync.ErrorMessage);
                            }
                            catch (TaskCanceledException terr)
                            {
                                _(terr.Message);
                            }
                        }
                        else
                        {
                            totalUnknownCount++;
                            LblUnkown.Content = totalUnknownCount;
                        }
                    }
                }
                catch (FaceAPIException ee)
                {
                    _(ee.ErrorMessage);
                }
            }
            else
            {
                MessageBox.Show("Upload faces first");
            }

            _("Identification done!");
        }

        private void BtnUploadFaces_Click(object sender, RoutedEventArgs e)
        {
            RemoteDetectFaces();
        }

        private void BtnRemoteIdentify_Click(object sender, RoutedEventArgs e)
        {
            RemoteIdentifyFaces();
        }

        private void BtnGroupFaces_Click(object sender, RoutedEventArgs e)
        {
            GroupResponse r = GroupPersons();
            totalUniquesCount = totalUniquesCount + r.groups.Length;
            totalMessyCount = totalMessyCount + r.messyGroup.Length;

            LblUnique.Content = totalUniquesCount;
            LblMessy.Content = totalMessyCount;
        }

        private void BtnClearServer_Click(object sender, RoutedEventArgs e)
        {
            RemoteClearServer(api_person_group);
        }

        private void RemoteClearServer(String group)
        {
            try
            {
                http.Request.RawHeaders.Add("Ocp-Apim-Subscription-Key", api_key);
            }
            catch (System.ArgumentException ex)
            {
                Console.Write(ex);
            }
            http.Request.Accept = "application/json";


            String url = String.Format("{0}/persongroups/{1}", api_url, group);

            _("Clearing server...");

            HttpResponse response = http.Delete(url);

            GetPersonsInGroup(group);
            _("Done.");
        }
    }
}

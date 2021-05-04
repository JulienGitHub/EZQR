using AForge.Imaging.Filters;
using AForge.Video;
using AForge.Video.DirectShow;
using EZQRReader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using ZXing;

namespace QRReader
{
    public partial class Main : Form
    {
        private readonly ZXing.BarcodeReader barcodeReader;
        private readonly IList<Result> lastResults;
        private readonly IList<Result> fullResults;
        public Main()
        {
            InitializeComponent();

            FormBorderStyle = FormBorderStyle.FixedSingle;

            this.SuspendLayout();

            barcodeReader = new ZXing.BarcodeReader
            {
                AutoRotate = false,
                TryInverted = false,
                Options = new ZXing.Common.DecodingOptions { TryHarder = false }
            };

            barcodeReader.ResultFound += result =>
            {
                lastResults.Add(result);
            };

            lastResults = new List<Result>();
            fullResults = new List<Result>();

            listView.Columns.Add("code");
            listView.Columns[0].Width = 200;

            statusStrip.Items.Add(" ");
        }

        public delegate void FrameDecoding(Bitmap b);
        string sLastCodeRedeemedBeep = "";
        int m_ColorCpt = 0;
        bool bIsDecoding = false;
        int m_iNoCode = 10;
        private void _FrameDecoding(Bitmap b)
        {
            if (bIsDecoding)
            {
                return;
            }
            bIsDecoding = true;
            barcodeReader.Options.TryHarder = false;

            Rectangle rectOut = new Rectangle();
            int a = Decode(b, false, new List<BarcodeFormat> { BarcodeFormat.QR_CODE }, 0, 0, 1, 1, out rectOut);
            int iCountNew = 0;
            if (a > 0)
            {
                m_iNoCode = 0;
                for (int j = 0; j < lastResults.Count; j++)
                {
                    bool bFound = false;
                    for (int k = 0; k < fullResults.Count && !bFound; k++)
                    {
                        if (fullResults[k].Text.Equals(lastResults[j].Text))
                            bFound = true;
                    }
                    if (!bFound)
                    {
                        fullResults.Add(lastResults[j]);

                        iCountNew++;
                    }
                }
                if (iCountNew >= 1)
                {
                    Action beep = Console.Beep;
                    beep.BeginInvoke(null, null);
                    UpdateResults();
                    sLastCodeRedeemedBeep = lastResults[0].Text;
                    statusStrip.Items[0].Text = lastResults[0].Text + " scanned";
                }
                else
                {
                    if (!lastResults[0].Text.Equals(sLastCodeRedeemedBeep))
                    {
                        Action beep = () => Console.Beep(500, 500);
                        beep.BeginInvoke(null, null);
                        sLastCodeRedeemedBeep = lastResults[0].Text;
                        statusStrip.Items[0].Text = lastResults[0].Text + " Already scanned";
                    }
                }
            }
            else
            {
                m_iNoCode++;
                if (m_iNoCode > 10)
                {
                    sLastCodeRedeemedBeep = "";
                    m_ColorCpt--;
                    if (m_ColorCpt < 0)
                        listView.BackColor = Color.FromArgb(255, 255, 200, 200);
                }
            }
            bIsDecoding = false;
        }
        private void UpdateResults()
        {
            listView.BeginUpdate();

            if (listView.Items.Count > 0)
                listView.Items.Clear();
            listView.BackColor = Color.FromArgb(255, 200, 255, 200);
            m_ColorCpt = 3;
            for (int k = 0; k < fullResults.Count; k++)
            {
                listView.Items.Insert(0, fullResults[k].Text.Replace("-", ""));
            }

            listView.EndUpdate();
        }

        private int Decode(Bitmap inputBitmap, bool tryMultipleBarcodes, IList<BarcodeFormat> possibleFormats, int x, int y, float fXMultiplier, float fYMultiplier, out Rectangle rectFound)
        {
            rectFound = new Rectangle();
            lastResults.Clear();
            IList<Result> results = null;
            var previousFormats = barcodeReader.Options.PossibleFormats;
            if (possibleFormats != null)
                barcodeReader.Options.PossibleFormats = possibleFormats;

            if (tryMultipleBarcodes)
            {
                results = barcodeReader.DecodeMultiple(inputBitmap);
            }
            else
            {
                var result = barcodeReader.Decode(inputBitmap);
                if (result != null)
                {
                    if (results == null)
                    {
                        results = new List<Result>();
                    }
                    results.Add(result);
                }
            }

            barcodeReader.Options.PossibleFormats = previousFormats;

            if (results == null)
            {
                return 0;
            }

            return results.Count;
        }

        private void ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            Clipboard.SetText(e.Item.Text);
            e.Item.BackColor = Color.Gray;
        }


        private static string _usbcamera;
        public string usbcamera
        {
            get { return _usbcamera; }
            set { _usbcamera = value; }
        }

        private FilterInfoCollection videoDevices;
        private VideoCaptureDevice videoDevice;
        private void getListCameraUSB()
        {
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count != 0)
            {
                // add all devices to combo
                foreach (FilterInfo device in videoDevices)
                {
                    comboBox1.Items.Add(device.Name);

                }
            }
            else
            {
                comboBox1.Items.Add("No DirectShow devices found");
            }

            comboBox1.SelectedIndex = 0;

        }

        bool m_bOpen = false;
        private void webcamButton_Click(object sender, EventArgs e)
        {
            if (comboBox1.Items.Count == 0)
                return;

            m_bOpen = !m_bOpen;

            if (m_bOpen)
            {
                OpenCamera();
                comboBox1.Enabled = false;
            }
            else
            {
                CloseCurrentVideoSource();
                comboBox1.Enabled = true;
            }
        }

        private void findCam_Click(object sender, EventArgs e)
        {
            getListCameraUSB();
        }

        private ArrayList listCamera = new ArrayList();

        public void OpenVideoSource(IVideoSource source)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                // stop current video source
                CloseCurrentVideoSource();

                // start new video source
                videoSourcePlayer1.VideoSource = source;

                videoSourcePlayer1.Start();

                this.Cursor = Cursors.Default;
            }
            catch { }
        }

        bool bStopping = false;
        public void CloseCurrentVideoSource()
        {
            try
            {
                bStopping = true;
                if (videoSourcePlayer1.VideoSource != null)
                {
                    videoSourcePlayer1.SignalToStop();

                    // wait ~ 3 seconds
                    for (int i = 0; i < 50; i++)
                    {
                        if (!videoSourcePlayer1.IsRunning)
                            break;
                        videoSourcePlayer1.SignalToStop();
                        System.Threading.Thread.Sleep(200);
                    }

                    if (videoSourcePlayer1.IsRunning)
                    {
                        videoSourcePlayer1.Stop();
                    }

                    videoSourcePlayer1.VideoSource = null;
                }
                bStopping = false;
            }
            catch { }
        }

        private void OpenCamera()
        {
            try
            {
                usbcamera = comboBox1.SelectedIndex.ToString();
                videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (videoDevices.Count != 0)
                {
                    foreach (FilterInfo device in videoDevices)
                    {
                        listCamera.Add(device.Name);

                    }
                }
                else
                {
                    MessageBox.Show("Camera devices found");
                }

                videoDevice = new VideoCaptureDevice(videoDevices[Convert.ToInt32(usbcamera)].MonikerString);
                for (int i = 0; i < videoDevice.VideoCapabilities.Length; i++)
                {
                    if (videoDevice.VideoCapabilities[i].FrameSize.Width == 800)
                        videoDevice.VideoResolution = videoDevice.VideoCapabilities[i];
                }
                OpenVideoSource(videoDevice);
            }
            catch (Exception err)
            {
                MessageBox.Show(err.ToString());
            }

        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            videoSourcePlayer1.SignalToStop();
            videoSourcePlayer1.NewFrame -= new AForge.Controls.VideoSourcePlayer.NewFrameHandler(videoSourcePlayer1_NewFrame); // as sugested
            videoSourcePlayer1 = null;
        }

        private void videoSourcePlayer1_NewFrame(object sender, ref Bitmap image)
        {
            try
            {
                if (bStopping)
                    return;

                Mirror filter = new Mirror(false, true);
                filter.ApplyInPlace(image);

                this.Invoke(new FrameDecoding(_FrameDecoding), image);
            }
            catch
            { }
        }

        private void clearList_Click(object sender, EventArgs e)
        {
            listView.Items.Clear();
            fullResults.Clear();
            lastResults.Clear();
        }

        private void infoButton_Click(object sender, EventArgs e)
        {
            InfoForm frm = new InfoForm();
            frm.Show();
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            string sText = "";
            for (int i = 0; i < listView.Items.Count; i++)
                sText += listView.Items[i].Text + "\n";
            Clipboard.SetText(sText);
        }
    }
}

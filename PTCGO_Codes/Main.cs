using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using ZXing;
using ZXing.Presentation;
using ZXing.Client.Result;

using Tesseract;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Threading;

namespace PTCGO_Codes
{
    public partial class Main : Form
    {
        private Size OriginalSize;

        bool m_bMouseDown;

        private readonly ZXing.BarcodeReader barcodeReader;
        private readonly IList<ResultPoint> resultPoints;
        private readonly IList<Result> lastResults;
        private readonly IList<Result> fullResults;
        private readonly IList<Result> newResults;
        private readonly IList<string> tesseractResults;
        private readonly IList<Point> croppedOrigin;
        private string sData = "";
        private Point RectStartPoint;
        private Rectangle SelectionRect = new Rectangle();
        private Brush selectionBrush = new SolidBrush(Color.FromArgb(255, 72, 145, 220));
        Image OriginalImage;
        MouseButtons currentButton = new MouseButtons();

        Bitmap bCurrent = new Bitmap(1, 1);
        bool boolCurrent = false;

        int iImageCount = 0;

        TimeSpan elapsed = new TimeSpan();

        public Main()
        {
            InitializeComponent();

            File.Delete("debug.txt");

            TextWriterTraceListener[] listeners = new TextWriterTraceListener[] {
            new TextWriterTraceListener(Console.Out)
            };

            Debug.Listeners.AddRange(listeners);

            barcodeReader = new ZXing.BarcodeReader
            {
                AutoRotate = true,
                TryInverted = false,
                Options = new ZXing.Common.DecodingOptions { TryHarder = false } 
            };
            barcodeReader.ResultPointFound += point =>
            {
                if (point == null)
                    resultPoints.Clear();
                else
                    resultPoints.Add(point);
            };
            barcodeReader.ResultFound += result =>
            {
                lastResults.Add(result);                
            };


            resultPoints = new List<ResultPoint>();
            lastResults = new List<Result>();
            fullResults = new List<Result>();
            newResults = new List<Result>();
            tesseractResults = new List<string>();
            croppedOrigin = new List<Point>();

            m_bMouseDown = false;
        }


        private void pictureBox_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            // Determine the initial rectangle coordinates...
            m_bMouseDown = true;
            currentButton = e.Button;
            RectStartPoint = e.Location;
            Invalidate();            
        }

        // Draw Rectangle
        //
        private void pictureBox_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            currentButton = e.Button;
            //if (e.Button == MouseButtons.Left)
            {
                Point tempEndPoint = e.Location;
                SelectionRect.Location = new Point(
                    Math.Min(RectStartPoint.X, tempEndPoint.X),
                    Math.Min(RectStartPoint.Y, tempEndPoint.Y));
                SelectionRect.Size = new Size(
                    Math.Abs(RectStartPoint.X - tempEndPoint.X),
                    Math.Abs(RectStartPoint.Y - tempEndPoint.Y));
                pictureBox.Invalidate();
            }
        }
        
        // Draw Area
        //
        private void pictureBox_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            // Draw the rectangle...
            if (pictureBox.Image != null)
            {
                if (SelectionRect != null && SelectionRect.Width > 0 && SelectionRect.Height > 0)
                {
                    if (currentButton == MouseButtons.Left)
                    {
                        Pen redPen = new Pen(Color.Red, 3);
                        e.Graphics.DrawRectangle(redPen, SelectionRect);
                    }
                    if (currentButton == MouseButtons.Right)
                    {
                        Pen bluePen = new Pen(Color.Blue, 3);
                        e.Graphics.DrawRectangle(bluePen, SelectionRect);
                    }
                }
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (!m_bMouseDown)
                return;
            m_bMouseDown = false;
            currentButton = e.Button;
            Point p = new Point(e.Location.X - 1, e.Location.Y - 1);
            if (SelectionRect.Contains(p))
            {
                // Debug.WriteLine("Right click");

                float ratioX = OriginalSize.Width / (float)pictureBox.Width;
                float ratioY = OriginalSize.Height / (float)pictureBox.Height;

                Rectangle cropping = new Rectangle();
                cropping.X = (int)(SelectionRect.X * ratioX);
                cropping.Y = (int)(SelectionRect.Y * ratioY);

                cropping.Width = (int)(SelectionRect.Width * ratioX);
                cropping.Height = (int)(SelectionRect.Height * ratioY);

                Bitmap cropped = CropImage(new Bitmap(pictureBox.Image), cropping);
                Image croppedImage = cropped;

                //croppedImage.Save("Cropped.jpg");

                Bitmap croppedSmoothed = FastSmoothing(cropped, false);
                croppedSmoothed = FastSmoothing(croppedSmoothed, false);

                //croppedSmoothed.Save("Cropped_smoothed.jpg");

                

                Bitmap croppedBinarized = FastBinarization(croppedSmoothed);

                //croppedBinarized.Save("Cropped_binarized.jpg");

                


                if (currentButton == MouseButtons.Left)
                {
                    barcodeReader.Options.TryHarder = true;
                    bool bFound = true;
                    while (bFound)
                    {
                        if (DecodeQR(croppedImage, SelectionRect.X, SelectionRect.Y, false) == 0)
                        {
                            if (DecodeQR(croppedSmoothed, SelectionRect.X, SelectionRect.Y, false) == 0)
                            {
                                if (DecodeQR(croppedBinarized, SelectionRect.X, SelectionRect.Y, false) == 0)
                                    bFound = false;
                            }
                        }

                        if (bFound)
                        {
                            cropped = CropImage(new Bitmap(pictureBox.Image), cropping);
                            croppedImage = cropped;

                            croppedImage.Save("Cropped.jpg");

                            croppedSmoothed = FastSmoothing(cropped, false);
                            croppedSmoothed = FastSmoothing(croppedSmoothed, false);

                            croppedSmoothed.Save("Cropped_smoothed.jpg");

                            croppedBinarized = FastBinarization(croppedSmoothed);
                        }
                    }
                    barcodeReader.Options.TryHarder = false;
                }
                /*if (currentButton == MouseButtons.Right)
                    Tesseract(croppedImage, SelectionRect.X, SelectionRect.Y);*/
                UpdateResults();
            }
        }

       

        private Bitmap FastSmoothing(Bitmap input, bool bChangeFormat)
        {
            if (bChangeFormat)
            {
                Bitmap processedBitmap = input.Clone(new Rectangle(0, 0, input.Width, input.Height), PixelFormat.Format1bppIndexed);
                Bitmap resized = new Bitmap(processedBitmap, new Size(processedBitmap.Width / 2, processedBitmap.Height / 2));
                processedBitmap = new Bitmap(resized, new Size(resized.Width * 2, resized.Height * 2));
                processedBitmap = processedBitmap.Clone(new Rectangle(0, 0, processedBitmap.Width, processedBitmap.Height), PixelFormat.Format1bppIndexed);
                return processedBitmap;
            }
            else
            {
                Bitmap resized = new Bitmap(input, new Size(input.Width / 2, input.Height / 2));
                Bitmap processedBitmap = new Bitmap(resized, new Size(resized.Width * 2, resized.Height * 2));
                return processedBitmap;
            }
        }

        private Bitmap FastBinarization(Image input)
        {
            Bitmap inputBitmap = new Bitmap(input);
            Bitmap binarized = inputBitmap.Clone(new Rectangle(0, 0, inputBitmap.Width, inputBitmap.Height), PixelFormat.Format1bppIndexed);
            return binarized;
        }

        /*
         * private double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private void Tesseract(Image input, int x, int y)
        {
            input.Save("ori.png");

            var ocrtext = string.Empty;
            TesseractEngine engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default);
            
            engine.SetVariable("tessedit_char_whitelist", "-23456789ABCDEFGHJKLMNPQRSTUVWXYZ");
            //Pix img = PixConverter.ToPix(new Bitmap(input));
            Bitmap binary = new Bitmap(FastBinarization(input));
            binary.SetResolution(30, 30);

            Bitmap resized = new Bitmap(input, new Size(400, 200));

            resized.Save("binary.png");
            Pix img = PixConverter.ToPix(resized);
            
            for (int i = 0; i < 360; i+=360)
            {
                Pix RoatatedImage = img.Rotate((float)DegreeToRadian(i));

                //RoatatedImage.Save("Cropped" + i.ToString() + ".jpg");

                using (Page page = engine.Process(RoatatedImage))
                {
                    string s = page.GetText();
                    s = s.Replace("\n", "");
                    s = s.Replace(" ", "");
                    if (s.Length > 0)
                    {
                        string pattern = @"[A-Z0-9][A-Z0-9][A-Z0-9][-][A-Z0-9][A-Z0-9][A-Z0-9][A-Z0-9][-][A-Z0-9][A-Z0-9][A-Z0-9][-][A-Z0-9][A-Z0-9][A-Z0-9]";
                        Match m = Regex.Match(s, pattern, RegexOptions.None);
                        //if (m.Success)
                        {
                            txtContent.Text += Environment.NewLine;
                            txtContent.Text += "Tesseract result : " + s;// + " " + i.ToString();
                            if (m.Success)
                            {
                                tesseractResults.Add(s);
                            }
                        }
                    }
                }
            }                       
        }
        */

        public Bitmap CropImage(Bitmap source, Rectangle section)
        {
            //source.Save("Source.jpg");
            // An empty bitmap which will hold the cropped image
            Bitmap bmp = new Bitmap(section.Width, section.Height);

            Graphics g = Graphics.FromImage(bmp);

            // Draw the given area (section) of the source image
            // at location 0,0 on the empty bitmap (bmp)
            g.DrawImage(source, 0, 0, section, GraphicsUnit.Pixel);
            
            return bmp;
        }



        private void buttonPaste_Click(object sender, EventArgs e)
        {
            boolCurrent = false;
            txtContent.Text = "";
            Paste();
        }

        private void Paste()
        {
            pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;

            OriginalImage = Clipboard.GetImage();

            pictureBox.Image = OriginalImage;

            bool isNullOrEmpty = pictureBox == null || pictureBox.Image == null;
            if (!isNullOrEmpty)
            {
                OriginalSize = Clipboard.GetImage().Size;
            }
            else
            {
                string[] file_names = (string[])
                Clipboard.GetData(DataFormats.FileDrop);


                if (file_names != null && file_names.Length > 0 && File.Exists(file_names[0]))
                {
                    OriginalImage = Image.FromFile(file_names[0]);
                    pictureBox.Image = OriginalImage;
                    isNullOrEmpty = pictureBox == null || pictureBox.Image == null;
                    if (!isNullOrEmpty)
                    {
                        OriginalSize = OriginalImage.Size;
                    }
                }
                else
                {
                    var fileContent = string.Empty;
                    var filePath = string.Empty;

                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        //openFileDialog.InitialDirectory = "c:\\";
                        openFileDialog.Filter = "image files (*.jpg/*.png)|*.jpg;*.png";
                        
                        if (openFileDialog.ShowDialog() == DialogResult.OK)
                        {
                            filePath = openFileDialog.FileName;
                            OriginalImage = Image.FromFile(filePath);
                            pictureBox.Image = OriginalImage;
                            isNullOrEmpty = pictureBox == null || pictureBox.Image == null;
                            if (!isNullOrEmpty)
                            {
                                OriginalSize = OriginalImage.Size;
                            }
                        }
                    }
                }
            }
        }

        private int DecodeQR(Image input, int x, int y, bool bMultiples)
        {
            if (input == null)
                return 0;

            input.Save("DecodeQRInput.png");
            var timerStart = DateTime.Now.Ticks;

            int iCountNew = 0;

            Bitmap bitmap1 = new Bitmap(input);
            int a = Decode(new[] { bitmap1 }, bMultiples, new List<BarcodeFormat> { BarcodeFormat.QR_CODE }, x, y);

            newResults.Clear();
            croppedOrigin.Clear();
            if (a > 0)
            {
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
                        float ratioX = OriginalSize.Width / (float)pictureBox.Width;
                        float ratioY = OriginalSize.Height / (float)pictureBox.Height;

                        croppedOrigin.Add(new Point((int)(x * ratioX), (int)(y * ratioY)));


                        fullResults.Add(lastResults[j]);
                        newResults.Add(lastResults[j]);
                        iCountNew++;
                    }
                }
                if(iCountNew == 1)
                    sData = "Found " + iCountNew.ToString() + " more code" + Environment.NewLine;
                if (iCountNew > 1)
                    sData = "Found " + iCountNew.ToString() + " more codes" + Environment.NewLine;
                txtContent.Text = sData;
            }
            bitmap1 = new Bitmap(pictureBox.Image);
            pictureBox.Image = bitmap1;
            Refresh();

            var timerStop = DateTime.Now.Ticks;

            elapsed = elapsed.Add(new TimeSpan(timerStop - timerStart));
            txtContent.Text += "Duration : " + elapsed.ToString() + Environment.NewLine + Environment.NewLine;
            txtContent.Text += fullResults.Count.ToString() + " Codes" + Environment.NewLine + Environment.NewLine;

            
            txtContent.Select(0, 0);

            return a;
        }

        private void UpdateResults()
        {
            listView.Items.Clear();

            for (int k = 0; k < fullResults.Count; k++)
            {
                listView.Items.Add(fullResults[k].Text.Replace("-", ""));
            }
            for (int k = 0; k < tesseractResults.Count; k++)
            {
                listView.Items.Add(tesseractResults[k].Replace("-", ""));
            }
        }

        private int Decode(IEnumerable<Bitmap> bitmaps, bool tryMultipleBarcodes, IList<BarcodeFormat> possibleFormats, int x, int y)
        {
            Debug.WriteLine("Decode {0} {1}", x, y);


            resultPoints.Clear();
            lastResults.Clear();
            txtContent.Text = String.Empty;


            IList<Result> results = null;
            var previousFormats = barcodeReader.Options.PossibleFormats;
            if (possibleFormats != null)
                barcodeReader.Options.PossibleFormats = possibleFormats;

            foreach (var bitmap in bitmaps)
            {
                if (tryMultipleBarcodes)
                    results = barcodeReader.DecodeMultiple(bitmap);
                else
                {
                    var result = barcodeReader.Decode(bitmap);
                    if (result != null)
                    {
                        if (results == null)
                        {
                            results = new List<Result>();
                        }
                        results.Add(result);
                        Debug.WriteLine("Decode {0}", result.Text);
                    }
                }
            }


            barcodeReader.Options.PossibleFormats = previousFormats;

            if (results == null)
            {
                //txtContent.Text = "No barcode recognized";
            }


            if (results != null)
            {
                foreach (var result in results)
                {
                    if (result.ResultPoints.Length > 0)
                    {
                        /*
                        var offsetX = pictureBox.SizeMode == PictureBoxSizeMode.CenterImage ? (pictureBox.Width - pictureBox.Image.Width) / 2 : 0;
                        var offsetY = pictureBox.SizeMode == PictureBoxSizeMode.CenterImage ? (pictureBox.Height - pictureBox.Image.Height) / 2 : 0;
                        */
                        int offsetX = pictureBox.Width/pictureBox.Image.Width;
                        int offsetY = pictureBox.Height/pictureBox.Image.Height;

                        for (int i = 0; i < result.ResultPoints.Length; i++)
                        { 
                            for (int j = i+1; j < result.ResultPoints.Length; j++)
                            {
                                ResultPoint tmp = result.ResultPoints[i];
                                if (result.ResultPoints[j].Y < tmp.Y)
                                {
                                    result.ResultPoints[i] = result.ResultPoints[j];
                                    result.ResultPoints[j] = tmp;
                                }
                                else
                                {
                                    if (result.ResultPoints[j].X < tmp.X)
                                    {
                                        result.ResultPoints[i] = result.ResultPoints[j];
                                        result.ResultPoints[j] = tmp;
                                    }
                                }
                            }
                        }

                        var rect = new Rectangle((int)result.ResultPoints[0].X + offsetX, (int)result.ResultPoints[0].Y + offsetY, 1, 1);
                        int iPointsCounter = 0;

                        Debug.WriteLine("Image Size {0} {1}", OriginalImage.Width, OriginalImage.Height); 

                        foreach (var point in result.ResultPoints)
                        {
                            Debug.WriteLine("Drawing {0} {1} {2}", iPointsCounter, point.X, point.Y);
                            iPointsCounter++;

                            if (point.X + offsetX < rect.Left)
                                rect = new Rectangle((int)point.X + offsetX, rect.Y, rect.Width + rect.X - (int)point.X - offsetX, rect.Height);
                            if (point.X + offsetX > rect.Right)
                                rect = new Rectangle(rect.X, rect.Y, rect.Width + (int)point.X - (rect.X - offsetX), rect.Height);
                            if (point.Y + offsetY < rect.Top)
                                rect = new Rectangle(rect.X, (int)point.Y + offsetY, rect.Width, rect.Height + rect.Y - (int)point.Y - offsetY);
                            if (point.Y + offsetY > rect.Bottom)
                                rect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height + (int)point.Y - (rect.Y - offsetY));
                        }

                        Debug.WriteLine("Drawing {0} {1} {2} {3} {4} {5} {6} {7}", rect.X, rect.Y, rect.Width, rect.Height, x, y, offsetX, offsetY);

                        Graphics g;
                        g = Graphics.FromImage(OriginalImage);

                        Pen greenPen = new Pen(Color.Green, 15);
                        float fX = (float)(pictureBox.Image.Width) / (float)(pictureBox.Width);
                        float fY = (float)(pictureBox.Image.Height) / (float)(pictureBox.Height);
                        rect.X += (int)(fX * x);
                        rect.Y += (int)(fY * y);
                        g.DrawRectangle(greenPen, rect);

                        Debug.WriteLine("Drawing {0} {1} {2} {3} {4} {5} {6} {7} {8} {9}", rect.X, rect.Y, rect.Width, rect.Height, fX, fY, x, y, offsetX, offsetY);

                        /*OriginalImage.Save(iImageCount.ToString() + ".png");
                        iImageCount++;*/

                    }
                }
                pictureBox.Image = OriginalImage;
                return results.Count;
            }
            return 0;
        }

        private void buttonDecode_Click(object sender, EventArgs e)
        {
            if(pictureBox.Image == null)
                return;

            if (!boolCurrent)
            {
                boolCurrent = true;
                bCurrent = new Bitmap(pictureBox.Image);
            }

            Bitmap bBinarized = FastBinarization(bCurrent);
            Bitmap bSmoothed = FastSmoothing(bBinarized, true);

            bSmoothed.Save("smoothed.png");

            DecodeQR(bSmoothed, 0, 0, true);
            UpdateResults();

            bCurrent = bSmoothed;
        }

        private void ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            Clipboard.SetText(e.Item.Text);
            e.Item.BackColor = Color.Gray;
        }

        private void buttonHarder_Click(object sender, EventArgs e)
        {
            if (pictureBox.Image == null)
                return;

            boolCurrent = true;
            bCurrent = new Bitmap(pictureBox.Image);
            

            Bitmap bBinarized = FastBinarization(bCurrent);
            Bitmap bSmoothed = FastSmoothing(bBinarized, true);


            barcodeReader.Options.TryHarder = true;
            bool bFound = true;
            int iTry = 0;
            while (bFound || iTry < 3)
            {
                bBinarized.Save("Binarized.png");
                bSmoothed.Save("Smoothed.png");

                if (DecodeQR(bBinarized, SelectionRect.X, SelectionRect.Y, false) == 0)
                {
                    if (DecodeQR(bSmoothed, SelectionRect.X, SelectionRect.Y, false) == 0)
                    {
                        bFound = false;
                    }
                }

                if (bFound)
                {
                    bCurrent = new Bitmap(pictureBox.Image);
                    bBinarized = FastBinarization(bCurrent);
                    bSmoothed = FastSmoothing(bBinarized, true);
                    iTry = 0;
                }
                else
                {
                    iTry++;
                    bBinarized = FastBinarization(bSmoothed);
                    bSmoothed = FastSmoothing(bBinarized, true);
                }
            }
            barcodeReader.Options.TryHarder = false;
            UpdateResults();
    }
    }
}

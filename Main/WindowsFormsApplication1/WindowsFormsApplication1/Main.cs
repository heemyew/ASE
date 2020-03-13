using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Diagnostics;
using System.Data.Sql;
using System.Linq;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using WindowsFormsApplication1.App_Code;

namespace WindowsFormsApplication1
{
    public partial class Main : Form
    {
        Image<Bgr, Byte> currentFrame;
        Capture grabber;
        HaarCascade face;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result;
        Image<Gray, byte> gray = null;
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        // string name, names = null;
        public static string name = "";
        public static string names = "";
        SFaceManager sfm = new SFaceManager();


        public Main()
        {
            InitializeComponent();
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            retrain();
        }
        public void FrameGrabber(object sender, EventArgs e)
        {
            //currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            //imageBox1.Image = currentFrame;

            label3.Text = "0";

            NamePersons.Add("");


            // capture a frame form  device both face and all things on the image
            currentFrame = grabber.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);


            gray = currentFrame.Convert<Gray, Byte>();

            //(TestImageBox.Image = currentFrame);

            //Result of haarCascade will be on the "MCvAvgComp"-facedetected (is it face or not )
            MCvAvgComp[][] facesDetected = gray.DetectHaarCascade(
          face,
          1.2,
          10,
          Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
          new Size(20, 20));


            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                result = currentFrame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                currentFrame.Draw(f.rect, new Bgr(Color.Red), 2); //Frame detect colour is 'read'


                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);


                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                       trainingImages.ToArray(),
                       labels.ToArray(),
                       3000,
                       ref termCrit);

                    name = recognizer.Recognize(result); // detected name of the face is been saved  to the 'name'-variable
                    //the colour of  the face label name 
                    currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));

                }

                NamePersons[t - 1] = name;
                NamePersons.Add("");
            }
            t = 0;


            for (int nnn = 0; nnn < facesDetected[0].Length; nnn++)
            {
                names = names + NamePersons[nnn] + ", ";
            }

            imageBox1.Image = currentFrame;

        }
        public void retrain()
        {
            List<SFace> facelist = sfm.FaceList;
            trainingImages.Clear();
            labels.Clear();
            foreach (SFace sface in facelist)
            {
                Image<Gray, byte> g = convertImagetoImageGRAYBYTE(sface.Image);
                trainFace(g, sface.matriNo);
            }
        }
        ConnectionString cs = new ConnectionString();

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPage2"])
            {
                grabber = new Capture();
                grabber.QueryFrame();
                Application.Idle += new EventHandler(FrameGrabber);
            }
            else {
                Application.Idle -= new EventHandler(FrameGrabber);
                grabber.Dispose();
            }
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPage4"])
            {
                SqlConnection con = new SqlConnection(cs.DBConn);
                SqlCommand cmd = null;
                con.Open();
                string cmdString = "select * from ClassSchedule";
                cmd = new SqlCommand(cmdString);
                cmd.Connection = con;
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows) {
                    DataTable dt = new DataTable();
                    dt.Load(reader);
                    dataGridView1.DataSource = dt;

                }
                con.Close();
                dataGridView1.Columns["id"].Visible = false;

            }
        }
        public Image<Gray, Byte> convertImagetoImageGRAYBYTE(Image img) {
            Bitmap masterImage = (Bitmap)img;
            Image<Gray, Byte> normalizedMasterImage = new Image<Gray, Byte>(masterImage);
            return normalizedMasterImage;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Image img = Image.FromFile(@"C:\Users\Desmond95\Desktop\test.jpg");
            Image<Gray, Byte> normalizedMasterImage = convertImagetoImageGRAYBYTE(img);
            Image<Bgr, Byte> myImage = new Image<Bgr, Byte>((Bitmap)img); 
            MCvAvgComp[][] facesDetected = normalizedMasterImage.DetectHaarCascade(
             face,
             1.2,
             10,
             Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
             new Size(20, 20));

            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                result = normalizedMasterImage.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                       trainingImages.ToArray(),
                       labels.ToArray(),
                       3000,
                       ref termCrit);
                    name = recognizer.Recognize(result); // detected name of the face is been saved  to the 'name'-variable
                    //the colour of  the face label name 
                    myImage.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));
                    name = recognizer.Recognize(result); // detected name of the face is been saved  to the 'name'-variable
                    label2.Text = name;
                }
                

                
            }
            imageBox3.Image = myImage;

        }

        Bitmap a = null;
        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";


                if (dlg.ShowDialog() == DialogResult.OK)
                {

                    // Create a new Bitmap object from the picture file on disk,
                    // and assign that to the PictureBox.Image property
                    a = new Bitmap(dlg.FileName);
                    Image image = ResizeImage(a, 200, 200);
                    pictureBox1.Image = image;

                }
            }      

        }
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
        private void button4_Click(object sender, EventArgs e)
        {
            sfm.uploadStudentFace(a, textBox2.Text);
            retrain();
        }
        public void trainFace(Image<Gray, Byte> img,string name)
        {
            MCvAvgComp[][] facesDetected = img.DetectHaarCascade(
             face,
             1.2,
             10,
             Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
             new Size(20, 20));
            foreach (MCvAvgComp f in facesDetected[0])
            {
                t = t + 1;
                trainingImages.Add(img.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC));
                labels.Add(name);
                ContTrain++; NumLabels++;
                break;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            label6.Text = dataGridView1.CurrentRow.Cells[0].Value.ToString();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            Image<Gray, byte> ScreenCapture = grabber.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            MCvAvgComp[][] facesDetected = ScreenCapture.DetectHaarCascade(
                    face,
                    1.2,
                    10,
                    Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                    new Size(20, 20));
            foreach (MCvAvgComp f in facesDetected[0])
            {
                result = ScreenCapture.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                if (trainingImages.ToArray().Length != 0)
                {
                    MCvTermCriteria termCrit = new MCvTermCriteria(ContTrain, 0.001);
                    EigenObjectRecognizer recognizer = new EigenObjectRecognizer(
                       trainingImages.ToArray(),
                       labels.ToArray(),
                       3000,
                       ref termCrit);
                    name = recognizer.Recognize(result); // detected name of the face is been saved  to the 'name'-variable
                }
            }
            if (name !="")
                MessageBox.Show(name+" Attendance Taken", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);


        }
    }
}

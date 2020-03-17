using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApplication1.App_Code;

namespace WindowsFormsApplication1
{
    public partial class TakeAttendace : Form
    {
        SFaceManager sfm = new SFaceManager();
        Capture grabber;
        HaarCascade face;
        List<string> labels = new List<string>();
        Image<Gray, byte> result;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        int ContTrain;
        ConnectionString cs = new ConnectionString();
        string message = "";

        public TakeAttendace()
        {
            InitializeComponent();
            face = new HaarCascade("haarcascade_frontalface_default.xml");
            retrain();
            grabber = new Capture();
            grabber.QueryFrame();
            Application.Idle += new EventHandler(FrameGrabber);

        }
        public void FrameGrabber(object sender, EventArgs e)
        {
            Image<Gray, byte> gray = null;
            Image<Bgr, Byte> currentFrame;
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

                    string name = recognizer.Recognize(result); // detected name of the face is been saved  to the 'name'-variable
                    //the colour of  the face label name 
                    currentFrame.Draw(name, ref font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.LightGreen));

                }
            }

            imageBox1.Image = currentFrame;

        }
        public void retrain()
        {
            List<SFace> facelist = sfm.FaceList;
            trainingImages.Clear();
            labels.Clear();
            ContTrain = 0;
            foreach (SFace sface in facelist)
            {
                Image<Gray, byte> g = convertImagetoImageGRAYBYTE(sface.Image);
                trainFace(g, sface.matriNo);
            }
        }
        public Image<Gray, Byte> convertImagetoImageGRAYBYTE(Image img)
        {
            Bitmap masterImage = (Bitmap)img;
            Image<Gray, Byte> normalizedMasterImage = new Image<Gray, Byte>(masterImage);
            return normalizedMasterImage;
        }
        public void trainFace(Image<Gray, Byte> img, string name)
        {
            MCvAvgComp[][] facesDetected = img.DetectHaarCascade(
             face,
             1.2,
             10,
             Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
             new Size(20, 20));
            foreach (MCvAvgComp f in facesDetected[0])
            {
                trainingImages.Add(img.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC));
                labels.Add(name);
                ContTrain++;
                break;
            }
        }

        public void ShowMyDialogBox()
        {
            input testDialog = new input();
            DialogResult dr = testDialog.ShowDialog(this);

            // Show testDialog as a modal dialog and determine if DialogResult = OK.
            if (dr == DialogResult.Yes)
            {
                // Read the contents of testDialog's TextBox.
                message = testDialog.textBox1.Text;
            }
            else
            {
                message = "Cancelled";
            }
            testDialog.Dispose();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int counter = 0;
            Image image = grabber.QueryFrame().Bitmap;
            Image<Gray, byte> ScreenCapture = grabber.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            MCvAvgComp[][] facesDetected = ScreenCapture.DetectHaarCascade(
                    face,
                    1.2,
                    10,
                    Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING,
                    new Size(20, 20));
            string name="";
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
            if (name != "")
            {
                //MessageBox.Show(name + " Attendance Taken", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                SqlConnection con = new SqlConnection(cs.DBConn);
                SqlCommand cmd = null;
                con.Open();
                string cmdString = "select ClassSchedule.id from Student  inner join StudentClassEnroll  on Student.MatriCardNo = StudentClassEnroll.MatriCardNo inner join ClassSchedule on ClassSchedule.[index] = StudentClassEnroll.[index] where Status='Open'  and Student.MatriCardNo=@mc";
                cmd.Parameters.AddWithValue("@mc", message);
                cmd = new SqlCommand(cmdString);
                cmd.Connection = con;
                SqlDataReader reader = cmd.ExecuteReader();
                int classindex = -1;
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        classindex = reader.GetInt32(0);
                    }
                    reader.Close();
                    cmd = null;
                    cmdString = "insert into Attendance(ClassScheduleID,MatriCard,Photo,Status) VALUES (@ClassScheduleID,@MatriCard,@Photo,@Status)";
                    cmd = new SqlCommand(cmdString);
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@ClassScheduleID", classindex);
                    cmd.Parameters.AddWithValue("@MatriCard", name);
                    cmd.Parameters.AddWithValue("@Status", "Present");
                    SqlParameter p = new SqlParameter("@Photo", SqlDbType.Image);
                    MemoryStream ms = new MemoryStream();
                    Bitmap bmpImage = new Bitmap(image);
                    bmpImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    byte[] data = ms.GetBuffer();
                    p.Value = data;
                    cmd.Parameters.Add(p);
                    cmd.ExecuteReader();

                }
                else
                {
                    MessageBox.Show("Class has ended or class session not opened yet!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                con.Close();
            }
            else
            {
                if (counter == 3)
                {
                    counter = 0;
                    ShowMyDialogBox();
                    if (message != "" && message != "Cancelled")
                    {
                        SqlConnection con = new SqlConnection(cs.DBConn);
                        SqlCommand cmd = null;
                        con.Open();
                        string cmdString = "select ClassSchedule.id from Student  inner join StudentClassEnroll  on Student.MatriCardNo = StudentClassEnroll.MatriCardNo inner join ClassSchedule on ClassSchedule.[index] = StudentClassEnroll.[index] where Status='Open'  and Student.MatriCardNo=@mc";
                        cmd = new SqlCommand(cmdString);
                        cmd.Parameters.AddWithValue("@mc", message);
                        cmd.Connection = con;
                        SqlDataReader reader = cmd.ExecuteReader();
                        int classindex = -1;
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                classindex = reader.GetInt32(0);
                            }
                            reader.Close();
                            cmd = null;
                            cmdString = "insert into Attendance(ClassScheduleID,MatriCard,Photo,Status) VALUES (@ClassScheduleID,@MatriCard,@Photo,@Status)";
                            cmd = new SqlCommand(cmdString);
                            cmd.Connection = con;
                            cmd.Parameters.AddWithValue("@ClassScheduleID", classindex);
                            cmd.Parameters.AddWithValue("@MatriCard", name);
                            cmd.Parameters.AddWithValue("@Status", "Present");
                            SqlParameter p = new SqlParameter("@Photo", SqlDbType.Image);
                            MemoryStream ms = new MemoryStream();
                            Bitmap bmpImage = new Bitmap(image);
                            bmpImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                            byte[] data = ms.GetBuffer();
                            p.Value = data;
                            cmd.Parameters.Add(p);
                            cmd.ExecuteReader();

                        }
                        else
                        {
                            MessageBox.Show("Class has ended or class session not opened yet!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                        con.Close();

                    }
                }
                else
                {
                    MessageBox.Show("Unable to detect your face, Please try again!", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    counter++;
                }

            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            DateTime date1 = dateTimePicker1.Value;
            DateTime date2 = dateTimePicker2.Value;
            DateTime datenow = DateTime.Now.AddDays(7);

            /* if (result < 0)
                 label14.Text = "is earlier than";
             else if (result == 0)
                 label14.Text = "is the same time as";
             else
                 label14.Text = "is later than";*/
            //label14.Text = datenow.ToShortDateString();
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            int result = DateTime.Compare(date1, date2);
            if (result < 0)
            {
                if (radioButton1.Checked == true)
                {
                    //label14.Text = "medical";
                    string cmdString = "insert into LOA(MatricNo,StartDate,EndDate,Reason,ExtraReason,Status) VALUES (@MatricNo,@StartDate,@EndDate,@Reason,@ExtraReason,@Status)";
                    cmd = new SqlCommand(cmdString);
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@MatricNo", textBox3.Text);
                    cmd.Parameters.AddWithValue("@StartDate", date1.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@EndDate", date2.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@Reason", "Medical");
                    cmd.Parameters.AddWithValue("@ExtraReason", textBox1.Text);
                    cmd.Parameters.AddWithValue("@Status", "Pending");
                    cmd.ExecuteNonQuery();
                    con.Close();
                    MessageBox.Show("Application submitted, must submit the MC within 7 working days");
                }
                else if (radioButton2.Checked == true)
                {
                    //label14.Text = "compassionate";
                    string cmdString = "insert into LOA(MatricNo,StartDate,EndDate,Reason,ExtraReason,Status) VALUES (@MatricNo,@StartDate,@EndDate,@Reason,@ExtraReason,@Status)";
                    cmd = new SqlCommand(cmdString);
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@MatricNo", textBox3.Text);
                    cmd.Parameters.AddWithValue("@StartDate", date1.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@EndDate", date2.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@Reason", "compassionate");
                    cmd.Parameters.AddWithValue("@ExtraReason", textBox1.Text);
                    cmd.Parameters.AddWithValue("@Status", "Pending");
                    cmd.ExecuteNonQuery();
                    con.Close();
                    MessageBox.Show("LOA will be granted within 7 days of death,inclusive of weekend and public holiday");
                }
                else if (radioButton3.Checked == true)
                {
                    label14.Text = "others";
                    string cmdString = "insert into LOA(MatricNo,StartDate,EndDate,Reason,ExtraReason,Status) VALUES (@MatricNo,@StartDate,@EndDate,@Reason,@ExtraReason,@Status)";
                    cmd = new SqlCommand(cmdString);
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@MatricNo", textBox3.Text);
                    cmd.Parameters.AddWithValue("@StartDate", date1.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@EndDate", date2.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@Reason", "others");
                    cmd.Parameters.AddWithValue("@ExtraReason", textBox1.Text);
                    cmd.Parameters.AddWithValue("@Status", "Pending");
                    cmd.ExecuteNonQuery();
                    con.Close();
                    MessageBox.Show("Application sent. To be considered,application must be submitted with the supporting documents at least 7 working days in advance");
                }

            }
            else if (result == 0)
            {
                MessageBox.Show("Cannot apply for LOA on the date itself or earlier");
            }
            else
            {
                MessageBox.Show("Cannot apply for LOA on the date itself or earlier");
            }
        }

        private void lblName_Click(object sender, EventArgs e)
        {
            lblName.Text = "Welcome" + Login.personName;
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            login.Show();
        }
    }
}

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
            //Take attendance
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPage2"])
            {
                grabber = new Capture();
                grabber.QueryFrame();
                Application.Idle += new EventHandler(FrameGrabber);
            }
            else {
                Application.Idle -= new EventHandler(FrameGrabber);
                if(grabber !=null)
                    grabber.Dispose();
            }
            //open session
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPage4"])
            {
                SqlConnection con = new SqlConnection(cs.DBConn);
                SqlCommand cmd = null;
                con.Open();
                string cmdString = "select * from ClassSchedule where Status='Closed' and day(startDate)=day(GETDATE()) and  month(startDate)=month(GETDATE()) and year(startDate)=year(GETDATE())";
                cmd = new SqlCommand(cmdString);
                cmd.Connection = con;
                SqlDataReader reader = cmd.ExecuteReader();
                DataTable dt = null;
                if (reader.HasRows)
                {
                    dt = new DataTable();
                    dt.Load(reader);
                    dataGridView1.DataSource = dt;
                    dataGridView1.Columns["id"].Visible = false;
                }
                else
                {
                    dataGridView1.DataSource = dt;
                }
                
                reader.Close();

                cmd = null;
                cmdString = "select * from ClassSchedule where Status='Open' ";
                cmd = new SqlCommand(cmdString);
                cmd.Connection = con;
                SqlDataReader reader1 = cmd.ExecuteReader();
                DataTable dt2 = null;
                if (reader1.HasRows)
                {
                    dt2 = new DataTable();
                    dt2.Load(reader1);
                    dataGridView2.DataSource = dt2;
                    dataGridView2.Columns["id"].Visible = false;

                }
                else
                {
                    dataGridView2.DataSource = dt2;
                }
                con.Close();
                

            }
            //view attendance tab
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPage5"])
            {
                //load attendance 
                SqlConnection con = new SqlConnection(cs.DBConn);
                SqlCommand cmd = null;
                con.Open();
                string cmdString = "select isnull(Attendance.Id,-1)as id,ClassSchedule.[index],ClassSchedule.startDate,Student.MatriCardNo,Student.Name,ISNULL(Attendance.[Status],'Absence') as 'Status',isnull(Attendance.Photo,'Absence') as Photo from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[index]=StudentClassEnroll.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo order by ClassSchedule.[index],startDate,student.MatriCardNo";
                cmd = new SqlCommand(cmdString);
                cmd.Connection = con;
                SqlDataReader reader = cmd.ExecuteReader();
                DataTable dt = null;
                if (reader.HasRows)
                {
                    dt = new DataTable();
                    dt.Load(reader);
                    dataGridView3.DataSource = dt;
                    dataGridView3.Columns["id"].Visible = false;
                    dataGridView3.Columns["Photo"].Visible = false;
                    DataGridViewButtonColumn btn = new DataGridViewButtonColumn();
                    dataGridView3.Columns.Add(btn);
                    btn.HeaderText = "Photo";
                    btn.Text = "View Photo";
                    btn.Name = "btn";
                    btn.UseColumnTextForButtonValue = true;

                }
                else
                {
                    dataGridView3.DataSource = dt;
                }
                con.Close();
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
            int index = (int)dataGridView1.CurrentRow.Cells[0].Value;
            DateTime startDate = (DateTime)dataGridView1.CurrentRow.Cells[2].Value;
            DateTime endDate = (DateTime)dataGridView1.CurrentRow.Cells[3].Value;
            DateTime currentTime = DateTime.Now;
            //DateTime.Now.ToString(@"MM\/dd\/yyyy h\:mm tt");

            DateTime span = startDate.AddMinutes(-30);
            if (currentTime >= span && currentTime <= endDate)
            {
                SqlConnection con = new SqlConnection(cs.DBConn);
                SqlCommand cmd = null;
                con.Open();
                string cmdString = "update ClassSchedule set Status='Open' where id = @id";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@id", index);
                cmd.Connection = con;
                SqlDataReader reader = cmd.ExecuteReader();
                reader.Close();

                SqlCommand cmd1 = new SqlCommand();
                string cmdString1 = "select * from ClassSchedule where Status='Closed'  and day(startDate)=day(GETDATE()) and  month(startDate)=month(GETDATE()) and year(startDate)=year(GETDATE())";
                cmd1 = new SqlCommand(cmdString1);
                cmd1.Connection = con;
                SqlDataReader reader1 = cmd1.ExecuteReader();
                DataTable dt = null;
                if (reader1.HasRows)
                {
                    dt = new DataTable();
                    dt.Load(reader1);
                    dataGridView1.DataSource = dt;
                    dataGridView1.Columns["id"].Visible = false;
                }
                else
                {
                    dataGridView1.DataSource = dt;
                }
                reader1.Close();

                cmd1 = null;
                cmdString1 = "select * from ClassSchedule where Status='Open'";
                cmd1 = new SqlCommand(cmdString1);
                cmd1.Connection = con;
                SqlDataReader reader2 = cmd1.ExecuteReader();
                DataTable dt2 = null;
                if (reader2.HasRows)
                {
                    dt2 = new DataTable();
                    dt2.Load(reader2);
                    dataGridView2.DataSource = dt2;
                    dataGridView2.Columns["id"].Visible = false;
                }
                else
                {
                    dataGridView2.DataSource = dt2;
                }
                con.Close();
                
            }
            else {
                if (currentTime < span)
                    MessageBox.Show("Too early to open class session", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else {
                    MessageBox.Show("Class has ended", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            //Console.WriteLine( "Time Difference (minutes): " + span.TotalMinutes );


            
        }
        int counter = 0;
        private void button2_Click_1(object sender, EventArgs e)
        {
            
            Image image = grabber.QueryFrame().Bitmap;
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
            else {
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
        string message = "";
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
        Image pictureToDisplay = null;
        private void dataGridView3_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 7)
            {
                //get ID of the select row
                int id = (int)dataGridView3.CurrentRow.Cells[0].Value;
                if (id == -1)
                    return;
                //display the picture
                SqlConnection con = new SqlConnection(cs.DBConn);
                SqlCommand cmd = null;
                con.Open();
                string cmdString = "select Photo from Attendance where id=@id";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Connection = con;
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows) {
                    while (reader.Read()) {
                        byte[] photo = (byte[])reader[0];
                        pictureToDisplay = byteArrayToImage(photo);
                        Image image = ResizeImage(pictureToDisplay, 333, 249);
                        picturebox frm2 = new picturebox();
                        frm2.pictureBox1.Image = image;
                        frm2.Show(); 
                    }
                }

            }
        }
        public Image byteArrayToImage(byte[] bytesArr)
        {
            using (MemoryStream memstr = new MemoryStream(bytesArr))
            {
                Image img = Image.FromStream(memstr);
                return img;
            }
        }
    }
    
}

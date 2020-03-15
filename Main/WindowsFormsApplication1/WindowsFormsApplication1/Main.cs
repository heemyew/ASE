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
        Capture grabber;
        MCvFont font = new MCvFont(FONT.CV_FONT_HERSHEY_TRIPLEX, 0.5d, 0.5d);
        Image<Gray, byte> result;
        
        List<Image<Gray, byte>> trainingImages = new List<Image<Gray, byte>>();
        List<string> labels = new List<string>();
        List<string> NamePersons = new List<string>();
        int ContTrain, NumLabels, t;
        // string name, names = null;
        public static string name = "";
        public static string names = "";
        SFaceManager sfm = new SFaceManager();
        ConnectionString cs = new ConnectionString();


        public Main()
        {
            InitializeComponent();
        }
        

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
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
                string cmdString = "select isnull(Attendance.Id,-1)as id,ClassSchedule.Id as ClassID,ClassSchedule.[CourseCode] as 'CourseCode',convert(varchar,ClassSchedule.startDate) as Date ,Student.MatriCardNo as 'Matriculation No',Student.Name,ISNULL(Attendance.[Status],'Absence') as 'Status',Attendance.Photo as Photo from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[CourseCode]=StudentClassEnroll.[CourseCode] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo order by ClassSchedule.[CourseCode],startDate,student.MatriCardNo";
                cmd = new SqlCommand(cmdString);
                cmd.Connection = con;
                SqlDataReader reader = cmd.ExecuteReader();
                DataTable dt = null;
                if (reader.HasRows)
                {
                    dataGridView3.Columns.Clear();
                    dt = new DataTable();
                    dt.Load(reader);
                    dt.Columns[3].ReadOnly = false;
                    dt.Columns[0].ReadOnly = false;
                    dataGridView3.DataSource = dt;
                    dataGridView3.Columns["id"].Visible = false;
                    dataGridView3.Columns["ClassID"].Visible = false;
                    dataGridView3.Columns["Photo"].Visible = false;

                    DataGridViewTextBoxColumn btn = new DataGridViewTextBoxColumn();
                    dataGridView3.Columns.Add(btn);
                    btn.HeaderText = "Photo";
                    btn.Name = "btn";
                    format();

            
                }
                else
                {
                    dataGridView3.DataSource = dt;
                }
                con.Close();
            }
        }
        public void format()
        {
            DataGridViewColumn column = dataGridView3.Columns[3];
            column.Width = 230;
            DataGridViewColumn columns = dataGridView3.Columns[2];
            columns.Width = 70;
            
            foreach (DataGridViewColumn a in dataGridView3.Columns)
            {
                a.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            foreach (DataGridViewColumn a in dataGridView3.Columns)
            {
                a.ReadOnly = true;
            }
            foreach (DataGridViewRow row in dataGridView3.Rows)
            {
                string date = row.Cells["Date"].Value.ToString();
                DateTime datet = DateTime.Parse(date);
                row.Cells[3].Value = datet.ToString("dddd, dd MMMM yyyy HH:mm tt");

              

                if (((int)row.Cells[0].Value) != -1)
                {
                    string a = row.Cells[7].Value.ToString().Trim();
                    if (row.Cells[7].Value.ToString().Trim() != "") {
                        DataGridViewButtonCell btn = new DataGridViewButtonCell();
                        btn.Value = "View Photo";
                        row.Cells[8] = btn;

                    }

                }
                if (((int)row.Cells[0].Value) != -1   && row.Cells["Status"].Value.ToString().Trim() == "Absence")
                {
                    row.Cells[0].Value = -1;
                }

            }

        }

        public Image<Gray, Byte> convertImagetoImageGRAYBYTE(Image img)
        {
            Bitmap masterImage = (Bitmap)img;
            Image<Gray, Byte> normalizedMasterImage = new Image<Gray, Byte>(masterImage);
            return normalizedMasterImage;
        }
      

        Bitmap a = null;
        private void button3_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Choose Image";


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
            //retrain();
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
            else
            {
                if (currentTime < span)
                    MessageBox.Show("Too early to open class session", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                else
                {
                    MessageBox.Show("Class has ended", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            //Console.WriteLine( "Time Difference (minutes): " + span.TotalMinutes );



        }
        int counter = 0;
        private void button2_Click_1(object sender, EventArgs e)
        {

            
        }
        
        Image pictureToDisplay = null;

        private void imageBox1_Click(object sender, EventArgs e)
        {

        }
        public void rebindFilter(string studentMatrNo, string CourseCode) {

            //load attendance 
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            string cmdString = "";

            if (studentMatrNo == "" && CourseCode == "")
            {
                cmdString = "select isnull(Attendance.Id,-1)as id,ClassSchedule.Id as ClassID,ClassSchedule.[CourseCode] as 'CourseCode',convert(varchar,ClassSchedule.startDate) as Date ,Student.MatriCardNo as 'Matriculation No',Student.Name,ISNULL(Attendance.[Status],'Absence') as 'Status',Attendance.Photo as Photo from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[index]=StudentClassEnroll.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo order by ClassSchedule.[CourseCode],startDate,student.MatriCardNo";
                cmd = new SqlCommand(cmdString);
            }
            else if (studentMatrNo != "" && CourseCode == "")
            {
                cmdString = "select isnull(Attendance.Id,-1)as id,ClassSchedule.Id as ClassID,ClassSchedule.[CourseCode] as 'CourseCode',convert(varchar,ClassSchedule.startDate) as Date ,Student.MatriCardNo as 'Matriculation No',Student.Name,ISNULL(Attendance.[Status],'Absence') as 'Status',Attendance.Photo as Photo from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[CourseCode]=StudentClassEnroll.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo where Student.MatriCardNo = @studentMC order by ClassSchedule.[CourseCode],startDate,student.MatriCardNo";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@studentMC", studentMatrNo);
            }
            else if (studentMatrNo == "" && CourseCode != "")
            {
                cmdString = "select isnull(Attendance.Id,-1)as id,ClassSchedule.Id as ClassID,ClassSchedule.[CourseCode] as 'CourseCode',convert(varchar,ClassSchedule.startDate) as Date ,Student.MatriCardNo  as 'Matriculation No',Student.Name,ISNULL(Attendance.[Status],'Absence') as 'Status',Attendance.Photo as Photo from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[index]=StudentClassEnroll.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo where ClassSchedule.[CourseCode] = @coursecode order by ClassSchedule.[CourseCode],startDate,student.MatriCardNo";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@coursecode", CourseCode);
            }
            else if (studentMatrNo != "" && CourseCode != "")
            {
                cmdString = "select isnull(Attendance.Id,-1)as id,ClassSchedule.Id as ClassID,ClassSchedule.[CourseCode] as 'CourseCode',convert(varchar,ClassSchedule.startDate) as Date ,Student.MatriCardNo as 'Matriculation No',Student.Name,ISNULL(Attendance.[Status],'Absence') as 'Status',Attendance.Photo as Photo from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[index]=StudentClassEnroll.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo where Student.MatriCardNo = @studentMC and ClassSchedule.[CourseCode] = @coursecode order by ClassSchedule.[CourseCode],startDate,student.MatriCardNo";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@studentMC", studentMatrNo);
                cmd.Parameters.AddWithValue("@coursecode", CourseCode);
            }
            cmd.Connection = con;
            SqlDataReader reader = cmd.ExecuteReader();
            DataTable dt = null;
            if (reader.HasRows)
            {
                dataGridView3.Columns.Clear();
                dt = new DataTable();
                dt.Load(reader);
                dt.Columns[0].ReadOnly = false;

                dt.Columns[3].ReadOnly = false;
                dataGridView3.DataSource = dt;
                dataGridView3.Columns["id"].Visible = false;
                dataGridView3.Columns["ClassID"].Visible = false;
                dataGridView3.Columns["Photo"].Visible = false;

                DataGridViewTextBoxColumn btn = new DataGridViewTextBoxColumn();
                dataGridView3.Columns.Add(btn);
                btn.HeaderText = "Photo";
                btn.Name = "btn";
                format();
            }
            else
            {
                dataGridView3.DataSource = dt;
            }
            con.Close();
        }
        private void txtStudentName_TextChanged(object sender, EventArgs e)
        {
            string studentMatrNo = txtStudentName.Text;
            string CourseCode = txtCourse.Text;
            rebindFilter(studentMatrNo, CourseCode);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            string studentMatrNo = txtStudentName.Text;
            string CourseCode = txtCourse.Text;
            rebindFilter(studentMatrNo, CourseCode);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int index = (int)dataGridView3.CurrentRow.Cells["ClassID"].Value;
            int id = (int)dataGridView3.CurrentRow.Cells["id"].Value;
            string matri = (string)dataGridView3.CurrentRow.Cells["Matriculation No"].Value;
            if (id!=-1) return;
            
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            string cmdString = "select * from attendance where ClassScheduleID=@dd1 and MatriCard=@dd2";
            cmd = new SqlCommand(cmdString);
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@dd1", index);
            cmd.Parameters.AddWithValue("@dd2", matri);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Close();
                cmdString = "update Attendance set status ='Present' where ClassScheduleID=@dd1 and MatriCard=@dd2";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@dd1", index);
                cmd.Parameters.AddWithValue("@dd2", matri);
                cmd.Connection = con;
                cmd.ExecuteNonQuery();
                rebindFilter(txtStudentName.Text, txtCourse.Text);
            }
            else {
                reader.Close();
                cmdString = "insert into Attendance(ClassScheduleID,MatriCard,[Status]) values (@d1,@d2,@d3)";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@d1", index);
                cmd.Parameters.AddWithValue("@d2", matri);
                cmd.Parameters.AddWithValue("@d3", "Present");
                cmd.Connection = con;
                cmd.ExecuteNonQuery();
                rebindFilter(txtStudentName.Text, txtCourse.Text);
            }


            

        }

        private void button2_Click(object sender, EventArgs e)
        {
            int index = (int)dataGridView3.CurrentRow.Cells["ClassID"].Value;
            int id = (int)dataGridView3.CurrentRow.Cells["id"].Value;

            if (id == -1) return;
            string matri = (string)dataGridView3.CurrentRow.Cells["Matriculation No"].Value;
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            string cmdString = "update Attendance set Status=@d3 where id=@d1";
            cmd = new SqlCommand(cmdString);
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@d3", "Absence");
            cmd.Parameters.AddWithValue("@d1", id);

            cmd.ExecuteNonQuery();
            rebindFilter(txtStudentName.Text, txtCourse.Text);
        }

        private void button7_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click_1(object sender, EventArgs e)
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

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }
        
        private void button8_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            //string cmdString = "SELECT * FROM LOA";
            //cmd = new SqlCommand(cmdString);
            // cmd.Connection = con;
            //SqlDataReader reader = cmd.ExecuteReader();
            //DataTable dt = null;
            dataGridView4.DataSource = null;
            dataGridView4.Rows.Clear();
            dataGridView4.Columns.Clear();
            dataGridView4.Refresh();
            if (radioButton4.Checked ==true)
            {
                SqlDataAdapter sqlda = new SqlDataAdapter("SELECT * FROM LOA WHERE Status=@pending", con);
                sqlda.SelectCommand.Parameters.AddWithValue("@pending", "Pending");
                DataTable dtb4 = new DataTable();
                sqlda.Fill(dtb4);
                dataGridView4.DataSource = dtb4;
                //cbStatus.ValueMember = "StatusID";
                //cbStatus.DisplayMember = "Status";
                //cbStatus.D
                //cbStatus.DataSource = dtb4;
                DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn();
                buttonColumn.HeaderText = "";
                buttonColumn.Width = 60;
                buttonColumn.Name = "buttonColumn";
                buttonColumn.Text = "Approve";
                buttonColumn.UseColumnTextForButtonValue = true;
                dataGridView4.Columns.Insert(7, buttonColumn);
            }
            else if (radioButton5.Checked == true)
            {
                
                SqlDataAdapter sqlda = new SqlDataAdapter("SELECT * FROM LOA WHERE Status=@approved", con);
                sqlda.SelectCommand.Parameters.AddWithValue("@approved", "Approved");
                DataTable dtb4 = new DataTable();
                sqlda.Fill(dtb4);
                dataGridView4.DataSource = dtb4;
            }


        }

        private void button9_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView4_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 7)
            {
                DataGridViewRow row = dataGridView4.Rows[e.RowIndex];
                if (MessageBox.Show(string.Format("Do you want to update ID: {0}?", row.Cells[1].Value), "Confirmation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    using (SqlConnection con = new SqlConnection(cs.DBConn))
                    {
                        using (SqlCommand cmd = new SqlCommand("UPDATE LOA SET Status= @approved WHERE Id = @ID", con))
                        {
                            cmd.Parameters.AddWithValue("@approved", "Approved");
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@ID", row.Cells[0].Value);
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }
                    }
                    dataGridView4.Refresh();
                    //this.BindGrid();
                }
            }
        }
        private void dataGridView3_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            if (e.ColumnIndex == 8)
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
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
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

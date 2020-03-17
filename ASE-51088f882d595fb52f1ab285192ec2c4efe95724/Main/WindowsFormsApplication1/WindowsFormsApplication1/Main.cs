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

        readonly System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

        private void StartMethod(object sender, EventArgs e)
        {
            maintain();
        }

        public Main()
        {
            InitializeComponent();
            maintain();
            myTimer.Interval = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;
            myTimer.Tick += StartMethod;
            myTimer.Enabled = true;

        }
        public void maintain()
        {
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            string cmdString = "update ClassSchedule set Status='Ended' where endDate < GETDATE()";
            cmd = new SqlCommand(cmdString);
            cmd.Connection = con;
            cmd.ExecuteNonQuery();
        }
        

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //open session
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPage4"])
            {
                SqlConnection con = new SqlConnection(cs.DBConn);
                SqlCommand cmd = null;
                con.Open();
                string cmdString = "select ClassSchedule.Id, ClassSchedule.CourseCode,ClassSchedule.[index], startDate as 'Start Date', endDate as 'End Date', Status from ClassSchedule inner join Responsibility on Responsibility.[index] =ClassSchedule.[index] where status='Closed' and day(startDate)=day(GETDATE()) and  month(startDate)=month(GETDATE()) and year(startDate)=year(GETDATE()) and staffId = @staffid order by ClassSchedule.CourseCode,ClassSchedule.[index], startDate ";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("staffid", LoginInfo.StaffID);
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
                cmdString = "select ClassSchedule.Id, ClassSchedule.CourseCode,ClassSchedule.[index], startDate as 'Start Date', endDate as 'End Date', Status from ClassSchedule inner join Responsibility on Responsibility.[index] =ClassSchedule.[index] where status='Open' and day(startDate)=day(GETDATE()) and  month(startDate)=month(GETDATE()) and year(startDate)=year(GETDATE()) and staffId = @staffid order by ClassSchedule.CourseCode,ClassSchedule.[index], startDate  ";
                cmd = new SqlCommand(cmdString);
                cmd.Connection = con;
                cmd.Parameters.AddWithValue("staffid", LoginInfo.StaffID);
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
                string cmdString = "select isnull(Attendance.Id,-1)as id,ClassSchedule.Id as ClassID,ClassSchedule.[CourseCode] as 'CourseCode',convert(varchar,ClassSchedule.startDate) as Date ,Student.MatriCardNo as 'Matriculation No',Student.Name,ISNULL(Attendance.[Status],'Absence') as 'Status',Attendance.Photo as Photo,Remarks from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[CourseCode]=StudentClassEnroll.[CourseCode] and ClassSchedule.[index]=StudentClassEnroll.[index] inner join Responsibility on Responsibility.[index] = ClassSchedule.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo where staffId=@staffid   order by ClassSchedule.[CourseCode],startDate,student.MatriCardNo";
                cmd = new SqlCommand(cmdString);
                cmd.Connection = con;
                cmd.Parameters.AddWithValue("@staffid", LoginInfo.StaffID);
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
                    dataGridView3.Columns["Remarks"].Visible = false;

                    DataGridViewTextBoxColumn btn = new DataGridViewTextBoxColumn();
                    dataGridView3.Columns.Add(btn);
                    btn.HeaderText = "Photo";
                    btn.Name = "btn";

                    DataGridViewTextBoxColumn btn2 = new DataGridViewTextBoxColumn();
                    dataGridView3.Columns.Add(btn2);
                    btn2.HeaderText = "Remark";
                    btn2.Name = "btn2";

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
            DataGridViewColumn column = dataGridView3.Columns["Date"];
            column.Width = 230;
            DataGridViewColumn columns = dataGridView3.Columns["CourseCode"];
            columns.Width = 90;

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
                row.Cells["Date"].Value = datet.ToString("dddd, dd MMMM yyyy HH:mm tt");

                if (((int)row.Cells[0].Value) != -1)
                {
                    string a = row.Cells["Photo"].Value.ToString().Trim();
                    if (row.Cells["Photo"].Value.ToString().Trim() != "")
                    {
                        DataGridViewButtonCell btn = new DataGridViewButtonCell();
                        btn.Value = "View Photo";
                        row.Cells[9] = btn;
                    }
                }
                if (row.Cells["Remarks"].Value.ToString().Trim() != "")
                {
                    DataGridViewButtonCell btn = new DataGridViewButtonCell();
                    btn.Value = "View Remarks";
                    row.Cells[10] = btn;

                }
                if (((int)row.Cells[0].Value) != -1 && row.Cells["Status"].Value.ToString().Trim() == "Absence")
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
            int index = (int)dataGridView1.CurrentRow.Cells["id"].Value;
            DateTime startDate = (DateTime)dataGridView1.CurrentRow.Cells["Start Date"].Value;
            DateTime endDate = (DateTime)dataGridView1.CurrentRow.Cells["End Date"].Value;
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
                string cmdString1 = "select ClassSchedule.Id, ClassSchedule.CourseCode,ClassSchedule.[index], startDate as 'Start Date', endDate as 'End Date', Status from ClassSchedule inner join Responsibility on Responsibility.[index] =ClassSchedule.[index] where status='Closed' and day(startDate)=day(GETDATE()) and  month(startDate)=month(GETDATE()) and year(startDate)=year(GETDATE()) and staffId = @staffid order by ClassSchedule.CourseCode,ClassSchedule.[index], startDate ";
                cmd1 = new SqlCommand(cmdString1);
                cmd1.Connection = con;
                cmd1.Parameters.AddWithValue("staffid", LoginInfo.StaffID);
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
                cmdString1 = "select ClassSchedule.Id, ClassSchedule.CourseCode,ClassSchedule.[index], startDate as 'Start Date', endDate as 'End Date', Status from ClassSchedule inner join Responsibility on Responsibility.[index] =ClassSchedule.[index] where status='Open' and day(startDate)=day(GETDATE()) and  month(startDate)=month(GETDATE()) and year(startDate)=year(GETDATE()) and staffId = @staffid order by ClassSchedule.CourseCode,ClassSchedule.[index], startDate ";
                cmd1 = new SqlCommand(cmdString1);
                cmd1.Parameters.AddWithValue("staffid", LoginInfo.StaffID);
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
        public void rebindFilter(string studentMatrNo, string CourseCode)
        {

            //load attendance 
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            string cmdString = "";

            if (studentMatrNo == "" && CourseCode == "")
            {
                cmdString = "select isnull(Attendance.Id,-1)as id,ClassSchedule.Id as ClassID,ClassSchedule.[CourseCode] as 'CourseCode',convert(varchar,ClassSchedule.startDate) as Date ,Student.MatriCardNo as 'Matriculation No',Student.Name,ISNULL(Attendance.[Status],'Absence') as 'Status',Attendance.Photo as Photo,Remarks from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[CourseCode]=StudentClassEnroll.[CourseCode] and ClassSchedule.[index]=StudentClassEnroll.[index] inner join Responsibility on Responsibility.[index] = ClassSchedule.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo where  staffId=@staffid  order by ClassSchedule.[CourseCode],startDate,student.MatriCardNo";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@staffid", LoginInfo.StaffID);

            }
            else if (studentMatrNo != "" && CourseCode == "")
            {
                cmdString = "select isnull(Attendance.Id,-1)as id,ClassSchedule.Id as ClassID,ClassSchedule.[CourseCode] as 'CourseCode',convert(varchar,ClassSchedule.startDate) as Date ,Student.MatriCardNo as 'Matriculation No',Student.Name,ISNULL(Attendance.[Status],'Absence') as 'Status',Attendance.Photo as Photo ,Remarks from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[CourseCode]=StudentClassEnroll.[CourseCode] and ClassSchedule.[index]=StudentClassEnroll.[index] inner join Responsibility on Responsibility.[index] = ClassSchedule.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo where staffId=@staffid and Student.MatriCardNo like @studentMC order by ClassSchedule.[CourseCode],startDate,student.MatriCardNo";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@studentMC", "%" + studentMatrNo + "%");
                cmd.Parameters.AddWithValue("@staffid", LoginInfo.StaffID);

            }
            else if (studentMatrNo == "" && CourseCode != "")
            {
                cmdString = "select isnull(Attendance.Id,-1)as id,ClassSchedule.Id as ClassID,ClassSchedule.[CourseCode] as 'CourseCode',convert(varchar,ClassSchedule.startDate) as Date ,Student.MatriCardNo  as 'Matriculation No',Student.Name,ISNULL(Attendance.[Status],'Absence') as 'Status',Attendance.Photo as Photo,Remarks from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[CourseCode]=StudentClassEnroll.[CourseCode] and ClassSchedule.[index]=StudentClassEnroll.[index] inner join Responsibility on Responsibility.[index] = ClassSchedule.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo where staffId=@staffid and  ClassSchedule.[CourseCode] like @coursecode order by ClassSchedule.[CourseCode],startDate,student.MatriCardNo";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@coursecode", "%" + CourseCode + "%");
                cmd.Parameters.AddWithValue("@staffid", LoginInfo.StaffID);

            }
            else if (studentMatrNo != "" && CourseCode != "")
            {
                cmdString = "select isnull(Attendance.Id,-1)as id,ClassSchedule.Id as ClassID,ClassSchedule.[CourseCode] as 'CourseCode',convert(varchar,ClassSchedule.startDate) as Date ,Student.MatriCardNo as 'Matriculation No',Student.Name,ISNULL(Attendance.[Status],'Absence') as 'Status',Attendance.Photo as Photo,Remarks from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[CourseCode]=StudentClassEnroll.[CourseCode] and ClassSchedule.[index]=StudentClassEnroll.[index] inner join Responsibility on Responsibility.[index] = ClassSchedule.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo where staffId=@staffid and  Student.MatriCardNo like @studentMC and ClassSchedule.[CourseCode] like @coursecode order by ClassSchedule.[CourseCode],startDate,student.MatriCardNo";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@studentMC", "%" + studentMatrNo + "%");
                cmd.Parameters.AddWithValue("@coursecode", "%" + CourseCode + "%");
                cmd.Parameters.AddWithValue("@staffid", LoginInfo.StaffID);

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
                dataGridView3.Columns["Remarks"].Visible = false;

                DataGridViewTextBoxColumn btn = new DataGridViewTextBoxColumn();
                dataGridView3.Columns.Add(btn);
                btn.HeaderText = "Photo";
                btn.Name = "btn";

                DataGridViewTextBoxColumn btn2 = new DataGridViewTextBoxColumn();
                dataGridView3.Columns.Add(btn2);
                btn2.HeaderText = "Remark";
                btn2.Name = "btn2";
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
        string reason = "";
        public void ShowMyDialogBox()
        {
            reason testDialog = new reason();
            DialogResult dr = testDialog.ShowDialog(this);

            // Show testDialog as a modal dialog and determine if DialogResult = OK.
            if (dr == DialogResult.Yes)
            {
                // Read the contents of testDialog's TextBox.
                reason = testDialog.txtreason.Text;
            }
            else
            {
                reason = "Cancelled";
            }
            testDialog.Dispose();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            int index = (int)dataGridView3.CurrentRow.Cells["ClassID"].Value;
            int id = (int)dataGridView3.CurrentRow.Cells["id"].Value;
            string matri = (string)dataGridView3.CurrentRow.Cells["Matriculation No"].Value;
            if (id != -1) return;

            ShowMyDialogBox();
            if (reason == "Cancelled")
            {
                return;
            }
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
                cmdString = "update Attendance set status ='Present', Remarks=@reason where ClassScheduleID=@dd1 and MatriCard=@dd2";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@dd1", index);
                cmd.Parameters.AddWithValue("@dd2", matri);
                cmd.Parameters.AddWithValue("@reason", reason);
                cmd.Connection = con;
                cmd.ExecuteNonQuery();
                rebindFilter(txtStudentName.Text, txtCourse.Text);
            }
            else
            {
                reader.Close();
                cmdString = "insert into Attendance(ClassScheduleID,MatriCard,[Status],Remarks) values (@d1,@d2,@d3,@reason)";
                cmd = new SqlCommand(cmdString);
                cmd.Parameters.AddWithValue("@d1", index);
                cmd.Parameters.AddWithValue("@d2", matri);
                cmd.Parameters.AddWithValue("@d3", "Present");
                cmd.Parameters.AddWithValue("@reason", reason);

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


            ShowMyDialogBox();
            if (reason == "Cancelled")
            {
                return;
            }
            string matri = (string)dataGridView3.CurrentRow.Cells["Matriculation No"].Value;
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            string cmdString = "update Attendance set Status=@d3, Remarks=@d4 where id=@d1";
            cmd = new SqlCommand(cmdString);
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@d3", "Absence");
            cmd.Parameters.AddWithValue("@d1", id);
            cmd.Parameters.AddWithValue("@d4", reason);

            cmd.ExecuteNonQuery();
            rebindFilter(txtStudentName.Text, txtCourse.Text);
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

        private void lblName_Click(object sender, EventArgs e)
        {
            lblName.Text = "Welcome" + Login.personName;
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            Login login = new Login();
            login.Show();
        }

        private void dataGridView3_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == 9)
            {
                //get ID of the select row
                int id = (int)dataGridView3.CurrentRow.Cells[0].Value;

                if (id == -1)
                {
                    return;
                }

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
                        if (DBNull.Value == reader[0]) return;
                        byte[] photo = (byte[])reader[0];
                        pictureToDisplay = byteArrayToImage(photo);
                        Image image = ResizeImage(pictureToDisplay, 333, 249);
                        picturebox frm2 = new picturebox();
                        frm2.pictureBox1.Image = image;
                        frm2.Show();
                    }
                }
            }
            if (e.ColumnIndex == 10)
            {
                string c = dataGridView3.CurrentRow.Cells["Remarks"].Value.ToString().Trim();
                if (c != "")
                {
                    reason testDialog = new reason();
                    testDialog.Show();
                    testDialog.txtreason.Text = dataGridView3.CurrentRow.Cells["Remarks"].Value.ToString().Trim();
                    testDialog.button1.Visible = false;
                    testDialog.button2.Visible = false;
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

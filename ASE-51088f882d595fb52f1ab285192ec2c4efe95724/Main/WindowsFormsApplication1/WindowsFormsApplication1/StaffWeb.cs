using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Forms.DataVisualization.Charting;
using WindowsFormsApplication1.App_Code;
using System.IO;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace WindowsFormsApplication1
{
    public partial class StaffWeb : Form
    {
        ConnectionString cs = new ConnectionString();
        Image pictureToDisplay = null;
        public StaffWeb()
        {
            InitializeComponent();
            fillCombo();

        }
        
        public void bindgridview(){
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
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPage1"])
            {
                bindgridview();
            }
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPage2"])
            {
                //https://www.youtube.com/watch?v=RuI3LL2kf98
                //https://www.c-sharpcorner.com/UploadFile/1e050f/chart-control-in-windows-form-application/
                //https://www.youtube.com/watch?v=6ua-IegyKB4
                //this.chart1
            }
        }
        public void bindchart() {
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            string countTotalAttendance = "Select count(*) as TotalClassCount from ClassSchedule where [index]='10124'";
            cmd = new SqlCommand(countTotalAttendance);
            cmd.Connection = con;
            SqlDataReader reader = cmd.ExecuteReader();
            int TotalAttendance = 0;
            if (reader.HasRows) {
                while (reader.Read()) {
                    TotalAttendance = (int)reader[0];
                }
            }
            reader.Close();

            cmd = null;
            string a = comboBox2.SelectedItem.ToString();
            string cmdString = "";
            if(a=="All")
                cmdString = " select weightedScore, count(table3.weightedScore) as CountGrade from ( select DISTINCT table1.MatriCardNo,table2.weightedScore  from( select Student.MatriCardNo, case when Attendance.[status] = 'Present' then 1 else 0 end as counts from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[CourseCode]=StudentClassEnroll.[CourseCode] and ClassSchedule.[index]=StudentClassEnroll.[index] inner join Responsibility on Responsibility.[index] = ClassSchedule.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo  where staffId=@staffid ) as table1  inner join ( SELECT MatriCardNo,StudentGrade.CourseCode,  case when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>90 then 'A+' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>85 then 'A' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>80 then 'A-' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>75 then 'B+' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>70 then 'B' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>65 then 'B-' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>60 then 'C+' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>55 then 'C' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>50 then 'C-' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>40 then 'D' else 'F' end as weightedScore  from [StudentGrade] group by MatriCardNo,StudentGrade.CourseCode ) as table2 on table1.MatriCardNo=table2.MatriCardNo ) as table3 group by weightedScore";
            else
                cmdString = " select weightedScore, count(table3.weightedScore) as CountGrade from ( select DISTINCT table1.MatriCardNo,table2.weightedScore  from( select Student.MatriCardNo, case when Attendance.[status] = 'Present' then 1 else 0 end as counts from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[CourseCode]=StudentClassEnroll.[CourseCode] and ClassSchedule.[index]=StudentClassEnroll.[index] inner join Responsibility on Responsibility.[index] = ClassSchedule.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo  where [ClassSchedule].[index]=@index and staffId=@staffid ) as table1  inner join ( SELECT MatriCardNo,StudentGrade.CourseCode,  case when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>90 then 'A+' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>85 then 'A' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>80 then 'A-' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>75 then 'B+' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>70 then 'B' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>65 then 'B-' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>60 then 'C+' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>55 then 'C' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>50 then 'C-' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>40 then 'D' else 'F' end as weightedScore  from [StudentGrade] group by MatriCardNo,StudentGrade.CourseCode ) as table2 on table1.MatriCardNo=table2.MatriCardNo ) as table3 group by weightedScore";
            cmd = new SqlCommand(cmdString);
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@staffid", LoginInfo.StaffID);
            if(a!="All")
                cmd.Parameters.AddWithValue("@index", a);
            DataSet ds = new DataSet();
            DataTable dt = new DataTable("MyTable");
            SqlDataReader reader2 = cmd.ExecuteReader();
            dt.Load(reader2);
            Boolean A0, B0, C0;
            Boolean A, B, C,D,F;
            Boolean A1, B1, C1;
            A0 = B0 = C0  = false;
            A = B = C = D = F = false;
            A1 = B1 = C1  = false;
            foreach(DataRow row in dt.Rows){
                string g = row["weightedScore"].ToString();
                if (g == "A+") A0 = true;
                if (g == "A") A = true;
                if (g == "A-") A1 = true;

                if (g == "B+") B0 = true;
                if (g == "B") B = true;
                if (g == "B-") B1 = true;

                if (g == "C+") C0 = true;
                if (g == "C") C= true;
                if (g == "C-") C1 = true;

                if (g == "D") D = true;
                if (g == "F") F = true;
                
                
                
                
            }
            if (!A0) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "A+"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }
            if (!A) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "A"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }
            if (!A1) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "A-"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }

            if (!B0) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "B+"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }
            if (!B) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "B"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }
            if (!B1) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "B-"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }

            if (!C0) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "C+"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }
            if (!C) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "C"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }
            if (!C1) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "C-"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }

            if (!D) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "D"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }
            if (!F) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "F"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }

            dt.Columns.Add("SortOrder", typeof(System.Int32));
            foreach (DataRow row in dt.Rows)
            {
                if (row["weightedScore"].ToString() == "A+") row["SortOrder"] = 0;
                if (row["weightedScore"].ToString() == "A") row["SortOrder"] = 1;
                if (row["weightedScore"].ToString() == "A-") row["SortOrder"] = 2;

                if (row["weightedScore"].ToString() == "B+") row["SortOrder"] = 3;
                if (row["weightedScore"].ToString() == "B") row["SortOrder"] = 4;
                if (row["weightedScore"].ToString() == "B-") row["SortOrder"] = 5;

                if (row["weightedScore"].ToString() == "C+") row["SortOrder"] = 6;
                if (row["weightedScore"].ToString() == "C") row["SortOrder"] = 7;
                if (row["weightedScore"].ToString() == "C-") row["SortOrder"] = 8;

                if (row["weightedScore"].ToString() == "D") row["SortOrder"] = 9;
                if (row["weightedScore"].ToString() == "F") row["SortOrder"] = 10;               
            }

            dt.DefaultView.Sort = "SortOrder";
            dt = dt.DefaultView.ToTable();

            chart1.ChartAreas[0].AxisX.LabelStyle.Interval = 1;

            ds.Tables.Add(dt);
            chart1.DataSource = ds;
            //chart1.Series.Add(new Series("Grade"));
            //chart1.Series[0].XValueMember = "weightedScore";
            //chart1.Series[0].YValueMembers = "CountGrade";
            //chart1.ChartAreas[0].AxisX.Title = "Grade";
            //chart1.ChartAreas[0].AxisY.Title = "Number of Student";
            //chart1.DataBind();
        }

        public void bindchart2(){
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            string countTotalAttendance = "Select count(*) as TotalClassCount from ClassSchedule where [index]='10124'";
            cmd = new SqlCommand(countTotalAttendance);
            cmd.Connection = con;
            SqlDataReader reader = cmd.ExecuteReader();
            int TotalAttendance = 0;
            if (reader.HasRows) {
                while (reader.Read()) {
                    TotalAttendance = (int)reader[0];
                }
            }
            reader.Close();

            cmd = null;
            string cmdString = " select weightedScore,TotalAttendanceCount,count(*) as TotalGradeCount from( select table1.MatriCardNo ,weightedScore, sum(counts) as 'TotalAttendanceCount'  from( select Student.MatriCardNo, case when Attendance.[status] = 'Present' then 1 else 0 end as counts from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[CourseCode]=StudentClassEnroll.[CourseCode] and ClassSchedule.[index]=StudentClassEnroll.[index] inner join Responsibility on Responsibility.[index] = ClassSchedule.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo  where [ClassSchedule].[index]=10124 and staffId=@staffid ) as table1  inner join ( SELECT MatriCardNo,StudentGrade.CourseCode,  case when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>90 then 'A+'  when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>85 then 'A' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>80 then 'A-' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>75 then 'B+' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>70 then 'B' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>65 then 'B-' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>60 then 'C+' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>55 then 'C' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>50 then 'C-' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>40 then 'D' else 'F' end as weightedScore   from [StudentGrade] group by MatriCardNo,StudentGrade.CourseCode ) as table2 on table1.MatriCardNo=table2.MatriCardNo group by weightedScore,table1.MatriCardNo) as table3 group by weightedScore,TotalAttendanceCount";
            cmd = new SqlCommand(cmdString);
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@staffid", LoginInfo.StaffID);
            DataSet ds = new DataSet();
            DataTable dt = new DataTable("MyTable");
            dt.Columns.Add("TotalGradeCount", typeof(System.Double));

            SqlDataReader reader2 = cmd.ExecuteReader();
            dt.Load(reader2);
            ds.Tables.Add(dt);
            chart2.DataSource = ds;
            chart2.Series.Add(new Series("A+"));
            chart2.Series.Add(new Series("A"));
            chart2.Series.Add(new Series("A-"));

            chart2.Series.Add(new Series("B+"));
            chart2.Series.Add(new Series("B"));
            chart2.Series.Add(new Series("B-"));

            chart2.Series.Add(new Series("C+"));
            chart2.Series.Add(new Series("C"));
            chart2.Series.Add(new Series("C-"));

            chart2.Series.Add(new Series("D"));
            chart2.Series.Add(new Series("F"));
            
            
            int[,] data = new int[11, TotalAttendance + 1];

            int colTocheck = -1;
            int temp = -1;
            foreach (DataRow row in dt.Rows)
            {
                int rowToCheck = -1;
                if (row[1].ToString().Trim() == "A+") { rowToCheck = 0; }
                if (row[1].ToString().Trim() == "A") { rowToCheck = 1; }
                if (row[1].ToString().Trim() == "A-") { rowToCheck = 2; }
                if (row[1].ToString().Trim() == "B+") { rowToCheck = 3; }
                if (row[1].ToString().Trim() == "B") { rowToCheck = 4; }
                if (row[1].ToString().Trim() == "B-") { rowToCheck = 5; }
                if (row[1].ToString().Trim() == "C+") { rowToCheck = 6; }
                if (row[1].ToString().Trim() == "C") { rowToCheck = 7; }
                if (row[1].ToString().Trim() == "C-") { rowToCheck = 8; }
                if (row[1].ToString().Trim() == "D") { rowToCheck = 9; }
                if (row[1].ToString().Trim() == "F") { rowToCheck = 10; }
                colTocheck = Int32.Parse(row[0].ToString().Trim());
                data[rowToCheck, colTocheck] = 1;
            }
            for (int i = 0; i<11; i++)
            {
                for (int k = 0; k <= TotalAttendance; k++)
                {
                    if(data[i,k]==0){
                        DataRow dr = dt.NewRow();
                        string grade = "";
                        if (i == 0) grade = "A+";
                        if (i == 1) grade = "A";
                        if (i == 2) grade = "A-";

                        if (i == 3) grade = "B+";
                        if (i == 4) grade = "B";
                        if (i == 5) grade = "B-";

                        if (i == 6) grade = "C+";
                        if (i == 7) grade = "C";
                        if (i == 8) grade = "C-";

                        if (i == 9) grade = "D";
                        if (i == 10) grade = "F";
                        
                        dr["weightedScore"] = grade;
                        dr["TotalAttendanceCount"] = k;
                        dr["TotalGradeCount"] = (TotalAttendance/100.0); 
                        dt.Rows.Add(dr); 
                    }
                }
            }
            dt.Columns.Add("SortOrder", typeof(System.Double));
            foreach (DataRow row in dt.Rows)
            {
                if (row["weightedScore"].ToString() == "A+") row["SortOrder"] = 0;
                if (row["weightedScore"].ToString() == "A") row["SortOrder"] = 1;
                if (row["weightedScore"].ToString() == "A-") row["SortOrder"] = 2;

                if (row["weightedScore"].ToString() == "B+") row["SortOrder"] = 3;
                if (row["weightedScore"].ToString() == "B") row["SortOrder"] = 4;
                if (row["weightedScore"].ToString() == "B-") row["SortOrder"] = 5;

                if (row["weightedScore"].ToString() == "C+") row["SortOrder"] = 6;
                if (row["weightedScore"].ToString() == "C") row["SortOrder"] = 7;
                if (row["weightedScore"].ToString() == "C-") row["SortOrder"] = 8;

                if (row["weightedScore"].ToString() == "D") row["SortOrder"] = 9;
                if (row["weightedScore"].ToString() == "F") row["SortOrder"] = 10;
            }

            dt.DefaultView.Sort = "TotalAttendanceCount, SortOrder";
            dt = dt.DefaultView.ToTable();

            foreach (DataRow row in dt.Rows)
            {
                foreach (Series s in chart2.Series)
                {
                    string seriesName = s.Name;
                    string score = row["weightedScore"].ToString();
                    if (seriesName.Equals(score))
                    {
                        s.Points.AddXY(row["TotalAttendanceCount"].ToString(), row["TotalGradeCount"]);
                        break;
                    }
                }
                

            }
            
            chart2.ChartAreas[0].AxisX.Title = "Total Lesson Attended";
            chart2.ChartAreas[0].AxisY.Title = "Number of Student per Grade";
            
            chart2.DataBind();
        }
        public void fillCombo()
        {
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand getIndex = new SqlCommand("select * from Responsibility where staffId=@id", con);
            getIndex.Parameters.AddWithValue("@id", LoginInfo.StaffID);
            SqlDataReader reader;
            con.Open();
            comboBox1.Items.Add("All");
            comboBox2.Items.Add("All");
            try
            {
                reader = getIndex.ExecuteReader();

                while (reader.Read())
                {
                    //Help fix https://www.youtube.com/watch?v=cdkDHkXyVFI
                    string index = reader["index"].ToString();
                    comboBox1.Items.Add(index);
                    comboBox2.Items.Add(index);

                }
            }
            catch (Exception ex)
            {

            }
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
        }

        public void format()
        {
            DataGridViewColumn column = dataGridView3.Columns[3];
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

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            bindchart();
            bindchart();
        }

        private void StaffWeb_Load(object sender, EventArgs e)
        {
            populateStudentGrade();
        }

        void populateStudentGrade()
        {
            using (SqlConnection con = new SqlConnection(cs.DBConn))
            {
                con.Open();
                //SqlDataAdapter adapter = new SqlDataAdapter("SELECT Student.Name, StudentGrade.MatriCardNo, StudentGrade.CourseCode, StudentGrade.sScore, StudentGrade.OverallScore, StudentGrade.Weightage FROM StudentGrade, Student WHERE StudentGrade.MatriCardNo = Student.MatriCardNo", con);
                SqlDataAdapter adapter = new SqlDataAdapter("SELECT id,MatriCardNo, CourseCode, OverallScore, sScore, Weightage FROM StudentGrade", con);
                DataTable dt = new DataTable();

                adapter.Fill(dt);
                dataGridView1.DataSource = dt;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                DataGridViewRow row = dataGridView1.CurrentRow;
                string a = row.Cells[0].Value.ToString().Trim();
                if (row.Cells[0].Value.ToString().Trim() == "")
                {
                    using (SqlConnection con = new SqlConnection(cs.DBConn))
                    {
                        con.Open();
                        SqlCommand cmd = new SqlCommand("INSERT INTO StudentGrade(MatriCardNo,CourseCode,OverallScore,sScore,Weightage) VALUES (@matri, @cc, @overall, @score, @weightage)", con);
                        //cmd.CommandType = CommandType.StoredProcedure;
                        if (row.Cells["txtMatri"].Value != DBNull.Value && row.Cells["txtcc"].Value != DBNull.Value && row.Cells["txtTotal"].Value != DBNull.Value && row.Cells["txtScore"].Value != DBNull.Value && row.Cells["txtWeightage"].Value != DBNull.Value)
                        {
                            cmd.Parameters.AddWithValue("@matri", row.Cells["txtMatri"].Value.ToString());
                            cmd.Parameters.AddWithValue("@cc", row.Cells["txtcc"].Value.ToString());
                            cmd.Parameters.AddWithValue("@overall", Convert.ToInt32(row.Cells["txtTotal"].Value == DBNull.Value ? "0" : row.Cells["txtTotal"].Value.ToString()));
                            cmd.Parameters.AddWithValue("@score", Convert.ToInt32(row.Cells["txtScore"].Value == DBNull.Value ? "0" : row.Cells["txtScore"].Value.ToString()));
                            cmd.Parameters.AddWithValue("@weightage", Convert.ToInt32(row.Cells["txtWeightage"].Value == DBNull.Value ? "0" : row.Cells["txtWeightage"].Value.ToString()));
                            cmd.ExecuteNonQuery();
                            populateStudentGrade();
                        }
                    }
                }
                else 
                {
                    
                }
            }
        }
    }
}

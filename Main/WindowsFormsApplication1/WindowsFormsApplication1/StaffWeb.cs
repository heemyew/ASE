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

namespace WindowsFormsApplication1
{
    public partial class StaffWeb : Form
    {
        ConnectionString cs = new ConnectionString();

        public StaffWeb()
        {
            InitializeComponent();
            fillCombo();
            bindchart();
            bindchart2();
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
            string cmdString = " select weightedScore, count(table3.weightedScore) as CountGrade from ( select DISTINCT table1.MatriCardNo,table2.weightedScore  from( select Student.MatriCardNo, case when Attendance.[status] = 'Present' then 1 else 0 end as counts from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[CourseCode]=StudentClassEnroll.[CourseCode] and ClassSchedule.[index]=StudentClassEnroll.[index] inner join Responsibility on Responsibility.[index] = ClassSchedule.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo  where [ClassSchedule].[index]=10124 and staffId=@staffid ) as table1  inner join ( SELECT MatriCardNo,StudentGrade.CourseCode,  case when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>70 then 'A' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>50 then 'B' else 'C' end as weightedScore from [StudentGrade] group by MatriCardNo,StudentGrade.CourseCode ) as table2 on table1.MatriCardNo=table2.MatriCardNo ) as table3 group by weightedScore";
            cmd = new SqlCommand(cmdString);
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@staffid", LoginInfo.StaffID);
            DataSet ds = new DataSet();
            DataTable dt = new DataTable("MyTable");
            SqlDataReader reader2 = cmd.ExecuteReader();
            dt.Load(reader2);
            Boolean A,B, C, D, E;
            A = B = C = D = E = false;
            foreach(DataRow row in dt.Rows){
                switch (row["weightedScore"].ToString()){
                    case "A":
                        A = true;
                        break;
                    case "B":
                        B = true;
                        break;
                    case "C":
                        C = true;
                        break;
                    case "D":
                        D = true;
                        break;
                    case "E":
                        E = true;
                        break;
                    default:
                        break;
                }
            }
            if (!A) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "A"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }
            if (!B) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "B"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }
            if (!C) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "C"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }
            if (!D) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "D"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }
            if (!E) { DataRow dr = dt.NewRow(); dr["weightedScore"] = "E"; dr["CountGrade"] = "0"; dt.Rows.Add(dr); }

            ds.Tables.Add(dt);
            chart1.DataSource = ds;
            chart1.Series.Add(new Series("Grade"));
            chart1.Series[0].XValueMember = "weightedScore";
            chart1.Series[0].YValueMembers = "CountGrade";  
            chart1.DataBind();
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
            string cmdString = " select weightedScore,TotalAttendanceCount,count(*) as TotalGradeCount from( select table1.MatriCardNo ,weightedScore, sum(counts) as 'TotalAttendanceCount'  from( select Student.MatriCardNo, case when Attendance.[status] = 'Present' then 1 else 0 end as counts from student inner join StudentClassEnroll on StudentClassEnroll.MatriCardNo = student.MatriCardNo inner join ClassSchedule on ClassSchedule.[CourseCode]=StudentClassEnroll.[CourseCode] and ClassSchedule.[index]=StudentClassEnroll.[index] inner join Responsibility on Responsibility.[index] = ClassSchedule.[index] left join Attendance on  ClassSchedule.id=Attendance.ClassScheduleID and Attendance.MatriCard=Student.MatriCardNo  where [ClassSchedule].[index]=10124 and staffId=@staffid ) as table1  inner join ( SELECT MatriCardNo,StudentGrade.CourseCode,  case when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>70 then 'A' when Sum(((sScore+0.0)/(OverallScore+0.0))*((Weightage)))>50 then 'B' else 'C' end as weightedScore  from [StudentGrade] group by MatriCardNo,StudentGrade.CourseCode ) as table2 on table1.MatriCardNo=table2.MatriCardNo group by weightedScore,table1.MatriCardNo) as table3 group by weightedScore,TotalAttendanceCount";
            cmd = new SqlCommand(cmdString);
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@staffid", LoginInfo.StaffID);
            DataSet ds = new DataSet();
            DataTable dt = new DataTable("MyTable");
            SqlDataReader reader2 = cmd.ExecuteReader();
            dt.Load(reader2);
            ds.Tables.Add(dt);
            chart2.DataSource = ds;
            chart2.Series.Add(new Series("A"));
            chart2.Series.Add(new Series("B"));
            chart2.Series.Add(new Series("C"));
            chart2.Series.Add(new Series("D"));
            int[,] data = new int[4, TotalAttendance+1];
            foreach (DataRow row in dt.Rows)
            {
                int rowToCheck=-1;
                if (row[0].ToString().Trim() == "A") { rowToCheck = 0; }
                if (row[0].ToString().Trim() == "B") { rowToCheck = 1; }
                if (row[0].ToString().Trim() == "C") { rowToCheck = 2; }
                if (row[0].ToString().Trim() == "D") { rowToCheck = 3; }
                int colTocheck = (int)row[1];
                data[rowToCheck, colTocheck] = 1;
                foreach (Series s in chart2.Series)
                {
                    string seriesName = s.Name;
                    if (seriesName.Equals(row["weightedScore"]))
                    {
                        s.Points.AddXY(row["TotalAttendanceCount"].ToString(), row["TotalGradeCount"]);
                        break;
                    }
                }
            }

            for(int i =0; i<4;i++){
                for (int k = 0;k<=TotalAttendance ; k++)
                {
                    if(data[i,k]==0){
                        chart2.Series[i].Points.AddXY(k.ToString(),0.01);
                    }
                }
            }

            

            chart2.ChartAreas[0].AxisY.Interval = 1;
            chart2.DataBind();
        }
        public void fillCombo()
        {
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand getIndex = new SqlCommand("select * from Responsibility where staffId=@id", con);
            getIndex.Parameters.AddWithValue("@id", LoginInfo.StaffID);
            SqlDataReader reader;
            con.Open();
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
            comboBox2.SelectedIndex=0;
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
    }
}

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
        System.Windows.Forms.Form f = System.Windows.Forms.Application.OpenForms["Login"];

        public StaffWeb()
        {
            InitializeComponent();
            fillCombo();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPage1"])
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
            if (tabControl1.SelectedTab == tabControl1.TabPages["tabPage2"])
            {
                //https://www.youtube.com/watch?v=RuI3LL2kf98
                //https://www.c-sharpcorner.com/UploadFile/1e050f/chart-control-in-windows-form-application/
                //https://www.youtube.com/watch?v=6ua-IegyKB4
                //this.chart1
            }
        }

        public void fillCombo()
        {
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand getId = new SqlCommand("select staffId from Staff where emailAddress=@email");
            getId.Parameters.AddWithValue("@email", ((Login)f).email);
            con.Open();
            SqlCommand getIndex = new SqlCommand("select * from Responsibility where staffId=@id", con);
            getIndex.Parameters.AddWithValue("@id", getId);
            SqlDataReader reader;
            try
            {
                reader = getIndex.ExecuteReader();

                while (reader.Read())
                {
                    //Help fix https://www.youtube.com/watch?v=cdkDHkXyVFI
                    //string index = reader.GetString("index");
                    //comboBox1.Items.Add(index);
                }
            }
            catch (Exception ex)
            {

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
                    if (row.Cells[7].Value.ToString().Trim() != "")
                    {
                        DataGridViewButtonCell btn = new DataGridViewButtonCell();
                        btn.Value = "View Photo";
                        row.Cells[8] = btn;

                    }

                }
                if (((int)row.Cells[0].Value) != -1 && row.Cells["Status"].Value.ToString().Trim() == "Absence")
                {
                    row.Cells[0].Value = -1;
                }

            }

        }
    }
}

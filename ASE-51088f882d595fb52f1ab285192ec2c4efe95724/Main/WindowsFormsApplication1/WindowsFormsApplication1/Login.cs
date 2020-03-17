using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsFormsApplication1.App_Code;

namespace WindowsFormsApplication1
{
    public partial class Login : Form
    {
        string selectedDomain;
        ConnectionString cs = new ConnectionString();

        public Login()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedDomain = comboBox1.Text;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = new SqlCommand("select * from Account where emailAddress=@email and password=@password and domain=@domain", con);
            cmd.Parameters.AddWithValue("@email", textBox1.Text);
            cmd.Parameters.AddWithValue("@password", textBox2.Text);
            cmd.Parameters.AddWithValue("@domain", selectedDomain);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            con.Open();
            int i = cmd.ExecuteNonQuery();
            //con.Close();
            SqlCommand scmd = new SqlCommand("select * from Staff where email='" + textBox1.Text + "'", con);
            string position ="";
            SqlDataReader reader = scmd.ExecuteReader();
            if(reader.HasRows){
                while (reader.Read()) {
                    LoginInfo.StaffID = reader[0].ToString();
                    LoginInfo.Name = reader[1].ToString();
                    LoginInfo.Email = reader[2].ToString();
                    position = reader[5].ToString();
                }
            }
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("Successfully logged in");
                this.Hide();
                if (selectedDomain == "Student")
                {
                    TakeAttendace takeAtt = new TakeAttendace();
                    takeAtt.Show();
                }
                else
                {
                    if (position.Equals("Lab Technician"))
                    {
                        Main mainPage = new Main();
                        mainPage.Show();
                    }
                    else
                    {
                        StaffWeb sw = new StaffWeb();
                        sw.Show();
                    }
                }

            }
            else
            {
                MessageBox.Show("Please enter Correct Username and Password");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data;
using System.IO;

namespace WindowsFormsApplication1.App_Code
{
    class SFace
    {
        ConnectionString cs = new ConnectionString();

        public Image Image { get; set; }
        public string matriNo { get; set; }
        public string name { get; set; }

        public SFace() { }
        public SFace(Image Image, string mc, string name) {
            this.Image = Image;
            this.matriNo = mc;
            this.name = name;
        }
        public void uploadStudentFace(Image i, string matriNumber){
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            string cmdString = "insert into StudentFace(SFace,MatriCardNo) VALUES (@d1,@d2)";
            cmd = new SqlCommand(cmdString);
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@d2", matriNumber);

            SqlParameter p = new SqlParameter("@d1", SqlDbType.Image);
            MemoryStream ms = new MemoryStream();
            Bitmap bmpImage = new Bitmap(i);
            bmpImage.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] data = ms.GetBuffer();
            p.Value = data;
            cmd.Parameters.Add(p);
            cmd.ExecuteReader();
            con.Close();
        }

    }
}

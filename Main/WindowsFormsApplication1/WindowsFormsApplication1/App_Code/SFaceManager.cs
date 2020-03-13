using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1.App_Code
{
    class SFaceManager
    {
        private List<SFace> faceList;
        public List<SFace> FaceList { get { return faceList; } set { faceList = value; } }

        ConnectionString cs = new ConnectionString();
        public SFaceManager() {
            faceList = new List<SFace>();
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            string cmdString = "select SFace,Student.MatriCardNo,Student.Name from StudentFace inner join Student on StudentFace.MatriCardNo = Student.MatriCardNo";
            cmd = new SqlCommand(cmdString);
            cmd.Connection = con;
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                while (reader.Read())
                {
                    byte[] photo = (byte[])reader[0];
                    Image i = byteArrayToImage(photo);
                    SFace sf = new SFace(i,reader.GetString(1),reader.GetString(2));
                    faceList.Add(sf);
                }
            }


        }

        public void uploadStudentFace(Image i, string matriNumber)
        {
            SFace sf = new SFace();
            sf.uploadStudentFace(i,matriNumber);
            SqlConnection con = new SqlConnection(cs.DBConn);
            SqlCommand cmd = null;
            con.Open();
            string cmdString = "select top 1 Student.Name from StudentFace inner join Student on StudentFace.MatriCardNo = Student.MatriCardNo where Student.MatriCardNo=@matricard";
            cmd = new SqlCommand(cmdString);
            cmd.Connection = con;
            cmd.Parameters.AddWithValue("@matricard", matriNumber);
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.HasRows) {
                while (reader.Read()) {
                    SFace sf1 = new SFace(i, matriNumber, reader.GetString(0));
                    faceList.Add(sf1);
                    break;
                }
            }
            con.Close();
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

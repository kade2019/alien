using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.Sql;
using System.Data.SqlClient;
using AForge;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using ZXing.Aztec;

namespace CHDMS
{
    public partial class Form1 : Form
    {
        public static SqlConnection usercon;
        public static DataTable table;
        private FilterInfoCollection CaptureDevice;
        private VideoCaptureDevice FinalFrame;
        string decoded;

        public Form1()
        {
            InitializeComponent();
            string con = "Data Source= 192.168.1.101, 1433;Initial Catalog=CHDMS;User ID=chdms;Password=admin";
            usercon = new SqlConnection(con);

            CaptureDevice = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo Device in CaptureDevice)
                comboBox1.Items.Add(Device.Name);
            FinalFrame = new VideoCaptureDevice();
        }

        private void FinalFrame_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                pictureBox1.Image = (Bitmap)eventArgs.Frame.Clone();
            }
            catch { }
        }
        


        private void Form1_Load(object sender, EventArgs e)
        {
            int x = this.Width, y = this.Height;

            this.WindowState = FormWindowState.Maximized;

            panel1.Left = (this.Width / 2) - (panel1.Width / 2);
            panel1.Top = (this.Height / 2) - (panel1.Height / 2);
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                FinalFrame = new VideoCaptureDevice(CaptureDevice[comboBox1.SelectedIndex].MonikerString);
                FinalFrame.NewFrame += new NewFrameEventHandler(FinalFrame_NewFrame);
                FinalFrame.Start();
                comboBox1.Enabled = false;
                timer1.Start();
            }
            catch { }
        }

        public static int user;

        private void Button1_Click(object sender, EventArgs e)
        {
            string query = "SELECT * FROM USERS WHERE USERNAME = '" + uname.Text + "' AND PASSWORD = '" + pword.Text + "'";
            table = queries(query);

            if (table.Rows.Count == 1)
            {
                user = Convert.ToInt32(table.Rows[0][0]);
                Departments a = new Departments();
                a.ShowDialog();
                a.Back = this;
                uname.Clear();
                pword.Clear();
            }
        }

        public static DataTable queries(string query)
        {
            SqlDataAdapter sda = new SqlDataAdapter(query, usercon);
            DataTable dt = new DataTable();
            sda.Fill(dt);
            return dt;
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                BarcodeReader reader = new BarcodeReader();
                Result result = reader.Decode((Bitmap)pictureBox1.Image);
                try
                {
                    decoded = result.ToString().Trim();
                    if (decoded != string.Empty)
                        uname.Text = decoded;
                    
                }
                catch { }
            }
            catch { timer1.Start(); }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using CSTM;
using System.IO;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Diagnostics;

namespace CHDMS
{
    public partial class Departments : Form
    {
        public Form Back
        { get; set; }

        public Departments()
        {
            InitializeComponent();
        }

        private void Label7_Click(object sender, EventArgs e)
        {

        }

        private void Departments_Load(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            string query = @"SELECT USERNAME, PASSWORD, D.departmentName FROM USERS U
                            INNER JOIN DEPARTMENTS D ON U.DEPARTMENT = D.DEPARTMENTID
                            WHERE USERID = " + Form1.user;
            Form1.table = Form1.queries(query);
            uname.Text = Form1.table.Rows[0][0].ToString();
            dept.Text = Form1.table.Rows[0][2].ToString();
        }

        public static int deptid;

        private void dept_Click(object sender, EventArgs e)
        {
            Label l = sender as Label;
            string query = "SELECT * FROM DEPARTMENTS WHERE DEPARTMENTNAME = '" + l.Text.Replace('\'', '\0') + "'";
            Form1.table = Form1.queries(query);
            deptid = Convert.ToInt32(Form1.table.Rows[0][0]);
            string value = "";
            if (cstm.InputBox("Department PIN", "PIN:", ref value, true) == DialogResult.OK)
            {
                if (value == Form1.table.Rows[0][2].ToString())
                {
                    MessageBox.Show("Files Accessed!");
                }
            }
        }

        private void AddFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        public static string filepath;

        private void DocumentsToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files|*.*";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filepath = ofd.FileName;
                byte[] bytes = File.ReadAllBytes(filepath);


                string check = "SELECT * FROM DATAS WHERE DATANAME = '" + ofd.SafeFileName + "' AND DATADEPARTMENT = '" + deptid + "'";
                Form1.table = Form1.queries(check);
                if (Form1.table.Rows.Count == 0)
                {
                    string pin = "";
                    string value = "";
                    if (cstm.InputBox("Create PIN for your file", "PIN:", ref value, true) == DialogResult.OK)
                    {
                        pin = value;
                        Form1.usercon.Open();
                        SqlCommand cmd = new SqlCommand("INSERT INTO DATAS (DATANAME, DATADEPARTMENT, DATAPIN, DATACREATOR, DATACREATED, DATAFILE, DATATYPE) VALUES (@dn, @dd, @dp, @dc, @dcreate, @df, @dt)", Form1.usercon);
                        cmd.Parameters.Add("@dn", ofd.SafeFileName);
                        cmd.Parameters.Add("@dd", deptid);
                        cmd.Parameters.Add("@dp", pin);
                        cmd.Parameters.Add("@dc", Form1.user);
                        cmd.Parameters.Add("@dcreate", DateTime.Now.ToString("MMMM dd, yyyy"));
                        cmd.Parameters.Add("@df", bytes);
                        cmd.Parameters.Add("@dt", "DOCUMENTS");
                        cmd.ExecuteNonQuery();
                        Form1.usercon.Close();
                    }
                    else
                        return;
                }
                else
                {
                    if(MessageBox.Show("File already exists, do you wish to ovoerwrite?", "Overwrite", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Form1.usercon.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE DATAS SET DATACREATED = @dc, DATAFILE = @df WHERE DATANAME = '" + ofd.SafeFileName + "'", Form1.usercon);
                        cmd.Parameters.Add("@dc", DateTime.Now.ToString("MMMM dd, yyyy"));
                        cmd.Parameters.Add("@df", bytes);
                        cmd.ExecuteNonQuery();
                        Form1.usercon.Close();
                    }
                }
            }
        }

        private void FilesToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void DocumentsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel2.Controls.Clear();

            string query = "SELECT * FROM DATAS WHERE DATATYPE = 'DOCUMENTS' AND DATADEPARTMENT = '" + deptid + "'";
            Form1.table = Form1.queries(query);
            for (int x = 0; x < Form1.table.Rows.Count; x++)
            {
                Label l = new Label();
                PictureBox pb = new PictureBox();
                l.Parent = panel2;
                pb.Parent = panel2;
                l.Text = Form1.table.Rows[x][1].ToString();
                l.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
                l.Location = new Point(8, 46 + (x * 35));
                l.Size = new Size(495, 20);
                l.BackColor = Color.FromArgb(163, 211, 243);
                pb.BackColor = Color.FromArgb(163, 211, 243);
                pb.Location = new Point(1, 40 + (x * 35));
                pb.Size = new Size(495, 30);
                l.MouseClick += new MouseEventHandler(l_rclick);
                pb.MouseClick += new MouseEventHandler(pb_rclick);
            }
        }

        public static string fname;
        byte[] file;

        private void l_rclick(object sender, MouseEventArgs e)
        {
            Label l = sender as Label;
            if (e.Button == MouseButtons.Right)
            {
                string query = "SELECT * FROM DATAS WHERE DATANAME = '" + l.Text + "' AND DATADEPARTMENT = '" + deptid + "'";
                Form1.table = Form1.queries(query);
                fname = Form1.table.Rows[0][1].ToString();
                file = (byte[])Form1.table.Rows[0][6];

                ContextMenuStrip ms = new ContextMenuStrip();
                ms.Items.Add("Open");
                ms.Items.Add("Delete");
                ms.Show(Cursor.Position);

                ms.ItemClicked += new ToolStripItemClickedEventHandler(item_clicked);
            }
            else if (e.Button == MouseButtons.Left)
                history();
        }

        private void pb_rclick(object sender, MouseEventArgs e)
        {

        }

        private void item_clicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (e.ClickedItem.Text == "Open")
            {
                ByteArrayToFile();
                Form1.usercon.Open();
                SqlCommand cmd = new SqlCommand("INSERT INTO DATAMONITOR (USERACT, FILENAME, ACTIVITY, TIME, DATE) VALUES (@ua, @fn, @act, @time, @date)", Form1.usercon);
                cmd.Parameters.Add("@ua", Form1.user);
                cmd.Parameters.Add("@fn", Convert.ToInt32(Form1.table.Rows[0][0]));
                string query = @"SELECT * FROM USERS
                            WHERE USERID = " + Form1.user;
                Form1.table = Form1.queries(query);
                cmd.Parameters.Add("@dept", Convert.ToInt32(Form1.table.Rows[0][3]));
                cmd.Parameters.Add("@act", "Open");
                cmd.Parameters.Add(@"time", DateTime.Now.ToString("hh:mm tt"));
                cmd.Parameters.Add(@"date", DateTime.Now.ToString("MMMM dd, yyyy"));
                cmd.ExecuteNonQuery();
                Form1.usercon.Close();
                history();
            }
            else
            {
                MessageBox.Show("Deleted!");
            }
        }
        int a;
        private void ByteArrayToFile()
        {
            a = 0;
            try { File.WriteAllBytes(@"C:\Users\" + Environment.UserName.ToString() + @"\Desktop\" + fname, file); } catch { }
            MessageBox.Show(@"C:\Users\" + Environment.UserName.ToString() + @"\Desktop\" + fname);
            Process.Start(@"C:\Users\" + Environment.UserName.ToString() + @"\Desktop\" + fname);
            timer1.Start();
        }

        private void history()
        {
            panel3.Controls.Clear();

            string query = @"SELECT U.username, dept.departmentName, D.dataName, dm.activity, dm.time, dm.date FROM dataMonitor DM
                            INNER JOIN USERS U ON U.userid = DM.userAct
                            INNER JOIN DATAS D ON D.dataID = DM.filename
                            INNER JOIN Departments DEPT ON U.department = DEPT.departmentID
                            WHERE D.DATANAME = '" + fname + "'";
            Form1.table = Form1.queries(query);

            for (int x = 0; x < Form1.table.Rows.Count; x++)
            {
                Label l = new Label();
                l.Text = Form1.table.Rows[x][0].ToString() + "   " + Form1.table.Rows[x][1].ToString() + "   " + Form1.table.Rows[x][3].ToString() +
                "   " + Form1.table.Rows[x][4].ToString() + "   " + Form1.table.Rows[x][5].ToString();
                l.Parent = panel3;
                l.Location = new Point(6, 39 + (25 * x));
                l.Size = new Size(500, 16);
                l.Font = new Font("Microsoft Sans Serif", 10);
                        
            }
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            if (a == 10)
            {
                try
                {
                    File.Delete(@"C:\Users\" + Environment.UserName.ToString() + @"\Desktop\" + fname);
                    timer1.Stop();
                }
                catch { }
            }
            else
                a++;
        }

        private void PhotosToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            panel2.Controls.Clear();

            string query = "SELECT * FROM DATAS WHERE DATATYPE = 'PHOTOS' AND DATADEPARTMENT = '" + deptid + "'";
            Form1.table = Form1.queries(query);
            for (int x = 0; x < Form1.table.Rows.Count; x++)
            {
                Label l = new Label();
                PictureBox pb = new PictureBox();
                l.Parent = panel2;
                pb.Parent = panel2;
                l.Text = Form1.table.Rows[x][1].ToString();
                l.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
                l.Location = new Point(8, 46 + (x * 35));
                l.Size = new Size(495, 20);
                l.BackColor = Color.FromArgb(163, 211, 243);
                pb.BackColor = Color.FromArgb(163, 211, 243);
                pb.Location = new Point(1, 40 + (x * 35));
                pb.Size = new Size(495, 30);
                l.MouseClick += new MouseEventHandler(l_rclick);
                pb.MouseClick += new MouseEventHandler(pb_rclick);
            }
        }

        private void PhotosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Photos|*.jpeg; *.jpg; *.png; *.gif";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filepath = ofd.FileName;
                byte[] bytes = File.ReadAllBytes(filepath);


                string check = "SELECT * FROM DATAS WHERE DATANAME = '" + ofd.SafeFileName + "' AND DATADEPARTMENT = '" + deptid + "'";
                Form1.table = Form1.queries(check);
                if (Form1.table.Rows.Count == 0)
                {
                    string pin = "";
                    string value = "";
                    if (cstm.InputBox("Create PIN for your file", "PIN:", ref value, true) == DialogResult.OK)
                    {
                        pin = value;
                        Form1.usercon.Open();
                        SqlCommand cmd = new SqlCommand("INSERT INTO DATAS (DATANAME, DATADEPARTMENT, DATAPIN, DATACREATOR, DATACREATED, DATAFILE, DATATYPE) VALUES (@dn, @dd, @dp, @dc, @dcreate, @df, @dt)", Form1.usercon);
                        cmd.Parameters.Add("@dn", ofd.SafeFileName);
                        cmd.Parameters.Add("@dd", deptid);
                        cmd.Parameters.Add("@dp", pin);
                        cmd.Parameters.Add("@dc", Form1.user);
                        cmd.Parameters.Add("@dcreate", DateTime.Now.ToString("MMMM dd, yyyy"));
                        cmd.Parameters.Add("@df", bytes);
                        cmd.Parameters.Add("@dt", "PHOTOS");
                        cmd.ExecuteNonQuery();
                        Form1.usercon.Close();
                    }
                    else
                        return;
                }
                else
                {
                    if (MessageBox.Show("File already exists, do you wish to ovoerwrite?", "Overwrite", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Form1.usercon.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE DATAS SET DATACREATED = @dc, DATAFILE = @df WHERE DATANAME = '" + ofd.SafeFileName + "'", Form1.usercon);
                        cmd.Parameters.Add("@dc", DateTime.Now.ToString("MMMM dd, yyyy"));
                        cmd.Parameters.Add("@df", bytes);
                        cmd.ExecuteNonQuery();
                        Form1.usercon.Close();
                    }
                }
            }
        }

        private void VideosToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel2.Controls.Clear();

            string query = "SELECT * FROM DATAS WHERE DATATYPE = 'VIDEOS' AND DATADEPARTMENT = '" + deptid + "'";
            Form1.table = Form1.queries(query);
            for (int x = 0; x < Form1.table.Rows.Count; x++)
            {
                Label l = new Label();
                PictureBox pb = new PictureBox();
                l.Parent = panel2;
                pb.Parent = panel2;
                l.Text = Form1.table.Rows[x][1].ToString();
                l.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
                l.Location = new Point(8, 46 + (x * 35));
                l.Size = new Size(495, 20);
                l.BackColor = Color.FromArgb(163, 211, 243);
                pb.BackColor = Color.FromArgb(163, 211, 243);
                pb.Location = new Point(1, 40 + (x * 35));
                pb.Size = new Size(495, 30);
                l.MouseClick += new MouseEventHandler(l_rclick);
                pb.MouseClick += new MouseEventHandler(pb_rclick);
            }
        }

        private void VideosToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files|*.mp4; *.avi; *.wmv; *.mpeg; *.mov";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filepath = ofd.FileName;
                byte[] bytes = File.ReadAllBytes(filepath);


                string check = "SELECT * FROM DATAS WHERE DATANAME = '" + ofd.SafeFileName + "' AND DATADEPARTMENT = '" + deptid + "'";
                Form1.table = Form1.queries(check);
                if (Form1.table.Rows.Count == 0)
                {
                    string pin = "";
                    string value = "";
                    if (cstm.InputBox("Create PIN for your file", "PIN:", ref value, true) == DialogResult.OK)
                    {
                        pin = value;
                        Form1.usercon.Open();
                        SqlCommand cmd = new SqlCommand("INSERT INTO DATAS (DATANAME, DATADEPARTMENT, DATAPIN, DATACREATOR, DATACREATED, DATAFILE, DATATYPE) VALUES (@dn, @dd, @dp, @dc, @dcreate, @df, @dt)", Form1.usercon);
                        cmd.Parameters.Add("@dn", ofd.SafeFileName);
                        cmd.Parameters.Add("@dd", deptid);
                        cmd.Parameters.Add("@dp", pin);
                        cmd.Parameters.Add("@dc", Form1.user);
                        cmd.Parameters.Add("@dcreate", DateTime.Now.ToString("MMMM dd, yyyy"));
                        cmd.Parameters.Add("@df", bytes);
                        cmd.Parameters.Add("@dt", "VIDEOS");
                        cmd.ExecuteNonQuery();
                        Form1.usercon.Close();
                    }
                    else
                        return;
                }
                else
                {
                    if (MessageBox.Show("File already exists, do you wish to ovoerwrite?", "Overwrite", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Form1.usercon.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE DATAS SET DATACREATED = @dc, DATAFILE = @df WHERE DATANAME = '" + ofd.SafeFileName + "'", Form1.usercon);
                        cmd.Parameters.Add("@dc", DateTime.Now.ToString("MMMM dd, yyyy"));
                        cmd.Parameters.Add("@df", bytes);
                        cmd.ExecuteNonQuery();
                        Form1.usercon.Close();
                    }
                }
            }
        }

        private void FilesToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            panel2.Controls.Clear();

            string query = "SELECT * FROM DATAS WHERE DATATYPE = 'MUSICS' AND DATADEPARTMENT = '" + deptid + "'";
            Form1.table = Form1.queries(query);
            for (int x = 0; x < Form1.table.Rows.Count; x++)
            {
                Label l = new Label();
                PictureBox pb = new PictureBox();
                l.Parent = panel2;
                pb.Parent = panel2;
                l.Text = Form1.table.Rows[x][1].ToString();
                l.Font = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
                l.Location = new Point(8, 46 + (x * 35));
                l.Size = new Size(495, 20);
                l.BackColor = Color.FromArgb(163, 211, 243);
                pb.BackColor = Color.FromArgb(163, 211, 243);
                pb.Location = new Point(1, 40 + (x * 35));
                pb.Size = new Size(495, 30);
                l.MouseClick += new MouseEventHandler(l_rclick);
                pb.MouseClick += new MouseEventHandler(pb_rclick);
            }
        }

        private void MusicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "All files|*.wav; *.mp3; *.wma; *.m4a";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filepath = ofd.FileName;
                byte[] bytes = File.ReadAllBytes(filepath);


                string check = "SELECT * FROM DATAS WHERE DATANAME = '" + ofd.SafeFileName + "' AND DATADEPARTMENT = '" + deptid + "'";
                Form1.table = Form1.queries(check);
                if (Form1.table.Rows.Count == 0)
                {
                    string pin = "";
                    string value = "";
                    if (cstm.InputBox("Create PIN for your file", "PIN:", ref value, true) == DialogResult.OK)
                    {
                        pin = value;
                        Form1.usercon.Open();
                        SqlCommand cmd = new SqlCommand("INSERT INTO DATAS (DATANAME, DATADEPARTMENT, DATAPIN, DATACREATOR, DATACREATED, DATAFILE, DATATYPE) VALUES (@dn, @dd, @dp, @dc, @dcreate, @df, @dt)", Form1.usercon);
                        cmd.Parameters.Add("@dn", ofd.SafeFileName);
                        cmd.Parameters.Add("@dd", deptid);
                        cmd.Parameters.Add("@dp", pin);
                        cmd.Parameters.Add("@dc", Form1.user);
                        cmd.Parameters.Add("@dcreate", DateTime.Now.ToString("MMMM dd, yyyy"));
                        cmd.Parameters.Add("@df", bytes);
                        cmd.Parameters.Add("@dt", "MUSICS");
                        cmd.ExecuteNonQuery();
                        Form1.usercon.Close();
                    }
                    else
                        return;
                }
                else
                {
                    if (MessageBox.Show("File already exists, do you wish to ovoerwrite?", "Overwrite", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        Form1.usercon.Open();
                        SqlCommand cmd = new SqlCommand("UPDATE DATAS SET DATACREATED = @dc, DATAFILE = @df WHERE DATANAME = '" + ofd.SafeFileName + "'", Form1.usercon);
                        cmd.Parameters.Add("@dc", DateTime.Now.ToString("MMMM dd, yyyy"));
                        cmd.Parameters.Add("@df", bytes);
                        cmd.ExecuteNonQuery();
                        Form1.usercon.Close();
                    }
                }
            }
        }
    }
}

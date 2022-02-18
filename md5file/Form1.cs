using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace md5file
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InstallEventHandlers(this);
        }
        private Pen borderColor = Pens.Red;
        public Pen ActiveControlAngleColor { get { return this.borderColor; } set { this.borderColor = value; this.Refresh(); } }
        public void InstallEventHandlers(Control containerControl)
        {
            foreach (Control c in containerControl.Controls)
            {
                if (c is Button) c.Cursor = Cursors.Hand;
            }
            containerControl.Paint -= ContainerControl_Paint;
            containerControl.Paint += ContainerControl_Paint;
            foreach (Control nestedControl in containerControl.Controls)
            {
                nestedControl.Enter -= Control_ReceivedFocus;
                nestedControl.Enter += Control_ReceivedFocus;
                if (nestedControl is ScrollableControl ||
                    nestedControl is TabControl ||
                    nestedControl is TabPage ||
                    nestedControl is Panel ||
                    nestedControl is GroupBox ||
                    nestedControl is DataGrid ||
                    nestedControl is ToolStrip ||
                    nestedControl is MenuStrip)
                    InstallEventHandlers(nestedControl);
            }
        }
        private void Control_ReceivedFocus(object sender, EventArgs e)
        {
            this.Refresh();
        }
        private void ContainerControl_Paint(object sender, PaintEventArgs e)
        {
            Control activeControl = this.ActiveControl;
            if (ActiveControl != null && activeControl.Parent == sender)
            {
                int vline = 8;
                int hline = 8;
                Rectangle rect;
                if (activeControl is CheckBox)
                {
                    rect = new Rectangle(activeControl.Location.X - 2, activeControl.Location.Y - 2, activeControl.Size.Width + 2, activeControl.Size.Height + 2);
                }
                else
                {
                    rect = new Rectangle(activeControl.Location.X - 2, activeControl.Location.Y - 2, activeControl.Size.Width + 3, activeControl.Size.Height + 3);
                }
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                if (vline * 2 > rect.Size.Height) vline = (rect.Size.Height / 2) - 2;
                if (hline * 2 > rect.Size.Width) hline = (rect.Size.Width / 2) - 2;
                // Draw Angle NO
                e.Graphics.DrawLine(borderColor, rect.X, rect.Y, rect.X + hline, rect.Y);
                e.Graphics.DrawLine(borderColor, rect.X, rect.Y, rect.X, rect.Y + vline);
                // Draw Angle NE
                e.Graphics.DrawLine(borderColor, rect.X + rect.Width - hline, rect.Y, rect.X + rect.Width, rect.Y);
                e.Graphics.DrawLine(borderColor, rect.X + rect.Width, rect.Y, rect.X + rect.Width, rect.Y + vline);
                // Draw Angle SE
                e.Graphics.DrawLine(borderColor, rect.X + rect.Width, rect.Y + rect.Height - vline, rect.X + rect.Width, rect.Y + rect.Height);
                e.Graphics.DrawLine(borderColor, rect.X + rect.Width - hline, rect.Y + rect.Height, rect.X + rect.Width, rect.Y + rect.Height);
                // Draw Angle SO
                e.Graphics.DrawLine(borderColor, rect.X, rect.Y + rect.Height - vline, rect.X, rect.Y + rect.Height);
                e.Graphics.DrawLine(borderColor, rect.X, rect.Y + rect.Height, rect.X + hline, rect.Y + rect.Height);
            }
        }
        protected string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch
            {
                MessageBox.Show("Il file potrebbe essere in uso\nChiuderlo o riprovare più tardi", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return "";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog d = new OpenFileDialog();
            d.Title = "Apri file per creare MD5";
            d.FileName = "";
            d.Filter = "Tutti i file (*.*)|*.*";
            d.CheckFileExists = true;
            d.CheckPathExists = true;
            if(d.ShowDialog()==DialogResult.OK)
            {
                textBox1.Text = d.FileName;
                button2.Enabled = true;
            }
            else
            {
                textBox1.Text = "";
                button2.Enabled = false;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            string file = textBox1.Text;
            string md5 = GetMD5HashFromFile(file);
            if (md5=="")
            {
                textBox1.Text = ""; button2.Enabled = false;
            }else
            {
                textBox2.Text = md5;
            }
        }
        private void copiaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox2.Text);
        }
        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Title = "Apri file da controllare";
            o.FileName = "";
            o.Filter = "Tutti i file (*.*)|*.*";
            o.CheckFileExists = true;
            o.CheckPathExists = true;
            if(o.ShowDialog()==DialogResult.OK)
            {
                textBox3.Text = o.FileName;
            }
            else { textBox3.Text = ""; }
        }
        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            button4.Enabled = (textBox4.Text.Length > 8);
        }
        private void button4_Click(object sender, EventArgs e)
        {
            string md5 = textBox4.Text;
            string file = textBox3.Text;
            string md5file = GetMD5HashFromFile(file);
            if(string.IsNullOrEmpty(md5file)==true)
            {
                MessageBox.Show("E' stato rilevato un errore durante il calcolo md5 del file", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                if(md5!=md5file)
                {
                    MessageBox.Show("Codice MD5 non uguale, file non sicuro", "Errore", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Codice MD5 uguale.", "MD5 corretto", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void incollaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(Clipboard.ContainsText()==true)
            {
                textBox4.Text = Clipboard.GetText(TextDataFormat.Text);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EasySignManager
{
    public partial class Form5 : Form
    {

        public string imagePath;
        clientConfig cc;

        private Form1 parent;

        public Form5()
        {
            InitializeComponent();
        }


        public Form5(Form1 prt)
        {
            InitializeComponent();
            parent = prt;
            this.Text = parent.listBox1.SelectedItem.ToString();
        }

        private void Form5_Closed(object sender, EventArgs e)
        {
            parent.Enabled = true;
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            ftpmanager ftp = new ftpmanager(parent.configfile);

            cc = new clientConfig(ftp.getClientConfig(parent.listBox1.SelectedItem.ToString()));

            textBox1.Text = cc.updateInterval.ToString();
            textBox2.Text = cc.getTime();

            if (cc.enableCheckBox)
            {
                checkBox1.Checked = true;
                dateTimePicker1.Value = cc.updateDate;
            }
                
            

        }

        //Close button
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //Upload the new server-sided config / image
        private void button2_Click(object sender, EventArgs e)
        {
            ftpmanager ftp = new ftpmanager(parent.configfile);
            cc.updateInterval = Int32.Parse(textBox1.Text);
            cc.updateDate = clientConfig.formatDateTimeWithCorrectTime(dateTimePicker1.Value, textBox2.Text);

            if (imagePath == null && checkBox1.Checked == true) 
            {
                MessageBox.Show("Es wurde kein Bild ausgewählt. Deaktivieren Sie die Reservierungsoption zum Fortfahren.");
                return;
            } else if (checkBox1.Checked == false)
            {
                ftp.uploadClientConfig(parent.listBox1.SelectedItem.ToString(), cc.getConfigWithoutDate());
                this.Close();
                return;
            }

            ftp.uploadClientConfig(parent.listBox1.SelectedItem.ToString(), cc.getConfig());
            ftp.uploadDatedPicture(imagePath, parent.listBox1.SelectedItem.ToString());
            this.Close();
        }

        //enable date reserved image option
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                button3.Enabled = true;
                dateTimePicker1.Enabled = true;
                textBox2.Enabled = true;
            }
            else
            {
                button3.Enabled = false;
                dateTimePicker1.Enabled = false;
                textBox2.Enabled = false;
            }

        }

        //OpenFileDialog for image
        private void button3_Click(object sender, EventArgs e)
        {

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {

                    imagePath = openFileDialog.FileName;
                    MessageBox.Show("Bild wurde ausgewählt.");

                }


            }

        }

    }

    public class clientConfig
    {

        public int updateInterval = 0;
        public DateTime updateDate;
        public bool enableCheckBox = false;

        public clientConfig(String s)
        {
            formatAndInitializeClientConfig(s);
        }

        //Initializes the parameters
        public void formatAndInitializeClientConfig(String s)
        {
            if (s == null)
                return;

            String[] temp = s.Split(';');

            if (temp.Length != 2) //fucking bullshit
            {
                updateInterval = Int32.Parse(temp[0]);
                //MessageBox.Show(temp.Length.ToString());
                return;
            }

            updateInterval = Int32.Parse(temp[0]);
            updateDate = DateTime.Parse(temp[1]);
            enableCheckBox = true;

        }

        public string getConfig()
        {
            return updateInterval.ToString() + ";" + updateDate.ToString();
        }

        public string getConfigWithoutDate()
        {
            return updateInterval.ToString();
        }

        //holy shit
        static public DateTime formatDateTimeWithCorrectTime(DateTime d, string time)
        {
            DateTime x;

            string[] temp = d.ToString().Split(' ');
            temp[1] = time;

            x = DateTime.Parse(temp[0] + " " + temp[1]);
            return x;

        }

        public string getTime()
        {

            string[] temp = updateDate.ToString().Split(' ');

            return temp[1];
        }


    }

}

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


        private Form1 parent;

        public Form5()
        {
            InitializeComponent();
        }


        public Form5(Form1 prt)
        {
            InitializeComponent();
            parent = prt;
            this.Text = "Einstellungen von " + parent.listBox1.SelectedItem.ToString();
        }

        private void Form5_Closed(object sender, EventArgs e)
        {
            parent.Enabled = true;
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            ftpmanager ftp = new ftpmanager(parent.configfile);
            clientConfig cc = new clientConfig(ftp.getClientConfig(parent.listBox1.SelectedItem.ToString()));

            textBox1.Text = cc.updateInterval.ToString();
            

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            ftpmanager ftp = new ftpmanager(parent.configfile);
            ftp.uploadClientConfig(parent.listBox1.SelectedItem.ToString(), textBox1.Text);
            this.Close();
        }
    }

    public class clientConfig
    {

        public int updateInterval;

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
            updateInterval = Int32.Parse(temp[0]);

        }

    }

}

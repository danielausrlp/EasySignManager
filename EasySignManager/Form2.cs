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
    public partial class Form2 : Form
    {

        //declare parent form
        Form1 parent;

        public Form2()
        {
            InitializeComponent();
        }

        //constructor when parent form is needed
        public Form2(Form1 prt)
        {
            InitializeComponent();
            this.parent = prt;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            initConfig();
        }

        //Enable settings button again after closing and check if server is available
        private void Form2_Closed(object sender, EventArgs e)
        {
            //utter bullshit
            ftpmanager ftp = new ftpmanager(this.textBox1.Text, this.textBox2.Text, this.textBox3.Text, this.textBox4.Text);
            parent.configfile.writeConfig(this.textBox1.Text, this.textBox2.Text, this.textBox3.Text, this.textBox4.Text);
            //update current configs to config class
            parent.configfile.loadConfig();
            ftp.loadRoomList(parent.listBox1);

            parent.Enabled = true;


        }

        private void label4_Click(object sender, EventArgs e)
        {
            parent.configfile.writeConfig(this.textBox1.Text, this.textBox2.Text, this.textBox3.Text, this.textBox4.Text);

        }

        //Put Text into TextBoxes
        private void initConfig()
        {
            this.textBox1.Text = parent.configfile.s_address;
            this.textBox2.Text = parent.configfile.s_username;
            this.textBox3.Text = parent.configfile.s_password;
            this.textBox4.Text = parent.configfile.s_path;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            Form4 form = new Form4(this);
            form.Show();
            this.Enabled = false;

        }
    }
}

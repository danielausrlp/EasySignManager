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
    public partial class Form3 : Form
    {

        //declare parent form
        Form1 prt;

        public Form3()
        {
            InitializeComponent();
        }

        //constructor with parent form
        public Form3(Form1 p)
        {
            InitializeComponent();
            prt = p;
        }


        private void Form3_Load(object sender, EventArgs e)
        {

        }

        //renable parent form after closing
        private void Form3_Closed(object sender, EventArgs e)
        {
            prt.Enabled = true;
        }

        //add roomname to listbox (all checks included)
        private void button1_Click(object sender, EventArgs e)
        {


            if (this.textBox1.Text == "")
            {
                MessageBox.Show("Geben Sie bitte einen Raumnamen an.");
                return;
            }
                

            ftpmanager ftp = new ftpmanager(prt.configfile);

            //check if duplicate
            if (ftp.isRoomExistant(this.textBox1.Text))
            {
                MessageBox.Show("Der Raum existiert schon.");
            } else
            {
                if (ftp.addRoom(this.textBox1.Text))
                {
                    MessageBox.Show("Raum wurde angelegt.");
                    ftp.loadRoomList(prt.listBox1);
                }
                else
                    MessageBox.Show("Raum konnte nicht angelegt werden.");
            }


        }
    }
}

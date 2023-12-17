using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;


namespace EasySignManager
{
    public partial class Form1 : Form
    {
        //FTP-Creds
        public config configfile;


        public Form1()
        {
            InitializeComponent();
        }

        //opens the settings window
        private void button2_Click(object sender, EventArgs e)
        {

            //Disable form after going to settings, reenabling after closing settings
            Form2 form2 = new Form2(this);
            form2.Show();
            this.Enabled = false;

        }

        //opens the create new room window
        private void button3_Click(object sender, EventArgs e)
        {

            //Disable form after going to create new room, reeanbling after closing new room window
            Form3 form3 = new Form3(this);
            form3.Show();
            this.Enabled = false;


        }


        //Add Element to ListBox
        //returns 0 on failure, 1 on success
        public bool addElementToList(string s)
        {
            if (isDuplicate(s))
            {
                return false;
            }
            else
            {
                listBox1.Items.Add(s);
                return true;
            }


        }

        //Check for duplicates in Listbox
        //returns true if duplicate exists, false when not
        public bool isDuplicate(string s)
        {

            for(int i = 0; i < listBox1.Items.Count; i++)
            {
                if(s == listBox1.Items[i].ToString())
                {
                    return true;
                }
            }

            return false; ;
        }

        //enable room delete button after selecting atleast one object in list
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            button4.Enabled = true;
        }

        //delete the currently selected room
        private void button4_Click(object sender, EventArgs e)
        {
            
        }

        //Creates/Loads config file on startup and calls loadRoomList() TODO: Check for invalid/non existing config file
        private void Form1_Load(object sender, EventArgs e)
        {

            configfile = new config();
            ftpmanager ftp = new ftpmanager(configfile);

            ftp.loadRoomList(this.listBox1);

        }

        //double click to open room settings menu
        //TODO: check mouse position to disable clicking anywhere on the listBox to open settings for room
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {

            if(listBox1.SelectedItem != null)
            {
                MessageBox.Show(listBox1.SelectedItem.ToString());
            }

        }

        //Upload button
        private void button1_Click(object sender, EventArgs e)
        {


        }

    }

    //FTP Manager class
    public class ftpmanager
    {

        //FTP-Server creds
        private string address;
        private string username;
        private string password;
        private string path;



        //constructor that adds ftp:// to the address WHY?
        public ftpmanager(string a, string u, string p, string spath)
        {
            address = "ftp://" + a;
            username = u;
            password = p;
            path = spath;

        }

        //constructor with the config class
        public ftpmanager(config c)
        {
            address = "ftp://"+ c.s_address;
            username = c.s_username;
            password = c.s_password;
            path = c.s_path;

        }

        //Add folder (room) to root directory
        //returns 1 on success, 0 on failure
        public bool addRoom(string s)
        {

            bool isCreated = true;

            try
            {
                MessageBox.Show(address + path + s);
                WebRequest request = WebRequest.Create(address + path + s);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    MessageBox.Show(response.StatusCode.ToString());
                }



            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                isCreated = false;

            }


            return isCreated;

        }

        //Remove folder (room) from root directory
        //returns 1 on success, 0 on failure
        public bool deleteRoom(string s)
        {
            bool isDeleted = true;

            try
            {
                WebRequest request = WebRequest.Create(address + path + s);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.RemoveDirectory;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    MessageBox.Show(response.StatusCode.ToString());
                }



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                isDeleted = false;

            }


            return isDeleted;
        }

        //Check if a room is already existing
        //returns 1 on exist, 0 on non existance
        public bool isRoomExistant(string s)
        {
            bool exist = false;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address + path + s);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    exist = true;
                }

            } catch (WebException ex)
            {

                if(ex.Response != null)
                {
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        return false;
                    }
                }

            }


            return exist;
        }

        //fills the listbox with the rooms
        public void loadRoomList(ListBox l)
        {

            string[] arr = null;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address + path);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using(FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Stream responeStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responeStream);

                    string temp = reader.ReadToEnd();

                    arr = temp.Split('\n');

                    reader.Close();
                }

            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //dirty hack to filter out whitespace
            foreach(string s in arr)
            {
                if (s == "")
                    continue;

                l.Items.Add(s);
            }

        }

    }

    //Config File class
    public class config
    {
        public string s_address;
        public string s_username;
        public string s_password;
        public string s_path;

        //get application path for config file
        static private string configtxt = "config.txt";
        static private string dirpath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);

        //configpath with "config.txt"
        string configpath = System.IO.Path.Combine(dirpath, configtxt);

        //saves the unformatted config content 
        public string configcontent;


        //Default Constructor, auto loads default config
        public config()
        {
            loadConfig();
            applyConfig();
        }

        //put the contents of the string into the variables, seperated with a ;
        private void applyConfig()
        {
            if (configcontent == "")
                return;
            else
            {
                string[] arr = configcontent.Split(';');
                s_address = arr[0];
                s_username = arr[1];
                s_password = arr[2];

                if (arr[3] == "")
                    s_path = "\\";
                else
                    s_path = arr[3];
            }

            
        }

        //messagebox the config conents out
        public void debugConfig()
        {
            MessageBox.Show(s_address + " " + s_username + " " + s_password + " " + s_path);
        }

        //Load config into configcontent
        public void loadConfig() {


            try
            {
                if (!File.Exists(configpath))
                {
                    File.Create(configpath);
                }
                else
                {
                    string content;

                        using (StreamReader sr = new StreamReader(configpath))
                        {
                            content = sr.ReadToEnd();
                            configcontent = content;

                        }

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }


            applyConfig();

        }

        //Write new config to config file
        public void writeConfig(string a, string u, string p, string path)
        {

            try
            {

                    
                        using (StreamWriter sr = new StreamWriter(configpath))
                        {


                            sr.Write(a);
                            sr.Write(";");
                            sr.Write(u);
                            sr.Write(";");
                            sr.Write(p);
                            sr.Write(";");

                            if (!(path == ""))
                            {
                                 sr.Write(path);
                                 sr.Write(";");
                            }



                        

                }

                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }



    }
   
}

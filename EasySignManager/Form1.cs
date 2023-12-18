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
using System.Reflection;
using System.Drawing.Imaging;

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
            button1.Enabled = true;
            ftpmanager ftp = new ftpmanager(configfile);

            if(listBox1.SelectedItem == null)
                return;


            Image im = ftp.getPicture(listBox1.SelectedItem.ToString());

            if(im == null)
            {
                MessageBox.Show("Fehler beim Downloaden / Darstellen des Bildes.");
                pictureBox1.Image = null;
            }
            else
            {
                pictureBox1.Image = im;
            }

        }

        //delete the currently selected room
        private void button4_Click(object sender, EventArgs e)
        {

            ftpmanager ftp = new ftpmanager(configfile);
            DialogResult dialogResult = MessageBox.Show("Wollen Sie den Raum " + listBox1.SelectedItem.ToString() + " wirklich löschen?", "Raum löschen", MessageBoxButtons.YesNo);

            if(dialogResult == DialogResult.Yes)
            {
                if (!ftp.deleteRoom(listBox1.SelectedItem.ToString()))
                {
                    MessageBox.Show("Raum konnte nicht gelöscht werden.");
                }

            }


            ftp.loadRoomList(this.listBox1);

            //disable button if listbox is empty
            if (listBox1.Items.Count == 0)
            {
                button4.Enabled = false;
                button1.Enabled = false;
            }

            pictureBox1.Image = null;

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
            ftpmanager ftp = new ftpmanager(configfile);

            string filePath = "";


            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "png files (*.png)|*.png|jpg files (*.jpg)|*.jpg";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {

                    filePath = openFileDialog.FileName;

                    if (ftp.uploadPicture(filePath, listBox1.SelectedItem.ToString()))
                    {
                        MessageBox.Show("Bild wurde hochgeladen.");
                        pictureBox1.Image = new Bitmap(filePath);
                    } else
                    {
                        MessageBox.Show("Bild konnte nicht hochgeladen werden.");
                    }

                }


            }

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



        //constructor that adds ftp:// to the address 
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
                //MessageBox.Show(address + path + s);
                WebRequest request = WebRequest.Create(address + path + s);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.MakeDirectory;
                
                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    //MessageBox.Show(response.StatusCode.ToString());
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
            
            //if room is empty, just delete
            if (isEmpty(s))
            {
                if (deleteDir(s))
                    return true;
                else
                    return false;
                
            } else //delete the files 
            {

                if (deleteAllFiles(s) && deleteDir(s))
                    return true;
                else
                    return false;
            }


        }

        //Check if a room is already existing
        //returns 1 on exist, 0 on non existance TODO: not working as intented
        public bool isRoomExistant(string s)
        {
            

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address + path + s);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    //MessageBox.Show(response.StatusCode.ToString());
                    return true;
                }

            } catch (WebException ex)
            {

                if(ex.Response != null)
                {
                    FtpWebResponse response = (FtpWebResponse)ex.Response;
                    //MessageBox.Show(response.StatusCode.ToString());
                    if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                    {
                        return false;
                    }
                }

            }


            return false;
        }

        //fills the listbox with the rooms
        public void loadRoomList(ListBox l)
        {

            string[] arr = null;

            l.Items.Clear();

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

                    arr = temp.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                    reader.Close();
                }

            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;
            }

            //dirty hack to filter out whitespace
            foreach(string s in arr)
            {
                if (s == "")
                    continue;

                l.Items.Add(s);
            }

        }

        //upload a picture to the ftp server, calls uploadFile | returns true on success, false on failure
        public bool uploadPicture(string p_path, string roomName)
        {

                    byte[] data = File.ReadAllBytes(p_path);

                    try
                    {
                        WebRequest request = WebRequest.Create(address + path + roomName + "/bild.png");
                        request.Credentials = new NetworkCredential(username, password);
                        request.Method = WebRequestMethods.Ftp.UploadFile;


                        using (MemoryStream fs = new MemoryStream(data))
                        {
                            using (Stream ftpStream = request.GetRequestStream())
                            {
                                fs.CopyTo(ftpStream);
                            }
                        }


                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        return false;
                    }

            return true;

            }

        //gets the picture of given room
        public Image getPicture(string roomName)
        {


            try
            {
                WebRequest request = WebRequest.Create(address + path + roomName + "/bild.png");
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.DownloadFile;

                using(WebResponse response = request.GetResponse())
                {
                    Stream s = response.GetResponseStream();
                    return Image.FromStream(s);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }

            


        }

        //checks if room is empty (no pictures)
        public bool isEmpty(string roomName)
        {
            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address + path + roomName);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using(FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Stream s = response.GetResponseStream();

                    StreamReader sr = new StreamReader(s);

                    if (sr.ReadToEnd() == "")
                        return true;
                    else
                        return false;

                }


            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message);
            }


            return false;

        }

        //deletes all files in a directory
        public bool deleteAllFiles(string roomName) {

            string[] files = null;

            try
            {
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(address + path + roomName);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.ListDirectory;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    Stream responeStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(responeStream);

                    string temp = reader.ReadToEnd();
                    string al = temp.Replace(roomName, "");
                    //MessageBox.Show(al);

                    files = al.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                    reader.Close();
                }

            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            foreach(string s in files)
            {

                if (s == "")
                    continue;

                try
                {

                    WebRequest request = WebRequest.Create(address + path + roomName + "/" + s);
                    request.Credentials = new NetworkCredential(username, password);
                    request.Method = WebRequestMethods.Ftp.DeleteFile;

                    using(FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                    {
                        
                    }


                } catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    return false;
                }

                

            }

            
            return true;

        }

        //delete dir
        public bool deleteDir(string s)
        {

            try
            {
                WebRequest request = WebRequest.Create(address + path + s);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.RemoveDirectory;

                using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
                {
                    //MessageBox.Show(response.StatusCode.ToString());
                }



            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;

            }

            return true;
        }

        //upload picture with placeholder overload
        public bool uploadPicture(string roomName)
        {

            Image i = Properties.Resources.platzhalter;

            try
            {
                WebRequest request = WebRequest.Create(address + path + roomName + "/bild.png");
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.UploadFile;


                using (MemoryStream s = new MemoryStream())
                {
                    i.Save(s, ImageFormat.Png);
                    s.Position = 0;
                    using (Stream ftpStream = request.GetRequestStream())
                    {
                        s.CopyTo(ftpStream);
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }

            return true;

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
                    s_path = "/";
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
   


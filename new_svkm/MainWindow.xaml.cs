using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Text.RegularExpressions;

namespace new_svkm
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int appid = 4015559;

        int userid;
        string token;

        string cache_path = Environment.CurrentDirectory + "\\temp\\cache";
        //string data_path = Environment.CurrentDirectory + "\\data";
        string data_path = Environment.ExpandEnvironmentVariables("%AppData%") + "Local\\svkm\\data";

        bool hastoken = false;

        Dictionary<Int64, String> names;

        public MainWindow()
        {
            InitializeComponent();


            if (!Directory.Exists(cache_path))
                Directory.CreateDirectory(cache_path);

            if (!Directory.Exists(data_path))
                Directory.CreateDirectory(data_path);

      

        }

        public void setvisibility(System.Windows.Visibility state)
        {
            button_send.Visibility = state;
            dialog_list.Visibility = state;
            message_list.Visibility = state;
            type_form.Visibility = state;
            user_name.Visibility = state;
            title.Visibility = state;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void button_close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void button_min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //  FileStream user_data_file = File.OpenRead()
            if (File.Exists(data_path + "\\u.vdf"))
            {

                FileStream file_read = File.OpenRead(data_path + "\\u.vdf");
                BinaryReader data_reader = new BinaryReader(file_read);

                userid = Convert.ToInt32(data_reader.ReadString());
                token = data_reader.ReadString();

                data_reader.Close();
                file_read.Close();

                data_reader = null;
                file_read = null;

                
                if(Vk.CheckToken(userid,token))
                {
                    hastoken = true;
                }
                else
                {
                    hastoken = false;
                }

                /*try
                {
                   *File.Decrypt(data_path + "\\u.vdf");
                    FileStream udf_file = File.OpenRead(data_path + "\\u.vdf");
                    BinaryReader read_data = new BinaryReader(udf_file);
                    userid = read_data.ReadInt32();
                    token = read_data.ReadString();
                    read_data.Close();
                    udf_file.Close();
                    File.Encrypt(data_path + "\\u.vdf");
                    hastoken = true; 

                    //File.Decrypt(data_path + "\\u.vdf");
                    string[] lines;
                    lines = File.ReadAllLines(data_path + "\\u.vdf");

                    userid = Convert.ToInt32(lines[0]);
                    token = lines[1];

                    lines = null;

                    //File.Encrypt(data_path + "\\u.vdf");

                    Init_Interface();
                }
                catch (IOException)
                {
                    MessageBox.Show("Ошибка авторизации! Код ошибки: 3", "SVKM Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    hastoken = false;
                    if (File.Exists(data_path + "\\u.vdf"))
                        File.Delete(data_path + "\\u.vdf");
                }
                catch (ObjectDisposedException)
                {
                    MessageBox.Show("Ошибка авторизации! Код ошибки: 4", "SVKM Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    hastoken = false;
                    if (File.Exists(data_path + "\\u.vdf"))
                        File.Delete(data_path + "\\u.vdf");
                }

                catch(FormatException)
                {
                    MessageBox.Show("Ошибка авторизации! Код ошибки: 5", "SVKM Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    hastoken = false;
                    if (File.Exists(data_path + "\\u.vdf"))
                        File.Delete(data_path + "\\u.vdf");
                }

                catch(IndexOutOfRangeException ex)
                {
                    MessageBox.Show("Ошибка авторизации! Код ошибки: 8.\n" + ex.Message.ToString(),"SVKM Error!",MessageBoxButton.OK, MessageBoxImage.Error);
                    if (File.Exists(data_path + "\\u.vdf"))
                        File.Delete(data_path + "\\u.vdf");
                }*/

            }

            if (!hastoken)
            {
                vbrowser.Source = new Uri("https://oauth.vk.com/authorize?client_id=" + appid.ToString() + "&scope=audio,wall,messages,friends,groups&redirect_uri=https://oauth.vk.com/blank.html&display=popup&v=5.27&response_type=token");
            }
            else
            {
                Init_Interface(true);
            }
        }


        public void Init_Interface(bool authorisation_ok)
        {
            vbrowser.Visibility = System.Windows.Visibility.Hidden;
            setvisibility(System.Windows.Visibility.Visible);


            if(authorisation_ok)
            {
                names = new Dictionary<Int64, String>();
                List<VkMessage> dialogs = Vk.GetDialogs(userid, token, 200, true,names);

                

                foreach(VkMessage n in dialogs)
                {
                    ListBoxItem item = new ListBoxItem();
                    

                    if(n.is_chat)
                    {
                        item.Content = n.title;
                        item.Uid = n.chat_id.ToString();
                        item.Tag = true;
                    }
                    else
                    {
                        item.Content = names[n.user_id];
                        item.Uid = n.user_id.ToString();
                        item.Tag = false;
                    }

                    dialog_list.Items.Add(item);
                    item = null; 

                }
            }
        }


        private void vbrowser_AddressChanged(object sender, Awesomium.Core.UrlEventArgs e)
        {

        }

        private void vbrowser_DocumentReady(object sender, Awesomium.Core.UrlEventArgs e)
        {
            string url = vbrowser.Source.ToString();


            if ((url.IndexOf("access_token") != -1) && (!hastoken))
            {
                Regex token_patt = new Regex("token=(.+)&expires_in");
                Match token_match = token_patt.Match(url);
                Regex user_patt = new Regex("user_id=(.+)");
                Match user_match = user_patt.Match(url);

                if ((!token_match.Success) || (!user_match.Success))
                {
                    MessageBox.Show("Ошибка!", "SVKM Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                }
                else
                {
                    token = token_match.Groups[1].Value.ToString();
                    userid = Convert.ToInt32(user_match.Groups[1].Value.ToString());


                    if (File.Exists(data_path + "\\u.vdf"))
                    {
                        File.Delete(data_path + "\\u.vdf");
                    }


                    //try
                    //{
                       /* BinaryWriter write_data = new BinaryWriter(File.OpenWrite(data_path + "\\u.vdf"));
                        write_data.Write(userid);
                        write_data.Write(token);

                        write_data.Close();
                        write_data = null;*/
                        //File.Create(data_path + "\\u.vdf");
                        //string[] lines = { userid.ToString(), token };
                        //File.WriteAllLines(data_path + "\\u.vdf", lines);
                        
                      //  File.Encrypt(data_path + "\\u.vdf");
                        
                   // }
                    /*catch (IOException ex)
                    {
                        MessageBox.Show("Ошибка сохранения данных! Код ошибки: 1."+"\n"+ex.Message.ToString(), "SVKM Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (File.Exists(data_path + "\\u.vdf"))
                            File.Delete(data_path + "\\u.vdf");
                    }
                    catch (ObjectDisposedException ex)
                    {
                        MessageBox.Show("Ошибка сохранения данных! Код ошибки: 2."+"\n"+ex.Message.ToString(), "SVKM Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (File.Exists(data_path + "\\u.vdf"))
                            File.Delete(data_path + "\\u.vdf");
                    }
                    
                    catch(IndexOutOfRangeException ex)
                    {
                        MessageBox.Show("Ошибка сохранения данных! Код ошибки: 7.\n" + ex.Message.ToString(), "SVKM Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                        if (File.Exists(data_path + "\\u.vdf"))
                            File.Delete(data_path + "\\u.vdf");
                    }

                   /* try
                    {
                        File.Encrypt(data_path + "\\u.vdf");
                    }
                    catch(IOException ex)
                    {
                        MessageBox.Show("Ошибка сохранения данных! Код ошибки: 5." + "\n" + ex.Message.ToString(), "SVKM Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    } */

                    FileStream file_writer = File.OpenWrite(data_path + "\\u.vdf");
                    BinaryWriter data_writer = new BinaryWriter(file_writer);


                    data_writer.Write(userid.ToString());
                    data_writer.Write(token);
                    data_writer.Close();

                    file_writer.Close();

                    data_writer = null;
                    file_writer = null;

                    Init_Interface(true);

                }
            }


        }



        private void dialog_list_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem it = (ListBoxItem)dialog_list.SelectedItem;

            if (!(bool)it.Tag)
            {
                string friend_id = it.Uid;
                it = null;
                Show_Dialog(Vk.Dialog_History(friend_id, token, 200));
            }
            else
            {
                String chat_id = it.Uid;
                it = null;
                Show_Chat(Vk.Chat_History(chat_id, token, 200),chat_id);
            }


        }

        /*public void Show_Message(List<VkMessage> dialog)
        {

        }*/

        private void Show_Dialog(List<VkMessage> dialog)
        {
            message_list.Items.Clear();
            foreach(VkMessage msg in dialog)
            {
                ListBoxItem item = new ListBoxItem();

                item.Content = names[msg.user_id] + "\n" + msg.body + "\n";
                item.Uid = msg.user_id.ToString();

                message_list.Items.Add(item);

                item = null;
            }
        }

        private void Show_Chat(List<VkMessage> chat, String chat_id)
        {
            Vk.add_chat_names(chat_id, ref names, token);

            message_list.Items.Clear();
            foreach (VkMessage msg in chat)
            {
                ListBoxItem item = new ListBoxItem();

                item.Content = names[msg.user_id] + "\n" + msg.body + "\n";
                item.Uid = msg.user_id.ToString();

                message_list.Items.Add(item);

                item = null;
            }
        }

       
    }
}

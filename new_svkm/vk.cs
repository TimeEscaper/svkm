using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;

namespace new_svkm
{
    struct VkAttachment
    {
       public string id;
       public string source;
       public string type;
       public string owner_id;
    }
     class VkMessage
     {
         public String body;
         public Int64 user_id;
         public DateTime date;
         public bool IsOut;
         public bool is_chat;
         public int chat_id;
         public String title;
         public bool is_read;

         public List<VkAttachment> attachments;

         public void FromXml(XmlNode n)
         {
  
                    
                 if (n.SelectSingleNode("out").InnerText == "0")
                 {
                     IsOut = false;
                     user_id = Convert.ToInt64(n.SelectSingleNode("uid").InnerText);
                 }
                 else
                 {
                     IsOut = true;
                     user_id = Convert.ToInt64(n.SelectSingleNode("uid").InnerText);
                 }

                 if (n.SelectSingleNode("chat_id") != null)
                 {
                     if (n.SelectSingleNode("title") != null)
                        title = n.SelectSingleNode("title").InnerText;
                     chat_id = Convert.ToInt32(n.SelectSingleNode("chat_id").InnerText);
                     is_chat = true;
                     
                 }
                 else
                 {
                     is_chat = false;
                  
                 }

                 body = n.SelectSingleNode("body").InnerText;
                 //date = Convert.ToDateTime(n.SelectSingleNode("date").InnerText);

                 if (n.SelectSingleNode("read_state").InnerText == "0")
                     is_read = true;
                 else
                     is_read = false;

                if(n.SelectSingleNode("attachments") != null)
                {
                    
                    foreach(XmlNode attach in n.SelectNodes("/attachments/attachment"))
                    {
                        VkAttachment attach_struct = new VkAttachment();
                        string type = n.SelectSingleNode("type").InnerText;
                      
                        switch(type)
                        {
                            case "photo":
                                {
                                    attach_struct.type = type;
                                    attach_struct.id = n.SelectSingleNode("/photo/pid").InnerText;
                                    attach_struct.source = n.SelectSingleNode("/photo/src_big").InnerText;
                                    attachments.Add(attach_struct);
                                    break;
                                }

                            case "wall":
                                {
                                    attach_struct.type = type;
                                    attach_struct.id = n.SelectSingleNode("/wall/id").InnerText;
                                    attach_struct.owner_id = n.SelectSingleNode("/wall/owner_id").InnerText;
                                    attachments.Add(attach_struct);

                                    break;
                                }


                        }

                    }
                }

                
             
         }

         public void FromJson(string msg)
         {

         }

     }
    class Vk
    {

        private static XmlDocument VkAPI_Perform(string request, string token="", string par1="", string par2="",string par3="",string par4="",string par5="")
        {
            XmlDocument res = new XmlDocument();
            
            switch(request)
            {
                case "messages.getDialogs":
                    {
                        res.Load("https://api.vk.com/method/messages.getDialogs.xml?count=" + par1 + "&access_token=" + token);
                        break;
                    }

                case "users.get":
                    {
                        res.Load("https://api.vk.com/method/users.get.xml?user_ids=" + par1 + "&access_token=" + token);
                        break;
                    }

                case "messages.getChat":
                    {
                        res.Load("https://api.vk.com/method/messages.getChat.xml?chat_id=" + par1 + "&access_token=" + token);
                        break;
                    }
            }

            return res;
        }

        public static string get_name(int userid, String token)
        {
            string res;
            XmlDocument data = VkAPI_Perform("users.get",userid.ToString(),token);

            res = data.SelectSingleNode("/response/user/first_name") + " " + data.SelectSingleNode("/response/user/last_name");

            return res;
        }

        public static bool CheckToken(int userid, string token)
        {
            bool valid = false;

            XmlDocument data = new XmlDocument();

            data = VkAPI_Perform("users.get", token, userid.ToString());

            if (data.SelectNodes("/response").Count != 0)
            {
                valid = true;
            }
            else
                MessageBox.Show(data.InnerXml);

            return valid;
        }

        public static List<VkMessage> GetDialogs(int userid, string token, int count, bool need_names, Dictionary<Int64,String> names)
        {
            List<VkMessage> res = new List<VkMessage>();
            XmlDocument dialogs = new XmlDocument();
            dialogs = VkAPI_Perform("messages.getDialogs", token, count.ToString());
            dialogs.Save("data.xml");

            string for_names = "";

            if (dialogs.SelectNodes("/response").Count != 0)
            {
                foreach (XmlNode n in dialogs.SelectNodes("/response/message"))
                {
                    VkMessage msg = new VkMessage();
                    msg.FromXml(n);
                    res.Add(msg);

                    if (need_names)
                        for_names += n.SelectSingleNode("uid").InnerText + ",";

                }

                if(need_names)
                {
                    for_names.Remove(for_names.Length-1);

                    XmlDocument name_list = VkAPI_Perform("users.get", token, for_names);
                    
                    foreach(XmlNode n in name_list.SelectNodes("/response/user"))
                    {
                        Int64 uid = Convert.ToInt64(n.SelectSingleNode("uid").InnerText);
                        if(!names.ContainsKey(uid))
                        {
                            names.Add(uid, n.SelectSingleNode("first_name").InnerText + " " + n.SelectSingleNode("last_name").InnerText);
                        }
                    }
                }
            }

            else 
            {
                MessageBox.Show("Ошибка! Полный текст:\n" + dialogs.InnerXml, "Svkm: error", MessageBoxButton.OK, MessageBoxImage.Error);
            }


            //MessageBox.Show(res.Count.ToString());
            return res;
        }

        public static List<VkMessage> Dialog_History(String friend_id, String token, int count)
        {
            List<VkMessage> res = new List<VkMessage>();
            XmlDocument dialog = new XmlDocument();
            dialog.Load("https://api.vk.com/method/messages.getHistory.xml?user_id=" + friend_id + "&count=" + count.ToString() + "&access_token=" + token);


            if(dialog.SelectNodes("/response/message").Count != 0)
            {
                foreach(XmlNode n in dialog.SelectNodes("/response/message"))
                {
                    VkMessage msg = new VkMessage();
                    msg.FromXml(n);
                    res.Add(msg);
                    msg = null;
                }
            }

            return res;
        }

        public static List<VkMessage> Chat_History(String chat_id, String token, int count)
        {
            List<VkMessage> res = new List<VkMessage>();
            XmlDocument dialog = new XmlDocument();
            dialog.Load("https://api.vk.com/method/messages.getHistory.xml?chat_id=" + chat_id + "&count=" + count.ToString() + "&access_token=" + token);

            

            if(dialog.SelectNodes("/response/message").Count != 0)
            {
                foreach(XmlNode n in dialog.SelectNodes("/response/message"))
                {
                    VkMessage msg = new VkMessage();
                    msg.FromXml(n);
                    res.Add(msg);
                    msg = null;
                }
            }

            return res;
        }

        public static void add_chat_names(String chat_id, ref Dictionary<Int64,String> names, String token)
        {
            XmlDocument data = new XmlDocument();
            string for_names = "";
            data = VkAPI_Perform("messages.getChat", token, chat_id);
            
            foreach(XmlNode n in data.SelectNodes("/response/users/uid"))
            {
                for_names += n.InnerText + ",";

            }

            for_names.Remove(for_names.Length - 1);

            XmlDocument name_list = VkAPI_Perform("users.get", token, for_names);

            foreach (XmlNode n in name_list.SelectNodes("/response/user"))
            {
                Int64 uid = Convert.ToInt64(n.SelectSingleNode("uid").InnerText);
                if (!names.ContainsKey(uid))
                {
                    names.Add(uid, n.SelectSingleNode("first_name").InnerText + " " + n.SelectSingleNode("last_name").InnerText);
                }
            }
           
        }
        
    }
}

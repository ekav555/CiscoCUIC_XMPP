
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using S22.Xmpp.Client;
using S22.Xmpp.Core;
using Newtonsoft.Json;
using System.Configuration;

namespace CiscoCUIC_XMPP
{
    class Program
    {
        static string _hostname = string.Empty;
        static string _username = string.Empty;
        static string _password = string.Empty;
        static int _port = 0;
        static string _topicEndpointURL = string.Empty;
        static string _to = string.Empty;
        static XmppCore _server;
        static XmppClient _client;
        static List<string> _errors;
        static CiscoHttpHelper _chttp;


        #region MAINCODE
        static void Main(string[] args)
        {

            loadErrorCodes();
            loadSettings();
            SetupXmpp();
            subscribeToQueueEvents();


            Console.ReadLine();
        }

        private static void subscribeToQueueEvents()
        {
            //READ THE CISCO DOCS
            var sresult = subscribe(IqType.Get, _to, _client.Jid.ToString(), _topicEndpointURL);
        }

        static void SetupXmpp()
        {
            try
            {
                //We need to to connect as a client so we can receive messages
                connectClient();

                //We connect a server to setup and remove subscribed events/topics
                connectToServer();

                if (_client.Connected && _server.Connected)
                {
                    //check if this account has previously signed up for events/topics and remove it. Because each time we connected we get a new trailing ID after the account name.
                    var result = getSubscriptions(_to, _client.Jid.ToString());

                    var dresult = deserializeXMLToJSON<IQResult>(result.Data);

                    if (dresult != null)
                    {
                        string removePreviousUser = $"{_client.Jid.Node}@{_client.Jid.Domain}/";
                        foreach (var item in dresult.iq.pubsub.subscriptions.subscription)
                        {
                            if (item.jid.Contains(removePreviousUser))
                            {
                                var re = unSubscribe(IqType.Set, _to, item.jid, item.node);
                                Console.WriteLine($"Removed {item.jid} from events {item.node}");
                            }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #endregion

        #region COREMETHODS

        

        private static void loadSettings()
        {
            _hostname = ConfigurationManager.AppSettings["Hostname"];
            _username = ConfigurationManager.AppSettings["Username"];
            _password = ConfigurationManager.AppSettings["Password"];
            _port = Convert.ToInt32(ConfigurationManager.AppSettings["Port"]);
            _to = ConfigurationManager.AppSettings["PubsubURL"];
            _topicEndpointURL = ConfigurationManager.AppSettings["TopicEndpointURL"];
            _chttp = new CiscoHttpHelper();
        }

        private static void loadErrorCodes()
        {
            //Cisco Not Ready Reason Codes I want to Watch for
            _errors = new List<string>();
            _errors.Add("Connection Failure");
            _errors.Add("System Issue");
            _errors.Add("Phone Failure");
        }

        private static void connectClient()
        {
            try
            {
                Console.WriteLine("Connecting to Client");
                _client = new XmppClient(_hostname, _port, true);
                _client.Username = _username;
                _client.Password = _password;
                _client.Message += messageRecieved;
                _client.Error += client_Error;
                _client.Connect();
                Console.WriteLine("Connected to Client Successfully");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void connectToServer()
        {
            try
            {
                Console.WriteLine("Connecting to Server");
                _server = new XmppCore(_hostname, _port, true);
                _server.Username = _username;
                _server.Password = _password;
                _server.Connect();
                Console.WriteLine("Connected to Server Successfully");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static Iq subscribe(IqType type, string to, string from, string topic)
        {
            XmlDocument xml = new XmlDocument();
            XmlDocument xml2 = new XmlDocument();
            XmlElement iq2 = xml.CreateElement("iq");
            XmlElement pubsub2 = xml.CreateElement("pubsub");
            XmlElement sub2 = xml.CreateElement("subscribe");
            sub2.SetAttribute("node", topic);
            sub2.SetAttribute("jid", from);
            pubsub2.SetAttribute("xmlns", "http://jabber.org/protocol/pubsub");
            pubsub2.AppendChild(sub2);
            return _server.IqRequest(IqType.Set, to, from, pubsub2);
        }

        private static Iq getSubscriptions(string to, string from)
        {
            XmlDocument xml = new XmlDocument();
            XmlElement iq = xml.CreateElement("iq");
            XmlElement pubsub = xml.CreateElement("pubsub");
            XmlElement sub = xml.CreateElement("subscriptions");
            pubsub.SetAttribute("xmlns", "http://jabber.org/protocol/pubsub");
            pubsub.AppendChild(sub);
            return _server.IqRequest(S22.Xmpp.Core.IqType.Get, to, from, pubsub);
        }

        private static Iq unSubscribe(IqType type, string to, string from, string topic)
        {
            XmlDocument xml = new XmlDocument();
            XmlDocument xml2 = new XmlDocument();
            XmlElement iq2 = xml.CreateElement("iq");
            XmlElement pubsub2 = xml.CreateElement("pubsub");
            XmlElement sub2 = xml.CreateElement("unsubscribe");
            sub2.SetAttribute("node", topic);
            sub2.SetAttribute("jid", from);
            pubsub2.SetAttribute("xmlns", "http://jabber.org/protocol/pubsub");
            pubsub2.AppendChild(sub2);
            return _server.IqRequest(IqType.Set, to, from, pubsub2);
        }

        private static void writeConsoleLog(CuicMessageV1Text userEvent)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            string reasonCode = "";
            if (userEvent.Update.data?.user?.state != null)
            {
                reasonCode = userEvent.Update.data?.user?.state;
                if (reasonCode == "NOT_READY")
                {
                    if (_errors.Contains(userEvent.Update.data?.user.reasonCode?.label))
                    {
                        //POST TO TEAMS
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                    Console.WriteLine($"Name: {userEvent.Update.data?.user?.firstName} {userEvent.Update.data?.user?.lastName} - State: {userEvent.Update.data?.user?.state} - Reason Code: {userEvent.Update.data?.user?.reasonCode?.label}");
                }
                else
                {
                    Console.WriteLine($"Name: {userEvent.Update.data?.user?.firstName} {userEvent.Update.data?.user?.lastName} - State: {userEvent.Update.data?.user?.state}");
                }
            }
        }

        #endregion

        #region EventHandlers

        private static void client_Error(object sender, S22.Xmpp.Im.ErrorEventArgs e)
        {
            Console.WriteLine($"ERROR: {e.Reason}");
        }

        private static void messageRecieved(object sender, S22.Xmpp.Im.MessageEventArgs e)
        {
            try
            {
                var result = deserializeXMLToJSON<CuicMessageV1>(e.Message.Data);
                if (result?.message._event?.items.item != null)
                {
                    var userEvent = deserializeStringXMLToJSON<CuicMessageV1Text>(result?.message._event?.items?.item?.notification.text);
                    if (userEvent != null)
                    {
                        writeConsoleLog(userEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        #endregion

        #region Deserialization

        private static T deserializeObject<T>(XmlElement message)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StringReader reader = new StringReader(message.OuterXml))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
        private static T deserializeXMLToJSON<T>(XmlElement message)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(message.OuterXml);
            string json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented, false);
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
        }

        private static T deserializeStringXMLToJSON<T>(string message)
        {
            //message = message.Replace("<actions>", "<actions json:Array='true'>");
            StringReader stringReader = new StringReader(message);
            XmlDocument doc = new XmlDocument();
            doc.Load(stringReader);
            string json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented, false);
            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore });
        }
        #endregion
    }
}
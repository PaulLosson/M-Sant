using Microsoft.VisualBasic.Logging;
using System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Opc.UaFx.Client;
using System.Threading;
using Microsoft.VisualBasic.ApplicationServices;
using static System.Net.Mime.MediaTypeNames;
using Opc.UaFx;
using System.ComponentModel.DataAnnotations;
using Opc.Ua;
using System.Xml.Linq;

namespace MOM_Santé
{
    [OpcDataType("ns=2;s=Serial_Number_DataType")]
    [OpcDataTypeEncoding("ns=2;s=Serial_Number_DataType-Binary")]
    public class Serial_Number_DataType
    {
        public string Customer_Id { get; set; }
        public string Emotors_Id { get; set; }
        public string Supplier_Id { get; set; }
        public string Tracking_Id { get; set; }
    }


    [OpcDataType("ns=1;i=97")]
    [OpcDataTypeEncoding(
        "<ns=1;i=97>",
        NamespaceUri = "<http://Emotors/Types/Shared.Value of binary Dictionary-Node>")]
    internal struct CheckReferenceDataType
    {
        public Boolean Bad;
        public Boolean CheckRequested;
        public Boolean ErrorTrackingId;
        public Boolean Good;
        public String ReferenceToCheck;
        public Boolean ResultPresent;
        public String TrackingId;
        public String Type;
    }


        public partial class Form1 : Form
    {
        

        //Parametre connexion serveur
        string endpoint = "Startup";
        OpcClient client;
        private Boolean autoConnect = false;
        private Boolean isConnexion = true;
        

        // Default folder    
        static readonly string rootFolder = @"C:\Temp\Data\";
        //Default file. MAKE SURE TO CHANGE THIS LOCATION AND FILE PATH TO YOUR FILE   
        static readonly string textFile = @"C:\Users\JV16065\Desktop\PreProd local\Line_Middleware_V2\Logs\VpiLine-26000.log";
        static readonly string sharedClass = @"C:\Users\JV16065\Desktop\TEST SHAReD\PREPROD (local) SPRINT 7\LM\Project\Opc.Ua.NodeSet2.Emotors.Types.Shared.xml";

        public Form1()
        {
            InitializeComponent();
            textBox_endpoint.Text = "opc.tcp://EX0012.inetemotors.com:6002/LM";
            
            endpoint = textBox_endpoint.Text;
            if (autoConnect)
            {
                client = new OpcClient(endpoint);
                client.Connect();
                //dotConnexionThread.Abort();
                button_connexion.Text = "Connecté";
                button_connexion.BackColor = Color.Green;
            }
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

            //Thread dotConnexionThread = new Thread(new ThreadStart(ThreadDotConnexion));
            //dotConnexionThread.Start();
            // Read entire text file content in one string  
            //string text = File.ReadAllText(textFile);
            //textBox_Log.Text = text;

            if(isConnexion)
            {
                Connexion("tt");
            }
            else
            {
                Console.WriteLine("Deconnexion du serveur avant fermeture ...");
                client.Disconnect();
                textBox_Log.Text = "Deconnecté";
                button_connexion.Text = "Déconnecté";
                button_connexion.BackColor = Color.Red;
            }
        isConnexion = !isConnexion;
        }

        public void Connexion(string _endpoint)
        {
            try
            {
                _endpoint = textBox_endpoint.Text;
                if (client == null)
                {
                    client = new OpcClient(_endpoint);
                    //client.NodeSet = OpcNodeSet.Load(@"C:\Users\JV16065\Desktop\TEST SHAReD\PREPROD (local) SPRINT 7\LM\Project\Opc.Ua.NodeSet2.Emotors.Types.Shared.xml");
                }
                else
                {
                    Console.WriteLine("Merci d'arreter d'essayer de vous connecter plusieurs fois au serveur");
                }
                client.UseDynamic = true;
                
                client.Connect();
                //dotConnexionThread.Abort();
                button_connexion.Text = "Connecté";
                button_connexion.BackColor = Color.Green;
            }
            catch
            {
                Console.WriteLine("Problème de connection au serveur");
                button_connexion.Text = "XXXXXXX";
                /*
                for (int i = 0; i > 2; i++)
                {
                    client.Connect();
                    textBox_endpoint.Text = "tentative " + i + " sur 3";
                }
                */
            }
        }

        /*
        public void ThreadDotConnexion()
        {
            String dots = ".";

            for (int i = 0; i < 3; i++)
            {
                
                button_connexion.Invoke(new MethodInvoker(delegate
                {
                    //button_connexion.Text = dots;
                }));
                dots = dots + ".";
                Thread.Sleep(250);
            }
        }
        */

        private void button_find_Click(object sender, EventArgs e)
        {
            //var objectNode = client.BrowseNode(OpcObjectTypes.ObjectsFolder);
            //BrowseOP(objectNode);

            //CLASS//
            
            NodeBrower nodeBrowser = new NodeBrower(client);
            nodeBrowser.BrowsePostPonedComment(nodeBrowser.rootNode);

            System.Diagnostics.Debug.WriteLine(nodeBrowser.OPList.Count().ToString() + nodeBrowser.PostPoneList.Count().ToString());

            int i= 0;
            foreach(string op in nodeBrowser.OPList)
            {
                textBox_Assemblage.Text = textBox_Assemblage.Text + op;
                System.Diagnostics.Debug.WriteLine(op);
                System.Diagnostics.Debug.WriteLine(nodeBrowser.PostPoneList[i]);
                UASubscribe(nodeBrowser.PostPoneList[i]);
                i++;
            }
        }
        
        public void UASubscribe(String _nodeId)
        {
            //Conversion of String in Byte array for OPC UA Subscription
            byte[] bytes = Encoding.Default.GetBytes(_nodeId);

            //Creating subscription for Each PostponedComment
            OpcSubscription subscription = client.SubscribeDataChange(
            _nodeId,
            HandleDataChanged);
        }


        private static void HandleDataChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            // The 'sender' variable contains the OpcMonitoredItem with the NodeId.
            OpcMonitoredItem item = (OpcMonitoredItem)sender;

            System.Diagnostics.Debug.WriteLine(
                    "Data Change from NodeId '{0}': {1}",
                    item.NodeId,
                    e.Item.Value);
        }
    }
}
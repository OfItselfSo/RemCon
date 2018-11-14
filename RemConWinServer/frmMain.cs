using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using OISCommon;
using RemConCommon;

/// +------------------------------------------------------------------------------------------------------------------------------+
/// ¦                                                   TERMS OF USE: MIT License                                                  ¦
/// +------------------------------------------------------------------------------------------------------------------------------¦
/// ¦Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation    ¦
/// ¦files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,    ¦
/// ¦modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software¦
/// ¦is furnished to do so, subject to the following conditions:                                                                   ¦
/// ¦                                                                                                                              ¦
/// ¦The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.¦
/// ¦                                                                                                                              ¦
/// ¦THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE          ¦
/// ¦WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR         ¦
/// ¦COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,   ¦
/// ¦ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                         ¦
/// +------------------------------------------------------------------------------------------------------------------------------+

/// This application implements a simple TCP/IP server which demonstrates the sending 
/// and receiving of a typed object to a client. There is a companion client project
/// named RemConClient. This client is will work on either windows or linux
/// without any modification.
/// 
/// The RemConCommon project contains the typed object which is used as the data
/// container and a class named TCPDataTransporter which is an "easy to setup"
/// transport mechanism which works for both the client or server.
/// 
/// The server will listen on the ipAddr and port defined in the RemConConstants
/// static class and the client will try to connect to that address.
/// 
/// To keep things simple the server only accepts one client connection and 
/// it will close the listener after that connection is made.
/// 
/// Both the client and the server send CONNECT and DISCONNECT messages 
/// which enables the other side to trigger actions based on these events.

namespace RemConWinServer
{

    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// The main form for the application
    /// </summary>
    /// <history>
    ///    10 Nov 18  Cynic - Started
    /// </history>
    public partial class frmMain : frmOISBase
    {
        private const string DEFAULTLOGDIR = @"C:\Dump\Project Logs";
        private const string APPLICATION_NAME = "RemConWinServer";
        private const string APPLICATION_VERSION = "01.00";

        // this handles the data transport to and from the client 
        TCPDataTransporter dataTransporter = null;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        public frmMain()
        {
            bool retBOOL = false;

            if (DesignMode == false)
            {
                // set the current directory equal to the exe directory. We do this because
                // people can start from a link and if the start-in directory is not right
                // it can put the log file in strange places
                Directory.SetCurrentDirectory(Application.StartupPath);

                // set up the Singleton g_Logger instance. Simply using it in a test
                // creates it.
                if (g_Logger == null)
                {
                    // did not work, nothing will start say so now in a generic way
                    MessageBox.Show("Logger Class Failed to Initialize. Nothing will work well.");
                    return;
                }
                // record this in the logger for everybodys use
                g_Logger.ApplicationMainForm = this;
                g_Logger.DefaultDialogBoxTitle = APPLICATION_NAME;
                try
                {
                    // set the icon for this form and for all subsequent forms
                    g_Logger.AppIcon = new Icon(GetType(), "App.ico");
                    this.Icon = new Icon(GetType(), "App.ico");
                }
                catch (Exception)
                {
                }

                // Register the global error handler as soon as we can in Main
                // to make sure that we catch as many exceptions as possible
                // this is a last resort. All execeptions should really be trapped
                // and handled by the code.
                OISGlobalExceptions ex1 = new OISGlobalExceptions();
                Application.ThreadException += new ThreadExceptionEventHandler(ex1.OnThreadException);

                // set the culture so our numbers convert consistently
                System.Threading.Thread.CurrentThread.CurrentCulture = g_Logger.GetDefaultCulture();
            }

            InitializeComponent();

            if (DesignMode == false)
            {
                // set up our logging
                retBOOL = g_Logger.InitLogging(DEFAULTLOGDIR, APPLICATION_NAME, false);
                if (retBOOL == false)
                {
                    // did not work, nothing will start say so now in a generic way
                    MessageBox.Show("The log file failed to create. No log file will be recorded.");
                }
                // pump out the header
                g_Logger.EmitStandardLogfileheader(APPLICATION_NAME);
                LogMessage("");
                LogMessage("Version: " + APPLICATION_VERSION);
                LogMessage("");

            }

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Form loading handler
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        private void frmMain_Load(object sender, EventArgs e)
        {

            // we are not connected at this point
            SetScreenVisualsBasedOnConnectionState(false);

            try
            {
                LogMessage("frmMain_Load Setting up the Data Transporter");

                // set up our data transporter
                dataTransporter = new TCPDataTransporter(TCPDataTransporterModeEnum.TCPDATATRANSPORT_SERVER, RemConConstants.SERVER_TCPADDR, RemConConstants.SERVER_PORT_NUMBER);
                // set up the event so the data transporter can send us the data it recevies
                dataTransporter.ServerClientDataEvent += ServerClientDataEventHandler;
                LogMessage("frmMain_Load Data Transporter Setup complete");
            }
            catch (Exception ex)
            {
                LogMessage("frmMain_Load exception: " + ex.Message);
                LogMessage("frmMain_Load exception: " + ex.StackTrace);
                OISMessageBox("Exception setting up the data transporter: " + ex.Message);
            }

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Form closing handler
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            ShutdownDataTransporter();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles presses on the buttonSendMessage button
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        private void buttonSendMessage_Click(object sender, EventArgs e)
        {
            LogMessage("buttonSendMessage_Click");

            if (dataTransporter == null)
            {
                OISMessageBox("No data transporter");
                return;
            }

            ServerClientData scData = new ServerClientData(111, "test from server to client");
             // display it
            AppendDataToTrace("OUT: dataInt=" + scData.DataInt.ToString() + " dataStr=" + scData.DataStr);
           // send it
            dataTransporter.SendData(scData);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles inbound data events
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        private void ServerClientDataEventHandler(object sender, ServerClientData scData)
        {
            if(scData==null)
            {
                LogMessage("ServerClientDataEventHandler scData==null");
                return;
            }

            // what type of data is it
            if (scData.DataContent == ServerClientDataContentEnum.USER_DATA)
            {
                // it is user defined data, log it
                LogMessage("ServerClientDataEventHandler dataInt=" + scData.DataInt.ToString() + " dataStr=" + scData.DataStr);
                // display it
                AppendDataToTrace("IN: dataInt=" + scData.DataInt.ToString() + " dataStr=" + scData.DataStr);
            }
            else if (scData.DataContent == ServerClientDataContentEnum.REMOTE_CONNECT)
            {
                // the remote side has connected
                LogMessage("ServerClientDataEventHandler REMOTE_CONNECT");
                // display it
                AppendDataToTrace("IN: REMOTE_CONNECT");
                // set the screen
                SetScreenVisualsBasedOnConnectionState(true);
            }
            else if (scData.DataContent == ServerClientDataContentEnum.REMOTE_DISCONNECT)
            {
                // the remote side has connected
                LogMessage("ServerClientDataEventHandler REMOTE_DISCONNECT");
                // display it
                AppendDataToTrace("IN: REMOTE_DISCONNECT");
                // set the screen
                SetScreenVisualsBasedOnConnectionState(false);
                // shut things down on our end
                ShutdownDataTransporter();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Shutsdown the data transporter safely
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        private void ShutdownDataTransporter()
        {
            // shutdown the data transporter
            if (dataTransporter != null)
            {
                dataTransporter.Shutdown();
                dataTransporter = null;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if we have a connection. 
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        private bool IsConnected()
        {
            if (dataTransporter == null) return false;
            if (dataTransporter.IsConnected() == false) return false;
            if (buttonSendMessage.Enabled == false) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets up the screen visuals based on the connections state
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        private void SetScreenVisualsBasedOnConnectionState(bool connectionState)
        {
            if(connectionState == true)
            {
                buttonSendMessage.Enabled = true;
            }
            else
            {
                buttonSendMessage.Enabled = false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Appends data to our data trace
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        private void AppendDataToTrace(string dataToAppend)
        {
            if ((dataToAppend == null) || (dataToAppend.Length == 0)) return;
            textBoxDataTrace.Text =  textBoxDataTrace.Text + "\r\n" + dataToAppend;
        }

    }
}

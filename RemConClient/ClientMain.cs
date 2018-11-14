using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using OISCommon;
using RemConCommon;
using System.Runtime.Serialization.Formatters.Binary;

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

/// This application implements a simple TCP/IP client which demonstrates the sending 
/// and receiving of a typed object to a server. There is a companion server project
/// named RemConWinServer. This client is will work on either windows or linux
/// without any modification.
/// 
/// The RemConCommon project contains the typed object which is used as the data
/// container and also a class named TCPDataTransporter which is an "easy to setup"
/// transport mechanism which works for both the client or server.
/// 
/// The client will try to connect to the ipAddr and port defined in the RemConConstants
/// static class and the server will listen on that address and port for incoming 
/// connections.
/// 
/// To keep things simple the client does not re-connect after the connection has been
/// terminated. You would be expected to create a new TcpDataTransporter objet for 
/// that purpose.
/// 
/// Both the client and the server send CONNECT and DISCONNECT messages 
/// which enables the other side to trigger actions based on these events.
namespace RemConClient
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// The main class for the application
    /// </summary>
    /// <history>
    ///    10 Nov 18  Cynic - Started
    /// </history>
    public class ClientMain : OISObjBase
    {
        private const string DEFAULTLOGDIR = @"/home/devuser/Dump/ProjectLogs";
        private const string APPLICATION_NAME = "RemConClient";
        private const string APPLICATION_VERSION = "01.00";

        // this handles the data transport to and from the server 
        TCPDataTransporter dataTransporter = null;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        public ClientMain ()
        {
            bool retBOOL = false;

            Console.WriteLine(APPLICATION_NAME + " started");

            // set the current directory equal to the exe directory. We do this because
            // people can start from a link and if the start-in directory is not right
            // it can put the log file in strange places
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // set up the Singleton g_Logger instance. Simply using it in a test
            // creates it.
            if (g_Logger == null)
            {
                // did not work, nothing will start say so now in a generic way
                Console.WriteLine("Logger Class Failed to Initialize. Nothing will work well.");
                return;
            }

            // Register the global error handler as soon as we can in Main
            // to make sure that we catch as many exceptions as possible
            // this is a last resort. All execeptions should really be trapped
            // and handled by the code.
            System.AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionTrapper;

            // set up our logging
            retBOOL = g_Logger.InitLogging(DEFAULTLOGDIR, APPLICATION_NAME, false, true);
            if (retBOOL == false)
            {
                // did not work, nothing will start say so now in a generic way
                Console.WriteLine("The log file failed to create. No log file will be recorded.");
            }

            // pump out the header
            g_Logger.EmitStandardLogfileheader(APPLICATION_NAME);
            LogMessage("");
            LogMessage("Version: " + APPLICATION_VERSION);
            LogMessage("");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Called out of the program main() function. This is where all of the 
        /// application execution starts (other than the constructer above).
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        public void BeginProcessing()
        {
            Console.WriteLine(APPLICATION_NAME + " processing begins");

            // set up our data transporter
            dataTransporter = new TCPDataTransporter(TCPDataTransporterModeEnum.TCPDATATRANSPORT_CLIENT, RemConConstants.SERVER_TCPADDR, RemConConstants.SERVER_PORT_NUMBER);
            // set up the event so the data transporter can send us the data it recevies
            dataTransporter.ServerClientDataEvent += ServerClientDataEventHandler;

            // we sit and wait for the user to press return. The handler is dealing with the responses
            Console.WriteLine("Press <Return> to quit");
            Console.ReadLine();
            dataTransporter.Shutdown();
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
            if (scData == null)
            {
                LogMessage("ServerClientDataEventHandler scData==null");
                Console.WriteLine("ServerClientDataEventHandler scData==null");
                return;
            }

            // what type of data is it
            if (scData.DataContent == ServerClientDataContentEnum.USER_DATA)
            {
                // user content
                LogMessage("ServerClientDataEventHandler dataInt=" + scData.DataInt.ToString() + " dataStr=" + scData.DataStr);
                Console.WriteLine("inbound data received: dataInt=" + scData.DataInt.ToString() + " dataStr=" + scData.DataStr);

                // for the purposes of demonstrationm, send an ack now
                if (dataTransporter == null)
                {
                    LogMessage("ServerClientDataEventHandler dataTransporter==null");
                    Console.WriteLine("ServerClientDataEventHandler dataTransporter==null");
                    return;
                }

                // send it
                ServerClientData ackData = new ServerClientData(222, "ACK from client to server");
                dataTransporter.SendData(ackData);
            }
            else if (scData.DataContent == ServerClientDataContentEnum.REMOTE_CONNECT)
            {
                // the remote side has connected
                LogMessage("ServerClientDataEventHandler REMOTE_CONNECT");
                Console.WriteLine("ServerClientDataEventHandler REMOTE_CONNECT");
            }
            else if (scData.DataContent == ServerClientDataContentEnum.REMOTE_DISCONNECT)
            {
                // the remote side has connected
                LogMessage("ServerClientDataEventHandler REMOTE_DISCONNECT");
                Console.WriteLine("ServerClientDataEventHandler REMOTE_DISCONNECT");
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// This is where we handle exceptions
        /// </summary>
        /// <history>
        ///    10 Nov 18  Cynic - Started
        /// </history>
        static void UnhandledExceptionTrapper(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject.ToString());
            Console.WriteLine("Press Enter to continue");
            Console.ReadLine();
            Environment.Exit(1);
        }
    }
}

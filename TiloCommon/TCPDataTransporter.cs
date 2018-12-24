using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TiloCommon;
using System.Net.Sockets;
using System.Threading;
using OISCommon;
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

/// NOTE: this class and the entire TiloCommon project is shared with the client which runs on the Beaglebone Black. If your primary
/// interest is in working out how a Typed object is sent between a Server and Client (and back) to transmit complex data you should
/// have a look at the RemCon demonstrator project at http://www.OfItselfSo.com/RemCon which is devoted to that topic. This class 
/// is directly derived from that project.

/// This class is intended to be a simple "drop in" object that C# programs can use to
/// transfer data as a typed object via TCP. It can be used on both the server and 
/// client sides (depending on how you start it) and will function on both Windows
/// and Linux (via Mono)

namespace TiloCommon
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A class to handle data transfer between the server and client. This data
    /// is always assumed to be imbedded in a class of type ServerClientData. The
    /// ServerClientData is also used to send "in-band" signals to the remote
    /// endpoint depending on the state of the ServerClientDataContentEnum flag.
    /// </summary>
    /// <history>
    ///    19 Nov 18  Cynic - Started
    /// </history>
    public class TCPDataTransporter : OISObjBase
    {
        // our server client data received delegate + event
        public delegate void ServerClientDataEvent_Delegate(object sender, ServerClientData scData);
        public ServerClientDataEvent_Delegate ServerClientDataEvent = null;

        private string ipAddr = "";
        private int portNumber = 0;
        private TcpClient tcpClient = null;
        private TcpListener tcpListener = null;
        private BinaryFormatter binaryFormatter;
        private Thread clientReadThread = null;
        private bool shutdownInProgress = false;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor, for a client end connecting to a known ip and port
        /// </summary>
        /// <param name="transportMode">the mode we use to setup the transport</param>
        /// <param name="ipAddrIn">the ip address to connect to</param>
        /// <param name="portNumberIn">the port number to connect to</param>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public TCPDataTransporter(TCPDataTransporterModeEnum transportMode, string ipAddrIn, int portNumberIn)
        {
            // set these values now
            ipAddr = ipAddrIn;
            portNumber = portNumberIn;

            // set up our binary formatter
            binaryFormatter = new BinaryFormatter();

            if (transportMode == TCPDataTransporterModeEnum.TCPDATATRANSPORT_CLIENT)
            {
                // start up our client read thread, we use the above ipAddr and port
                clientReadThread = new Thread(new ThreadStart(InitiateConnectionAndWaitForData));
                clientReadThread.Start();
            }
            else if (transportMode == TCPDataTransporterModeEnum.TCPDATATRANSPORT_SERVER)
            {
                // start up our server listener, we use the above ipAddr and port
                clientReadThread = new Thread(new ThreadStart(InitiateListenerAndWaitForData));
                clientReadThread.Start();
            }
            else
            {
                throw new Exception("TCPDataTransporter unknown Transport Mode:" + transportMode.ToString());
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Initiates the connection to the preset ip address and port and waits
        /// for data. It expects all data to be presented to it as ServerClientData
        /// objects serialized by a BinaryFormatter
        /// 
        /// We exect to be placed in our own thread or we will block the caller
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public void InitiateListenerAndWaitForData()
        {
            LogMessage("InitiateListenerAndWaitForData, started");
            Console.WriteLine("InitiateListenerAndWaitForData, started");

            InitiateListener();

            // now wait for the data
            WaitForData();

            LogMessage("InitiateListenerAndWaitForData, concludes");
            Console.WriteLine("InitiateListenerAndWaitForData, concludes");

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if we have a connection. 
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public bool IsConnected()
        {
            if (tcpClient == null) return false;
            // if we have a tcpClient we assume we have a connection
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Initiates the connection to the preset ip address and port and waits
        /// for data. It expects all data to be presented to it as ServerClientData
        /// objects serialized by a BinaryFormatter
        /// 
        /// We exect to be placed in our own thread or we will block the caller
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public void InitiateConnectionAndWaitForData()
        {
            LogMessage("InitiateConnectionAndWaitForData, started");
            Console.WriteLine("InitiateConnectionAndWaitForData, started");

            InitiateConnection();
            LogMessage("InitiateConnectionAndWaitForData, connection opened");
            Console.WriteLine("InitiateConnectionAndWaitForData, connection opened");

            // now wait for the data
            WaitForData();

            LogMessage("InitiateConnectionAndWaitForData, concludes");
            Console.WriteLine("InitiateConnectionAndWaitForData, concludes");
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Initiates the connection to the preset ip address and port 
        /// 
        /// We exect to be placed in our own thread or we will block the caller
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void InitiateListener()
        {
            try
            {
                // Create a TcpClient. If this client to work you need to have a TcpServer 
                // listening on the same ip address and port
                // start the TCP Listener
                tcpListener = new TcpListener(IPAddress.Parse(IPAddr), PortNumber);
                tcpListener.Start();                
                    
                LogMessage("InitiateListener, tcpListener opened");
                Console.WriteLine("InitiateListener, tcpListener opened");
                
                // Accept will block until someone connects                       
                tcpClient = tcpListener.AcceptTcpClient();

                // we have a connection. log it
                LogMessage("InitiateListener, client connection accepted");
                Console.WriteLine("InitiateListener, client connection accepted");

                // stop and remove the listener - we only accept one client
                tcpListener.Stop();
                tcpListener = null;

                // now we send a message saying we have connected
                ServerClientData scData = new ServerClientData(ServerClientDataContentEnum.REMOTE_CONNECT);
                SendData(scData);

            }
            catch (Exception ex)
            {

                if (shutdownInProgress == true)
                {
                    // ignore the exception. Probably the shutdown triggered it
                    return;
                }
                LogMessage("InitiateListener, exception: " + ex.Message);
                Console.WriteLine("InitiateListener, exception: " + ex.Message);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Initiates the connection to the preset ip address and port 
        /// 
        /// We exect to be placed in our own thread or we will block the caller
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void InitiateConnection()
        {
            try
            {
                // Create a TcpClient. If this client to work you need to have a TcpServer 
                // listening on the same ip address and port
                tcpClient = new TcpClient(IPAddr, PortNumber);

                LogMessage("InitiateConnection, tcpClient opened");
                Console.WriteLine("InitiateConnection, tcpClient opened");

                // now we send a message saying we have connected
                ServerClientData scData = new ServerClientData(ServerClientDataContentEnum.REMOTE_CONNECT);
                SendData(scData);
            }
            catch (Exception ex)
            {

                if (shutdownInProgress == true)
                {
                    // ignore the exception. Probably the shutdown triggered it
                    return;
                }
                LogMessage("InitiateConnection, exception: " + ex.Message);
                Console.WriteLine("InitiateConnection, exception: " + ex.Message);
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Waits for data on the existing tcpClient connection
        /// 
        /// We exect to be placed in our own thread or we will block the caller
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void WaitForData()
        { 
            try
            { 
                // we loop waiting for data 
                while (true)
                {
                    // sit waiting for the data
                    ServerClientData scData = (ServerClientData)binaryFormatter.Deserialize(tcpClient.GetStream());
                    if (scData == null)
                    {
                        LogMessage("WaitForData, scData == null");
                        Console.WriteLine("WaitForData, scData == null");
                        continue;
                    }

                    DebugMessage("Received scData: scData.DataInt, scData.DataStr" + scData.DataStr);

                    // send the data to the interested parties
                    if (ServerClientDataEvent != null)
                    {
                        // NOTE: we are in our own thread here. Caller must be aware of this
                        ServerClientDataEvent(this, scData);
                    }
                } // bottom of while (true)
            }
            catch (Exception ex)
            {

                if (shutdownInProgress == true)
                {
                    // ignore the exception. Probably the shutdown triggered it
                    return;
                }
                LogMessage("WaitForData, exception: " + ex.Message);
                Console.WriteLine("WaitForData, exception: " + ex.Message);
            }

            // if we get here the thread ends
            LogMessage("WaitForData, ending");
            Console.WriteLine("WaitForData, ending");

        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends data via the tcpConnection. 
        /// </summary>
        /// <param name="scData">the data object we send</param>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public void SendData(ServerClientData scData)
        {
            if (tcpClient == null)
            {
                LogMessage("SendData, tcpClient == null");
                Console.WriteLine("SendData, tcpClient == null");
                throw new Exception("SendData, tcpClient == null");
            }
            if (scData == null)
            {
                LogMessage("SendData, scData == null");
                Console.WriteLine("SendData, scData == null");
                throw new Exception("SendData, scData == null");
            }
            binaryFormatter.Serialize(tcpClient.GetStream(), scData);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Shuts down the tcpConnection. Can be called from any thread.
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public void Shutdown()
        {
            LogMessage("Shutdown, called");
            Console.WriteLine("Shutdown, called");

            shutdownInProgress = true;

            // shutdown the tcpListener
            if(tcpListener != null)
            {
                tcpListener.Stop();
                tcpListener = null;
            }

            if (tcpClient != null)
            {

                try
                {
                    // now we send a message saying we have connected
                    ServerClientData scData = new ServerClientData(ServerClientDataContentEnum.REMOTE_DISCONNECT);
                    SendData(scData);
                    Thread.Sleep(500);
                }
                catch { }

                LogMessage("Shutdown, tcpClient closing");
                Console.WriteLine("Shutdown,  tcpClient closing");

                tcpClient.Client.Disconnect(false);

                tcpClient.Client.Close();
                LogMessage("Shutdown, tcpClient stream closed");
                Console.WriteLine("Shutdown, tcpClient stream closed");

                tcpClient.Close();
                LogMessage("Shutdown, tcpClient closed");
                Console.WriteLine("Shutdown, tcpClient closed");
                tcpClient = null;
            }
            else
            {
                // client did not successfully start. Just abort the thread
                clientReadThread.Abort();
                LogMessage("Shutdown, tcpClient thread aborted");
                Console.WriteLine("Shutdown, tcpClient thread aborted");
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the IP address. Will never return null. Will return empty. 
        /// There is no set accessor - that value is set in the constructor
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public string IPAddr
        {
            get
            {
                if (ipAddr == null) ipAddr = "";
                return ipAddr;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the port number.
        /// There is no set accessor - that value is set in the constructor
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public int PortNumber
        {
            get
            {
                return portNumber;
            }
        }

    }
}

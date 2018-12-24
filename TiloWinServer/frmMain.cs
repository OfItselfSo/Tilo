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
using TiloCommon;

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

/// This application interacts with a client program on the Beaglebone Black
/// and sends it commands to output stepper motor pulses and direction 
/// signals. The client is called TiloClient and is co-located in this 
/// same repository as this application. The operation of this software
/// is intimately related to the architecture of the client. Up to
/// six stepper motors can be controlled. The client software runs
/// an assembly language program in the PRU1 which ensures that the 
/// timings set are absolutely fixed and consistent irregardless of 
/// the number of motors running and their pulse frequenies.
/// 
/// This application implements a simple TCP/IP server which sends the 
/// frequency and direction data via a typed C# object. The mechanism
/// behind this is more clearly demonstrated in the RemCon project found
/// at http://www.OfItselfSo.com/RemCon See that project for more details
/// on how the information is transmitted to the Beaglebone Black.
/// The server will listen on the ipAddr and port defined in the TiloConstants
/// static class and the client will try to connect to that address. 
/// 
/// Up to six stepper motors are configured on the screen. There is an 
/// overall "disable all" stepper motors and each can be individually
/// disabled as well. The speed can be set in terms of delay cycles
/// (useful for calibration) and in Hertz. The delay cycle value
/// corresponds to the "wait time" over in the PRU assembly language
/// program. Each cycle corresponds to a certain "delay" in the pulse
/// and that value is reflected in the SECONDS_PER_CYCLE constant.
/// If the assembly language is changed the SECONDS_PER_CYCLE constant
/// will be different and you will need to re-calibrate.
/// 
/// Re-Calibrate by putting in a cycles value of 1 and seeing what 
/// frequency you get. Once you have that you can work it out from there.
///
/// NOTE: it does not matter if you start the WinServer or the Client first.
/// They figure it out between them.
/// 

namespace TiloWinServer
{

    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// The main form for the application
    /// </summary>
    /// <history>
    ///    19 Nov 18  Cynic - Started
    /// </history>
    public partial class frmMain : frmOISBase
    {
        private const string DEFAULTLOGDIR = @"C:\Dump\Project Logs";
        private const string APPLICATION_NAME = "Tilo WinServer";
        private const string APPLICATION_VERSION = "01.00";

        // this handles the data transport to and from the client 
        private TCPDataTransporter dataTransporter = null;

        private bool inhibitAutoSend = false;
        private const double SECONDS_PER_CYCLE = 660e-9;  // yep, 660 x 10^-9

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
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
        /// Form load handler
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void frmMain_Load(object sender, EventArgs e)
        {

            SetupStepperControls();

            // we are not connected at this point
            SetScreenVisualsBasedOnConnectionState(false);

            try
            {
                LogMessage("frmMain_Load Setting up the Data Transporter");

                // set up our data transporter
                dataTransporter = new TCPDataTransporter(TCPDataTransporterModeEnum.TCPDATATRANSPORT_SERVER, TiloConstants.SERVER_TCPADDR, TiloConstants.SERVER_PORT_NUMBER);
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
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            ShutdownDataTransporter();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets up the controls on the form
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void SetupStepperControls()
        {
            SyncAllStepperControlsToScreenState(false);

            stepperControlSTEP0.Title = "STEPPER 0";
            stepperControlSTEP1.Title = "STEPPER 1";
            stepperControlSTEP2.Title = "STEPPER 2";
            stepperControlSTEP3.Title = "STEPPER 3";
            stepperControlSTEP4.Title = "STEPPER 4";
            stepperControlSTEP5.Title = "STEPPER 5";

            SetStepperSpeedModeVisuals("Cycles");

            // wire up our stepper control state changed handlers
            stepperControlSTEP0.StepperStateChangedEvent = StepperControlStateChangedEventHandler;
            stepperControlSTEP1.StepperStateChangedEvent = StepperControlStateChangedEventHandler;
            stepperControlSTEP2.StepperStateChangedEvent = StepperControlStateChangedEventHandler;
            stepperControlSTEP3.StepperStateChangedEvent = StepperControlStateChangedEventHandler;
            stepperControlSTEP4.StepperStateChangedEvent = StepperControlStateChangedEventHandler;
            stepperControlSTEP5.StepperStateChangedEvent = StepperControlStateChangedEventHandler;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle inbound notices that a stepper has changed. We funnel all
        /// of the stepper controls through here. If any one thing on any
        /// stepper changes we resend the state of all of them to the client
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void StepperControlStateChangedEventHandler(object sender)
        {
            // we never send if this is true
            if (inhibitAutoSend == true) return;

            // sends the data from the screen to the client
            if (checkBoxAutoSend.Checked == true) SendDataFromScreenToClient();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles presses on the buttonSendData button
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void buttonSendData_Click(object sender, EventArgs e)
        {
            LogMessage("buttonSendData_Click");

            if (dataTransporter == null)
            {
                OISMessageBox("No data transporter");
                return;
            }
            if(IsConnected() == false)
            {
                OISMessageBox("Not connected");
                return;
            }

            // sends the data from the screen to the client
            SendDataFromScreenToClient();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sends the data from the screen to the client
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void SendDataFromScreenToClient()
        { 
            if (dataTransporter == null)
            {
                LogMessage("SendDataFromScreenToClient, dataTransporter == null");
                return;
            }
            if (IsConnected() == false)
            {
                LogMessage("SendDataFromScreenToClient, Not connected");
                return;
            }

            // some sanity checks
            if (stepperControlSTEP0.StepSpeed == 0)
            {
                OISMessageBox("STEPPER 0, zero is not a valid speed");
                return;
            }
            if (stepperControlSTEP1.StepSpeed == 0)
            {
                OISMessageBox("STEPPER 1, zero is not a valid speed");
                return;
            }
            if (stepperControlSTEP2.StepSpeed == 0)
            {
                OISMessageBox("STEPPER 2, zero is not a valid speed");
                return;
            }
            if (stepperControlSTEP3.StepSpeed == 0)
            {
                OISMessageBox("STEPPER 3, zero is not a valid speed");
                return;
            }
            if (stepperControlSTEP4.StepSpeed == 0)
            {
                OISMessageBox("STEPPER 4, zero is not a valid speed");
                return;
            }
            if (stepperControlSTEP5.StepSpeed == 0)
            {
                OISMessageBox("STEPPER 5, zero is not a valid speed");
                return;
            }

            // get the server client data from the screen
            ServerClientData scData = GetSCDataFromScreen("Data from server to client");

            // display it
            AppendDataToTrace("OUT: dataStr=" + scData.DataStr);
            // send it
            dataTransporter.SendData(scData);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets the server client data from the screen and returns the populated
        /// container
        /// </summary>
        /// <returns>a populated ServerClientData container</returns>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private ServerClientData GetSCDataFromScreen(string scDataText)
        {
            if (scDataText == null) scDataText = "Data from Server to Client";
            ServerClientData scData = new ServerClientData(scDataText);

            // now get the data off the screen
            scData.Step0_Enable = stepperControlSTEP0.StepEnabled;
            scData.Step0_DirState = stepperControlSTEP0.StepDir;
            if (radioButtonHz.Checked == true) scData.Step0_StepSpeed = ConvertHzToCycles(stepperControlSTEP0.StepSpeed);
            else scData.Step0_StepSpeed = stepperControlSTEP0.StepSpeed;

            scData.Step1_Enable = stepperControlSTEP1.StepEnabled;
            scData.Step1_DirState = stepperControlSTEP1.StepDir;
            if (radioButtonHz.Checked == true) scData.Step1_StepSpeed = ConvertHzToCycles(stepperControlSTEP1.StepSpeed);
            else scData.Step1_StepSpeed = stepperControlSTEP1.StepSpeed;

            scData.Step2_Enable = stepperControlSTEP2.StepEnabled;
            scData.Step2_DirState = stepperControlSTEP2.StepDir;
            if (radioButtonHz.Checked == true) scData.Step2_StepSpeed = ConvertHzToCycles(stepperControlSTEP2.StepSpeed);
            else scData.Step2_StepSpeed = stepperControlSTEP2.StepSpeed;

            scData.Step3_Enable = stepperControlSTEP3.StepEnabled;
            scData.Step3_DirState = stepperControlSTEP3.StepDir;
            if (radioButtonHz.Checked == true) scData.Step3_StepSpeed = ConvertHzToCycles(stepperControlSTEP3.StepSpeed);
            else scData.Step3_StepSpeed = stepperControlSTEP3.StepSpeed;

            scData.Step4_Enable = stepperControlSTEP4.StepEnabled;
            scData.Step4_DirState = stepperControlSTEP4.StepDir;
            if (radioButtonHz.Checked == true) scData.Step4_StepSpeed = ConvertHzToCycles(stepperControlSTEP4.StepSpeed);
            else scData.Step4_StepSpeed = stepperControlSTEP4.StepSpeed;

            scData.Step5_Enable = stepperControlSTEP5.StepEnabled;
            scData.Step5_DirState = stepperControlSTEP5.StepDir;
            if (radioButtonHz.Checked == true) scData.Step5_StepSpeed = ConvertHzToCycles(stepperControlSTEP5.StepSpeed);
            else scData.Step5_StepSpeed = stepperControlSTEP5.StepSpeed;

            // get the global enable
            if (checkBoxEnabledSTEPALL.Checked == true) scData.AllStep_Enable = 1;
            else scData.AllStep_Enable = 0;

            return scData;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles inbound data events.
        /// 
        /// NOTE: You are not on the Main Form Thread here.
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void ServerClientDataEventHandler(object sender, ServerClientData scData)
        {
            if(scData==null)
            {
                LogMessage("ServerClientDataEventHandler scData==null");
                return;
            }

            // Ok, you probably already know this but I'll note it here because this is so important
            // You do NOT want to update any form controls from a thread that is not the forms main
            // thread. Very odd, intermittent and hard to debug problems will result. Even if your 
            // handler does not actually update any form controls do not do it! Sooner or later you 
            // or someone else will make changes that calls something that eventually updates a
            // form or control and then you will have introduced a really hard to find bug.

            // So, we always use the InvokeRequired...Invoke sequence to get us back on the form thread
            if (InvokeRequired == true)
            {
                // call ourselves again but this time be on the form thread.
                Invoke(new TCPDataTransporter.ServerClientDataEvent_Delegate(ServerClientDataEventHandler), new object[] { sender, scData });
                return;
            }

            // Now we KNOW we are on the main form thread.

            // what type of data is it
            if (scData.DataContent == ServerClientDataContentEnum.USER_DATA)
            {
                // it is user defined data, log it
                LogMessage("ServerClientDataEventHandler dataStr=" + scData.DataStr);
                // display it
                AppendDataToTrace("IN: dataInt= dataStr=" + scData.DataStr);
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
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void ShutdownDataTransporter()
        {
            // shutdown the data transporter
            if (dataTransporter != null)
            {
                // are we connected? we want to tell the client to exit 
                if (IsConnected() == true)
                {
                    // get the server client data from the screen
                    ServerClientData scData = GetSCDataFromScreen("Client close down message");
                    // set a special flag in here
                    scData.AllStep_Enable = 2;

                    // display it
                    AppendDataToTrace("OUT: dataStr=" + scData.DataStr);
                    // send it
                    dataTransporter.SendData(scData);
                }

                dataTransporter.Shutdown();
                dataTransporter = null;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Detects if we have a connection. 
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private bool IsConnected()
        {
            if (dataTransporter == null) return false;
            if (dataTransporter.IsConnected() == false) return false;
            if (buttonSendData.Enabled == false) return false;
            return true;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets up the screen visuals based on the connections state
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void SetScreenVisualsBasedOnConnectionState(bool connectionState)
        {
            if(connectionState == true)
            {
                buttonSendData.Enabled = true;
            }
            else
            {
                buttonSendData.Enabled = false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Appends data to our data trace
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void AppendDataToTrace(string dataToAppend)
        {
            if ((dataToAppend == null) || (dataToAppend.Length == 0)) return;
            textBoxDataTrace.Text =  textBoxDataTrace.Text + "\r\n" + dataToAppend;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a check changed on the "All Steppers enabled box"
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void checkBoxEnabledSTEPALL_CheckedChanged(object sender, EventArgs e)
        {
            SyncAllStepperControlsToScreenState(checkBoxEnabledSTEPALL.Checked);
            // sends the data from the screen to the client
            if(checkBoxAutoSend.Checked == true) SendDataFromScreenToClient();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Synchronizes all stepper based controls to the screen state
        /// </summary>
        /// <param name="enableState">if true they are all enabled, false they are not</param>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void SyncAllStepperControlsToScreenState(bool enableState)
        {
            // set them now
            stepperControlSTEP0.Enabled = enableState;
            stepperControlSTEP1.Enabled = enableState;
            stepperControlSTEP2.Enabled = enableState;
            stepperControlSTEP3.Enabled = enableState;
            stepperControlSTEP4.Enabled = enableState;
            stepperControlSTEP5.Enabled = enableState;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sets the speed mode visuals on all stepper controls
        /// </summary>
        /// <param name="speedModeString">the text of the label</param>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void SetStepperSpeedModeVisuals(string speedModeString)
        {
            if (speedModeString == null) speedModeString = "unknown";
            stepperControlSTEP0.SpeedMode = speedModeString;
            stepperControlSTEP1.SpeedMode = speedModeString;
            stepperControlSTEP2.SpeedMode = speedModeString;
            stepperControlSTEP3.SpeedMode = speedModeString;
            stepperControlSTEP4.SpeedMode = speedModeString;
            stepperControlSTEP5.SpeedMode = speedModeString;
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a check changed on the "speed mode is cycles radio button"
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void radioButtonCycles_CheckedChanged(object sender, EventArgs e)
        {
            // we have to be changing to cycles
            if (radioButtonCycles.Checked == false) return;
            try
            {
                inhibitAutoSend = true;
                SetStepperSpeedModeVisuals("Cycles");
                stepperControlSTEP0.StepSpeed = ConvertHzToCycles(stepperControlSTEP0.StepSpeed);
                stepperControlSTEP1.StepSpeed = ConvertHzToCycles(stepperControlSTEP1.StepSpeed);
                stepperControlSTEP2.StepSpeed = ConvertHzToCycles(stepperControlSTEP2.StepSpeed);
                stepperControlSTEP3.StepSpeed = ConvertHzToCycles(stepperControlSTEP3.StepSpeed);
                stepperControlSTEP4.StepSpeed = ConvertHzToCycles(stepperControlSTEP4.StepSpeed);
                stepperControlSTEP5.StepSpeed = ConvertHzToCycles(stepperControlSTEP5.StepSpeed);
            }
            finally
            {
                inhibitAutoSend = false;
                // send the data now
                SendDataFromScreenToClient();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handles a check changed on the "speed mode is Hz radio button"
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void radioButtonHz_CheckedChanged(object sender, EventArgs e)
        {
            // we have to be changing to Hz
            if (radioButtonHz.Checked == false) return;
            try
            {
                inhibitAutoSend = true;
                SetStepperSpeedModeVisuals("Hertz");
                stepperControlSTEP0.StepSpeed = ConvertCyclesToHz(stepperControlSTEP0.StepSpeed);
                stepperControlSTEP1.StepSpeed = ConvertCyclesToHz(stepperControlSTEP1.StepSpeed);
                stepperControlSTEP2.StepSpeed = ConvertCyclesToHz(stepperControlSTEP2.StepSpeed);
                stepperControlSTEP3.StepSpeed = ConvertCyclesToHz(stepperControlSTEP3.StepSpeed);
                stepperControlSTEP4.StepSpeed = ConvertCyclesToHz(stepperControlSTEP4.StepSpeed);
                stepperControlSTEP5.StepSpeed = ConvertCyclesToHz(stepperControlSTEP5.StepSpeed);
            }
            finally
            {
                inhibitAutoSend = false;
                // send the data now
                SendDataFromScreenToClient();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts Cycles to Hz
        /// </summary>
        /// <param name="cycles">the number of cycles</param>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private uint ConvertCyclesToHz(uint cycles)
        {
            if (cycles == 0) return ConvertCyclesToHz(ServerClientData.DEFAULT_SPEED);
            return (uint)(1/(SECONDS_PER_CYCLE * ((double)cycles)));
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Converts Cycles to Hz
        /// </summary>
        /// <param name="hz">the number of Hz</param>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private uint ConvertHzToCycles(uint hz)
        {
            if (hz == 0) return ServerClientData.DEFAULT_SPEED;
            return (uint)(1 / (SECONDS_PER_CYCLE * ((double)hz)));
        }
    }
}

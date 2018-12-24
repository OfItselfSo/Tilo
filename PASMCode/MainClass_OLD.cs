using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using OISCommon;
using BBBCSIO;

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

/// NOTE: This is an older version of the MainClass class and is not used. It just
///       shows how to launch the various sample programs.

namespace TiloClient
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// The main class for the application
    /// </summary>
    /// <history>
    ///    19 Nov 18  Cynic - Started
    /// </history>
    public class MainClass : OISObjBase
    {
        private const string DEFAULTLOGDIR = @"/home/devuser/Dump/ProjectLogs";
        private const string APPLICATION_NAME = "TiloClient";
        private const string APPLICATION_VERSION = "01.00";

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public MainClass() 
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
        ///   19 Nov 18  Cynic - Started
        /// </history>
        public void BeginProcessing()
        {
            Console.WriteLine(APPLICATION_NAME + " processing begins");

            // PRUBlinkUSR3LED(PRUEnum.PRU_1, "./PRU1_SimpleTogglePin.bin");
            // PRUToggleFromConsole(PRUEnum.PRU_1, "./PRU1_TogglePinFromConsole.bin");
            // PRUToggleFromConsole(PRUEnum.PRU_1, "./PRU1_SquareWave.bin");
            // PRUToggleFromConsole(PRUEnum.PRU_1, "./PRU1_SquareWave2.bin");
            // PRUToggleFromConsole(PRUEnum.PRU_1, "./PRU1_StepperIO.bin");

            Console.WriteLine("Press Return");
            Console.ReadLine();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Sample code to launch various PASM binaries in a PRU.
        /// 
        /// </summary>
        /// <param name="pruID">The pruID</param>
        /// <param name="binaryToRun">The binary file to run</param>
        /// <history>
        ///    19 Nov 18  Cynic - Originally written
        /// </history>
        public void PRUToggleFromConsole(PRUEnum pruID, string binaryToRun)
        {
            uint tmpInt = 0;

            // ###
            // ### these are the offsets into the data store we pass into the PRU
            // ### each data item is a uint (it is simpler that way)
            // ###

            // our semaphore flag is stored at this offset
            const uint SEMAPHORE_OFFSET = 0;
            // the all steppers enabled flag is stored at this offset
            const uint ALLSTEP_ENABLE_OFFSET = 4;

            // STEP0
            const uint STEP0_ENABLED_OFFSET = 8;     // 0 disabled, 1 enabled
            const uint STEP0_FULLCOUNT = 12;         // this is the count we reset to when we toggle
            const uint STEP0_DIRSTATE = 16;          // this is the state of the direction pin

            // ###
            // ### this is the END of the data items, we need to allocate space for the 
            // ### above number of UINTS
            // ###
            const int NUM_DATA_UINTS = 5;


            // this is the array we use to pass in the data to the PRU. The
            // single byte will act as a toggle flag. Because we are only
            // transmitting a byte (an atomic value) we do not need to set
            // up any complicated semaphore system.

            // The size of this array is the number of 
            byte[] dataBytes = new byte[NUM_DATA_UINTS * sizeof(UInt32)];

            // sanity checks
            if (pruID == PRUEnum.PRU_NONE)
            {
                throw new Exception("No such PRU: " + pruID.ToString());
            }
            if ((binaryToRun == null) || (binaryToRun.Length == 0))
            {
                throw new Exception("Null or zero length binary file name specified");
            }

            // build the driver
            PRUDriver pruDriver = new PRUDriver(pruID);

            // initialize the dataBytes array. the PRU code expects to see a 
            // zero semaphore, and enable flags when it starts
            Array.Clear(dataBytes, 0, dataBytes.Length);

            // run the binary, pass in our initial array
            pruDriver.ExecutePRUProgram(binaryToRun, dataBytes);

            Console.WriteLine("Now toggling. Press 0, 1 or 2 to quit");

            // our outer loop we operate until the user quits
            while (true)
            {
                Console.WriteLine("Enter value: ");
                string inputStr = Console.ReadLine();
                try
                {
                    tmpInt = Convert.ToUInt32(inputStr);
                }
                catch (Exception)
                {
                    Console.WriteLine("The value " + inputStr + " is not an integer");
                    continue;
                }
                // write the allstep_enable flag
                pruDriver.WritePRUDataUInt32(tmpInt, ALLSTEP_ENABLE_OFFSET);

                // write the STEP0 enable/disable flag
                pruDriver.WritePRUDataUInt32(1, STEP0_ENABLED_OFFSET);
                // write the STEP0 fullcount value
                pruDriver.WritePRUDataUInt32(5000000, STEP0_FULLCOUNT);
                // write the STEP0 direction flag
                pruDriver.WritePRUDataUInt32(1, STEP0_DIRSTATE);

                // write the semaphore. This must come last
                pruDriver.WritePRUDataUInt32(1, SEMAPHORE_OFFSET);

                // wait for a bit,
                Thread.Sleep(100);

                if (tmpInt == 0)
                {
                    Console.WriteLine("Pin low");
                }
                else if (tmpInt == 1)
                {
                    Console.WriteLine("Pin high");
                }
                else
                {
                    // it is time to go
                    // tell the user
                    Console.WriteLine("Done ");
                    break;
                }
            }

            // close the driver, the code in the PRU remains running
            pruDriver.Dispose();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Configures a PRU and runs a compiled binary in it.
        /// </summary>
        /// <param name="pruID">The pruID</param>
        /// <param name="binaryToRun">The binary file to run</param>
        /// <history>
        ///    19 Nov 18  Cynic - Originally written
        /// </history>
        public void PRUBlinkUSR3LED(PRUEnum pruID, string binaryToRun)
        {
            // sanity checks
            if (pruID == PRUEnum.PRU_NONE)
            {
                throw new Exception("No such PRU: " + pruID.ToString());
            }
            if ((binaryToRun == null) || (binaryToRun.Length == 0))
            {
                throw new Exception("Null or zero length binary file name specified");
            }

            // build the driver
            PRUDriver pruDriver = new PRUDriver(pruID);
            // run the binary
            pruDriver.ExecutePRUProgram(binaryToRun);

            // close the driver, the code in the PRU remains running
            pruDriver.Dispose();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// This is where we handle exceptions
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
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

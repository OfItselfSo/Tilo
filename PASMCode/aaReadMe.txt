Extra PASM Code Files

 This directory contains sample PASM source code which were built during the development of the TiloClient project.
 Each demonstrates various techniques of interacting with the Beaglebone Black Programable Realtime Units (PRUs). 
 None of this code is now used (they were all temporary proof of concept idea type things) they have been left
 here in the thought that they may be useful as examples of PASM coding.

 The example .p files are written in the free and open source PASM Assembler and are extensively commented. 
 
 MainClass_OLD.cs       - an older version of the TiloClient MainClass which 
                          illustrates how to launch the compiled binarys in the PRU.

 PRU0_SimpleTogglePin.p - a PASM assembly language program, intended to run in
                           the Beaglebone Black PRU0 which will toggle on a 
                           header pin on and off a predetermined number of times
                           at a rate of 1 Hz
  
 PRU1_SimpleTogglePin.p - a PASM assembly language program, intended to run in
                           the Beaglebone Black PRU1 which will toggle on a 
                           header pin on and off a predetermined number of times
                           at a rate of 1 Hz
                           
 PRU1_SquareWave.p      - a PASM assembly language program, intended to run in
                           the Beaglebone Black PRU1 which will start and stop
                           sending a 1Hz square wave according to commands
                           sent to it from the console.
 
 PRU1_SquareWave2.p    - a PASM assembly language program, intended to run in
                          the Beaglebone Black PRU1 which will start and stop
                          sending an approx 1Hz square wave according to commands
                          sent to it from the console. Uses a different algorythm 
                          than PRU1_SquareWave.p
                          
 PRU1_TogglePinFromConsole.p - a PASM assembly language program, intended to run in
                          the Beaglebone Black PRU1 which will toggle on a 
                          header pin on and off using a flag from the console.
 
 
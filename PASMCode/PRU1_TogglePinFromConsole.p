/// +------------------------------------------------------------------------------------------------------------------------------+
/// |                                                   TERMS OF USE: MIT License                                                  |
/// +------------------------------------------------------------------------------------------------------------------------------|
/// |Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation    |
/// |files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy,    |
/// |modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software|
/// |is furnished to do so, subject to the following conditions:                                                                   |
/// |                                                                                                                              |
/// |The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.|
/// |                                                                                                                              |
/// |THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE          |
/// |WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR         |
/// |COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,   |
/// |ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.                         |
/// +------------------------------------------------------------------------------------------------------------------------------+

/// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
///  PRU1_TogglePinFromConsole.p 
///           - a PASM assembly language program, intended to run in
///             the Beaglebone Black PRU1 which will toggle on a 
///             header pin on and off using a flag from the console.
///
///                   Compile with the command
///                      pasm -b PRU1_TogglePinFromConsole.p
/// 
///               References to the PRM refer to the AM335x PRU-ICSS Reference Guide
///               References to the TRM refer to the AM335x Sitara Processors
///                   Technical Reference Manual
///            
///               History
///                   19 Nov 18  Cynic - Originally Written
///
///               Home Page
///                   http://www.OfItselfSo.com/Tilo/Tilo.php
/// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=

.origin 0
.entrypoint START

// this defines the data RAM memory location inside the PRUs address space. 
#define PRU_DATARAM 0        // yep, starts at address zero no matter which PRU you are using

#define DATAFLAG_OFFSET 0    // this is the offset of the data flag

        // this label is where the code execution starts
START:

        // Note we do NOT need to enable the OCP master port. This is because this 
        // code does not write directly out to the BBB's userspace memory. It writes 
        // to its own data RAM and this is presented to userspace as a MemoryMapped 
		// file by the UIO driver.

CHKPIN: MOV       R3, PRU_DATARAM           // put the address of our 8Kb DataRAM space in R3
        MOV       R0, 0                     // clear R0
        LBBO      R0.b0, R3, DATAFLAG_OFFSET, 1    // our flag byte is at offset 0, we read in that 
        QBEQ      PINLOW, R0, 0             // if R0 = 0 pin goes off
        QBEQ      PINHIGH, R0, 1            // if R0 = 1 pin goes on
		 
		// anything else, we put the pin low and stop
		CLR r30.t0                          // R30 bit 3 (header 8 pin 45) goes low
        HALT                                // stop the PRU

		// put the pin low
PINLOW: CLR r30.t0                          // R30 bit 3 (header 8 pin 45) goes low
        JMP CHKPIN                          // check again

		// put the pin high
PINHIGH:SET r30.t0                          // R30 bit 3 (header 8 pin 45) goes high
        JMP CHKPIN                          // check again

        HALT                                // stop the PRU

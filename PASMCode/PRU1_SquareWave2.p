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
///  PRU1_SquareWave2.p - a PASM assembly language program, intended to run in
///                       the Beaglebone Black PRU1 which will start and stop
///                       sending an approx 1Hz square wave according to commands
///                       sent to it from the console.
///
///                       Uses a different algorythm than PRU1_SquareWave.p
///
///                   Compile with the command
///                      pasm -b PRU1_SquareWave2.p
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

#define DEFAULT_STARTCOUNT 5000000 // should not be zero
#define DEFAULT_STARTSTATE 0 
#define DEFAULT_ENABLEDSTATE 0

        // Note we do NOT need to enable the OCP master port. This is because this 
        // code does not write directly out to the BBB's userspace memory. It writes 
        // to its own data RAM and this is presented to userspace as a MemoryMapped 
        // file by the UIO driver.

        // this label is where the code execution starts
START:

        // stepper 0: we use R10 as the down counter,  R20 as the storage for the full count
        // stepper 1: we use R11 as the down counter,  R21 as the storage for the full count
        // and so on ..

#define STEPPER0_OUTPUTREG r30.t0      
#define STEPPER0_DOWNCOUNT R10    
#define STEPPER0_ENABLED   R15    
#define STEPPER0_FULLCOUNT R20    
#define STEPPER0_STATE     R25    

INIT:        MOV  STEPPER0_FULLCOUNT, DEFAULT_STARTCOUNT
             MOV  STEPPER0_DOWNCOUNT, STEPPER0_FULLCOUNT
             MOV  STEPPER0_STATE, DEFAULT_STARTSTATE
             MOV  STEPPER0_ENABLED, DEFAULT_ENABLEDSTATE

             // perform our decrements, we test later
DECCNT:      SUB  STEPPER0_DOWNCOUNT, STEPPER0_DOWNCOUNT, 1

// there is one block below for each Stepper motor. The code is
// structured in such a way that no matter which path is taken
// through it, the processing takes 10 instructions. This keeps 
// things consistent. The various steppers can have different
// frequencies, be enabled or disabled, and the processing time
// is identical

             // now perform Stepper0 specific actions
STEP0:       QBNE STEP0_NOP7, STEPPER0_DOWNCOUNT, 0      // is STEPPER0_DOWNCOUNT == 0? if no, then goto STEP0_NOP6
             MOV  STEPPER0_DOWNCOUNT, STEPPER0_FULLCOUNT // reset the count now
             QBEQ STEP0_TOGG, STEPPER0_ENABLED, 1        // is STEPPER0_ENABLED == 1? if yes, then goto STEP0_TOGG    
             CLR  STEPPER0_OUTPUTREG                     // not enabled, clear the pin 
			 JMP  STEP0_NOP3                             // leave now
STEP0_TOGG:  QBEQ STEP0_HIGH, STEPPER0_STATE, 0          // is the pin state low == 0? if yes, then goto STEP0_HIGH
STEP0_LOW:   CLR  STEPPER0_OUTPUTREG                     // clear the pin 
             MOV  STEPPER0_STATE, 0                      // remember this
             JMP  STEP0_NOP1
STEP0_HIGH:  SET  STEPPER0_OUTPUTREG                     // set the pin 
             MOV  STEPPER0_STATE, 1                      // remember this
             JMP  STEP0_NOP1
STEP0_NOP7:  MOV  R0, R0                                 // just a NOP
STEP0_NOP6:  MOV  R0, R0                                 // just a NOP
STEP0_NOP5:  MOV  R0, R0                                 // just a NOP
STEP0_NOP4:  MOV  R0, R0                                 // just a NOP
STEP0_NOP3:  MOV  R0, R0                                 // just a NOP
STEP0_NOP2:  MOV  R0, R0                                 // just a NOP
STEP0_NOP1:  MOV  R0, R0                                 // just a NOP
STEP0_NOP0:  MOV  R0, R0                                 // just a NOP
STEP0_DONE:  MOV  R0, R0                                 // just a NOP


CHKPIN: MOV       R3, PRU_DATARAM                        // put the address of our 8Kb DataRAM space in R3
        MOV       STEPPER0_ENABLED, 0                    // clear STEPPER0_ENABLED
        LBBO      STEPPER0_ENABLED.b0, R3, DATAFLAG_OFFSET, 1    // our flag byte is at offset 0, we read in that 
        QBEQ      DECCNT, STEPPER0_ENABLED, 1            // if R0 = 1 stepper enabled, resume counting
        QBEQ      DECCNT, STEPPER0_ENABLED, 0            // if R0 = 0 stepper disabled, resume counting
         
        // anything else, we put the pin low and stop
        CLR STEPPER0_OUTPUTREG                           // clear the pin
        HALT                                             // stop the PRU

/*
        // put the pin low
PINLOW: CLR STEPPER0_OUTPUTREG                          // R30 bit 3 (header 8 pin 45) goes low
        JMP CHKPIN                          // check again

TOGPIN: MOV R1, 50000000                    // set up for a half second delay

        // perform a half second delay
L1:     SUB R1, R1, 1                       // subtract 1 from R1
        QBNE L1, R1, 0                      // is R1 == 0? if no, then goto L1

        // now we turn the header pin on
        SET STEPPER0_OUTPUTREG                         // R30 bit 0 (header 8 pin 45) goes high

        // perform a half second delay. Note the delay works because of the determinate nature
        // of the execution times of PRU instructions. Instructions that do not reference data
        // RAM take 5ns. We have two instructions in our loop (the SUB and the QBNE). If you do the
        // math on that you find that a value of 50000000 gives you a half second delay. Of course
        // we are ignoring the time taken by all the other instructions because the small amount of
        // time they take does not add much error. One can conceive of applications where one might
        // have to take note of such things and compensate.

        MOV R1, 50000000                    // set up for a half second delay
L2:     SUB R1, R1, 1                       // subtract 1 from R1
        QBNE L2, R1, 0                      // is R1 == 0? if no, then goto L2

        // now we turn the header pin off again
        CLR STEPPER0_OUTPUTREG                         // R30 bit 3 (header 8 pin 45) goes low

        // the bottom of the TOGPIN loop. Just check the pin again
        JMP CHKPIN

        HALT                                // stop the PRU
        */
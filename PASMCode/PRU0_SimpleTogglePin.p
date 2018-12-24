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
///  PRU0_SimpleTogglePin.p - a PASM assembly language program, intended to run in
///                           the Beaglebone Black PRU0 which will toggle on a 
///                           header pin on and off a predetermined number of times
///                           at a rate of 1 Hz
///
///                   Compile with the command
///                      pasm -b PRU0_SimpleTogglePin.p
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

// the number of times we toggle the pin
#define NUMBER_OF_BLINKS 10

        // this label is where the code execution starts
START:

        MOV r0, NUMBER_OF_BLINKS            // this is the number of times we wish to blink          

        // there are three loops in this code. The outermost loop defined by the BLINK
        // label executes once for each on/off cycle of the pin. L1 and L2 are half second
        // delay loops

BLINK:  MOV R1, 50000000                    // set up for a half second delay

        // perform a half second delay
L1:     SUB R1, R1, 1                       // subtract 1 from R1
        QBNE L1, R1, 0                      // is R1 == 0? if no, then goto L1

        // now we turn the header pin on
        SET r30.t3                         // R30 bit 3 (header 9 pin 28) goes high

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
        CLR r30.t3                         // R30 bit 3 (header 9 pin 28) goes low

        // the bottom of the BLINK loop. Note that R0 contains the number of remaining blinks. It
        // is UP TO YOU to remember that and not use R0 for something else in the code above. There 
        // is no safety net in assembler :-)
        SUB R0, R0, 1
        QBNE BLINK, R0, 0

        // if we get here we have blinked the led as many times as we need to. 

        HALT                                // stop the PRU

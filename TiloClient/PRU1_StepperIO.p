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
///  PRU1_StepperIO.p - a PASM assembly language program, intended to run in
///                     the Beaglebone Black PRU1 which will send strictly
///                     defined waveforms and direction signals to up to 
///                     6 stepper motors.
///
///                     This code uses almost all of the I/O capabilities of the
///                     PRU1 and it must be run in a headless environment due
///                     to PimMux collisions with the output lines.
///
///                     NOTE: you must set the following pins to output
///                         for the signal and stepper direction lines
///                         P8_46, P8_45, P8_43, P8_44, P8_41, P8_42, 
///                         P8_39, P8_40, P8_27, P8_28, P8_29, P8_30
///
///                     Below is a subsection of the relevant device tree overlay
///                     
///                     0x0A0 0x25  /* P8_45 70   OUTPUT MODE5 - pr1_pru1_pru_r30_0 */
///                     0x0A4 0x25  /* P8_46 71   OUTPUT MODE5 - pr1_pru1_pru_r30_1 */
///                     0x0A8 0x25  /* P8_43 72   OUTPUT MODE5 - pr1_pru1_pru_r30_2 */
///                     0x0AC 0x25  /* P8_44 73   OUTPUT MODE5 - pr1_pru1_pru_r30_3 */
///                     0x0B0 0x25  /* P8_41 74   OUTPUT MODE5 - pr1_pru1_pru_r30_4 */
///                     0x0B4 0x25  /* P8_42 75   OUTPUT MODE5 - pr1_pru1_pru_r30_5 */
///                     0x0B8 0x25  /* P8_39 74   OUTPUT MODE5 - pr1_pru1_pru_r30_6 */
///                     0x0BC 0x25  /* P8_40 75   OUTPUT MODE5 - pr1_pru1_pru_r30_7 */
///                     0x0E0 0x25  /* P8_27 86   OUTPUT MODE5 - pr1_pru1_pru_r30_8 */
///                     0x0E8 0x25  /* P8_28 88   OUTPUT MODE5 - pr1_pru1_pru_r30_10 */
///                     0x0E4 0x25  /* P8_29 87   OUTPUT MODE5 - pr1_pru1_pru_r30_9 */
///                     0x0EC 0x25  /* P8_30 89   OUTPUT MODE5 - pr1_pru1_pru_r30_11 */

///                     The general mode of operation is to setup a series
///                     six blocks (one for each stepper) in which the path
///                     through the block, no matter which branches are taken,
///                     always takes exactly 10 statement executions. On the PRU's 
///                     each statement, as long as it does not access system
///                     memory, takes the same amount of time (5ns).
///
///                     This consistent timing means that various stepper motors
///                     can be enabled or disabled or have wildly varying pulse
///                     frequencies without any effect on the pulse widths of the
///                     other stepper motors.
///
///                     In order to keep things simple, we use the registers as
///                     variables. We do not have a lot of data to store and 
///                     this helps keeps the timings consistent too.
///
///                     Each stepper has an enable flag (0 or 1), a timing count
///                     which directly correlates to the frequency, and a 
///                     direction flag. There is also a global flag which turns 
///                     off all stepper motors and can generate a HALT on the PRU.
///
///                   Compile with the command
///                      pasm -b PRU1_StepperIO.p
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
#define BYTES_IN_REG 4       // there are 4 bytes in a register. This is only
                             // defined as a constant so its use is obviious
                             // in the BYTES_IN_CLIENT_DATA (and other) values below

#define DEFAULT_STARTCOUNT 5000000 // should not (or ever) be zero
#define DEFAULT_STARTSTATE 0 
#define DEFAULT_ENABLEDSTATE 0

// Note we do NOT need to enable the OCP master port. This is because this 
// code does not read directly out to the BBB's userspace memory. It reads 
// from its own data RAM and this is presented to userspace as a MemoryMapped 
// file by the UIO driver.

// these define the pins we update for the various outputs

#define STEP0_OUTPUTREG r30.t0           // P8_45 the bit in R30 we toggle to set the state
#define STEP0_DIRREG    r30.t1           // P8_46 the bit in R30 we toggle to set the direction
#define STEP1_OUTPUTREG r30.t2           // P8_43 the bit in R30 we toggle to set the state
#define STEP1_DIRREG    r30.t3           // P8_44 the bit in R30 we toggle to set the direction
#define STEP2_OUTPUTREG r30.t4           // P8_41 the bit in R30 we toggle to set the state
#define STEP2_DIRREG    r30.t5           // P8_42 the bit in R30 we toggle to set the direction
#define STEP3_OUTPUTREG r30.t6           // P8_39 the bit in R30 we toggle to set the state
#define STEP3_DIRREG    r30.t7           // P8_40 the bit in R30 we toggle to set the direction
#define STEP4_OUTPUTREG r30.t9           // P8_29 the bit in R30 we toggle to set the state
#define STEP4_DIRREG    r30.t11          // P8_30 the bit in R30 we toggle to set the direction
#define STEP5_OUTPUTREG r30.t8           // P8_27 the bit in R30 we toggle to set the state
#define STEP5_DIRREG    r30.t10          // P8_28 the bit in R30 we toggle to set the direction

// these registers are used as temporary variables and the code updates them dynamically

#define STEP0_DOWNCOUNT R4              // downcounter, 0 means toggle line state
#define STEP1_DOWNCOUNT R5              // downcounter, 0 means toggle line state
#define STEP2_DOWNCOUNT R6              // downcounter, 0 means toggle line state
#define STEP3_DOWNCOUNT R7              // downcounter, 0 means toggle line state
#define STEP4_DOWNCOUNT R8              // downcounter, 0 means toggle line state
#define STEP5_DOWNCOUNT R9              // downcounter, 0 means toggle line state

// the content of these registers is obtained from the client and, other than the semaphore,
// this code does not update them once they have been read. Think of them as temporary constants
// NOTE: the registers below MUST be sequential. They are loaded as a block. You can NEVER
// use the registers R30 and above

#define SEMAPHORE_OFFSET 0               // this is the offset of the semaphore data in the register
#define SEMAPHORE_REG   R10              // 0 no data to read, 1 data can be read
                                         // this is the last byte set by the client
                                         // when it has updated the freq and dir data for the steppers
#define STEPALL_ENABLED R11              // 0 all steppers disabled, 1 steppers can be enabled
                                         // anything other than 0 or 1 means clear all outputs
                                         // clear all dir pins and HALT the PRU
#define STEP0_ENABLED   R12              // 0 disabled, 1 enabled
#define STEP0_FULLCOUNT R13              // this is the count we reset to when we toggle
#define STEP0_DIRSTATE  R14              // this is the state of the direction pin
#define STEP1_ENABLED   R15              // 0 disabled, 1 enabled
#define STEP1_FULLCOUNT R16              // this is the count we reset to when we toggle
#define STEP1_DIRSTATE  R17              // this is the state of the direction pin
#define STEP2_ENABLED   R18              // 0 disabled, 1 enabled
#define STEP2_FULLCOUNT R19              // this is the count we reset to when we toggle
#define STEP2_DIRSTATE  R20              // this is the state of the direction pin
#define STEP3_ENABLED   R21              // 0 disabled, 1 enabled
#define STEP3_FULLCOUNT R22              // this is the count we reset to when we toggle
#define STEP3_DIRSTATE  R23              // this is the state of the direction pin
#define STEP4_ENABLED   R24              // 0 disabled, 1 enabled
#define STEP4_FULLCOUNT R25              // this is the count we reset to when we toggle
#define STEP4_DIRSTATE  R26              // this is the state of the direction pin
#define STEP5_ENABLED   R27              // 0 disabled, 1 enabled
#define STEP5_FULLCOUNT R28              // this is the count we reset to when we toggle
#define STEP5_DIRSTATE  R29              // this is the state of the direction pin

// this is the sum of the count of the bytes in all of the registers from SEMAPHORE_OFFSET to STEP6_DIRSTATE
#define BYTES_IN_CLIENT_DATA (20 * BYTES_IN_REG)   // the total number of bytes in the client data 

             // this label is where the code execution starts
START:

             // initialize
INIT:        CLR  STEP0_OUTPUTREG
             MOV  STEP0_ENABLED,   DEFAULT_ENABLEDSTATE
             MOV  STEP0_FULLCOUNT, DEFAULT_STARTCOUNT
             MOV  STEP0_DOWNCOUNT, STEP0_FULLCOUNT

			 CLR  STEP1_OUTPUTREG
             MOV  STEP1_ENABLED,   DEFAULT_ENABLEDSTATE
             MOV  STEP1_FULLCOUNT, DEFAULT_STARTCOUNT
             MOV  STEP1_DOWNCOUNT, STEP1_FULLCOUNT

			 CLR  STEP2_OUTPUTREG
             MOV  STEP2_ENABLED,   DEFAULT_ENABLEDSTATE
             MOV  STEP2_FULLCOUNT, DEFAULT_STARTCOUNT
             MOV  STEP2_DOWNCOUNT, STEP2_FULLCOUNT

			 CLR  STEP3_OUTPUTREG
             MOV  STEP3_ENABLED,   DEFAULT_ENABLEDSTATE
             MOV  STEP3_FULLCOUNT, DEFAULT_STARTCOUNT
             MOV  STEP3_DOWNCOUNT, STEP3_FULLCOUNT

			 CLR  STEP4_OUTPUTREG
             MOV  STEP4_ENABLED,   DEFAULT_ENABLEDSTATE
             MOV  STEP4_FULLCOUNT, DEFAULT_STARTCOUNT
             MOV  STEP4_DOWNCOUNT, STEP4_FULLCOUNT

			 CLR  STEP5_OUTPUTREG
             MOV  STEP5_ENABLED,   DEFAULT_ENABLEDSTATE
             MOV  STEP5_FULLCOUNT, DEFAULT_STARTCOUNT
             MOV  STEP5_DOWNCOUNT, STEP5_FULLCOUNT

       // The top of the loop
LOOP_TOP:      

// there is one block below for each Stepper motor. The code is
// structured in such a way that no matter which path is taken
// through it, the processing takes 10 instructions. This keeps 
// timings consistent. The various steppers can have different
// frequencies, be enabled or disabled, and the processing time
// through the block identical for each

             // #######
             // ####### STEPPER 0 specific actions
             // #######
STEP0:       QBEQ STEP0_TEST, STEP0_ENABLED, 1        // is STEP0_ENABLED == 1? if yes, then toggle    
             CLR  STEP0_OUTPUTREG                     // not enabled, clear the pin 
             JMP  STEP0_NOP6                          
STEP0_TEST:  SUB  STEP0_DOWNCOUNT, STEP0_DOWNCOUNT, 1 // decrement the count
             QBNE STEP0_NOP6, STEP0_DOWNCOUNT, 0      // is the downcount == 0? if no, then NOPout
             MOV  STEP0_DOWNCOUNT, STEP0_FULLCOUNT    // reset the count now
STEP0_TOGG:  QBBC STEP0_HIGH, STEP0_OUTPUTREG         // we need to toggle, are we currently high?
STEP0_LOW:   CLR  STEP0_OUTPUTREG                     // clear the pin 
             JMP  STEP0_NOP2
STEP0_HIGH:  SET  STEP0_OUTPUTREG                     // set the pin 
             JMP  STEP0_NOP2
STEP0_NOP7:  MOV  R0, R0                              // just a NOP
STEP0_NOP6:  MOV  R0, R0                              // just a NOP
STEP0_NOP5:  MOV  R0, R0                              // just a NOP
STEP0_NOP4:  MOV  R0, R0                              // just a NOP
STEP0_NOP3:  MOV  R0, R0                              // just a NOP
STEP0_NOP2:  MOV  R0, R0                              // just a NOP
STEP0_NOP1:  MOV  R0, R0                              // just a NOP
STEP0_NOP0:  MOV  R0, R0                              // just a NOP

             // #######
             // ####### STEPPER 1 specific actions
             // #######
STEP1:       QBEQ STEP1_TEST, STEP1_ENABLED, 1        // is STEP1_ENABLED == 1? if yes, then toggle    
             CLR  STEP1_OUTPUTREG                     // not enabled, clear the pin 
             JMP  STEP1_NOP6                          
STEP1_TEST:  SUB  STEP1_DOWNCOUNT, STEP1_DOWNCOUNT, 1 // decrement the count
             QBNE STEP1_NOP6, STEP1_DOWNCOUNT, 0      // is the downcount == 0? if no, then NOPout
             MOV  STEP1_DOWNCOUNT, STEP1_FULLCOUNT    // reset the count now
STEP1_TOGG:  QBBC STEP1_HIGH, STEP1_OUTPUTREG         // we need to toggle, are we currently high?
STEP1_LOW:   CLR  STEP1_OUTPUTREG                     // clear the pin 
             JMP  STEP1_NOP2
STEP1_HIGH:  SET  STEP1_OUTPUTREG                     // set the pin 
             JMP  STEP1_NOP2
STEP1_NOP7:  MOV  R0, R0                              // just a NOP
STEP1_NOP6:  MOV  R0, R0                              // just a NOP
STEP1_NOP5:  MOV  R0, R0                              // just a NOP
STEP1_NOP4:  MOV  R0, R0                              // just a NOP
STEP1_NOP3:  MOV  R0, R0                              // just a NOP
STEP1_NOP2:  MOV  R0, R0                              // just a NOP
STEP1_NOP1:  MOV  R0, R0                              // just a NOP
STEP1_NOP0:  MOV  R0, R0                              // just a NOP

             // #######
             // ####### STEPPER 2 specific actions
             // #######
STEP2:       QBEQ STEP2_TEST, STEP2_ENABLED, 1        // is STEP2_ENABLED == 1? if yes, then toggle    
             CLR  STEP2_OUTPUTREG                     // not enabled, clear the pin 
             JMP  STEP2_NOP6                          
STEP2_TEST:  SUB  STEP2_DOWNCOUNT, STEP2_DOWNCOUNT, 1 // decrement the count
             QBNE STEP2_NOP6, STEP2_DOWNCOUNT, 0      // is the downcount == 0? if no, then NOPout
             MOV  STEP2_DOWNCOUNT, STEP2_FULLCOUNT    // reset the count now
STEP2_TOGG:  QBBC STEP2_HIGH, STEP2_OUTPUTREG         // we need to toggle, are we currently high?
STEP2_LOW:   CLR  STEP2_OUTPUTREG                     // clear the pin 
             JMP  STEP2_NOP2
STEP2_HIGH:  SET  STEP2_OUTPUTREG                     // set the pin 
             JMP  STEP2_NOP2
STEP2_NOP7:  MOV  R0, R0                              // just a NOP
STEP2_NOP6:  MOV  R0, R0                              // just a NOP
STEP2_NOP5:  MOV  R0, R0                              // just a NOP
STEP2_NOP4:  MOV  R0, R0                              // just a NOP
STEP2_NOP3:  MOV  R0, R0                              // just a NOP
STEP2_NOP2:  MOV  R0, R0                              // just a NOP
STEP2_NOP1:  MOV  R0, R0                              // just a NOP
STEP2_NOP0:  MOV  R0, R0                              // just a NOP

             // #######
             // ####### STEPPER 3 specific actions
             // #######
STEP3:       QBEQ STEP3_TEST, STEP3_ENABLED, 1        // is STEP3_ENABLED == 1? if yes, then toggle    
             CLR  STEP3_OUTPUTREG                     // not enabled, clear the pin 
             JMP  STEP3_NOP6                          
STEP3_TEST:  SUB  STEP3_DOWNCOUNT, STEP3_DOWNCOUNT, 1 // decrement the count
             QBNE STEP3_NOP6, STEP3_DOWNCOUNT, 0      // is the downcount == 0? if no, then NOPout
             MOV  STEP3_DOWNCOUNT, STEP3_FULLCOUNT    // reset the count now
STEP3_TOGG:  QBBC STEP3_HIGH, STEP3_OUTPUTREG         // we need to toggle, are we currently high?
STEP3_LOW:   CLR  STEP3_OUTPUTREG                     // clear the pin 
             JMP  STEP3_NOP2
STEP3_HIGH:  SET  STEP3_OUTPUTREG                     // set the pin 
             JMP  STEP3_NOP2
STEP3_NOP7:  MOV  R0, R0                              // just a NOP
STEP3_NOP6:  MOV  R0, R0                              // just a NOP
STEP3_NOP5:  MOV  R0, R0                              // just a NOP
STEP3_NOP4:  MOV  R0, R0                              // just a NOP
STEP3_NOP3:  MOV  R0, R0                              // just a NOP
STEP3_NOP2:  MOV  R0, R0                              // just a NOP
STEP3_NOP1:  MOV  R0, R0                              // just a NOP
STEP3_NOP0:  MOV  R0, R0                              // just a NOP

             // #######
             // ####### STEPPER 4 specific actions
             // #######
STEP4:       QBEQ STEP4_TEST, STEP4_ENABLED, 1        // is STEP4_ENABLED == 1? if yes, then toggle    
             CLR  STEP4_OUTPUTREG                     // not enabled, clear the pin 
             JMP  STEP4_NOP6                          
STEP4_TEST:  SUB  STEP4_DOWNCOUNT, STEP4_DOWNCOUNT, 1 // decrement the count
             QBNE STEP4_NOP6, STEP4_DOWNCOUNT, 0      // is the downcount == 0? if no, then NOPout
             MOV  STEP4_DOWNCOUNT, STEP4_FULLCOUNT    // reset the count now
STEP4_TOGG:  QBBC STEP4_HIGH, STEP4_OUTPUTREG         // we need to toggle, are we currently high?
STEP4_LOW:   CLR  STEP4_OUTPUTREG                     // clear the pin 
             JMP  STEP4_NOP2
STEP4_HIGH:  SET  STEP4_OUTPUTREG                     // set the pin 
             JMP  STEP4_NOP2
STEP4_NOP7:  MOV  R0, R0                              // just a NOP
STEP4_NOP6:  MOV  R0, R0                              // just a NOP
STEP4_NOP5:  MOV  R0, R0                              // just a NOP
STEP4_NOP4:  MOV  R0, R0                              // just a NOP
STEP4_NOP3:  MOV  R0, R0                              // just a NOP
STEP4_NOP2:  MOV  R0, R0                              // just a NOP
STEP4_NOP1:  MOV  R0, R0                              // just a NOP
STEP4_NOP0:  MOV  R0, R0                              // just a NOP

             // #######
             // ####### STEPPER 5 specific actions
             // #######
STEP5:       QBEQ STEP5_TEST, STEP5_ENABLED, 1        // is STEP5_ENABLED == 1? if yes, then toggle    
             CLR  STEP5_OUTPUTREG                     // not enabled, clear the pin 
             JMP  STEP5_NOP6                          
STEP5_TEST:  SUB  STEP5_DOWNCOUNT, STEP5_DOWNCOUNT, 1 // decrement the count
             QBNE STEP5_NOP6, STEP5_DOWNCOUNT, 0      // is the downcount == 0? if no, then NOPout
             MOV  STEP5_DOWNCOUNT, STEP5_FULLCOUNT    // reset the count now
STEP5_TOGG:  QBBC STEP5_HIGH, STEP5_OUTPUTREG         // we need to toggle, are we currently high?
STEP5_LOW:   CLR  STEP5_OUTPUTREG                     // clear the pin 
             JMP  STEP5_NOP2
STEP5_HIGH:  SET  STEP5_OUTPUTREG                     // set the pin 
             JMP  STEP5_NOP2
STEP5_NOP7:  MOV  R0, R0                              // just a NOP
STEP5_NOP6:  MOV  R0, R0                              // just a NOP
STEP5_NOP5:  MOV  R0, R0                              // just a NOP
STEP5_NOP4:  MOV  R0, R0                              // just a NOP
STEP5_NOP3:  MOV  R0, R0                              // just a NOP
STEP5_NOP2:  MOV  R0, R0                              // just a NOP
STEP5_NOP1:  MOV  R0, R0                              // just a NOP
STEP5_NOP0:  MOV  R0, R0                              // just a NOP

// this section obtains the data from the Tilo Client, and places it in the
// registers for use. The overhead of this is consistent and will
// not affect the frequency of the steppers since the timings are calibrated
// with it in place

CHKPIN:      MOV  R3, PRU_DATARAM                     // put the address of our 8Kb DataRAM space in R3
             MOV  SEMAPHORE_REG, 0                    // reset this
             LBBO SEMAPHORE_REG, R3, SEMAPHORE_OFFSET, BYTES_IN_REG    // read in just the semaphore
             QBNE LOOP_TOP, SEMAPHORE_REG, 1          // is the semaphore set? if not, no new data, 
                                                      // carry on processing with what we have
                                                      
             // else, read in all of the client data
             LBBO SEMAPHORE_REG, R3, SEMAPHORE_OFFSET, BYTES_IN_CLIENT_DATA    // read in our client data
                                                                               // this writes to multiple
                                                                               // contiguous registers
             MOV  SEMAPHORE_REG, 0                    // reset the semaphore reg
             SBBO SEMAPHORE_REG, R3, SEMAPHORE_OFFSET, BYTES_IN_REG    // reset the semaphore in memory
             QBEQ TEST_COUNT, STEPALL_ENABLED, 1      // all steppers enabled, processing can proceed
             QBEQ ALLLOW, STEPALL_ENABLED, 0          // not 0 or 1? this means exit
			 JMP  ALLSTOP

	    // here we test the full counts, they can never be zero, we do not permit this
TEST_COUNT: QBNE STEP0_FCOK, STEP0_FULLCOUNT, 0       // is the fullcount 0?
            MOV  STEP0_ENABLED, 0                     // full count sent in as zero? disable stepper
STEP0_FCOK:                                           // no worries, full count is acceptable
            QBNE STEP1_FCOK, STEP1_FULLCOUNT, 0       // is the fullcount 0?
            MOV  STEP1_ENABLED, 0                     // full count sent in as zero? disable stepper
STEP1_FCOK:                                           // no worries, full count is acceptable
            QBNE STEP2_FCOK, STEP2_FULLCOUNT, 0       // is the fullcount 0?
            MOV  STEP2_ENABLED, 0                     // full count sent in as zero? disable stepper
STEP2_FCOK:                                           // no worries, full count is acceptable
            QBNE STEP3_FCOK, STEP3_FULLCOUNT, 0       // is the fullcount 0?
            MOV  STEP3_ENABLED, 0                     // full count sent in as zero? disable stepper
STEP3_FCOK:                                           // no worries, full count is acceptable
            QBNE STEP4_FCOK, STEP4_FULLCOUNT, 0       // is the fullcount 0?
            MOV  STEP4_ENABLED, 0                     // full count sent in as zero? disable stepper
STEP4_FCOK:                                           // no worries, full count is acceptable
            QBNE STEP5_FCOK, STEP5_FULLCOUNT, 0       // is the fullcount 0?
            MOV  STEP5_ENABLED, 0                     // full count sent in as zero? disable stepper
STEP5_FCOK:                                           // no worries, full count is acceptable


        // here we test the current down count is not greater than the new full count
		// this can happen if the user increases the speed suddenly, we do not want
		// to have to wait for the current cycle to complete before the new count kicks in
            QBLT STEP0_DCOK, STEP0_FULLCOUNT, STEP0_DOWNCOUNT    // is the downcount < fullcount?
            MOV  STEP0_DOWNCOUNT, STEP0_FULLCOUNT     // reset the downcount to the new maximum
STEP0_DCOK:                                           // no worries, down count is acceptable
            QBLT STEP1_DCOK, STEP1_FULLCOUNT, STEP1_DOWNCOUNT    // is the downcount < fullcount?
            MOV  STEP1_DOWNCOUNT, STEP1_FULLCOUNT     // reset the downcount to the new maximum
STEP1_DCOK:                                           // no worries, down count is acceptable
            QBLT STEP2_DCOK, STEP2_FULLCOUNT, STEP2_DOWNCOUNT    // is the downcount < fullcount?
            MOV  STEP2_DOWNCOUNT, STEP2_FULLCOUNT     // reset the downcount to the new maximum
STEP2_DCOK:                                           // no worries, down count is acceptable
            QBLT STEP3_DCOK, STEP3_FULLCOUNT, STEP3_DOWNCOUNT    // is the downcount < fullcount?
            MOV  STEP3_DOWNCOUNT, STEP3_FULLCOUNT     // reset the downcount to the new maximum
STEP3_DCOK:                                           // no worries, down count is acceptable
            QBLT STEP4_DCOK, STEP4_FULLCOUNT, STEP4_DOWNCOUNT    // is the downcount < fullcount?
            MOV  STEP4_DOWNCOUNT, STEP4_FULLCOUNT     // reset the downcount to the new maximum
STEP4_DCOK:                                           // no worries, down count is acceptable
            QBLT STEP5_DCOK, STEP5_FULLCOUNT, STEP5_DOWNCOUNT    // is the downcount < fullcount?
            MOV  STEP5_DOWNCOUNT, STEP5_FULLCOUNT     // reset the downcount to the new maximum
STEP5_DCOK:                                           // no worries, down count is acceptable

        // here we set the direction pins
STEP0_SDIR:  QBEQ STEP0_DLOW, STEP0_ENABLED, 0        // not enabled, set it low    
             QBEQ STEP0_DLOW, STEP0_DIRSTATE, 0       // what does the dir state say?
             SET  STEP0_DIRREG                        // it is nz, set the pin
			 JMP  STEP0_DEND
STEP0_DLOW:  CLR  STEP0_DIRREG                        // not enabled, clear the pin 
STEP0_DEND:                                           // the end of the STEP0 direction pin 

STEP1_SDIR:  QBEQ STEP1_DLOW, STEP1_ENABLED, 0        // not enabled, set it low    
             QBEQ STEP1_DLOW, STEP1_DIRSTATE, 0       // what does the dir state say?
             SET  STEP1_DIRREG                        // it is nz, set the pin
			 JMP  STEP1_DEND
STEP1_DLOW:  CLR  STEP1_DIRREG                        // not enabled, clear the pin 
STEP1_DEND:                                           // the end of the STEP1 direction pin 

STEP2_SDIR:  QBEQ STEP2_DLOW, STEP2_ENABLED, 0        // not enabled, set it low    
             QBEQ STEP2_DLOW, STEP2_DIRSTATE, 0       // what does the dir state say?
             SET  STEP2_DIRREG                        // it is nz, set the pin
			 JMP  STEP2_DEND
STEP2_DLOW:  CLR  STEP2_DIRREG                        // not enabled, clear the pin 
STEP2_DEND:                                           // the end of the STEP2 direction pin 

STEP3_SDIR:  QBEQ STEP3_DLOW, STEP3_ENABLED, 0        // not enabled, set it low    
             QBEQ STEP3_DLOW, STEP3_DIRSTATE, 0       // what does the dir state say?
             SET  STEP3_DIRREG                        // it is nz, set the pin
			 JMP  STEP3_DEND
STEP3_DLOW:  CLR  STEP3_DIRREG                        // not enabled, clear the pin 
STEP3_DEND:                                           // the end of the STEP3 direction pin 

STEP4_SDIR:  QBEQ STEP4_DLOW, STEP4_ENABLED, 0        // not enabled, set it low    
             QBEQ STEP4_DLOW, STEP4_DIRSTATE, 0       // what does the dir state say?
             SET  STEP4_DIRREG                        // it is nz, set the pin
			 JMP  STEP4_DEND
STEP4_DLOW:  CLR  STEP4_DIRREG                        // not enabled, clear the pin 
STEP4_DEND:     

STEP5_SDIR:  QBEQ STEP5_DLOW, STEP5_ENABLED, 0        // not enabled, set it low    
             QBEQ STEP5_DLOW, STEP5_DIRSTATE, 0       // what does the dir state say?
             SET  STEP5_DIRREG                        // it is nz, set the pin
			 JMP  STEP5_DEND
STEP5_DLOW:  CLR  STEP5_DIRREG                        // not enabled, clear the pin 
STEP5_DEND:                                           // the end of the STEP5 direction pin 
             JMP LOOP_TOP                             // go back to the start

        // anything else, we put the pin low and stop
ALLSTOP:    CLR  STEP0_DIRREG                         // clear the direction
            CLR  STEP0_OUTPUTREG                      // clear the output state
			CLR  STEP1_DIRREG                         // clear the direction
            CLR  STEP1_OUTPUTREG                      // clear the output state
			CLR  STEP2_DIRREG                         // clear the direction
            CLR  STEP2_OUTPUTREG                      // clear the output state
			CLR  STEP3_DIRREG                         // clear the direction
            CLR  STEP3_OUTPUTREG                      // clear the output state
			CLR  STEP4_DIRREG                         // clear the direction
            CLR  STEP4_OUTPUTREG                      // clear the output state
			CLR  STEP5_DIRREG                         // clear the direction
            CLR  STEP5_OUTPUTREG                      // clear the output state
            HALT                                      // stop the PRU
            
        // all steppers disabled, set all steppers low
ALLLOW:     CLR  STEP0_DIRREG                         // clear the direction
            CLR  STEP0_OUTPUTREG                      // clear the output state
			CLR  STEP1_DIRREG                         // clear the direction
            CLR  STEP1_OUTPUTREG                      // clear the output state
			CLR  STEP2_DIRREG                         // clear the direction
            CLR  STEP2_OUTPUTREG                      // clear the output state
   			CLR  STEP3_DIRREG                         // clear the direction
            CLR  STEP3_OUTPUTREG                      // clear the output state
			CLR  STEP4_DIRREG                         // clear the direction
            CLR  STEP4_OUTPUTREG                      // clear the output state
			CLR  STEP5_DIRREG                         // clear the direction
            CLR  STEP5_OUTPUTREG                      // clear the output state

            JMP  CHKPIN                               // look for the re-enable

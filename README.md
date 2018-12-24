# Tilo

A Client-Server application for Windows and the Beaglebone Black which controls the pulse and direction signals for up to six stepper motors.

The purpose of this project is to have an application running on Windows be able to control the stepper motor pulse and direction signals output by a Beaglebone Black. Since the Debian operating system on the Beaglebone Black is not designed for real time, the client software running on the Beaglebone Black delegates the responsibility for sending the signals to a PASM assembler program running in the Programmable Real Time Unit (PRU1). The executable running in the PRU is designed to send consistent and accurate pulse widths even while various motors are enabled and disabled or the frequencies are changed.

The project includes a demonstration Server, a Client, the full PASM Assembly Language source and a Device Tree Overlay which configures the PRU pins. 

The Tilo Applications are open source and released under the MIT License. The home page for this project can be found at [http://www.OfItselfSo.com/Tilo](http://www.OfItselfSo.com/Tilo).

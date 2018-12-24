Overlays necessary to map the PRU1 pins in the PinMux so that they 
  are presented to the outside world on the Beaglebone Black P8 and 
  P9 headers.
  
The Tilo-00A0.dtbo compiled overlay should be placed in the 
  /lib/firmware of the Beaglebone Black and it should be added
  as an overlay in the /boot/uEnv.txt. A section with a suitable line 
  from the uEnv.txt file is shown below
  
        #uboot_overlay_addr3=/lib/firmware/<file3>.dtbo
        ###
        ###Additional custom capes
        uboot_overlay_addr4=/lib/firmware/Tilo-00A0.dtbo
        #uboot_overlay_addr5=/lib/firmware/<file5>.dtbo
        #uboot_overlay_addr6=/lib/firmware/<file6>.dtbo

See this help file for more information
http://www.ofitselfso.com/BeagleNotes/Beaglebone_Black_And_Device_Tree_Overlays.php

Tilo-00A0.dts file contained in this directory is the source code 
for the Tilo-00A0.dtbo overlay. The comments in this file indicate
the pins used for pulses and direction signals.
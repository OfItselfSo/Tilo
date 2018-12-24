using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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

namespace TiloWinServer
{
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
    /// <summary>
    /// A control which acts as the on screen container for a stepper motors
    /// configuration state
    /// </summary>
    /// <history>
    ///    19 Nov 18  Cynic - Started
    /// </history>
    public partial class ctlStepperControl : UserControl
    {

        // our control state changed delegate + event. We just send a simple
        // notice if anystate changes occur
        public delegate void StepperControlStateChangedEvent_Delegate(object sender);
        public StepperControlStateChangedEvent_Delegate StepperStateChangedEvent = null;

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Constructor
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public ctlStepperControl()
        {
            InitializeComponent();
            StepSpeedAsText = ServerClientData.DEFAULT_SPEED.ToString();
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the direction
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public uint StepDir
        {
            get
            {
                if (checkBoxDirSTEP.Checked == true) return 1;
                else return 0;
            }
            set
            {
                if (value == 1) checkBoxDirSTEP.Checked = true;
                else checkBoxDirSTEP.Checked = false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the stepper enabled state
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public uint StepEnabled
        {
            get
            {
                if (checkBoxEnabledSTEP.Checked == true) return 1;
                else return 0;
            }
            set
            {
                if (value == 1) checkBoxEnabledSTEP.Checked = true;
                else checkBoxEnabledSTEP.Checked = false;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the title
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public string Title
        {
            get
            {
                return labelSTEP.Text;
            }
            set
            {
                labelSTEP.Text = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the speed mode label
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public string SpeedMode
        {
            get
            {
                return labelSpeedMode.Text;
            }
            set
            {
                labelSpeedMode.Text = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the stepper speed
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private string StepSpeedAsText
        {
            get
            {
                return textBoxSpeedSTEP.Text;
            }
            set
            {
                textBoxSpeedSTEP.Text = value;
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Gets/Sets the stepper speed
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        public uint StepSpeed
        {
            get
            {
                try
                {
                    return Convert.ToUInt32(textBoxSpeedSTEP.Text);
                }
                catch
                {
                    return ServerClientData.DEFAULT_SPEED;
                }
            }
            set
            {
                if (value == 0) value = ServerClientData.DEFAULT_SPEED;
                textBoxSpeedSTEP.Text = value.ToString();
            }
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a state change on the control
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void checkBoxEnabledSTEP_CheckedChanged(object sender, EventArgs e)
        {
            // if we have anyone interested in our state changed events send it now
            if (StepperStateChangedEvent != null) StepperStateChangedEvent(this);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a state change on the control
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void checkBoxDirSTEP_CheckedChanged(object sender, EventArgs e)
        {
            // if we have anyone interested in our state changed events send it now
            if (StepperStateChangedEvent != null) StepperStateChangedEvent(this);
        }

        /// +=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=
        /// <summary>
        /// Handle a state change on the control
        /// </summary>
        /// <history>
        ///    19 Nov 18  Cynic - Started
        /// </history>
        private void textBoxSpeedSTEP_TextChanged(object sender, EventArgs e)
        {
            // if we have anyone interested in our state changed events send it now
            if (StepperStateChangedEvent != null) StepperStateChangedEvent(this);
        }
    }
}

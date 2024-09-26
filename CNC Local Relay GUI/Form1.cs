using CncLocalRelay;

namespace CNC_Local_Relay_GUI
{
    public partial class Form1 : Form
    {
        Thread RelayThread;
        bool RelayRunning = false;

        public Form1()
        {
            InitializeComponent();
        }
        readonly string sfln = "settings.ini";

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists(sfln))
            {
                foreach (string line in File.ReadAllLines(sfln))
                {
                    if (line.StartsWith("port="))
                    {
                        int port_no = 0;
                        bool parseok = int.TryParse(line.Substring(5), out port_no);
                        if (parseok)
                        {
                            txtPortOffset.Text = port_no.ToString();
                        }
                    }
                    if (line.Equals("upnp=disabled"))
                    {
                        chkUPNP.Checked = false;
                    }
                }
            }
            else
            {
                using (StreamWriter SW = new StreamWriter(sfln))
                {
                    SW.Close();
                }
            }

            //Port number not set, allocate random high range start port.
            if (String.IsNullOrEmpty(txtPortOffset.Text))
            {
                Random random = new Random();
                txtPortOffset.Text = random.Next(15000, 49152).ToString();
            }

            HostrecDisplay();

        }

        private void txtPortOffset_TextChanged(object sender, EventArgs e)
        {
            bool intok = false;
            int portnumber = 0;
            intok = int.TryParse(txtPortOffset.Text, out portnumber);

            if (!intok)
            {
                return;
            }

            if (!FileControl.LineExists(sfln, "port=" + portnumber.ToString()))
            {
                FileControl.RemoveStartingWithFromFile(sfln, "port=");
                FileControl.AddLineToFile(sfln, "port=" + portnumber.ToString());
            }

        }

        void HostrecDisplay()
        {
            if (HostControl.CheckHostRecord())
            {
                txtHostSetting.Text = "Enabled";
            }
            else
            {
                txtHostSetting.Text = "Disabled";
            }

        }
        readonly string hostwarningadmin = "Failed to update system host file - this requires administrator rights.\nPlease re-launch this program using 'Run As Administrator'\nAlternatively check you have rights to the file:\nC:\\Windows\\System32\\Drivers\\etc\\hosts";
        private void btnHostEnable_Click(object sender, EventArgs e)
        {
            try
            {
                HostControl.EnableHostRecord(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(hostwarningadmin);
                Console.WriteLine(ex);
            }

            HostrecDisplay();
        }

        private void btnHostDisable_Click(object sender, EventArgs e)
        {
            try
            {
                HostControl.EnableHostRecord(false); ;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show(hostwarningadmin);
            }
            HostrecDisplay();
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            int startport = int.Parse(txtPortOffset.Text);
            btnStart.Enabled = false;
            SetControlState(false);
            RelayThread = new Thread(() => RelayControl.RunRelay(startport,chkUPNP.Checked));
            RelayThread.Start();
            RelayRunning = true;
            btnStop.Enabled = true;
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            btnStop.Enabled = false;
            SetControlState(true);
            RelayControl.StopRelay();
            RelayRunning = false;
            btnStart.Enabled = true;
        }

        void SetControlState(bool State)
        {
            txtPortOffset.Enabled = State;
            chkUPNP.Enabled = State;
        }

        private void chkUPNP_CheckedChanged(object sender, EventArgs e)
        {
            FileControl.RemoveLineFromFile(sfln, "upnp=disabled");
            if (!chkUPNP.Checked)
            {
                FileControl.AddLineToFile(sfln, "upnp=disabled");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (RelayRunning)
            {
                RelayControl.StopRelay();
            }
        }
    }
}
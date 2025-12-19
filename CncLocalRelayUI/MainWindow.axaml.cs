using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CncLocalRelay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CncLocalRelayUI
{
    public partial class MainWindow : Window
    {
        static readonly string sfln = "settings.ini";
        static readonly string hostwarningadmin = "Failed to update system host file - this requires administrator rights.\nPlease re-launch this program using 'Run As Administrator'\nAlternatively check you have rights to the file:\nC:\\Windows\\System32\\Drivers\\etc\\hosts";
        bool RelayRunning = false;
        private CancellationTokenSource _cts;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += OnClosing;

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
                            txtPortStart.Text = port_no.ToString();
                        }
                    }
                    if (line.Equals("upnp=disabled"))
                    {
                        chkUPNP.IsChecked = false;
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
            if (String.IsNullOrEmpty(txtPortStart.Text))
            {
                Random random = new Random();
                txtPortStart.Text = random.Next(15000, 49152).ToString();
            }

            HostrecDisplay();

            RunMonitor();
        }
        private void OnClosing(object? sender, WindowClosingEventArgs e)
        {
            if (RelayRunning)
            {
                RelayControl.StopRelay();
            }
            _cts?.Cancel();
        }



        private void RunMonitor()
        {
            _cts = new CancellationTokenSource();

            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    string monitor_data = "";
                    foreach (var nb in LocalNeighbours.GetList())
                    {
                        monitor_data += $"Linked peer: {nb.Address}:{nb.StartPort}\n";
                    }

                    Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        txtMonitor.Text = monitor_data;
                    });

                    await Task.Delay(1000, _cts.Token);
                }
            });
        }

        public void OnStartClicked(object sender, RoutedEventArgs args)
        {
            int startport = int.Parse(txtPortStart.Text);
            btnStart.IsEnabled = false;
            SetControlState(false);
            bool upnp_enabled = (bool)chkUPNP.IsChecked;
            Debug.WriteLine($"Starting relay on port: {startport} with upnp: {upnp_enabled.ToString()}");
            txtRelayStatus.Text = "Relay Status: Running";
            _ = Task.Run(() => RelayControl.RunRelay(startport, upnp_enabled)).ContinueWith(t => RelayException(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
            RelayRunning = true;
            btnStop.IsEnabled = true;
        }

        async void RelayException(Exception ex)
        {
            Debug.WriteLine(ex);
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                txtRelayStatus.Text = $"Relay Status: Error: {ex.Message}";
            });
            RelayRunning = false;
        }


        public void OnStopClicked(object sender, RoutedEventArgs args)
        {
            btnStop.IsEnabled = false;
            SetControlState(true);
            RelayControl.StopRelay();
            RelayRunning = false;
            btnStart.IsEnabled = true;
            txtRelayStatus.Text = "Relay Status: Stopped";
        }
        void SetControlState(bool State)
        {
            txtPortStart.IsEnabled = State;
            chkUPNP.IsEnabled = State;
        }



        private void txtPortChanged(object? sender, TextChangedEventArgs e)
        {
            bool intok = false;
            int portnumber = 0;
            intok = int.TryParse(txtPortStart.Text, out portnumber);

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


        public void EnableHost(object sender, RoutedEventArgs args)
        {
            try
            {
                HostControl.EnableHostRecord(true);
            }
            catch (Exception ex)
            {
                txtRedirection.Text = $"NAT-NEG Redirection: {ex.Message}";
            }

            HostrecDisplay();
        }

        public void DisableHost(object sender, RoutedEventArgs args)
        {
            try
            {
                HostControl.EnableHostRecord(false); ;
            }
            catch (Exception ex)
            {
                txtRedirection.Text = $"NAT-NEG Redirection: {ex.Message}";
            }
            HostrecDisplay();
        }

        void HostrecDisplay()
        {
            if (HostControl.CheckHostRecord())
            {
                txtRedirection.Text = "NAT-NEG Redirection: Enabled";
            }
            else
            {
                txtRedirection.Text = "NAT-NEG Redirection: Disabled";
            }

        }

        private void chkUPNP_CheckedChanged(object? sender, RoutedEventArgs e)
        {
            FileControl.RemoveLineFromFile(sfln, "upnp=disabled");
            if (!(bool)chkUPNP.IsChecked)
            {
                FileControl.AddLineToFile(sfln, "upnp=disabled");
            }
        }

        private void OnLinkClicked(object? sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/mrhteriyaki/RA3LANHelper",
                UseShellExecute = true
            });
        }
    }
}
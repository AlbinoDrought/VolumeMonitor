using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VolumeMonitorTestForm
{
    using NAudio.CoreAudioApi;

    public partial class Form1 : Form
    {
        private Dictionary<MMDevice, int> deviceVolume = new Dictionary<MMDevice, int>(); 

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void volumeMonitor1_OnTick(NAudio.CoreAudioApi.MMDevice device, int volume)
        {
            deviceVolume[device] = volume;
        }

        private void volumeMonitor1_OnThresholdHit(NAudio.CoreAudioApi.MMDevice device, int volume)
        {
            this.addOutput(String.Format("{0} hit the threshold of {1}-{2} at {3}", device.FriendlyName, volumeMonitor1.ThresholdMinimum, volumeMonitor1.ThresholdMaximum, volume));
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            deviceVolume.Clear();

            List<MMDevice> devices = volumeMonitor1.GetDevices();
            foreach (MMDevice device in devices)
            {
                volumeMonitor1.Add(device);
                deviceVolume.Add(device, 0);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            deviceVolume.Clear();

            List<MMDevice> devices = volumeMonitor1.GetDevices();
            foreach (MMDevice device in devices)
            {
                volumeMonitor1.Remove(device);
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            string finalString = "Devices:\r\n";

            foreach (MMDevice device in deviceVolume.Keys)
            {
                finalString += String.Format("{0}: {1}\r\n", device.FriendlyName, deviceVolume[device].ToString());
            }

            lblDevices.Text = finalString;
        }

        private void addOutput(string text)
        {
            txtOutput.Text += "\r\n" + text;
            txtOutput.SelectionStart = txtOutput.Text.Length;
            txtOutput.ScrollToCaret();
        }
    }
}

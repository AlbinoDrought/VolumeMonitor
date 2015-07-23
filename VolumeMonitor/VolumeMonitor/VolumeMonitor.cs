using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolumeMonitor
{
    using System.Threading;

    using NAudio.CoreAudioApi;

    public class VolumeMonitor : System.ComponentModel.Component
    {
        public delegate void UpdateDelegate(MMDevice device, int volume);

        public event UpdateDelegate OnTick;

        public event UpdateDelegate OnThresholdHit;

        public int UpdateInterval
        {
            get
            {
                return timer.Interval;
            }
            set
            {
                timer.Interval = value;
            }
        }

        public bool CooldownEnabled { get; set; }

        public int Cooldown { get; set; }

        public int CurrentCooldown
        {
            get
            {
                return currentCooldown;
            }
        }

        public int ThresholdMinimum { get; set; }

        public int ThresholdMaximum { get; set; }

        private System.Windows.Forms.Timer timer;

        private Dictionary<MMDevice, WasapiCapture> deviceCaptures;

        private int currentCooldown = 0;

        public List<MMDevice> Devices
        {
            get; private set;
        }

        public bool Listening
        {
            get
            {
                return Devices.Count > 0;
            }
        }

        public VolumeMonitor()
        {
            Devices = new List<MMDevice>();
            deviceCaptures = new Dictionary<MMDevice, WasapiCapture>();
            ThresholdMaximum = 100;
            ThresholdMinimum = 0;
            timer = new System.Windows.Forms.Timer();
            timer.Tick += timer_Tick;
            timer.Interval = 10;
            CooldownEnabled = true;
            Cooldown = 1000;
        }

        /// <summary>
        /// Adds a device to the monitoring list
        /// </summary>
        /// <param name="device">Device to add</param>
        public void Add(MMDevice device)
        {
            if (Devices.Contains(device)) return;

            if (device.DataFlow == DataFlow.Capture)
            {
                WasapiCapture deviceCapture = new WasapiCapture(device);
                deviceCapture.StartRecording();
                deviceCaptures.Add(device, deviceCapture);
            }

            timer.Enabled = true;

            Devices.Add(device);
        }

        /// <summary>
        /// Removes a device from the monitoring list
        /// </summary>
        /// <param name="device">Device to remove</param>
        public void Remove(MMDevice device)
        {
            if (!Devices.Contains(device)) return;

            if (deviceCaptures.ContainsKey(device))
            {
                WasapiCapture deviceCapture = deviceCaptures[device];
                deviceCapture.StopRecording();
                deviceCapture.Dispose();
                deviceCaptures.Remove(device);
            }

            // disable timer if there will be no devices left after removal
            timer.Enabled = Devices.Count - 1 > 0;

            Devices.Remove(device);
        }

        /// <summary>
        /// Get MMDevices for this system
        /// </summary>
        /// <param name="df">DataFlow.Capture for Input (Microphones), DataFlow.Render for Output (Soundcards), DataFlow.All for all</param>
        /// <param name="ds">Which DeviceStates to show</param>
        /// <returns></returns>
        public List<MMDevice> GetDevices(DataFlow df = DataFlow.All, DeviceState ds = DeviceState.Active)
        {
            List<MMDevice> devices = new List<MMDevice>();

            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(df, ds))
            {
                devices.Add(device);
            }

            return devices;
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            foreach (MMDevice device in Devices)
            {
                int volume = this.calculateVolume(device);

                if (volume >= ThresholdMinimum && volume <= ThresholdMaximum)
                {
                    if (OnThresholdHit != null && (!CooldownEnabled || currentCooldown <= 0))
                    {
                        OnThresholdHit(device, volume);
                        if (CooldownEnabled)
                        {
                            currentCooldown = Cooldown;
                        }
                    }
                }

                if (OnTick != null)
                {
                    OnTick(device, volume);
                }
            }

            if (CooldownEnabled && currentCooldown > 0)
            {
                currentCooldown -= timer.Interval;
            }
        }

        private int calculateVolume(MMDevice device)
        {
            return (int)(Math.Round(device.AudioMeterInformation.MasterPeakValue * 100));
        }
    }
}

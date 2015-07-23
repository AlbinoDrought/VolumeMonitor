## VolumeMonitor

VolumeMonitor is a WinForms component that enables easy event-based volume monitoring using NAudio's implementation of WASAPI.

## Code Example

```C#
public void Start()
{
	List<MMDevice> devices = volumeMonitor1.GetDevices();
	foreach (MMDevice device in devices)
	{
		volumeMonitor1.Add(device);
	}

	volumeMonitor1.OnThresholdHit += volumeMonitor1_OnThresholdHit;
}

private void volumeMonitor1_OnThresholdHit(NAudio.CoreAudioApi.MMDevice device, int volume)
{
	MessageBox.Show(String.Format("{0} hit the threshold of {1}-{2} at {3}", device.FriendlyName, volumeMonitor1.ThresholdMinimum, volumeMonitor1.ThresholdMaximum, volume));
}
```

See VolumeMonitorTestForm for more.

## Why?

Used it in another project and wanted to be able to easily use it for future projects as well.

## License

Licensed under GPL-3 - https://tldrlegal.com/license/gnu-general-public-license-v3-(gpl-3)

Uses **NAudio**: NAudio is an open source .NET audio library written by Mark Heath (mark.heath@gmail.com) licensed under Ms-PL
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using NAudio.CoreAudioApi;
using NAudio.Wave;

/// <summary>
/// Detects and monitors audio volume from system devices or specific applications.
/// Requires: NAudio (NuGet: Install-Package NAudio)
/// </summary>
public class DetectAudio : IDisposable
{
    private WasapiCapture? _capture;
    private MMDevice? _device;
    private bool _isMonitoring;
    private bool _disposed;

    /// <summary>Fired continuously while monitoring. Provides peak volume 0.0 - 1.0 and dB level.</summary>
    public event Action<float, float>? OnVolumeChanged;

    /// <summary>Fired when volume crosses above the threshold.</summary>
    public event Action<float>? OnVolumeThresholdExceeded;

    /// <summary>Volume threshold (0.0 - 1.0) that triggers OnVolumeThresholdExceeded.</summary>
    public float Threshold { get; set; } = 0.1f;

    /// <summary>The current peak volume, updated while monitoring (0.0 - 1.0).</summary>
    public float CurrentVolume { get; private set; }

    /// <summary>The current volume in decibels (dB). Silence is -∞, loud is close to 0.</summary>
    public float CurrentVolumeDb { get; private set; }

    // -------------------------------------------------------------------------
    // Device Enumeration
    // -------------------------------------------------------------------------

    /// <summary>
    /// Lists all active audio input (capture) devices, e.g. microphones.
    /// </summary>
    public static IReadOnlyList<string> GetInputDevices()
    {
        var list = new List<string>();
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        for (int i = 0; i < devices.Count; i++)
            list.Add( devices[i].FriendlyName);
        return list;
    }

    /// <summary>
    /// Lists all active audio output (render) devices, e.g. speakers, headphones.
    /// </summary>
    public static IReadOnlyList<string> GetOutputDevices()
    {
        var list = new List<string>();
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        for (int i = 0; i < devices.Count; i++)
            list.Add( devices[i].FriendlyName);
        return list;
    }

    // -------------------------------------------------------------------------
    // Initialization
    // -------------------------------------------------------------------------

    /// <summary>
    /// Sets up monitoring on the system's default microphone (input device).
    /// </summary>
    public void UseDefaultInputDevice()
    {
        CleanupCapture();
        using var enumerator = new MMDeviceEnumerator();
        _device = enumerator.GetDefaultAudioEndpoint(DataFlow.Capture, Role.Multimedia);
        _capture = new WasapiCapture(_device);
        AttachDataHandler();
    }

    /// <summary>
    /// Sets up monitoring on the system's default speaker/output (loopback).
    /// This captures what is actually playing through your speakers — similar to OBS.
    /// </summary>
    public void UseDefaultOutputDevice()
    {
        CleanupCapture();
        using var enumerator = new MMDeviceEnumerator();
        _device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        _capture = new WasapiLoopbackCapture(_device);
        AttachDataHandler();
    }

    /// <summary>
    /// Sets up monitoring on a specific input device by index (from GetInputDevices()).
    /// </summary>
    public void UseInputDevice(int deviceIndex)
    {
        CleanupCapture();
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
        if (deviceIndex < 0 || deviceIndex >= devices.Count)
            throw new ArgumentOutOfRangeException(nameof(deviceIndex));
        _device = devices[deviceIndex];
        _capture = new WasapiCapture(_device);
        AttachDataHandler();
    }
    public bool SetDeviceToUse(string deviceName)
    {
        var inputDevices = GetInputDevices().ToList();
        int index=   inputDevices.IndexOf(deviceName);
        GD.Print($"Setting device to use: {deviceName}");
        if(index!=-1)
        {
            UseInputDevice(index);
            return true;
        }
        var outputDevices = GetOutputDevices().ToList();
        index= outputDevices.IndexOf(deviceName);
        if (index != -1)
        {
            UseOutputDevice(index);
            return true;
        }
        UseDefaultInputDevice();
        GD.Print("Failed to set device using default input instead");
        return false;
    }
    /// <summary>
    /// Sets up loopback monitoring on a specific output device by index (from GetOutputDevices()).
    /// Captures audio playing through that output — great for per-app detection when combined
    /// with Windows audio sessions.
    /// </summary>
    public void UseOutputDevice(int deviceIndex)
    {
        CleanupCapture();
        using var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
        if (deviceIndex < 0 || deviceIndex >= devices.Count)
            throw new ArgumentOutOfRangeException(nameof(deviceIndex));
        _device = devices[deviceIndex];
        _capture = new WasapiLoopbackCapture(_device);
        AttachDataHandler();
    }

    // -------------------------------------------------------------------------
    // Per-Application Volume (using Windows Audio Sessions / WASAPI)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Gets the current volume level of a specific application by its process name
    /// (e.g. "chrome", "discord") using the Windows Audio Session API.
    /// Returns the peak volume as a float (0.0 - 1.0), or -1 if not found.
    /// Note: This is a snapshot, not a continuous monitor. Call repeatedly to poll.
    /// </summary>
    public static float GetApplicationVolume(string processName)
    {
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        var sessions = device.AudioSessionManager.Sessions;
        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            try
            {
                uint pid = session.GetProcessID;
                var process = System.Diagnostics.Process.GetProcessById((int)pid);
                if (process.ProcessName.Equals(processName, StringComparison.OrdinalIgnoreCase))
                {
                    return session.AudioMeterInformation.MasterPeakValue;
                }
            }
            catch
            {
                // Process may have exited; skip it.
            }
        }
        return -1f;
    }

    /// <summary>
    /// Lists all applications currently producing audio, with their process names and peak volumes.
    /// </summary>
    public static IReadOnlyList<(string ProcessName, float PeakVolume)> GetAllApplicationVolumes()
    {
        var result = new List<(string, float)>();
        using var enumerator = new MMDeviceEnumerator();
        using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

        var sessions = device.AudioSessionManager.Sessions;
        for (int i = 0; i < sessions.Count; i++)
        {
            var session = sessions[i];
            try
            {
                uint pid = session.GetProcessID;
                var process = System.Diagnostics.Process.GetProcessById((int)pid);
                float peak = session.AudioMeterInformation.MasterPeakValue;
                result.Add((process.ProcessName, peak));
            }
            catch { }
        }
        return result;
    }

    // -------------------------------------------------------------------------
    // Monitoring Control
    // -------------------------------------------------------------------------

    /// <summary>
    /// Starts continuous audio monitoring. UseDefaultInputDevice(), UseDefaultOutputDevice(),
    /// UseInputDevice(), or UseOutputDevice() must be called first.
    /// </summary>
    public void StartMonitoring()
    {
        if (_capture == null)
            throw new InvalidOperationException("No device selected. Call a Use*Device() method first.");
        if (_isMonitoring) return;

        _capture.StartRecording();
        _isMonitoring = true;
    }

    /// <summary>Stops continuous audio monitoring.</summary>
    public void StopMonitoring()
    {
        if (!_isMonitoring) return;
        _capture?.StopRecording();
        _isMonitoring = false;
    }

    public bool IsMonitoring => _isMonitoring;

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private void AttachDataHandler()
    {
        if (_capture == null) return;
        _capture.DataAvailable += (_, e) =>
        {
            float peak = CalculatePeakVolume(e.Buffer, e.BytesRecorded, _capture.WaveFormat);
            float db = peak > 0 ? 20f * MathF.Log10(peak) : float.NegativeInfinity;

            CurrentVolume = peak;
            CurrentVolumeDb = db;

            OnVolumeChanged?.Invoke(peak, db);

            if (peak >= Threshold)
                OnVolumeThresholdExceeded?.Invoke(peak);
        };
    }

    private static float CalculatePeakVolume(byte[] buffer, int bytesRecorded, WaveFormat format)
    {
        float peak = 0f;

        // Handle 32-bit IEEE float (common with WASAPI)
        if (format.Encoding == WaveFormatEncoding.IeeeFloat)
        {
            int samples = bytesRecorded / 4;
            for (int i = 0; i < samples; i++)
            {
                float sample = MathF.Abs(BitConverter.ToSingle(buffer, i * 4));
                if (sample > peak) peak = sample;
            }
        }
        // Handle 16-bit PCM
        else if (format.Encoding == WaveFormatEncoding.Pcm && format.BitsPerSample == 16)
        {
            int samples = bytesRecorded / 2;
            for (int i = 0; i < samples; i++)
            {
                float sample = MathF.Abs(BitConverter.ToInt16(buffer, i * 2) / 32768f);
                if (sample > peak) peak = sample;
            }
        }

        return Math.Clamp(peak, 0f, 1f);
    }

    private void CleanupCapture()
    {
        if (_isMonitoring) StopMonitoring();
        _capture?.Dispose();
        _capture = null;
        _device?.Dispose();
        _device = null;
    }

    public void Dispose()
    {
        if (_disposed) return;
        CleanupCapture();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;

//Quick bitmask to check for legal device connection types. I'm pretty sure
//it's just keyboard or controller but you never know if someone brings the
//legendary Osu drawing tablet
[Flags]
public enum DeviceType
{
    None      = 0,
    Keyboard  = 1 << 0,
    Mouse     = 1 << 1,
    Gamepad   = 1 << 2,
    Touch     = 1 << 3,

    All = ~0
}

public class DeviceManager : MonoBehaviour {
    private const int LISTENING = 1;
    private const int DISABLED = 0;

    [SerializeField] private DeviceType legalDevices = DeviceType.Keyboard | DeviceType.Gamepad;
    [SerializeField] private bool debug = false;
    
    public List<InputDevice> RegisteredDevices { get; private set; } = new();

    public delegate void DevicePair(InputDevice device, DeviceType deviceType, string displayName);
    public event DevicePair OnDevicePair;

    public delegate void DeviceDisconnect(InputDevice device);
    public event DeviceDisconnect OnDeviceDisconnect;
    
    private void OnEnable() {
        InputUser.listenForUnpairedDeviceActivity = LISTENING;
        InputUser.onUnpairedDeviceUsed += RegisterDevice;
        InputSystem.onDeviceChange += DeregisterDevice;
    }

    private void OnDisable() {
        InputUser.listenForUnpairedDeviceActivity = DISABLED;
        InputUser.onUnpairedDeviceUsed -= RegisterDevice; 
        InputSystem.onDeviceChange -= DeregisterDevice;
    }

    private void RegisterDevice(InputControl control, InputEventPtr ptr) {
        InputDevice device = control.device;
        DeviceType deviceType = DeviceType.None;

        if (RegisteredDevices.Contains(device)) return;
        
        //Device validity check
        if (device is Keyboard) deviceType = DeviceType.Keyboard;
        else if (device is Mouse) deviceType = DeviceType.Mouse;
        else if (device is Gamepad) deviceType = DeviceType.Gamepad;
        else if (device is Touchscreen) deviceType = DeviceType.Touch;

        if ((legalDevices & deviceType) == 0)
            return;
        
        if (debug) Debug.Log($"Device {device.displayName} joined.");

        RegisteredDevices.Add(device);
        InputUser.PerformPairingWithDevice(device);
        
        OnDevicePair?.Invoke(device, deviceType, device.name);
    }
    
    private void DeregisterDevice(InputDevice device, InputDeviceChange change)
    {
        if (change == InputDeviceChange.Disconnected 
            || change == InputDeviceChange.Removed)
        {
            if (RegisteredDevices.Remove(device))
            { 
                if (debug) Debug.Log($"Device {device.name} has disconnected.");
                OnDeviceDisconnect?.Invoke(device);
            }
        }
    }
}

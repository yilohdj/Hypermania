using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

public class DeviceDisplayController : MonoBehaviour {
    
    private enum Orientation {
        Left,
        Center,
        Right
    }
    
    //oops all serializedfields
    [Header("Grid Displays")]
    [SerializeField] private GridLayoutGroup centerGridDisplay;
    [SerializeField] private GridLayoutGroup player1GridDisplay;
    [SerializeField] private GridLayoutGroup player2GridDisplay;

    //ADDRESS ME
    [Space] [Header("Button Displays")] [SerializeField]
    private Button startButton;
    
    [Space] [Header("Prefabs")] 
    [SerializeField] private GameObject ControllerPrefab;
    [SerializeField] private GameObject KeyboardPrefab;
    [SerializeField] private GameObject MousePrefab;
    [SerializeField] private GameObject TouchPrefab;
    
    [Space]
    [SerializeField] private DeviceManager deviceManager;

    private Dictionary<InputDevice, (GameObject icon, Orientation orientation)> _deviceIconMap = new();

    private List<InputDevice> _player1Devices = new List<InputDevice>();
    private List<InputDevice> _player2Devices = new List<InputDevice>();
    
    private void OnEnable() {
        deviceManager.OnDevicePair += UpdateDeviceStack;
        deviceManager.OnDeviceDisconnect += RemoveDevice;

        InputSystem.onAnyButtonPress.Call(HandlePlayerInputSelect);
    }

    private void OnDisable() {
        deviceManager.OnDevicePair -= UpdateDeviceStack;
        deviceManager.OnDeviceDisconnect -= RemoveDevice; 
    }

    //ADDRESS ME
    private void Update() {
        startButton.interactable = (player1GridDisplay.transform.childCount == 1
                               || player2GridDisplay.transform.childCount == 1);
    }
    
    //TODO: Should refactor so that the device manager holds P1/P2 device references this is so cooked
    public void SendDevices(bool trainingMode) {
        if (_player1Devices.Count == 1 && _player2Devices.Count == 1) {
            DeviceDataInjector.Instance.RegisterDevices(_player1Devices[0], _player2Devices[0], trainingMode ? GameRunnerMode.Training : GameRunnerMode.Local);
        }
    }

    private void UpdateDeviceStack(InputDevice device, DeviceType type, string deviceName) {
        GameObject inputIcon = Instantiate(GetInputIcon(type), centerGridDisplay.transform);
        inputIcon.transform.GetChild(1).GetComponent<TextMeshProUGUI>().SetText(deviceName);   //ADDRESS ME...
        
        _deviceIconMap.Add(device, (inputIcon, Orientation.Center));
    }

    private void RemoveDevice(InputDevice device) {
        Destroy(_deviceIconMap[device].icon); //bleh
        _deviceIconMap.Remove(device);
    }

    private GameObject GetInputIcon(DeviceType type) {
        if ((type & DeviceType.Gamepad) != 0)
            return ControllerPrefab;

        if ((type & DeviceType.Keyboard) != 0)
            return KeyboardPrefab;

        if ((type & DeviceType.Mouse) != 0)
            return MousePrefab;

        if ((type & DeviceType.Touch) != 0)
            return TouchPrefab;

        return null;
    }

    private void HandlePlayerInputSelect(InputControl control) {
        InputDevice activeDevice = control.device;
        string controlName = control.name;

        if (!_deviceIconMap.TryGetValue(activeDevice, out var iconPair)) return;

        if (iconPair.icon == null) return;
        
        if (controlName.Contains("Left", StringComparison.OrdinalIgnoreCase)) {
            if (iconPair.orientation == Orientation.Left) return;
            
            iconPair.orientation = iconPair.orientation == Orientation.Center ? Orientation.Left : Orientation.Center;
            
            ShiftIcon(iconPair.icon.transform, iconPair.orientation);
            UpdateDeviceList(activeDevice, iconPair.orientation);
            
        } else if (controlName.Contains("Right", StringComparison.OrdinalIgnoreCase)) {
            if (iconPair.orientation == Orientation.Right) return;
            
            iconPair.orientation = iconPair.orientation == Orientation.Center ? Orientation.Right : Orientation.Center;

            ShiftIcon(iconPair.icon.transform, iconPair.orientation);
            UpdateDeviceList(activeDevice, iconPair.orientation);
        }
        
        _deviceIconMap[activeDevice] = iconPair;
    }

    private void ShiftIcon(Transform icon, Orientation orientation) {
        switch (orientation) {
            case Orientation.Left:
                icon.transform.parent = player1GridDisplay.transform;
                break;
            case Orientation.Right:
                icon.transform.parent = player2GridDisplay.transform;
                break;
            case Orientation.Center:
                icon.transform.parent = centerGridDisplay.transform;
                break;
        }
    }

    private void UpdateDeviceList(InputDevice device, Orientation orientation) {
        if (orientation == Orientation.Left) {
            _player1Devices.Add(device);
            _player2Devices.Remove(device);
        }
        else if (orientation == Orientation.Right) {
            _player2Devices.Add(device);
            _player1Devices.Remove(device);
        }
        else {
            _player1Devices.Remove(device);
            _player2Devices.Remove(device);
        }
    }
}

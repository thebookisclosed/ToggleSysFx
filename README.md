# ToggleSysFx
This tool offers a less cumbersome way to toggle audio processing effects for any multimedia device you ever plugged into your system, including *currently disconnected* or *unavailable* ones.

This is the only way to disable effects for devices that cause the Windows Audio Service to crash upon connection — a rare issue happening on select Windows 11 devices where OEMs ship the *Invert Volume Mode Effect by Realtek (RtkInvVolMFX)* for capture/input/microphone devices and the device you connected uses an incompatible sampling rate.
By disabling processing effects you retain the ability to use the microphone, you're no longer required to keep the problematic microphone disabled.

# Usage
1. Run ToggleSysFx.exe from the Command Prompt
2. Type the number of the device you'd like to toggle this setting for
3. Confirm that you want to change the setting in case the current one is undesired

This tool can be used to toggle the setting on any device but by default it only displays microphones. If you'd like to include output/render devices in the list, run the tool with the `/all` parameter.

# Is Microsoft aware of this issue?
Yes. The bug is already tracked internally and a fix should be in the works. Until the fix goes live, this is the best solution.

# Example
![Tool screenshot](https://github.com/thebookisclosed/ToggleSysFx/assets/13197516/fc7e7c89-a0b7-42df-bce2-9b70f925f5a7)

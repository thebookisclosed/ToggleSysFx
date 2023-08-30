/*
   ToggleSysFx
   Copyright (C) 2023  @thebookisclosed

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Runtime.InteropServices;

namespace ToggleSysFx
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DoMain(args);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
        
        static void DoMain(string[] args)
        {
            Console.WriteLine("ToggleSysFx 1.0 - @thebookisclosed, 2023");
            Console.WriteLine("Command line tool for toggling audio processing effects for multimedia devices");
            Console.WriteLine();
            var device = SelectMMDevice(args.Length > 0 && args[0].ToLowerInvariant() == "/all");
            if (device == null)
            {
                Console.WriteLine("Failed to select multimedia device");
                return;
            }
            ToggleSysFxForDevice(device);
        }

        static IMMDevice SelectMMDevice(bool includeRender)
        {
            var en = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            var hr = en.EnumAudioEndpoints(includeRender ? EDataFlow.eAll : EDataFlow.eCapture, MMConstants.DEVICE_STATEMASK_ALL, out IMMDeviceCollection devices);
            if (hr < 0)
            {
                Console.WriteLine("Failed to enumerate audio endpoints (hr = 0x{0:X8})", hr);
                return null;
            }
            hr = devices.GetCount(out uint cDevices);
            if (hr < 0 || cDevices == 0)
            {
                Console.WriteLine("Didn't find any configured multimedia devices (hr = 0x{0:X8})", hr);
                return null;
            }
            var devArray = new IMMDevice[cDevices];
            var ogColor = Console.ForegroundColor;
            Console.WriteLine("Device list:");
            for (int i = 0; i < cDevices; i++)
            {
                hr = devices.Item((uint)i, out IMMDevice device);
                devArray[i] = device;
                if (device == null || hr < 0)
                    continue;
                hr = device.OpenPropertyStore(MMConstants.STGM_READ, out IPropertyStore properties);
                if (hr < 0)
                {
                    Console.WriteLine("Failed to open the property store for device #{0} (hr = 0x{1:X8})", i, hr);
                    continue;
                }
                hr = properties.GetValue(MMConstants.PKEY_Device_FriendlyName, out PropVariant pv);
                if (hr < 0)
                {
                    Console.WriteLine("Failed to read the name for device #{0} (hr = 0x{1:X8})", i, hr);
                    continue;
                }
                var name = Marshal.PtrToStringUni(pv.pwszVal);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("[{0}]", i);
                Console.ForegroundColor = ogColor;
                Console.WriteLine(" {0}", name);
            }
            Console.WriteLine();
            var desiredDev = -1;
            while (desiredDev < 0 || desiredDev > devArray.Length - 1) {
                Console.Write("Enter the number of the device to configure: ");
                var dvStr = Console.ReadLine();
                if (!int.TryParse(dvStr, out desiredDev))
                    desiredDev = -1;
            }

            return devArray[desiredDev];
        }

        static void ToggleSysFxForDevice(IMMDevice dev)
        {
            var leDev = dev as ILocalEndpointDevice;
            if (leDev == null)
            {
                Console.WriteLine("Failed to cast to Local Endpoint Device");
                return;
            }
            var hr = leDev.OpenFXPropertyStore(MMConstants.STGM_READWRITE, out IPropertyStore fxProperties);
            if (hr < 0) 
            {
                Console.WriteLine("Failed to open effects property store (hr = 0x{0:X8})", hr);
                return;
            }
            hr = fxProperties.GetValue(MMConstants.PKEY_AudioEndpoint_Disable_SysFx, out PropVariant pv);
            var sysFxDisabled = false;
            if (hr >= 0)
                sysFxDisabled = pv.boolVal != 0;
            var ogColor = Console.ForegroundColor;
            Console.Write("Effects are currently ");
            Console.ForegroundColor = sysFxDisabled ? ConsoleColor.Red : ConsoleColor.Green;
            Console.WriteLine(sysFxDisabled ? "Disabled" : "Enabled");
            Console.ForegroundColor = ogColor;
            Console.WriteLine();
            var ynRes = ConsoleKey.A;
            while (!(ynRes == ConsoleKey.Y || ynRes == ConsoleKey.N))
            {
                Console.Write("Do you wish to toggle the setting [Y/N]? ");
                ynRes = Console.ReadKey().Key;
                Console.WriteLine();
            }

            if (ynRes == ConsoleKey.N)
                return;

            // Invert the value
            if (sysFxDisabled)
            {
                pv.varType = VarEnum.VT_EMPTY;
            }
            else
            {
                pv.varType = VarEnum.VT_BOOL;
                pv.boolVal = 1;
            }
            hr = fxProperties.SetValue(MMConstants.PKEY_AudioEndpoint_Disable_SysFx, pv);
            if (hr < 0)
                Console.WriteLine("Setting update failed (hr = 0x{0:X8})", hr);
            else
                Console.WriteLine("Setting updated successfully");
        }
    }
}

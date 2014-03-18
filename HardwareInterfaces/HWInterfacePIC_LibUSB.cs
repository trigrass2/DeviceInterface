﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
//#if IPHONE
//#else
using LibUsbDotNet;
using LibUsbDotNet.Main;
using LibUsbDotNet.DeviceNotify;
//#endif

namespace ECore.HardwareInterfaces
{
    //class that provides raw HW access to the device
    public class HWInterfacePIC_LibUSB: EDeviceHWInterface
    {
		private int tempFrameCounter = 0;
		private const int COMMAND_READ_ENDPOINT_SIZE = 16;
		private bool isConnected;
#if ANDROID



		public HWInterfacePIC(){
		}
		public override void WriteControlBytes(byte[] message){
		}
		public override byte[] ReadControlBytes(int length){ return null;
		}
		public override byte[] GetData(int numberOfBytes) { return null;
		}


#else
        //needed for plug-unplug events
        private static IDeviceNotifier UsbDeviceNotifier = DeviceNotifier.OpenDeviceNotifier();

        private UsbDevice scop3UsbDevice;
        private UsbEndpointWriter commandWriteEndpoint;
        private UsbEndpointReader commandReadEndpoint;
        private UsbEndpointReader dataEndpoint;
        

        public HWInterfacePIC_LibUSB()
        {
#if IPHONE || ANDROID
#else
            // Hook the device notifier event
            UsbDeviceNotifier.OnDeviceNotify += OnDeviceNotifyEvent;            
#endif
            //and call the method, to check if device is already connected
            OnDeviceNotifyEvent(null, null);

        }

        //called at init, and each time a system event occurs
        private void OnDeviceNotifyEvent(object sender, DeviceNotifyEventArgs e)
        {           
			UsbRegDeviceList usbDeviceList = UsbDevice.AllDevices;
			Logger.AddEntry (this, LogMessageType.Persistent, "Total number of USB devices attached: "+usbDeviceList.Count.ToString ());
			foreach (UsbRegistry device in usbDeviceList)
			{
				string sAdd = string.Format("Vid:0x{0:X4} Pid:0x{1:X4} (rev:{2}) - {3}",
				                            device.Vid,
				                            device.Pid,
				                            (ushort) device.Rev,
				                            device[SPDRP.DeviceDesc]);

				Logger.AddEntry (this, LogMessageType.Persistent, sAdd);
			}

            //locate USB device
            UsbDeviceFinder scop3UsbFinder = new UsbDeviceFinder(1240, 82);
            scop3UsbDevice = UsbDevice.OpenUsbDevice(scop3UsbFinder);            
            
            //if device is attached
            if (scop3UsbDevice != null)
            {
				Logger.AddEntry(this, LogMessageType.ECoreInfo, "SmartScope connected!");

                //check whether device was already connected, as in that case we don't have to do anything
                if (!isConnected)
                {
					IUsbDevice wholeUsbDevice = scop3UsbDevice as IUsbDevice;
					if (!ReferenceEquals(wholeUsbDevice, null))
					{
						// This is a "whole" USB device. Before it can be used,
						// the desired configuration and interface must be selected.

						// Select config
						bool succes1 = wholeUsbDevice.SetConfiguration(1);


						// Claim interface
						bool succes2 = wholeUsbDevice.ClaimInterface(0);
						Logger.AddEntry (this, LogMessageType.Persistent, "Claim interface: "+succes2.ToString ());
					} 

                    //init endpoints
                    dataEndpoint = scop3UsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                    commandWriteEndpoint = scop3UsbDevice.OpenEndpointWriter(WriteEndpointID.Ep02);
                    commandReadEndpoint = scop3UsbDevice.OpenEndpointReader(ReadEndpointID.Ep03);

		            if (commandWriteEndpoint == null)
		            {
			            Logger.AddEntry(this, LogMessageType.Persistent, "commandWriteEndpoint==null");
			            return;
		            }

                    //indicate device is connected
                    isConnected = true;
                    
                }
            }
            else
            {
                isConnected = false;
                Logger.AddEntry(this, LogMessageType.ECoreInfo, "No device found");
            }
            
        }

        public override void WriteControlBytes(byte[] message)
        {
            //log
            string logString = "";
            foreach (byte b in message)
                logString += b.ToString() + ",";

            //Logger.AddEntry(this, LogMessageType.ECoreInfo, "Request to send command to HW: [" + logString+"]");

            //see if device is connected properly
            if (commandWriteEndpoint == null)
            {
                Logger.AddEntry(this, LogMessageType.ECoreWarning, "Trying to write to device, but commandWriteEndpoint==null");
                return;
            }

            //try to send data
            ErrorCode errorCode = ErrorCode.None;
            try
            {
                int bytesWritten;
                errorCode = commandWriteEndpoint.Write(message, 5000, out bytesWritten);                
            }
            catch (Exception ex)
            {
                Logger.AddEntry(this, LogMessageType.ECoreError, "Writing control bytes failed");
                Logger.AddEntry(this, LogMessageType.ECoreError, "ExceptionMessage: " + ex.Message);
                Logger.AddEntry(this, LogMessageType.ECoreError, "USB ErrorCode: " + errorCode);
                Logger.AddEntry(this, LogMessageType.ECoreError, "message: " + message.ToString());
            }   
        }
        
        public override byte[] ReadControlBytes(int length)
        {
            //see if device is connected properly
            if (commandReadEndpoint == null)
            {
                Logger.AddEntry(this, LogMessageType.ECoreWarning, "Trying to read from device, but commandReadEndpoint==null");
                return new byte[0];
            }

            //try to read data
            ErrorCode errorCode = ErrorCode.None;
            try
            {
                //send read command
                byte[] readBuffer = new byte[COMMAND_READ_ENDPOINT_SIZE];
                int bytesRead;
                errorCode = commandReadEndpoint.Read(readBuffer, 5000, out bytesRead);

                //log
                string logString = "";
                foreach (byte b in readBuffer)
                    logString += b.ToString() + ",";

                //Logger.AddEntry(this, LogMessageType.ECoreInfo, "Answer received from HW: [" + logString + "]");

				//extract required data
				byte[] returnBuffer = new byte[length];
				for (int i = 0; i < length; i++) 
					returnBuffer[i] = readBuffer[i];

                //return read data
                return returnBuffer;
            }
            catch (Exception ex)
            {
                Logger.AddEntry(this, LogMessageType.ECoreError, "Reading control bytes failed");
                Logger.AddEntry(this, LogMessageType.ECoreWarning, "ExceptionMessage: " + ex.Message);
                Logger.AddEntry(this, LogMessageType.ECoreWarning, "USB ErrorCode: " + errorCode);
                Logger.AddEntry(this, LogMessageType.ECoreWarning, "requested length: " + length.ToString());

                return new byte[0];
            }   
        }

        public override byte[] GetData(int numberOfBytes)
        {
            //see if device is connected properly
            if (dataEndpoint == null)
            {
                Logger.AddEntry(this, LogMessageType.ECoreWarning, "Trying to stream data from device, but dataEndpoint==null");
                return new byte[0];
            }

            //try to read data
            ErrorCode errorCode = ErrorCode.None;
            try
            {
                //send read command
                byte[] readBuffer = new byte[numberOfBytes];
                int bytesRead;
                errorCode = dataEndpoint.Read(readBuffer, 10000, out bytesRead);

                // Asynchronously check for data
                /*
                UsbTransfer dataReadTransfer;
                errorCode = dataEndpoint.SubmitAsyncTransfer(readBuffer, 0, 4096, 100, out dataReadTransfer);
                if(errorCode != ErrorCode.None) throw new Exception("Failed to send async USB transfer");
                dataReadTransfer.AsyncWaitHandle.WaitOne(200);
                if (!dataReadTransfer.IsCompleted) dataReadTransfer.Cancel();
                errorCode = dataReadTransfer.Wait(out bytesRead);
                dataReadTransfer.Dispose();
                */

				if (tempFrameCounter++ < 10)
				{
					string dataString = "";
					for (int i = 0; i < 10; i++) {
						dataString = dataString + readBuffer[i].ToString ()+";";
					}
                    //This line causes a deadlock on the thread.join() called from the GUI thread - should be async somehow!
					//Logger.AddEntry (this, LogMessageType.ECoreInfo, numberOfBytes.ToString () + " " + errorCode.ToString () + "| Bytes received:" + dataString);
				}

                //return read data
                return readBuffer;
            }
            catch (Exception ex)
            {
                Logger.AddEntry(this, LogMessageType.ECoreError, "Streaming data from camera failed");
                Logger.AddEntry(this, LogMessageType.ECoreWarning, "ExceptionMessage: " + ex.Message);
                Logger.AddEntry(this, LogMessageType.ECoreWarning, "USB ErrorCode: " + errorCode);
                Logger.AddEntry(this, LogMessageType.ECoreWarning, "requested length: " + numberOfBytes.ToString());

                return new byte[0];
            }   
        }
                    
        public virtual void Dispose() 
        {/*
                UsbEndpointReader epReader = this as UsbEndpointReader;
                if (!ReferenceEquals(epReader, null))
                {
                    if (epReader.DataReceivedEnabled) epReader.DataReceivedEnabled = false;
                }
                Abort();
                mUsbDevice.ActiveEndpoints.RemoveFromList(this);
                */
            //scop3UsbDevice.ActiveEndpoints.Remove(commandReadEndpoint);
            //scop3UsbDevice.ActiveEndpoints.Remove(commandWriteEndpoint);
            //scop3UsbDevice.ActiveEndpoints.Remove(dataEndpoint);

        }

        ~HWInterfacePIC_LibUSB()
        {
            //int i = 0;

            if ((dataEndpoint != null) && (!dataEndpoint.IsDisposed))
            {
                /*
                dataEndpoint.Reset();
                commandWriteEndpoint.Reset();
                commandReadEndpoint.Reset();
                */
                if (dataEndpoint != null)
                {
                    dataEndpoint.Abort();
                    dataEndpoint.Dispose();
                    dataEndpoint = null;
                }
                if (commandWriteEndpoint != null)
                {
                    commandWriteEndpoint.Abort();
                    commandWriteEndpoint.Dispose();
                    commandWriteEndpoint = null;
                }

                if (commandReadEndpoint!= null)
                {
                    commandReadEndpoint.Abort();
                    commandReadEndpoint.Dispose();
                    commandReadEndpoint = null;
                }
                
                /*
                scop3UsbDevice.ActiveEndpoints.Remove(commandReadEndpoint);
                scop3UsbDevice.ActiveEndpoints.Remove(commandWriteEndpoint);
                scop3UsbDevice.ActiveEndpoints.Remove(dataEndpoint);
                */
                /*
                dataEndpoint = scop3UsbDevice.OpenEndpointReader(ReadEndpointID.Ep01);
                commandWriteEndpoint = scop3UsbDevice.OpenEndpointWriter(WriteEndpointID.Ep02);
                commandReadEndpoint = scop3UsbDevice.OpenEndpointReader(ReadEndpointID.Ep03);
                */
                if (scop3UsbDevice == null) return;
                
                scop3UsbDevice.Close();
                scop3UsbDevice = null;
                
                UsbDeviceNotifier.Enabled = false;
                UsbDevice.Exit();
            }
        }
#endif       
		public override bool Connected
		{
			get { return isConnected; }
		}

		public override void StartInterface()
		{            
		}

		public override void StopInterface()
		{
		}
    }
}

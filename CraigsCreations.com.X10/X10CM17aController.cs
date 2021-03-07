//<file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Craig Hamilton" email="x10library@craigscreations.com"/>
//     <version value="$version"/>
// </file>
using System;
using System.Threading;
using System.Collections;
using System.IO.Ports;
//using CraigsCreations.com.IO.Serial;

namespace CraigsCreations.com.X10
{
	/// <summary>
	/// This class controls the CM17a X10 Controller.  It is responsible for all
	/// IO operations between the Hardware and the Module classes.  It has implemented
	/// Some global functionality as defined in the IX10Controller Interface
	/// <see cref="CraigsCreations.com.X10.IX10Controller"/>
	/// </summary>
	public class X10CM17aController : IX10Controller
	{
		/// <summary>
		/// The CM17aData class facilitates formating the transmission stream.
		/// Data transmission for the CM17a has a set header and footer as well as
		/// 2 data sections.  The easiest way to translate the predefined bit patterns to
		/// transmission format was through the BitArray. Unfortunately this screws up order.
		/// Also the Header codes are reversed from the header codes in the CM11a module.
		/// This class reverses the Header code as well as the bitorder of the transmission
		/// itself.
		/// </summary>
		private class CM17aData {
			private const byte header1 = 0xD5; //Header 1
			private const byte header2 = 0xAA; //Header 2
			private const byte footer = 0xAD; //Footer
			private byte data1 = 0;
			private byte data2 = 0;

			/// <value>Data1 sets or returns a byte indicating the first data byte in the transmission</value>
			public byte Data1 {
				get {
					return(data1);
				}
				set {
					data1 = value;
				}
			}
			/// <value>Data2 sets or returns a byte indicating the second data byte in the transmission</value>
			public byte Data2 {
				get {
					return(data2);
				}
				set {
					data2 = value;
				}
			}
			/// <summary>
			/// Since BitArray does not have a Reverse functionality and for the purposes
			/// of this library I saw no need to extend BitArray I just created a simple reverse routine
			/// for reversing the bits in a bitarray
			/// </summary>
			/// <param name="bitstream">  A BitArray type</param>
			/// <param name="index">The starting Index for the reversal process</param>
			/// <param name="length">The number of bits to reverse from Index</param>
			private void ReverseStream(BitArray bitstream,int index,int length) {
				BitArray temp = new BitArray(length);
				int x=0;
				for(x=0;x<length;x++){
					temp[length-1-x] = bitstream[index+x];
				}
				for(x=0;x<length;x++){
					bitstream[index+x] = temp[x] ;
				}
			}
			/// <summary>
			/// Format the Data Blocks including the Header for CM17a Transmission
			/// </summary>
			/// <param name="bitstream">BitArray of CM17a data block</param>
			/// <returns>A properly formatted stream of bits for CM17a Transmission</returns>
			private BitArray FormatBuffer(BitArray bitstream) {
				//Reverse Entire Stream
				for(int x=0;x<5;x++) {
					ReverseStream(bitstream,x*8,8);
				}
				//Since HouseCode value is based on the CM11a the House Code needs
				//to be reversed for the CM17a
				ReverseStream(bitstream,16,4);
				return(bitstream);
			}
			/// <summary>
			/// Converts the header + data + footer data for the CM17a module to
			/// a bit array
			/// </summary>
			/// <returns>A BitArray representing a properly formated CM17a transmission</returns>
			public BitArray ToBitArray() {
				byte[] buffer = new byte[] { header1,header2,data1,data2,footer };
	
				return(FormatBuffer(new BitArray(buffer)));
			}
		}

		/// <summary>
		///	Device Codes for the CM17a as defined by the X10 Developer documentation 
		/// </summary>
		[Flags]
			private enum CM17aDeviceCode	{
			ONEON		= 0x00,
			ONEOFF		= 0x20,
			TWOON		= 0x10,
			TWOOFF		= 0x30,
			THREEON		= 0x08,
			THREEOFF	= 0x28,
			FOURON		= 0x18,
			FOUROFF		= 0x38,
			FIVEON		= 0x40,
			FIVEOFF		= 0x60,
			SIXON		= 0x50,
			SIXOFF		= 0x70,
			SEVENON		= 0x48,
			SEVENOFF	= 0x68,
			EIGHTON		= 0x58,
			EIGHTOFF	= 0x78,
			NINEON		= 0x00,
			NINEOFF		= 0x20,
			TENON		= 0x10,
			TENOFF		= 0x30,
			ELEVENON	= 0x08,
			ELEVENOFF	= 0x28,
			TWELVEON	= 0x18,
			TWELVEOFF	= 0x38,
			THIRTEENON	= 0x40,
			THIRTEENOFF	= 0x60,
			FOURTEENON	= 0x50,
			FOURTEENOFF	= 0x70,
			FIFTEENON	= 0x48,
			FIFTEENOFF	= 0x68,
			SIXTEENON	= 0x58,
			SIXTEENOFF	= 0x78
	}

		private SerialPort comm;				//Serial Port that CM17a is on
		private HouseCode x10HouseCode;			//X10 House Code
		private int transmissionRetries = 5;	//Number of times to retry to send command
		private const ControllerType controller = ControllerType.CM17A; //defines the controller type 

		/// <summary>
		///	Constructor for CM17a Controller object
		/// </summary>
		/// <param name="port">String representation of what COM port the controller is on</param>
		/// <param name="houseCode">Defines what HouseCode the object should control</param>
		public X10CM17aController(HouseCode houseCode,string port) {
			try {
				comm = new SerialPort(port);
				comm.Open();
				x10HouseCode = houseCode;
			} catch(System.ApplicationException) {
				throw(new ApplicationException("Error Initializing X10 Controller"));	
			}
		}
		/// <summary>
		/// Sends the CM17a Module into standby mode and leaves the CM17a powered up
		/// </summary>
		private void FirecrackerStandby() {
			comm.DtrEnable = true;
			comm.RtsEnable = true;
		}

		/// <summary>
		///	Removes power from the CM17a module and effectively resets the controller 
		/// </summary>
		private void FirecrackerReset() {
			comm.DtrEnable = false;
			comm.RtsEnable = false;
		}

		/// <summary>
		/// CM17a does not interpret sending bits is a standard serial manner.
		/// Rather it interprets a bit based on the powered/unpowered nature of RTS/DTR
		/// Bit transmission occour by having either
		/// RTS=on and DTR=off which is 1 or
		/// RTS=off and DTR=on which is 0
		/// </summary>
		/// <param name="bit">bit value to send</param>
		private void FirecrackerSendBit(bool bit) {
			if(bit) {
				comm.RtsEnable = true;
				comm.DtrEnable = false;
			} else {
				comm.DtrEnable = true;
				comm.RtsEnable = false;
			}
		}

		/// <summary>
		/// Initializes the CM17a module
		/// </summary>
		private void FirecrackerInit() {
			try {
				FirecrackerReset(); //Removes power from the controller returning it to origianl state
			} catch(ApplicationException) {
				throw(new ApplicationException("Error Reseting Firecracker"));
			}
			Thread.Sleep(1);
			try {
				FirecrackerStandby(); //Applies powers to the controller
			}catch(ApplicationException) {
				throw(new ApplicationException("Error Putting Firecracker into Standby"));
			}
			Thread.Sleep(150);
		}

		/// <summary>
		/// Between transmissions of sequential commands the CM17a must be reset.
		/// To allow for the previous command to be processed the system waits 70ms for the
		/// command to go before removing power
		/// </summary>
		private void FirecrackerSleep() {
			Thread.Sleep(70 /*300*/);
			try {
				FirecrackerReset();
			} catch(ApplicationException) {
				throw(new ApplicationException("Error putting Firecracker to sleep"));
			}
		}

		/// <summary>
		/// Sends the data stream to the CM17a
		/// </summary>
		/// <param name="cmd">the Command to be sent to the CM17a</param>
		private void Send(CM17aData cmd) {
			BitArray bitstream = cmd.ToBitArray(); //Convert command to bit array

			FirecrackerInit(); //Initialize CM17a
			foreach(bool bit in bitstream) {
				try {
					FirecrackerSendBit(bit); //Send one bit to CM17a
					Thread.Sleep(1);
					FirecrackerStandby(); 
					Thread.Sleep(1);
				} catch(ApplicationException) {
					throw(new ApplicationException("Error Transmitting Stream"));
				}
			}
			FirecrackerSleep();
		}

		/// <summary>
		/// Sends more complicated commands to CM17a
		/// Dim/Bright functions only work in increments of 5%. In order to raise the
		/// lighting by 30 percent the Bright command would have to be sent 6 times
		/// </summary>
		/// <param name="cmdAddress">Cmd to address the Device Code to be operated on</param>
		/// <param name="cmdDimBright">Cmd to Dim or Brighten Lamp Modules</param>
		/// <param name="lightLevel">Percentage to brighten or lower.</param>
		private void Send(CM17aData cmdAddress,CM17aData cmdDimBright,int lightLevel) {
			FirecrackerInit();

			System.Diagnostics.Debug.WriteLine("Sending cmdAddress");
			BitArray bitstream = cmdAddress.ToBitArray(); //Convert command to bit array

			FirecrackerInit(); //Initialize CM17a
			foreach(bool bit in bitstream) {
				try {
					FirecrackerSendBit(bit); //Send one bit to CM17a
					Thread.Sleep(1);
					FirecrackerStandby();
					Thread.Sleep(1);
				} catch(ApplicationException) {
					throw(new ApplicationException("Error Transmitting Stream"));
				}
			}
			System.Diagnostics.Debug.WriteLine("Finished Sending cmdAddress");

			//Start sending the Dim / Bright Command
			bitstream = cmdDimBright.ToBitArray();
			for(int x=0;x<lightLevel;x++) { //For each Dim / Bright Command
				System.Diagnostics.Debug.WriteLine("Sending cmdDimBright");
				Thread.Sleep(500); //Give the system a chance to process
				foreach(bool bit in bitstream) {
					try {
						FirecrackerSendBit(bit);
						Thread.Sleep(1);
						FirecrackerStandby();
						Thread.Sleep(1);
					} catch(ApplicationException) {
						throw(new ApplicationException("Error Transmitting Stream"));
					}
				}
				System.Diagnostics.Debug.WriteLine("Finished Sending cmdAddress");
			}
			FirecrackerSleep();
		}

		/// <summary>
		/// The CM71a has the device and function code intertwined with each other.
		/// Instead of computing what the bit values programatically should be these values
		/// have been predefined in the CM1aDeviceCode Enumeration <see cref="CraigsCreations.com.X10.X10CM17aController.CM17aDeviceCode"/>
		/// This enumeration has been defined from the development documentaion provided by X10
		/// </summary>
		/// <param name="deviceCode">Device Code to be operated on</param>
		/// <param name="function">Function to be performed</param>
		/// <returns>Returns a CM17aDeviceCode enumeration value</returns>
		private CM17aDeviceCode GetCM17aDeviceCode(int deviceCode,Function function) {
			switch(deviceCode) {
			case 1:
				if(Function.On==function) {
					return(CM17aDeviceCode.ONEON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.ONEOFF);
				}
				break;
			case 2:
				if(Function.On==function) {
					return(CM17aDeviceCode.TWOON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.TWOOFF);
				}
				break;
			case 3:
				if(Function.On==function) {
					return(CM17aDeviceCode.THREEON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.THREEOFF);
				}
				break;
			case 4:
				if(Function.On==function) {
					return(CM17aDeviceCode.FOURON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.FOUROFF);
				}
				break;
			case 5:
				if(Function.On==function) {
					return(CM17aDeviceCode.FIVEON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.FIVEOFF);
				}
				break;
			case 6:
				if(Function.On==function) {
					return(CM17aDeviceCode.SIXON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.SIXOFF);
				}
				break;
			case 7:
				if(Function.On==function) {
					return(CM17aDeviceCode.SEVENON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.SEVENOFF);
				}
				break;
			case 8:
				if(Function.On==function) {
					return(CM17aDeviceCode.EIGHTON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.EIGHTOFF);
				}
				break;
			case 9:
				if(Function.On==function) {
					return(CM17aDeviceCode.NINEON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.NINEOFF);
				}
				break;
			case 10:
				if(Function.On==function) {
					return(CM17aDeviceCode.TENON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.TENOFF);
				}
				break;
			case 11:
				if(Function.On==function) {
					return(CM17aDeviceCode.ELEVENON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.ELEVENOFF);
				}
				break;
			case 12:
				if(Function.On==function) {
					return(CM17aDeviceCode.TWELVEON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.TWELVEOFF);
				}
				break;
			case 13:
				if(Function.On==function) {
					return(CM17aDeviceCode.THIRTEENON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.THIRTEENOFF);
				}
				break;
			case 14:
				if(Function.On==function) {
					return(CM17aDeviceCode.FOURTEENON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.FOURTEENOFF);
				}
				break;
			case 15:
				if(Function.On==function) {
					return(CM17aDeviceCode.FIFTEENON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.FIFTEENOFF);
				}
				break;
			case 16:
				if(Function.On==function) {
					return(CM17aDeviceCode.SIXTEENON);
				} else if(Function.Off==function) {
					return(CM17aDeviceCode.SIXTEENOFF);
				}
				break;
			}
			throw(new ApplicationException("Device Code out of range or Invalid Function"));
		}

		#region IX10Controller Members

		/// <summary>
		/// This is the interface for actually sending individual commands to a particular device
		/// </summary>
		/// <param name="deviceCode">Device code to be operated on</param>
		/// <param name="deviceCommand">Command to send to device</param>
		/// <param name="lightLevel">if Dimming or Brightening then this is a percentage to raise or lower
		/// light level.  If not Dimming or Brightening then this parameter is ignored </param>
		public void ControlDevice(int deviceCode, Function deviceCommand, int lightLevel) {
			int actualDeviceCode = 0;
			byte[] buffer = new byte[5];
			
			if(Function.AllUnitsOff==deviceCommand) {
				TurnAllUnitsOff();
			} else if(Function.AllLightsOn==deviceCommand) {
				TurnAllLampsOn();
			} else if(Function.AllLightsOff==deviceCommand) {
				TurnAllLampsOff();
			} else if(Function.Dim==deviceCommand || Function.Bright==deviceCommand) {
				actualDeviceCode = (int) GetCM17aDeviceCode(deviceCode,Function.On); //Determinde device code transmission value
				CM17aData cmdAddress = new CM17aData();
				//Initializes the 2 data blocks to values to be transmitted to CM17a
				cmdAddress.Data1 = (byte) (((byte)x10HouseCode) | ((byte)((deviceCode>8) ? 0x04 : 0x00)));
				cmdAddress.Data2 = (byte) actualDeviceCode;

				CM17aData cmdDimBright = new CM17aData();
				//bright = 0x88 dim = 0x98 
				actualDeviceCode = (Function.Dim==deviceCommand) ? 0x98 : 0x88;
				cmdDimBright.Data1 = (byte) x10HouseCode;
				cmdDimBright.Data2 = (byte) actualDeviceCode;
				lightLevel = (lightLevel > 100) ? 100 : lightLevel;
				if (lightLevel <= 0) { throw(new ApplicationException("Error Dimming/Brightening. Amount to Dim/Brighten must be > 0")); }
				Send(cmdAddress,cmdDimBright,lightLevel/5);
			} else if(Function.On==deviceCommand || Function.Off==deviceCommand){
				actualDeviceCode = (int) GetCM17aDeviceCode(deviceCode,deviceCommand);
				CM17aData cmd = new CM17aData();
				cmd.Data1 = (byte) (((byte)x10HouseCode) | ((byte)((deviceCode>8) ? 0x04 : 0x00)));
				cmd.Data2 = (byte) actualDeviceCode;
				Send(cmd);
			} else {
				throw(new ApplicationException("Unsupported Function"));
			}

		}
		/// <summary>
		/// Sends the Turn All Units Off code
		/// </summary>
		public void TurnAllUnitsOff() {
			CM17aData cmd = new CM17aData();
			cmd.Data1 = (byte) x10HouseCode;
			cmd.Data2 = (byte) 0x80; //0x40;
			Send(cmd);
		}

		/// <summary>
		/// Sends the Turn All Lamps On Signal
		/// </summary>
		public void TurnAllLampsOn() {
			CM17aData cmd = new CM17aData();
			cmd.Data1 = (byte) x10HouseCode;
			cmd.Data2 = (byte) 0x90;
			Send(cmd);
		}

		/// <summary>
		/// Turn All Lamps Off call the TurnAllUntisOff command.
		/// There is a seperate signal for turn all lamps off but does not seem to
		/// work on the CM17a or CM11a controllers
		/// </summary>
		public void TurnAllLampsOff() {
			TurnAllUnitsOff();
		}

		/// <value>Returns the ControllerType of this object.</value>
		public ControllerType Controller {
			get {
				return(controller);
			}
		}

		#endregion
	}
}

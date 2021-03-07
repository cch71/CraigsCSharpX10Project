//<file>
//     <copyright see="prj:///doc/copyright.txt"/>
//     <license see="prj:///doc/license.txt"/>
//     <owner name="Craig Hamilton" email="x10library@craigscreations.com"/>
//     <version value="$version"/>
// </file>
using System;

namespace CraigsCreations.com.X10
{
	/// <summary>
	/// Set of Global enumeration that support the X10 system.
	/// </summary>
	[Flags]
	public enum HouseCode {
		A = 0x60,
		B = 0xE0,
		C = 0x20,
		D = 0xA0,
		E = 0x10,
		F = 0x90,
		G = 0x50,
		H = 0xD0,
		I = 0x70,
		J = 0xF0,
		K = 0x30,
		L = 0xB0,
		M = 0X00,
		N = 0x80,
		O = 0x40,
		P = 0xC0
	}

	[Flags]
	public enum Function {
		AllUnitsOff = 0x00,
		AllLightsOn = 0x01,
		On = 0x02,
		Off = 0x03,
		Dim = 0x04,
		Bright = 0x05,
		AllLightsOff = 0x06,
		ExtendedCode = 0x07,
		HailRequest = 0x08,
		HailAcknowledge = 0x09,
		PresetDim1 = 0x0A,
		PresetDim2 = 0x0B,
		ExtededDataTransfer = 0x0C,
		StatusOn = 0X0D,
		StatusOff = 0x0E,
		StatusRequest = 0x0F
	}

	public enum ControllerType { CM11A,CM17A }
}

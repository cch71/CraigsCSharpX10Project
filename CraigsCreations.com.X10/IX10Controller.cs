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
	/// Summary description for IX10Controller.
	/// </summary>
	public interface IX10Controller {
		/// <summary>
		/// Controls individual devices
		/// </summary>
		/// <param name="deviceCode">Device Code to be operated on</param>
		/// <param name="deviceCommand">Command to send to device</param>
		/// <param name="lightLevel">Ammount to lower or raise light levels for lamp modules</param>
		void ControlDevice(int deviceCode,Function deviceCommand,int lightLevel);
		/// <summary>
		/// Turns all X10 controlled Devices Off.
		/// </summary>
		void TurnAllUnitsOff();
		/// <summary>
		/// Turns all X10 controlled Lamps On.
		/// </summary>
		void TurnAllLampsOn();
		/// <summary>
		/// Turns all X10 controlled Lamps Off.
		/// </summary>
		void TurnAllLampsOff();
		/// <value>Returns the ControllerType of this object.</value>
		ControllerType Controller {
			get;
		}
	}
}

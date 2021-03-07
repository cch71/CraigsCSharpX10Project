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
	/// X10Application is a class desinged to handle funtions relating to the Application Module.
	/// </summary>
	public class X10Application
	{
		/// <remarks>Application Device Code</remarks>
		protected int deviceCode;
		/// <remarks>x10Controller object that controls this device </remarks>
		protected IX10Controller x10Controller;

		/// <summary>
		/// X10Application Constructor. Initializes the internal x10Controller and deviceCode variables
		/// The <paramref name="controller"/> Parameter takes an object that has implemented the
		/// IX10Controller interface.
		/// The <paramref name="code"/> Parameter takes an Integer setting the device code of the lamp
		/// module.
		/// </summary>
		public X10Application(IX10Controller controller,int code) {
			x10Controller = controller;
			deviceCode = code;
		}
		
		/// <value>DeviceCode sets or returns the assigned Device Code </value>
		public int DeviceCode {
			get {
				return(deviceCode);
			}
			set {
				deviceCode = value;
			}
		}

		/// <value>Controller sets or returns the assigned X10 Controller Object </value>
		public IX10Controller Controller {
			get {
				return(x10Controller);
			}
			set {
				x10Controller = value;
			}
		}

		/// <summary>
		/// Turns the Application Module On.
		/// </summary>
		public virtual void On() {
			x10Controller.ControlDevice(deviceCode,Function.On,0);
		}

		/// <summary>
		/// Turns the Application Module Off.
		/// </summary>
		public virtual void Off() {
			x10Controller.ControlDevice(deviceCode,Function.Off,0);
		}
	}
}

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
	/// X10Lamp is a class desinged to handle funtions relating to the Lamp Module.
	/// It is inherited from X10Application as the X10Lamp has all the functionality
	/// of the Application Module with the addision of Dim/Bright Capabilities
	/// </summary>
	public class X10Lamp : X10Application
	{
		/// <summary>
		/// Light Level of Lamp module
		/// </summary>
		protected int lightLevel;

		/// <summary>
		/// X10Lamp Constructor.  This initializes the light level to zero and passes the controller
		/// and device code to the application constructor.
		/// The <paramref name="controller"/> Parameter takes an object that has implemented the
		/// IX10Controller interface.
		/// The <paramref name="code"/> Parameter takes an Integer setting the device code of the lamp
		/// module.
		/// </summary>
		/// <remarks>
		/// The dimming/brighening functions depend on the lamp module being off or light level at 0
		/// when module is instantiated.  If this is not the case then the dimm/brighten function may
		/// perform strangely until the device has been turned off/on by this interface.
		/// Also since there is no feed back from module dimming or brightening through other
		/// remote controllers will cause problems.
		/// </remarks>
		public X10Lamp(IX10Controller controller,int code) : base(controller,code)
		{
			lightLevel = 0;
		}

		/// <value>LightLevel gets or sets the Light Level (by percentage) of the lamp module</value>
		public int LightLevel {
			get{
				return(lightLevel);
			}
			set{
				if(value==lightLevel) {
					return;
				} else if(lightLevel>value) {
					if(0>value) { value = 0; }
					x10Controller.ControlDevice(deviceCode,Function.Dim,lightLevel-value);
				} else if(value>lightLevel) {
					if(value>100) { value = 100; }
					x10Controller.ControlDevice(deviceCode,Function.Bright,value-lightLevel);
				}
				lightLevel = value;
			}
		}

		/// <summary>
		/// Dims the Lamp Module by 5 percent.
		/// </summary>
		public virtual void Dim() {
			Dim(5);
		}

		/// <summary>
		/// Brightens the Lamp Module by 5 percent.
		/// </summary>
		public virtual void Brighten() {
			Brighten(5);
		}

		/// <summary>
		/// Dims the Lamp Module by the percentage passed in.
		/// The <paramref name="Level"/> takes an integer representing the level to Dim
		/// </summary>
		public virtual void Dim(int Level) {
			LightLevel = lightLevel - Level;
		}

		/// <summary>
		/// Brightens the Lamp Module by the percentage passed in.
		/// The <paramref name="Level"/> takes an integer representing the level to Brighten.
		/// </summary>
		public virtual void Brighten(int Level) {
			LightLevel = lightLevel + Level;
		}

		/// <summary>
		/// Turns the Lamp Module off and sets the Light Level to zero 
		/// </summary>
		public override void Off() {
			base.Off ();
			//TODO: Can not seem to brighten if Lamp Module has been turned off although if
			//dimmed to zero this does not seem to cause a problem.
			lightLevel = 0;
		}
	
		/// <summary>
		/// Turns the Lamp Module on and sets the Light Level to 100 percent.
		/// </summary>
		public override void On() {
			base.On ();
			lightLevel = 100;
		}
	}
}

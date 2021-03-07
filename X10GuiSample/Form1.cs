using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using CraigsCreations.com.X10;

namespace X10Controller
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.TrackBar trackBar1;
		private IX10Controller x10;
		private bool bMouseDown = false;
		private int dimValue = 0;
		private System.Windows.Forms.Button button4;
		private X10Lamp Lamp2;


		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
//			x10 = new X10CM11aController(HouseCode.A,"COM2");
			x10 = new X10CM17aController(HouseCode.A,"COM1");
			Lamp2 = new X10Lamp(x10,2);
			trackBar1.Enabled=false;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.trackBar1 = new System.Windows.Forms.TrackBar();
			this.button4 = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(0, 0);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(96, 80);
			this.button1.TabIndex = 0;
			this.button1.Text = "Turn All Lamps On";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(96, 0);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(96, 80);
			this.button2.TabIndex = 1;
			this.button2.Text = "Turn All Lamps Off";
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(0, 88);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(96, 88);
			this.button3.TabIndex = 2;
			this.button3.Text = "Lamp 2 On";
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// trackBar1
			// 
			this.trackBar1.LargeChange = 1;
			this.trackBar1.Location = new System.Drawing.Point(216, 24);
			this.trackBar1.Maximum = 20;
			this.trackBar1.Name = "trackBar1";
			this.trackBar1.Orientation = System.Windows.Forms.Orientation.Vertical;
			this.trackBar1.RightToLeft = System.Windows.Forms.RightToLeft.No;
			this.trackBar1.Size = new System.Drawing.Size(42, 200);
			this.trackBar1.TabIndex = 3;
			this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.Both;
			this.trackBar1.Value = 20;
			this.trackBar1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trackBar1_MouseUp);
			this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
			this.trackBar1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.trackBar1_MouseDown);
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(96, 88);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(96, 88);
			this.button4.TabIndex = 4;
			this.button4.Text = "Lamp 2 Off";
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 266);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.trackBar1);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.Form1_Load);
			((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void button1_Click(object sender, System.EventArgs e) {
			try {
				x10.TurnAllLampsOn();
			} catch(System.ApplicationException err) {
				MessageBox.Show(err.Message,"X10 Error");
			}
		}

		private void button2_Click(object sender, System.EventArgs e) {
			try {
				x10.TurnAllLampsOff();
			} catch(System.ApplicationException err) {
				MessageBox.Show(err.Message,"X10 Error");
			}
		}

		private void button3_Click(object sender, System.EventArgs e) {
			Lamp2.On();
			dimValue = 20;
			trackBar1.Value=20;
			trackBar1.Enabled=true;
		}

		private void trackBar1_ValueChanged(object sender, System.EventArgs e) {
			if(!bMouseDown) {
				if(trackBar1.Value!=dimValue) {
					System.Diagnostics.Debug.WriteLine("Track Value " + trackBar1.Value);
					if(trackBar1.Value>dimValue) {
						Lamp2.Brighten( (trackBar1.Value-dimValue) * 5);
					} else {
						Lamp2.Dim((dimValue-trackBar1.Value) *5);
					}
					dimValue = trackBar1.Value;
				}
			}
		}

		private void trackBar1_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e) {
			bMouseDown = false;
			trackBar1_ValueChanged(sender,e);
		}

		private void trackBar1_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
			bMouseDown = true;
		}

		private void button4_Click(object sender, System.EventArgs e) {
			Lamp2.Off();
			dimValue = 0;
			trackBar1.Value=0;
			trackBar1.Enabled=false;
		}

		private void Form1_Load(object sender, System.EventArgs e) {
		
		}
	}
}

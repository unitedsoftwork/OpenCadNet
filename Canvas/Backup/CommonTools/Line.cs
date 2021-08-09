using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices; 

namespace CommonTools
{
	class Line : Control
	{
		AnchorStyles m_linePositions = AnchorStyles.Bottom;
		public AnchorStyles LinePositions
		{
			get { return m_linePositions; }
			set
			{
				m_linePositions = value;
				Invalidate();
			}
		}
		public Line()
		{
			TabStop = false;
		    SetStyle(ControlStyles.Selectable, false);
			SetStyle(ControlStyles.UserPaint, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			SetStyle(ControlStyles.SupportsTransparentBackColor, true);
		    SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			//WndUtil.SetWindowLong(Handle, Win32Defines.GWL_EXSTYLE, Win32Defines.WS_EX_TRANSPARENT);
		}
		protected override void OnPaintBackground(PaintEventArgs pevent)
		{
			base.OnPaintBackground(pevent);
		}
		protected override void OnPaint(PaintEventArgs e)
		{
			//base.OnPaint(e);
			Rectangle r = ClientRectangle;
			r.Inflate(-2,-2);
			Pen p = new Pen(ForeColor, 1);
			if ((int)(LinePositions & AnchorStyles.Left) > 0)
			{
				e.Graphics.DrawLine(p, r.Left-1, r.Top, r.Left-1, r.Bottom-1);
				e.Graphics.DrawLine(Pens.WhiteSmoke, r.Left, r.Top, r.Left, r.Bottom-1);
			}
			if ((int)(LinePositions & AnchorStyles.Right) > 0)
			{
				e.Graphics.DrawLine(p, r.Right, r.Top, r.Right, r.Bottom-1);
				e.Graphics.DrawLine(Pens.WhiteSmoke, r.Right+1, r.Top, r.Right+1, r.Bottom-1);
			}
			if ((int)(LinePositions & AnchorStyles.Top) > 0)
			{
				e.Graphics.DrawLine(p, r.Left, r.Top-1, r.Right-1, r.Top-1);
				e.Graphics.DrawLine(Pens.WhiteSmoke, r.Left, r.Top, r.Right-1, r.Top);
			}
			if ((int)(LinePositions & AnchorStyles.Bottom) > 0)
			{
				e.Graphics.DrawLine(p, r.Left, r.Bottom, r.Right-1, r.Bottom);
				e.Graphics.DrawLine(Pens.WhiteSmoke, r.Left, r.Bottom+1, r.Right-1, r.Bottom+1);
			}
			p.Dispose();
		}
	}

	public class WndUtil
	{
		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewLong);
	}

	public class Win32Defines
	{
		public const int SC_SIZE         			= 0xF000;
		public const int SC_MOVE         			= 0xF010;
		public const int SC_MINIMIZE     			= 0xF020;
		public const int SC_MAXIMIZE     			= 0xF030;
		public const int SC_NEXTWINDOW   			= 0xF040;
		public const int SC_PREVWINDOW   			= 0xF050;
		public const int SC_CLOSE        			= 0xF060;
		public const int SC_VSCROLL      			= 0xF070;
		public const int SC_HSCROLL      			= 0xF080;
		public const int SC_MOUSEMENU    			= 0xF090;
		public const int SC_KEYMENU      			= 0xF100;
		public const int SC_ARRANGE      			= 0xF110;
		public const int SC_RESTORE      			= 0xF120;
		public const int SC_TASKLIST     			= 0xF130;
		public const int SC_SCREENSAVE   			= 0xF140;
		public const int SC_HOTKEY       			= 0xF150;

		public const int GWL_WNDPROC     			= (-4);
		public const int GWL_HINSTANCE   			= (-6);
		public const int GWL_HWNDPARENT  			= (-8);
		public const int GWL_STYLE       			= (-16);
		public const int GWL_EXSTYLE     			= (-20);
		public const int GWL_USERDATA    			= (-21);
		public const int GWL_ID          			= (-12);

		public const int	WS_OVERLAPPED			= (int)0x00000000;
		public const UInt32 WS_POPUP				= (UInt32)0x80000000;
		public const int	WS_CHILD				= (int)0x40000000;
		public const int	WS_VISIBLE				= 0x10000000;
		public const int	WS_DISABLED				= 0x08000000;

		public const int	WS_EX_DLGMODALFRAME     = 0x00000001;
		public const int	WS_EX_NOPARENTNOTIFY    = 0x00000004;
		public const int	WS_EX_TOPMOST           = 0x00000008;
		public const int	WS_EX_ACCEPTFILES       = 0x00000010;
		public const int	WS_EX_TRANSPARENT       = 0x00000020;
		public const int	WS_EX_MDICHILD          = 0x00000040;
		public const int	WS_EX_TOOLWINDOW        = 0x00000080;
		public const int	WS_EX_WINDOWEDGE        = 0x00000100;
		public const int	WS_EX_CLIENTEDGE        = 0x00000200;
		public const int	WS_EX_CONTEXTHELP       = 0x00000400;
		public const int	WS_EX_RIGHT             = 0x00001000;
		public const int	WS_EX_LEFT              = 0x00000000;
		public const int	WS_EX_RTLREADING        = 0x00002000;
		public const int	WS_EX_LTRREADING        = 0x00000000;
		public const int	WS_EX_LEFTSCROLLBAR     = 0x00004000;
		public const int	WS_EX_RIGHTSCROLLBAR    = 0x00000000;
		public const int	WS_EX_CONTROLPARENT     = 0x00010000;
		public const int	WS_EX_STATICEDGE        = 0x00020000;
		public const int	WS_EX_APPWINDOW         = 0x00040000;

		public const int	HTNOWHERE           =0;
		public const int	HTCLIENT            =1;
		public const int	HTCAPTION           =2;
		public const int	HTSYSMENU           =3;
		public const int	HTGROWBOX           =4;
		public const int	HTSIZE              =HTGROWBOX;
		public const int	HTMENU              =5;
		public const int	HTHSCROLL           =6;
		public const int	HTVSCROLL           =7;
		public const int	HTMINBUTTON         =8;
		public const int	HTMAXBUTTON         =9;
		public const int	HTLEFT              =10;
		public const int	HTRIGHT             =11;
		public const int	HTTOP               =12;
		public const int	HTTOPLEFT           =13;
		public const int	HTTOPRIGHT          =14;
		public const int	HTBOTTOM            =15;
		public const int	HTBOTTOMLEFT        =16;
		public const int	HTBOTTOMRIGHT       =17;
		public const int	HTBORDER            =18;

		public const int	SWP_NOSIZE          =0x0001;
		public const int	SWP_NOMOVE          =0x0002;
		public const int	SWP_NOZORDER        =0x0004;
		public const int	SWP_NOREDRAW        =0x0008;
		public const int	SWP_NOACTIVATE      =0x0010;
		public const int	SWP_FRAMECHANGED    =0x0020;  /* The frame changed: send WM_NCCALCSIZE */
		public const int	SWP_SHOWWINDOW      =0x0040;
		public const int	SWP_HIDEWINDOW      =0x0080;
		public const int	SWP_NOCOPYBITS      =0x0100;
		public const int	SWP_NOOWNERZORDER   =0x0200;  /* Don't do owner Z ordering */
		public const int	SWP_NOSENDCHANGING  =0x0400;  /* Don't send WM_WINDOWPOSCHANGING */

		public const int	HWND_TOP        =0;
		public const int	HWND_BOTTOM     =1;
		public const int	HWND_TOPMOST    =-1;
		public const int	HWND_NOTOPMOST  =-2;

		public const int	HH_DISPLAY_TOPIC	= 0;
		public const int	HH_DISPLAY_TOC		= 1;
		public const int	HH_DISPLAY_INDEX	= 2;
		public const int	HH_DISPLAY_SEARCH	= 3;
		public const int	HH_HELP_CONTEXT		= 0x0F;
		public const int	HH_CLOSE_ALL		= 0x12;
	}

}

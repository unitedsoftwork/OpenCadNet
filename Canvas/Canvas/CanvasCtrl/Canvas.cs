using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Web.UI.WebControls;
using System.Drawing.Drawing2D;
using System.Linq;

namespace Canvas
{
	public struct CanvasWrapper : ICanvas
	{
		CanvasCtrl m_canvas; 
		Graphics m_graphics;
		Rectangle m_rect;
		public CanvasWrapper(CanvasCtrl canvas)
		{
			m_canvas = canvas;
			m_graphics = canvas.CreateGraphics();
			m_rect = new Rectangle();
		}
		public CanvasWrapper(CanvasCtrl canvas, Graphics graphics, Rectangle clientrect)
		{
			m_canvas = canvas;
			m_graphics = graphics;
			m_rect = clientrect;
		}
		public IModel Model
		{
			get { return m_canvas.Model; }
		}
		public CanvasCtrl CanvasCtrl
		{
			get { return m_canvas; }
		}
		public void Dispose()
		{
			m_graphics = null;
		}
		#region ICanvas Members
		public IModel DataModel
		{
			get { return m_canvas.Model; }
		}
		public UnitPoint ScreenTopLeftToUnitPoint()
		{
			return m_canvas.ScreenTopLeftToUnitPoint();
		}
		public UnitPoint ScreenBottomRightToUnitPoint()
		{
			return m_canvas.ScreenBottomRightToUnitPoint();
		}
		public PointF ToScreen(UnitPoint unitpoint)
		{
			return m_canvas.ToScreen(unitpoint);
		}
		public float ToScreen(double unitvalue)
		{
			return m_canvas.ToScreen(unitvalue);
		}
		public double ToUnit(float screenvalue)
		{
			return m_canvas.ToUnit(screenvalue);
		}
		public UnitPoint ToUnit(PointF screenpoint)
		{
			return m_canvas.ToUnit(screenpoint);
		}
		public Graphics Graphics
		{
			get { return m_graphics; }
		}
		public Rectangle ClientRectangle
		{
			get { return m_rect; }
			set { m_rect = value; }
		}
		public Pen CreatePen(Color color, float unitWidth)
		{
			return m_canvas.CreatePen(color, unitWidth);
		}
		public void DrawLine(ICanvas canvas, Pen pen, UnitPoint p1, UnitPoint p2)
		{
			m_canvas.DrawLine(canvas, pen, p1, p2);
		}
		public void DrawArc(ICanvas canvas, Pen pen, UnitPoint center, float radius, float beginangle, float angle)
		{
			m_canvas.DrawArc(canvas, pen, center, radius, beginangle, angle);
		}
        public void DrawText(ICanvas canvas, Pen pen, UnitPoint position,string text,Font font,float rotation)
        {
            m_canvas.DrawText(canvas,  pen, position,text,font,rotation);
        }
        public void DrawMultilineText(ICanvas canvas, Pen pen, UnitPoint position, string text, Font font, float rotation)
        {
            m_canvas.DrawMultilineText( canvas,  pen,  position,  text,  font,  rotation);
        }
        public void DrawSolid(ICanvas canvas, Pen pen, UnitPoint p1, UnitPoint p2, UnitPoint p3, UnitPoint p4)
        {
            m_canvas.DrawSolid( canvas,  pen,  p1,  p2,  p3,  p4);
        }
        public void Invalidate()
		{
			m_canvas.DoInvalidate(false);
		}
		public IDrawObject CurrentObject
		{
			get { return m_canvas.NewObject; }
		}
		#endregion
	}
	public partial class CanvasCtrl : UserControl
	{

		enum eCommandType
		{
			select,
			lasso_select,
			pan,
			move,
			draw,
			edit,
			editNode,
		}

		ICanvasOwner		m_owner;
		CursorCollection	m_cursors = new CursorCollection();
		public IModel				m_model;
		MoveHelper			m_moveHelper = null;
		NodeMoveHelper		m_nodeMoveHelper = null;
		CanvasWrapper		m_canvaswrapper;
		eCommandType		m_commandType = eCommandType.select;
		bool				m_runningSnaps = true;
		Type[]				m_runningSnapTypes = null;
		PointF				m_mousedownPoint;
		IDrawObject			m_newObject = null;
		IEditTool			m_editTool = null;
		SelectionShape	m_selection = null;
		string				m_drawObjectId = string.Empty;
		string				m_editToolId = string.Empty;
		Bitmap				m_staticImage = null;
		bool				m_staticDirty = true;
		ISnapPoint			m_snappoint = null;
		
		public Type[] RunningSnaps
		{
			get { return m_runningSnapTypes; }
			set { m_runningSnapTypes = value; }
		}
		public bool RunningSnapsEnabled
		{
			get { return m_runningSnaps; }
			set { m_runningSnaps = value; }
		}
        public bool CenterOnScreen { get; set; } = false;
        System.Drawing.Drawing2D.SmoothingMode	m_smoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
		public System.Drawing.Drawing2D.SmoothingMode SmoothingMode
		{
			get { return m_smoothingMode; }
			set { m_smoothingMode = value;}
		}
     
		public IModel Model
		{
			get { return m_model; }
			set { m_model = value; }
		}

		public CanvasCtrl(ICanvasOwner owner, IModel datamodel,bool centerscreen)
		{
			m_canvaswrapper = new CanvasWrapper(this);

			m_owner = owner;
			m_model = datamodel;

           

         

            InitializeComponent();
			this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
          
			m_commandType = eCommandType.select;
			m_cursors.AddCursor(eCommandType.select, Cursors.Arrow);
			m_cursors.AddCursor(eCommandType.draw, Cursors.Cross);
			m_cursors.AddCursor(eCommandType.pan, "hmove.cur");
			m_cursors.AddCursor(eCommandType.move, Cursors.SizeAll);
			m_cursors.AddCursor(eCommandType.edit, Cursors.Cross);
			UpdateCursor();

			m_moveHelper = new MoveHelper(this);
			m_nodeMoveHelper = new NodeMoveHelper(m_canvaswrapper);
            this.CenterOnScreen = centerscreen;
  
        }
		public UnitPoint GetMousePoint()
		{
			Point point = this.PointToClient(Control.MousePosition);
			return ToUnit(point);
		}
		public void SetCenter(UnitPoint unitPoint)
		{
			PointF point = ToScreen(unitPoint);
			m_lastCenterPoint = unitPoint;
			SetCenterScreen(point, false);
		}
		public void SetCenter()
		{
			Point point = this.PointToClient(Control.MousePosition);
			SetCenterScreen(point, true);
		}
		public UnitPoint GetCenter()
		{
			return ToUnit(new PointF(this.ClientRectangle.Width/2, this.ClientRectangle.Height/2));
		}
        protected void SetCenterScreen(PointF screenPoint, bool setCursor)
        {
            float centerX = ClientRectangle.Width / 2;
            m_panOffset.X += centerX - screenPoint.X;

            float centerY = ClientRectangle.Height / 2;
            m_panOffset.Y += centerY - screenPoint.Y;

            if (setCursor)
                Cursor.Position = this.PointToScreen(new Point((int)centerX, (int)centerY));
            DoInvalidate(true);
        }

        static void findPoint(int x1, int y1,
                      int x2, int y2)
        {
            Console.WriteLine("(" + (int)(2 * x2 - x1)
                   + "," + (int)(2 * y2 - y1) + " )");
        }



        protected void ZoomToPoint(MouseEventArgs e)
        {




			UnitPoint zoom_model_point = GetMousePoint();

			UnitPoint upper_left = ToUnit(new PointF(0, 0));
			PointF unit_zoom_pt = ToScreen(GetMousePoint());

			UnitPoint center_pre_zoom = CenterPointUnit();
			double wheel = e.Delta < 0 ? -1 : 1;
			float zoom = (float)Math.Exp(wheel * 0.2);

			float old_zoom = m_model.Zoom;
			float new_zoom = m_model.Zoom * zoom;
			m_model.Zoom = new_zoom;
			UnitPoint center_post_zoom = CenterPointUnit();
			SetCenterScreen(ToScreen(zoom_model_point), false);
			return;

			UnitPoint new_zoom_model_point = GetMousePoint();

			UnitPoint new_upper_left = ToUnit(new PointF(0, 0));

			UnitPoint zoom_model_point_difference = zoom_model_point - new_zoom_model_point;

			m_panOffset.X -= ToScreen(zoom_model_point_difference.X);
			m_panOffset.Y -=ToScreen(zoom_model_point_difference.Y);
			this.CreateGraphics().FillPie(Brushes.Red, unit_zoom_pt.X, unit_zoom_pt.Y, 30, 30, 0, 359);
            this.CreateGraphics().FillPie(Brushes.Wheat, 0,0 , 30, 30, 0, 359);

			//zoom point - upper left = 
			PointF origin = ToScreen(new UnitPoint(0, 0));


			//float x_offset = (((float)unit_zoom_pt.X * new_zoom) - ((float)unit_zoom_pt.X * old_zoom) / new_zoom);
            //float y_offset = (((float)unit_zoom_pt.Y * new_zoom) - ((float)unit_zoom_pt.Y * old_zoom) / new_zoom);


           // origin.X -= x_offset;
           // origin.Y -= y_offset;

            //m_panOffset = origin;

   

            this.CreateGraphics().FillPie(Brushes.Green, m_panOffset.X, m_panOffset.Y, 30, 30, 0, 359);

          

            DoInvalidate(true);

            return;

            //double new_map_width = screen_width / scale;


            //m_panOffset.X = ((float)(pu.X / ( zoom) ));
          //  m_panOffset.Y = ((float)(pu.Y / ( zoom)));
          //  m_model.Zoom *= (float)zoom;

            //float ox = (float)((pu.X / scale)-(pu.X / (scale * zoom))) ;
         //   float oy = (float) ((pu.Y / scale)-(pu.Y / (scale * zoom))) ;


            // Console.WriteLine(ox + ", " + oy);
            //
            //PointF uoff = (m_panOffset);

            // Console.WriteLine(uoff);
            //uoff.X -= ToScreen(ox);
            //uoff.Y += ToScreen(oy);

            //  m_panOffset = (uoff);




            Console.WriteLine();


            DoInvalidate(true);
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            CommonTools.Tracing.StartTrack(Program.TracePaint);
            ClearPens();
            e.Graphics.SmoothingMode = m_smoothingMode;

            CanvasWrapper dc = new CanvasWrapper(this, e.Graphics, ClientRectangle);
            Rectangle cliprectangle = e.ClipRectangle;

            if (m_staticImage == null)
            {
                cliprectangle = ClientRectangle;
                m_staticImage = new Bitmap(ClientRectangle.Width, ClientRectangle.Height);
                m_staticDirty = true;
            }

            RectangleF r = ScreenUtils.ToUnitNormalized(dc, cliprectangle);
            if (float.IsNaN(r.Width) || float.IsInfinity(r.Width))
            {
                r = ScreenUtils.ToUnitNormalized(dc, cliprectangle);
            }

            if (m_staticDirty)
            {
                m_staticDirty = false;
                CanvasWrapper dcStatic = new CanvasWrapper(this, Graphics.FromImage(m_staticImage), ClientRectangle);
                dcStatic.Graphics.SmoothingMode = m_smoothingMode;
                m_model.BackgroundLayer.Draw(dcStatic, r);
                if (m_model.GridLayer.Enabled)
                    m_model.GridLayer.Draw(dcStatic, r);

                //Draw Origin
                PointF nullPoint = ToScreen(new UnitPoint(0, 0));
                dcStatic.Graphics.DrawLine(Pens.Blue, nullPoint.X - 10, nullPoint.Y, nullPoint.X + 10, nullPoint.Y);
                dcStatic.Graphics.DrawLine(Pens.Blue, nullPoint.X, nullPoint.Y - 10, nullPoint.X, nullPoint.Y + 10);

                //Draw Each Layer
                ICanvasLayer[] layers = m_model.Layers;
                for (int layerindex = layers.Length - 1; layerindex >= 0; layerindex--)
                {
                    if (layers[layerindex] != m_model.ActiveLayer && layers[layerindex].Visible)
                        layers[layerindex].Draw(dcStatic, r);
                }
                if (m_model.ActiveLayer != null)
                    m_model.ActiveLayer.Draw(dcStatic, r);

                dcStatic.Dispose();
            }
            e.Graphics.DrawImage(m_staticImage, cliprectangle, cliprectangle, GraphicsUnit.Pixel);

            foreach (IDrawObject drawobject in m_model.SelectedObjects)
                drawobject.Draw(dc, r);

            if (m_newObject != null)
                m_newObject.Draw(dc, r);

            if (m_snappoint != null)
                m_snappoint.Draw(dc);

            if (m_selection != null)
            {
                m_selection.Reset();
                m_selection.SetMousePoint(e.Graphics, this.PointToClient(Control.MousePosition), false);
            }
            if (m_moveHelper.IsEmpty == false)
                m_moveHelper.DrawObjects(dc, r);

            if (m_nodeMoveHelper.IsEmpty == false)
                m_nodeMoveHelper.DrawObjects(dc, r);


            //diagnostics
            e.Graphics.DrawString(m_panOffset.ToString(), this.Font, Brushes.White, 5, 5);
            e.Graphics.DrawString(m_model.Zoom.ToString() + "%", this.Font, Brushes.White, 5,15);
            e.Graphics.DrawArc(new Pen(Brushes.Wheat), m_panOffset.X, m_panOffset.Y, 30, 30, 0, 359);

            e.Graphics.DrawRectangle(new Pen(Brushes.Pink,5), ClientRectangle);

            dc.Dispose();
            ClearPens();
            CommonTools.Tracing.EndTrack(Program.TracePaint, "OnPaint complete");


        }

        void RepaintStatic(Rectangle r)
		{
			if (m_staticImage == null)
				return;
			Graphics dc = Graphics.FromHwnd(Handle);
			if (r.X < 0) r.X = 0;
			if (r.X > m_staticImage.Width) r.X = 0;
			if (r.Y < 0) r.Y = 0;
			if (r.Y > m_staticImage.Height) r.Y = 0;
			
			if (r.Width > m_staticImage.Width || r.Width < 0)
				r.Width = m_staticImage.Width;
			if (r.Height > m_staticImage.Height || r.Height < 0)
				r.Height = m_staticImage.Height;
			dc.DrawImage(m_staticImage, r, r, GraphicsUnit.Pixel);
			dc.Dispose();
		}
		void RepaintSnappoint(ISnapPoint snappoint)
		{
			if (snappoint == null)
				return;
			CanvasWrapper dc = new CanvasWrapper(this, Graphics.FromHwnd(Handle), ClientRectangle);
			snappoint.Draw(dc);
			dc.Graphics.Dispose();
			dc.Dispose();
		}
		void RepaintObject(IDrawObject obj)
		{
			if (obj == null)
				return;
			CanvasWrapper dc = new CanvasWrapper(this, Graphics.FromHwnd(Handle), ClientRectangle);
			RectangleF invalidaterect = ScreenUtils.ConvertRect(ScreenUtils.ToScreenNormalized(dc, obj.GetBoundingRect(dc)));
			obj.Draw(dc, invalidaterect);
			dc.Graphics.Dispose();
			dc.Dispose();
		}
		public void DoInvalidate(bool dostatic, RectangleF rect)
		{
			if (dostatic)
				m_staticDirty = true;
			Invalidate(ScreenUtils.ConvertRect(rect));
		}
		public void DoInvalidate(bool dostatic)
		{
			if (dostatic)
				m_staticDirty = true;
			Invalidate();
		}
		public IDrawObject NewObject
		{
			get { return m_newObject; }
		}
		protected void HandleSelection(List<IDrawObject> selected)
		{
			bool add = Control.ModifierKeys == Keys.Shift;
			bool toggle = Control.ModifierKeys == Keys.Control;
			bool invalidate = false;
			bool anyoldsel = false;
			int selcount = 0;
			if (selected != null)
				selcount = selected.Count;
			foreach(IDrawObject obj in m_model.SelectedObjects)
			{
				anyoldsel = true;
				break;
			}
			if (toggle && selcount > 0)
			{
				invalidate = true;
				foreach (IDrawObject obj in selected)
				{
					if (m_model.IsSelected(obj))
						m_model.RemoveSelectedObject(obj);
					else
						m_model.AddSelectedObject(obj);
				}
			}
			if (add && selcount > 0)
			{
				invalidate = true;
				foreach (IDrawObject obj in selected)
					m_model.AddSelectedObject(obj);
			}
			if (add == false && toggle == false && selcount > 0)
			{
				invalidate = true;
				m_model.ClearSelectedObjects();
				foreach (IDrawObject obj in selected)
					m_model.AddSelectedObject(obj);
			}
			if (add == false && toggle == false && selcount == 0 && anyoldsel)
			{
				invalidate = true;
				m_model.ClearSelectedObjects();
			}

			if (invalidate)
				DoInvalidate(false);
		}
		void FinishNodeEdit()
		{
			m_commandType = eCommandType.select;
			m_snappoint = null;
		}
		protected virtual void HandleMouseDownWhenDrawing(UnitPoint mouseunitpoint, ISnapPoint snappoint)
		{
			if (m_commandType == eCommandType.draw)
			{
				if (m_newObject == null)
				{
					m_newObject = m_model.CreateObject(m_drawObjectId, mouseunitpoint, snappoint);
					DoInvalidate(false, m_newObject.GetBoundingRect(m_canvaswrapper));
				}
				else
				{
					if (m_newObject != null)
					{
						eDrawObjectMouseDown result = m_newObject.OnMouseDown(m_canvaswrapper, mouseunitpoint, snappoint);
						switch (result)
						{
							case eDrawObjectMouseDown.Done:
								m_model.AddObject(m_model.ActiveLayer, m_newObject);
								m_newObject = null;
								DoInvalidate(true);
								break;
							case eDrawObjectMouseDown.DoneRepeat:
								m_model.AddObject(m_model.ActiveLayer, m_newObject);
								m_newObject = m_model.CreateObject(m_newObject.Id, m_newObject.RepeatStartingPoint, null);
								DoInvalidate(true);
								break;
							case eDrawObjectMouseDown.Continue:
								break;
						}
					}
				}
			}
		}
		protected override void OnMouseDown(MouseEventArgs e)
		{
			m_mousedownPoint = new PointF(e.X, e.Y); // used when panning
			m_dragOffset = new PointF(0,0);

			UnitPoint mousepoint = ToUnit(m_mousedownPoint);
            if (e.Button == MouseButtons.Middle)
            {
                this.CommandPan();
            }


			if (m_snappoint != null)
				mousepoint = m_snappoint.SnapPoint;
			
			if (m_commandType == eCommandType.editNode)
			{
				bool handled = false;
				if (m_nodeMoveHelper.HandleMouseDown(mousepoint, ref handled))
				{
					FinishNodeEdit();
					base.OnMouseDown(e);
					return;
				}
			}
            if (m_commandType == eCommandType.select)
            {
             
                bool handled = false;
                if (m_nodeMoveHelper.HandleMouseDown(mousepoint, ref handled))
                {
                    m_commandType = eCommandType.editNode;
                    m_snappoint = null;
                    base.OnMouseDown(e);
                    return;
                }
                m_selection = new SelectionShape(m_mousedownPoint);
            }
            if (m_commandType == eCommandType.lasso_select)
            {
                bool handled = false;
                if (m_nodeMoveHelper.HandleMouseDown(mousepoint, ref handled))
                {
                    m_commandType = eCommandType.editNode;
                    m_snappoint = null;
                    base.OnMouseDown(e);
                    return;
                }
                if (m_selection == null)
                {
                    m_selection = new SelectionShape(m_mousedownPoint);
                }
                else
                {
                    m_selection.SetMousePoint(m_canvaswrapper.Graphics, m_mousedownPoint,m_commandType == eCommandType.lasso_select);
                }
            }
            if (m_commandType == eCommandType.move)
			{
				m_moveHelper.HandleMouseDownForMove(mousepoint, m_snappoint);
			}
			if (m_commandType == eCommandType.draw)
			{
				HandleMouseDownWhenDrawing(mousepoint, null);
				DoInvalidate(true);
			}
			if (m_commandType == eCommandType.edit)
			{
				if (m_editTool == null)
					m_editTool = m_model.GetEditTool(m_editToolId);
				if (m_editTool != null)
				{
					if (m_editTool.SupportSelection)
						m_selection = new SelectionShape(m_mousedownPoint);

					eDrawObjectMouseDown mouseresult = m_editTool.OnMouseDown(m_canvaswrapper, mousepoint, m_snappoint);
					/*
					if (mouseresult == eDrawObjectMouseDown.Continue)
					{
						if (m_editTool.SupportSelection)
							m_selection = new SelectionRectangle(m_mousedownPoint);
					}
					 * */
					if (mouseresult == eDrawObjectMouseDown.Done)
					{
						m_editTool.Finished();
						m_editTool = m_model.GetEditTool(m_editToolId); // continue with new tool
						//m_editTool = null;
						
						if (m_editTool.SupportSelection)
							m_selection = new SelectionShape(m_mousedownPoint);
					}
				}
				DoInvalidate(true);
				UpdateCursor();
			}
			base.OnMouseDown(e);
		}
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);

			if (m_commandType == eCommandType.pan)
			{
                m_panOffset.X += m_dragOffset.X;
                m_panOffset.Y += m_dragOffset.Y;
                m_dragOffset = new PointF(0, 0);
                if (e.Button == MouseButtons.Middle)
                {
                    CommandEscape();
                }
			}

			List<IDrawObject> hitlist = null;
			Rectangle screenSelRect = Rectangle.Empty;
			if (m_selection != null)
			{
				screenSelRect = m_selection.ScreenRect();
				RectangleF selectionRect = m_selection.Selection(m_canvaswrapper);
				if (selectionRect != RectangleF.Empty)
				{
					// is any selection rectangle. use it for selection
					hitlist = m_model.GetHitObjects(m_canvaswrapper, Utils.GetPointsFromRectangle(selectionRect), m_selection.AnyPoint());
					DoInvalidate(true);
				}
				else
				{
					// else use mouse point
					UnitPoint mousepoint = ToUnit(new PointF(e.X, e.Y));
					hitlist = m_model.GetHitObjects(m_canvaswrapper, mousepoint);
				}
                if (m_commandType != eCommandType.lasso_select)
                {
                    m_selection = null;
                }
				
			}
			if (m_commandType == eCommandType.select)
			{
				if (hitlist != null) {
                    HandleSelection(hitlist);
                    DoInvalidate(true);
                }
                else
                {
   

                }
					
			}
			if (m_commandType == eCommandType.edit && m_editTool != null)
			{
				UnitPoint mousepoint = ToUnit(m_mousedownPoint);
				if (m_snappoint != null)
					mousepoint = m_snappoint.SnapPoint;
				if (screenSelRect != Rectangle.Empty)
					m_editTool.SetHitObjects(mousepoint, hitlist);
				m_editTool.OnMouseUp(m_canvaswrapper, mousepoint, m_snappoint);
			}
			if (m_commandType == eCommandType.draw && m_newObject != null)
			{
				UnitPoint mousepoint = ToUnit(m_mousedownPoint);
				if (m_snappoint != null)
					mousepoint = m_snappoint.SnapPoint;
				m_newObject.OnMouseUp(m_canvaswrapper, mousepoint, m_snappoint);
			}
		}
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (m_selection != null)
			{
                if(m_commandType == eCommandType.lasso_select)
                {
                    Graphics dc = Graphics.FromHwnd(Handle);
                    m_selection.SetMousePoint(dc, new PointF(e.X, e.Y), true);
                    dc.Dispose();
                    return;
                }
                else
                {
                    Graphics dc = Graphics.FromHwnd(Handle);
                    m_selection.SetMousePoint(dc, new PointF(e.X, e.Y), false);
                    dc.Dispose();
                    return;
                }

                

			}


            if (m_commandType == eCommandType.pan && e.Button == MouseButtons.Middle)
            {
                m_dragOffset.X = -(m_mousedownPoint.X - e.X);
                m_dragOffset.Y = -(m_mousedownPoint.Y - e.Y);
                m_lastCenterPoint = CenterPointUnit();
                DoInvalidate(true);
              
            }


            if (m_commandType == eCommandType.pan && e.Button == MouseButtons.Left )
			{
				m_dragOffset.X = -(m_mousedownPoint.X - e.X);
				m_dragOffset.Y = -(m_mousedownPoint.Y - e.Y);
				m_lastCenterPoint = CenterPointUnit();
				DoInvalidate(true);
			}
			UnitPoint mousepoint;
			UnitPoint unitpoint = ToUnit(new PointF(e.X, e.Y));
			if (m_commandType == eCommandType.draw || m_commandType == eCommandType.move || m_nodeMoveHelper.IsEmpty == false)
			{
				Rectangle invalidaterect = Rectangle.Empty;
				ISnapPoint newsnap = null;
				mousepoint = GetMousePoint();
				if (RunningSnapsEnabled)
					newsnap = m_model.SnapPoint(m_canvaswrapper, mousepoint, m_runningSnapTypes, null);
				if (newsnap == null)
					newsnap = m_model.GridLayer.SnapPoint(m_canvaswrapper, mousepoint, null);
				if ((m_snappoint != null) && ((newsnap == null) || (newsnap.SnapPoint != m_snappoint.SnapPoint) || m_snappoint.GetType() != newsnap.GetType()))
				{
					invalidaterect = ScreenUtils.ConvertRect(ScreenUtils.ToScreenNormalized(m_canvaswrapper, m_snappoint.BoundingRect));
					invalidaterect.Inflate(2, 2);
					RepaintStatic(invalidaterect); // remove old snappoint
					m_snappoint = newsnap;
				}
				if (m_commandType == eCommandType.move)
					Invalidate(invalidaterect);

				if (m_snappoint == null)
					m_snappoint = newsnap;
			}
			m_owner.SetPositionInfo(unitpoint);
			m_owner.SetSnapInfo(m_snappoint);


			//UnitPoint mousepoint;
			if (m_snappoint != null)
				mousepoint = m_snappoint.SnapPoint;
			else
				mousepoint = GetMousePoint();

			if (m_newObject != null)
			{
				Rectangle invalidaterect = ScreenUtils.ConvertRect(ScreenUtils.ToScreenNormalized(m_canvaswrapper, m_newObject.GetBoundingRect(m_canvaswrapper)));
				invalidaterect.Inflate(2, 2);
				RepaintStatic(invalidaterect);

				m_newObject.OnMouseMove(m_canvaswrapper, mousepoint);
				RepaintObject(m_newObject);
			}
			if (m_snappoint != null)
				RepaintSnappoint(m_snappoint);

			if (m_moveHelper.HandleMouseMoveForMove(mousepoint))
				Refresh(); //Invalidate();

			RectangleF rNoderect = m_nodeMoveHelper.HandleMouseMoveForNode(mousepoint);
			if (rNoderect != RectangleF.Empty)
			{
				Rectangle invalidaterect = ScreenUtils.ConvertRect(ScreenUtils.ToScreenNormalized(m_canvaswrapper, rNoderect));
				RepaintStatic(invalidaterect);

				CanvasWrapper dc = new CanvasWrapper(this, Graphics.FromHwnd(Handle), ClientRectangle);
				dc.Graphics.Clip = new Region(ClientRectangle);
				//m_nodeMoveHelper.DrawOriginalObjects(dc, rNoderect);
				m_nodeMoveHelper.DrawObjects(dc, rNoderect);
				if (m_snappoint != null)
					RepaintSnappoint(m_snappoint);

				dc.Graphics.Dispose();
				dc.Dispose();
			}
		}
		protected override void OnMouseWheel(MouseEventArgs e)
		{
            
            ZoomToPoint(e);
			//UnitPoint pu = GetMousePoint();
            //Console.WriteLine("MOUSE PT: " + pu);
            //PointF screenPoint = pu.Point;
            //screenPoint = Control.MousePosition;
            //Console.WriteLine("SCREEN PT: " + screenPoint);
            //Console.WriteLine("ZOOM: " + m_model.Zoom);
            //
            ////float wheeldeltatick = 120;
            ////aka scale :|
            ////float zoomdelta = (1.25f * (Math.Abs(e.Delta) / wheeldeltatick));
            //
            //
            //
            //
            //double wheel = e.Delta < 0 ? -1 : 1;
            //double zoom = Math.Exp(wheel * 0.2);
            ////double zoom = e.Delta * 0.2;
            //double scale = m_model.Zoom * zoom ;
            //
            //float tsx = ((float)(screenPoint.X / (scale * zoom) - screenPoint.X / scale));
            //float tsy = ((float)(screenPoint.Y / (scale * zoom) - screenPoint.Y / scale));
            //
            //Rectangle r = ClientRectangle;
            //
            //if (tsx > r.X/2) {
            //    m_panOffset.X -= tsx;
            //}
            //else
            //{
            //
            //}
            //
            //m_panOffset.Y -= tsy;
            //
            //    
            //
            //
            ////m_panOffset.Y += (float)(screenPoint.Y / (scale * zoom) - screenPoint.Y / scale);
            //
            ////m_model.Zoom *= (float)zoom;
            //
            //
            //// ZoomToPoint(ToScreen(p), zoomdelta, m_model.Zoom);
            //
            ////set new zoom 
            //// if (e.Delta < 0)
            ////	m_model.Zoom = m_model.Zoom / zoomdelta;
            ////else
            ////	m_model.Zoom = m_model.Zoom * zoomdelta;
            //
            //Console.WriteLine("PAN OFFSET: " +  m_panOffset);
            DoInvalidate(true);
			base.OnMouseWheel(e);
		}
		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);
	//TODO RE ENABLE
			//if (m_lastCenterPoint != UnitPoint.Empty && Width != 0)
			//	SetCenterScreen(ToScreen(m_lastCenterPoint), false);
			m_lastCenterPoint = CenterPointUnit();
			m_staticImage = null;
			DoInvalidate(true);
		}

        UnitPoint m_lastCenterPoint;

        public PointF m_panOffset = new PointF(0, -0);


        //   PointF m_panOffset = new PointF(0,0);
        PointF m_dragOffset = new PointF(0, 0);
		float m_screenResolution = 96;

		PointF Translate(UnitPoint point)
		{
			return point.Point;
		}

        float ScreenHeight()
		{
            return (float)(ToUnit(this.ClientRectangle.Height)); /// m_model.Zoom);
		}
        float ScreenWidth()
        {
            return (float)(ToUnit(this.ClientRectangle.Width) / m_model.Zoom);
        }
        #region ICanvas
        public UnitPoint CenterPointUnit()
		{
			UnitPoint p1 = ScreenTopLeftToUnitPoint();
			UnitPoint p2 = ScreenBottomRightToUnitPoint();
			UnitPoint center = new UnitPoint();
			center.X = (p1.X + p2.X) / 2;
			center.Y = (p1.Y + p2.Y) / 2;
			return center;
		}
		public UnitPoint ScreenTopLeftToUnitPoint()
		{
			return ToUnit(new PointF(0, 0));
		}
		public UnitPoint ScreenBottomRightToUnitPoint()
		{
			return ToUnit(new PointF(this.ClientRectangle.Width, this.ClientRectangle.Height));
		}
        public PointF ToScreen(UnitPoint point)
        {
            PointF transformedPoint = Translate(point);
            transformedPoint.Y = ScreenHeight() - transformedPoint.Y;
            transformedPoint.Y *= m_screenResolution * m_model.Zoom;
            transformedPoint.X *= m_screenResolution * m_model.Zoom;

            transformedPoint.X += m_panOffset.X + m_dragOffset.X;
            transformedPoint.Y += m_panOffset.Y + m_dragOffset.Y;
            return transformedPoint;
        }
        public PointF Test_ToScreen(UnitPoint point)
		{



            PointF transformedPoint = Translate(point);

            float screenHeight = ScreenHeight();

            transformedPoint.X = ToScreen(transformedPoint.X);
            transformedPoint.Y = ToScreen(transformedPoint.Y);
            Console.WriteLine("ToScreenPrePt: " + transformedPoint.ToString());
            transformedPoint.Y = -transformedPoint.Y;
            //transformedPoint.Y *= m_screenResolution * m_model.Zoom;
            //transformedPoint.X *= m_screenResolution * m_model.Zoom;

            transformedPoint.X += m_panOffset.X + m_dragOffset.X;
            transformedPoint.Y += m_panOffset.Y + m_dragOffset.Y + ToScreen(screenHeight); 
            Console.WriteLine("ToScreenPt: " + transformedPoint.ToString());
			return transformedPoint;
		}
		public float ToScreen(double value)
		{
			return (float)(value * m_screenResolution * m_model.Zoom);
		}
		public double ToUnit(float screenvalue)
		{
			return (double)screenvalue / (double)(m_screenResolution * m_model.Zoom);
		}
		public UnitPoint ToUnit(PointF screenpoint)
		{
			float panoffsetX = m_panOffset.X + m_dragOffset.X;
			float panoffsetY = m_panOffset.Y + m_dragOffset.Y;
			float xpos = (screenpoint.X - panoffsetX) / (m_screenResolution * m_model.Zoom);
			float ypos = ScreenHeight() - ((screenpoint.Y - panoffsetY)) / (m_screenResolution * m_model.Zoom);
			return new UnitPoint(xpos, ypos);
		}
		public Pen CreatePen(Color color, float unitWidth)
		{
			return GetPen(color, ToScreen(unitWidth));
		}

        #endregion

        #region DrawCanvas
        public void DrawLine(ICanvas canvas, Pen pen, UnitPoint p1, UnitPoint p2)
		{
			PointF tmpp1 = ToScreen(p1);
			PointF tmpp2 = ToScreen(p2);
			canvas.Graphics.DrawLine(pen, tmpp1, tmpp2);
		}
		public void DrawArc(ICanvas canvas, Pen pen, UnitPoint center, float radius, float startAngle, float sweepAngle)
		{
			PointF p1 = ToScreen(center);
			radius = (float)Math.Round(ToScreen(radius));
			RectangleF r = new RectangleF(p1, new SizeF());
			r.Inflate(radius, radius);
			if (radius > 0 && radius < 1e8f )
				canvas.Graphics.DrawArc(pen, r, -startAngle, -sweepAngle);
		}
        public void DrawText(ICanvas canvas, Pen pen, UnitPoint position, string text, Font font, float rotation)
        {

            font = new Font(font.FontFamily, font.Size * canvas.DataModel.Zoom);

            // Only process visible text
            if (font.Height > 2)
            {
                //font.Bold = true;
                StringFormat stringformat = new StringFormat();

                SolidBrush drawBrush = new SolidBrush(Color.WhiteSmoke);
                drawBrush.Color = pen.Color;


                GraphicsState state = canvas.Graphics.Save();


                canvas.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                PointF p = ToScreen(position);
                p.Y += (float)4;
                canvas.Graphics.TranslateTransform(p.X, p.Y);
                //Puts rotaion the other clockwise
                rotation = 360 - rotation;
                canvas.Graphics.RotateTransform(rotation);


                SizeF textsize = TextRenderer.MeasureText(text, font);
                textsize.Height = (int)(textsize.Height);
                textsize.Width = (int)(textsize.Width);
                PointF textpos = new PointF((float)position.X, (float)position.Y);

                RectangleF hitbox = new RectangleF(new PointF(0, (float)-font.Height), textsize);

                RectangleF[] c = new RectangleF[] { hitbox };
                //Pen pp = new Pen(Color.Red);
                //canvas.Graphics.DrawRectangles(pp, c);

                canvas.Graphics.DrawString(text, font, drawBrush, 0, -font.Height, stringformat);


                canvas.Graphics.Restore(state);
            }
        }


        public void DrawMultilineText(ICanvas canvas, Pen pen, UnitPoint position, string text, Font font, float rotation)
        {

            font = new Font(font.FontFamily, font.Size * canvas.DataModel.Zoom);
            PointF screen_position = ToScreen(position);
            // Only process visible text
            if (font.Height > 2)
            {

				text = text.Replace("\\\\", "{%\\\\%}");
				text = text.Replace("\\P", "\n");


                rotation = 360 - rotation;
                foreach (char character in text)
                {
                    //font.Bold = true;
                    StringFormat stringformat = new StringFormat();

                    SolidBrush drawBrush = new SolidBrush(pen.Color);
              

                    GraphicsState state = canvas.Graphics.Save();


                    canvas.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                    PointF p = screen_position;
                    p.Y += (float)4;
                    canvas.Graphics.TranslateTransform(p.X, p.Y);
              
                    //Puts rotaion the other clockwise
                  
                    canvas.Graphics.RotateTransform(rotation);


                    SizeF textsize = TextRenderer.MeasureText(character.ToString(), font);
                    textsize.Height = (int)(textsize.Height);
                    textsize.Width = (int)(textsize.Width);

                   
             if (rotation > 0)
                    {
                        //Calculate new point using trig
                        double A = rotation;
                        double B = 90 - A;

                        double c = textsize.Width / 2; //Divide by 2 to account for silly white space
                        double a = c * Math.Sin(A);
                        double b = c * Math.Cos(A);



                        // Offset the text
                        screen_position.X += (float)b;
                        screen_position.Y += (float)a;
                    }
                    else
                    {
                        // Offset the text
                        screen_position.X += (float)textsize.Width;
                    }

                    PointF textpos = new PointF((float)position.X, (float)position.Y);

                    RectangleF hitbox = new RectangleF(new PointF(0, (float)-font.Height), textsize);



                    canvas.Graphics.DrawString(character.ToString(), font, drawBrush, 0, -font.Height, stringformat);


                    canvas.Graphics.Restore(state);
                }

            }

        }


        public void DrawSolid(ICanvas canvas, Pen pen, UnitPoint p1, UnitPoint p2, UnitPoint p3, UnitPoint p4)
        {

            //pen.Color = Color.Magenta;
            SolidBrush drawBrush = new SolidBrush(pen.Color);
            
            PointF tmpp1 = ToScreen(p1);
            PointF tmpp2 = ToScreen(p2);
            PointF tmpp3 = ToScreen(p3);
            PointF tmpp4 = ToScreen(p4);
            

            // Had to re-order the points for the draw function. This prevents the silly hourglass shape that is drawn when the point order isnt good.
            // The point order windows uses is different than the one CAD uses
            canvas.Graphics.FillPolygon(drawBrush, new PointF[] {  tmpp3, tmpp1, tmpp2, tmpp4 },FillMode.Winding);

        }





            Dictionary<float, Dictionary<Color, Pen>> m_penCache = new Dictionary<float,Dictionary<Color,Pen>>();
		Pen GetPen(Color color, float width)
		{
			if (m_penCache.ContainsKey(width) == false)
				m_penCache[width] = new Dictionary<Color,Pen>();
			if (m_penCache[width].ContainsKey(color) == false)
				m_penCache[width][color] = new Pen(color, width);
			return m_penCache[width][color];
		}
		void ClearPens()
		{
			m_penCache.Clear();
		}
        #endregion

        void UpdateCursor()
		{
			Cursor = m_cursors.GetCursor(m_commandType);
		}

		Dictionary<Keys, Type> m_QuickSnap = new Dictionary<Keys,Type>();
		public void AddQuickSnapType(Keys key, Type snaptype)
		{
			m_QuickSnap.Add(key, snaptype);
		}

		public void CommandSelectDrawTool(string drawobjectid)
		{
			CommandEscape();
			m_model.ClearSelectedObjects();
			m_commandType = eCommandType.draw;
			m_drawObjectId = drawobjectid;
			UpdateCursor();
		}
		public void CommandEscape()
		{
			bool dirty = (m_newObject != null) || (m_snappoint != null);
			m_newObject = null;
			m_snappoint = null;
			if (m_editTool != null)
				m_editTool.Finished();
			m_editTool	= null;
			m_commandType = eCommandType.select;
			m_moveHelper.HandleCancelMove();
			m_nodeMoveHelper.HandleCancelMove();
			DoInvalidate(dirty);
			UpdateCursor();
		}
        public void CommandPan()
        {
            if (m_commandType == eCommandType.select || m_commandType == eCommandType.move)
                m_commandType = eCommandType.pan;
            UpdateCursor();
        }
        public void CommandLasso()
        {
            CommandEscape();
            m_model.ClearSelectedObjects();
            m_commandType = eCommandType.draw;
          
            UpdateCursor();
        }
        public void CommandMove(bool handleImmediately)
		{
			if (m_model.SelectedCount > 0)
			{
				if (handleImmediately && m_commandType == eCommandType.move)
					m_moveHelper.HandleMouseDownForMove(GetMousePoint(), m_snappoint);
				m_commandType = eCommandType.move;
				UpdateCursor();
			}
		}
		public void CommandDeleteSelected()
		{
			m_model.DeleteObjects(m_model.SelectedObjects);
			m_model.ClearSelectedObjects();
			DoInvalidate(true);
			UpdateCursor();
		}
		public void CommandEdit(string editid)
		{
			CommandEscape();
			m_model.ClearSelectedObjects();
			m_commandType = eCommandType.edit;
			m_editToolId = editid;
			m_editTool = m_model.GetEditTool(m_editToolId);
			UpdateCursor();
		}

        public void CommandZoomExtents()
        {
            

            List<UnitPoint> points = new List<UnitPoint>();
            for(int i = 0; i < Model.Layers.Count(); i++)
            {

           
            IEnumerable<IDrawObject> objects = Model.Layers[i].Objects;
            foreach (var o in objects)
            {
                if (o.GetType() == typeof(DrawTools.Line)){
                    DrawTools.Line l = (DrawTools.Line)o;
                    UnitPoint start = l.P1;
                    UnitPoint end = l.P2;

                    points.Add(start);
                    points.Add(end);
                        
                }
                else if(o.GetType() == typeof(DrawTools.Circle))
                    {
                        DrawTools.Circle l = (DrawTools.Circle)o;
                        points.Add(l.Center);
                    }
                    else if(o.GetType() == typeof(DrawTools.Text))
                    {
                        DrawTools.Text l = (DrawTools.Text)o;
                        points.Add(l.Position);
                    }
                    else if(o.GetType() == typeof(DrawTools.MText))
                    {
                        DrawTools.MText l = (DrawTools.MText)o;
                        points.Add(l.Position);

                    }
                    else if(o.GetType() == typeof(DrawTools.Solid))
                    {
                        DrawTools.Solid l = (DrawTools.Solid)o;
                        points.Add(l.Vertex1);
                        points.Add(l.Vertex2);
                        points.Add(l.Vertex3);
                        points.Add(l.Vertex4);
                    }
                }
            }

            if (points.Count() == 0)
            {
                return;
            }
            Rectangle client = this.ClientRectangle;

            Rectangle object_rect = new Rectangle();

         

            double min_x = points.Min(x => x.X);
            double min_y = points.Min(x => x.Y);

            double max_x = points.Max(x => x.X);
            double max_y = points.Max(x => x.Y);



            UnitPoint min_point = new UnitPoint(min_x, min_y);
            UnitPoint max_point = new UnitPoint(max_x, max_y);

            UnitPoint new_center = new UnitPoint((max_x - min_x) / 2, (max_y - min_y) / 2);

            PointF screen_lower_left = ToScreen(min_point);
            PointF screen_upper_right = ToScreen(max_point);

            object_rect.Location = new Point((int)screen_lower_left.X,(int)screen_lower_left.Y);
            object_rect.Width = (int)(screen_lower_left.X - screen_upper_right.X);
            object_rect.Height = (int)(screen_lower_left.Y - screen_upper_right.Y);

            //double x_ratio = Math.Abs(((client.X - client.Width )- client.X) / (screen_upper_right.X - screen_lower_left.X));
            //double y_ratio = Math.Abs(((client.Y - client.Height) - client.Y) / (screen_upper_right.Y - screen_lower_left.Y));

            double x_ratio = Math.Abs( (screen_upper_right.X - screen_lower_left.X) / (client.X - client.Width));
            double y_ratio = Math.Abs((screen_upper_right.Y - screen_lower_left.Y) / (client.Y - client.Height)  );

            float zoom_ratio = (float)Math.Min(x_ratio, y_ratio);
            //zoom_ratio *= .80f;



            //There are a couple bugs here.
            //This code works but the .80f a the end of this line shouldnt be there
            //Also the verticle zoom doesnt seem to be working to its fullest ability.
            //there are gaps at the top of the drawing. 
            //  PointF new_pan_offset = new PointF(((screen_lower_left.X - (m_panOffset.X )* .9f)), (ScreenHeight()- ( ScreenHeight() - screen_lower_left.Y + ( m_panOffset.Y )* .80f)));
           // PointF new_pan_offset = new PointF(-((screen_lower_left.X - (m_panOffset.X )* .9f)), (ClientRectangle.Height- (ClientRectangle.Height - screen_lower_left.Y + ( m_panOffset.Y )* .80f)));
             //m_panOffset = (new_pan_offset) ;
            ///m_panOffset.Y -= ClientRectangle.Height;
            //m_panOffset.Y += ClientRectangle.Height;
            //Get Top Right Point
            //UnitPoint TopLeft = new UnitPoint(min_x, max_y);

            //m_panOffset = ToScreen(TopLeft);
            //m_panOffset.Y -= ToScreen(TopLeft).Y;
            m_model.Zoom = zoom_ratio;

            SetCenterScreen(ToScreen(new_center), false);

            DoInvalidate(true);

        }
		void HandleQuickSnap(KeyEventArgs e)
		{
			if (m_commandType == eCommandType.select || m_commandType == eCommandType.pan)
				return;
			ISnapPoint p = null;
			UnitPoint mousepoint = GetMousePoint();
			if (m_QuickSnap.ContainsKey(e.KeyCode))
				p = m_model.SnapPoint(m_canvaswrapper, mousepoint, null, m_QuickSnap[e.KeyCode]);
			if (p != null)
			{
				if (m_commandType == eCommandType.draw)
				{
					HandleMouseDownWhenDrawing(p.SnapPoint, p);
					if (m_newObject != null)
						m_newObject.OnMouseMove(m_canvaswrapper, GetMousePoint());
					DoInvalidate(true);
					e.Handled = true;
				}
				if (m_commandType == eCommandType.move)
				{
					m_moveHelper.HandleMouseDownForMove(p.SnapPoint, p);
					e.Handled = true;
				}
				if (m_nodeMoveHelper.IsEmpty == false)
				{
					bool handled = false;
					m_nodeMoveHelper.HandleMouseDown(p.SnapPoint, ref handled);
					FinishNodeEdit();
					e.Handled = true;
				}
				if (m_commandType == eCommandType.edit)
				{
				}
			}
		}
		protected override void OnKeyDown(KeyEventArgs e)
		{
			HandleQuickSnap(e);

			if (m_nodeMoveHelper.IsEmpty == false)
			{
				m_nodeMoveHelper.OnKeyDown(m_canvaswrapper, e);
				if (e.Handled)
					return;
			}
			base.OnKeyDown(e);
			if (e.Handled)
			{
				UpdateCursor();
				//return;
			}
			if (m_editTool != null)
			{
				m_editTool.OnKeyDown(m_canvaswrapper, e);
				if (e.Handled)
					return;
			}
			if (m_newObject != null)
			{
				m_newObject.OnKeyDown(m_canvaswrapper, e);
				if (e.Handled)
					return;
			}
			foreach (IDrawObject obj in m_model.SelectedObjects)
			{
				obj.OnKeyDown(m_canvaswrapper, e);
				//if (e.Handled)
					//return;
			}

			if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
			{
				if (e.KeyCode == Keys.G)
				{
					m_model.GridLayer.Enabled = !m_model.GridLayer.Enabled;
					DoInvalidate(true);
				}
				if (e.KeyCode == Keys.S)
				{
					RunningSnapsEnabled = !RunningSnapsEnabled;
					if (!RunningSnapsEnabled)
						m_snappoint = null;
					DoInvalidate(false);
				}
				return;
			}

			if (e.KeyCode == Keys.Escape)
			{
                if(m_commandType == eCommandType.select)
                {
                    //De Select objects on escape
                    HandleSelection(new List<IDrawObject>());
                }
                else
                {
                    m_selection = null;
                    CommandEscape();
                }
				

                
			}
            if (e.KeyCode == Keys.P)
            {
                CommandPan();
            }
            if (e.KeyCode == Keys.Q)
            {
                m_commandType = eCommandType.lasso_select;
            }
            if (e.KeyCode == Keys.S)
			{
				RunningSnapsEnabled = !RunningSnapsEnabled;
				if (!RunningSnapsEnabled)
					m_snappoint = null;
				DoInvalidate(false);
			}
			if (e.KeyCode >= Keys.D1 && e.KeyCode <= Keys.D9)
			{
				int layerindex = (int)e.KeyCode - (int)Keys.D1;
				if (layerindex >=0 && layerindex < m_model.Layers.Length)
				{
					m_model.ActiveLayer = m_model.Layers[layerindex];
					DoInvalidate(true);
				}
			}
			if (e.KeyCode == Keys.Delete)
			{
				CommandDeleteSelected();
			}
			if (e.KeyCode == Keys.O)
			{
				CommandEdit("linesmeet");
			}
			UpdateCursor();
		}

        private void CanvasCtrl_Load(object sender, EventArgs e)
        {
            
            
        }

        private void CanvasCtrl_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                CommandZoomExtents();
            }
        }
    }
}

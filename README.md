# OpenCadNet
An opensource 2D Computer Assisted Drawing application/ component written in C#

This application is being transfered from a very old Code Project application (https://www.codeproject.com/Articles/22549/OpenS-CAD-a-simple-2D-CAD-application) 
in order to keep development alive and organsized.

This application supports import directly from .dxf files using the NetDxf library.
Rather than forcing this appliation to be used in it's entirity, the goal is to allow the DocumentForm component to be used in other 
.net programs that require dxf viewing/ modification.

Supported Entities:

Entity  | Progress
------------- | -------------
Line  | Complete
Arc  | Complete
Text | Complete
Mtext | In Progress
Solid | Complete
Insert | Complete

Example Usage For Imbedding DocumentForm using a NetDxf instance.
```
public void ShowDrawing(DxfDocument drawing)
{
    this.df = new Canvas.DocumentForm(drawing)
    {
        TopLevel = false,
        FormBorderStyle = FormBorderStyle.None
    };
    this.Panel1.Controls.Add(df);
    this.df.Dock = DockStyle.Fill;
    this.df.Show();
}
```
Short Term Goals:

* Use NetDxf as the internal database for each entitiy type. (currently OpenCadNet transaltes the NetDxf objects into a class contained within OpenCadNet)
* Fix zoom to point in canvas.cs


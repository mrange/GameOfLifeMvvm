using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Linq;

namespace ItIsMyLife;

public partial class ItIsMyLifeControl : Control
{
  const int _width  = 256;
  const int _height = 256;


  byte[] _current = new byte[_width*_height];
  byte[] _next    = new byte[_width*_height];

  const byte Dead     = 0;
  const byte Infant   = 1;

  DispatcherTimer _timer;

  readonly IBrush[] _brushes;

  // License: CC BY-NC-SA 3.0, author: Stephane Cuillerdier - Aiekick/2015 (twitter:@aiekick), found: https://www.shadertoy.com/view/Mt3GW2
  static Color BlackbodyRadiation(double temp) {
    var x = 56100000.0 * Math.Pow(temp,(-3.0 / 2.0)) + 148.0;
    var y = 100.04 * Math.Log(temp) - 623.6;
    if (temp > 6500.0)
    {
      y = 35200000.0 * Math.Pow(temp,(-3.0 / 2.0)) + 184.0;
    }
    var z = 194.18 * Math.Log(temp) - 1448.6;

    x = Math.Clamp(x, 0.0, 255.0);
    y = Math.Clamp(y, 0.0, 255.0);
    z = Math.Clamp(z, 0.0, 255.0);

    if (temp < 1000.0)
    {
      var tt = temp/1000.0;
      x *= tt;
      y *= tt;
      z *= tt;
    }

    return Color.FromRgb((byte)x, (byte)y, (byte)z);
  }

  public ItIsMyLifeControl()
  {
    static IBrush CreateBrush(int i)
    {
      if (i == 0) return Brushes.Black;
      if (i == 1) return Brushes.White;

      const double Max    = 8000.0;
      const double Min    = 200.0;
      const double Ratio  = Max/Min;

      var ii = (i-2)/253.0;

      var temp  = Max*Math.Exp(-ii*Math.Log(Ratio));
      var col   = BlackbodyRadiation(temp);
      var brush = new SolidColorBrush(col);
      return brush;
    }
    _brushes = Enumerable
      .Range(0, 256)
      .Select(CreateBrush)
      .ToArray()
      ;

    Ragnarök();

    _timer = new DispatcherTimer(
        TimeSpan.FromSeconds(0.125*0.5)
      , DispatcherPriority.SystemIdle
      , OnTimer
      );
    _timer.Start();
  }

  void OnTimer(object? sender, EventArgs e)
  {
    Next();
    InvalidateVisual();
  }

  void Ragnarök()
  {
    var rnd = Random.Shared;
//    var rnd = new Random(19740531);
    for (var y = 0; y < _height; ++y)
    {
      var yoff = y*_width;
      for (var x = 0; x < _width; ++x)
      {
        var isAlive = rnd.NextDouble() > 0.5;
        _current[x + yoff] = isAlive ? Infant : Dead;
      }
    }
    Next();
  }

  void Next()
  {
    for (var y = 0; y < _height; ++y)
    {
      var yoff = y*_width;
      for (var x = 0; x < _width; ++x)
      {
        var aliveNeighbours = 0;
        for (var yy = -1; yy < 2; ++yy)
        {
          var fy = (_height + y + yy)%_height;
          var fyoff = fy*_width;

          for (var xx = -1; xx < 2; ++xx)
          {
            var fx = (_width + x + xx)%_width;
            aliveNeighbours += _current[fx + fyoff] != Dead
              ? 1 
              : 0
              ;
          }
        }

        var current = _current[yoff + x];
        // If the current cell is alive the alive neighbours is +1
        //  because the loop above loops over all cells in 3x3 block
        //  including current
        aliveNeighbours -= current != Dead ? 1 : 0;

        var aliveAndWell = (byte)(Math.Min(current, (byte)254) + 1);

        byte next = aliveNeighbours switch
          {
            0 => Dead
          , 1 => Dead
          , 2 => current == Dead ? Dead :aliveAndWell
          , 3 => aliveAndWell
          , _ => Dead
          };
        _next[yoff + x] = next;
      }
    }
    var tmp = _current;
    _current = _next;
    _next     = tmp;
  }

  public override void Render(DrawingContext context)
  {
    var b = Bounds;
    var cellWidth = Math.Floor(b.Width/_width);
    var cellHeight = Math.Floor(b.Height/_height);
    var cell = Math.Min(cellWidth, cellHeight);

    var totWidth  = cell*_width;
    var totHeight = cell*_height;

    var offX = Math.Round((b.Width  - totWidth)*0.5);
    var offY = Math.Round((b.Height - totHeight)*0.5);

    for (var y = 0; y < _height; ++y)
    {
      var yoff = y*_width;
      for (var x = 0; x < _width; ++x)
      {
        var current = _current[x + yoff];
        if (current != Dead)
        {
          var brush = _brushes[current];
          context.DrawRectangle(brush, null, new Rect(offX+x*cell, offY+y*cell, cell-1, cell-1));
        }
      }
    }
    base.Render(context);
  }
}

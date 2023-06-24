using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;

namespace ItIsMyLife;

public partial class ItIsMyLifeControl : Control
{
  const int _width  = 256;
  const int _height = 256;

  enum Cell : byte
  {
    Dead  = 0
  , Young = 1
  , Old   = 2
  }

  Cell[] _current = new Cell[_width*_height];
  Cell[] _next    = new Cell[_width*_height];

  DispatcherTimer _timer;
  public ItIsMyLifeControl()
  {
    Ragnarök();

    _timer = new DispatcherTimer(
        TimeSpan.FromSeconds(0.125)
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
        _current[x + yoff] = isAlive ? Cell.Young : Cell.Dead;
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
            aliveNeighbours += !(xx == 0 && yy == 0) && _current[fx + fyoff] != Cell.Dead
              ? 1 
              : 0
              ;
          }
        }

        var current = _current[yoff + x];

        var next = aliveNeighbours switch
          {
            0 => Cell.Dead
          , 1 => Cell.Dead
          , 2 => current == Cell.Dead ? Cell.Dead   : Cell.Old
          , 3 => current == Cell.Dead ? Cell.Young  : Cell.Old
          , _ => Cell.Dead
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

    for (var y = 0; y < _height; ++y)
    {
      var yoff = y*_width;
      for (var x = 0; x < _width; ++x)
      {
        var current = _current[x + yoff];
        if (current != Cell.Dead)
        {
          var brush = current == Cell.Young ? Brushes.HotPink : Brushes.Purple;
          context.DrawRectangle(brush, null, new Rect(x*cell+1, y*cell+1, cell-1, cell-1));
        }
      }
    }
    base.Render(context);
  }
}

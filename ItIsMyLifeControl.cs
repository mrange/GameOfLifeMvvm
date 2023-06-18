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
  , Alive = 1
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
        _current[x + yoff] = isAlive ? Cell.Alive : Cell.Dead;
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

        _next[yoff + x] = aliveNeighbours switch
          {
            0 => Cell.Dead
          , 1 => Cell.Dead
          , 2 => _current[yoff + x]
          , 3 => Cell.Alive
          , _ => Cell.Dead
          };
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
        if (_current[x + yoff] != Cell.Dead)
        {
          context.DrawRectangle(Brushes.Purple, null, new Rect(x*cell+1, y*cell+1, cell-1, cell-1));
        }
      }
    }
    base.Render(context);
  }
}

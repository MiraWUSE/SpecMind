using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SpecMind.Views;

public partial class SimpleLineChart : UserControl
{
    public static readonly StyledProperty<ObservableCollection<double>> DataProperty =
        AvaloniaProperty.Register<SimpleLineChart, ObservableCollection<double>>(nameof(Data));

    public static readonly StyledProperty<IBrush> LineBrushProperty =
        AvaloniaProperty.Register<SimpleLineChart, IBrush>(nameof(LineBrush), new SolidColorBrush(Color.FromRgb(122, 169, 224)));

    public static readonly StyledProperty<double> MaxValueProperty =
        AvaloniaProperty.Register<SimpleLineChart, double>(nameof(MaxValue), 100);

    public ObservableCollection<double> Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public IBrush LineBrush
    {
        get => GetValue(LineBrushProperty);
        set => SetValue(LineBrushProperty, value);
    }

    public double MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    private Canvas? _chartCanvas;
    private bool _isDrawing;
    private List<Point> _points = new();
    private int _hoveredIndex = -1;
    private Border? _tooltipBorder;
    private TextBlock? _tooltipValue;
    private TextBlock? _tooltipTime;
    private Line? _verticalLine;
    private Ellipse? _hoverDot;

    public SimpleLineChart()
    {
        _chartCanvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
            ClipToBounds = true
        };

        // Tooltip с двумя строками
        _tooltipBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(40, 40, 60)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(122, 169, 224)),
            BorderThickness = new Thickness(1.5),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(12, 8),
            IsVisible = false,
            IsHitTestVisible = false,
            ZIndex = 100,
            BoxShadow = new BoxShadows(new BoxShadow
            {
                Color = Color.FromArgb(150, 0, 0, 0),
                Blur = 15,
                OffsetX = 0,
                OffsetY = 6
            })
        };

        var tooltipStack = new StackPanel { Orientation = Orientation.Vertical, Spacing = 2 };

        _tooltipValue = new TextBlock
        {
            FontSize = 14,
            Foreground = Brushes.White,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        _tooltipTime = new TextBlock
        {
            FontSize = 11,
            Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 200)),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center
        };

        tooltipStack.Children.Add(_tooltipValue);
        tooltipStack.Children.Add(_tooltipTime);
        _tooltipBorder.Child = tooltipStack;

        // Вертикальная линия
        _verticalLine = new Line
        {
            Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255), 0.2),
            StrokeThickness = 1,
            IsVisible = false,
            ZIndex = 50
        };

        // Точка при наведении (без DropShadowEffect)
        _hoverDot = new Ellipse
        {
            Width = 12,
            Height = 12,
            Fill = LineBrush,
            Stroke = new SolidColorBrush(Color.FromRgb(255, 255, 255)),
            StrokeThickness = 2.5,
            IsVisible = false,
            ZIndex = 60
        };

        var grid = new Grid();
        grid.Children.Add(_chartCanvas);
        grid.Children.Add(_verticalLine);
        grid.Children.Add(_hoverDot);
        grid.Children.Add(_tooltipBorder);

        Content = grid;

        _chartCanvas.PointerMoved += OnPointerMoved;
        _chartCanvas.PointerExited += OnPointerExited;
    }

    private bool _isAttached;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DataProperty)
        {
            if (change.OldValue is ObservableCollection<double> oldCollection)
            {
                oldCollection.CollectionChanged -= OnDataCollectionChanged;
            }

            if (_isAttached && change.NewValue is ObservableCollection<double> newCollection)
            {
                newCollection.CollectionChanged += OnDataCollectionChanged;
            }

            DrawChart();
        }
        else if (change.Property == MaxValueProperty || change.Property == LineBrushProperty)
        {
            DrawChart();
        }
    }

    private void OnDataCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!_isDrawing)
        {
            DrawChart();
        }
    }

    private void DrawChart()
    {
        if (_isDrawing) return;
        _isDrawing = true;

        try
        {
            var canvas = _chartCanvas;
            if (canvas == null) return;

            canvas.Children.Clear();
            _points.Clear();

            if (Data == null || Data.Count < 2) return;

            var width = canvas.Bounds.Width;
            var height = canvas.Bounds.Height;

            if (width <= 0 || height <= 0) return;

            var max = MaxValue > 0 ? MaxValue : 100;
            var stepX = width / (Data.Count - 1);

            // Улучшенная сетка
            var gridColor = new SolidColorBrush(Color.FromRgb(60, 60, 80));
            for (int i = 0; i <= 4; i++)
            {
                var y = (height / 4) * i;
                var line = new Line
                {
                    StartPoint = new Point(0, y),
                    EndPoint = new Point(width, y),
                    Stroke = gridColor,
                    StrokeThickness = 1
                };
                canvas.Children.Add(line);
            }

            // Точки
            for (int i = 0; i < Data.Count; i++)
            {
                var x = i * stepX;
                var value = Data[i];
                if (value < 0) value = 0;
                if (value > max) value = max;
                var y = height - (value / max) * height;
                _points.Add(new Point(x, y));
            }

            // Градиентная заливка (исправленный синтаксис для Avalonia)
            var fillPoints = new List<Point>(_points);
            fillPoints.Add(new Point(width, height));
            fillPoints.Add(new Point(0, height));

            var lineColor = (LineBrush as SolidColorBrush)?.Color ?? Colors.Blue;
            var fillPolyline = new Polygon
            {
                Points = new Points(fillPoints),
                Fill = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop { Color = Color.FromArgb(80, lineColor.R, lineColor.G, lineColor.B), Offset = 0 },
                        new GradientStop { Color = Color.FromArgb(10, lineColor.R, lineColor.G, lineColor.B), Offset = 1 }
                    }
                },
                Stroke = null
            };
            canvas.Children.Add(fillPolyline);

            // Более толстая и яркая линия
            var polyline = new Polyline
            {
                Points = new Points(_points),
                Stroke = LineBrush,
                StrokeThickness = 3
            };
            canvas.Children.Add(polyline);

            // Последнее значение
            if (_points.Count > 0)
            {
                var lastValue = Data[Data.Count - 1];
                var valueText = new TextBlock
                {
                    Text = $"{lastValue:F1}%",
                    FontSize = 13,
                    FontWeight = FontWeight.Bold,
                    Foreground = LineBrush
                };
                Canvas.SetLeft(valueText, width - 50);
                Canvas.SetTop(valueText, _points[_points.Count - 1].Y - 25);
                canvas.Children.Add(valueText);
            }
        }
        finally
        {
            _isDrawing = false;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_chartCanvas == null || _tooltipBorder == null || _tooltipValue == null || _tooltipTime == null || Data == null) return;
        if (Data.Count < 2 || _points.Count < 2) return;

        var pos = e.GetPosition(_chartCanvas);
        var width = _chartCanvas.Bounds.Width;
        var height = _chartCanvas.Bounds.Height;

        if (width <= 0 || height <= 0) return;

        var stepX = width / (Data.Count - 1);
        var index = (int)Math.Round(pos.X / stepX);

        if (index < 0) index = 0;
        if (index >= Data.Count) index = Data.Count - 1;

        if (index == _hoveredIndex) return;
        _hoveredIndex = index;

        var value = Data[index];
        var secondsAgo = Data.Count - 1 - index;

        // Вертикальная линия
        if (_verticalLine != null)
        {
            var pointX = _points[index].X;
            _verticalLine.StartPoint = new Point(pointX, 0);
            _verticalLine.EndPoint = new Point(pointX, height);
            _verticalLine.IsVisible = true;
        }

        // Точка
        if (_hoverDot != null)
        {
            Canvas.SetLeft(_hoverDot, _points[index].X - 6);
            Canvas.SetTop(_hoverDot, _points[index].Y - 6);
            _hoverDot.IsVisible = true;
        }

        // Tooltip
        _tooltipValue.Text = $"{value:F1}%";
        _tooltipTime.Text = $"{secondsAgo} сек. назад";
        _tooltipBorder.IsVisible = true;

        // Позиция tooltip
        var tooltipX = _points[index].X + 20;
        var tooltipY = _points[index].Y - 50;

        if (tooltipX + 100 > width)
        {
            tooltipX = _points[index].X - 115;
        }

        if (tooltipY < 0)
        {
            tooltipY = _points[index].Y + 20;
        }

        Canvas.SetLeft(_tooltipBorder, tooltipX);
        Canvas.SetTop(_tooltipBorder, tooltipY);
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        _hoveredIndex = -1;

        if (_tooltipBorder != null)
            _tooltipBorder.IsVisible = false;

        if (_verticalLine != null)
            _verticalLine.IsVisible = false;

        if (_hoverDot != null)
            _hoverDot.IsVisible = false;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        DrawChart();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttached = true;
        if (Data != null)
        {
            Data.CollectionChanged -= OnDataCollectionChanged;
            Data.CollectionChanged += OnDataCollectionChanged;
        }
        DrawChart();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttached = false;
        if (Data != null)
            Data.CollectionChanged -= OnDataCollectionChanged;
        base.OnDetachedFromVisualTree(e);
    }
}

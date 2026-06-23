using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
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
    private Border? _tooltipBorder;
    private TextBlock? _tooltipText;
    private bool _isDrawing;

    public SimpleLineChart()
    {
        _chartCanvas = new Canvas
        {
            Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)),
            ClipToBounds = true
        };

        _tooltipBorder = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(30, 30, 45)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(122, 169, 224)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(8, 4),
            IsVisible = false,
            IsHitTestVisible = false,
            ZIndex = 100
        };

        _tooltipText = new TextBlock
        {
            FontSize = 12,
            Foreground = Brushes.White
        };

        _tooltipBorder.Child = _tooltipText;

        var grid = new Grid();
        grid.Children.Add(_chartCanvas);
        grid.Children.Add(_tooltipBorder);

        Content = grid;

        _chartCanvas.PointerMoved += OnPointerMoved;
        _chartCanvas.PointerExited += OnPointerExited;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DataProperty)
        {
            if (change.OldValue is ObservableCollection<double> oldCollection)
            {
                oldCollection.CollectionChanged -= OnDataCollectionChanged;
            }

            if (change.NewValue is ObservableCollection<double> newCollection)
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

            if (Data == null || Data.Count < 2) return;

            var width = canvas.Bounds.Width;
            var height = canvas.Bounds.Height;

            if (width <= 0 || height <= 0) return;

            var max = MaxValue > 0 ? MaxValue : 100;
            var stepX = width / (Data.Count - 1);

            // Сетка
            var gridColor = new SolidColorBrush(Color.FromRgb(50, 50, 70));
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
            var points = new List<Point>();
            for (int i = 0; i < Data.Count; i++)
            {
                var x = i * stepX;
                var value = Data[i];
                if (value < 0) value = 0;
                if (value > max) value = max;
                var y = height - (value / max) * height;
                points.Add(new Point(x, y));
            }

            // Заливка
            var fillPoints = new List<Point>(points);
            fillPoints.Add(new Point(width, height));
            fillPoints.Add(new Point(0, height));

            var lineColor = (LineBrush as SolidColorBrush)?.Color ?? Colors.Blue;
            var fillPolyline = new Polygon
            {
                Points = new Points(fillPoints),
                Fill = new SolidColorBrush(lineColor, 0.15),
                Stroke = null
            };
            canvas.Children.Add(fillPolyline);

            // Линия
            var polyline = new Polyline
            {
                Points = new Points(points),
                Stroke = LineBrush,
                StrokeThickness = 2
            };
            canvas.Children.Add(polyline);

            // Последнее значение
            if (points.Count > 0)
            {
                var lastValue = Data[Data.Count - 1];
                var valueText = new TextBlock
                {
                    Text = $"{lastValue:F0}%",
                    FontSize = 12,
                    FontWeight = FontWeight.Bold,
                    Foreground = LineBrush
                };
                Canvas.SetLeft(valueText, width - 40);
                Canvas.SetTop(valueText, points[points.Count - 1].Y - 20);
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
        if (_chartCanvas == null || _tooltipBorder == null || _tooltipText == null || Data == null) return;
        if (Data.Count < 2) return;

        var pos = e.GetPosition(_chartCanvas);
        var width = _chartCanvas.Bounds.Width;

        if (width <= 0) return;

        var stepX = width / (Data.Count - 1);
        var index = (int)Math.Round(pos.X / stepX);

        if (index < 0) index = 0;
        if (index >= Data.Count) index = Data.Count - 1;

        var value = Data[index];
        _tooltipText.Text = $"{value:F1}%";
        _tooltipBorder.IsVisible = true;

        var tooltipX = pos.X + 10;
        var tooltipY = pos.Y - 30;

        if (tooltipX + 80 > width) tooltipX = pos.X - 80;
        if (tooltipY < 0) tooltipY = pos.Y + 10;

        Canvas.SetLeft(_tooltipBorder, tooltipX);
        Canvas.SetTop(_tooltipBorder, tooltipY);
    }

    private void OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (_tooltipBorder != null)
            _tooltipBorder.IsVisible = false;
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        DrawChart();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        DrawChart();
    }
}
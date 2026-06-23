using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using System;

namespace SpecMind.Views;

public class CircularGauge : Control
{
    public static readonly StyledProperty<double> ValueProperty =
        AvaloniaProperty.Register<CircularGauge, double>(nameof(Value), 0);

    public static readonly StyledProperty<double> MaxValueProperty =
        AvaloniaProperty.Register<CircularGauge, double>(nameof(MaxValue), 100);

    public static readonly StyledProperty<IBrush> GaugeBrushProperty =
        AvaloniaProperty.Register<CircularGauge, IBrush>(nameof(GaugeBrush), new SolidColorBrush(Color.FromRgb(122, 169, 224)));

    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<CircularGauge, string>(nameof(Label), "");

    public static readonly StyledProperty<string> UnitProperty =
        AvaloniaProperty.Register<CircularGauge, string>(nameof(Unit), "%");

    public double Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public IBrush GaugeBrush
    {
        get => GetValue(GaugeBrushProperty);
        set => SetValue(GaugeBrushProperty, value);
    }

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Unit
    {
        get => GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var width = Bounds.Width;
        var height = Bounds.Height;
        var size = Math.Min(width, height);
        var radius = (size - 20) / 2;
        var center = new Point(width / 2, height / 2);

        var gaugeColor = (GaugeBrush as SolidColorBrush)?.Color ?? Color.FromRgb(122, 169, 224);
        var bgColor = Color.FromRgb(50, 50, 70);
        var labelColor = Color.FromRgb(160, 160, 180);

        // Фоновый круг (полный)
        var backgroundEllipse = new EllipseGeometry(new Rect(
            center.X - radius,
            center.Y - radius,
            radius * 2,
            radius * 2
        ));
        context.DrawGeometry(null, new Pen(new SolidColorBrush(bgColor, 0.3), 10), backgroundEllipse);

        // Прогресс (дуга)
        var percentage = Math.Clamp(Value / MaxValue, 0, 1);
        var sweepAngle = percentage * 360;

        if (sweepAngle > 0.1)
        {
            var startAngle = -90.0; // Начинаем сверху
            var endAngle = startAngle + sweepAngle;

            var startPoint = new Point(
                center.X + radius * Math.Cos(startAngle * Math.PI / 180),
                center.Y + radius * Math.Sin(startAngle * Math.PI / 180)
            );

            var endPoint = new Point(
                center.X + radius * Math.Cos(endAngle * Math.PI / 180),
                center.Y + radius * Math.Sin(endAngle * Math.PI / 180)
            );

            var isLargeArc = sweepAngle > 180;

            var arcGeometry = new PathGeometry();
            var figures = new PathFigures();

            figures.Add(new PathFigure
            {
                StartPoint = startPoint,
                Segments = new PathSegments
                {
                    new ArcSegment
                    {
                        Point = endPoint,
                        Size = new Size(radius, radius),
                        RotationAngle = 0,
                        IsLargeArc = isLargeArc,
                        SweepDirection = SweepDirection.Clockwise,
                        IsStroked = true
                    }
                },
                IsFilled = false,
                IsClosed = false
            });

            arcGeometry.Figures = figures;

            context.DrawGeometry(null, new Pen(new SolidColorBrush(gaugeColor), 10), arcGeometry);
        }

        // Текст в центре
        var text = $"{Value:F0}{Unit}";
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI", FontStyle.Normal, FontWeight.Bold),
            28,
            new SolidColorBrush(gaugeColor)
        );

        var textOrigin = new Point(
            center.X - formattedText.Width / 2,
            center.Y - formattedText.Height / 2 - 10
        );

        context.DrawText(formattedText, textOrigin);

        // Подпись снизу
        if (!string.IsNullOrEmpty(Label))
        {
            var labelFormatted = new FormattedText(
                Label,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI", FontStyle.Normal, FontWeight.Normal),
                12,
                new SolidColorBrush(labelColor)
            );

            var labelOrigin = new Point(
                center.X - labelFormatted.Width / 2,
                center.Y + 15
            );

            context.DrawText(labelFormatted, labelOrigin);
        }
    }
}
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace MyWorkoutApp.Services;

public class InteractiveLineChartDrawable : IDrawable
{
    private readonly List<(string Label, double Value, string ExtraInfo)> _data;
    private readonly string _accentHex;
    private readonly string _suffix;
    private readonly double? _fixedMin;
    private readonly double? _fixedMax;
    private int? _selectedIndex;
    private PointF[] _points;

    public InteractiveLineChartDrawable(List<(string, double, string)> data, string accentHex, string suffix = "ק\"ג", double? fixedMin = null, double? fixedMax = null)
    {
        _data = data;
        _accentHex = accentHex;
        _suffix = suffix;
        _fixedMin = fixedMin;
        _fixedMax = fixedMax;
    }

    public void HandleTap(float x, float y)
    {
        if (_points == null || _points.Length == 0) return;

        float minDist = float.MaxValue;
        int nearestIdx = -1;

        for (int i = 0; i < _points.Length; i++)
        {
            float dist = MathF.Sqrt(
                MathF.Pow(_points[i].X - x, 2) +
                MathF.Pow(_points[i].Y - y, 2)
            );
            if (dist < minDist && dist < 30)
            {
                minDist = dist;
                nearestIdx = i;
            }
        }

        _selectedIndex = nearestIdx >= 0 ? nearestIdx : null;
    }

    public void ClearSelection() => _selectedIndex = null;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_data == null || _data.Count < 2) return;

        float w = dirtyRect.Width; float h = dirtyRect.Height;
        float padL = 40, padR = 16, padT = 12, padB = 28;
        float chartW = w - padL - padR; float chartH = h - padT - padB;

        double dataMin = _data.Min(d => d.Value);
        double dataMax = _data.Max(d => d.Value);

        double minV = _fixedMin ?? dataMin;
        double maxV = _fixedMax.HasValue ? Math.Max(_fixedMax.Value, dataMax) : dataMax;
        if (minV > dataMin) minV = dataMin;

        double range = maxV == minV ? 1 : maxV - minV;

        var accent = Color.FromArgb(_accentHex);

        // ── Grid lines ──
        canvas.StrokeColor = Color.FromArgb("#2a2a55");
        canvas.StrokeSize = 1;
        for (int i = 0; i <= 4; i++)
        {
            float y = padT + chartH - (chartH * i / 4f);
            canvas.DrawLine(padL, y, padL + chartW, y);

            double labelVal = minV + (range * i / 4.0);
            string labelStr = range < 4 ? labelVal.ToString("F1") : labelVal.ToString("F0");

            canvas.FontColor = Color.FromArgb("#6666aa");
            canvas.FontSize = 9;
            canvas.DrawString(labelStr, 0, y - 6, padL - 4, 14, HorizontalAlignment.Right, VerticalAlignment.Center);
        }

        // ── Compute points ──
        _points = new PointF[_data.Count];
        for (int i = 0; i < _data.Count; i++)
        {
            float x = padL + (chartW * i / (_data.Count - 1));
            float y = padT + chartH - (float)(((_data[i].Value - minV) / range) * chartH);
            _points[i] = new PointF(x, y);
        }

        // ── Area fill ──
        var path = new PathF();
        path.MoveTo(_points[0].X, padT + chartH);
        foreach (var pt in _points) path.LineTo(pt.X, pt.Y);
        path.LineTo(_points[^1].X, padT + chartH);
        path.Close();
        canvas.FillColor = accent.WithAlpha(0.12f);
        canvas.FillPath(path);

        // ── Line ──
        canvas.StrokeColor = accent;
        canvas.StrokeSize = 2.5f;
        canvas.StrokeLineCap = LineCap.Round;
        canvas.StrokeLineJoin = LineJoin.Round;
        var linePath = new PathF();
        linePath.MoveTo(_points[0]);
        for (int i = 1; i < _points.Length; i++) linePath.LineTo(_points[i]);
        canvas.DrawPath(linePath);

        // ── Dots + X labels + Tooltip ──
        for (int i = 0; i < _points.Length; i++)
        {
            bool isSelected = _selectedIndex == i;

            canvas.FillColor = accent.WithAlpha(isSelected ? 0.5f : 0.3f);
            canvas.FillCircle(_points[i].X, _points[i].Y, isSelected ? 8 : 6);
            canvas.FillColor = accent;
            canvas.FillCircle(_points[i].X, _points[i].Y, isSelected ? 5 : 3.5f);

            if (_data.Count <= 7 || i % 2 == 0)
            {
                canvas.FontColor = Color.FromArgb("#6666aa");
                canvas.FontSize = 9;
                canvas.DrawString(_data[i].Label, _points[i].X - 18, padT + chartH + 6, 36, 16,
                    HorizontalAlignment.Center, VerticalAlignment.Top);
            }

            // ── Tooltip ──
            if (isSelected)
            {
                string valueStr = _data[i].Value % 1 == 0 ? _data[i].Value.ToString("F0") : _data[i].Value.ToString("F1");
                string extra = string.IsNullOrEmpty(_data[i].ExtraInfo) ? "" : $" {_data[i].ExtraInfo}";
                string tooltipText = $"{valueStr} {_suffix}{extra}";

                float tooltipW = string.IsNullOrEmpty(_data[i].ExtraInfo) ? 75 : 95;
                float tooltipH = 28;
                float tooltipX = _points[i].X - tooltipW / 2;
                float tooltipY = _points[i].Y - tooltipH - 12;

                if (tooltipX < padL) tooltipX = padL;
                if (tooltipX + tooltipW > w - padR) tooltipX = w - padR - tooltipW;
                if (tooltipY < padT) tooltipY = _points[i].Y + 12;

                canvas.FillColor = Color.FromArgb("#1a1a3a");
                canvas.FillRoundedRectangle(tooltipX, tooltipY, tooltipW, tooltipH, 8);
                canvas.StrokeColor = accent;
                canvas.StrokeSize = 2;
                canvas.DrawRoundedRectangle(tooltipX, tooltipY, tooltipW, tooltipH, 8);

                canvas.FontColor = Colors.White;
                canvas.FontSize = 13;
                canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
                canvas.DrawString(tooltipText, tooltipX, tooltipY, tooltipW, tooltipH,
                    HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }
    }
}

public class BarbellDrawable : IDrawable
{
    private readonly List<double> _platesOneSide;
    private readonly double _barWeight;
    private static readonly Dictionary<double, string> PlateColors = new()
    {
        { 25, "#cc2222" }, { 20, "#2255cc" }, { 15, "#cc8800" },
        { 10, "#22aa44" }, { 5, "#333366" }, { 2.5, "#555577" }, { 1.25, "#444444" }
    };

    public BarbellDrawable(List<double> platesOneSide, double barWeight)
    {
        _platesOneSide = platesOneSide;
        _barWeight = barWeight;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float w = dirtyRect.Width; float cy = dirtyRect.Height / 2f;
        float barH = 10;
        canvas.FillColor = Color.FromArgb("#888899");
        canvas.FillRoundedRectangle(0, cy - barH / 2f, w, barH, 5);

        float collarW = 8, collarH = 26; float center = w / 2f;
        canvas.FillColor = Color.FromArgb("#aaaacc");
        canvas.FillRoundedRectangle(center - collarW / 2f - 30, cy - collarH / 2f, collarW, collarH, 3);
        canvas.FillRoundedRectangle(center + 30 - collarW / 2f, cy - collarH / 2f, collarW, collarH, 3);

        if (_platesOneSide.Count == 0) return;

        float startRight = center + 30 + collarW; float startLeft = center - 30 - collarW;
        float offsetR = 0, offsetL = 0;

        foreach (var plate in _platesOneSide)
        {
            float plateW = (float)(6 + plate * 0.5); float plateH = (float)(20 + plate * 1.4);
            string hex = PlateColors.TryGetValue(plate, out var c) ? c : "#555555";
            var col = Color.FromArgb(hex);

            canvas.FillColor = col; canvas.StrokeColor = Colors.White.WithAlpha(0.15f); canvas.StrokeSize = 1;
            canvas.FillRoundedRectangle(startRight + offsetR, cy - plateH / 2f, plateW, plateH, 2);
            canvas.DrawRoundedRectangle(startRight + offsetR, cy - plateH / 2f, plateW, plateH, 2);
            canvas.FillRoundedRectangle(startLeft - offsetL - plateW, cy - plateH / 2f, plateW, plateH, 2);
            canvas.DrawRoundedRectangle(startLeft - offsetL - plateW, cy - plateH / 2f, plateW, plateH, 2);

            offsetR += plateW + 2; offsetL += plateW + 2;
        }
    }
}
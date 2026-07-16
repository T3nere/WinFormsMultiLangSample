using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class ChartView : Panel
{
    private const float PointSpacing = 10f;
    private const float PointRadius = 3f;

    private readonly HScrollBar _hScrollBar;
    private readonly ChartPathBuilder _pathBuilder = new ChartPathBuilder();
    private readonly Pen _linePen = new Pen(Color.DodgerBlue, 2f);
    private readonly Brush _pointBrush = new SolidBrush(Color.DodgerBlue);

    private ChartDataManager _dataManager;
    private int _visibleStartIndex;
    private bool _followLatest = true;

    public ChartView()
    {
        DoubleBuffered = true;

        _hScrollBar = new HScrollBar
        {
            Dock = DockStyle.Bottom,
            Visible = false
        };
        _hScrollBar.Scroll += HScrollBar_Scroll;
        Controls.Add(_hScrollBar);
    }

    public ChartDataManager DataManager
    {
        get => _dataManager;
        set
        {
            if (_dataManager != null)
            {
                _dataManager.DataAdded -= OnDataAdded;
                _dataManager.MaxMinChanged -= OnMaxMinChanged;
            }

            _dataManager = value;

            if (_dataManager != null)
            {
                _dataManager.DataAdded += OnDataAdded;
                _dataManager.MaxMinChanged += OnMaxMinChanged;
            }

            UpdateScrollBarRange();
            Invalidate();
        }
    }

    private void OnDataAdded(object sender, EventArgs e)
    {
        UpdateScrollBarRange();

        if (_followLatest)
        {
            _hScrollBar.Value = GetScrollMaxValue();
            _visibleStartIndex = (int)(_hScrollBar.Value / PointSpacing);
        }

        Invalidate();
    }

    private void OnMaxMinChanged(object sender, EventArgs e)
    {
        Invalidate();
    }

    private void HScrollBar_Scroll(object sender, ScrollEventArgs e)
    {
        _visibleStartIndex = (int)(_hScrollBar.Value / PointSpacing);
        _followLatest = _hScrollBar.Value >= GetScrollMaxValue();
        Invalidate();
    }

    private int GetScrollMaxValue()
    {
        return Math.Max(0, _hScrollBar.Maximum - _hScrollBar.LargeChange + 1);
    }

    private void UpdateScrollBarRange()
    {
        if (_dataManager == null) return;

        int contentWidth = (int)(_dataManager.Count * PointSpacing);
        int clientWidth = ClientSize.Width;

        if (contentWidth <= clientWidth)
        {
            _hScrollBar.Visible = false;
            _visibleStartIndex = 0;
        }
        else
        {
            _hScrollBar.Visible = true;
            _hScrollBar.Minimum = 0;
            _hScrollBar.Maximum = contentWidth;
            _hScrollBar.LargeChange = clientWidth;
            _hScrollBar.SmallChange = (int)PointSpacing;
        }
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        UpdateScrollBarRange();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        if (_dataManager == null || _dataManager.Count == 0) return;
        if (_dataManager.Max == null || _dataManager.Min == null) return;

        int panelHeight = ClientSize.Height - (_hScrollBar.Visible ? _hScrollBar.Height : 0);
        int panelWidth = ClientSize.Width;

        int visibleCount = (int)Math.Ceiling(panelWidth / PointSpacing) + 1;
        var visibleData = _dataManager.GetRange(_visibleStartIndex, visibleCount);

        var mapper = new CoordinateMapper
        {
            PointSpacing = PointSpacing,
            PanelWidth = panelWidth,
            PanelHeight = panelHeight,
            Max = _dataManager.Max.Value,
            Min = _dataManager.Min.Value
        };

        var (linePath, pointPath) = _pathBuilder.Build(visibleData, _visibleStartIndex, _dataManager, mapper, PointRadius);

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.DrawPath(_linePen, linePath);
        e.Graphics.FillPath(_pointBrush, pointPath);

        linePath.Dispose();
        pointPath.Dispose();
    }
}

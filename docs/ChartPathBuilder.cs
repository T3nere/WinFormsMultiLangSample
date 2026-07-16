using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

public class ChartPathBuilder
{
    public (GraphicsPath LinePath, GraphicsPath PointPath) Build(
        IReadOnlyList<decimal?> visibleData,
        int visibleStartIndex,
        ChartDataManager dataManager,
        CoordinateMapper mapper,
        float pointRadius)
    {
        var linePath = new GraphicsPath();
        var pointPath = new GraphicsPath();
        PointF? previousPoint = null;

        for (int i = 0; i < visibleData.Count; i++)
        {
            int dataIndex = visibleStartIndex + i;
            decimal? value = visibleData[i];

            if (value == null)
            {
                previousPoint = null;
                continue;
            }

            var currentPoint = new PointF(
                mapper.GetX(dataIndex, visibleStartIndex),
                mapper.GetY(value.Value));

            pointPath.AddEllipse(
                currentPoint.X - pointRadius,
                currentPoint.Y - pointRadius,
                pointRadius * 2f,
                pointRadius * 2f);

            if (previousPoint != null && !dataManager.IsSegmentStart(dataIndex))
            {
                linePath.StartFigure();
                linePath.AddLine(previousPoint.Value, currentPoint);
            }

            previousPoint = currentPoint;
        }

        return (linePath, pointPath);
    }
}

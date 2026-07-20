using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace WinFormsMultiLangSample
{
    internal class HitDetection
    {
        public class PathHitTestForm : Form
        {
            // 判定対象のGraphicsPathのリスト
            private readonly List<GraphicsPath> _paths = new List<GraphicsPath>();
            private readonly Label _infoLabel = new Label
            {
                AutoSize = true,
                Location = new Point(10, 10),
                Font = new Font("Consolas", 10)
            };

            public PathHitTestForm()
            {
                Text = "GraphicsPath Hit Test";
                Width = 800;
                Height = 600;
                DoubleBuffered = true; // 描画のちらつき防止

                // --- サンプルとして図形をいくつか作成 ---
                var circlePath = new GraphicsPath();
                circlePath.AddEllipse(50, 50, 150, 150);
                _paths.Add(circlePath);

                var rectPath = new GraphicsPath();
                rectPath.AddRectangle(new Rectangle(300, 100, 200, 120));
                _paths.Add(rectPath);

                var polygonPath = new GraphicsPath();
                polygonPath.AddPolygon(new[]
                {
            new Point(500, 300),
            new Point(650, 300),
            new Point(600, 450),
            new Point(450, 400)
        });
                _paths.Add(polygonPath);
                // --------------------------------------

                Controls.Add(_infoLabel);

                MouseMove += OnMouseMove;
                Paint += OnPaint;
            }

            private void OnMouseMove(object sender, MouseEventArgs e)
            {
                bool hit = false;

                foreach (var path in _paths)
                {
                    // カーソル座標がパスの内部にあるかを判定
                    if (path.IsVisible(e.Location))
                    {
                        hit = true;
                        _infoLabel.Text = $"Y座標: {e.Y}";
                        break; // 最初に見つかったパスで確定する場合
                    }
                }

                if (!hit)
                {
                    _infoLabel.Text = "(カーソルはどのパスにも重なっていません)";
                }
            }

            private void OnPaint(object sender, PaintEventArgs e)
            {
                using (var pen = new Pen(Color.Black, 2))
                {
                    foreach (var path in _paths)
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }
    }
}

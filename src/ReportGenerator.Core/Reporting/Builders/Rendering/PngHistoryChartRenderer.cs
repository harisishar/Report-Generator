using System.Collections.Generic;
using System.IO;
using System.Linq;
using Palmmedia.ReportGenerator.Core.Parser.Analysis;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Palmmedia.ReportGenerator.Core.Reporting.Builders.Rendering
{
    /// <summary>
    /// Renderes history chart in PNG format.
    /// </summary>
    internal class PngHistoryChartRenderer
    {
        /// <summary>
        /// Renderes the given historic coverages as PNG image.
        /// </summary>
        /// <param name="historicCoverages">The historic coverages.</param>
        /// <param name="methodCoverageAvailable">if set to <c>true</c> method coverage is available.</param>
        /// <returns>The image in PNG format.</returns>
        public static byte[] RenderHistoryChart(IReadOnlyList<HistoricCoverage> historicCoverages, bool methodCoverageAvailable)
        {
            using (Image<Rgba32> image = new Image<Rgba32>(1450, 150))
            using (MemoryStream output = new MemoryStream())
            {
                var grayPen = Pens.Dash(Color.LightGray, 1);
                var redPen = Pens.Solid(Color.ParseHex("cc0000"), 2);
                var bluePen = Pens.Solid(Color.ParseHex("1c2298"), 2);
                var greenPen = Pens.Solid(Color.ParseHex("0aad0a"), 2);

                var redBrush = Brushes.Solid(Color.ParseHex("cc0000"));
                var blueBrush = Brushes.Solid(Color.ParseHex("1c2298"));
                var greenBrush = Brushes.Solid(Color.ParseHex("0aad0a"));

                int numberOfLines = historicCoverages.Count;

                if (numberOfLines == 1)
                {
                    numberOfLines = 2;
                }

                float totalWidth = 1445 - 50;
                float width = totalWidth / (numberOfLines - 1);

                float totalHeight = 115 - 15;

                image.Mutate(ctx =>
                {
                    ctx.Fill(Color.White);

                    ctx.DrawLines(grayPen, new PointF(50, 115), new PointF(1445, 115));
                    ctx.DrawLines(grayPen, new PointF(50, 90), new PointF(1445, 90));
                    ctx.DrawLines(grayPen, new PointF(50, 65), new PointF(1445, 65));
                    ctx.DrawLines(grayPen, new PointF(50, 40), new PointF(1445, 40));
                    ctx.DrawLines(grayPen, new PointF(50, 15), new PointF(1445, 15));

                    for (int i = 0; i < numberOfLines; i++)
                    {
                        ctx.DrawLines(grayPen, new PointF(50 + (i * width), 15), new PointF(50 + (i * width), 115));
                    }

                    if (historicCoverages.Any(h => h.CoverageQuota.HasValue))
                    {
                        for (int i = 1; i < historicCoverages.Count; i++)
                        {
                            if (!historicCoverages[i - 1].CoverageQuota.HasValue
                                || !historicCoverages[i].CoverageQuota.HasValue)
                            {
                                continue;
                            }

                            float x1 = 50 + ((i - 1) * width);
                            float y1 = 15 + (((100 - (float)historicCoverages[i - 1].CoverageQuota.Value) * totalHeight) / 100);

                            float x2 = 50 + (i * width);
                            float y2 = 15 + (((100 - (float)historicCoverages[i].CoverageQuota.Value) * totalHeight) / 100);

                            ctx.DrawLines(redPen, new PointF(x1, y1), new PointF(x2, y2));
                        }
                    }

                    if (historicCoverages.Any(h => h.BranchCoverageQuota.HasValue))
                    {
                        for (int i = 1; i < historicCoverages.Count; i++)
                        {
                            if (!historicCoverages[i - 1].BranchCoverageQuota.HasValue
                                || !historicCoverages[i].BranchCoverageQuota.HasValue)
                            {
                                continue;
                            }

                            float x1 = 50 + ((i - 1) * width);
                            float y1 = 15 + (((100 - (float)historicCoverages[i - 1].BranchCoverageQuota.Value) * totalHeight) / 100);

                            float x2 = 50 + (i * width);
                            float y2 = 15 + (((100 - (float)historicCoverages[i].BranchCoverageQuota.Value) * totalHeight) / 100);

                            ctx.DrawLines(bluePen, new PointF(x1, y1), new PointF(x2, y2));
                        }
                    }

                    if (methodCoverageAvailable && historicCoverages.Any(h => h.CodeElementCoverageQuota.HasValue))
                    {
                        for (int i = 1; i < historicCoverages.Count; i++)
                        {
                            if (!historicCoverages[i - 1].CodeElementCoverageQuota.HasValue
                                || !historicCoverages[i].CodeElementCoverageQuota.HasValue)
                            {
                                continue;
                            }

                            float x1 = 50 + ((i - 1) * width);
                            float y1 = 15 + (((100 - (float)historicCoverages[i - 1].CodeElementCoverageQuota.Value) * totalHeight) / 100);

                            float x2 = 50 + (i * width);
                            float y2 = 15 + (((100 - (float)historicCoverages[i].CodeElementCoverageQuota.Value) * totalHeight) / 100);

                            ctx.DrawLines(greenPen, new PointF(x1, y1), new PointF(x2, y2));
                        }
                    }

                    for (int i = 0; i < historicCoverages.Count; i++)
                    {
                        if (!historicCoverages[i].CoverageQuota.HasValue)
                        {
                            continue;
                        }

                        float x1 = 50 + (i * width);
                        float y1 = 15 + (((100 - (float)historicCoverages[i].CoverageQuota.Value) * totalHeight) / 100);

                        ctx.Fill(redBrush, new EllipsePolygon(x1, y1, 3));
                    }

                    for (int i = 0; i < historicCoverages.Count; i++)
                    {
                        if (!historicCoverages[i].BranchCoverageQuota.HasValue)
                        {
                            continue;
                        }

                        float x1 = 50 + (i * width);
                        float y1 = 15 + (((100 - (float)historicCoverages[i].BranchCoverageQuota.Value) * totalHeight) / 100);

                        ctx.Fill(blueBrush, new EllipsePolygon(x1, y1, 3));
                    }

                    if (methodCoverageAvailable)
                    {
                        for (int i = 0; i < historicCoverages.Count; i++)
                        {
                            if (!historicCoverages[i].CodeElementCoverageQuota.HasValue)
                            {
                                continue;
                            }

                            float x1 = 50 + (i * width);
                            float y1 = 15 + (((100 - (float)historicCoverages[i].CodeElementCoverageQuota.Value) * totalHeight) / 100);

                            ctx.Fill(greenBrush, new EllipsePolygon(x1, y1, 3));
                        }
                    }

                    try
                    {
                        var font = SystemFonts.CreateFont("Arial", 11, FontStyle.Regular);

                        TextOptions options = new TextOptions(font)
                        {
                            HorizontalAlignment = HorizontalAlignment.Right
                        };

                        options.Origin = new PointF(38, 5);
                        ctx.DrawText(options, "100", Color.Gray);

                        options.Origin = new PointF(38, 30);
                        ctx.DrawText(options, "75", Color.Gray);

                        options.Origin = new PointF(38, 55);
                        ctx.DrawText(options, "50", Color.Gray);

                        options.Origin = new PointF(38, 80);
                        ctx.DrawText(options, "25", Color.Gray);

                        options.Origin = new PointF(38, 1055);
                        ctx.DrawText(options, "0", Color.Gray);
                    }
                    catch (FontFamilyNotFoundException)
                    {
                        // Font 'Arial' may not be present on Linux
                    }
                });

                image.Save(output, new PngEncoder());
                return output.ToArray();
            }
        }
    }
}

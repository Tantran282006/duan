using System;
using System.Collections.Generic;
using UnityEngine;

namespace PhoNho.Art.Editor
{
    // All pixel arrays in this class use a TOP-LEFT origin.
    public static class CharacterSheetPixels
    {
        public const int CellWidth = 512;
        public const int CellHeight = 640;
        public const int Baseline = 576;
        private const byte AlphaCutoff = 32;

        public sealed class Frame
        {
            public int X, Y, Width, Height, MinX, MinY, MaxX, MaxY;
            public float AnchorX;
            public int BodyHeight => MaxY - MinY + 1;
        }

        public static void ScrubLowAlpha(Color32[] pixels)
        {
            for (int i = 0; i < pixels.Length; i++)
                if (pixels[i].a < AlphaCutoff) pixels[i] = new Color32(0, 0, 0, 0);
        }

        public static Color32[] Clean(Color32[] source, int width, int height,
            int columns, int rows, out List<Frame> frames)
        {
            if (columns < 1 || rows < 1 || columns > 16 || rows > 16 ||
                source.Length != width * height || width < columns || height < rows)
                throw new ArgumentException("Invalid sprite grid.");
            var output = new Color32[source.Length];
            frames = new List<Frame>();
            int[] edges = RowEdges(source, width, height, rows);
            for (int row = 0; row < rows; row++)
            for (int column = 0; column < columns; column++)
            {
                var f = new Frame { X = column * width / columns, Y = edges[row],
                    Width = (column + 1) * width / columns - column * width / columns,
                    Height = edges[row + 1] - edges[row] };
                var visited = new bool[f.Width * f.Height];
                var largest = new List<int>();
                for (int i = 0; i < visited.Length; i++)
                {
                    if (visited[i]) continue;
                    visited[i] = true;
                    if (source[Index(f, i % f.Width, i / f.Width, width)].a < AlphaCutoff) continue;
                    var component = new List<int> { i };
                    for (int j = 0; j < component.Count; j++)
                    {
                        int x = component[j] % f.Width, y = component[j] / f.Width;
                        for (int dy = -1; dy <= 1; dy++)
                        for (int dx = -1; dx <= 1; dx++)
                        {
                            int nx = x + dx, ny = y + dy;
                            if (nx < 0 || nx >= f.Width || ny < 0 || ny >= f.Height) continue;
                            int n = ny * f.Width + nx;
                            if (visited[n]) continue;
                            visited[n] = true;
                            if (source[Index(f, nx, ny, width)].a >= AlphaCutoff) component.Add(n);
                        }
                    }
                    if (component.Count > largest.Count) largest = component;
                }
                if (largest.Count < visited.Length * .02f)
                    throw new InvalidOperationException("No reliable character silhouette. Check the grid.");
                var mask = new bool[visited.Length];
                f.MinX = f.Width; f.MinY = f.Height;
                foreach (int i in largest)
                {
                    mask[i] = true;
                    int x = i % f.Width, y = i / f.Width;
                    f.MinX = Math.Min(f.MinX, x); f.MaxX = Math.Max(f.MaxX, x);
                    f.MinY = Math.Min(f.MinY, y); f.MaxY = Math.Max(f.MaxY, y);
                }
                foreach (int i in largest)
                {
                    int x = i % f.Width, y = i / f.Width, p = Index(f, x, y, width);
                    Color32 color = source[p];
                    if (color.a < 220)
                    {
                        int best = int.MaxValue, neighbor = -1;
                        for (int dy = -3; dy <= 3; dy++)
                        for (int dx = -3; dx <= 3; dx++)
                        {
                            int nx = x + dx, ny = y + dy, distance = dx * dx + dy * dy;
                            if (nx < 0 || nx >= f.Width || ny < 0 || ny >= f.Height || !mask[ny * f.Width + nx]) continue;
                            int n = Index(f, nx, ny, width);
                            if (source[n].a >= 220 && distance < best) { best = distance; neighbor = n; }
                        }
                        if (neighbor >= 0)
                        { color.r = source[neighbor].r; color.g = source[neighbor].g; color.b = source[neighbor].b; }
                    }
                    output[p] = color;
                }
                var centers = new List<float>();
                int low = f.MinY + (int)((f.MaxY - f.MinY) * .45f);
                int high = f.MinY + (int)((f.MaxY - f.MinY) * .58f);
                for (int y = low; y <= high; y++)
                {
                    int start = -1, bestStart = 0, bestLength = 0;
                    for (int x = f.MinX; x <= f.MaxX + 1; x++)
                    {
                        if (x <= f.MaxX && mask[y * f.Width + x]) { if (start < 0) start = x; }
                        else if (start >= 0)
                        { if (x - start > bestLength) { bestLength = x - start; bestStart = start; } start = -1; }
                    }
                    if (bestLength > 0) centers.Add(bestStart + (bestLength - 1) * .5f);
                }
                centers.Sort(); f.AnchorX = centers[centers.Count / 2]; frames.Add(f);
            }
            return output;
        }

        private static int Index(Frame frame, int x, int y, int width) => (frame.Y + y) * width + frame.X + x;

        private static int[] RowEdges(Color32[] pixels, int width, int height, int rows)
        {
            var edges = new int[rows + 1]; edges[rows] = height;
            for (int r = 1; r < rows; r++)
            {
                int expected = r * height / rows, radius = (int)(height / (float)rows * .12f);
                int start = -1, bestStart = -1, bestLength = 0;
                for (int y = expected - radius; y <= expected + radius + 1; y++)
                {
                    int count = 0;
                    if (y <= expected + radius)
                        for (int x = 0; x < width; x++) if (pixels[y * width + x].a >= AlphaCutoff) count++;
                    if (y <= expected + radius && count == 0) { if (start < 0) start = y; }
                    else if (start >= 0)
                    { if (y - start > bestLength) { bestStart = start; bestLength = y - start; } start = -1; }
                }
                edges[r] = bestStart >= 0 ? bestStart + bestLength / 2 : expected;
            }
            return edges;
        }

        public static Color32[] Normalize(Color32[] clean, int sourceWidth, List<Frame> frames, int columns, int rows)
        {
            var heights = new List<int>(); foreach (Frame f in frames) heights.Add(f.BodyHeight);
            heights.Sort(); float scale = 512f / heights[heights.Count / 2];
            int width = columns * CellWidth;
            var output = new Color32[width * rows * CellHeight];
            for (int i = 0; i < frames.Count; i++)
            {
                Frame f = frames[i];
                int cropWidth = f.MaxX - f.MinX + 1;
                int resizedWidth = Mathf.RoundToInt(cropWidth * scale), resizedHeight = Mathf.RoundToInt(f.BodyHeight * scale);
                int left = Mathf.RoundToInt(CellWidth * .5f - (f.AnchorX - f.MinX) * scale), top = Baseline - resizedHeight;
                if (left < 8 || left + resizedWidth > CellWidth - 8 || top < 8)
                    throw new InvalidOperationException("Character cannot fit without clipping; use a custom profile.");
                for (int y = 0; y < resizedHeight; y++)
                for (int x = 0; x < resizedWidth; x++)
                {
                    float sx = (x + .5f) * cropWidth / resizedWidth - .5f;
                    float sy = (y + .5f) * f.BodyHeight / resizedHeight - .5f;
                    output[((i / columns) * CellHeight + top + y) * width +
                        (i % columns) * CellWidth + left + x] = Sample(clean, sourceWidth, f, sx, sy);
                }
            }
            ScrubLowAlpha(output);
            return output;
        }

        private static Color32 Sample(Color32[] pixels, int width, Frame f, float x, float y)
        {
            int ix = Mathf.FloorToInt(x), iy = Mathf.FloorToInt(y);
            float tx = x - ix, ty = y - iy, alpha = 0, red = 0, green = 0, blue = 0;
            for (int dy = 0; dy <= 1; dy++)
            for (int dx = 0; dx <= 1; dx++)
            {
                int sx = Mathf.Clamp(ix + dx, 0, f.MaxX - f.MinX), sy = Mathf.Clamp(iy + dy, 0, f.BodyHeight - 1);
                Color32 c = pixels[(f.Y + f.MinY + sy) * width + f.X + f.MinX + sx];
                float weight = (dx == 0 ? 1 - tx : tx) * (dy == 0 ? 1 - ty : ty), a = c.a * weight;
                alpha += a; red += c.r * a; green += c.g * a; blue += c.b * a;
            }
            if (alpha < .5f) return new Color32(0, 0, 0, 0);
            return new Color32((byte)Mathf.RoundToInt(red / alpha), (byte)Mathf.RoundToInt(green / alpha),
                (byte)Mathf.RoundToInt(blue / alpha), (byte)Mathf.RoundToInt(alpha));
        }
    }
}

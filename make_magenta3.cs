using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;

public class ProgramMagenta3 {
    public static void ProcessImage(string path) {
        try {
            Bitmap bmp = new Bitmap(path);
            Bitmap final = new Bitmap(bmp.Width, bmp.Height);
            Color magenta = Color.FromArgb(255, 255, 0, 255);
            
            Color bgKey = bmp.GetPixel(5, 5);
            float thresh = 40f; 

            bool[,] visited = new bool[bmp.Width, bmp.Height];
            Queue<Point> q = new Queue<Point>();
            q.Enqueue(new Point(0, 0));
            q.Enqueue(new Point(bmp.Width - 1, 0));
            q.Enqueue(new Point(0, bmp.Height - 1));
            q.Enqueue(new Point(bmp.Width - 1, bmp.Height - 1));

            for(int y = 0; y < bmp.Height; y++) {
                for(int x = 0; x < bmp.Width; x++) {
                    final.SetPixel(x, y, bmp.GetPixel(x,y));
                }
            }

            while(q.Count > 0) {
                Point p = q.Dequeue();
                if (p.X < 0 || p.X >= bmp.Width || p.Y < 0 || p.Y >= bmp.Height) continue;
                if (visited[p.X, p.Y]) continue;
                
                Color c = bmp.GetPixel(p.X, p.Y);
                bool isBg = c.A < 10 || 
                           (Math.Abs(c.R - bgKey.R) + Math.Abs(c.G - bgKey.G) + Math.Abs(c.B - bgKey.B) < thresh);

                if (isBg || c.R > 230 && c.G > 230 && c.B > 230) {
                    visited[p.X, p.Y] = true;
                    final.SetPixel(p.X, p.Y, magenta);
                    
                    q.Enqueue(new Point(p.X - 1, p.Y));
                    q.Enqueue(new Point(p.X + 1, p.Y));
                    q.Enqueue(new Point(p.X, p.Y - 1));
                    q.Enqueue(new Point(p.X, p.Y + 1));
                }
            }
            
            // Dispose original file handler to release file lock, then save
            bmp.Dispose();
            string tempPath = path + ".tmp";
            final.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);
            final.Dispose();
            
            File.Delete(path);
            File.Move(tempPath, path);
            Console.WriteLine("Painted magenta background: " + path);
        } catch (Exception e) {
            Console.WriteLine("Error on " + path + ": " + e.Message);
        }
    }

    public static void Main() {
        ProcessImage(@"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\cryptominer_sprite_1772827344346.png");
        ProcessImage(@"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\stock_options_sprite_1772827497492.png");
    }
}

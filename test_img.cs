using System;
using System.Drawing;
public class Program {
    public static void Main() {
        try {
            Bitmap bmp = new Bitmap(@"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\cryptominer_sprite_1772827344346.png");
            Color corner = bmp.GetPixel(0, 0);
            Color center = bmp.GetPixel(bmp.Width/2, bmp.Height/2);
            Console.WriteLine("CryptoCorner: " + corner);
            Console.WriteLine("CryptoCenter: " + center);

            Bitmap bmp2 = new Bitmap(@"c:\CODE\IA\Unity\Money-Survivor\Assets\Sprites\stock_options_sprite_1772827497492.png");
            Console.WriteLine("StockCorner: " + bmp2.GetPixel(0, 0));
        } catch (Exception e) {
            Console.WriteLine("Error: " + e.Message);
        }
    }
}

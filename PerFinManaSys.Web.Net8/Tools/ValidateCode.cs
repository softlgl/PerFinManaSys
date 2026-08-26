using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;

namespace PerFinManaSys.Web.Net8.Tools;

/// <summary>
/// 验证码生成（.NET 8 使用 System.Drawing.Common）。
/// 返回验证码明文与图片字节，供 LoginController 存入 Session 并输出。
/// </summary>
public static class ValidateCode
{
    private static readonly char[] Chars =
        "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public static string CreateValidateCode(int length = 4)
    {
        var sb = new StringBuilder();
        // 使用线程安全的随机数
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
        foreach (var b in bytes)
        {
            sb.Append(Chars[b % Chars.Length]);
        }
        return sb.ToString();
    }

    public static byte[] CreateValidateGraphic(string code)
    {
        const int width = 100;
        const int height = 36;
        using var bitmap = new Bitmap(width, height);
        using var g = Graphics.FromImage(bitmap);
        g.Clear(Color.White);

        // 干扰线
        using var rng = RandomNumberGenerator.Create();
        var lineBytes = new byte[6];
        rng.GetBytes(lineBytes);
        var rnd = new Random(BitConverter.ToInt32(lineBytes, 0));
        for (int i = 0; i < 3; i++)
        {
            int x1 = rnd.Next(width), y1 = rnd.Next(height);
            int x2 = rnd.Next(width), y2 = rnd.Next(height);
            g.DrawLine(new Pen(Color.LightGray), x1, y1, x2, y2);
        }

        // 文字
        using var font = new Font("Arial", 18, FontStyle.Bold);
        using var brush = new SolidBrush(Color.Black);
        var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        g.DrawString(code, font, brush, new RectangleF(0, 0, width, height), sf);

        using var ms = new MemoryStream();
        bitmap.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }
}

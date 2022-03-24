/*
*   Author  :   JackerKun
*   Date    :   Friday, 04 March 2022 11:24:18
*   About   :
*/


using SkiaSharp;

namespace Jc.QRCode;

/// <summary>
/// 二维码模块
/// </summary>
public class JcQRCode
{
    /// <summary>
    /// 生成二维码
    /// </summary>
    /// <param name="text">文本内容</param>
    /// <param name="outPath">输出位置</param>
    /// <param name="format">文件格式</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="logoImgae">logo路径</param>
    /// <param name="keepWhiteBorderPixelVal">白边处理(负值表示不做处理，最大值不超过真实二维码区域的1/10)</param>
    /// <returns></returns>
    public bool Create(
        string text,
        string outPath,
        SKEncodedImageFormat format = SKEncodedImageFormat.Png,
        int width = 320,
        int height = 320,
        string logoImgae = null,
        int keepWhiteBorderPixelVal = -1)
    {
        try
        {
            byte[] logo = null;
            if (!string.IsNullOrEmpty(logoImgae) && File.Exists(logoImgae))
            {
                logo = File.ReadAllBytes(logoImgae);
            }

            var code = Create(text, format, width, height, logo, keepWhiteBorderPixelVal);
            FileStream fs = new FileStream(outPath, FileMode.Create);
            fs.Write(code);
            fs.Dispose();
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return false;
    }

    /// <summary>
    /// 生成二维码
    /// </summary>
    /// <param name="text">文本内容</param>
    /// <param name="format">保存格式</param>
    /// <param name="width">宽度</param>
    /// <param name="height">高度</param>
    /// <param name="logoImgae">Logo图片(缩放到真实二维码区域尺寸的1/6)</param>
    /// <param name="keepWhiteBorderPixelVal">白边处理(负值表示不做处理，最大值不超过真实二维码区域的1/10)</param>
    /// <returns></returns>
    public byte[] Create(
        string text,
        SKEncodedImageFormat format = SKEncodedImageFormat.Png,
        int width = 320,
        int height = 320,
        byte[] logoImgae = null,
        int keepWhiteBorderPixelVal = -1)
    {
        byte[] reval = null;
        try
        {
            var qRCodeWriter = new ZXing.QrCode.QRCodeWriter();
            var hints = new Dictionary<ZXing.EncodeHintType, object>();
            hints.Add(ZXing.EncodeHintType.CHARACTER_SET, "utf-8");
            hints.Add(ZXing.EncodeHintType.QR_VERSION, 8);
            hints.Add(ZXing.EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.Q);
            var bitMatrix = qRCodeWriter.encode(text, ZXing.BarcodeFormat.QR_CODE, width, height, hints);
            var w = bitMatrix.Width;
            var h = bitMatrix.Height;
            var sKBitmap = new SkiaSharp.SKBitmap(w, h);

            int blackStartPointX = 0;
            int blackStartPointY = 0;
            int blackEndPointX = w;
            int blackEndPointY = h;

            #region --绘制二维码(同时获取真实的二维码区域起绘点和结束点的坐标)--

            var sKCanvas = new SkiaSharp.SKCanvas(sKBitmap);
            var sKColorBlack = SkiaSharp.SKColor.Parse("000000");
            var sKColorWihte = SkiaSharp.SKColor.Parse("ffffff");
            sKCanvas.Clear(sKColorWihte);
            bool blackStartPointIsNotWriteDown = true;
            for (var y = 0; y < h; y++)
            {
                for (var x = 0; x < w; x++)
                {
                    var flag = bitMatrix[x, y];
                    if (flag)
                    {
                        if (blackStartPointIsNotWriteDown)
                        {
                            blackStartPointX = x;
                            blackStartPointY = y;
                            blackStartPointIsNotWriteDown = false;
                        }

                        blackEndPointX = x;
                        blackEndPointY = y;
                        sKCanvas.DrawPoint(x, y, sKColorBlack);
                    }
                    else
                    {
                        //sKCanvas.DrawPoint(x, y, sKColorWihte);//不用绘制(背景是白色的)
                    }
                }
            }

            sKCanvas.Dispose();

            #endregion

            int qrcodeRealWidth = blackEndPointX - blackStartPointX;
            int qrcodeRealHeight = blackEndPointY - blackStartPointY;

            #region -- 处理白边 --

            if (keepWhiteBorderPixelVal > -1) //指定了边框宽度
            {
                var borderMaxWidth = (int) Math.Floor((double) qrcodeRealWidth / 10);
                if (keepWhiteBorderPixelVal > borderMaxWidth)
                {
                    keepWhiteBorderPixelVal = borderMaxWidth;
                }

                var nQrcodeRealWidth = width - keepWhiteBorderPixelVal - keepWhiteBorderPixelVal;
                var nQrcodeRealHeight = height - keepWhiteBorderPixelVal - keepWhiteBorderPixelVal;

                var sKBitmap2 = new SkiaSharp.SKBitmap(width, height);
                var sKCanvas2 = new SkiaSharp.SKCanvas(sKBitmap2);
                sKCanvas2.Clear(sKColorWihte);
                //二维码绘制到临时画布上时无需抗锯齿等处理(避免文件增大)
                sKCanvas2.DrawBitmap(
                    sKBitmap,
                    new SkiaSharp.SKRect
                    {
                        Location = new SkiaSharp.SKPoint {X = blackStartPointX, Y = blackStartPointY},
                        Size = new SkiaSharp.SKSize {Height = qrcodeRealHeight, Width = qrcodeRealWidth}
                    },
                    new SkiaSharp.SKRect
                    {
                        Location = new SkiaSharp.SKPoint {X = keepWhiteBorderPixelVal, Y = keepWhiteBorderPixelVal},
                        Size = new SkiaSharp.SKSize {Width = nQrcodeRealWidth, Height = nQrcodeRealHeight}
                    });

                blackStartPointX = keepWhiteBorderPixelVal;
                blackStartPointY = keepWhiteBorderPixelVal;
                qrcodeRealWidth = nQrcodeRealWidth;
                qrcodeRealHeight = nQrcodeRealHeight;

                sKCanvas2.Dispose();
                sKBitmap.Dispose();
                sKBitmap = sKBitmap2;
            }

            #endregion

            #region -- 绘制LOGO --

            if (logoImgae != null && logoImgae.Length > 0)
            {
                SkiaSharp.SKBitmap sKBitmapLogo = SkiaSharp.SKBitmap.Decode(logoImgae);
                if (!sKBitmapLogo.IsEmpty)
                {
                    var sKPaint2 = new SkiaSharp.SKPaint
                    {
                        FilterQuality = SkiaSharp.SKFilterQuality.None,
                        IsAntialias = true
                    };
                    var logoTargetMaxWidth = (int) Math.Floor((double) qrcodeRealWidth / 6);
                    var logoTargetMaxHeight = (int) Math.Floor((double) qrcodeRealHeight / 6);
                    var qrcodeCenterX = (int) Math.Floor((double) qrcodeRealWidth / 2);
                    var qrcodeCenterY = (int) Math.Floor((double) qrcodeRealHeight / 2);
                    var logoResultWidth = sKBitmapLogo.Width;
                    var logoResultHeight = sKBitmapLogo.Height;
                    if (logoResultWidth > logoTargetMaxWidth)
                    {
                        var r = (double) logoTargetMaxWidth / logoResultWidth;
                        logoResultWidth = logoTargetMaxWidth;
                        logoResultHeight = (int) Math.Floor(logoResultHeight * r);
                    }

                    if (logoResultHeight > logoTargetMaxHeight)
                    {
                        var r = (double) logoTargetMaxHeight / logoResultHeight;
                        logoResultHeight = logoTargetMaxHeight;
                        logoResultWidth = (int) Math.Floor(logoResultWidth * r);
                    }

                    var pointX = qrcodeCenterX - (int) Math.Floor((double) logoResultWidth / 2) + blackStartPointX;
                    var pointY = qrcodeCenterY - (int) Math.Floor((double) logoResultHeight / 2) + blackStartPointY;

                    var sKCanvas3 = new SkiaSharp.SKCanvas(sKBitmap);
                    var sKPaint = new SkiaSharp.SKPaint
                    {
                        FilterQuality = SkiaSharp.SKFilterQuality.Medium,
                        IsAntialias = true
                    };
                    sKCanvas3.DrawBitmap(
                        sKBitmapLogo,
                        new SkiaSharp.SKRect
                        {
                            Location = new SkiaSharp.SKPoint {X = 0, Y = 0},
                            Size = new SkiaSharp.SKSize {Height = sKBitmapLogo.Height, Width = sKBitmapLogo.Width}
                        },
                        new SkiaSharp.SKRect
                        {
                            Location = new SkiaSharp.SKPoint {X = pointX, Y = pointY},
                            Size = new SkiaSharp.SKSize {Height = logoResultHeight, Width = logoResultWidth}
                        }, sKPaint);
                    sKCanvas3.Dispose();
                    sKPaint.Dispose();
                    sKBitmapLogo.Dispose();
                }
                else
                {
                    sKBitmapLogo.Dispose();
                }
            }

            #endregion

            SkiaSharp.SKImage sKImage = SkiaSharp.SKImage.FromBitmap(sKBitmap);
            sKBitmap.Dispose();
            var data = sKImage.Encode(format, 75);
            sKImage.Dispose();
            reval = data.ToArray();
            data.Dispose();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return reval;
    }

    /// <summary>
    /// 解析二维码
    /// </summary>
    /// <param name="qrCodeFilePath">二维码文件路径</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string Decoder(string qrCodeFilePath)
    {
        if (!System.IO.File.Exists(qrCodeFilePath))
        {
            throw new Exception("文件不存在");
        }

        System.IO.FileStream fileStream = new System.IO.FileStream(qrCodeFilePath, System.IO.FileMode.Open,
            System.IO.FileAccess.Read, System.IO.FileShare.Read);
        return Decoder(fileStream);
    }

    /// <summary>
    /// 解析二维码
    /// </summary>
    /// <param name="qrCodeBytes"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string Decoder(byte[] qrCodeBytes)
    {
        if (qrCodeBytes == null || qrCodeBytes.Length < 1)
        {
            throw new Exception("参数qrCodeBytes不存在");
        }

        System.IO.MemoryStream ms = new System.IO.MemoryStream(qrCodeBytes);
        return Decoder(ms);
    }

    /// <summary>
    /// 解析二维码
    /// </summary>
    /// <param name="qrCodeStream"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string Decoder(System.IO.Stream qrCodeStream)
    {
        var sKManagedStream = new SkiaSharp.SKManagedStream(qrCodeStream, true);
        var sKBitmap = SkiaSharp.SKBitmap.Decode(sKManagedStream);
        sKManagedStream.Dispose();
        if (sKBitmap.IsEmpty)
        {
            sKBitmap.Dispose();
            throw new Exception("未识别的图片文件");
        }

        var w = sKBitmap.Width;
        var h = sKBitmap.Height;
        int ps = w * h;
        byte[] bytes = new byte[ps * 3];
        int byteIndex = 0;
        for (var x = 0; x < w; x++)
        {
            for (var y = 0; y < h; y++)
            {
                var color = sKBitmap.GetPixel(x, y);
                bytes[byteIndex + 0] = color.Red;
                bytes[byteIndex + 1] = color.Green;
                bytes[byteIndex + 2] = color.Blue;
                byteIndex += 3;
            }
        }

        sKBitmap.Dispose();
        var qRCodeReader = new ZXing.QrCode.QRCodeReader();
        var rGBLuminanceSource = new ZXing.RGBLuminanceSource(bytes, w, h);
        var hybridBinarizer = new ZXing.Common.HybridBinarizer(rGBLuminanceSource);
        var binaryBitmap = new ZXing.BinaryBitmap(hybridBinarizer);
        var hints = new Dictionary<ZXing.DecodeHintType, object>();
        hints.Add(ZXing.DecodeHintType.CHARACTER_SET, "utf-8");
        var result = qRCodeReader.decode(binaryBitmap, hints);
        return result != null ? result.Text : "";
    }

    /// <summary>
    /// 获取图片信息
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Tuple<int, int, long, SkiaSharp.SKEncodedImageFormat> GetImageInfo(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new Exception("路径不能为空");
        }

        if (!System.IO.File.Exists(path))
        {
            throw new Exception("文件不存在");
        }

        var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Open, System.IO.FileAccess.Read,
            System.IO.FileShare.Read); //fileInfo.OpenRead();
        var fileLength = fileStream.Length;
        var sKManagedStream = new SkiaSharp.SKManagedStream(fileStream, true);
        var sKBitmap = SkiaSharp.SKBitmap.Decode(sKManagedStream);
        sKManagedStream.Dispose();

        if (sKBitmap.IsEmpty)
        {
            sKBitmap.Dispose();
            throw new Exception("文件无效");
        }

        int w = sKBitmap.Width;
        int h = sKBitmap.Height;
        return new Tuple<int, int, long, SkiaSharp.SKEncodedImageFormat>(w, h, fileLength, GetImageFormatByPath(path));
    }

    /// <summary>
    /// 获取图片格式
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    private SkiaSharp.SKEncodedImageFormat GetImageFormatByPath(string path)
    {
        var suffix = "";
        if (System.IO.Path.HasExtension(path))
        {
            suffix = System.IO.Path.GetExtension(path);
        }

        return GetImageFormatBySuffix(suffix);
    }

    /// <summary>
    /// 获取图片格式
    /// </summary>
    /// <param name="suffix"></param>
    /// <returns></returns>
    private SkiaSharp.SKEncodedImageFormat GetImageFormatBySuffix(string suffix)
    {
        var format = SkiaSharp.SKEncodedImageFormat.Jpeg;
        if (string.IsNullOrEmpty(suffix))
        {
            return format;
        }

        if (suffix[0] == '.')
        {
            suffix = suffix.Substring(1);
        }

        if (string.IsNullOrEmpty(suffix))
        {
            return format;
        }

        suffix = suffix.ToUpper();
        switch (suffix)
        {
            case "PNG":
                format = SkiaSharp.SKEncodedImageFormat.Png;
                break;
            case "GIF":
                format = SkiaSharp.SKEncodedImageFormat.Gif;
                break;
            case "BMP":
                format = SkiaSharp.SKEncodedImageFormat.Bmp;
                break;
            case "ICON":
                format = SkiaSharp.SKEncodedImageFormat.Ico;
                break;
            case "ICO":
                format = SkiaSharp.SKEncodedImageFormat.Ico;
                break;
            case "DNG":
                format = SkiaSharp.SKEncodedImageFormat.Dng;
                break;
            case "WBMP":
                format = SkiaSharp.SKEncodedImageFormat.Wbmp;
                break;
            case "WEBP":
                format = SkiaSharp.SKEncodedImageFormat.Webp;
                break;
            case "PKM":
                format = SkiaSharp.SKEncodedImageFormat.Pkm;
                break;
            case "KTX":
                format = SkiaSharp.SKEncodedImageFormat.Ktx;
                break;
            case "ASTC":
                format = SkiaSharp.SKEncodedImageFormat.Astc;
                break;
        }
        return format;
    }
    
}


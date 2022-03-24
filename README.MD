# Jc.QRCode

二维码生成模块

```shell
Install-Package Jc.QRCode
```
# 生成二维码

实例化

Jc.QRCode.JcQRCode qr = new JcQRCode();

#### 生成二维码文件
```c#

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
```

#### 返回二维码Byte[]

```c#
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
        int keepWhiteBorderPixelVal = -1
```

# 解析二维码

#### 根据本地文件解析
```c#
    /// <summary>
    /// 解析二维码
    /// </summary>
    /// <param name="qrCodeFilePath">二维码文件路径</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string Decoder(string qrCodeFilePath)
```

#### 根据文件流解析

```c#
    /// <summary>
    /// 解析二维码
    /// </summary>
    /// <param name="qrCodeStream"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public string Decoder(System.IO.Stream qrCodeStream)
```
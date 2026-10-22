using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using SkiaSharp;

namespace VAA.EAPI.Automation.MobileApp.UI.VisualValidation;

public sealed class AppiumComponentScreenshot
{
    public byte[] Capture(AppiumDriver driver, IWebElement component)
        => CaptureRegion(driver, component, 0, 0, component.Size.Width, component.Size.Height);

    public byte[] CaptureRegion(
        AppiumDriver driver,
        IWebElement component,
        int relativeX,
        int relativeY,
        int width,
        int height)
    {
        if (relativeX < 0 || relativeY < 0 || width <= 0 || height <= 0 ||
            relativeX + width > component.Size.Width || relativeY + height > component.Size.Height)
        {
            throw new ArgumentOutOfRangeException(
                nameof(relativeX),
                "The requested visual region must fit within the owning Appium component.");
        }

        Screenshot screenshot = driver.GetScreenshot();
        System.Drawing.Size viewportSize = driver.Manage().Window.Size;
        return Crop(
            screenshot.AsByteArray,
            viewportSize.Width,
            viewportSize.Height,
            component.Location.X + relativeX,
            component.Location.Y + relativeY,
            width,
            height);
    }

    internal byte[] Crop(
        byte[] screenshotPng,
        int viewportWidth,
        int viewportHeight,
        int componentX,
        int componentY,
        int componentWidth,
        int componentHeight)
    {
        if (viewportWidth <= 0 || viewportHeight <= 0 || componentWidth <= 0 || componentHeight <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(viewportWidth),
                "Viewport and component dimensions must be greater than zero.");
        }

        using SKBitmap screenshot = SKBitmap.Decode(screenshotPng)
                                    ?? throw new InvalidDataException("Could not decode the Appium screenshot as PNG.");
        double horizontalScale = (double)screenshot.Width / viewportWidth;
        double verticalScale = (double)screenshot.Height / viewportHeight;
        int left = Math.Clamp((int)Math.Floor(componentX * horizontalScale), 0, screenshot.Width);
        int top = Math.Clamp((int)Math.Floor(componentY * verticalScale), 0, screenshot.Height);
        int right = Math.Clamp(
            (int)Math.Ceiling((componentX + componentWidth) * horizontalScale),
            left,
            screenshot.Width);
        int bottom = Math.Clamp(
            (int)Math.Ceiling((componentY + componentHeight) * verticalScale),
            top,
            screenshot.Height);

        if (right == left || bottom == top)
        {
            throw new InvalidOperationException(
                "The component bounds do not intersect the captured Appium viewport.");
        }

        using SKBitmap cropped = new(new SKImageInfo(right - left, bottom - top));
        using SKCanvas canvas = new(cropped);
        SKRect source = new(left, top, right, bottom);
        SKRect destination = new(0, 0, cropped.Width, cropped.Height);
        canvas.DrawBitmap(screenshot, source, destination, new SKSamplingOptions(SKFilterMode.Nearest));

        using SKImage image = SKImage.FromBitmap(cropped);
        using SKData encodedImage = image.Encode(SKEncodedImageFormat.Png, 100);
        return encodedImage.ToArray();
    }
}

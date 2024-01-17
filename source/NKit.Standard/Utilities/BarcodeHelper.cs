namespace NKit.Utilities
{
    #region Using Directives

    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Text;
    using ZXing;
    using ZXing.Windows.Compatibility;

    #endregion //Using Directives

    public class BarcodeHelper
    {
        #region Methods

        #region Writing Methods

        /// <summary>
        /// Generates a barcode image based with the text being encoded into the barcode.
        /// </summary>
        /// <param name="barcodeText">The text that will be encoded into the barcode.</param>
        /// <param name="barcodeFormat">The symbology of the barcode e.g. Code 128 or QR Code.</param>
        /// <param name="width">The width of the barcode image.</param>
        /// <param name="height">The height of the barcode image.</param>
        /// <param name="margin">Specifies margin, in pixels, to use when generating the barcode. The meaning can vary by format; for example it controls margin before and after the barcodehorizontally for most 1D formats.</param>
        /// <param name="imageBytes">The bytes of the bitmap that has been generated.</param>
        /// <returns></returns>
        public static Bitmap GenerateBarcode(
            string barcodeText,
            BarcodeFormat barcodeFormat,
            int width,
            int height,
            int margin,
            out byte[] imageBytes)
        {
            return GenerateBarcode(barcodeText, barcodeFormat, width, height, margin, null, ImageFormat.Png, out imageBytes);
        }

        /// <summary>
        /// Generates a barcode image based with the text being encoded into the barcode.
        /// </summary>
        /// <param name="barcodeText">The text that will be encoded into the barcode.</param>
        /// <param name="barcodeFormat">The symbology of the barcode e.g. Code 128 or QR Code.</param>
        /// <param name="width">The width of the barcode image.</param>
        /// <param name="height">The height of the barcode image.</param>
        /// <param name="margin">Specifies margin, in pixels, to use when generating the barcode. The meaning can vary by format; for example it controls margin before and after the barcodehorizontally for most 1D formats.</param>
        /// <param name="outputFilePath">The file path where the image should be saved. If this parameter is null the image will not be saved anywhere.</param>
        /// <param name="imageFormat">The format of the image file that will be saved. If the outputFilePath is null the image will not be saved anywhere.</param>
        /// <param name="imageBytes">The bytes of the bitmap that has been generated.</param>
        /// <returns></returns>
        public static Bitmap GenerateBarcode(
            string barcodeText,
            BarcodeFormat barcodeFormat,
            int width,
            int height,
            int margin,
            string outputFilePath,
            ImageFormat imageFormat,
            out byte[] imageBytes)
        {
            BarcodeWriter writer = new BarcodeWriter();
            writer.Format = barcodeFormat;
            writer.Options.Width = width;
            writer.Options.Height = height;
            writer.Options.Margin = margin;
            Bitmap result = writer.Write(barcodeText);
            imageBytes = ImageHandler.GetBytesFromImageUsingImageConverter(result);
            if (string.IsNullOrEmpty(outputFilePath))
            {
                return result;
            }
            if (File.Exists(outputFilePath))
            {
                FileSystemHelper.DeleteFileForce(new FileInfo(outputFilePath), swallowExceptions: false, out string errorMessage);
            }
            result.Save(outputFilePath, imageFormat);
            return result;
        }

        #endregion //Writing Methods

        #region Reading Methods

        /// <summary>
        /// Attempts to read an image containing a barcode and returns the text contents encoded within the barcode.
        /// </summary>
        /// <param name="imageFilePath"></param>
        /// <param name="tryHarder">Gets or sets a flag which cause a deeper look into the bitmap.</param>
        /// <param name="tryInverted">Gets or sets a value indicating whether the image should be automatically inverted if no result is found in the original image. ATTENTION: Please be carefully because it slows down the decoding process if it is used</param>
        /// <param name="barcodeFormat">The symbology of the barcode that was detected when decoding the barcode.</param>
        /// <param name="imageBytes">The raw bytes encoded by the barcode, if applicable, otherwise null.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static string ReadBarcodeImage(string imageFilePath, bool tryHarder, bool tryInverted, out BarcodeFormat barcodeFormat, out byte[] imageBytes)
        {
            if (!ImageHandler.IsFileAnImage(imageFilePath, validFileExtensions: null, validateFileExists: true, loadImageToValidate: true, out string validationError))
            {
                throw new Exception(validationError);
            }
            using (Bitmap bitmap = new Bitmap(imageFilePath))
            {
                BarcodeReader reader = new BarcodeReader();
                reader.Options.TryHarder = tryHarder;
                reader.Options.TryInverted = tryInverted;
                Result decodeResult = reader.Decode(bitmap);
                barcodeFormat = decodeResult.BarcodeFormat;
                imageBytes = decodeResult.RawBytes;
                return decodeResult.Text;
            }
        }

        #endregion //Reading Methods

        #endregion //Methods
    }
}

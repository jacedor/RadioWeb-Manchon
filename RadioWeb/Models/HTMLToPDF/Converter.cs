using Ganss.XSS;
using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using iTextSharp.tool.xml.css;
using iTextSharp.tool.xml.html;
using iTextSharp.tool.xml.parser;
using iTextSharp.tool.xml.pipeline.css;
using iTextSharp.tool.xml.pipeline.end;
using iTextSharp.tool.xml.pipeline.html;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace RadioWeb.Models.HTMLToPDF
{
    public class Converter
    {


        public static byte[] ConsentFromHtml(string htmlPath)
        {
            //htmlPath = @"C:\RadioWeb2.0\RadioWeb\Reports\pdf\templates\consen17371539.html";
            byte[] aPdfReport;
            using (var ms = new MemoryStream())
            {
                using (var doc = new Document(PageSize.A4, 50, 50, 90, 30))
                {
                    using (var writer = PdfWriter.GetInstance(doc, ms))
                    {
                        doc.Open();
                        var tagProcessors = (DefaultTagProcessorFactory)Tags.GetHtmlTagProcessorFactory();
                        tagProcessors.RemoveProcessor(HTML.Tag.IMG); // remove the default processor
                        //Add CustomImageTagProcessor to be able to add images 
                        tagProcessors.AddProcessor(HTML.Tag.IMG, new CustomImageTagProcessor());
                        StreamReader sIn = new StreamReader(htmlPath);
                        string aHtmlReport = sIn.ReadToEnd();
                        sIn.Close();
                        sIn.Dispose();
                        CssFilesImpl cssFiles = new CssFilesImpl();
                        cssFiles.Add(XMLWorkerHelper.GetInstance().GetDefaultCSS());
                        var cssResolver = new StyleAttrCSSResolver(cssFiles);
                        var charset = Encoding.UTF8;
                        var hpc = new HtmlPipelineContext(new CssAppliersImpl(new XMLWorkerFontProvider()));
                        hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(tagProcessors);
                        var htmlPipeline = new HtmlPipeline(hpc, new PdfWriterPipeline(doc, writer));
                        var pipeline = new CssResolverPipeline(cssResolver, htmlPipeline);
                        var worker = new XMLWorker(pipeline, true);
                        try
                        {
                            var xmlParser = new XMLParser(true, worker, charset);
                            xmlParser.Parse(new StringReader(aHtmlReport));
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        finally
                        {
                            doc.Close();
                        }
                    }
                }
                aPdfReport = addHeader(ms.ToArray(), "/img/LogoGrupManchon.jpg", 250, 250);
            }
            return aPdfReport;
        }

        public static byte[] InformeFromHtml2(string htmlPath, string urlImagen, int heightImagen, int widthImage)

        {

            HtmlDocument htmlDoc = new HtmlDocument();
            StreamReader sIn = new StreamReader(htmlPath, System.Text.Encoding.UTF8, false);

            string aHtmlReport = sIn.ReadToEnd();
            aHtmlReport = aHtmlReport.Replace("&nbsp;", ((Char)9).ToString());
            sIn.Close();
            sIn.Dispose();
            htmlDoc.LoadHtml(aHtmlReport);

            htmlDoc.OptionFixNestedTags = true;

            htmlDoc.OptionOutputAsXml = true;

            // htmlDoc.OptionCheckSyntax = true;

            HtmlNode bodyNode = htmlDoc.DocumentNode;

            aHtmlReport = bodyNode.WriteTo();


            //Create a byte array that will hold final PDF
            Byte[] bytes;
            var ms = new MemoryStream();
            var doc = new Document(PageSize.A4, 50, 50, 100, 30);
            var writer = PdfWriter.GetInstance(doc, ms);
            writer.CloseStream = false;
            var srHtml = new StringReader(aHtmlReport);
            //Open the document for writing
            doc.Open();
            //Add support for embeded images
            var tagProcessors = (DefaultTagProcessorFactory)Tags.GetHtmlTagProcessorFactory();
            tagProcessors.RemoveProcessor(HTML.Tag.IMG);
            tagProcessors.AddProcessor(HTML.Tag.IMG, new CustomImageTagProcessor());
            var myfont = FontFactory.GetFont("Verdana", 10f, iTextSharp.text.Font.NORMAL);
            string arialuniTff = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "ARIALUNI.TTF");

            HtmlPipelineContext htmlContext = new HtmlPipelineContext(null);
            htmlContext.SetTagFactory(tagProcessors);
            //ICSSResolver cssResolver = XMLWorkerHelper.GetInstance().GetDefaultCssResolver(true);
            var cssResolver = new StyleAttrCSSResolver();
            var cssFile = XMLWorkerHelper.GetCSS(new FileStream(HttpContext.Current.Server.MapPath("~/pdf.css"), FileMode.Open));
            cssResolver.AddCss(cssFile);
            IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext,
                new PdfWriterPipeline(doc, writer)));



            XMLWorker worker = new XMLWorker(pipeline, true);
            XMLParser xmlParser = new XMLParser(true, worker, Encoding.UTF8);


            // xmlParser.Parse(new MemoryStream(Encoding.UTF8.GetBytes(aHtmlReport)));
            //   string result = Encoding.UTF8.GetString( bad_string.Select( c => (byte)c ).ToArray() );
            xmlParser.Parse(srHtml);
            srHtml.Close();
            doc.Close();
            writer.Close();
            doc.Close();
            bytes = ms.ToArray();
            ms.Close();

            if (urlImagen != "")
            {
                bytes = addHeader(ms.ToArray(), urlImagen, heightImagen, widthImage);
                //string urlImagenFooter = "/IMG/LOGO-NINO-PIE.png"; // Ruta de la imagen a utilizar
                //int widthImagenFooter = 595; // Ancho de la página en puntos (A4 tiene 595 puntos de ancho)
                //bytes = addFooter(bytes, urlImagenFooter, widthImagenFooter);


            }

            return bytes;
        }
        public static byte[] InformeFromHtml(string htmlPath, string urlImagen, int heightImagen, int widthImage, bool horizontal=false)
        {
  
            //htmlPath = @"C:\RadioWeb2.0\RadioWeb\Reports\pdf\templates\consen17371539.html";
            byte[] aPdfReport;
            int marginleft = 50;
            int marginRight = 50;
            int marginTop = 130;
            int marginBottom = 30;
            if (horizontal)
            {
                marginTop = 50;
                marginleft = 30;
                marginRight = 30;
                marginBottom = 15;
            }
            using (var ms = new MemoryStream())
            {
                using (var doc = new Document(PageSize.A4, marginleft, marginRight, marginTop, marginBottom))
                {
                    if (horizontal)
                    {
                        doc.SetPageSize(iTextSharp.text.PageSize.A4.Rotate());
                    }
                    using (var writer = PdfWriter.GetInstance(doc, ms))
                    {
                        doc.Open();
                        var tagProcessors = (DefaultTagProcessorFactory)Tags.GetHtmlTagProcessorFactory();
                        tagProcessors.RemoveProcessor(HTML.Tag.IMG); // remove the default processor
                        //Add CustomImageTagProcessor to be able to add images 
                        tagProcessors.AddProcessor(HTML.Tag.IMG, new CustomImageTagProcessor());
                        StreamReader sIn = new StreamReader(htmlPath);
                        string aHtmlReport = sIn.ReadToEnd();
                      
                        sIn.Close();
                        sIn.Dispose();
                        CssFilesImpl cssFiles = new CssFilesImpl();
                        cssFiles.Add(XMLWorkerHelper.GetInstance().GetDefaultCSS());
                        var cssResolver = new StyleAttrCSSResolver(cssFiles);
                        var charset = Encoding.UTF8;
                        var hpc = new HtmlPipelineContext(new CssAppliersImpl(new XMLWorkerFontProvider()));
                        hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(tagProcessors);
                        var htmlPipeline = new HtmlPipeline(hpc, new PdfWriterPipeline(doc, writer));
                        var pipeline = new CssResolverPipeline(cssResolver, htmlPipeline);
                        var worker = new XMLWorker(pipeline, true);
                        try
                        {
                            var xmlParser = new XMLParser(true, worker, charset);
                            xmlParser.Parse(new StringReader(aHtmlReport));
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        finally
                        {
                            doc.Close();
                        }
                    }
                }
                if (horizontal)
                {
                    aPdfReport = addHeaderHorizontalPdf(ms.ToArray(), urlImagen, widthImage,heightImagen );

                }
                else {
                    aPdfReport = addHeader(ms.ToArray(), urlImagen, heightImagen, widthImage);

                }
            }
            return aPdfReport;
        }

        public static byte[] InformeFromHtml(string htmlPath, bool logoIncrustado=false)
        {
            //htmlPath = @"C:\RadioWeb2.0\RadioWeb\Reports\pdf\templates\consen17371539.html";
            byte[] aPdfReport;
            using (var ms = new MemoryStream())
            {
                using (var doc = new Document(PageSize.A4, 50, 50, 40, 30))
                {
                    using (var writer = PdfWriter.GetInstance(doc, ms))
                    {
                        doc.Open();
                        var tagProcessors = (DefaultTagProcessorFactory)Tags.GetHtmlTagProcessorFactory();
                        tagProcessors.RemoveProcessor(HTML.Tag.IMG); // remove the default processor
                        //Add CustomImageTagProcessor to be able to add images 
                        tagProcessors.AddProcessor(HTML.Tag.IMG, new CustomImageTagProcessor());
                        StreamReader sIn = new StreamReader(htmlPath);
                        string aHtmlReport = sIn.ReadToEnd();
                        sIn.Close();
                        sIn.Dispose();
                        CssFilesImpl cssFiles = new CssFilesImpl();
                        cssFiles.Add(XMLWorkerHelper.GetInstance().GetDefaultCSS());
                        var cssResolver = new StyleAttrCSSResolver(cssFiles);
                        var charset = Encoding.UTF8;
                        var hpc = new HtmlPipelineContext(new CssAppliersImpl(new XMLWorkerFontProvider()));
                        hpc.SetAcceptUnknown(true).AutoBookmark(true).SetTagFactory(tagProcessors);
                        var htmlPipeline = new HtmlPipeline(hpc, new PdfWriterPipeline(doc, writer));
                        var pipeline = new CssResolverPipeline(cssResolver, htmlPipeline);
                        var worker = new XMLWorker(pipeline, true);
                        try
                        {
                            var xmlParser = new XMLParser(true, worker, charset);
                            xmlParser.Parse(new StringReader(aHtmlReport));
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                        finally
                        {
                            doc.Close();
                        }
                    }
                }
                if (!logoIncrustado)
                {
                    aPdfReport = addHeaderInformeCDPI(ms.ToArray(), 250, 250);
                }
                else {
                    aPdfReport = ms.ToArray();
                }
                
            }
            return aPdfReport;
        }

        private static byte[] addHeaderInformeCDPI(byte[] aPDFIn, int sizeX, int sizeY)
        {
            PdfReader aPdfReader = new PdfReader(aPDFIn);
            using (var aPDFOut = new MemoryStream())
            {
                using (PdfStamper aPdfStamper = new PdfStamper(aPdfReader, aPDFOut))
                {
                    int pages = aPdfReader.NumberOfPages;
                    for (int i = 1; i <= pages; i++)
                    {
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(Convert.FromBase64String("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"));
                        img.ScaleToFit(sizeX, sizeY);
                        img.SetAbsolutePosition(50, 750);
                        aPdfStamper.GetOverContent(i).AddImage(img);
                    }
                }
                aPdfReader.Close();
                return aPDFOut.ToArray();
            }
        }

        private static byte[] addHeaderHorizontalPdf(byte[] aPDFIn, string url_imagen, int sizeX, int sizeY)
        {
            PdfReader aPdfReader = new PdfReader(aPDFIn);
            using (var aPDFOut = new MemoryStream())
            {
                using (PdfStamper aPdfStamper = new PdfStamper(aPdfReader, aPDFOut))
                {
                    int pages = aPdfReader.NumberOfPages;
                    for (int i = 1; i <= pages; i++)
                    {
                        string imageURL = System.Web.HttpContext.Current.Server.MapPath(url_imagen);
                        // string imageURL =url_imagen;
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imageURL);
                        img.ScaleToFit(sizeX, sizeY);
                        img.Border = 1;
                        img.SetAbsolutePosition(0, 1440);
                        aPdfStamper.GetOverContent(i).AddImage(img);
                    }
                }
                aPdfReader.Close();
                return aPDFOut.ToArray();
            }
        }

        private static byte[] addHeader(byte[] aPDFIn, string url_imagen, int sizeX, int sizeY)
        {
            PdfReader aPdfReader = new PdfReader(aPDFIn);
            using (var aPDFOut = new MemoryStream())
            {
                using (PdfStamper aPdfStamper = new PdfStamper(aPdfReader, aPDFOut))
                {
                    int pages = aPdfReader.NumberOfPages;
                    for (int i = 1; i <= pages; i++)
                    {
                        string imageURL = System.Web.HttpContext.Current.Server.MapPath(url_imagen);
                       // string imageURL =url_imagen;
                        iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(imageURL);
                        img.ScaleToFit(sizeX, sizeY);
                        img.Border = 1;              
                        img.SetAbsolutePosition(48, 720);
                        aPdfStamper.GetOverContent(i).AddImage(img);
                    }
                }
                aPdfReader.Close();
                return aPDFOut.ToArray();
            }
        }

        public static void InsertBarCodeToPdf(string sourceFileName,  string newFileName, string barcode)
        {
            using (Stream pdfStream = new FileStream(sourceFileName, FileMode.Open))           
            using (Stream newpdfStream = new FileStream(newFileName, FileMode.Create, FileAccess.ReadWrite))
            {
                PdfReader pdfReader = new PdfReader(pdfStream);
                PdfStamper pdfStamper = new PdfStamper(pdfReader, newpdfStream);
                PdfContentByte pdfContentByte = pdfStamper.GetOverContent(1);
                Barcode39 bc39 = new Barcode39();
                bc39.BarHeight = 28f;
                bc39.Code = barcode;
                iTextSharp.text.Image image = bc39.CreateImageWithBarcode(pdfContentByte, null, null);// iTextSharp.text.Image.GetInstance(imageStream);
                image.SetAbsolutePosition(440, 681);
                pdfContentByte.AddImage(image);
                pdfStamper.Close();
            }
        }     
      

        // Write a single line to the PDF.
        private static void WriteLineToPdf(string tLine, PdfPTable tTableIn, out PdfPTable tTableOut)
        {
            BaseFont bfTimes = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, false);
            iTextSharp.text.Font timesN = new iTextSharp.text.Font(bfTimes, 12, iTextSharp.text.Font.NORMAL, BaseColor.RED);

            Paragraph par = new Paragraph(tLine, timesN);
            par.Alignment = Element.ALIGN_CENTER;
            PdfPCell cell = new PdfPCell();
            cell.AddElement(par);
            tTableIn.AddCell(cell);
            tTableOut = tTableIn;
        }


        public static void addPassword(string sourcePdf, string destPdf, string Password)
        {
            using (Stream input = new FileStream(sourcePdf, FileMode.Open, FileAccess.Read, FileShare.Read))
            //Passowrd the pwd for PDF security                 
            {
                using (Stream output = new FileStream(destPdf, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    PdfReader reader = new PdfReader(input);
                    PdfEncryptor.Encrypt(reader, output, true, Password, Password, PdfWriter.ALLOW_PRINTING);
                }
            }
        }
    }
}
using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
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
using System.Web.Mvc;

namespace RadioWeb.Models.HTMLToPDF
{
    public class Converter2
    {

        public class UnicodeFontFactory : FontFactoryImp
        {
            private static readonly string FontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts),
          "arialuni.ttf");

            private readonly BaseFont _baseFont;

            public UnicodeFontFactory()
            {
                _baseFont = BaseFont.CreateFont(FontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);

            }

            public override Font GetFont(string fontname, string encoding, bool embedded, float size, int style, BaseColor color,
          bool cached)
            {
                return new Font(_baseFont, size, style, color);
            }
        }

     
        private static void createPDF(string html)
        {
            //MemoryStream msOutput = new MemoryStream();
            TextReader reader = new StringReader(html);// step 1: creation of a document-object
            Document document = new Document(PageSize.A4, 30, 30, 30, 30);

            // step 2:
            // we create a writer that listens to the document
            // and directs a XML-stream to a file
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream("Test.pdf", FileMode.Create));

            // step 3: we create a worker parse the document
            HTMLWorker worker = new HTMLWorker(document);

            // step 4: we open document and start the worker on the document
            document.Open();

            // step 4.1: register a unicode font and assign it an allias
            FontFactory.Register("C:\\Windows\\Fonts\\ARIALUNI.TTF", "arial unicode ms");

            // step 4.2: create a style sheet and set the encoding to Identity-H
            iTextSharp.text.html.simpleparser.StyleSheet ST = new StyleSheet();
            ST.LoadTagStyle("body", "encoding", "Identity-H");

            // step 4.3: assign the style sheet to the html parser
            worker.SetStyleSheet(ST);

            worker.StartDocument();

            // step 5: parse the html into the document
            worker.Parse(reader);

            // step 6: close the document and the worker
            worker.EndDocument();
            worker.Close();
            document.Close();
        }

        public static void InformeFromHtml1(string htmlPath, string urlImagen, int heightImagen, int widthImage)

        {

            HtmlDocument htmlDoc = new HtmlDocument();
            StreamReader sIn = new StreamReader(htmlPath, System.Text.Encoding.UTF8, false);

            string aHtmlReport = sIn.ReadToEnd();
            aHtmlReport = aHtmlReport.Replace("&nbsp;", ((Char)9).ToString());
            sIn.Close();
            sIn.Dispose();
            htmlDoc.LoadHtml(aHtmlReport);

            //Convert HTML to well-formed XHTML

            htmlDoc.OptionFixNestedTags = true;

            htmlDoc.OptionOutputAsXml = true;

            htmlDoc.OptionCheckSyntax = true;

            HtmlNode bodyNode = htmlDoc.DocumentNode;
           
            aHtmlReport = bodyNode.InnerHtml;


            //Create a byte array that will hold final PDF
            Byte[] bytes;


            createPDF(aHtmlReport);



         

        }



        public static byte[] InformeFromHtml2(string htmlContent, string urlImagen, int heightImagen, int widthImage)

        {

            HtmlDocument htmlDoc = new HtmlDocument();
          

            string aHtmlReport = htmlContent;
            aHtmlReport = aHtmlReport.Replace("&nbsp;", ((Char)9).ToString());
            
            htmlDoc.LoadHtml(aHtmlReport);

            htmlDoc.OptionFixNestedTags = true;

            htmlDoc.OptionOutputAsXml = true;

             htmlDoc.OptionCheckSyntax = true;

            HtmlNode bodyNode = htmlDoc.DocumentNode;

            aHtmlReport = bodyNode.WriteTo();


            //Create a byte array that will hold final PDF
            Byte[] bytes;
            var ms = new MemoryStream();
            var doc = new Document(PageSize.A4, 50, 50, 130, 30);
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

            bytes = addHeader(ms.ToArray(), urlImagen, heightImagen, widthImage);



            return bytes;

        }
        public static byte[] InformeFromHtml(string htmlPath, string urlImagen, int heightImagen, int widthImage)

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
            var doc = new Document(PageSize.A4, 50, 50, 130, 30);
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

            //var cssResolver = new StyleAttrCSSResolver();
            ////Change the path to your CSS file
            //var cssFile = XMLWorkerHelper.GetCSS(new FileStream(HttpContext.Current.Server.MapPath("~/pdf.css"), FileMode.Open));
            //cssResolver.AddCss(cssFile);
            //IPipeline pipeline = new CssResolverPipeline(cssResolver, new HtmlPipeline(htmlContext,
            //    new PdfWriterPipeline(doc, writer)));

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

            bytes = addHeader(ms.ToArray(), urlImagen, heightImagen, widthImage);



            return bytes;

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

        public class CustomImageTagProcessor : iTextSharp.tool.xml.html.Image

        {

            public override IList<IElement> End(IWorkerContext ctx, Tag tag, IList<IElement> currentContent)

            {

                IDictionary<string, string> attributes = tag.Attributes;

                string src;

                if (!attributes.TryGetValue(HTML.Attribute.SRC, out src))

                    return new List<IElement>(1);


                if (string.IsNullOrEmpty(src))

                    return new List<IElement>(1);


                if (src.StartsWith("data:image/", StringComparison.InvariantCultureIgnoreCase))

                {

                    // data:[<MIME-type>][;charset=<encoding>][;base64],<data>

                    var base64Data = src.Substring(src.IndexOf(",") + 1);

                    var imagedata = Convert.FromBase64String(base64Data);

                    var image = iTextSharp.text.Image.GetInstance(imagedata);


                    var list = new List<IElement>();

                    var htmlPipelineContext = GetHtmlPipelineContext(ctx);

                    list.Add(GetCssAppliers().Apply(new Chunk((iTextSharp.text.Image)GetCssAppliers().Apply(image, tag, htmlPipelineContext), 0, 0, true), tag, htmlPipelineContext));

                    return list;

                }

                else

                {

                    return base.End(ctx, tag, currentContent);

                }

            }

        }

    }
}

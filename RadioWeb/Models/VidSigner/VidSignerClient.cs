using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RestSharp;
using System.Diagnostics;
using RadioWeb.Models.Repos;
using SelectPdf;
using System.Web;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Text.RegularExpressions;
using System.Web.UI;
using iTextSharp.text.html.simpleparser;
using Newtonsoft.Json;
using RadioWeb.ViewModels;
using ADPM.Common;
using System.Web.Routing;
using RadioWeb.Controllers;
using RadioWeb.Utils;
using System.Net;

namespace RadioWeb.Models.VidSigner
{
    public class VidSignerClient
    {


        private RestClient ClienteRest { get; set; }
        public string DocUID { get; set; }
        public string ErrorMessage { get; set; }
        private string endPoint = "";// System.Configuration.ConfigurationManager.AppSettings["EndPointVidSigner"];

      
        private static FormTitleDTO getTitle(String sText, FormPositionDTO aPositon)
        {
            FormTitleDTO aFormTitleDTO = new FormTitleDTO();
            aFormTitleDTO.Text = sText;
            aFormTitleDTO.Position = aPositon;
            return aFormTitleDTO;
        }

        private static FormPositionDTO getPosition(int iSizeX, int iPage, string sAnchor)//
        {
            FormPositionDTO aFormPositionDTO = new FormPositionDTO();
            aFormPositionDTO.SizeX = iSizeX;
            //aFormPositionDTO.PosX = iPosX;
            //aFormPositionDTO.PosY = iPosY;
            //aFormPositionDTO.Page = iPage;
            aFormPositionDTO.Anchor = sAnchor;
            return aFormPositionDTO;
        }

        private static FormPositionDTO getPositionRadioButton(int iSizeX, int iPosX, int iPosY, int iPage, string sAnchor)//
        {
            FormPositionDTO aFormPositionDTO = new FormPositionDTO();
            aFormPositionDTO.SizeX = iSizeX;
            aFormPositionDTO.PosX = iPosX;
            aFormPositionDTO.PosY = iPosY;
            aFormPositionDTO.Page = iPage;
            aFormPositionDTO.Anchor = sAnchor;
            return aFormPositionDTO;
        }

        private static FormCheckBoxDTO getCheckBox(String sId, FormTitleDTO aTitle, FormConditionDTO aFormConditionDTO)
        {
            FormCheckBoxDTO aFormCheckBoxDTO = new FormCheckBoxDTO();
            aFormCheckBoxDTO.Id = sId;
            aFormCheckBoxDTO.Title = aTitle;
            aFormCheckBoxDTO.Condition = aFormConditionDTO;
            return aFormCheckBoxDTO;
        }

        private static FormConditionDTO getConditions(String sId, int iResult)
        {
            FormConditionDTO aFormTextBoxConditionDTO = new FormConditionDTO();
            aFormTextBoxConditionDTO.Id = sId;
            aFormTextBoxConditionDTO.Result = iResult;
            return aFormTextBoxConditionDTO;
        }

        private static FormTextBoxDTO getTextBox(String sId, FormTitleDTO aTitle, FormConditionDTO aCondition, FormTitleDTO aResponse, int iMaxLines)
        {
            FormTextBoxDTO aFormTextBoxDTO = new FormTextBoxDTO();
            aFormTextBoxDTO.Id = sId;
            aFormTextBoxDTO.Title = aTitle;
            aFormTextBoxDTO.Condition = aCondition;
            aFormTextBoxDTO.Response = aResponse;
            aFormTextBoxDTO.MaxLines = iMaxLines;
            return aFormTextBoxDTO;
        }


        private static FormRadioButtonDTO getRadioButton(String sId, FormTitleDTO aTitle, List<FormRadioButtonChoiceDTO> aChoices, FormConditionDTO aFormConditionDTO)
        {
            FormRadioButtonDTO aFormRadioButtonDTO = new FormRadioButtonDTO();
            aFormRadioButtonDTO.Id = sId;
            aFormRadioButtonDTO.Title = aTitle;
            aFormRadioButtonDTO.Choices = aChoices;
            aFormRadioButtonDTO.Condition = aFormConditionDTO;
            return aFormRadioButtonDTO;
        }

        private static FormRadioButtonChoiceDTO getRadioButton(FormTitleDTO aTitle)
        {
            FormRadioButtonChoiceDTO aFormRadioButton = new FormRadioButtonChoiceDTO();
            aFormRadioButton.Title = aTitle;
            return aFormRadioButton;
        }

        public string GenerarPDFiText(bool conCabecera, int oidPlantilla, int oidExploracion, string observaciones = "")
        {
            string textoPlantilla = P_InformesRepositorio.ObtenerHtmlDelInforme(oidPlantilla, true);
            RadioDBContext db = new RadioDBContext();
            P_INFORMES oPlantilla = db.P_Informes.Single(p => p.OID == oidPlantilla);
            string grupo = oPlantilla.OWNER.ToString();

            if (oPlantilla.TITULO.Contains("LOPD"))
            {
                grupo = "LOPD";
            }
            if (oPlantilla.TITULO.Contains("JUSTIFICANTE"))
            {
                grupo = "JUSTI";
            }

            //CAMBIADO NOVIEMBRE 2022 NUEVA FUNCIONALIDAD SOLICITADA POR MASSANA
            if (oPlantilla.TITULO.Contains("FICHA") || oPlantilla.TITULO.Contains("FITXA"))
            {
                grupo = "FICHA";
            }
            EXPLORACION oExploracion2 = ExploracionRepositorio.Obtener(oidExploracion);

            //En el nombre del fichero ponemos el OID de la exploración y el ior paciente para bajar luego los documentos
            string rutaInformeDefi = System.Configuration.ConfigurationManager.AppSettings["RutaCONS"] + @"\" +
                oExploracion2.PACIENTE.PACIENTE1.Replace(" ", "") + "_" + oExploracion2.OID + "_"
                + oExploracion2.IOR_PACIENTE + "_" + grupo + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";

            if (System.IO.File.Exists(rutaInformeDefi))
            {
                System.IO.File.Delete(rutaInformeDefi);
            }
            string htmlText = textoPlantilla;
            htmlText = htmlText.Replace("Times New Roman", "Arial");
            htmlText = htmlText.Replace("@PA", oExploracion2.PACIENTE.TRATAMIENTODESC.ToUpper() + " " + oExploracion2.PACIENTE.PACIENTE1);

            //Cambios Massana PacienteResponsable
            if (!String.IsNullOrEmpty(oExploracion2.PACIENTE.RESPONSABLE))
            {
                htmlText = htmlText.Replace("@RESPONSABLE", oExploracion2.PACIENTE.RESPONSABLE.ToUpper());
            }
            else
            {
                htmlText = htmlText.Replace("@RESPONSABLE", "");
            }

            if (!String.IsNullOrEmpty(oExploracion2.PACIENTE.DNIRESPONSABLE))
            {
                htmlText = htmlText.Replace("@DNIRESPONSABLE", oExploracion2.PACIENTE.DNIRESPONSABLE.ToUpper());
            }
            else
            {
                htmlText = htmlText.Replace("@DNIRESPONSABLE", "");
            }

            htmlText = htmlText.Replace("@DNI", oExploracion2.PACIENTE.DNI.ToUpper());
            htmlText = htmlText.Replace("@MAIL", oExploracion2.PACIENTE.EMAIL.ToUpper());
            //htmlText = htmlText.Replace("@CENTRO", oExploracion2.EMPRESA.ToUpper());


            htmlText = htmlText.Replace("@DESFIL", oExploracion2.APARATO.DES_FIL.ToUpper());
            htmlText = htmlText.Replace("@CENTRO", CentrosRepositorio.Obtener(oExploracion2.OWNER.Value).DESCRIPCIO);
            htmlText = htmlText.Replace("@DIR", CentrosRepositorio.Obtener(oExploracion2.OWNER.Value).DIRECCION);

            htmlText = htmlText.Replace("@EMAIL", oExploracion2.PACIENTE.EMAIL);
            htmlText = htmlText.Replace("@OBSERVACIONES", observaciones);
            foreach (TELEFONO item in oExploracion2.PACIENTE.TELEFONOS)
            {
                if (item.NUMERO.StartsWith("6"))
                {
                    htmlText = htmlText.Replace("@MOVIL", item.NUMERO);
                }
            }

            htmlText = htmlText.Replace("@FEEX", oExploracion2.FECHA.Value.ToShortDateString());
            htmlText = htmlText.Replace("@FE", DateTime.Now.ToShortDateString());
            htmlText = htmlText.Replace("@HORA", oExploracion2.HORA_LL);
            htmlText = htmlText.Replace("@HORASALIDA", DateTime.Now.ToString("HH:mm"));
            htmlText = htmlText.Replace("&#9;", "");

            
            //cambiamos el color de las macros a blanco para que no se vean en el documento
            string color = "black";
            if (oPlantilla.CID == 1)
            {
                color = "white";
            }
            for (int i = 0; i < 40; i++)
            {

                htmlText = htmlText.Replace("<span class=\"st" + i.ToString() + "\">M_", "<span style=\"color: " + color + "\" class=\"st" + i.ToString() + "\">M_");
                htmlText = htmlText.Replace("<span class=\"st" + i.ToString() + "\">R_", "<span style=\"color: " + color + "\" class=\"st" + i.ToString() + "\">R_");
            }

            htmlText = htmlText.Replace("XXXptXXX", "pt");
            htmlText = htmlText.Replace("<body>", "<body><div style =\"height:80px;display:block;\" ></div>");
            htmlText = htmlText.Replace("<br><a href=\"http://www.sautin.com/order.php#RTOHNET\"></a>", "");
            htmlText = htmlText.Replace("charset=UTF-8\">", "charset=UTF-8\"/>");
            htmlText = htmlText.Replace(".dll\">", ".dll\"/>");

            string htmlString = htmlText;
            string rutaHtmlTemp = HttpContext.Current.Server.MapPath("~/Reports/pdf/templates/consen" + oidExploracion.ToString() + ".html");
            if (File.Exists(rutaHtmlTemp))
            {
                File.Delete(rutaHtmlTemp);
            }
            StreamWriter Swr = new StreamWriter(rutaHtmlTemp);
            Swr.Write(htmlString);
            Swr.Close();
            Swr.Dispose();


            CENTROS oCentro = db.Centros.Single(c => c.OID == oExploracion2.OWNER);

            byte[] aPDF = Models.HTMLToPDF.Converter.InformeFromHtml(rutaHtmlTemp,
                oCentro.LOGO_URL,
                oCentro.LOGO_HEIGHT,
                oCentro.LOGO_WIDTH);


            if (File.Exists(rutaHtmlTemp))
            {
                File.Delete(rutaHtmlTemp);
            }
            File.WriteAllBytes(rutaInformeDefi, aPDF);

            return rutaInformeDefi;

        }


        private string GenerarPDF(bool conCabecera, int oidPlantilla, int oidExploracion)
        {
            // instantiate a html to pdf converter object
            HtmlToPdf converter = new HtmlToPdf();
            string textoPlantilla = P_InformesRepositorio.ObtenerHtmlDelInforme(oidPlantilla, true);

            EXPLORACION oExploracion2 = ExploracionRepositorio.Obtener(oidExploracion);


            string rutaInformeDefi = System.Configuration.ConfigurationManager.AppSettings["RutaCONS"] + @"\" + oExploracion2.PACIENTE.PACIENTE1.Replace(" ", "") + "_" + oExploracion2.OID + "_" + +oExploracion2.IOR_PACIENTE + ".pdf";
            if (System.IO.File.Exists(rutaInformeDefi))
            {
                System.IO.File.Delete(rutaInformeDefi);
            }
            string rutaFicheroCabeceraTemp = "";
            if (conCabecera)
            {
                string headerUrl = HttpContext.Current.Server.MapPath("~/Reports/pdf/templates/CabeceraConsenLogo_CDPI.html");
                string html = System.IO.File.ReadAllText(headerUrl);

                html = html.Replace("@PA", oExploracion2.PACIENTE.TRATAMIENTODESC.ToUpper() + " " + oExploracion2.PACIENTE.PACIENTE1);
                html = html.Replace("@DNI", oExploracion2.PACIENTE.DNI.ToUpper());
                html = html.Replace("@FE", DateTime.Now.ToShortDateString());

                rutaFicheroCabeceraTemp = HttpContext.Current.Server.MapPath("~/Reports/pdf/templates/cabecera_" + oidExploracion.ToString() + ".html");
                System.IO.File.WriteAllText(rutaFicheroCabeceraTemp, html);
                PdfHtmlSection headerHtml = new PdfHtmlSection(rutaFicheroCabeceraTemp);
                headerHtml.AutoFitHeight = HtmlToPdfPageFitMode.AutoFit;

                // header settings
                converter.Options.DisplayHeader = true;
                converter.Header.DisplayOnFirstPage = true;
                converter.Header.DisplayOnOddPages = true;
                converter.Header.DisplayOnEvenPages = true;
                converter.Header.Height = 100;
                converter.Header.Add(headerHtml);

            }
            string htmlText = textoPlantilla;
            htmlText = htmlText.Replace("Times New Roman", "Arial");
            htmlText = htmlText.Replace("@PA", oExploracion2.PACIENTE.TRATAMIENTODESC.ToUpper() + " " + oExploracion2.PACIENTE.PACIENTE1);
            htmlText = htmlText.Replace("@DNI", oExploracion2.PACIENTE.DNI.ToUpper());
            htmlText = htmlText.Replace("@FE", DateTime.Now.ToShortDateString());
            for (int i = 0; i < 40; i++)
            {
                htmlText = htmlText.Replace("font-size:" + i.ToString() + "pt;", "font-size:" + (i + 5).ToString() + "XXXptXXX;");
                htmlText = htmlText.Replace("<span class=\"st" + i.ToString() + "\">M_", "<span style=\"color: white\" class=\"st" + i.ToString() + "\">M_");
                htmlText = htmlText.Replace("<span class=\"st" + i.ToString() + "\">R", "<span style=\"color: white\" class=\"st" + i.ToString() + "\">R");

            }


            htmlText = htmlText.Replace("XXXptXXX", "pt");
            htmlText = htmlText.Replace("<span class=\"st2\">M_", "<span style=\"color: white\" class=\"st2\">M_");
            htmlText = htmlText.Replace("<span class=\"st2\">R_", "<span style=\"color: white\" class=\"st2\">R_");

            // read parameters from the webpage
            string htmlString = htmlText;


            string pdf_page_size = "A4";
            PdfPageSize pageSize = (PdfPageSize)Enum.Parse(typeof(PdfPageSize), pdf_page_size, true);

            string pdf_orientation = "Portrait";
            PdfPageOrientation pdfOrientation = (PdfPageOrientation)Enum.Parse(typeof(PdfPageOrientation), pdf_orientation, true);
            int webPageWidth = 1024;
            try
            {
                webPageWidth = Convert.ToInt32("1024");
            }
            catch { }

            int webPageHeight = 0;
            // set converter options
            converter.Options.EmbedFonts = true;
            converter.Options.PdfPageSize = pageSize;
            converter.Options.PdfPageOrientation = pdfOrientation;
            converter.Options.WebPageWidth = webPageWidth;
            converter.Options.WebPageHeight = webPageHeight;
            converter.Options.MarginLeft = 50;
            converter.Options.MarginRight = 30;
            converter.Options.MinPageLoadTime = 2;


            // create a new pdf document converting an url
            SelectPdf.PdfDocument doc = converter.ConvertHtmlString(htmlString);

            // save pdf document
            doc.Save(rutaInformeDefi);
            // close pdf document
            doc.Close();
            //if (conCabecera)
            //{
            //    System.IO.File.Delete(rutaFicheroCabeceraTemp);
            //}

            return rutaInformeDefi;

        }


        private static Form createForm(int oidPlantilla, string exploracion)
        {
            RadioDBContext db = new RadioDBContext();
            List<VID_PREGUNTAS> oMacros = db.Vid_Preguntas.Where(p => p.OWNER == oidPlantilla && p.BORRADO == null).OrderBy(p => p.OID).ToList();
            if (oMacros.Count == 0)
            {
                return null;
            }

            Form aForm = new Form();
            aForm.Layout = new FormLayout();
            aForm.Layout.FontFamily = "Times-Roman";
            aForm.Layout.FontSize = "11";
            aForm.Sections = new List<FormSection>();
            List<VID_PREGUNTAS> oPreguntasSeccion;
            for (int i = 0; i < 3; i++)
            {
                oPreguntasSeccion = oMacros.Where(p => p.VERS.Value == i).ToList();
                if (oPreguntasSeccion.Count > 0)
                {

                    aForm.Sections.Add(new FormSection());
                    aForm.Sections[i] = new FormSection();
                    aForm.Sections[i].Groups = new List<FormGroup>();

                    aForm.Sections[i].Groups.Add(new FormGroup());
                    aForm.Sections[i].Groups[0] = new FormGroup();
                    aForm.Sections[i].Groups[0].Title = getTitle(exploracion, getPosition(100, 1, "M_TITULO"));
                    aForm.Sections[i].Groups[0].CheckBoxes = new List<FormCheckBoxDTO>();

                    foreach (var macro in oPreguntasSeccion)
                    {

                        //El tipo uno son los checkboxs
                        if (macro.TIPO == 1)
                        {

                            aForm.Sections[i].Groups[0].CheckBoxes.Add(getCheckBox(macro.OID.ToString(), getTitle(macro.NOMBRE,
                                getPosition(100, 1, macro.MACRO)), null));
                        }
                        //Campos de texto
                        if (macro.TIPO == 2)
                        {
                            //Si un campo del formulario es tipo texto, necesitamos saber por un lado la macro anterior que condiciona la respuesta
                            //y por otro el documento tiene que tener una macro que empieze por R y el nombre de la macro
                            if (aForm.Sections[i].Groups[0].TextBoxes == null)
                            {
                                aForm.Sections[i].Groups[0].TextBoxes = new List<FormTextBoxDTO>();
                            }
                            if (macro.MACROCONDICION> -1)
                            {
                                aForm.Sections[i].Groups[0].TextBoxes.Add(getTextBox(macro.OID.ToString(), getTitle(macro.NOMBRE,
                                                                getPosition(100, 1, macro.MACRO)),
                                                                getConditions(macro.MACROCONDICION.ToString(), 1), getTitle("", getPosition(100, 1, macro.MACRORESPUESTA)), 1));
                            }
                            else
                            {
                                aForm.Sections[i].Groups[0].TextBoxes.Add(getTextBox(macro.OID.ToString(), getTitle(macro.NOMBRE,
                                                              getPosition(100, 1, macro.MACRO)),
                                                             null, getTitle("", getPosition(100, 1, macro.MACRORESPUESTA)), 1));
                            }
                            
                        }

                        if(macro.TIPO == 3)
                        {

                            aForm.Sections[i].Groups[0].RadioButtons = new List<FormRadioButtonDTO>();
                            FormRadioButtonDTO oChoice = new FormRadioButtonDTO();
                            oChoice.Title = getTitle(macro.NOMBRE, getPositionRadioButton(100, 220, 195, 2, macro.MACRO));
                            oChoice.Id = macro.OID.ToString();
                            oChoice.Choices = new List<FormRadioButtonChoiceDTO>();

                            FormRadioButtonChoiceDTO oChoiceYes = new FormRadioButtonChoiceDTO
                            {
                                Title = getTitle("Si", getPosition(100, 2, "R_" + macro.MACRO.Substring(2) + "_SI"))
                            };

                            FormRadioButtonChoiceDTO oChoiceNo = new FormRadioButtonChoiceDTO
                            {
                                Title = getTitle("No", getPosition(100, 2, "R_" + macro.MACRO.Substring(2) + "_NO"))
                            };

                            FormRadioButtonChoiceDTO oChoiceNoEstoySegura = new FormRadioButtonChoiceDTO
                            {
                                Title = getTitle("No estoy segura", getPosition(100, 2, "R_" + macro.MACRO.Substring(2) + "_NSS"))
                            };

                            oChoice.Choices.Add(oChoiceYes);
                            oChoice.Choices.Add(oChoiceNo);
                            oChoice.Choices.Add(oChoiceNoEstoySegura);
                            aForm.Sections[i].Groups[0].RadioButtons.Add(oChoice);
                        }

                    }
                }
            }
            

            return aForm;
        }

        public VidSignerClient()
        {

        }

        public VidSignerClient(string userName, string password)
        {
            WebConfigRepositorio oConfig = new WebConfigRepositorio();
            endPoint = oConfig.ObtenerValor("EndPointVidSigner");
            ClienteRest = new RestClient();
            ClienteRest.BaseUrl = new Uri(endPoint);
            ClienteRest.AddDefaultHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(
                 string.Format("{0}:{1}", userName, password))));

        }

        public List<Device> GetDevices()
        {
            var request = new RestRequest("api/devices", Method.GET);
            var queryResult = ClienteRest.Execute<List<Device>>(request).Data;
            return queryResult;
        }

        public bool EnviarDocumento(int oidDocumento, int oidExploracion, string tablet)
        {
            EXPLORACION oExploracion2 = ExploracionRepositorio.Obtener(oidExploracion);
            PACIENTE oPaciente = PacienteRepositorio.Obtener(oExploracion2.IOR_PACIENTE);
            Document oDocument = new Document();
            string firmante = oExploracion2.PACIENTE.PACIENTE1;
            string firmanteDni = oExploracion2.PACIENTE.DNI;

            if (!String.IsNullOrEmpty(oExploracion2.PACIENTE.RESPONSABLE)
                && !String.IsNullOrEmpty(oExploracion2.PACIENTE.DNIRESPONSABLE))
            {
                firmante = oExploracion2.PACIENTE.RESPONSABLE;
                firmanteDni = oExploracion2.PACIENTE.DNIRESPONSABLE;
            }
            IMAGENES oDocumento = ImagenesRepositorio.Obtener(oidDocumento);
            //Si llegamos a este punto en que el documento no ha sido generado pero se quiere enviar a firmar
            //Significa que es un documento de los que se generan al vuelo mediante render view y que no están basados
            //en plantillas de informes

            if (oDocumento.OID < 0)
            {
                List<VWImprimirFicha> oFichaImprimir = new List<VWImprimirFicha>();
                oFichaImprimir = ExploracionRepositorio.ImprimirFichaPri(oidExploracion, false);
                RadioDBContext db = new RadioDBContext();
                REFRACTOMETROS tipoDocumento = db.Refractometros.Single(p => p.OID == oidDocumento);
                WebConfigRepositorio oConfig = new WebConfigRepositorio();
                string RutaDOCS = oConfig.ObtenerValor("RUTADOCS");
                //Generamos el HTML de ejemplo de la factura para poder combinarlo con el modelo de datos
                string rutaFichaTemplate = RutaDOCS + @"\RWEXPLORACION_" + oFichaImprimir.First().IOR_PACIENTE + ".html";
                string rutaFicha = RutaDOCS + @"\RWEXPLORACION_" + oFichaImprimir.First().IOR_PACIENTE + ".pdf";
                StreamWriter Swr = new StreamWriter(rutaFichaTemplate);
                if (oExploracion2.GAPARATO.COD_GRUP == "PET")
                {
                    tipoDocumento.BAUDIOS = "ImprimirDelfosPET";
                }
                var context = ViewRenderer.CreateController<ExploracionController>().ControllerContext;
                var renderer = new ViewRenderer(context);           
                string html = renderer.RenderViewToString("~/Views/Exploracion/" + tipoDocumento.BAUDIOS + ".cshtml", oFichaImprimir.First());

                Swr.Write(html);
                Swr.Close();
                Swr.Dispose();
                CENTROS oCentro = db.Centros.Single(c => c.OID == oExploracion2.OWNER);
                byte[] aPDF = Models.HTMLToPDF.Converter.InformeFromHtml(
                    rutaFichaTemplate,
                    oCentro.LOGO_URL,
                    500,
                    1000);


            }
            else {
                oDocument.FileName = oDocumento.NOMBRE + "." + oDocumento.EXT;
                byte[] pdfBytes = System.IO.File.ReadAllBytes(oDocumento.RUTACOMPLETA);
                oDocument.DocContent = Convert.ToBase64String(pdfBytes);
            }

            
            tablet = tablet;
            Signer oFirmante = new Signer
            {
                NumberID = firmanteDni,
                SignerGUI = null,
                SignerName = firmante,
                TypeOfID = "DNI",
                DeviceName = tablet,
                Visible = new Visible
                {
                    Anchor = "FIRMAPACIENTE",
                    SizeX = 80,
                    SizeY = 40
                },
                SignerFullName = firmante,
                //  Form = VidSignerClient.createForm(oidPlantilla, oExploracion2.APARATO.DES_FIL)
            };
            oDocument.OrderedSignatures = true;
            oDocument.Signers = new List<Signer>();
            oDocument.Signers.Add(oFirmante);
            var respuesta = PostDocument(oDocument, true);

            if (respuesta.StatusCode == System.Net.HttpStatusCode.OK)
            {
                oDocument.DocGui = respuesta.Content.ToString().Split(':')[1].Substring(1, 36);
                this.DocUID = oDocument.DocGui;
                ImagenesRepositorio.UpdateCampo("DESCRIP", oDocument.DocGui, oDocumento.OID);
                return true;
            }
            else
            {
                oDocument.DocGui = "";
                this.ErrorMessage = respuesta.Content;
                return false;
            }           

            

        }


        public bool EnviarConFormulario(int oidPlantilla, int oidExploracion, string tablet)
        {

            EXPLORACION oExploracion2 = ExploracionRepositorio.Obtener(oidExploracion);
            PACIENTE oPaciente = PacienteRepositorio.Obtener(oExploracion2.IOR_PACIENTE);
            Document oDocument = new Document();
            string firmante = oExploracion2.PACIENTE.PACIENTE1;
            string firmanteDni = oExploracion2.PACIENTE.DNI;

            if (!String.IsNullOrEmpty(oExploracion2.PACIENTE.RESPONSABLE) && !String.IsNullOrEmpty(oExploracion2.PACIENTE.DNIRESPONSABLE))
            {
                firmante = oExploracion2.PACIENTE.RESPONSABLE;
                firmanteDni = oExploracion2.PACIENTE.DNIRESPONSABLE;
            }
            oDocument.FileName = GenerarPDFiText(true, oidPlantilla, oidExploracion);
            byte[] pdfBytes = System.IO.File.ReadAllBytes(oDocument.FileName);
            oDocument.DocContent = Convert.ToBase64String(pdfBytes);
            tablet = tablet;
            Signer oFirmante = new Signer
            {
                NumberID = firmanteDni,
                SignerGUI = null,
                SignerName = firmante,
                TypeOfID = "DNI",
                DeviceName = tablet,
                Visible = new Visible
                {
                    Anchor = "FIRMAPACIENTE",
                    SizeX = 80,
                    SizeY = 40
                },
                SignerFullName = firmante,
                Form = VidSignerClient.createForm(oidPlantilla, oExploracion2.APARATO.DES_FIL)
            };
            oDocument.OrderedSignatures = true;
            oDocument.Signers = new List<Signer>();
            oDocument.Signers.Add(oFirmante);
            var respuesta = PostDocument(oDocument, true);

            if (respuesta.StatusCode == System.Net.HttpStatusCode.OK)
            {
                oDocument.DocGui = respuesta.Content.ToString().Split(':')[1].Substring(1, 36);
            }
            else
            {
                oDocument.DocGui = "";
            }



            if (respuesta.StatusCode == System.Net.HttpStatusCode.OK)
            {
                this.DocUID = respuesta.Content;
                return true;
            }
            else
            {
                this.ErrorMessage = respuesta.Content;
                return false;
            }

        }

        public IRestResponse PostDocument(Document Documento, bool borrarTraEnviar = false)
        {

            //   ServicePointManager.SecurityProtocol =  (SecurityProtocolType)3072;
              ServicePointManager.SecurityProtocol =  (SecurityProtocolType)3072;

            var request = new RestRequest("api/v2.1/documents", Method.POST);
            request.AddHeader("Content-type", "application/json");
            request.AddHeader("Accept", "application/json");
            request.RequestFormat = DataFormat.Json;
            WebConfigRepositorio oConfig = new WebConfigRepositorio();

            try
            {               
                
                    Documento.NotificationURL = oConfig.ObtenerValor("urlVidSignerNotification");
              

            }
            catch (Exception)
            {

                
            }
                     

            request.AddBody(Documento);            
        
            IRestResponse result = ClienteRest.Execute(request);
            
            if (result.StatusCode == System.Net.HttpStatusCode.OK)
            {
                // System.IO.File.Delete(Documento.FileName);
            }
            return result;
        }
    }
}

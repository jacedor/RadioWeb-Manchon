using Neodynamic.SDK.Web;
using RadioWeb.Models;
using RadioWeb.Models.Repos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RadioWeb.Controllers
{
    [AllowAnonymous]
    public class ZebraController : Controller
    {

        const string PRINTER_ID = "-PID";
        const string INSTALLED_PRINTER_NAME = "-InstalledPrinterName";
        const string NET_PRINTER_HOST = "-NetPrinterHost";
        const string NET_PRINTER_PORT = "-NetPrinterPort";
        const string PARALLEL_PORT = "-ParallelPort";
        const string SERIAL_PORT = "-SerialPort";
        const string SERIAL_PORT_BAUDS = "-SerialPortBauds";
        const string SERIAL_PORT_DATA_BITS = "-SerialPortDataBits";
        const string SERIAL_PORT_STOP_BITS = "-SerialPortStopBits";
        const string SERIAL_PORT_PARITY = "-SerialPortParity";
        const string SERIAL_PORT_FLOW_CONTROL = "-SerialPortFlowControl";
        const string PRINTER_COMMANDS = "-PrinterCommands";

        //
        // GET: /Zebra/

        public void PrintCommands(string sid)
        {
            if (WebClientPrint.ProcessPrintJob(System.Web.HttpContext.Current.Request))
            {
                HttpApplicationStateBase app = HttpContext.Application;

                //Create a ClientPrintJob obj that will be processed at the client side by the WCPP
                ClientPrintJob cpj = new ClientPrintJob();

                //get printer commands for this user id
                object printerCommands = app[sid + PRINTER_COMMANDS];
                if (printerCommands != null)
                {
                    cpj.PrinterCommands = printerCommands.ToString();
                    cpj.FormatHexValues = true;
                }

                //get printer settings for this user id
                int printerTypeId = (int)app[sid + PRINTER_ID];

                if (printerTypeId == 0) //use default printer
                {
                    cpj.ClientPrinter = new DefaultPrinter();
                }
                else if (printerTypeId == 1) //show print dialog
                {
                    cpj.ClientPrinter = new UserSelectedPrinter();
                }
                else if (printerTypeId == 2) //use specified installed printer
                {
                    cpj.ClientPrinter = new InstalledPrinter(app[sid + INSTALLED_PRINTER_NAME].ToString());
                }
                else if (printerTypeId == 3) //use IP-Ethernet printer
                {
                    cpj.ClientPrinter = new NetworkPrinter(app[sid + NET_PRINTER_HOST].ToString(), int.Parse(app[sid + NET_PRINTER_PORT].ToString()));
                }
                else if (printerTypeId == 4) //use Parallel Port printer
                {
                    cpj.ClientPrinter = new ParallelPortPrinter(app[sid + PARALLEL_PORT].ToString());
                }
                else if (printerTypeId == 5) //use Serial Port printer
                {
                    cpj.ClientPrinter = new SerialPortPrinter(app[sid + SERIAL_PORT].ToString(),
                                                              int.Parse(app[sid + SERIAL_PORT_BAUDS].ToString()),
                                                              (System.IO.Ports.Parity)Enum.Parse(typeof(System.IO.Ports.Parity), app[sid + SERIAL_PORT_PARITY].ToString()),
                                                              (System.IO.Ports.StopBits)Enum.Parse(typeof(System.IO.Ports.StopBits), app[sid + SERIAL_PORT_STOP_BITS].ToString()),
                                                              int.Parse(app[sid + SERIAL_PORT_DATA_BITS].ToString()),
                                                              (System.IO.Ports.Handshake)Enum.Parse(typeof(System.IO.Ports.Handshake), app[sid + SERIAL_PORT_FLOW_CONTROL].ToString()));
                }

                //Send ClientPrintJob back to the client
                cpj.SendToClient(System.Web.HttpContext.Current.Response);

            }

        }



        private List<string> PrinterList()
        {
            List<string> impresoras = new List<string>();
            // POPULATE THE COMBO BOX.
            foreach (string sPrinters in System.Drawing.Printing.PrinterSettings.InstalledPrinters)
            {
                impresoras.Add(sPrinters);
            }
            return impresoras;
        }

        [HttpPost]
        public void ClientPrinterSettings1(string sid, int oidExploracion, string installedPrinterName, int copies = 1, string pid = "2")
        {

            try
            {
                HttpApplicationStateBase app = HttpContext.Application;

                //save settings in the global Application obj

                //save the type of printer selected by the user
                int i = int.Parse(pid);
                app[sid + PRINTER_ID] = i;

                if (i == 2)
                {
                    app[sid + INSTALLED_PRINTER_NAME] = installedPrinterName;
                }

                EXPLORACION oExplo = ExploracionRepositorio.Obtener(oidExploracion);

                WebConfigRepositorio oConfig = new WebConfigRepositorio();
                string lineFeed = "0x0A";

                string commandPrinters = "";
                commandPrinters += lineFeed;
                commandPrinters += "N";
                commandPrinters += lineFeed;
                commandPrinters += "Q609,24";
                commandPrinters += lineFeed;
                commandPrinters += "q784";
                commandPrinters += lineFeed;
                commandPrinters += "A30,60,0,1,2,2,N,\"NOMBREPACIENTE\"";
                commandPrinters += lineFeed;
                commandPrinters += "A30,110,0,1,2,2,N,\"MUTUA\"";
                commandPrinters += lineFeed;
                commandPrinters += "A30,160,0,1,2,2,N,\"NOMBREMEDICO\"";
                commandPrinters += lineFeed;
                commandPrinters += "A30,210,0,1,2,2,N,\"EXPLORACION: \"";
                commandPrinters += lineFeed;
                commandPrinters += "B280,340,0,3C,2,6,120,B,\"OIDEXPLORACION\"";

                commandPrinters += lineFeed;
                commandPrinters += "P1";
                commandPrinters += lineFeed;

                commandPrinters = commandPrinters.Replace("NOMBREPACIENTE", oExplo.PACIENTE.PACIENTE1);
                commandPrinters = commandPrinters.Replace("OIDEXPLORACION", oExplo.OID.ToString());
                commandPrinters = commandPrinters.Replace("NOMBREMEDICO", oExplo.COLEGIADO.TRATA + " " + oExplo.COLEGIADO.NOMBRE);
                commandPrinters = commandPrinters.Replace("MUTUA", oExplo.ENTIDAD_PAGADORA.NOMBRE);
                commandPrinters = commandPrinters.Replace("EXPLORACION", oExplo.APARATO.DES_FIL);
                commandPrinters = commandPrinters.Replace("FECHA", oExplo.FECHA.Value.ToShortDateString() + "-" + oExplo.HORA);
                commandPrinters = commandPrinters.Replace("NOMBRECAP", "");

                //save the printer commands specified by the user
                app[sid + PRINTER_COMMANDS] = commandPrinters;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost]
        public void ClientPrinterSettings(string sid, int oidExploracion, string installedPrinterName, int copies = 1, string pid = "2")
        {

            try
            {
                HttpApplicationStateBase app = HttpContext.Application;

                //save settings in the global Application obj

                //save the type of printer selected by the user
                int i = int.Parse(pid);
                app[sid + PRINTER_ID] = i;

                if (i == 2)
                {
                    app[sid + INSTALLED_PRINTER_NAME] = installedPrinterName;
                }

                EXPLORACION oExplo = ExploracionRepositorio.Obtener(oidExploracion);
                string commandPrinters = "";
                WebConfigRepositorio oConfig = new WebConfigRepositorio();
                if (installedPrinterName.ToUpper().Contains("ETIQUETAS"))
                {
                    try
                    {
                        commandPrinters = oConfig.ObtenerValor("FormatoZEBRA");
                    }
                    catch (Exception)
                    {
                    }
                }
                if (installedPrinterName.Contains("ETIQUETAS3"))
                {
                    commandPrinters = oConfig.ObtenerValor("FormatoZEBRA3");
                }
                if (installedPrinterName.Contains("ETIQUETAS2"))
                {
                    commandPrinters = oConfig.ObtenerValor("FormatoZEBRA2");
                }
                if (installedPrinterName.Contains("ETIQUETAS1"))
                {
                    try
                    {
                        commandPrinters = oConfig.ObtenerValor("FormatoZEBRA1");
                    }
                    catch (Exception)
                    {
                    }
                }

                if (installedPrinterName.Contains("DELFOS"))
                {
                    try
                    {
                        string lineFeed = "0x0A";
                        commandPrinters = "";
                        commandPrinters += lineFeed;
                        commandPrinters += "N";
                        commandPrinters += lineFeed;
                        commandPrinters += "Q609,24";
                        commandPrinters += lineFeed;
                        commandPrinters += "q784";
                        commandPrinters += lineFeed;
                        commandPrinters += "A30,40,0,1,2,2,N,\"NOMBREPACIENTE\"";
                        commandPrinters += lineFeed;
                        commandPrinters += "A30,90,0,1,2,2,N,\"MUTUA\"";
                        commandPrinters += lineFeed;
                        commandPrinters += "A30,140,0,1,2,2,N,\"FECHA\"";
                        commandPrinters += lineFeed;
                        commandPrinters += "A30,190,0,1,2,2,N,\"NOMBREMEDICO\"";
                        commandPrinters += lineFeed;
                        commandPrinters += "A30,240,0,1,2,2,N,\"EXPLORACION \"";
                        commandPrinters += lineFeed;
                        commandPrinters += "B280,350,0,3C,2,6,120,B,\"OIDEXPLORACION\"";
                        commandPrinters += lineFeed;
                        commandPrinters += "P1";
                        commandPrinters += lineFeed;
                        commandPrinters = commandPrinters.Replace("NOMBREPACIENTE", oExplo.PACIENTE.PACIENTE1);
                        commandPrinters = commandPrinters.Replace("OIDEXPLORACION", oExplo.OID.ToString());
                        commandPrinters = commandPrinters.Replace("NOMBREMEDICO", oExplo.COLEGIADO.TRATA + " " + oExplo.COLEGIADO.NOMBRE);
                        commandPrinters = commandPrinters.Replace("MUTUA", oExplo.ENTIDAD_PAGADORA.NOMBRE);
                        commandPrinters = commandPrinters.Replace("EXPLORACION", oExplo.APARATO.DES_FIL);
                        commandPrinters = commandPrinters.Replace("FECHA", oExplo.FECHA.Value.ToShortDateString() + "-" + oExplo.HORA);
                        commandPrinters = commandPrinters.Replace("NOMBRECAP", "");
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    commandPrinters = commandPrinters.Replace("NOMBREPACIENTE", oExplo.PACIENTE.PACIENTE1);
                    commandPrinters = commandPrinters.Replace("NUMEROHC", oExplo.OID.ToString());
                    commandPrinters = commandPrinters.Replace("NOMBREMEDICO", oExplo.COLEGIADO.TRATA + " " + oExplo.COLEGIADO.NOMBRE);
                    commandPrinters = commandPrinters.Replace("MUTUA", oExplo.ENTIDAD_PAGADORA.NOMBRE);
                    commandPrinters = commandPrinters.Replace("EXPLORACION", oExplo.APARATO.DES_FIL);
                    commandPrinters = commandPrinters.Replace("FECHA", oExplo.FECHA.Value.ToShortDateString() + "-" + oExplo.HORA);
                    commandPrinters = commandPrinters.Replace("NOMBRECAP", "");
                    commandPrinters = commandPrinters.Replace("^PR", "^PR" + copies);
                    commandPrinters = commandPrinters.Replace("^PQ", "^PQ" + copies);
                }
                //save the printer commands specified by the user
                app[sid + PRINTER_COMMANDS] = commandPrinters;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult Imprimir()
        {

            return View();
        }
        public ActionResult Index()
        {
            ViewBag.Impresoras = PrinterList();
            return View();
        }

        //
        // GET: /Zebra/Details/5

        public ActionResult Details()
        {
            return View();
        }

        public ActionResult Print()
        {
            return View();
        }


        //
        // GET: /Zebra/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Zebra/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Zebra/Edit/5

        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Zebra/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Zebra/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Zebra/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}

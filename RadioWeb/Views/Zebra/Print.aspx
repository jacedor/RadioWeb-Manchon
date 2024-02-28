<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>



<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="System.Collections.Generic" %>
<%@ Import Namespace="System.IO" %>
<%@ Import Namespace="Neodynamic.SDK.Web" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<script runat="server">


    protected void Page_Init(object sender, EventArgs e)
    {
        //Is a Print Request?
        if (WebClientPrint.ProcessPrintJob(Request))
        {
            
            bool useDefaultPrinter = (Request["useDefaultPrinter"] == "checked");
            string printerName = Server.UrlDecode(Request["printerName"]);

            
            
            //Create Zebra ZPL commands for sample receipt
            string cmds =  "^XA";
            cmds += "^FO20,30^GB750,1100,4^FS";
            cmds += "^FO20,30^GB750,200,4^FS";
            cmds += "^FO20,30^GB750,400,4^FS";
            cmds += "^FO20,30^GB750,700,4^FS";
            cmds += "^FO20,226^GB325,204,4^FS";
            cmds += "^FO30,40^ADN,36,20^FDShip to:^FS";
            cmds += "^FO30,260^ADN,18,10^FDPart number #^FS";
            cmds += "^FO360,260^ADN,18,10^FDDescription:^FS";
            cmds += "^FO30,750^ADN,36,20^FDFrom:^FS";
            cmds += "^FO150,125^ADN,36,20^FDAcme Printing^FS";
            cmds += "^FO60,330^ADN,36,20^FD14042^FS";
            cmds += "^FO400,330^ADN,36,20^FDScrew^FS";
            cmds += "^FO70,480^BY4^B3N,,200^FD12345678^FS";
            cmds += "^FO150,800^ADN,36,20^FDMacks Fabricating^FS";
            cmds += "^XZ";

            //Create a ClientPrintJob and send it back to the client!
            ClientPrintJob cpj = new ClientPrintJob();
            //set Zebra ZPL commands to print...
            cpj.PrinterCommands = cmds;
            cpj.FormatHexValues = true;

            //set client printer...
            if (useDefaultPrinter || printerName == "null")
                cpj.ClientPrinter = new DefaultPrinter();
            else
                cpj.ClientPrinter = new InstalledPrinter(printerName);
            //send it...
            cpj.SendToClient(Response);            

        }
    }
    
    
    
</script>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title>How to directly Print Zebra ZPL Commands without Printer Dialog</title>
</head>
<body>
    <%-- Store User's SessionId --%>
    <input type="hidden" id="sid" name="sid" value="<%=Session.SessionID%>" />

    <form id="form1" runat="server">

    <h1>How to directly Print Zebra ZPL Commands without Printer Dialog</h1>
    <label class="checkbox">
        <input type="checkbox" id="useDefaultPrinter" /> <strong>Use default printer</strong> or...
    </label>
    <div id="loadPrinters">
    <br />
    WebClientPrint can detect the installed printers in your machine.
    <br />
    <input type="button" onclick="javascript: jsWebClientPrint.getPrinters();" value="Load installed printers..." />
                    
    <br /><br />
    </div>
    <div id="installedPrinters" style="visibility:hidden">
    <br />
    <label for="installedPrinterName">Select an installed Printer:</label>
    <select name="installedPrinterName" id="installedPrinterName"></select>
    </div>
            
    <br /><br />
    <input type="button" style="font-size:18px" onclick="javascript: jsWebClientPrint.print('useDefaultPrinter=' + $('#useDefaultPrinter').attr('checked') + '&printerName=' + $('#installedPrinterName').val());" value="Print Label..." />
        
    <script type="text/javascript">
        var wcppGetPrintersDelay_ms = 5000; //5 sec

        function wcpGetPrintersOnSuccess() {
            <%-- Display client installed printers --%>
            if (arguments[0].length > 0) {
                var p = arguments[0].split("|");
                var options = '';
                for (var i = 0; i < p.length; i++) {
                    options += '<option>' + p[i] + '</option>';
                }
                $('#installedPrinters').css('visibility', 'visible');
                $('#installedPrinterName').html(options);
                $('#installedPrinterName').focus();
                $('#loadPrinters').hide();
            } else {
                alert("No printers are installed in your system.");
            }
        }

        function wcpGetPrintersOnFailure() {
            <%-- Do something if printers cannot be got from the client --%>
            alert("No printers are installed in your system.");
        }
    </script>
    


    </form>

    @section MyScript {
   
<script type="text/javascript" src="@Url.Content("~/scripts/formToWizard2.js")"></script>
    
<script type="text/javascript" src="@Url.Content("~/scripts/DemoPrintCommands.js")"></script>


@* Register the WebClientPrint script code. The param is the URL for the PrintCommands method in the DemoPrintCommandsController. *@
@Html.Raw(Neodynamic.SDK.Web.WebClientPrint.CreateScript(Url.Action("PrintCommands", "DemoPrintCommands", null, Request.Url.Scheme)))


}

    <%-- Add Reference to jQuery at Google CDN --%>
    <script src="http://ajax.googleapis.com/ajax/libs/jquery/1.8.2/jquery.min.js" type="text/javascript"></script>

    <%-- Register the WebClientPrint script code --%>
    <%=Neodynamic.SDK.Web.WebClientPrint.CreateScript()%>
       

</body>
</html>


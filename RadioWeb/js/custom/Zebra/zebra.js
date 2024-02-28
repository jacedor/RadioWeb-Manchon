var wcppGetPrintersDelay_ms = 5000; //5 sec

var impresoraEtiquetas = "";

function wcpGetPrintersOnSuccess() {

    if (arguments[0].length > 0) {
        var p = arguments[0].split("|");
        var options = '';

        for (var i = 0; i < p.length; i++) { 
            if (p[i].indexOf('ETIQUETAS') != -1) {
                impresoraEtiquetas = p[i];                
            }           

        }

    } else {
        toastr.error("No hay impresora de etiquetas en su sistema.");
    }
}

function wcpGetPrintersOnFailure() {
    toastr.error("No hay impresora de etiquetas en su sistema.");
}



$(document).on("click", "#btnEtiquetas,[data-etiquetas]", function myfunction() {


    //Condicion para saber si han presionado el desplegable y no un botón
    if (!($(this).attr("id") == null && $(this).data('etiquetas') == null)) {

        var copias = ($(this).data('etiquetas'));
       
        if (copias === null) {
            copias = 1;
        }

        var filaActiva = $("#ExploracionesTable* .ACTIVA");
        var oidExploracion = filaActiva.data('oid');

        //store printer settings in the server...
        $.post('/Zebra/ClientPrinterSettings', {
            sid: $("#sid").val(),
            copies: copias,
            oidExploracion: oidExploracion,
            installedPrinterName: impresoraEtiquetas
        }).done(function () {

            // Launch WCPP at the client side for printing...
            var sessionId = $("#sid").val();
            for (var i = 0; i < copias; i++) {
                jsWebClientPrint.print('sid=' + sessionId);
            }

        });
    }



    return false;

});

$(document).ready(function () {
    jsWebClientPrint.getPrinters();
});


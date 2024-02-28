$(document).on("click", "#EnviarEmail", function () {

    var restrictedLOPD = false;
    var mensageAlertaLOPD = "El usuario ha denegado algunos permisos en la LOPD, los cuales se muestran a continuación:";
    if ($("#ENVIO_MEDICO").length && $("#ENVIO_MEDICO").val() === 'T') {
        mensageAlertaLOPD += "\n-Envio de informes a Médicos.";
        restrictedLOPD = true;
    }
    if ($("#ENVIO_RESULTADOS").length && $("#ENVIO_RESULTADOS").val() === 'T') {
        mensageAlertaLOPD += "\n-Envio del resultado de informes al paciente.";
        restrictedLOPD = true;
    }
    if ($("#ENVIO_MAIL").length && $("#ENVIO_MAIL").val() === 'T') {
        mensageAlertaLOPD += "\n-Envio del notificaciones por correo.";
        restrictedLOPD = true;
    }
    /* Comentamos esto porque ya se esta controlando en otro lado.
    if ($("#ENVIO_SMS").length && $("#ENVIO_SMS").val() === 'T') {
        mensageAlertaLOPD += "\n-Envio del notificaciones por SMS.";
        restrictedLOPD = true;
    }
    */
    if ($("#ENVIO_PROPAGANDA").length && $("#ENVIO_PROPAGANDA").val() === 'T') {
        mensageAlertaLOPD += "\n-Envio del notificaciones por Propaganda.";
        restrictedLOPD = true;
    }

    mensageAlertaLOPD += "\nPor favor, revisa que no se estan infringiendo ninguna de estas reglas."
    if (restrictedLOPD) {
        $.confirm({
            title: 'Advertencia!',
            content: mensageAlertaLOPD.split('\n').join('<br>'),
            buttons: {
                enviar: {
                    btnClass: 'btn-warning',
                    action: function () {
                        enviarMail();
                    }
                },
                cancelar: {
                    btnClass: 'btn-primary'
                }
            }
        });
    } else {
        enviarMail();
    }

    return false;


});

function enviarMail() {
    //Comprobamos de que ventana estan cargando el modal, y cambiamos el comportamiento en consequencia
    if ($('div#cuerpoModelEnvioMail.modal-body').length != 0) {
        var filaActiva = $("#InformesList* .ACTIVA");
        var oidInforme = filaActiva.data('oid');

        if (!isValidEmailAddress($("#email").val())) {
            toastr.error('Dirección de mail no válida', 'Error', { timeOut: 5000 });
            return false;
        }

        if ($("#email").val().length === 0) {
            toastr.error('Dirección de mail no válida', 'Error', { timeOut: 5000 });
            return false;
        } else {
            var url = "/Informe/EnviarInformeMail"
            var email = {}; //The Object to Send Data Back to the Controller 
            email.USERNAME = $("#email").val();
            email.ASUNTO = $("#asunto").val();
            email.TEXTO = $("#texto").val();
            filaActiva = $("#InformesList* .ACTIVA");
            email.ADJUNTO1 = $("#adjunto1").data('ruta');
            email.ADJUNTO2 = $("#checkEnvioSMS:checked").val();
            var oidExploracion = filaActiva.data('exploracion');
            email.CID = oidExploracion;


            $.ajax({
                type: 'POST',
                url: url,
                data: email,
                success: function (data) {
                    toastr.success('Email enviado con informe adjunto', '', { timeOut: 5000 });
                    $('#myModalEnvioInforme').modal('toggle');
                }
            });

            if ($("#checkEnvioSMS:checked").val()) {
                filaActiva = $("#InformesList* .ACTIVA");
                oidInforme = filaActiva.data('oid');
                //texto que se enviará textoSMSPlantil SMS 
                $("#textoSMS").val($("#textoSMSPlantilla").val());
                var txtMensajeMovil = $("#textoSMS").val(function (i, v) {
                    return v.replace("CDPI", oidInforme);
                }).val();
                var options = {
                    url: "/SMS/Enviar",
                    data: "phone=" + $("#movilPaciente").val() + "&texto=" + txtMensajeMovil + "&idMensaje=" + oidInforme,
                    type: "GET"
                };
                $.ajax(options).complete(function (data) {
                    toastr.success('Enviado correctamente', 'SMS',
                        { timeOut: 5000 });
                });
            }
        }

    } else {
        var oidInforme = $("#OIDINFORME").val();
        var filaActiva = $("#ExploracionesTable* .ACTIVA");


        if (!isValidEmailAddress($("#email").val())) {
            toastr.error('Dirección de mail no válida', 'Error', { timeOut: 5000 });
            return false;
        }

        if ($("#email").val().length === 0) {
            toastr.error('Dirección de mail no válida', 'Error', { timeOut: 5000 });
            return false;
        } else {
            var url = "/Informe/EnviarInformeMail"
            var email = {}; //The Object to Send Data Back to the Controller
            email.USERNAME = $("#email").val();
            email.ASUNTO = $("#asunto").val();
            email.TEXTO = $("#texto").val();

            email.ADJUNTO1 = $("#adjunto1").data('ruta');
            email.ADJUNTO2 = $("#checkEnvioSMS:checked").val();
            var oidExploracion = filaActiva.data('oid');
            email.CID = oidExploracion;


            $.ajax({
                type: 'POST',
                url: url,
                data: email,
                success: function (data) {
                    toastr.success('Email enviado con informe adjunto', '', { timeOut: 5000 });
                    $('#myModalEnvioInforme').modal('toggle');
                }
            });

            if ($("#checkEnvioSMS:checked").val()) {

                //texto que se enviará textoSMSPlantil SMS
                $("#textoSMS").val($("#textoSMSPlantilla").val());
                var txtMensajeMovil = $("#textoSMS").val(function (i, v) {
                    return v.replace("CDPI", oidInforme);
                }).val();
                var options = {
                    url: "/SMS/Enviar",
                    data: "phone=" + $("#movilPaciente").val() + "&texto=" + txtMensajeMovil + "&idMensaje=" + oidInforme,
                    type: "GET"
                };
                $.ajax(options).complete(function (data) {
                    toastr.success('Enviado correctamente', 'SMS',
                        { timeOut: 5000 });
                });
            }
        }
    }
    return false;

}

/// <reference path="_references.js" />
/// <reference path="jquery-2.0.3.min.js" />
/// <reference path="jquery-ui-1.10.3.min.js" /




var oExploracion = {};
var Paciente = {};


$(document).on( 'change','#chkRecogido', function () {

    if ($(this).is(":checked")) {
        $('#fechaRecogido').val(moment().format('DD-MM-YYYY'));
    }
    else {
        $('#fechaRecogido').val('');
    }
});

$(document).on('click', '#GuardarExploracion, #GuardarExploracionYSalir', function () {

    var triger = $(this).attr('id');
    var form = $("#frmExploracion");
    
    if (form.valid()) {
        Paciente.TEXTO = $('#TextoImprimible').val();
        oExploracion.OID = $(".ExploracionContenedorPrincipal").data("oid");
        oExploracion.IOR_TIPOEXPLORACION = $("#ddlExploracionExplo option[value=" + $("#ddlExploracionExplo").val() + "]").val();
        oExploracion.IOR_COLEGIADO = $('.colegiado').data('oid');
        oExploracion.IOR_TECNICO = $("#ddlSanitario option[value=" + $("#ddlSanitario").val() + "]").val();
        oExploracion.IOR_ESTUDIANTE = $("#ddlEstudiante option[value=" + $("#ddlEstudiante").val() + "]").val();
        oExploracion.COD_FIL = $("#ddlAparatosExplo option[value=" + $("#ddlAparatosExplo").val() + "]").text();
        oExploracion.IOR_APARATO = $("#ddlAparatosExplo option[value=" + $("#ddlAparatosExplo").val() + "]").val();
        oExploracion.IOR_PACIENTE = $(".ExploracionContenedorPrincipal").data("owner");
        oExploracion.FIL = $("#ddlExploracionExplo option[value=" + $("#ddlExploracionExplo").val() + "]").val();
        oExploracion.CANTIDAD = $('#Cantidad').val();
        oExploracion.RECOGIDO = $('#chkRecogido').is(':checked') ? "T" : "F";
        oExploracion.FECHADERIVACION = $('#fechaRecogido').val();
        oExploracion.HORA_IDEN = moment().format('HH:mm');
        oExploracion.HORAMOD = moment().format('HH:mm');
        oExploracion.FECHA_IDEN = moment().format('DD-MM-YYYY');
        oExploracion.TEXTO = $("#textoExploracion").val();
        oExploracion.HORA = $("#horaExploracion").val();
        oExploracion.FECHAMAXENTREGA = $("#fechaMaximaEntrega").val();
        oExploracion.IOR_MEDICO = $("#ddlMedicoInformante option[value=" + $("#ddlMedicoInformante").val() + "]").val();


        oExploracion.PACIENTE = Paciente;

        if ($("#Cantidad") != null) {
            oExploracion.CANTIDAD = $("#Cantidad").val();
        }
        var divExploracion = $("div[data-tipoexplo]");
        switch (divExploracion.data("tipoexplo")) {
            case "ICS":
                alert("ICS");
                break;
            case "MUT":
                oExploracion.IOR_ENTIDADPAGADORA = $("#ddlMutuasExplo").val();

                break;
            case "PRI":
                oExploracion.IOR_ENTIDADPAGADORA = $("#ddlMutuasExplo").val();
                break;


            default:

        }


        if (($("#ddlExploracionExplo option[value=" + $("#ddlExploracionExplo").val() + "]").val() == "undefined")) {
            oExploracion.IOR_TIPOEXPLORACION = $("#ddlExploracionExplo").val();
        } else {
            oExploracion.IOR_TIPOEXPLORACION = $("#ddlExploracionExplo option[value=" + $("#ddlExploracionExplo").val() + "]").val();
        }
       

        var options = {
            url: "/Exploracion/Update",
            data: JSON.stringify({ oExploracion: oExploracion }),
            contentType: 'application/json',
            dataType: 'html',
            type: "POST"
        };

        $.ajax(options).done(function (data) {



            $.growl.notice({ title: "Exploracion", message: "Exploracion modificada correctamente!" });
         
            if (triger == "GuardarExploracionYSalir") {
                window.history.back();
            }
            else {
                window.location.reload();
            }

        });
    }


});



$(document).ready(function () {





});

$("#BuscarColegiado").keyup($.debounce(250, function () {
    var data = $(this).val();
    var url = "/Colegiado/List/"; //The Url to the Action  Method of the Controller
    var Colegiado = {}; //The Object to Send Data Back to the Controller
    Colegiado.NOMBRE = $("#BuscarColegiado").val();
    // Check whether the TextBox Contains text
    // if it does then make ajax call

    if ($("#BuscarColegiado").val().length > 3 && $("#BuscarColegiado").val() !== "") {
        $.ajax({
            type: 'POST',
            url: url,
            data: Colegiado,
            dataType: "html",
            success: function (evt) {
                $('#ColegiadoList > tbody').html(evt);
            },
        });
    }

}));



$(document).on("click", "#GuardarRecogidaMultiple", function myfunction() {
    $("#countOtrasExploraciones").html($("#tblRecogidaMultiple tbody tr.ACTIVA").length);
    if ($("#countOtrasExploraciones").length > 0) {
        var oidOtrasRecogidas = "";
        $.each($("#tblRecogidaMultiple tbody tr.ACTIVA"), function (i, recogida) {
            oidOtrasRecogidas = $(recogida).data('oid');
        });
        $("#OTRASEXPLORACIONESRECOGIDAS").val(oidOtrasRecogidas);
    }
 
    
});



function copiarColegiado(oidExploracion, oidColegiado) {

    $.ajax({
        type: 'POST',
        url: '/Exploracion/ActualizarColegiado',
        data: { oidExploracion: oidExploracion, oidColegiado: oidColegiado },
        complete: function () {

        }

    });

}

//mapeamos el evento click de redirección hacia la ficha del paciente para almacenar la URL Actual
$(document).on('click', '#linkFichaPaciente,.fichaPaciente', function () {
    saveURLRetorno(window.location.pathname);
});

$(document).on('click', '#copiarColegiado', function () {

    var IOR_MEDICOREFERIDOR = $("#IOR_COLEGIADO").val();
    var OID_EXPLORACION = $("#IOR_EXPLORACION").val();


    $.ajax({
        type: 'POST',
        url: "/Entrada/CopiarColegiado",
        data: "IOR_COLEGIADO=" + IOR_MEDICOREFERIDOR + "&" + "OIDEXPLORACION=" + OID_EXPLORACION,
        complete: function () {
            toastr.success('Exploración!', 'Medico asignado a todas exploraciones mismo dia', { timeOut: 5000 });
        }

    });

});

//$(document).on("click", "#btnImprimir", function myfunction() {

//    imprimirExploracion($(this).data('oid'));
//    return false;

//});


$(document).on("click", "#btnInfoMutua", function myfunction() {

    if ($("#indicacionesMutua").hasClass('hide')) {
        $("#indicacionesMutua").removeClass("hide");
        $("#ExploracionDetailsPaciente").addClass("hide");
    }
    else {
        $("#indicacionesMutua").addClass("hide");
        $("#ExploracionDetailsPaciente").removeClass("hide");
    }

    return false;

});

$(document).on('click', '#ExploracionesList tbody tr', function () {
    $(this).siblings().removeClass('ACTIVA');
    $(this).addClass('ACTIVA');
    var filaActiva = $("#ExploracionesList tbody tr.ACTIVA");
    var oExploracion = {};

    oExploracion.OID = filaActiva.data('oid');
    oExploracion.URLPREVIAEXPLORACION = $("#URLPREVIAEXPLORACION").val();
    $(".spiner-cargando-exploracion").removeClass('hide');
    $("#PanelExploracionInner").find(".panel-body").addClass('hide');
    var request = $.ajax({
        url: "/Exploracion/GetPanelExploracion",
        data: JSON.stringify({ oExploracion: oExploracion, cambiaRegimenEconomicoActual: false }),
        contentType: 'application/json',
        type: "POST"
    });


    request.done(function (data) {
        var $target = $("#PanelExploracionInner");
        $target.empty();
        var $newHtml = $(data);
        $target.html(data);
    });
    request.complete(function () {
        $(".select2").select2({
            theme: "bootstrap",
            width: "100%"
        });
        $(".spiner-cargando-exploracion").addClass('hide');
        $("#PanelExploracionInner").find(".panel-body").removeClass('hide');
        $(".fecha-picker,.date-picker").datepicker({
            format: "dd/mm/yyyy",
            todayBtn: true,
            language: "es",
            autoclose: true,
            todayHighlight: true
        });
    });

});



$(document).on('click', '#tblRecogidaMultiple tbody tr', function () {
    // do something
    if ($(this).hasClass('ACTIVA')) {
        $(this).removeClass('ACTIVA');
    } else {
        $(this).addClass('ACTIVA');
    }

});

$(document).on('click', '#countOtrasExploraciones', function () {


    if ($("#tblRecogidaMultiple tbody tr").length > 0) {
        $('#modal-recogida-informes').modal('show');
    }


});

$('#modalColegiado').on('shown.bs.modal', function (e) {
    $("#BuscarColegiado").val('').focus();
});



$(document).on('click', "input.TipoExploracion", function (event) {

    var oExploracion = {};
    oExploracion.HORA = $("#HORA").val();
    oExploracion.FECHA = $("#FECHA").val();
    oExploracion.OID = $("#IOR_EXPLORACION").val();
    oExploracion.IOR_APARATO = $("#IOR_APARATO").val();
    oExploracion.URLPREVIAEXPLORACION = $("#URLPREVIAEXPLORACION").val();
    var ENTIDAD_PAGADORA = {};
    oExploracion.ENTIDAD_PAGADORA = ENTIDAD_PAGADORA;
    switch ($(this).val()) {
        case "PRI":
            oExploracion.ENTIDAD_PAGADORA.OWNER = 1;
            //oExploracion.ENTIDAD_PAGADORA.OID = 3820080;
            break;
        case "MUT":
            oExploracion.ENTIDAD_PAGADORA.OWNER = 2;
            break;
        case "ICS":
            oExploracion.ENTIDAD_PAGADORA.OWNER = 3;
            break;
        default:

    }
    var request = $.ajax({
        url: "/Exploracion/GetPanelExploracion",
        data: JSON.stringify({ oExploracion: oExploracion }),
        contentType: 'application/json',
        type: "POST"
    });

    request.done(function (data) {
        var $target = $("#PanelExploracionInner");
        $target.empty();
        var $newHtml = $(data);
        $target.html(data);
    });
    request.complete(function () {
        $(".select2").select2({
            theme: "bootstrap",
            width: "100%"
        });
        $(".spiner-cargando-exploracion").addClass('hide');
        $("#PanelExploracionInner").find(".panel-body").removeClass('hide');
        $(".fecha-picker,.date-picker").datepicker({
            format: "dd/mm/yyyy",
            todayBtn: true,
            language: "es",
            autoclose: true,
            todayHighlight: true
        });
    });

});


$(document).on("click", ".asignarColegiado", function (e) {
    var oidColegiado = $(this).data('oid');
    var url = "/Colegiado/GetParaFichaExploracion/"; //The Url to the Action  Method of the Controller
    var Colegiado = {}; //The Object to Send Data Back to the Controller
    Colegiado.OID = oidColegiado;
    Colegiado.IOR_EXPLORACION = $("#OID").val();
    $.ajax({
        type: 'POST',
        url: url,
        data: Colegiado,
        dataType: "html",
        success: function (evt) {
            $('#FichaColegiado').html(evt);
            toastr.success('Colegiado!', 'Colegiado Asignado!', {
                timeOut: 3000,
                positionClass: 'toast-bottom-right'
            });
            // $("#Guardar").trigger("click");
        },
    });
});



$('#modalSMS').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget); // Button that triggered the modal
    var oidInforme = button.data('oid');// Extract info from data-* attributes
    $("#textoSMS").val($("#textoSMSPlantilla").val());
    var txtMensajeMovil = $("#textoSMS").val(function (i, v) {
        return v.replace("CDPI", oidInforme);
    }).val();

    var idPaciente = $(".enviarSMS").data('iorpaciente');
    $.ajax({
        type: 'POST',
        url: '/Paciente/getLOPDsettingsById',
        data: {
            idPaciente: idPaciente
        },
        beforeSend: function () {
            $("#displayPermisoLOPDsms").empty();
            $("#EnviarSMS").prop("disabled", true);
        },
        success: function (data) {
            $("#displayPermisoLOPDsms").html(data);
            $("#EnviarSMS").prop("disabled", false);
            if ($("#ENVIO_SMS").val() === "T") {
                toastr.warning('El paciente no consiente el envío de SMS en su declaración de la LOPD', 'El SMS no se enviará', {
                    timeOut: 5000
                });
            }
        }
    });

});

$(document).on("click", "#EnviarSMS", function () {
    var button = $(event.relatedTarget); // Button that triggered the modal
    var oidInforme = button.data('oid');// Extract info from data-* attributes

    var envio_SMS = $("#ENVIO_SMS").val();

    if (envio_SMS === "" || envio_SMS === "F") {
        var options = {
            url: "/SMS/Enviar",
            data: "phone=" + $("#movil").val() + "&texto=" + $("#textoSMS").val() + "&idMensaje=" + oidInforme,
            type: "GET"
        };

        $.ajax(options).complete(function (data) {
            toastr.success('Enviado correctamente', 'SMS', { timeOut: 5000 });
            $("#modalSMS").modal("toggle");
        });
    } else {
        //No ha dado su permiso para enviar SMS.
        toastr.error('Este usuario no permite notificaciones por SMS', 'DERECHO LOPD', { timeOut: 5000 });
    }
});


$(document).on("click", "#btnHistoriaClinica", function (e) {
    var oidPaciente = $(this).data('iorpaciente');
    var url = "/HistoriaClinica/Index/" + oidPaciente; //The Url to the Action  Method of the Controller

    $.ajax({
        type: 'GET',
        url: url,
        dataType: "html",
        success: function (evt) {
            $('#modal-form-historia').remove();
            $("body").append(evt);
            $('#modal-form-historia').modal('show');

            $('#TEXTOHTML').summernote({
                tabsize: 2,
                height: 200,
                lang: 'es-ES',
                onInit: function () {
                    $("#summernote").summernote('code', '<p style="font-family: Verdana;"><br></p>')
                }
            });

        },
    });
});

$(document).on("click", "#btnInfoCentroExterno", function (e) {
    var oidColegiado = $(this).data('oid');
    var url = "/CentroExterno/GetTexto/" + $("#IOR_CENTROEXTERNO").val(); //The Url to the Action  Method of the Controller

    $.ajax({
        type: 'POST',
        url: url,
        dataType: "html",
        success: function (evt) {
            $('#modal-info-centro-Externo').remove();
            $("body").append(evt);
            $('#modal-info-centro-Externo').modal('show');

        },
    });
});


$(document).on('change keyup', '#IOR_APARATO, #IOR_ENTIDADPAGADORA', function () {


    var IOR_ENTIDADPAGADORA = $("#IOR_ENTIDADPAGADORA").val();
    var IOR_APARATO = $("#IOR_APARATO").val();

    var cantidad = $('#CANTIDAD').val('');
    var options = {
        url: "/Exploracion/GetTipoExploraciones",
        data: "IOR_APARATO=" + IOR_APARATO + "&" + "IOR_MUTUA=" + IOR_ENTIDADPAGADORA
    };

    $.ajax(options).success(function (data) {

        var sel = $("#IOR_TIPOEXPLORACION");
        sel.empty();
        var markup = '';
        for (var x = 0; x < data.length; x++) {

            markup += '<option value="' + data[x].OID + '" data-cod="' + data[x].FIL + '" data-text-value="' + data[x].DES_FIL + '">' + data[x].FIL + ' - ' + data[x].DES_FIL + '</option>';


        }
        sel.html(markup).show();

        $("#IOR_TIPOEXPLORACION option:first").attr('selected', 'selected');
    });
});


// evento click del SELECT DE TIPO DE EXPLORACION QUE CAMBIA EL PRECIO
$(document).on('change keyup', '#IOR_TIPOEXPLORACION', function () {

    var IOR_TIPOEXPLORACION = $("#IOR_TIPOEXPLORACION").val();
    var IOR_ENTIDADPAGADORA = $("#IOR_ENTIDADPAGADORA").val();


    var options = {
        url: "/Exploracion/GetPrecioExploracion",
        data: ({ IOR_TIPOEXPLORACION: IOR_TIPOEXPLORACION, IOR_MUTUA: IOR_ENTIDADPAGADORA }),
        dataType: "html",
        type: "GET"
    };

    $.ajax(options).complete(function (data) {

        $("#CANTIDAD").val(data.responseText);
    });
});

// evento click del SELECT DE TIPO DE EXPLORACION QUE CAMBIA EL PRECIO
$(document).on('change keyup', '#Q_ALFA', function () {

    var IOR_TIPOEXPLORACION = $("#IOR_TIPOEXPLORACION").val();
    var IOR_ENTIDADPAGADORA = $("#IOR_ENTIDADPAGADORA").val();
    var qAlfa = $("#Q_ALFA").val();

    if (IOR_TIPOEXPLORACION > 0) {
        var options = {
            url: "/Exploracion/GetPrecioExploracionConDescuento",
            data: (
                {
                    IOR_TIPOEXPLORACION: IOR_TIPOEXPLORACION,
                    IOR_MUTUA: IOR_ENTIDADPAGADORA,
                    Q_ALFA: qAlfa
                }
            ),
            dataType: "html",
            type: "GET"
        };

        $.ajax(options).complete(function (data) {
            $("#CANTIDAD").val(data.responseText);
        });
    } else {
        if (qAlfa !== "0") {
            toastr.error('', 'Para haces Descuento tiene que elegir tipo de exploración!', { timeOut: 5000 });
            $("#Q_ALFA").val("0").change();
        }
    }
});


$(document).ready(function () {

    $("li[data-view]").removeClass('active');
    $("[data-view=Exploracion]").parents("ul").removeClass("collapse");
    $("[data-view=Exploracion]").parents("li").addClass('active');
    $("[data-view=Exploracion]").addClass("active");


    $('.textoXeditable').editable({
        container: 'body',
        inputclass: 'anchoTexto'
    });

    $('#TEXTOHISTORIACLINICA').summernote('focus');



    if ($("#SaveColegiado").val() === "True") {
        toastr.success('Colegiado asignado!', 'Colegiado asignado correctamente!', {
            timeOut: 3000,
            positionClass: 'toast-bottom-right'
        });
    }


});
var EstadosExploracion = {
    Pendiente: 0,
    Borrado: 1,
    Presencia: 2,
    Confirmado: 3,
    NoPresentado: 4,
    LlamaAnulando: 5
};

var ctrlPressed = false;
$(window).keydown(function (evt) {
    if (evt.which === 17) { // ctrl
        ctrlPressed = true;
    }
}).keyup(function (evt) {
    if (evt.which === 17) { // ctrl
        ctrlPressed = false;
    }
});

function horarioSorter(a, b) {

    if (a > b) return 1;
    if (a < b) return -1;
    return 0;
}
//function cambiarEstadoExploracion(estadoActual, estadoNuevo, Oid, hhora, imprimirficha) {

//    var request = $.ajax({
//        url: "/Exploracion/CambiarEstado",
//        data: "estadoActual=" + estadoActual + "&" + "estadoNuevo=" + estadoNuevo + "&" + "oidExploracion=" + Oid + "&" + "hhora=" + hhora + "&" + "oidAparato=" + estadoNuevo,
//        type: "GET",
//        dataType: "html",
//        async: "false"
//    });
//    //$('.spinnerCalendario').removeClass('hide');

//    request.done(function (data) {
//        //LoadListaDia(false);
//        LoadListaDia(false);
//        if (imprimirficha) {
//            imprimirExploracion(Oid);
//        }

//    });

//    toastr.success('Estado', 'Estado exploracion modificado', {
//        timeOut: 3000,
//        positionClass: 'toast-bottom-right'
//    });
//}

function filterRows(statusName) {
    $("#ExploracionesTable tbody tr").each(function () {
        if (!$(this).hasClass(statusName) && !$(this).hasClass('ACTIVA')) {
            $(this).hide();
        }
    });
}





//mapeamos el evento click de cada una de las filas del carrito sobre el aparato que lanza el loadListaDia
$(document).on('click', '.filtroAparatoCarrito', function () {

    $('#FILTROS_IOR_APARATO option:contains("' + $(this).data('aparato') + '")').prop('selected', true);
    $("#FILTROS_IOR_APARATO").trigger("change");
    $('.popover').hide();
});

//mapeamos el evento click de cada una de las filas de la tabla de exploraciones de dentro del carrito que permite trasladar exploraciones
//$(document).on('click', '.trasladarExploracion', function () {
//    trasladarExploracion($(this).data('oid'));
//});

////mapeamos el evento click de cada una de las filas de la tabla de exploraciones de dentro del carrito que permite trasladar exploraciones
//$(document).on('click', '.vincularHL7', function () {
//    var url = $(this).attr("href");

//    $.ajax({
//        type: 'GET',
//        url: url,       
//        success: function (data, textStatus, xhr) {
//            if (xhr.statusText === "VINCULADO") {
//                toastr.info('Exploracion vinculada correctamente. ', '', { timeOut: 3000 });
//                LoadListaDia(false);
//                  $('#modal-form-vincular').modal('hide');
//            } 
//        },
//        error: function (xhr, ajaxOptions, thrownError) {
//            switch (xhr.status) {
//                case 404:
//                     toastr.error(xhr.statusText, '', { timeOut: 3000 });
//            }
//        }

//    });
//    return false;
//});


////mapeamos el evento click de cada una de las filas de la tabla de exploraciones de dentro del carrito que permite trasladar exploraciones
//$(document).on('click', '.duplicarExploracion,.exploracionRelacionada', function (e) {
//    var filaActiva = $("#ExploracionesTable* .ACTIVA");
//    if (filaActiva.length === 1 && filaActiva.data('anulada') !== 'True') {
//        var oidRelacionada = -1;
//        var filtroActivos = getFiltrosBusqueda();
//        var fechaExplo = "01-01-1990";
//        if ($(this).hasClass('exploracionRelacionada')) {
//            //Si queremos duplicar relacionando con otra exploración 
//            //tenemos que obtener la fecha en la que estamos en el lista dia

//            fechaExplo = filtroActivos.Fecha;
//            oidRelacionada = $(this).data('oid');
//        }
//        window.location = "/Exploracion/Duplicar/" + $(this).data('oid')
//            + "?hora=" + filaActiva.data('hhora') + '&ioraparato=' + filtroActivos.oidAparato + '&relacionada=' + oidRelacionada + '&fecha=' + fechaExplo;
//    } else {
//        toastr.error('Debe seleccionar un hueco en la lista de exploracion', 'Duplicar', { timeOut: 5000 });
//    }
//    return false;
//});

////mapeamos el evento click de cada una de las filas de la tabla de exploraciones de dentro del carrito que permite trasladar exploraciones
//$(document).on('click', '.eliminarDelCarrito', function () {
//    eliminarDelCarrito($(this).data('oid'));
//});

////los evento change y mover son up y down de todos los filtros del calendario
//$(document).on('change', '#ddlEmail', function () {
//    var idPlantillaMail = $(this).data('oid');
//    var texto = $("#ddlEmail option[value=" + $("#ddlEmail").val() + "]").data('contenido');
//    $("#textoSMS").val(texto);
//});

////mapeamos el evento click de cada una de las filas de la tabla de exploraciones
//$(document).on('click', '#ExploracionesTable tbody tr', function () {
//    ajustaEstadoMenuSuperior($(this));
//});

//$(document).on('click', '#btnDiaAnterior', function () {
//    setCurrentDayActions(addDays($("#fechaSelect").val(), -1));

//    return false;
//});

//$(document).on('click', '#btnHoy', function () {
//    setCurrentDayActions(moment(new Date()).format('DD-MM-YYYY'));
//    return false;
//});


//$(document).on('click', '#btnDiaSiguiente', function () {
//    setCurrentDayActions(addDays($("#fechaSelect").val(), 1));

//    return false;
//});
//$(document).on("click", "#btnConfirmar", function myfunction() {

//    var filaActiva = $("#ExploracionesTable* .ACTIVA");


//    //var estadoNuevo = EstadosExploracion.Confirmado;
//    var estadoNuevo = filaActiva.data('estado') === EstadosExploracion.Confirmado
//        ? EstadosExploracion.Presencia
//        : EstadosExploracion.Confirmado;

//    var hhora = filaActiva.data('hhora');

//    var oidExploracionSeleccionada = filaActiva.data('oid');

//    cambiarEstadoExploracion(filaActiva.data('estado'), estadoNuevo, oidExploracionSeleccionada, hhora);
//    return false;
//});

//$(document).on("click", "#btnActualizarPresencia", function myfunction() {

//    var filaActiva = $("#ExploracionesTable* .ACTIVA");
//    var estadoNuevo = filaActiva.data('estado') === 0
//    

    ? '2'
//        : '0';

//    var hhora = filaActiva.data('hhora');
//    var oidExploracionSeleccionada = filaActiva.data('oid');

//    cambiarEstadoExploracion(filaActiva.data('estado'), estadoNuevo, oidExploracionSeleccionada, hhora);
//    return false;

//});

$(document).on("click", "#btnPagoRapido", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActiva.data('oid');

    window.location = "/Pagos/Index/" + oidExploracion;
    return false;

});


$(document).on("click", "#btnCapturarDesdeTablet", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActiva.data('oid');

    window.location = "/Imagenes/Create/" + oidExploracion;
    return false;

});




$(document).on("click", "#btnPresenciaImprimir", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var estadoNuevo = filaActiva.data('estado') === 0
        ? '2'
        : '0';

    var hhora = filaActiva.data('hhora');
    var oidExploracionSeleccionada = filaActiva.data('oid');

    cambiarEstadoExploracion(filaActiva.data('estado'), estadoNuevo, oidExploracionSeleccionada, hhora, true);




    return false;

});

$('#modalImprimirJustificante').on('show.bs.modal', function (e) {
    var filaActiva = $("#ExploracionesTable* .ACTIVA");

    $("#HoraLLegadaJustificante").val(filaActiva.data('horall'));
    // $("#HoraRealizadaJustificante").val(filaActiva.data('horaex'));
    $("#HoraRealizadaJustificante").val(moment().format("HH:mm"));

});

$(document).on("click", "#btnImprimir", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActiva.data('oid');

    imprimirExploracion(oidExploracion);
    return false;

});

$(document).on("click", "#btnImprimirJustificante", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActiva.data('oid');

    imprimirJustificante(oidExploracion,
        $("#HoraLLegadaJustificante").val(),
        $("#HoraRealizadaJustificante").val(),
        $("#textoLibreJustificante").val());

    return false;

});



$(document).on("click", "#btnFichaPaciente", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidPaciente = filaActiva.data('owner');

    window.location = "/Paciente/Details?ior_paciente=" + oidPaciente + "&TraerInformesYExplos=true&ReturnUrl=/Home/Index";
    return false;

});



//Al escribir sobre la caja de texto del modal popup de pacintes
$("#txtPaciente").keyup($.debounce(500, function () {

    if (($("#txtPaciente").val().length === 0) || ($("#txtPaciente").val().length > 3 && $("#txtPaciente").val() !== "")) {

        $("#ExploracionesTable").bootstrapTable('filterBy', {
            paciente: $("#txtPaciente").val()
        });
        filtrosBusqueda.Paciente = $("#txtPaciente").val();

        SaveFilters();
    }

}));

$(document).on("click", "#BuscarPaciente", function myfunction() {


    $("#ExploracionesTable").bootstrapTable('filterBy', {
        paciente: $("#txtPaciente").val()
    });

    return false;

});


$(document).on("click", "#btnAgregarExploracion ,.agregarExploracion", function myfunction() {

    if ($(this).hasClass("agregarExploracion")) {
        var trAlta = $(this).closest('tr');
        trAlta.siblings().removeClass('ACTIVA');
        trAlta.addClass('ACTIVA');
    }
    //  SaveFilters();
    sessionStorage.vistaActual = $(this).data('href');

});



$(document).on("sort.bs.table", "#ExploracionesTable", function (name, order) {
    var direccionOrden = name.handleObj.handler.arguments[2];
    var campoOrden = order;
    $("#orderDirection").val(direccionOrden);
    $("#orderField").val(campoOrden);
});



$("#BuscarConsumible").keyup($.debounce(250, function () {
    var data = $(this).val();
    var url = "/Consumible/List/"; //The Url to the Action  Method of the Controller
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var mutua = filaActiva.data('mutua');
    var grupoOid = filaActiva.data('grupo');
    var grupoDesc = $("#ddlGrupo option[value=" + grupoOid + "]").text();
    var Consumbile = {}; //The Object to Send Data Back to the Controller
    Consumbile.USERNAME = $("#BuscarConsumible").val();
    Consumbile.IOR_ENTIDADPAGADORA = mutua;
    Consumbile.OWNER = grupoOid;
    // Check whether the TextBox Contains text
    // if it does then make ajax call

    if ($("#BuscarConsumible").val().length > 3 && $("#BuscarConsumible").val() !== "") {
        $.ajax({
            type: 'POST',
            url: url,
            data: Consumbile,
            dataType: "html",
            success: function (data) {
                $('#Consumbile > tbody').html(data);
            },
        });
    }

}));

// store the currently selected tab in the hash value
$(document).on("click", ".iconVerRespuestas", function myfunction() {
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActiva.data('oid');
    //var ContenedorModalImagenes = $('#contenedorModalImagenes');
    var ContenedorRespuestas = $('#contenedorRespuestas');
    $.ajax({
        type: 'POST',
        url: '/VidSigner/ListaRespuestas',
        data: { oid: oidExploracion },
        beforeSend: function () {

            $(".spiner-cargando").removeClass('hide');
            ContenedorRespuestas.html('');
            $('#modal-form-Respuestas').modal('show');
        },
        success: function (data) {
            $(".spiner-cargando").addClass('hide');
            ContenedorRespuestas.html(data);
        }
    });
});



$(document).on("click", "#ImprimirImagen", function myfunction() {

    $(".imagenExploracion").print({
        globalStyles: true,
        mediaPrint: false,
        stylesheet: null,
        noPrintSelector: ".no-print",
        iframe: true,
        append: null,
        prepend: null,
        manuallyCopyFormValues: true,
        deferred: $.Deferred(),
        timeout: 250,
        title: null,
        doctype: '<!doctype html>'
    });
    return false;

});







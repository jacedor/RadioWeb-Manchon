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

//formateador para las cantidades del bootstrap-table
function priceSorter(a, b, rowA, rowB) {
    if (a === null) {
        a = 0;
    }
    if (b === null) {
        b = 0;
    }
    a = +a.substring(0, a.length - 5).replace(/\./g, ""); // remove $
    b = +b.substring(0, b.length - 5).replace(/\./g, "");
    if (a > b) return 1;
    if (a < b) return -1;
    return 0;
}

function dateSorter(a, b) {
    var d1 = new Date(a);
    var d2 = new Date(b);

    if (d1 < d2) return -1;
    if (d1 > d2) return 1;
    return 0;
}

//formateador para las fechas del bootstrap-table
function dateFormat(value, row, index) {
    var date = moment(value);
    if (date.isValid()) {
        return moment(value).format('DD/MM/YYYY');
    } else {
        return '';
    }

}
function accionesTrasCargarTable(scrollTablaExploraciones) {
    var l = $('#EnviarFiltros').addClass('btn-info').ladda();
    l.ladda('stop');
    var exploracionescuenta = $('#ExploracionesTable tbody tr[data-oid!=0]').length + " exploraciones";
    var huecosCuenta = $('#ExploracionesTable tbody tr[data-oid=-1]').length;
    $("#ExploracioneTable").removeClass("hide");
    var aparato = $("#FILTROS_IOR_APARATO option:selected").text();
    var oidAparato = $("#FILTROS_IOR_APARATO option:selected").val();
    $('#cuentaExploraciones').removeClass('hide');
    moment.locale('es');
    if (oidAparato > 0) {
        $('#cuentaExploraciones').html(moment($("#FILTROS_FECHA").val(), "DD/MM/YYYY").format('dddd') + ", " + $("#FILTROS_FECHA").val() + "|" + exploracionescuenta + " - " + huecosCuenta + " huecos (" + aparato + ")");

    } else {
        $('#cuentaExploraciones').html(moment($("#FILTROS_FECHA").val(), "DD/MM/YYYY").format('dddd') + ", " + $("#FILTROS_FECHA").val() + "|" + exploracionescuenta + " - " + huecosCuenta + " huecos ");

    }
    sessionStorage.vistaActual = "ViewListaDia";
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    ajustaEstadoMenuSuperior(filaActiva);
    actualizaEspera();
    var w = $(window);
    var row = $("#ExploracionesTable").find('tr.ACTIVA');
    $('.bootstrap-table').removeClass('hide');
    $('#btnHoy').removeClass('disabled').removeAttr('disabled');
    $('#btnDiaSiguiente').removeClass('disabled').removeAttr('disabled');
    $('#btnDiaAnterior').removeClass('disabled').removeAttr('disabled');
    $('.bootstrap-table').removeClass('hide');
    var el = document.querySelector('.bootstrap-table');

    // var restaAlto = (w.height() / 2) - 100;
    var restaAlto = ($('.fixed-table-body').height() / 2) - 100;
    $('.fixed-table-body').scrollTop(scrollTablaExploraciones);
    //if (row.length && el.scrollTop === 0) {
    //   // $('.fixed-table-body').scrollTop(row.offset().top - restaAlto); //animate({ scrollTop: row.offset().top - restaAlto }, 1);
    //    $('.fixed-table-body').scrollTop(scrollTablaExploraciones); //animate({ scrollTop: row.offset().top - restaAlto }, 1);
    //} 

}
function DocumentoSubido(data) {
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidExploracionSeleccionada = filaActiva.data('oid');
    if (filaActiva.length === 0) {
        oidExploracionSeleccionada = $("#IOR_EXPLORACION").val();
    }
    var ContenedorListaDocumentos = $('#modalDocsContentEntrada');
    if (ContenedorListaDocumentos.length === 0) {
        ContenedorListaDocumentos = $("#DocumentosExploracion").children('.panel-body');
    }

    $.ajax({
        type: 'GET',
        url: '/Documentos/List/' + oidExploracionSeleccionada,
        beforeSend: function () {

            ContenedorListaDocumentos.html('');
        },
        success: function (datosLista) {

            ContenedorListaDocumentos.html(datosLista);
        },
        complete: function () {
            v
        }
    });
}


function FailurePaciente(data) {
    toastr.error('', 'Error al guardar!', { timeOut: 5000 });
}
function Success(data) {
    toastr.success('Actualización!', 'Exploración modificada!', { timeOut: 5000 });
    if ($('#VOLVERTRASGUARDAREXPLORACION').val() === 'T') {
        window.location.href = $('#URLPREVIAEXPLORACION').val();
    }
}
$('form')
    .each(function () {
        $(this).data('serialized', $(this).serialize())
    })
    .on('change input', function () {

        $(this)
            .find('input[data-protect=true], button[data-protect=true]')
            .attr('disabled', $(this).serialize() === $(this).data('serialized'));
    })
    .find('input[data-protect=true], button[data-protect=true]')
    .attr('disabled', true);



function SuccessPaciente(data) {
    toastr.success('Actualización!', 'Paciente modificado', { timeOut: 5000 });
    if ($('#VOLVERTRASGUARDARPACIENTE').val() === 'T') {
        window.location.href = $('#URLPREVIAPACIENTE').val();
    }
}


function ajustaEstadoMenuSuperior(currentRow) {

    $('#btnActualizarPresencia').addClass('disabled').prop('disabled', true);//deactivamos el botón de Presencia
    $('#btnActualizarPresenciaEntrada').addClass('disabled').prop('disabled', true);//deactivamos el botón de Presencia
    $('#btnPresenciaImprimirEntrada').addClass('disabled').prop('disabled', true);//deactivamos el botón de Presencia
    $('#btnPagoRapido').addClass('disabled').prop('disabled', true);//deactivamos el botón de Presencia
    $('#btnCapturarDesdeTablet').addClass('disabled');
    $('#btnPresenciaImprimir').addClass('disabled');//deactivamos el botón de Presencia + Imprimir Ficha
    $('#btnConsumible').addClass('disabled');//deactivamos el botón de consumibles
    $('#btnConfirmar').addClass('disabled');
    $("#btnImprimir").addClass('disabled');
    $('#btnBorrar').addClass('disabled').prop('disabled', true);
    $("#btnFichaPaciente").addClass('disabled').prop('disabled', true);
    $('#btnAgregarExploracion').addClass('disabled').prop('disabled', true);
    $("#btnEtiquetas").addClass('disabled').prop('disabled', true);
    $("#btnEtiquetasMasOpciones").addClass('disabled').prop('disabled', true);
    $("#btnWebCam").addClass('disabled').prop('disabled', true);
    $("#btnVerFoto").addClass('disabled').prop('disabled', true);
    $("#btnTablet").addClass('disabled').prop('disabled', true);
    $("#btnEntrada").addClass('disabled');
    $("#btnModalJustificante").addClass('disabled').prop('disabled', true);
    $("#btnKiosko").addClass('disabled').prop('disabled', true);
    $("#btnKioskoOpciones").addClass('disabled').prop('disabled', true);
    $(currentRow).find(".textoXeditable").editable({
        container: 'body',
        inputclass: 'anchoTexto'
    });
    if (currentRow !== null && currentRow.length > 0) {

        //Ponemos la fila actual con clase activa
        if (!ctrlPressed && $(".ACTIVA").length === 1) {
            currentRow.siblings().removeClass('ACTIVA');
        }
        currentRow.addClass('ACTIVA');

        horaExploracionSeleccionada = $('#ExploracionesTable tbody tr.ACTIVA').data('hhora');
        //Es un hueco libre
        if (currentRow.data('oid') <= 0) {
            if ($('#AlertFiltro').hasClass('hidden'))//sabemos que no estamos en un dia festivo
            {
                //TO-DO $('#btnAgregarExploracion').removeClass('disabled').removeAttr('disabled');
            }
            $("#btnEntrada").addClass('disabled').prop('disabled', true);
            $('#btnActualizarPresencia').addClass('disabled').prop('disabled', true);//deactivamos el botón de Presencia
            $('#btnActualizarPresenciaEntrada').addClass('disabled').prop('disabled', true);//deactivamos el botón de Presencia
            $('#btnPresenciaImprimirEntrada').addClass('disabled').prop('disabled', true);//deactivamos el botón de Presencia

            $('#btnPagoRapido').addClass('disabled').prop('disabled', true);//deactivamos el botón de Pago Rapido
            $('#btnCapturarDesdeTablet').addClass('disabled').prop('disabled', true);
            $('#btnPresenciaImprimir').addClass('disabled');//deactivamos el botón de Presencia + Imprimir Ficha
            $('#btnConfirmar').addClass('disabled').prop('disabled', true);
            $("#btnImprimir").addClass('disabled').prop('disabled', true);
            $("#btnEtiquetas").addClass('disabled').prop('disabled', true);
            $("#btnEtiquetasMasOpciones").addClass('disabled').prop('disabled', true);
            $("#btnWebCam").addClass('disabled').prop('disabled', true);
            $("#btnVerFoto").addClass('disabled').prop('disabled', true);
            $('#btnAgregarExploracion').removeClass('disabled').removeAttr('disabled');
            //$('#btnAviso').addClass('disabled');//activamos el botón del Telefono del aviso
            $('#ColorPickerParent').find('.btn-info').addClass('disabled');//desactivamos el botón cambio de color
            $('#btnConsumible').addClass('disabled').prop('disabled', true);
            $("#btnModalJustificante").addClass('disabled').prop('disabled', true);
            //$("#btnInformes").addClass('disabled');
            $("#btnPaciente").addClass('disabled').prop('disabled', true);
            if (currentRow.data('anulada') === 'True') {
                $('#btnAgregarExploracion').addClass('disabled').prop('disabled', true);
            }
            $("#btnTablet").addClass('disabled').prop('disabled', true);
            $("#btnKiosko").addClass('disabled').prop('disabled', true);
            $("#btnKioskoOpciones").addClass('disabled').prop('disabled', true);

        }//Si es un hueco con Hora            
        else {
            $('#oidExploracionSeleccionada').val($(this).data('oid'));
            $('#btnAgregarExploracion').removeClass('disabled').removeAttr('disabled');
            $('#ColorPickerParent').find('.btn-info').removeClass('disabled').removeAttr('disabled');//activamos el botón cambio de color
            //$('#btnAviso').removeClass('disabled').removeAttr('disabled');//activamos el botón del Telefono del aviso
            //$("#btnInformes").removeClass('disabled').removeAttr('disabled');
            $("#btnPaciente").removeClass('disabled').removeAttr('disabled');
            $("#btnImprimir").removeClass('disabled').removeAttr('disabled');
            $("#btnEtiquetas").removeClass('disabled').removeAttr('disabled');
            $("#btnTablet").removeClass('disabled').removeAttr('disabled');
            $("#btnEtiquetasMasOpciones").removeClass('disabled').removeAttr('disabled');
            $("#btnFichaPaciente").removeClass('disabled').removeAttr('disabled');
            $("#btnWebCam").removeClass('disabled').removeAttr('disabled');
            $("#btnVerFoto").removeClass('disabled').removeAttr('disabled');
            $('#btnConsumible').removeClass('disabled').removeAttr('disabled');
            $("#btnEntrada").removeClass('disabled').removeAttr('disabled');
            $("#btnKiosko").removeClass('disabled').removeAttr('disabled');
            $("#btnKioskoOpciones").removeClass('disabled').removeAttr('disabled');

            //activamos el botón de consumibles
            //Si la visita se puede actualizar el estado de la exploración se activa el botón de presencia
            if ((currentRow.data('estado') === EstadosExploracion.Confirmado) || ($("#PAGAANTESCONFIRMAR").val() === "T" && currentRow.data('estado') === EstadosExploracion.Presencia)) {
                $('#btnPagoRapido').removeClass('disabled').removeAttr('disabled');//deactivamos el botón de Presencia
                $("#btnModalJustificante").removeClass('disabled').removeAttr('disabled');
            } else {
                $('#btnPagoRapido').addClass('disabled').prop('disabled', true);//deactivamos el botón de Presencia
            }

            if (currentRow.data('estado') === EstadosExploracion.Presencia || currentRow.data('estado') === EstadosExploracion.Pendiente) {
                $('#btnActualizarPresencia').removeClass('disabled').removeAttr('disabled');
                $('#btnActualizarPresenciaEntrada').removeClass('disabled').removeAttr('disabled');
                $('#btnPresenciaImprimirEntrada').removeClass('disabled').removeAttr('disabled');//deactivamos el botón de Presencia

                $('#btnPresenciaImprimir').removeClass('disabled').removeAttr('disabled');//deactivamos el botón de Presencia + Imprimir Ficha

                if (currentRow.data('estado') === EstadosExploracion.Presencia ||
                    (currentRow.data('pagado') === "false" && currentRow.data('facturada') === "false")) {
                    $('#btnConfirmar').removeClass('disabled').removeAttr('disabled');
                } else {
                    $('#btnConfirmar').addClass('disabled').prop('disabled', true);
                }
                $('#btnCapturarDesdeTablet').removeClass('disabled').removeAttr('disabled');
                if (currentRow.data('anulaconsentimiento')) {
                    $('#btnActualizarPresencia').addClass('disabled').prop('disabled', true);
                    $('#btnActualizarPresenciaEntrada').addClass('disabled').prop('disabled', true);
                    $('#btnPresenciaImprimirEntrada').addClass('disabled').prop('disabled', true);//deactivamos el botón de Presencia

                    $('#btnPresenciaImprimir').addClass('disabled').prop('disabled', true);//deactivamos el botón de Presencia + Imprimir Ficha

                }
            }
            else {
                $('#btnActualizarPresencia').addClass('disabled').prop('disabled', true);
                $('#btnActualizarPresenciaEntrada').addClass('disabled').prop('disabled', true);
                $('#btnPresenciaImprimirEntrada').addClass('disabled').prop('disabled', true);//deactivamos el botón de Presencia
                $('#btnPresenciaImprimir').addClass('disabled').prop('disabled', true);//deactivamos el botón de Presencia + Imprimir Ficha
                //si una exploracion esta confirmada pero no esta pagada se puede desconfirmar, sino no

                if (currentRow.data('pagado') === "F" && currentRow.data('facturada') === "F") {
                    $('#btnConfirmar').removeClass('disabled').removeAttr('disabled');
                } else {
                    $('#btnConfirmar').addClass('disabled').prop('disabled', true);
                }
            }
            if (currentRow.data('firmada')) {
                $("#btnTablet").addClass('disabled').prop('disabled', true);
            }

            if (currentRow.data('estado') === EstadosExploracion.Pendiente || currentRow.data('estado') === EstadosExploracion.Borrado || currentRow.data('estado') === EstadosExploracion.NoPresentado || currentRow.data('estado') === EstadosExploracion.LlamaAnulando) {
                $('#btnBorrar').removeClass('disabled').removeAttr('disabled');
                $('#btnBorrarMasOpciones').removeClass('disabled').removeAttr('disabled');
            }
            else {
                $('#btnBorrar').addClass('disabled').prop('disabled', true);
                $('#btnBorrarMasOpciones').addClass('disabled').prop('disabled', true);
            }
        }
    }



}

$(document).on("click", ".firmarDocumento", function (e) {

    var button = $(this);
    var oEnviarTablet = {};
    var oidDocumento = button.data('oiddocumento');
    oEnviarTablet.OIDEXPLORACION = button.data('oidexploracion');
    oEnviarTablet.DocumentSelected = oidDocumento;
    oEnviarTablet.DeviceSelected = button.attr('id');
    oEnviarTablet.ESDOCUMENTO = true;
    button.addClass('disabled').text('Enviando');
    var request = $.ajax({
        url: "/api/VidSigner",
        type: "POST",
        contentType: "application/json",
        data: JSON.stringify(oEnviarTablet),
    });

    request.done(function (data) {
        toastr.success('Documento enviado a firmar', '', { timeOut: 5000 });
        button.removeClass('disabled').text('Enviar');
    });

    request.error(function (xhr, status, errorThrown) {
        toastr.error('Error al enviar el documento a Firmar', xhr.responseText, { timeOut: 5000 });
        button.removeClass('disabled').text('Enviar');
    });
});



function exportExcel(type, id, name, fn, dl) {
    var elt = document.getElementById(id);

    var wb = XLSX.utils.table_to_book(elt, { sheet: "Sheet JS", dateNF: "dd/mm/yyyy" });
    return dl ?
        XLSX.write(wb, { bookType: type, bookSST: true, type: 'base64' }) :
        XLSX.writeFile(wb, fn || (name + '.' + (type || 'xlsx')));
}
function getParameterByName(name) {
    var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
    return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
}

function replaceSpecialCharacters(myid) {

    return "#" + myid.replace(/(:|\.|\[|\]|,|=|@)/g, "\\$1");

}

$(document).on('shown.bs.popover', function (e) {
    if (!$(e.target).hasClass("textobolsa")) {
        $("#ExploracionesMiniTable").parents().find(".popover").css("min-width", "1200px");
        $("#ExploracionesMiniTable").parents().find(".popover-content").css("min-width", "1200px");
        $("#ExploracionesMiniTable").bootstrapTable();

        $("#PeticionesMiniTable").parents().find(".popover").css("min-width", "1300px");
        $("#PeticionesMiniTable").parents().find(".popover-content").css("min-width", "1300px");
        $("#PeticionesMiniTable").bootstrapTable();

        $('.textobolsa').editable();
    }

});


//mapeamos el evento click de cada una de las filas de la tabla de exploraciones de dentro del carrito que permite trasladar exploraciones
$(document).on('click', '#cerrarCarrito', function () {
    $('.popover').hide();
});

function eliminarDelCarrito(oid) {

    $.ajax({
        type: 'POST',
        url: '/Settings/RemoveExploracion',
        data: { oid: oid },
        success: function (data) {
            loadExploracionesPersonales();
        }
    });

}

function eliminarDePeticiones(oid) {



    cambiarEstadoExploracion(EstadosExploracion.Pendiente, EstadosExploracion.Borrado, oid, "09:00");


}

function loadPeticiones() {
    //$("#ExploracionesMiniTable").remove();
    $.ajax({
        type: 'GET',
        url: '/Settings/LoadPeticiones',
        success: function (data) {
            if (data) {
                $("#CarritoPeticiones").attr('data-content', '');
                $("#CarritoPeticiones").attr('data-content', data).data('bs.popover').setContent();
                $("#PeticionesPersonalesCuenta").html($(data).find("tbody tr.huecoOcupado").length);
                if ($(data).find("tr").length === 0) {
                    $('.popover').hide();

                } else {
                    $("#PeticionesMiniTable").parents().find(".popover").css("min-width", "1200px");
                    $("#PeticionesMiniTable").parents().find(".popover-content").css("min-width", "1200px");
                }
                $('.ui-popover').popover();
            }


        }, complete: function () {


            $("#PeticionesMiniTable").parents().find(".popover").css("min-width", "1200px");
            $("#PeticionesMiniTable").parents().find(".popover-content").css("min-width", "1200px");

            // $("#ExploracionesMiniTable").bootstrapTable();
        }
    });

}
function loadExploracionesPersonales() {
    //$("#ExploracionesMiniTable").remove();
    $.ajax({
        type: 'GET',
        url: '/Settings/LoadExploraciones',
        success: function (data) {

            $("#CarritoCitas").attr('data-content', '');
            $("#CarritoCitas").attr('data-content', data).data('bs.popover').setContent();
            $("#ExploracionesPersonalesCuenta").html($(data).find("tbody tr.huecoOcupado").length);
            if ($(data).find("tr").length === 0) {
                $('.popover').hide();

            } else {
                $("#ExploracionesMiniTable").parents().find(".popover").css("min-width", "1200px");
                $("#ExploracionesMiniTable").parents().find(".popover-content").css("min-width", "1200px");
            }
            $('.ui-popover').popover();

        }, complete: function () {


            $("#ExploracionesMiniTable").parents().find(".popover").css("min-width", "1200px");
            $("#ExploracionesMiniTable").parents().find(".popover-content").css("min-width", "1200px");

            // $("#ExploracionesMiniTable").bootstrapTable();
        }
    });

}


function getFiltrosBusqueda() {
    var Aparato = ($("#ddlAparatos option[value=" + $("#ddlAparatos").val() + "]").text() || -1);
    var Mutua = ($("#ddlMutuas option[value=" + $("#ddlMutuas").val() + "]").text().trim() || -1);
    var oFiltrosBusqueda = {};
    oFiltrosBusqueda.Fecha = $("#fechaSelect").val();
    oFiltrosBusqueda.Hora = $("#ExploracionesTable* .ACTIVA").data('hhora');
    oFiltrosBusqueda.oidMutua = $("#ddlMutuas").val();
    oFiltrosBusqueda.oidGrupoAparato = $("#ddlGrupo").val();
    oFiltrosBusqueda.oidAparato = $("#ddlAparatos").val();
    oFiltrosBusqueda.oidCentro = $("#ddlCentros").val();
    oFiltrosBusqueda.oidExploracion = $("#ddlTipoExploracion").val();
    oFiltrosBusqueda.DescAparato = Aparato;
    oFiltrosBusqueda.DescMutua = Mutua;
    oFiltrosBusqueda.Paciente = $("#txtPaciente").val().toUpperCase();
    oFiltrosBusqueda.oidMedicoInformante = $("#IOR_MEDICOINFORMANTE").val();
    var elem = document.querySelector('#chkBorrados');
    oFiltrosBusqueda.Borrados = elem.checked;
    var elemHuecos = document.querySelector('#chkHuecos');
    oFiltrosBusqueda.SoloHuecos = elemHuecos.checked;
    oFiltrosBusqueda.OrderField = ($("#orderField").val() || 'HORA');
    oFiltrosBusqueda.OrderDirection = ($("#OrderDirection").val() || 'ASC');

    //alert( oFiltrosBusqueda.SoloHuecos);
    return oFiltrosBusqueda;
}

function isValidEmailAddress(emailAddress) {
    var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    return re.test(emailAddress);
}


function SaveFiltersFueraCalendario(filtros) {

    $.ajax({
        type: 'POST',
        url: '/Settings/SaveFiltros',
        data: JSON.stringify(filtros),
        contentType: 'application/json; charset=utf-8',
        complete: function () {
            sessionStorage.vistaActual = "ViewListaDia"
            window.location.href = "/Home/Index";
        }
    });
}

function addDays(date, days) {
    var strDate = date;
    var formatoGuiones = true;
    var dateParts = strDate.split("-");
    if (dateParts.length === 1) {
        dateParts = strDate.split("/");
        formatoGuiones = false;
    }
    var result = new Date(dateParts[2], parseInt(dateParts[1]) - 1, dateParts[0]);
    result.setDate(result.getDate() + days);
    if (formatoGuiones) {
        return moment(result).format('DD-MM-YYYY');
    }
    else {
        return moment(result).format('DD/MM/YYYY');
    }

}

function ShowActividad(data) {
    $("#ultimasExplos").html('');
    $("#ultimasExplos").append(data);
}

function saveURLRetorno(url) {
    $.ajax({
        type: 'POST',
        url: '/Settings/SaveUrlRetono',
        data: { url: url }
    });
}

function loadURLRetorno() {
    $.get('/Settings/LoadUrlRetorno', function (data) {
        window.location.href = data;
    });
}

function saveActividad(Actividad) {
    $.post('/Settings/SaveActividad', Actividad, ShowActividad);
}


function waitForElementToDisplay(selector, time, popUp) {
    //El fast report lo obtnemos de hacer un Get que nos devuelve el server
    //el html, el report lo ponemos en el popup (tercer parametro) y este contiene un elemento que es el id del report
    //el selector... cuando llegamos a esta función aun no se ha descargado por completo el server, por lo cuall vamos haciendo
    //llamadas recurvisa hasta que existe en idReport. Una vez existe redicreccionamos el popup al Hadndler que exporta en formato
    //para imprimir en HTML
    if (popUp.document.querySelector(selector) !== null) {
        popUp.location.href = '/FastReport.Export.axd?previewobject=' + $(popUp.document.querySelector(selector)).val() + '&print_browser=1&s=7702';
        var document_focus = false; // var we use to monitor document focused status.
        // Now our event handlers.
        $(document).ready(function () { document_focus = true; });
        setInterval(function () {
            if (document_focus === true) {
                if (popUp) {
                    if (popUp.window) {
                        if ($("#UserLogged").html() !== "Admin") {
                            popUp.window.close();
                        }
                    }
                }
            }
        }, 600);
        return;
    }
    else {
        setTimeout(function () {
            waitForElementToDisplay(selector, time, popUp);
        }, time);
    }
}

function imprimirJustificante(oid, horaLlegada, horaRealizada, textoLibre) {
    var urlServer = '/Exploracion/ImprimirJustificante/' + oid;
    var newWin = null;

    $.ajax({
        type: "POST",
        url: urlServer,
        data: {
            "oid": oid,
            "horaLlegada": horaLlegada,
            "horaRealizada": horaRealizada,
            "textoJustificante": textoLibre
        },
        success: function (data, textStatus, xhr) {
            if ($('#NombreEmpresaGlobal').val().toUpperCase().indexOf('DELFOS') > 0) {
                newWin = window.open('/PDF/' + xhr.statusText, 'popup', 'width=900,height=500');
            } else {
                newWin = window.open('', 'popup', 'width=900,height=500');
                newWin.document.write('');
                newWin.document.write(data);
            }


        },
        complete: function () {

            var idReport = waitForElementToDisplay("#idReport", 3000, newWin);
        },
        error: function () {
            alert('Error al imprimir');
        }

    });

}


function reportPath(oidExploracion, oidTipoDocumento) {
    var urlServer = '/Exploracion/Imprimir/' + oidExploracion + "?oidTipoDocumento=" + oidTipoDocumento;

    //Si es el radioWeb de Delfos vamos a mirar si ya hemos impreso la hoja de recogida
    if ($('#NombreEmpresaGlobal').val().toUpperCase().indexOf('DELFOS') > 0) {
        //Lo primero que hacemos es mirar si ya existe este documento en el servidor
        //suponiendo que sea un documento de los que se guardan
        $.ajax({
            type: "GET",
            url: '/Documentos/Buscar?oidExploracion=' + oidExploracion + '&tipoDocumento=' + oidTipoDocumento,
            success: function (data, textStatus, xhr) {

                if (xhr.statusText === "FIRMADO") {
                    toastr.info('Este documento ya fue firmado. ', '', { timeOut: 3000 });
                    $("#btnDocs").trigger('click');
                } else {
                    swal({
                        title: "Este documento ya existe. Generar de nuevo? ",
                        type: "warning",
                        showCancelButton: true,
                        confirmButtonColor: "#DD6B55",
                        confirmButtonText: "Si",
                        cancelButtonText: "No",
                        closeOnConfirm: true
                    }, function (isConfirm) {
                        if (isConfirm) {
                            var oidDocumento = xhr.statusText;
                            $.ajax({
                                type: "DELETE",
                                url: '/Documentos/Delete/' + oidDocumento,
                                success: function (data) {
                                    imprimirDocumento(oidExploracion, oidTipoDocumento);
                                }
                            });

                        } else {

                            $("#btnDocs").trigger('click');
                        }
                    });

                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                switch (xhr.status) {
                    case 404:
                        var error = false;
                        //consentimiento Qreport
                        if (oidTipoDocumento === 18) {
                            if ($("#EMAIL").val() === "") {
                                toastr.error('Campo email obligatorio', '', { timeOut: 5000 });
                                error = true;
                            }
                            var telefonoMovil;
                            var telefonos = $("#listaTelefonos tr").find("[id*='.NUMERO']");
                            if (telefonos.length === 0) {
                                if ($(".telefonoPaciente").val().length !== 9 || !$(".telefonoPaciente").val().startsWith("6")) {
                                    toastr.error('Campo telefono movil incorrecto', '', { timeOut: 5000 });
                                    error = true;
                                }
                            } else {
                                telefonos.each(function (i, row) {
                                    telefonoMovil = $(row).val();
                                });
                            }



                        }

                        if (!error) {
                            var newWin = null;
                            newWin = window.open(urlServer, 'popup', 'width=900,height=500');
                        }
                }
            }
        });
    } else {
        var newWin = null;
        newWin = window.open(urlServer, 'popup', 'width=900,height=500');

    }
}




function imprimirExploracion(oid) {
    var urlServer = '/Exploracion/Imprimir/' + oid;// + "?recordatorioCita=true";
    var newWin = null;
    if ($('#NombreEmpresaGlobal').val().toUpperCase().indexOf('DELFOS') > 0) {
        //  newWin = window.open(urlServer, 'popup', 'width=900,height=500');
        //el oid 19 es el tipo de documento de la tabla refractometros
        imprimirDocumento(oid, 19);
    } else {
        $.ajax({
            type: "GET",
            url: urlServer,
            success: function (data) {
                newWin = window.open('', 'popup', 'width=900,height=500');
                newWin.document.write('');
                newWin.document.write(data);
            },
            complete: function () {
                var idReport = waitForElementToDisplay("#idReport", 3000, newWin);
            },
            error: function () {
                alert('Error al imprimir');
            }
        });
    }
}

var sess_pollInterval = 60000;
var sess_expirationMinutes = 30;
var sess_warningMinutes = 25;
var sess_intervalID;
var sess_lastActivity;

function initSession() {
    sess_lastActivity = new Date();
    sessSetInterval();
    $(document).bind('keypress.session', function (ed, e) {
        sessKeyPressed(ed, e);
    });
}
function sessInterval() {
    var now = new Date();
    //get milliseconds of differneces
    var diff = now - sess_lastActivity;
    //get minutes between differences
    var diffMins = (diff / 1000 / 60);
    console.log("expire en" + sess_expirationMinutes + "inactividad" + (diffMins - sess_expirationMinutes));
    if (diffMins >= sess_warningMinutes) {
        //wran before expiring
        //stop the timer
        sessClearInterval();
        //promt for attention
        var active = confirm('Su sesion expirará en  ' + (sess_expirationMinutes - sess_warningMinutes) +
            ' minutos (ultima actividad ' + sess_lastActivity.toTimeString() + '), presione OK para continuar logeado ' +
            'o presione Cancelar para Salir. \nSi sales cualquier modificación se perderá.');
        if (active === true) {
            now = new Date();
            diff = now - sess_lastActivity;
            diffMins = (diff / 1000 / 60);
            if (diffMins > sess_expirationMinutes) {
                sessLogOut();
            }
            else {
                initSession();
                sessSetInterval();
                sess_lastActivity = new Date();
            }
        }
        else {
            sessLogOut();
        }
    }
}
function sessSetInterval() {
    sess_intervalID = setInterval('sessInterval()', sess_pollInterval);
}
function sessClearInterval() {
    clearInterval(sess_intervalID);

}
function sessKeyPressed(ed, e) {
    sess_lastActivity = new Date();
}
function sessLogOut() {
    window.location.href = '/Users/Index';
}

Number.prototype.padLeft = function (n, str) {
    return (this < 0 ? '-' : '') +
        Array(n - String(Math.abs(this)).length + 1)
            .join(str || '0') +
        (Math.abs(this));
}

function time_diff(t1, t2) {
    var t1parts = t1.split(':');
    var t1cm = Number(t1parts[0]) * 60 + Number(t1parts[1]);

    var t2parts = t2.split(':');
    var t2cm = Number(t2parts[0]) * 60 + Number(t2parts[1]);

    var hour = Math.floor((t1cm - t2cm) / 60);
    var min = Math.floor((t1cm - t2cm) % 60);
    return (hour.padLeft(2) + ':' + min.padLeft(2));
}

function isNumber(evt, element) {

    var charCode = (evt.which) ? evt.which : event.keyCode

    if (
        (charCode !== 45 || $(element).val().indexOf('-') !== -1) &&      // “-” CHECK MINUS, AND ONLY ONE.
        (charCode !== 46 || $(element).val().indexOf('.') !== -1) &&      // “.” CHECK DOT, AND ONLY ONE.
        (charCode < 48 || charCode > 57))
        return false;

    return true;
}

function makeBootstrapTable(idTable) {

    if ($("#context-menu").length > 0) {

        //Esta funcion enlaza en el listadia el plugin de menu contextual        
        $('#' + idTable).bootstrapTable({
            contextMenu: '#context-menu',
            onContextMenuItem: function (row, $el) {

                if ($el.data("item") === "vincularaHL7") {

                    var filaActiva = $('#' + idTable + " tbody tr.ACTIVA");
                    var oidExploracion = filaActiva.data('oid');
                    var url = "/Exploracion/ListaBadalonaPendienteCitar/" + oidExploracion; //The Url to the Action  Method of the Controller

                    $.ajax({
                        type: 'GET',
                        url: url,
                        dataType: "html",
                        success: function (evt) {
                            $('#modal-form-vincular').remove();
                            $("body").append(evt);
                            $('#modal-form-vincular').modal('show');

                        },
                        complete: function () {
                            $("#ExploracionesVinculablesHL7").bootstrapTable();
                        }
                    });
                }
                if ($el.data("item") === "desvincularMaster") {

                    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
                    var oidExploracion = filaActiva.data('oid');
                    var url = "/Exploracion/DesvincularHija/" + oidExploracion; //The Url to the Action  Method of the Controller

                    $.ajax({
                        type: 'GET',
                        url: url,
                        dataType: "html",
                        success: function (evt) {
                            $('#modal-form-desvincular').remove();
                            $("body").append(evt);
                            $('#modal-form-desvincular').modal('show');

                        },
                    });

                }

                if ($el.data("item") === "entradaCarrito") {
                    $("#btnEntrada").trigger("click");
                }
                //Agregar Exploracion al carrito                    
                if ($el.data("item") === "carrito") {
                    var filaActiva = $('#' + idTable + ' tbody tr.ACTIVA');
                    var oidExploracion = filaActiva.data('oid');
                    if ($("#ExploracionesMiniTable tbody tr[data-oid=" + oidExploracion + "]").length !== 0) {
                        toastr.warning('Carrito', 'La exploracion ya está en el carrito', { timeOut: 5000 });
                    } else {

                        $.ajax({
                            type: 'POST',
                            url: '/Settings/SaveExploracion',
                            data: { oid: oidExploracion },
                            success: function (data) {
                                loadExploracionesPersonales();
                            }
                        });
                    }
                }

                //Agregar todas las exploraciones Exploracion al carrito
                if ($el.data("item") === "irACalendario") {
                    var filaActiva = $("#ExploracionesList tbody tr.ACTIVA");
                    window.location.href = "/Home/Index?fecha=" + filaActiva.data("fecha")
                        + "&oidExploracion=" + filaActiva.data("oid");
                }

                //Agregar todas las exploraciones Exploracion al carrito
                if ($el.data("item") === "carritoTodas") {
                    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
                    var oidExploracion = filaActiva.data('oid');

                    if ($("#ExploracionesMiniTable tbody tr[data-oid=" + oidExploracion + "]").length !== 0) {
                        toastr.warning('Carrito', 'La exploracion ya está en el carrito', { timeOut: 5000 });
                    } else {
                        $.ajax({
                            type: 'POST',
                            url: '/Settings/SaveExploracion',
                            data: { oid: oidExploracion, todasDelDia: 'true' },
                            success: function (data) {
                                loadExploracionesPersonales();
                            },
                            complete: function () {


                            }
                        });

                    }

                }
                if ($el.data("item") === "enviarQreport") {
                    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
                    var oidExploracion = filaActiva.data('oid');
                    if (filaActiva.data('informada') === "False") {
                        toastr.error('Imposible enviar a QReport. Exploración no informada.', 'QReport', { timeOut: 5000 });
                    } else {
                        var url = "/Informe/Qreport?oid=" + oidExploracion + "&esOidExploracion=true"; //T
                        $.ajax({
                            url: url,
                            type: 'GET',
                            success: function (data) {

                                if (data.success) {
                                    toastr.success('Enviado a QReport ', 'QReport', { timeOut: 5000 });
                                }
                                else {
                                    if (data.message != "") {
                                        toastr.error(data.message, 'QReport', { timeOut: 5000 });
                                    }
                                    else {
                                        toastr.error('Imposible enviar a QReport ', 'QReport', { timeOut: 5000 });
                                    }
                                }
                            },
                            error: function (x, y, z) {
                                toastr.error('Imposible enviar a QReport ', 'QReport', { timeOut: 5000 });
                            }
                        });

                    }
                }
                if ($el.data("item") === "enviarSMS") {
                    var filaActiva = $("#" + idTable + " tbody tr.ACTIVA");
                    var oidExploracion = filaActiva.data('oid');
                    $.ajax({
                        type: 'GET',
                        url: '/Telefono/GetMovilPacienteFromExploracion?oidExploracion=' + oidExploracion,
                        success: function (data) {
                            $("#movilEnvioSMS").val(data);
                        }
                    });

                }

                if ($el.data("item") === "tiempoEspera") {

                    var rowActiva = $("#" + idTable + " tbody tr[data-oid=" + row.oid + "]");
                    $("#HoraProgramada").html(rowActiva.data('hora'));
                    $("#tiempoEspera").html(rowActiva.data('espera'));
                    $("#HoraLLegada").html(rowActiva.data('horall'));
                    $("#HoraRealizada").html(rowActiva.data('horaex'));
                }
                //al hacer boton derecho sobre una fila la activamos desactivando previamente el resto
                if ($el.data("item") === "imprimirLista") {

                    ImprimirListaDia();
                }

                if ($el.data("item") === "visoresQreport") {
                    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
                    var oidExploracion = $("#ExploracionesTable tbody tr.ACTIVA").data("oid");

                    if (filaActiva.data('informada') === "F") {
                        toastr.error('Imposible mostrar los visores QReport. Exploración no informada.', 'QReport', { timeOut: 5000 });
                    } else {
                        var url = "/Informe/comprobarEstadoInformeQReport?oid=" + oidExploracion;
                        $.ajax({
                            url: url,
                            type: 'GET',
                            success: function (data) {
                                if (data.success) {
                                    $('#modalVisoresQreport').modal('show');
                                }
                                else {
                                    toastr.error(data.message, 'Visor QReport', { timeOut: 5000 });
                                }
                            },
                            error: function (x, y, z) {
                                toastr.error('Imposible abrir el visor de QReport', 'Visor QReport', { timeOut: 5000 });
                            }
                        });
                    }
                }

                return false;
            }, onClickRow: function (row, $el) {
                //Ponemos la fila actual con clase activa
                if (!ctrlPressed) {

                    $('#' + idTable).find('.ACTIVA').removeClass('ACTIVA');
                    $el.addClass('ACTIVA');
                    $(".agendarPeticion").removeClass('disabled');
                    // ajustaEstadoMenuSuperior(('#' + idTable).find('.ACTIVA'));
                }

                var filaActiva = $('#' + idTable + " tbody tr.ACTIVA");
                var oidMaster = filaActiva.data('ior_master');
                var oidMutua = filaActiva.data('mutua');


                if (oidMaster > 0) {
                    $("li[data-item='desvincularMaster']").removeClass('hide');
                } else {
                    $("li[data-item='desvincularMaster']").addClass('hide');
                }
                if (oidMutua === 11042 && oidMaster < 0) {
                    $("li[data-item='vincularaHL7']").removeClass('hide');
                } else {
                    $("li[data-item='vincularaHL7']").addClass('hide');
                }

            }

        });
    }
    else {

        $('#' + idTable).bootstrapTable();

    }

}


//***************FIN FUNCIONES*********************************
$(document).on("shown.bs.tab", "a[data-toggle='tab']", function (e) {

    var target = $(e.target).attr("href") // activated tab
    switch (target) {
        case "#Inyectables":
            var oidExploracion = $(target).data('oid');
            var ContenedorModalConsumbile = $(target).children('.panel-body');
            $.ajax({
                type: 'POST',
                url: '/Consumible/ListAsignados/' + oidExploracion,
                beforeSend: function () {
                    $(".spiner-cargando").removeClass('hide');
                    ContenedorModalConsumbile.html('');

                },
                success: function (data) {

                    ContenedorModalConsumbile.html(data);
                    $('.textoXeditable').editable();
                    $('#HORA .editable').editable({
                        type: 'text',
                        name: 'hora',
                        tpl: '<input type="text" id ="HoraDosis" placeholder="HH:mm" class="form-control input-sm" data-mask="99:99" style="padding-right: 24px;"> (Hora Dosis)',
                        validate: function (value) {
                            var validTime = value.match(/^([01]?[0-9]|2[0-3]):[0-5][0-9]$/);
                            if (!validTime) {
                                return 'Hora incorrecta';
                            }
                        }
                    }).on('shown', function () {
                        $(".editable-buttons").remove("#HoraActualConsumible").append('<button id="HoraActualConsumible" type="button" class="btn btn-warning btn-sm editable-submit" title="Hora Actual"><i title="Hora Actual" class="glyphicon glyphicon-time"></i></button>');
                        $(".editable-buttons").remove("#NotaHora").append('<span id="NotaHora" style="font-weight:bold;margin-left:5px;color:red;">Hora Dosis</span>');
                    });

                    $('#mci .editable').editable({
                        type: 'text',
                        name: 'mci',
                        tpl: '<input type="text" id ="mciDemo" placeholder="Dosis en mCi" class="form-control input-sm numericoDecimal"  style="padding-right: 24px;"> (Dosis en Mci)',
                    }).on('shown', function () {
                        $(".editable-buttons").remove("#Notamci")
                            .append('<span id="Notamci" style="font-weight:bold;margin-left:5px;color:red;">Dosis en mCi</span>');
                    });

                    $('#DLP .editable').editable({
                        type: 'text',
                        name: 'DLP',
                        tpl: '<input type="text" id ="DLPDemo" placeholder="Dosis en mGy/cm" class="form-control input-sm numericoDecimal"  style="padding-right: 24px;"> (Dosis en mGy/cm)'
                    }).on('shown', function () {
                        $(".editable-buttons").remove("#NotaDlp").append('<span id="NotaDlp" style="font-weight:bold;margin-left:5px;color:red;">Dosis en mGy/cm</span>');
                    });

                    $('#OWNER .tecnicoconsumible').editable({
                        showbuttons: true
                    });   

                }
            });
            break;
        case "#OtrasExploraciones":
            var oidPaciente = $(target).data('iorpaciente');
            var url = "/Exploracion/ListaExploracionesPaciente/" + oidPaciente; //The Url to the Action  Method of the Controller

            $.ajax({
                type: 'GET',
                url: url,
                dataType: "html",
                success: function (data) {

                    $(target).children('.panel-body').html('').append(data);

                }, complete: function () {
                    makeBootstrapTable("ExploracionesList");


                }
            });
            break;
        case "#historiaClinica":
            var oidPaciente = $(target).data('iorpaciente');
            var url = "/HistoriaClinica/IndexNew/" + oidPaciente; //The Url to the Action  Method of the Controller

            $.ajax({
                type: 'GET',
                url: url,
                dataType: "html",
                success: function (evt) {

                    $(target).children('.panel-body').html('').append(evt);

                }, complete: function () {
                    $('#TEXTOHTML').summernote({
                        tabsize: 2,
                        height: 200,
                        lang: 'es-ES',
                        onInit: function () {
                            $("#summernote").summernote('code', '<p style="font-family: Verdana;"><br></p>')
                        }
                    });
                }
            });
            break;
        case "#DocumentosExploracion":
            var oidExploracion = $(target).data('oid');

            var ContenedorListaDocumentos = $(target).children('.panel-body');

            $.ajax({
                type: 'GET',
                url: '/Documentos/List/' + oidExploracion,
                beforeSend: function () {

                    ContenedorListaDocumentos.html('');
                },
                success: function (datosLista) {

                    ContenedorListaDocumentos.html(datosLista);
                },
                complete: function () {
                    makeBootstrapTable("tblDocumentosExploracion");
                }
            });
            break;
        case "#ListInformesExploracion":
            var oidExploracion = $(target).data('oid');

            var ContenedorListaInformes = $(target).children('.panel-body');

            $.ajax({
                type: 'GET',
                url: '/Informe/ListaExploracion/' + oidExploracion,
                beforeSend: function () {

                    ContenedorListaInformes.html('');
                },
                success: function (datosLista) {

                    ContenedorListaInformes.html(datosLista);
                },
                complete: function () {
                    makeBootstrapTable("InformesList");
                }
            });
            break;
        case "#ListInformesPaciente":
            var oidPaciente = $(target).data('oid');

            var ContenedorListaInformes = $(target).children('.panel-body');

            $.ajax({
                type: 'GET',
                url: '/Informe/ListaPaciente?oidPaciente=' + oidPaciente,
                beforeSend: function () {

                    ContenedorListaInformes.html('');
                },
                success: function (datosLista) {

                    ContenedorListaInformes.html(datosLista);
                },
                complete: function () {
                    makeBootstrapTable("InformesList");
                }
            });
            break;
        case "#ListDocumentosPaciente":
            var oidPaciente = $(target).data('oid');

            var ContenedorListaDocumentos = $(target).children('.panel-body');

            $.ajax({
                type: 'GET',
                url: '/Documentos/ListaPaciente/' + oidPaciente,
                beforeSend: function () {

                    ContenedorListaDocumentos.html('');
                },
                success: function (datosLista) {

                    ContenedorListaDocumentos.html(datosLista);
                },
                complete: function () {
                    makeBootstrapTable("tblDocumentosExploracion");
                }
            });
            break;
        default:

    }
});
$(document).on("keypress", ".numericoDecimal", function () {
    return isNumber(event, this);
});



$(document).on("click", "#HoraActualConsumible", function () {
    $("#HoraDosis").val(moment().format('HH:mm'));
});

$(document).on("click", ".generarPDFSinClave", function () {
    var oidInforme = $(this).data('oid');
    var url = "/Informe/Imprimir?oid=" + oidInforme; //The Url to the Action  Method of the Controller
    window.open(url, 'popup', 'width=900,height=500');
    return false;
});

$(document).on('click', '#ExploracionesList tbody tr', function () {
    ajustaEstadoMenuSuperior($(this));
    if (ctrlPressed) {
        // do something
        if ($(this).hasClass('ACTIVA')) {
            $(this).removeClass('ACTIVA');
        } else {
            $(this).addClass('ACTIVA');
            if ($(this).hasClass('huecoOcupado')) {
                $("#FILTROS_OIDACTIVA").val($(this).data('oid'));
            }
        }
    } else {
        $(this).siblings().removeClass('ACTIVA');
        $(this).addClass('ACTIVA');
        if ($(this).hasClass('huecoOcupado')) {
            $("#FILTROS_OIDACTIVA").val($(this).data('oid'));

        }
    }

});


$(document).on("shown.bs.collapse", ".panel-collapse[data-controlador]", function () {

    var icono = $(this);
    var controller = icono.data('controlador');
    var objeto = icono.data('objeto');
    var collapsed = true;

    $.ajax({
        type: 'POST',
        url: '/Settings/Visual',
        data: {
            controlador: controller,
            objeto: objeto,
            collapsed: collapsed
        },
        success: function (data) {

        }
    });
});

$(document).on("hidden.bs.collapse", ".panel-collapse[data-controlador]", function () {

    var icono = $(this);
    var controller = icono.data('controlador');
    var objeto = icono.data('objeto');
    var collapsed = false;

    $.ajax({
        type: 'POST',
        url: '/Settings/Visual',
        data: {
            controlador: controller,
            objeto: objeto,
            collapsed: collapsed
        },
        success: function (data) {

        }
    });
});

$(document).on('keyup', '.codigoPostal', $.debounce(500, function () {

    if ($(this).val().length === 5) {
        var cp = $(this).val();
        var nameCampoCP = $(this).attr('name');
        var idSelectorCampoProvincia = replaceSpecialCharacters(nameCampoCP.replace('CP', 'PROVINCIA'));
        var idSelectorCampoPoblacion = replaceSpecialCharacters(nameCampoCP.replace('CP', 'POBLACION'));
        $(idSelectorCampoPoblacion).val('');
        $(idSelectorCampoProvincia).val('');
        $.ajax({
            url: "https://api.zippopotam.us/es/" + $(this).val(),
            cache: false,
            dataType: "json",
            type: "GET",
            success: function (result, success) {

                // ES Post Code Returns multiple location
                cuidad = [];
                set = {};
                provincia = [];

                // Copy cities and all possible states over
                for (ii in result['places']) {
                    cuidad.push(result['places'][ii]['place name']);

                    // Get only unique values
                    state = result['places'][ii]['state'];
                    if (!(state in set)) {
                        set[state] = true;
                        provincia.push(state);
                    }
                }


                if (result['places'].length > 0) {
                    $(idSelectorCampoPoblacion).val(cuidad[0].toUpperCase());
                    var provincia = "";
                    switch (cp.substring(0, 2)) {
                        case "08":
                            provincia = "BARCELONA";
                            break;
                        case "25":
                            provincia = "LLEIDA";
                            break;
                        case "17":
                            provincia = "GIRONA";
                            break;
                        case "43":
                            provincia = "TARRAGONA";
                            break;
                        default:

                    }
                    $(idSelectorCampoProvincia).val(provincia);

                }

            },
            error: function (result, success) {
                ;
            }
        });

    }


}));

$(document).on("show.bs.modal", "#modal-form-consumibles", function myfunction() {

    var filaActivaEntrada = $("#ExploracionesTable* .ACTIVA");

    var oidExploracion = filaActivaEntrada.data('oid');

    if (!oidExploracion) {
        oidExploracion = $("#OID").val();
    }
    var ContenedorModalConsumbile = $('#listaConsumiblesExplo');
    $.ajax({
        type: 'POST',
        url: '/Consumible/ListAsignados/' + oidExploracion,
        beforeSend: function () {
            $(".spiner-cargando").removeClass('hide');
            ContenedorModalConsumbile.html('');

        },
        success: function (data) {
            $(".spiner-cargando").addClass('hide');
            ContenedorModalConsumbile.html(data);
            $('.textoXeditable').editable();
            $('#HORA .editable').editable({
                type: 'text',
                name: 'hora',
                tpl: '<input type="text" id ="HoraDosis" placeholder="HH:mm" class="form-control input-sm" data-mask="99:99" style="padding-right: 24px;"> (Hora Dosis)',
                validate: function (value) {
                    var validTime = value.match(/^([01]?[0-9]|2[0-3]):[0-5][0-9]$/);
                    if (!validTime) {
                        return 'Hora incorrecta';
                    }
                }
            }).on('shown', function () {
                $(".editable-buttons").remove("#HoraActualConsumible").append('<button id="HoraActualConsumible" type="button" class="btn btn-warning btn-sm editable-submit" title="Hora Actual"><i title="Hora Actual" class="glyphicon glyphicon-time"></i></button>');
                $(".editable-buttons").remove("#NotaHora").append('<span id="NotaHora" style="font-weight:bold;margin-left:5px;color:red;">Hora Dosis</span>');
            });

            $('#mci .editable').editable({
                type: 'text',
                name: 'mci',
                tpl: '<input type="text" id ="mciDemo" placeholder="Dosis en mCi" class="form-control input-sm numericoDecimal"  style="padding-right: 24px;"> (Dosis en Mci)',
            }).on('shown', function () {
                $(".editable-buttons").remove("#Notamci")
                    .append('<span id="Notamci" style="font-weight:bold;margin-left:5px;color:red;">Dosis en mCi</span>');
            });

            $('#DLP .editable').editable({
                type: 'text',
                name: 'DLP',
                tpl: '<input type="text" id ="DLPDemo" placeholder="Dosis en mGy/cm" class="form-control input-sm numericoDecimal"  style="padding-right: 24px;"> (Dosis en mGy/cm)'
            }).on('shown', function () {
                $(".editable-buttons").remove("#NotaDlp").append('<span id="NotaDlp" style="font-weight:bold;margin-left:5px;color:red;">Dosis en mGy/cm</span>');
            });

            $('#OWNER .tecnicoconsumible').editable({
                showbuttons: true
            });   
        }

    });



});



$(document).on("click", "#btnTablet", function myfunction() {
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActiva.data('oid');
    //Si no hay ninguna fila activa es porque lo estamos enviando a firmar desde la exploración
    if (!oidExploracion) {
        oidExploracion = $("#OID").val();
    }

    var ContenedorModalFirmar = $('#modalVidContent');
    $.ajax({
        type: 'POST',
        url: '/VidSigner/ListaPartial',
        data: { oid: oidExploracion },
        beforeSend: function () {
            $(".spiner-cargando").removeClass('hide');
            ContenedorModalFirmar.html('');
        },
        success: function (data) {
            $('#modal-form-VidSigner').modal('show');
            $(".spiner-cargando").addClass('hide');

            ContenedorModalFirmar.html(data);
            if (localStorage.nombreTablet) {
                $("#TABLETA_NAME").val(localStorage.nombreTablet);
            }
            $('.i-checks').iCheck({
                checkboxClass: 'icheckbox_square-green',
                radioClass: 'iradio_square-green',
            });

            $(".select2").select2({
                theme: "bootstrap"
            }
            );
        }
    });
});

//$(document).on("click", "#visorJpeg", function () {
//    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
//    var oidExploracion = filaActiva.data('oid');

//    getVisorQReport(oidExploracion, 1);
//});

//$(document).on("click", "#visorDicom", function () {
//    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
//    var oidExploracion = filaActiva.data('oid');

//    getVisorQReport(oidExploracion, 2);
//});

//$(document).on("click", "#descargarVisor", function () {
//    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
//    var oidExploracion = filaActiva.data('oid');

//    getVisorQReport(oidExploracion, 3);
//});

//function getVisorQReport(oidExploracion, tipo) {
//    var url = "/Informe/VisorQReport?oid=" + oidExploracion + "&tipo=" + tipo;
//    $.ajax({
//        url: url,
//        type: 'GET',
//        success: function (data) {
//            if (data.success) {

//                switch (tipo) {
//                    case 1:
//                        window.open(data.message);
//                        break;
//                    case 2:
//                        window.open(data.message);
//                        break;
//                    case 3:
//                        window.location.assign(data.message);
//                        break;
//                }

//            }
//            else {
//                if (data.message != "") {
//                    toastr.error(data.message, 'Visor QReport', { timeOut: 5000 });
//                }
//                else {
//                    toastr.error('Imposible abrir el visor de QReport', 'Visor QReport', { timeOut: 5000 });
//                }
//            }
//        },
//        error: function (x, y, z) {
//            toastr.error('Imposible abrir el visor de QReport', 'Visor QReport', { timeOut: 5000 });
//        }
//    });
//}


var referenciaLlamadasLopd = 0;
var referenciaLlamadasCI = 0;
function documentoFirmado(oid, botonStatus, tipo) {

    var checkFirmar = botonStatus.attr('id').replace("Status", "");
    if (!$('#modal-form-entrada').is(':visible')) {
        // if modal is not shown/visible then do something
        clearTimeout(referenciaLlamadasCI);
        clearTimeout(referenciaLlamadasLopd);
    } else {
        $.ajax({
            type: "GET",
            url: '/api/docStatus?docGuid=' + oid,
            success: function (data, textStatus, xhr) {
                if (data === "FIRMADO") {
                    toastr.success('Documento firmado', '', { timeOut: 5000 });
                    if (tipo === "CI") {
                        clearTimeout(referenciaLlamadasCI);
                    } else {
                        clearTimeout(referenciaLlamadasLopd);
                    }

                    botonStatus.ladda('stop');
                    var tdParen = botonStatus.parents('td');
                    tdParen.html('<span  class="badge badge-success status-label">FIRMADO</span>')
                    //$("checkFirmar").remove();

                } else {
                    if (tipo === "CI") {
                        referenciaLlamadasCI = setTimeout(function () { documentoFirmado(oid, botonStatus, "CI") }, 2000);

                    } else {
                        referenciaLlamadasLopd = setTimeout(function () { documentoFirmado(oid, botonStatus, "LOPD") }, 2000);

                    }
                }
            },
            error: function (xhr, ajaxOptions, thrownError) {
                switch (xhr.status) {
                    case 404:
                        if (tipo === "CI") {
                            referenciaLlamadasCI = setTimeout(function () { documentoFirmado(oid, botonStatus, "CI") }, 2000);

                        } else {
                            referenciaLlamadasLopd = setTimeout(function () { documentoFirmado(oid, botonStatus, "LOPD") }, 2000);

                        }
                }
            }

        });
    }
}

$(document).on('click', '#EnviarAFirmar', function () {
    var oEnviarTablet = {};
    oEnviarTablet.DNI = $("#DNI").val();
    oEnviarTablet.DNIRESPOSABLE = $("#DNIRESPOSABLE").val();

    oEnviarTablet.DeviceSelected = $("#TABLETA_NAME option[value=" + $("#TABLETA_NAME").val() + "]").val();
    oEnviarTablet.RESPONSABLE = $("#RESPONSABLE").val();

    localStorage.nombreTablet = oEnviarTablet.DeviceSelected;

    $(".idDocumento").each(function () {
        //En la ventana de entrada todos los documentos chequeados
        if ($(this).prop("checked")) {
            var oidPlantilla = $(this).data('oid');
            var tipoDocumento = $(this).data('tipo');
            oEnviarTablet.DocumentSelected = oidPlantilla;
            oEnviarTablet.Accion = $("#Accion" + oidPlantilla).val();

            oEnviarTablet.OIDEXPLORACION = $(this).data('oidexploracion');
            $('#EnviarAFirmar').addClass('disabled').text('Enviando');
            var request = $.ajax({
                url: "/api/VidSigner",
                type: "POST",
                async: false,
                contentType: "application/json",
                data: JSON.stringify(oEnviarTablet),
            });
            request.done(function (data) {
                toastr.success('Documento enviado a firmar', '', { timeOut: 5000 });
                $('#EnviarAFirmar').removeClass('disabled').text('Enviar');
                $('#Status' + oidPlantilla).addClass('btn-info').text('FIRMANDO...');
                var l = $('#Status' + oidPlantilla).addClass('btn-info').ladda();
                l.ladda('start');
                //en la variable data esta el indentificador unico del documento dado por VidSginer
                documentoFirmado(data, l, tipoDocumento);

            });

            request.error(function (xhr, status, errorThrown) {
                toastr.error('Error al enviar el documento a Firmar', xhr.responseText, { timeOut: 5000 });
                $('#EnviarAFirmar').removeClass('disabled').text('Enviar');
            });
        }


    });


    return false;
});

//evento que abre la ventana modal para el envio del informe
$(document).on("click", ".enviarPorMail", function () {
    $(this).parents("tr").siblings().removeClass('ACTIVA');
    $(this).parents("tr").addClass('ACTIVA');
    var filaActiva = $("#InformesList* .ACTIVA");
    var oidInforme = filaActiva.data('oid');
    var url = "/Informe/GetParaEnvioMail/"; //Url con la vista parcial que agregamos a la ventana modal
    var email = {}; //The Object to Send Data Back to the Controller
    email.IOR_PACIENTE = $(this).data('iorpaciente');
    email.ASUNTO = "Informe";
    email.OWNER = oidInforme;
    var oidExploracion = filaActiva.data('exploracion');
    email.CID = oidExploracion;
    $.ajax({
        type: 'POST',
        url: url,
        data: email,
        dataType: "html",
        beforeSend: function () {
            $(".spiner-cargando").removeClass('hide');
        },
        success: function (evt) {
            $(".spiner-cargando").addClass('hide');
            $("#cargandoInforme").addClass('hide');
            $('#cuerpoModelEnvioMail').html(evt);

        },
    });

});


//los evento change y mover son up y down de todos los filtros del calendario
$(document).on('change', 'select[data-filter-calendar=true]', function () {
   
    var idSelect = $(this).attr('id');
    sessionStorage.forzarListaDia = 'F';
    $('#FILTROS_BUSQUEDATOTAL').prop("checked", false);
    $('#FILTROS_PACIENTE').val("");

    if (parseInt($(this).val()) > 0) {
        $(this).siblings('.select2').addClass('filtroAplicado');

    } else {
        if ($(this).val() === "T" || $(this).val() === "F") {
            $(this).siblings('.select2').addClass('filtroAplicado');

        } else {
            $(this).siblings('.select2').removeClass('filtroAplicado');
        }
    }
    switch (idSelect) {
        case "FILTROS_IOR_MEDICO":

            break;
        case 'FILTROS_IOR_APARATO':
            $('select[name=ddlGrupo]').val(-1);
            $('#txtPaciente').val('');
            $("#EnviarFiltros").trigger("click");
            break;
        case 'FILTROS_IOR_ENTIDADPAGADORA':

            break;
        case 'FILTROS_IOR_GRUPO':
            $('select[name=FILTROS_IOR_APARATO]').val(-1);
            $('select[name=FILTROS_IOR_CENTRO]').val(-1);
            $.ajax({
                type: 'POST',
                url: '/Aparato/GetAparatosPorGrupo',
                data: { oidGrupo: $(this).val() },
                async: 'false',
                beforeSend: function () {

                },
                success: function (data) {

                    var sel = $('#FILTROS_IOR_APARATO');
                    $('#FILTROS_IOR_APARATO').empty();
                    var markup = '<option value="-1"> </option>';
                    var ior_centro = +$("#FILTROS_IOR_CENTRO").val();

                    for (var x = 0; x < data.length; x++) {
                        if (ior_centro >= 0 && data[x].CID === ior_centro) {
                            markup += '<option value="' + data[x].OID + '">' + data[x].COD_FIL + '-' + data[x].DES_FIL + '</option>';

                        } else {
                            if (ior_centro<=0) {
                                markup += '<option value="' + data[x].OID + '">' + data[x].COD_FIL + '-' + data[x].DES_FIL + '</option>';

                            }
                        }
                    }

                    sel.html(markup).show();
                    $('select[name=FILTROS_IOR_APARATO]').val(-1);

                }
            });

            break;
        case 'FILTROS_ESTADO':
            if (parseInt($(this).val()) === 0) {
                $(this).siblings('.select2').addClass('filtroAplicado');
            }
            break;
        case 'FILTROS_IOR_CENTRO':
            var ior_Grupo =+$("#FILTROS_IOR_GRUPO").val();         
            $.ajax({
                type: 'POST',
                url: '/Aparato/GetAparatosPorCentro',
                data: { oidCentro: $(this).val() },
                async: 'false',
                success: function (data) {
                    var sel = $('#FILTROS_IOR_APARATO');
                    $('#FILTROS_IOR_APARATO').empty();
                    var markup = '<option value="-1"> </option>';
                    for (var x = 0; x < data.length; x++) {
                        if (ior_Grupo <= 0 ) {
                            markup += '<option value="' + data[x].OID + '">' + data[x].COD_FIL + '-' + data[x].DES_FIL + '</option>';

                        } else {
                            if (data[x].OWNER === ior_Grupo) {
                                

                            markup += '<option value="' + data[x].OID + '">' + data[x].COD_FIL + '-' + data[x].DES_FIL + '</option>';
                            }

                        }

                    }
                    sel.html(markup).show();
                    $('select[name=FILTROS_IOR_APARATO]').val(-1);

                }
            });
            break;

        default:

    }

     //$("#EnviarFiltros").trigger("click");

    //  SaveFilters();

});

//Evento general para cuando queramos volver a la página anterior almacenada en el servidor
$(document).on("click", ".volverControlado", function () {
    loadURLRetorno();
});

$(document).on("click", "li[data-view]", function (e) {
    $("li[data-view]").removeClass('active');
    $(this).addClass('active');
    var vistaVisible = $(this).data('view');
    $("[id*=View]").addClass('hide');
    if ($("#" + vistaVisible).length > 0) {
        $("#" + vistaVisible).removeClass('hide');
        e.preventDefault();
        return false;
    }

});

$(document).on('click', 'table.seleccionable tbody tr', function () {

    $(this).siblings().removeClass('ACTIVA');
    $(this).addClass('ACTIVA');
});


//Elimina un documento de la tabla imágenes e intenta borrarlo del sistema de archivos.
$(document).on('click', '.eliminarDocumento', function () {

    var botonPresionado = $(this);
    var oidDocumento = botonPresionado.data('oid');
    var filaActivaDocumento = botonPresionado.closest('tr');
    if (filaActivaDocumento.length === 0) {
        filaActivaDocumento = botonPresionado.parents('li');
    }
    swal({
        title: "Esta seguro de eliminar el documento? ",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "Si",
        cancelButtonText: "No",
        closeOnConfirm: true
    }, function (isConfirm) {
        if (isConfirm) {

            $.ajax({
                type: "DELETE",
                url: '/Documentos/Delete/' + oidDocumento,
                success: function (data) {
                    filaActivaDocumento.fadeOut("normal", function () {
                        $(this).remove();
                    });
                }
            });

        }
    });


});

window.addEventListener("submit", function (e) {
    var form = e.target;
    if (form.getAttribute("enctype") === "multipart/form-data") {
        if (form.dataset.ajax) {
            e.preventDefault();
            e.stopImmediatePropagation();
            var xhr = new XMLHttpRequest();
            xhr.open(form.method, form.action);
            xhr.onreadystatechange = function () {
                if (xhr.readyState == 4 && xhr.status == 200) {
                    if (form.dataset.ajaxUpdate) {
                        var updateTarget = document.querySelector(form.dataset.ajaxUpdate);
                        if (updateTarget) {
                            updateTarget.innerHTML = xhr.responseText;
                            DocumentoSubido();
                        }
                    }
                }
            };
            xhr.send(new FormData(form));
        }
    }
}, true);

$(function () {
    //$("body").toggleClass("mini-navbar");
    //Esconde el menu del a izquierda
    //SmoothlyMenu();
    loadExploracionesPersonales();
   loadPeticiones();
    if (localStorage.getItem('EscondeMenu') === "T") {
        $('#side-menu').removeAttr('style');
    }
    $("input:text,form").attr("autocomplete", "off");

    $('.fecha-mask').mask('00/00/0000', { placeholder: "__/__/____" });
    $('.hora-mask').mask('00:00', { placeholder: "HH:mm" });
    $(".fecha-picker,.date-picker").datepicker({
        format: "dd/mm/yyyy",
        language: "es",
        autoclose: true,
        todayHighlight: true
    });

    $('.multiFecha').daterangepicker(
        {
            format: 'DD/MM/YYYY',
            ranges: {
                'Ultimos 7 Dias': [moment().subtract(6, 'days'), moment()],
                'Ultimos 30 Dias': [moment().subtract(29, 'days'), moment()],
                'Hoy': [moment(), moment()]
            },
            locale: {
                applyLabel: 'Buscar',
                cancelLabel: 'Cancelar',
                fromLabel: 'Desde',
                toLabel: 'Hasta',
                firstDay: 1
            }
        }

    );


    $(".select2").select2({
        width: '100%',
        theme: "bootstrap"
    }
    );

    $("select[data-filter-calendar=true]").each(function (i, row) {
        if (parseInt($(row).val()) > 0) {
            $(row).siblings(".select2").addClass('filtroAplicado');
        } else {
            if ($(row).val() === "T" || $(row).val() === "F") {
                $(row).siblings('.select2').addClass('filtroAplicado')
            } else {
                $(row).siblings('.select2').removeClass('filtroAplicado');

            }
        }
    });

    if ($("#FILTROS_ESTADO").val() === "0") {
        $("#FILTROS_ESTADO").siblings('.select2').addClass('filtroAplicado');
    }

    $("select[id*='IOR_COLEGIADO']").select2(
        {
            theme: "bootstrap",
            placeholder: "",
            minimumInputLength: 3,
            quietMillis: 150,
            delay: 250,
            //Does the user have to enter any data before sending the ajax request               
            allowClear: true,
            ajax: {
                url: '/Colegiado/GetColegiados',
                data: function (params) {
                    return {
                        q: params.term// search term
                    };
                },
                processResults: function (data) {
                    return {
                        results: data.items
                    };
                }
            }

        });

    if ($.validator && $.validator.unobtrusive) {

        $.validator.setDefaults({
            ignore: ''
        });

        $.validator.unobtrusive.adapters.addSingleVal("contienecoma", "contienecoma");
        $.validator.addMethod("contienecoma", function (value, element, params) {
            return value.indexOf(",") > 0;
        });
        $.validator.unobtrusive.adapters.addSingleVal("enteropositivo", "enteropositivo");
        $.validator.addMethod("enteropositivo", function (value, element, params) {
            return value > 0;

        });

        $(function () {
            $.validator.methods.date = function (value, element) {
                return this.optional(element) || moment(value, "DD/MM/YYYY", true).isValid();
            }
        });

        $.validator.methods.number = function (value, element) {
            return this.optional(element) || /^-?(?:\d+|\d{1,3}(?:[\s\.,]\d{3})+)(?:[\.,]\d+)?$/.test(value);
        }

        //jQuery.extend(jQuery.validator.methods, {
        //    date: function (value, element) {
        //        var isChrome = window.chrome;
        //        // make correction for chrome
        //        if (isChrome) {
        //            var d = new Date();
        //            return this.optional(element) ||
        //                !/Invalid|NaN/.test(new Date(d.toLocaleDateString(value)));
        //        }
        //        // leave default behavior
        //        else {
        //            alert('fs');
        //            return this.optional(element) ||
        //                !/Invalid|NaN/.test(new Date(value));
        //        }
        //    }
        //}); 


    }
});

//$(document).ajaxError(function (event, request, options) {

//    if (request.status === 401) {
//        $("#session-timeout-dialog").dialog({
//            width: 500,
//            height: 400,
//            modal: true,
//            buttons: {
//                Ok: function () {
//                    $(this).dialog("close");
//                }
//            }
//        });
//    }
//    else {
//        //TODO: Another error occurred, which we could handle globally here. 
//    }
//});

$.ajaxSetup({
    cache: false,
    error: function (xhr, status, err) {
        if (xhr.status === 401)
            window.location.href = "/Users/Index";
    }
});


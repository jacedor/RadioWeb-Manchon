/// <reference path="jquery-2.0.3.min.js" />
/// <reference path="jquery-ui-1.10.3.min.js" /

//***********************************************************//
//*******************FUNCIONES ******************************//
//***********************************************************//
//***********************************************************//


var EstadosExploracion = {
    Pendiente: 0,
    Borrado: 1,
    Presencia: 2,
    Confirmado: 3,
    NoPresentado: 4,
    LlamaAnulando: 5
}

var ctrlPressed = false;
$(window).keydown(function (evt) {
    if (evt.which == 17) { // ctrl
        ctrlPressed = true;
    }
}).keyup(function (evt) {
    if (evt.which == 17) { // ctrl
        ctrlPressed = false;
    }
});

$(window).keydown(function (evt) {
    
    if (evt.which == 119) {
        $(".huecoLibre").addClass("ACTIVA");
    }
        
});

var tid = setInterval(actualizaEspera, 6000);
function actualizaEspera() {
    $(".espera").each(function () {
        var horall = $(this).data('horall');

        var now = moment().format('DD-MM-YYYY HH:mm');
        if (horall != "" && sessionStorage.fechaActual == moment().format('DD-MM-YYYY')) {

            var strDate = sessionStorage.fechaActual;
            var dateParts = strDate.split("-");
            var horaLLegada = new Date(dateParts[2], parseInt(dateParts[1]), parseInt(dateParts[0]), horall.substr(0, 2), horall.substr(3, 2));
            var espera = moment.utc(moment(now, "DD/MM/YYYY HH:mm:ss").diff(moment(horaLLegada, "DD/MM/YYYY HH:mm:ss"))).add(-1, 'hours').format("HH:mm");

            $(this).children('.valor').html(espera);
            // outputs: "00:39:30"

        }
    });
    // do some stuff...
    // no need to recall the function (it's an interval, it'll loop forever)
    // set interval
}




function ajustaEstadoMenuSuperior(currentRow) {
    //Ponemos la fila actual con clase activa
    if (!ctrlPressed && $(".ACTIVA").length == 1) {
        currentRow.siblings().removeClass('ACTIVA');
        
    }
   
    currentRow.addClass('ACTIVA');
    horaExploracionSeleccionada = $('#ExploracionesTable tbody tr.ACTIVA').data('hhora');
    if (currentRow.data('oid') == 0) {
        if ($('#AlertFiltro').hasClass('hidden'))//sabemos que no estamos en un dia festivo
        {
            $('#addExploracion').removeClass('disabled');
        }
        $('#btnActualizarPresencia').addClass('disabled');//deactivamos el botón de Presencia
        $('#btnConfirmar').addClass('disabled');
        $('#btnAviso').addClass('disabled');//activamos el botón del Telefono del aviso
        $('#ColorPickerParent').find('.btn-info').addClass('disabled');//desactivamos el botón cambio de color
        $("#btnInformes").addClass('disabled');
        $("#btnPaciente").addClass('disabled');
        if (currentRow.data('anulada') == 'True') {
            $('#addExploracion').addClass('disabled');
        }

    }
        //Si es un hueco con Hora
    else {
        $('#oidExploracionSeleccionada').val($(this).data('oid'));
        $('#addExploracion').removeClass('disabled');
        $('#ColorPickerParent').find('.btn-info').removeClass('disabled');//activamos el botón cambio de color
        $('#btnAviso').removeClass('disabled');//activamos el botón del Telefono del aviso
        $("#btnInformes").removeClass('disabled');
        $("#btnPaciente").removeClass('disabled');
        //Si la visita se puede actualizar el estado de la exploración se activa el botón de presencia
        if ((currentRow.data('estado') == EstadosExploracion.Presencia) || (currentRow.data('estado') == EstadosExploracion.Pendiente)) {
            $('#btnActualizarPresencia').removeClass('disabled');
            $('#btnConfirmar').removeClass('disabled');
        }
        else {
            $('#btnActualizarPresencia').addClass('disabled');
            $('#btnConfirmar').addClass('disabled');

        }
        if (currentRow.data('estado') == EstadosExploracion.Pendiente || currentRow.data('estado') == EstadosExploracion.Borrado || currentRow.data('estado') == EstadosExploracion.NoPresentado || currentRow.data('estado') == EstadosExploracion.LlamaAnulando) {
            $('#btnBorrar').removeClass('disabled');
            $('#btnBorrarMasOpciones').removeClass('disabled');
        }
        else {
            $('#btnBorrar').addClass('disabled');
            $('#btnBorrarMasOpciones').addClass('disabled');
        }
    }

}

var stringStartsWith = function (string, startsWith) {
    string = string || "";
    if (startsWith.length > string.length)
        return false;
    return string.substring(0, startsWith.length) === startsWith;
};




function cambiarEstadoExploracion(estadoActual, estadoNuevo, Oid, hhora) {

    var request = $.ajax({
        url: "/Exploracion/CambiarEstado",
        data: "estadoActual=" + estadoActual + "&" + "estadoNuevo=" + estadoNuevo + "&" + "oidExploracion=" + Oid + "&" + "hhora=" + hhora,
        type: "GET",
        dataType: "html",
        async: "false"
    });
    $('.spinnerCalendario').removeClass('hide');

    request.done(function (data) {

        var i = 0;
        $(data).find('tr').each(function () {

            var trServer = $(this);
            var trClient = $('#ExploracionesTable tbody tr[data-oid=' + trServer.data('oid') + ']');
            if (i == 0) {
                trServer.addClass('ACTIVA');
            }
            trClient.replaceWith(trServer);
            ajustaEstadoMenuSuperior($('#ExploracionesTable tbody tr[data-oid=' + trServer.data('oid') + ']'));
            SignalRActualizarEstado(trServer.data('oid'));
            i = i + 1;

        }
        );

        $('.ui-popover').popover();
        $('.spinnerCalendario').addClass('hide');


        $.growl.notice({ title: "Estado exploracion modificado.", message: "" });


    });
    request.fail(function (jqXHR, textStatus) {
        $.growl.error({ title: "Error al actualizar la exploracion.", message: "" });

    });
}

function trasladarExploracion(oid, fecha, hhora) {

    var request = $.ajax({
        url: "/Exploracion/Trasladar",
        data: "oid=" + oid + "&fecha=" + fecha + "&hhora=" + hhora + "&aparato=" + sessionStorage.valAparato,
        type: "GET",
        dataType: "html",
        async: "false"
    });
    $('.spinnerCalendario').removeClass('hide');

    request.done(function (data) {
        var i = 0;
        $(data).find('tr').each(function () {

            var trServer = $(this);
            var trClient = $('#ExploracionesTable tbody tr.ACTIVA');
            if (i == 0) {
                trServer.addClass('ACTIVA');
            }

            sessionStorage.oidExploracionTraslado = undefined;
            var filaQueConteniaExplo = $('#ExploracionesTable tbody tr[data-oid="' + oid + '"]');
            if (filaQueConteniaExplo.length > 0) {
                var requestHorario = $.ajax({
                    url: "/Exploracion/GetHoraHorarioRow",
                    data: "fecha=" + fecha + "&aparato=" + sessionStorage.valAparato + "&hhora=" + filaQueConteniaExplo.data('hhora'),
                    type: "GET",
                    dataType: "html",
                    async: "false"
                });

                requestHorario.done(function (datoshorario) {
                    filaQueConteniaExplo.replaceWith($(datoshorario).find('tr'));
                });
            }
            trClient.replaceWith(trServer);
            SignalRActualizarExploracionAnadida(trServer.data('oid'), trServer.data('hhora'));
            i = i + 1;

        }
        );

        $('.ui-popover').popover();
        $('.spinnerCalendario').addClass('hide');


        $.growl.notice({ title: "Exploracion trasladada.", message: "" });
        ajustaEstadoMenuSuperior($('#ExploracionesTable tbody tr.ACTIVA'));
        

    });
    request.fail(function (jqXHR, textStatus) {
        $.growl.error({ title: "Error al trasladar la exploracion.", message: "" });

    });
}

function AnadirAlCarrito(oidExploracion) {
    var request = $.ajax({
        url: "/Exploracion/AnadirAlCarrito",
        data: "oid=" + oidExploracion,
        type: "GET",
        dataType: "html",
        async: "false"
    });
    request.done(function (contenidoCart) {

        var contenidoCarroSinComillas = contenidoCart.replace(/"/g, '\'');

        if ($(contenidoCart).find('tr').length > 1) {
            $(".badge").html($(contenidoCart).find('tr').length - 1);
            $("#btnCartExploraciones").attr('data-content', contenidoCarroSinComillas).data('bs.popover').setContent();
            $('#btnCartExploraciones').popover('toggle');



        }


    });
}

function ActualizarFechaPicker() {
    var strDate = sessionStorage.fechaActual;
    var dateParts = strDate.split("-");
   
    if (dateParts.length == 1) {
        dateParts = strDate.split("/");
    }
    var fechaActual = new Date(dateParts[2], parseInt(dateParts[1]) - 1, dateParts[0]);
    var currentMonth = parseInt(dateParts[1]);
    if (currentMonth < 10) {
        currentMonth = '0' + currentMonth;
    } else {
        currentMonth = '' + currentMonth;
    }
    var currentDay = fechaActual.getDate();
    if (currentDay < 10) {
        currentDay = '0' + currentDay;
    } else {
        currentDay = '' + currentDay;
    }

   
    $("#fechaSelect").val(currentDay + "-" + currentMonth + "-" + fechaActual.getFullYear());

}
 
function anularHoraLibre() {
    
    $("#ExploracionesTable tbody tr.ACTIVA").each(function (i, row) {

        var request = $.ajax({
            url: "/Exploracion/AnularHoraLibre",
            data: "fecha=" + sessionStorage.fechaActual + "&iorAparato=" + $("#ddlAparatos").val() + "&horaHorario=" + $(this).data("hhora") + "&comentario=" + $("#MotivoAnulacion").val(),
            type: "GET",
            dataType: "html",
            async: "false"
        });

        $('.spinnerCalendario').removeClass('hide');

        request.done(function (data) {
            $.growl.error({ title: "Hora Anulada.", message: "" });
            $('#ModalAnularHoras').modal('hide');
            LoadListaDia(false);


        });
        request.fail(function (jqXHR, textStatus) {
            $.growl.error({ title: "Error al anular Hora.", message: "" });

        });
  
        $('.spinnerCalendario').addClass('hide');

    });


}

function imprimirExploracion(oidExploracion) {

    var request = $.ajax({
        url: "/Exploracion/ImprimirFicha",
        data: "oidExploracion=" + oidExploracion,
        type: "GET",
        dataType: "html",
        async: "false"
    });
    $('.spinnerCalendario').removeClass('hide');

    request.done(function (data) {
        var modalContent = $('#ContenidoFichaExploracion');
        modalContent.html('');
        modalContent.replaceWith(data);

        $('.spinnerCalendario').addClass('hide');
        $('#panel-ficha-exploracion').modal('show');



    });
    request.fail(function (jqXHR, textStatus) {
        $.growl.error({ title: "Error al imprimir la ficha.", message: "" });

    });
}

//funcion que carga el calendario con los filtros aplicados
function LoadCalendar() {

    var options = {
        url: "/Calendario",
        data: "mes=" + sessionStorage.fechaActual + "&" + "aparato=" + sessionStorage.valAparato + "&" + "GAparato=" + sessionStorage.valGrupo + "&" + "Mutua=" + sessionStorage.valMutua + "&Centro=" + sessionStorage.valCentro + "&TipoExploracion=" + $("#ddlTipoExploracion").val(),
        type: "POST"

    };

    $('.spinnerCalendario').removeClass('hide');
    $('#calendar div[data-fecha]').removeClass('day-highlight');
    $.ajax(options).done(function (data) {
        var $target = $("#calendario");
        var $newHtml = $(data);
        $target.html(data);
        $('.spinnerCalendario').addClass('hide');
        $("#calendar div[data-fecha='" + sessionStorage.fechaActual + "']").addClass('day-highlight');
        LoadDetailsDay();
        $('.ui-tooltip').tooltip();
        $('.ui-popover').popover();

    });
}

//funcion que carga el calendario con los filtros aplicados
function LoadDetailsDay() {

    var options = {
        url: "/Calendario/ResumenDiario",
        data: "fecha=" + sessionStorage.fechaActual + "&Mutua=" + $("#ddlMutuas").val() + "&GAparato=" + $("#ddlGrupo").val() + "&Aparato=" + $("#ddlAparatos").val() + "&Centro=" + $("#ddlCentros").val() + "&TipoExploracion=" + $("#ddlTipoExploracion").val(),
        type: "POST"

    };

    $('#calendar div[data-fecha]').removeClass('day-highlight');
    //dia.addClass('day-highlight');
    $('.spinnerResumen').removeClass('hide');
    $.ajax(options).done(function (data) {
        var $target = $("#resumen");
        var $newHtml = $(data);
        $target.html(data);
        $('.spinnerResumen').addClass('hide');
        $('#tableResumen ').fixedHeaderTable({ height: '340' });
        $("#calendar div[data-fecha='" + sessionStorage.fechaActual + "']").addClass('day-highlight');

    });
}

function LoadListaDia(busquedaTotal) {
    $('.spinnerExploraciones').removeClass('hide');
    if ($("div[data-fecha='" + sessionStorage.fechaActual + "']").hasClass('festivo')) {
        alert('D\xEDa Festivo');
        $('#AlertFiltro').removeClass('hidden');
        $('#lblAlerta').html('D\xEDa festivo, no podr\xE1 agregar ninguna exploraci\xF3n.');
        $('#addExploracion').addClass('disabled');
    }
    else {
        $('#AlertFiltro').addClass('hidden');
    }
    //Si no hay filtro de aparato y hay un paciente como filtro y no es una busqueda forzada
    if (sessionStorage.valCentro==-1 && sessionStorage.valMutua==-1  && sessionStorage.valTipo==-1 &&  sessionStorage.valAparato == -1 && $("#txtPaciente").val().length < 3 && !busquedaTotal && sessionStorage.forzarListaDia == 'F') {
        $('#txtPaciente').focus();
        $('.spinnerExploraciones').addClass('hide');
        $('#AlertFiltro').removeClass('hidden');
        $('#lblAlerta').html('Su b\xFAsqueda podr\xEDa devolver demasiados resultados. Por favor, filtre por aparato o especifique algun criterio de b\xFAsqueda.');
        $('#ExploracionesTable tbody').children('tr').remove();
        return;
    }
    else {
        if (!$("div[data-fecha='" + sessionStorage.fechaActual + "']").hasClass('festivo')) {
            $('#AlertFiltro').addClass('hidden');
        }

    }
    $('#ExploracionesTable tbody').children('tr').remove();

    var Contenedor = $(".contenidoListaDia");
    var options = {
        url: "/Exploracion/Lista",
        data: "term=" + sessionStorage.fechaActual + "&field=PACIENTE" + "&sidx=HORA" + "&sort=ASC" + "&Borrado=" + localStorage.Borrado + "&Aparato=" + sessionStorage.textAparato + "&Mutua=" + sessionStorage.textMutua + "&GAparato=" + sessionStorage.valGrupo + "&Centro=" + sessionStorage.valCentro + "&TipoExploracion=" + sessionStorage.valTipo + "&FiltroPaciente=" + $("#txtPaciente").val(),
        type: "GET",
        dataType: "html"
    };
    ActualizarFechaPicker();
   
    $.ajax(options).done(function (data) {
        $(".contenidoListaDia").replaceWith(data);
        $('.spinnerExploraciones').addClass('hide');
        $('.ui-popover').popover();

        //Si hemos llegado a esta función después de insertar una exploración la variable sessionStorage contendrá el OID de la inserción
        if (sessionStorage.ExploracionOidActual != undefined) {
            $('#oidExploracionSeleccionada').val(sessionStorage.ExploracionOidActual);
        }
        //Executamos el puglin que hace la tabla scrollable
        $('#ExploracionesTable').fixedHeaderTable({ height: '500' });
        $('#cuentaExploraciones').html($('#ExploracionesTable tbody tr[data-oid!=0]').length + " Exploraciones");

        if ($('#oidExploracionSeleccionada').val() != 0) {
            var filaExploracionSeleccionada = $("#ExploracionesTable tbody tr[data-oid='" + $('#oidExploracionSeleccionada').val() + "']");
            if (filaExploracionSeleccionada.length > 0) {
                filaExploracionSeleccionada.addClass('ACTIVA');
                ajustaEstadoMenuSuperior(filaExploracionSeleccionada);
                var container = $('.fht-tbody');
                var scrollTo = filaExploracionSeleccionada;
                if (scrollTo != null) {
                    container.animate({
                        scrollTop: scrollTo[0].offsetTop - 100
                    });
                }

            }
        }


        $("#ExploracionesTable tbody tr[data-estado='" + EstadosExploracion.Pendiente + "']").each(function () {

            HacerDragableUnaFila($(this));

            $(".huecoLibre").droppable({
                activeClass: "ui-state-default",
                hoverClass: "ui-state-hover",
                accept: ":not(.huecoOcupado)",
                drop: function (event, ui) {
                    var numElemente = +$(".badge").html();
                    $(".badge").html(numElemente - 1);
                    var oid = ui.draggable.data('oid');
                    $(this).addClass('ACTIVA');
                    trasladarExploracion(oid, sessionStorage.fechaActual, $(this).data('hhora'));
                    //$('.CarritoContent').find("[data-oid='" + oid + "']").remove();
                    var request = $.ajax({
                        url: "/Exploracion/QuitarDelcarrito",
                        data: "oid=" + oid,
                        type: "POST"
                    });
                    request.done(function (data) {
                        var contenidoCarroSinComillas = data.replace(/"/g, '\'');
                        $("#btnCartExploraciones").attr('data-content', contenidoCarroSinComillas).data('bs.popover').setContent();
                        $('#btnCartExploraciones').popover('hide');
                        $(".popover").hide();

                    });

                }
            });
        });

    });


    var container = $('.fht-tbody');
    var todayString = moment(new Date()).format('DD-MM-YYYY');

    var today = new Date();
    //Si hemos cargado el día de hoy movemos el scroll hasta la hora actual
    if (sessionStorage.fechaActual === todayString) {
        var n = today.getHours();
        var scrollTo = $("#ExploracionesTable tbody tr[data-hhora*='" + n + "']")[0];
        if (scrollTo != null) {
            container.animate({
                scrollTop: scrollTo.offsetTop - 100
            });
        }
    }

}

function HacerDragableUnaFila(fila) {
    var filaActiva = fila;

    if ((filaActiva.data('estado') == EstadosExploracion.Pendiente && filaActiva.data('oid') > 0)) {
        filaActiva.draggable(
             {
                 helper: "clone",
                 cursor: "move",
                 cursorAt: { top: 5, left: 5 },
                 drag: function (event, ui) {
                     $(this).siblings().removeClass('ACTIVA');
                     $(this).addClass('ACTIVA');
                 },
                 helper: function (event) {
                     var nombrePaciente = $(this).find('.nombrePaciente').text();
                     var fecha = sessionStorage.fechaActual;
                     var horaExploracion = $(this).find('.hhora').text();
                     var oid = $(this).data('oid');
                     var codMut = $(this).find('.codMut').text();
                     var aparato = $(this).find('.aparato').text();
                     return $("<div class='ui-widget-header' data-oid='" + oid + "' + data-fecha='" + fecha + "' + data-mutua='" + codMut + "' + data-paciente='" + nombrePaciente + "'>" + horaExploracion + "-" + nombrePaciente + "</div>").clone().appendTo(".tab-content").css("zIndex", 5).show();
                 }
             });
    }
}
function SetDayDescription(fecha) {
    var dayNumber = moment(fecha, "DD-MM-YYYY").day();


    switch (dayNumber) {
        case 0:
            $('#fechaActualValue').html("Domingo, " + fecha);
            break;
        case 1:
            $('#fechaActualValue').html("Lunes, " + fecha);

            break;
        case 2:
            $('#fechaActualValue').html("Martes, " + fecha);

            break;
        case 3:
            $('#fechaActualValue').html("Mi\xe9rcoles, " + fecha);

            break;
        case 4:
            $('#fechaActualValue').html("Jueves, " + fecha);

            break;
        case 5:
            $('#fechaActualValue').html("Viernes, " + fecha);

            break;
        case 6:
            $('#fechaActualValue').html("S\xe1bado, " + fecha);

            break;
        default:

    }
}

function setCurrentDayActions(fechaString) {
    sessionStorage.fechaActual = fechaString;

    if ($("div[data-fecha='" + fechaString + "']").length == 0) {
        LoadCalendar();
    }
    else {
        LoadDetailsDay();
    }

    ActualizarFechaPicker();
    SetDayDescription(sessionStorage.fechaActual);

}



function habilitarMenuContextual() {
    //************** MENU CONTEXTUAL DEL LISTA DIA****************
    $('#ExploracionesTable').contextMenu({
        selector: 'tbody tr',
        callback: function (key, options) {
            //var m = "ha echo click en: " + key + " on " + $(this).text();
            //alert(m);
            var bg = '154,240,117'; // green
            var trNueva = $('#ExploracionesTable tbody tr[ data-oid=' + $(this).data('oid') + ']');
            trNueva.flash(bg, 1000);
        },
        items: {
            "new": {
                name: "Nueva",
                icon: "New",
                disabled: function (key, opt) {

                    return (!this.data('oid') < 1 || this.hasClass('festivo') || this.data('anulada') == 'True');
                },
                callback: function (key, options) {
                    if (options.$trigger.data('oid') < 1) {
                        if (options.$trigger.data('hhora') != 'undefined') {
                            if (horaExploracionSeleccionada != null) {
                                var filaActiva = $(".ACTIVA");
                                sessionStorage.horaExploracion = filaActiva.data('hhora');
                                $('#ModalAddExploracion').modal('show');

                            }

                        }
                        else {
                            alert('HHROra no especificado');
                        }
                    }
                    else {
                        alert('Debe entrar filtrado  por aparato para asignar una exploración.');
                    }


                }
            },
            "edit": {
                name: "Editar Exploracion",
                disabled: function (key, opt) {
                    return (this.data('oid') < 1);
                },
                icon: "edit",
                callback: function (key, options) {
                    sessionStorage.ExploracionOidActual = options.$trigger.data('oid');

                    window.location = '/Exploracion/Details?oid=' + options.$trigger.data('oid');

                }

            },
            "editPatient": {
                name: "Editar Paciente",
                disabled: function (key, opt) {
                    return (this.data('oid') < 1);
                },
                icon: "edit",
                callback: function (key, options) {
                    if (options.$trigger.data('oid') > 1) {
                        window.location = "/Paciente/Details?oid=" + options.$trigger.data('owner');
                        sessionStorage.ExploracionOidActual = options.$trigger.data('oid');
                    }
                    else {
                        alert('Error');
                    }


                }
            },
            "informes": {
                name: "Informes",
                disabled: function (key, opt) {
                    return (this.data('oid') < 1);
                },
                icon: "",
                callback: function (key, options) {
                    sessionStorage.ExploracionOidActual = options.$trigger.data('oid');
                    window.location = '/Paciente/Details?oid=' + options.$trigger.data('owner') + '&oidExploracion=' + options.$trigger.data('oid') + '#ContenedorInformes/';

                }
            },

            "sep1": "---------",
            "Estado": {
                name: "Estado",
                disabled: function (key, opt) {
                    return (this.data('oid') < 1);
                },
                items: {
                    "estado-actualizar": {
                        name: "Presencia",
                        disabled: function (key, opt) {

                            return (this.data('estado') != EstadosExploracion.Pendiente && this.data('estado') != EstadosExploracion.Presencia);
                        },
                        callback: function (key, options) {
                            var estado = options.$trigger.data('estado');
                            //si es una exploracion con estado presenciable

                            if ((estado == EstadosExploracion.Pendiente) || (estado == EstadosExploracion.Presencia)) {
                                var estadoNuevo = options.$trigger.data('estado') == EstadosExploracion.Pendiente
                                 ? EstadosExploracion.Presencia // red
                                 : EstadosExploracion.Pendiente; // green
                                var oidExploracionSeleccionada = options.$trigger.data('oid');
                                cambiarEstadoExploracion(estado, estadoNuevo, oidExploracionSeleccionada, options.$trigger.data('hhora'));
                            }
                        }

                    },
                    "estado-confirmar": {
                        name: "Confirmar",
                        disabled: function (key, opt) {
                            return (this.data('oid') < 1);
                        },
                        callback: function (key, options) {
                            var estado = options.$trigger.data('estado');

                            if (estado == EstadosExploracion.Pendiente || estado == EstadosExploracion.Borrado || estado == EstadosExploracion.NoPresentado || estado == EstadosExploracion.LlamaAnulando) {
                                alert('No se puede confirmar una exploración borrada o no actualizada');
                            }
                            else {

                                if (estado == EstadosExploracion.Confirmado && (options.$trigger.data('facturada') == 'true' || options.$trigger.data('pagado') == 'True')) {
                                    alert('No se puede anular una exploración facturada o pagada');
                                }
                                else {

                                    //si es una exploracion con estado confirmable
                                    if (estado == EstadosExploracion.Presencia || estado == EstadosExploracion.Confirmado) {
                                        $("[data-ior_paciente='" + options.$trigger.data('ior_paciente') + "']").each(function () {
                                            var estadoNuevo = options.$trigger.data('estado') == EstadosExploracion.Confirmado ? EstadosExploracion.Presencia : EstadosExploracion.Confirmado; // green
                                            var oidExploracionSeleccionada = $(this).data('oid');
                                            cambiarEstadoExploracion(estado, estadoNuevo, oidExploracionSeleccionada, $(this).data('hhora'));
                                        });

                                    }
                                }
                            }

                        }
                    },
                    "estado-borrar": {
                        name: "Borrar"
                    },
                    "estado-noModificable": {
                        name: "No Modificable",
                        callback: function (key, options) {
                            if (options.$trigger.data('oid') > 1) {
                                var request = $.ajax({
                                    url: "/Exploracion/NoModificable",
                                    data: "oid=" + options.$trigger.data('oid'),
                                    type: "POST"
                                });


                                request.done(function (data) {

                                    $.growl.notice({ title: "Exploracion Bloqueada/Desbloqueada.", message: "" });
                                    LoadListaDia(false);
                                });
                                request.fail(function (jqXHR, textStatus) {
                                    $.growl.error({ title: "Error al bloquear la exploración.", message: "" });
                                });


                            }
                        }
                    }
                },


            },

            "Trasladar Exploracion": {
                name: "Trasladar",
                disabled: function (key, opt) {
                    return (this.data('estado') != EstadosExploracion.Pendiente || this.data('anulada') == 'True');
                },
                items: {
                    "seleccionar-exploracion": {
                        name: "Seleccionar exploracion",
                        disabled: function (key, opt) {
                            return (this.data('estado') != EstadosExploracion.Pendiente || (this).data('oid') == 0);
                        },
                        callback: function (key, options) {

                            sessionStorage.oidExploracionTraslado = options.$trigger.data('oid');

                            $.growl.notice({ title: "Exploracion seleccionada.", message: "" });
                        }

                    },
                    "trasladar-exploracion": {
                        name: "Trasladar exploracion",
                        disabled: function (key, opt) {
                            //esto solo estará solo deshabilitado cuando no has seleccinado ninguna exploración con el botón anterio
                            //si es un hueco ya ocupado, enseñaremos un mensaje advirtiendo de doblar

                            return (sessionStorage.oidExploracionTraslado == 'undefined');
                        },
                        callback: function (key, options) {


                            trasladarExploracion(sessionStorage.oidExploracionTraslado, sessionStorage.fechaActual, options.$trigger.data('hhora'));

                        }
                    },
                    "trasladar-todas": {
                        name: "Trasladar las exploraciones del paciente",
                        disabled: function (key, opt) {
                            //esto solo estará solo deshabilitado cuando no has seleccinado ninguna exploración con el botón anterio
                            //si es un hueco ya ocupado, enseñaremos un mensaje advirtiendo de doblar
                            return (sessionStorage.oidExploracionTraslado == 'undefined');
                        },
                        callback: function (key, options) {


                            trasladarExploracion(sessionStorage.oidExploracionTraslado, sessionStorage.fechaActual, options.$trigger.data('hhora'));

                        }
                    }
                },

            },
            "AgendaMultipleMismoAparato": {
                name: "Agenda m\xFAltiple: mismo aparato",
                disabled: function (key, opt) {
                    return (sessionStorage.valAparato == null && sessionStorage.valAparato > 0);
                },
                icon: "",
                callback: function (key, options) {
                    window.location = '/Exploracion/AgendaMultipleMismoAparato?fechaInicial=' + sessionStorage.fechaActual + '&Aparato=' + sessionStorage.textAparato;

                }
            },

            "AnularHoraLibre": {
                name: "Anular horas libres",
                disabled: function (key, opt) {
                    return (!this.data('oid') < 1 || this.hasClass('festivo') );
                },
                icon: "",
                callback: function (key, options) {
                    $('#ModalAnularHoras').modal('show');
                    //AnularHorasLibre();
                }
            },


            "sep2": "---------",
            "printExploracion": {
                name: "Imprimir Ficha Exploracion",
                disabled: function (key, opt) {
                    return (this.data('oid') == 0);
                },
                icon: "print",
                callback: function (key, options) {
                    var oidExploracionSeleccionada = options.$trigger.data('oid');
                    imprimirExploracion(oidExploracionSeleccionada);
                }
            },
            "quit": { name: "Quitar", icon: "quit" }
        },
        events: {
            show: function (opt) {
                // this is the trigger element
                var $this = this;

                ajustaEstadoMenuSuperior($this);

            }
        }
    });
}
//***********************************************************//
//*******************EVENTOS ******************************//
//***********************************************************//
//***********************************************************//
// store the currently selected tab in the hash value

$("#btnCartExploraciones").on("shown.bs.popover", function (e) {

    $('#tblExploracionesCarrito tbody tr').each(function myfunction() {

        HacerDragableUnaFila($(this));

    });
});

$(".ui-draggable").on("click", function (e) {
  
    $(this).siblings().removeClass('ACTIVA');
    $(this).addClass('ACTIVA');
});



//$("#ModalAddExploracion").on("show.bs.modal", function (e) {
//    var filaActiva = $(".ACTIVA");
//    if (filaActiva.data('oid') != 0) {

//        if (sessionStorage.vengoDelModalDeConfirmarDoblar == "F") {

//            $('#modalConfirmarDoblar').modal('show');
//            sessionStorage.vengoDelModalDeConfirmarDoblar = "T";
//            return false;
//        }
//    }
//});

//$(document).on('click', '#btnConfirmaDoblar', function () {
//    sessionStorage.vengoDelModalDeConfirmarDoblar = "T";
//    $('#modalConfirmarDoblar').modal('hide');
//    $('#ModalAddExploracion').modal('show');

//});


$("#ModalAddExploracion").on("shown.bs.modal", function (e) {
    //BUSCAR
    $('#FindPaciente').val('').focus();
    sessionStorage.vengoDelModalDeConfirmarDoblar = "F";
});

$("#btnUltimaBusquedas").on("shown.bs.popover", function (e) {

    var sel = $('.BusquedasTableBody');
    sel.empty();
    var markup = '';
    var arrayBusquedas = JSON.parse(sessionStorage["busquedas"]);
    for (var x = 0; x < arrayBusquedas.length; x++) {
        markup += '<tr class="filterRow" data-filter="' + arrayBusquedas[x].paciente + '" ><td>' + arrayBusquedas[x].paciente + '</td><td>' + arrayBusquedas[x].hora + '</td>';
        markup += '<td><i data-filter="' + arrayBusquedas[x].paciente + '" class="fa fa-check-circle selectBusqueda" style="font-size:20px;" data-filter="' + arrayBusquedas[x].paciente + '"></i></td></tr>';

    }
    sel.html(markup).show();

});


$(document).on("keypress", "#FindPaciente", function (e) {
    if (e.keyCode == 13) {
        var url = "/Paciente/Index/"; //The Url to the Action  Method of the Controller
        $.ajax({
            type: 'POST',
            url: url,
            data: 'NumRows=' + $("#NumRows").val() + '&Paciente=' + $("#FindPaciente").val() + '&Field=' + $("#btnCriteria").text(),
            dataType: "html",
            success: function (evt) {
                $('.PacientesList').html(evt);
                $('.ui-tooltip').tooltip();
                $('.spinnerPacientes').addClass('hide');
                sessionStorage.filterPaciente = $("#FindPaciente").val().trim().toUpperCase();
                //si esta busqueda no está incluida ya en el array de las 
                //10 ultimas búsquedas la incluímos
                AnadirA10Busquedas($("#FindPaciente").val());


            },
        });
    }
});

$(document).on('click', '.editarExploracionMobile', function () {

    window.location = "/Paciente/Details?oid=" + $(this).data('OWNER');
    sessionStorage.ExploracionOidActual = $(this).data('OWNER');
});
$(document).on('click', '.editarExploracionMobile', function () {

    //El oid =0 significa que es un hueco libre
    if ($(this).data('oid') == 0) {
        $('.bs-add-exploracion').modal('show');
    }
    else {
        $('#oidExploracionSeleccionada').val($(this).data('oid'));
        sessionStorage.ExploracionOidActual = $(this).data('oid');

        window.location = '/Exploracion/Details?oid=' + $(this).data('oid');

    }

});


$(document).on('click', '#GuardarAnularHoras', function () {

    anularHoraLibre();


});
$(document).on('click', '#ExploracionesTable tbody tr', function () {
   
    if (ctrlPressed) {
        
        // do something
        if ($(this).hasClass('ACTIVA')) {
            $(this).removeClass('ACTIVA');
        } else {
            $(this).addClass('ACTIVA');
        }
    } else {
        $(this).siblings().removeClass('ACTIVA');
    }

    

});

 //mapeamos el evento click de cada una de las filas de la tabla de exploraciones
$(document).on('dblclick', '#ExploracionesTable tbody tr', function () {

    //El oid =0 significa que es un hueco libre
    if ($(this).data('oid') == 0) {
        $('.bs-add-exploracion').modal('show');
    }
    else {
        $('#oidExploracionSeleccionada').val($(this).data('oid'));
        sessionStorage.ExploracionOidActual = $(this).data('oid');

        window.location = '/Exploracion/Details?oid=' + $(this).data('oid');

    }

});


$("ul.nav-tabs > li > a").on("shown.bs.tab", function (e) {
    var id = $(e.target).attr("href").substr(1);
    window.location.hash = id;
});

$(window).resize(function () {
    $('#ExploracionesTable').fixedHeaderTable({ height: '500' });
});

// click sobre el TABCALENDARIO
$(document).on('click', '#TabCalendario', function () {

    sessionStorage.forzarListaDia = 'F';
    $(this).addClass("active").show();
    $('#TabListaDia').removeClass('active');
    LoadCalendar();


});

// click sobre el TAB EXPLORACIONES
$(document).on('click', '#TabListaDia', function () {
    $(this).addClass('active');
    $(this).tab('show');
    $('#TabCalendario').removeClass('active');
    LoadListaDia(false);
});

$(document).on('click', '#ColorPicker li a', function () {
    var cidSelected = $(this).data('cid');
    var color = $(this).css('background-color');
    $('#ColorPickerParent').removeClass('open');
    //primero cambiamos el color de la fila activa
    var filaActiva = $('#ExploracionesTable tbody tr.ACTIVA');

    if (filaActiva.data('oid') > 0) {
        filaActiva.find('.cid').css('background-color', color);

        var request = $.ajax({
            url: "/Exploracion/CambiarColor",
            data: "cid=" + cidSelected + "&" + "oid=" + filaActiva.data('oid'),
            type: "POST"
        });


        request.done(function (data) {

            $.growl.notice({ title: "Color asignado a la exploracion.", message: "" });

        });
        request.fail(function (jqXHR, textStatus) {
            $.growl.error({ title: "Error al asignar el color a la exploracion.", message: "" });

        });
    }


    return false;
});


//mapeamos el evento click de cada una de las filas de la tabla de exploraciones
$(document).on('click', '#ExploracionesTable tbody tr', function () {
    $('#btnCartExploraciones').popover('hide');
    ajustaEstadoMenuSuperior($(this));
    //HacerDragableUnaFila($(this));
});

//mapeamos el evento cick de los dias activos para ver el resumen
$(document).on('click', 'div[data-fecha]', function () {
    setCurrentDayActions($(this).attr('data-fecha'));
});

//mapeamos el evento DOBLEclick de cada uno de los dias
$(document).on('dblclick', 'div[data-fecha]', function () {
    var textoAgenda = $(this).attr('data-content');
    if ($(this).hasClass('festivo')) {
        alert('D\xEDa Festivo');
        $('#addExploracion').addClass('disabled');
        return false;
    }
    if (textoAgenda !== undefined && textoAgenda.indexOf("#") == 0) {
        alert(textoAgenda);
    }

    $('.spinnerExploraciones').removeClass('hide');
    sessionStorage.fechaActual = $(this).attr('data-fecha');
    //Visualizamos la tab del ListaDia
    $('#myTab3 a:last').tab('show');
    LoadListaDia(false);
});

$(document).on('keyup', 'select[data-filter-calendar=true]', function (event) {

    if (event.which == 32) {
        var idSelect = $(this).attr('id');
        $('#' + idSelect).val($('#' + 'idSelect option:first').val());
        return true;
    }
});

//los evento change y mover son up y down de todos los filtros del calendario
$(document).on('change', 'select[data-filter-calendar=true]', function () {
    var idSelect = $(this).attr('id');
    sessionStorage.forzarListaDia = 'F';

    switch (idSelect) {
        case 'ddlAparatos':
            //Al seleccionar un Filtro por Aparato quitamos el filtro de Grupo;
            $('select[name=ddlGrupo]').val(-1);
            $('#txtPaciente').val('');
            sessionStorage.textGrupo = ' ';
            sessionStorage.valGrupo = -1;
            sessionStorage.textAparato = $("#ddlAparatos option[value=" + $("#ddlAparatos").val() + "]").text();
            sessionStorage.valAparato = $("#ddlAparatos").val();
            break;
        case 'ddlGrupo':
            $('select[name=ddlAparatos]').val(-1);
            sessionStorage.textAparato = ' ';
            sessionStorage.valAparato = -1;
            sessionStorage.textGrupo = $("#ddlGrupo option[value=" + $("#ddlGrupo").val() + "]").text();
            sessionStorage.valGrupo = $("#ddlGrupo").val();
            $.ajax({
                type: 'POST',
                url: '/Calendario/GetAparatosPorGrupo',
                data: { oidGrupo: $(this).val() },
                async: 'false',
                beforeSend: function () {

                },
                success: function (data) {

                    var sel = $('#ddlAparatos');
                    $('#ddlAparatos').empty();
                    var markup = '<option value="-1"> </option>';
                    for (var x = 0; x < data.length; x++) {
                        markup += '<option value="' + data[x].OID + '">' + data[x].COD_FIL + '</option>';
                    }
                    sel.html(markup).show();
                    $('select[name=ddlAparatos]').val(-1);
                    sessionStorage.textAparato = $("#ddlAparatos option[value=" + $("#ddlAparatos").val() + "]").text();
                    sessionStorage.valAparato = $("#ddlAparatos").val();
                }
            });

            break;
        case 'ddlCentros':
            sessionStorage.textCentro = $("#ddlCentros option[value=" + $("#ddlCentros").val() + "]").text();
            sessionStorage.valCentro = $("#ddlCentros").val();
            sessionStorage.valAparato = '-1';
            sessionStorage.textAparato = ' ';
            $.ajax({
                type: 'POST',
                url: '/Calendario/GetAparatosPorCentro',
                data: { oidCentro: $(this).val() },
                async: 'false',
                beforeSend: function () {

                },
                success: function (data) {

                    var sel = $('#ddlAparatos');
                    $('#ddlAparatos').empty();
                    var markup = '<option value="-1"> </option>';
                    for (var x = 0; x < data.length; x++) {
                        markup += '<option value="' + data[x].OID + '">' + data[x].COD_FIL + '</option>';
                    }
                    sel.html(markup).show();
                    $('select[name=ddlAparatos]').val(-1);
                    sessionStorage.textAparato = $("#ddlAparatos option[value=" + $("#ddlAparatos").val() + "]").text();
                    sessionStorage.valAparato = $("#ddlAparatos").val();
                }
            });
            break;
        case 'ddlMutuas':
            sessionStorage.textMutua = $("#ddlMutuas option[value=" + $("#ddlMutuas").val() + "]").text().trim();
            sessionStorage.valMutua = $("#ddlMutuas").val();
             break;
        case 'ddlTipoExploracion':
            sessionStorage.textTipo = $("#ddlTipoExploracion option[value=" + $("#ddlTipoExploracion").val() + "]").text().trim();
            sessionStorage.valTipo = $("#ddlTipoExploracion").val();
            break;
        default:

    }

    if ($('#TabCalendario').hasClass('active')) {
        LoadCalendar();
    }
    else {
        LoadListaDia(false);
    }
});




//mapeamos el evento click de la navegación para cambiar de meses
$(document).on('click', '#NavigationMonth.pager a', function () {
    var a = $(this);

    if (a.hasClass('btnHoy')) {
        sessionStorage.fechaActual = moment(new Date()).format('DD-MM-YYYY');
        setCurrentDayActions(sessionStorage.fechaActual);
    }
    else {
        setCurrentDayActions(a.attr('data-mes'));
    }
});


////Al Hacer Doble click sobre una row del modal popup de pacientes vamos a la exploración temporal en memoria
//$(document).on('dblclick', ".PacientesList .PacienteItem", function (e) {

//    var oidPaciente = $(this).data('oid');
//    var url = "/Exploracion/Details/";
//    var exploracion = { IOR_PACIENTE: oidPaciente, FECHA: sessionStorage.fechaActual, HORA: horaExploracionSeleccionada, IOR_APARATO: $('select[name=ddlAparatos]').val() }

//    $.ajax({
//        type: 'POST',
//        url: url,
//        data: JSON.stringify({ oExploracion: exploracion }),
//        contentType: 'application/json',
//        success: function (returndata) {
//            if (returndata.ok)
//                window.location = returndata.newurl;
//            else
//                window.alert(returndata.message);

//        }

//    });

//});


//*************ACCIONES RAPIDAS DEL MENU SUPERIOR************* 
$(document).on('click', '#addExploracion', function () {
    if (horaExploracionSeleccionada != null) {
        var filaActiva = $(".ACTIVA");
        if (filaActiva.data('oid') != 0) {
            //$('#modalConfirmarDoblar').modal('show');
            //return false;

        } else {
            sessionStorage.horaExploracion = filaActiva.data('hhora');
        }



    }
});

$(document).on('click', '#btnDiaAnterior', function () {

    setCurrentDayActions(addDays(sessionStorage.fechaActual, -1));
    LoadListaDia(false);
    return false;
});

$(document).on('click', '#btnHoy', function () {
    sessionStorage.fechaActual = moment(new Date()).format('DD-MM-YYYY');
    setCurrentDayActions(sessionStorage.fechaActual);
    LoadListaDia(moment(new Date()).format('DD-MM-YYYY'));
    return false;
});


$(document).on('click', '#btnDiaSiguiente', function () {
    setCurrentDayActions(addDays(sessionStorage.fechaActual, 1));
    LoadListaDia(false);
    return false;
});

$(document).on('click', '#btnInformes', function () {
    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
    window.location = '/Paciente/Details?oid=' + filaActiva.data('owner') + '&oidExploracion=' + filaActiva.data('oid') + '#ContenedorInformes/';
    return false;
});
$(document).on('click', '#btnPaciente', function () {
    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
    window.location = '/Paciente/Details?oid=' + filaActiva.data('owner');
    return false;
});

$(document).on("click", "#btnBorrar,#btnNoPresentado,#btnLlamaAnulando", function myfunction() {

    var trigger = $(this).attr("id");
    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
    //hay que enseñar el pop up si es igual a 0 solamente
    var estadoExploracion = filaActiva.data('estado');
    var estadoNuevo = null;

    if (estadoExploracion == EstadosExploracion.Borrado || estadoExploracion == EstadosExploracion.NoPresentado || estadoExploracion == EstadosExploracion.LlamaAnulando) {
        estadoNuevo = EstadosExploracion.Pendiente;
    }

    if (estadoExploracion == EstadosExploracion.Pendiente) {
        switch (trigger) {
            case "btnBorrar":
                estadoNuevo = EstadosExploracion.Borrado;
                break;
            case "btnNoPresentado":
                estadoNuevo = EstadosExploracion.NoPresentado;
                break;
            case "btnLlamaAnulando":
                estadoNuevo = EstadosExploracion.LlamaAnulando;
            default:

        }

    }

    var hhora = filaActiva.data('hhora');

    var oidExploracionSeleccionada = filaActiva.data('oid');

    cambiarEstadoExploracion(estadoExploracion, estadoNuevo, oidExploracionSeleccionada, hhora);
    LoadListaDia(false);
    return false;
});

$(document).on("click", "#btnActualizarPresencia", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");


    var estadoNuevo = filaActiva.data('estado') == 0
                              ? '2'
                              : '0';

    var hhora = filaActiva.data('hhora');

    var oidExploracionSeleccionada = filaActiva.data('oid');

    cambiarEstadoExploracion(filaActiva.data('estado'), estadoNuevo, oidExploracionSeleccionada, hhora);
    return false;

});

$(document).on("click", "#btnConfirmar", function myfunction() {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");


    var estadoNuevo = EstadosExploracion.Confirmado;

    var hhora = filaActiva.data('hhora');

    var oidExploracionSeleccionada = filaActiva.data('oid');

    cambiarEstadoExploracion(filaActiva.data('estado'), estadoNuevo, oidExploracionSeleccionada, hhora);
    return false;
});

// click sobre el texto de una exploracion
$(document).on('click', '#btnAviso', function () {

    var currentRow = $('#ExploracionesTable tbody tr.ACTIVA');
    if (currentRow.data('oid') != 0) {

        $('#panel-comentario-exploracion').modal('show');
        $("#TextoNotaExploracion").val(currentRow.find(".textoExploracion").data('content'));
        $("#GuardarTextoExploracion").data('oid', currentRow.find(".textoExploracion").data('oid'));
    }

    return false;

});




//Al escribir sobre la caja de texto del modal popup de pacintes
$("#txtPaciente").keyup($.debounce(250, function () {

    if ($("#txtPaciente").val().length > 3 && $("#txtPaciente").val() != "" && sessionStorage.valAparato == -1 && sessionStorage.valGrupo == -1) {

        sessionStorage.filtroPaciente = $("#txtPaciente").val();
        LoadListaDia(false);
    }

}));

$(document).on("keypress", "#txtPaciente", function (e) {

    if (e.keyCode == 13) {
        sessionStorage.forzarListaDia = 'T';
        LoadListaDia(true);
    }
});



// click sobre el texto de una exploracion
$(document).on('dblclick', '.textoExploracion', function () {

    if ($(this).data('oid') != 0) {
        $('#panel-comentario-exploracion').modal('show');
        $("#TextoNotaExploracion").val($(this).data('content'));
        $("#GuardarTextoExploracion").data('oid', $(this).data('oid'));
    }

    return false;

});


$(document).on('click', '#GuardarTextoExploracion', function () {

    var texto = {};
    texto.OWNER = $(this).data('oid');
    texto.TEXTO = $('#TextoNotaExploracion').val();

    $.ajax({
        type: 'POST',
        url: '/Textos/AddOrEdit',
        data: JSON.stringify({ oTexto: texto }),
        contentType: 'application/json',
        success: function (evt) {
            $.growl.notice({ title: "Texto modificado correctamente", message: "" });
            $('#panel-comentario-exploracion').modal('hide');
            var subtexto = texto.TEXTO;
            if (subtexto.length > 20) {
                subtexto = subtexto.substr(0, 20) + "...";
            }

            $('.textoExploracion[data-oid=' + texto.OWNER + ']').text(subtexto);
            $('.textoExploracion[data-oid=' + texto.OWNER + ']').data('content', evt);
            $('.ui-popover').popover();
        },
    });


});


$('#fechaSelect').on('changeDate', function (ev) {

    var newDate = new Date(ev.date);
    sessionStorage.fechaActual = moment(newDate).format('DD-MM-YYYY')
    if ($('#TabCalendario').hasClass('active')) {
        LoadCalendar();
    }
    else {
        LoadListaDia(false);
    }
    SetDayDescription(sessionStorage.fechaActual);
}

);

$('#chkBorrado').change(function () {
    if ($(this).is(":checked")) {
        localStorage.Borrado = 'T';
    }
    else {
        localStorage.Borrado = 'F';
    }

    if (!$('#TabCalendario').hasClass('active')) {
        LoadListaDia(false);
    }


});

$(document).ready(function () {


    $("#fechaSelect").datepicker({
        dateFormat: 'dd-mm-yy',
        changeMonth: true,
        changeYear: true,
        onSelect: function (dateText, inst) {
            sessionStorage.fechaActual = dateText;
            if ($('#TabCalendario').hasClass('active')) {
                LoadCalendar();
                sessionStorage.forzarListaDia = 'F';
            }
            else {
                LoadListaDia(true);
            }
        }
    });

    
    localStorage.Borrado = (localStorage.Borrado || 'T');
    sessionStorage.fechaActual = (sessionStorage.fechaActual || moment(new Date()).format('DD-MM-YYYY'));
    if ($("#ddlAparatos").val() != null) {
        sessionStorage.textAparato = $("#ddlAparatos option[value=" + $("#ddlAparatos").val() + "]").text();
        sessionStorage.valAparato = $("#ddlAparatos").val();       
    }
    else {
        sessionStorage.textAparato = (sessionStorage.textAparato || ' ');
        sessionStorage.valAparato = (sessionStorage.valAparato || -1);
    }

   
    sessionStorage.textMutua = (sessionStorage.textMutua || ' ');
    sessionStorage.textGrupo = (sessionStorage.textGrupo || ' ');
    sessionStorage.textCentro = (sessionStorage.textCentro || ' ');
    sessionStorage.vengoDelModalDeConfirmarDoblar = 'F';
    sessionStorage.textTipo = (sessionStorage.textTipo || ' ');
   
    sessionStorage.valMutua = (sessionStorage.valMutua || -1);
    sessionStorage.valGrupo = (sessionStorage.valGrupo || -1);
    if ($("#ddlCentros").val() != null) {
        sessionStorage.valCentro = $("#ddlCentros").val();
    }
    else {
        sessionStorage.valCentro = (sessionStorage.valCentro || -1);
    }
    
    sessionStorage.forzarListaDia = (sessionStorage.forzarListaDia || 'F');
    //PRI MUT ICS
    sessionStorage.valTipo = (sessionStorage.valTipo || -1);

    if (localStorage.Borrado == 'T') {
        $('#chkBorrado').attr('checked', true);
    }

    if (sessionStorage.valAparato != -1) {
        $('#ddlAparatos').val(sessionStorage.valAparato);
    }


    if (sessionStorage.valGrupo != -1) {
        $('#ddlGrupo').val(sessionStorage.valAparato);
    }

    if (sessionStorage.textMutua == "  - No asignado") {
        sessionStorage.textMutua = " ";
    }

    if (sessionStorage.valCentro != -1) {
        $('#ddlCentro').val(sessionStorage.valCentro);
    }

    if (sessionStorage.valTipo != -1) {
        $('#ddlTipoExploracion').val(sessionStorage.valTipo);
    }
    //$('.subnavbar-inner').hide();//escondemos el menu superior
    var fichaActiva = window.location.hash;//en la url tenemos la ficha activa 
    $('#myTab3 a[href="' + fichaActiva + '"]').tab('show');

    $('#tableResumen ').fixedHeaderTable({ height: '440' });

    setCurrentDayActions(sessionStorage.fechaActual);

    if ($('#TabCalendario').hasClass('active')) {
        LoadCalendar();
        sessionStorage.forzarListaDia = 'F';
    }
    else {
        LoadListaDia(true);
    }

   

    AnadirAlCarrito(0);

    $("#areaDropExploracion").droppable({
        activeClass: "ui-state-default",
        hoverClass: "ui-state-hover",
        accept: ":not(.ui-sortable-helper)",
        drop: function (event, ui) {
            var numElemente = +$(this).find(".badge").html();
            $(this).find(".badge").html(numElemente + 1);
            var oid = ui.draggable.data('oid');
            AnadirAlCarrito(oid);

        }
    });


    if (!$("#UsuarioLogeado").data("username") == "Manresa") {
        habilitarMenuContextual();
    }
    
    



});





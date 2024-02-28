/// <reference path="jquery-2.0.3.min.js" />
/// <reference path="jquery-ui-1.10.3.min.js" /

//***********************************************************//
//*******************FUNCIONES ******************************//
//***********************************************************//
//***********************************************************//
var EstadosBuscador = {
    MismoAparato: 0,
    MultiAparato: 1
};

var huecosReservados = new Array();

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


var filtrosBusqueda;

function filterRows(statusName,mostrar) {
    $("#ExploracionesTable tbody tr").each(function () {
        if (!$(this).hasClass(statusName) && !$(this).hasClass('ACTIVA')) {
            if (mostrar) {
                $(this).show();
            } else {
                $(this).hide();
            }
            
            
        }
    });
}

function SaveFilters(filtros) {

     $.ajax({
        type: 'POST',
        url: '/Settings/SaveFiltros',
        data: JSON.stringify(filtros),
        contentType: 'application/json; charset=utf-8',
        complete: function () {

        }
    });
}

function getFiltrosBusqueda(contenedorPrincipal) {
    var AparatoDropdown = $("#" + contenedorPrincipal).find('.aparato');
    var Aparato = (($("#" + contenedorPrincipal).find('.aparato option:selected')).text().trim() || -1);
    var oFiltrosBusqueda = {};
    oFiltrosBusqueda.Fecha = $("#" + contenedorPrincipal).find('.fecha').val();
    oFiltrosBusqueda.oidAparato = ($("#" + contenedorPrincipal).find('.aparato option:selected')).val();
    oFiltrosBusqueda.DescAparato = Aparato;
    oFiltrosBusqueda.Borrados = "F";
    var elemHuecos = document.querySelector('#chkHuecos');
    oFiltrosBusqueda.SoloHuecos = elemHuecos.checked;
    return oFiltrosBusqueda;
}

function activarMenuContextual(contenedor) {

    var tablaResultados = $(contenedor).find(".listadiaMultiple");
    //Esta funcion enlaza en el listadia el plugin de menu contextual        
    tablaResultados.bootstrapTable({
        contextMenu: '#context-menu',
        onContextMenuItem: function (row, $el) {
            //Agregar Exploracion al carrito
            if ($el.data("item") === "carrito") {
                if ($("#ExploracionesMiniTable tbody tr[data-oid=" + row.oid + "]").length !== 0) {
                    toastr.warning('Carrito', 'La exploracion ya está en el carrito', { timeOut: 5000 });
                } else {
                    $.ajax({
                        type: 'POST',
                        url: '/Settings/SaveExploracion',
                        data: { oid: row.oid },
                        success: function (data) {
                            loadExploracionesPersonales();
                        }
                    });

                }

            }
            //Agregar todas las exploraciones Exploracion al carrito
            if ($el.data("item") === "carritoTodas") {
                if ($("#ExploracionesMiniTable tbody tr[data-oid=" + row.oid + "]").length !== 0) {
                    toastr.warning('Carrito', 'La exploracion ya está en el carrito', { timeOut: 5000 });
                } else {
                    $.ajax({
                        type: 'POST',
                        url: '/Settings/SaveExploracion',
                        data: { oid: row.oid, todasDelDia: 'true' },
                        success: function (data) {
                            loadExploracionesPersonales();
                        }
                    });

                }

            }
            /* Comentamos esto y aplicamos lo de abajo
            if ($el.data("item") === "enviarSMS") {

                $.ajax({
                    type: 'GET',
                    url: '/Telefono/GetMovilPacienteFromExploracion',
                    data: "oidExploracion=" + row.oid,
                    success: function (data) {
                        $("#movilEnvioSMS").val(data);
                    }
                });

            }*/
            if ($el.data("item") === "enviarSMS") {
                var idExploracion = row.oid;
                var idPaciente = row._data.ior_paciente;
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

                $.ajax({
                    type: 'GET',
                    url: '/Telefono/GetMovilPacienteFromExploracion?oidExploracion=' + idExploracion,
                    success: function (data) {
                        $("#movilEnvioSMS").val(data);
                    }
                });
            }

            if ($el.data("item") === "tiempoEspera") {

                var rowActiva = $("#ExploracionesTable tbody tr[data-oid=" + row.oid + "]");
                $("#HoraProgramada").html(rowActiva.data('hora'));
                $("#tiempoEspera").html(rowActiva.data('espera'));
                $("#HoraLLegada").html(rowActiva.data('horall'));
                $("#HoraRealizada").html(rowActiva.data('horaex'));
            }
            //al hacer boton derecho sobre una fila la activamos desactivando previamente el resto
        }, onClickRow: function (row, $el) {
          //  tablaResultados.find('.ACTIVA').removeClass('ACTIVA');
          //   $el.addClass('ACTIVACONTEXTUAL');
          

        }
    });
}

function anularHoraLibre(accion) {

    var filaActiva = $("tr.ACTIVA");

    if (filaActiva.length === 0) {
        toastr.error('Debe seleccionar un hueco en la lista de exploracion', 'Anular', { timeOut: 5000 });
        return false;
    }

    $("tr.ACTIVA").each(function (i, row) {

        var filtros = getFiltrosBusqueda();
        if ($("#OtrosAnulacion").hasClass('hide')) {
            filtros.Comentario = $("#MotivoAnulacion").val();
        } else {

            filtros.Comentario = $("#OtrosAnulacion").val();
        }

        filtros.Hora = $(this).data("hhora");
        var request = $.ajax({
            url: "/Exploracion/AnularHoraLibre",
            data: JSON.stringify(filtros),
            contentType: 'application/json; charset=utf-8',
            type: "POST",
            dataType: "html"
        });
        request.done(function (data) {
            if (accion === "Desanular") {
                toastr.info(accion, 'Hora ' + accion, { timeOut: 5000 });
            } else {
                toastr.success(accion, 'Hora ' + accion, { timeOut: 5000 });
            }
            LoadListaDia();
        });
        request.fail(function (jqXHR, textStatus) {
            toastr.warning(accion, 'Error al anular Hora.', { timeOut: 5000 });
        });

    });


}


//funcion que carga o bien un solo buscador de la busqueda multiple
// si el parametro lo indica o todos los contenedores en un bucle del else.
function LoadBusquedaMultiple(contenedor, filtros, saltaFestivos, direccion, numeroBuscadores) {

    contenedor = contenedor || null;
    saltaFestivos = saltaFestivos || false;
    direccion = direccion || "ASC";
    numeroBuscadores = numeroBuscadores || "1";
    if (contenedor) {
        var oFiltros = filtros || getFiltrosBusqueda(contenedor);
        var options = {
            url: "/AgendaMulti/Index",
            data: JSON.stringify({
                oFiltros: oFiltros,
                Direccion: direccion,
                numeroBuscadores: numeroBuscadores,
                saltaFestivos: saltaFestivos
            }),
            contentType: 'application/json; charset=utf-8',
            type: "POST",
            dataType: "html"
        };

        $.ajax(options).success(function (data) {
            var contenedorResultados = $("#" + contenedor);            
            contenedorResultados.html(data);
            
            activarMenuContextual("#" + contenedor);

            $("#" + contenedor).find(".date-picker").datepicker({
                format: "dd/mm/yyyy",
                todayBtn: true,
                language: "es",
                autoclose: true,
                todayHighlight: true,
                calendarWeeks: false
            }).on('changeDate', dateChanged);

            $(".popover").css("min-width", "530px");
            $(".popover-content").css("min-width", "530px");

            $('.scroll_content').slimscroll({
                height: '900px',
                alwaysVisible: true
            });
           
            $(".select2").select2({
                width: '100%',
                theme: "bootstrap"
            }
            );
            if (oFiltros.SoloHuecos) {
                
                filterRows('huecoLibre');
            }
            
        });


    }

}

function trasladarExploracion(oid) {

    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var filtroActivos = getFiltrosBusqueda();
    if (filaActiva.length === 1) {
        var request = $.ajax({
            url: "/Exploracion/Trasladar",
            data: "oid=" + oid + "&fecha=" + filtroActivos.Fecha + "&hhora=" + filaActiva.data('hhora') + "&aparato=" + filtroActivos.oidAparato,
            type: "GET",
            dataType: "html",
            async: "false"
        });
        request.done(function (data) {
            LoadListaDia();
            toastr.success('Exploración Trasladada', 'Trasladar', { timeOut: 5000 });
        });
        request.fail(function (jqXHR, textStatus) {
            toastr.error('Trasladar', 'Error al trasladar', { timeOut: 5000 });
        });
    } else {
        toastr.error('Debe seleccionar un hueco en la lista de exploracion', 'Trasladar', { timeOut: 5000 });
    }

}

$(document).on('click', '.eliminarDelCarrito', function () {
    eliminarDelCarrito($(this).data('oid'));
});

//mapeamos el evento click de cada una de las filas de la tabla de exploraciones de dentro del carrito que permite trasladar exploraciones
$(document).on('click', '.trasladarExploracion', function () {
    trasladarExploracion($(this).data('oid'));
});

//Evento del botón que anula una un hueco libre
$(document).on('click', '#anularHuecoLibre,.desanularHora', function () {

    var accion = "Anular";
    if ($(this).hasClass("desanularHora")) {
        var trDesanular = $(this).closest('tr');
        trDesanular.siblings().removeClass('ACTIVA');
        trDesanular.addClass('ACTIVA');
        accion = "Desanular";
    }
    anularHoraLibre(accion);

});

////mapeamos el evento click de cada una de las filas de la tabla de exploraciones de dentro del carrito que permite trasladar exploraciones
//$(document).on('click', '.duplicarExploracion', function () {
//    var filaActiva = $("tr.ACTIVA");
//    var hueco = {};

//    if (filaActiva.length === 1) {
//        window.location = "/Exploracion/Duplicar/" + $(this).data('oid') + "?hora=" + filaActiva.data('hhora');
//    } else {
//        toastr.error('Debe seleccionar un hueco en la lista de exploracion', 'Duplicar', { timeOut: 5000 });
//    }
//    return false;
//});

//ESTE EVENTO LO BINDEAMOS TANTO AL BOTON BUSCAR PACIENTE, EN CUYO CASO TENEMOS QUE IR A LA PANTALLA DE BUSQUEDA DE PACIENTES Y CAMBIAR DATOS DE FILIACION
//O BIEN, SI PULSAMOS SOBRE EL CARRITO PARA DUPLICAR, SALTAMOS A LA ULTIMA PANTALLA, A LA DE SELECCIONAR LOS TIPOS DE EXPLORACION
$(document).on('click', '#btnBuscarPaciente,.duplicarExploracion', function (ev) {
    var filaActiva = $("tr.ACTIVA");

    if (filaActiva.length === 0) {
        toastr.error('Debe seleccionar un hueco en la lista de exploracion', 'Duplicar', { timeOut: 5000 });
        return false;
    } 

    // now you can search the returned html data using .find().
    var form = document.createElement("form");
    form.setAttribute("action", "/Exploracion/AddPaso1")
    filaActiva.each(function (i, row) {
        var $rowActual = $(row);
        var hora = $rowActual.data('hhora');
        if (hora.length === 0) {
            hora = $rowActual.data('hora');
        }       
        var input = document.createElement("input");
        input.setAttribute("type", "hidden");
        input.setAttribute("name", "HUECOS[" + i + "].HORA");
        input.setAttribute("value", hora);
        form.appendChild(input);
        var input = document.createElement("input");
        input.setAttribute("type", "hidden");
        input.setAttribute("name", "HUECOS[" + i + "].FECHA");
        input.setAttribute("value", $rowActual.data('fecha'));
        form.appendChild(input);
        var input = document.createElement("input");
        input.setAttribute("type", "hidden");
        input.setAttribute("name", "HUECOS[" + i + "].IOR_APARATO");
        input.setAttribute("value", $rowActual.data('aparato'));
        form.appendChild(input);
    });
    document.body.appendChild(form);
    form.submit();
    //var hueco = {};
    //var trigger = $(this);

    //$("tr.ACTIVA").each(function () {
    //    $this = $(this);

    //    var hueco = {
    //        HORA: $this.data("hhora"),
    //        FECHA: $this.data("fecha"),
    //        IOR_APARATO: $this.data("aparato")
    //    };

    //    if (trigger.hasClass('duplicarExploracion')) {
    //        hueco.IOR_PACIENTE = $(trigger).data('iorpaciente');
    //    }

    //    huecosReservados.push(hueco);
    //});

    //guardamos los huecos seleccionados en el lado del servidor para poder mostrarlas en el ultimo paso. 
    //var huecos = JSON.stringify({ 'oLista': huecosReservados });
    //$.ajax({
    //    url: '/AgendaMulti/GuardarHuecosSeleccionados',
    //    data: huecos,
    //    async:false,
    //    contentType: 'application/json; charset=utf-8',
    //    type: 'POST',
    //    success: function (data) {

    //    }
    //});
    //var condicionesBusqueda = new Array;
    //var aparatos = $(".aparato");
    //var fechas = $(".fecha");

    //for (var i = 0; i < aparatos.length; i++) {
    //    var condicionBusqueda = {};
    //    condicionBusqueda.Fecha = fechas[i].value;
    //    condicionBusqueda.oidAparato = aparatos[i].value;
    //    condicionesBusqueda.push(condicionBusqueda);
    //}

    //var condiciones = JSON.stringify({ 'oLista': condicionesBusqueda });
    //$.ajax({
    //    url: '/AgendaMulti/GuardarCondicionesBusqueda',
    //    data: condiciones,
    //    contentType: 'application/json; charset=utf-8',
    //    type: 'POST',
    //    success: function (data) {

    //    }
    //});

    //if (trigger.hasClass('duplicarExploracion')) {
    //    window.location = "/AgendaMulti/AddPaso3/" + $(trigger).data('iorpaciente');
    //    return false;
    //} else {
    //    window.location = "/AgendaMulti/AddPaso1";
    //    return true;
    //}
    
    
});

function dateChanged(ev) {

    var fechaSeleccionada = $(this).val();
    var idSelect = $(this).parents(".contenedorLista").attr('id');

    if (idSelect === "container0") {
        var modo = $('input[name=optModo]:checked').val();
        var filtros = getFiltrosBusqueda("container0");
        //si estamos en modo mismo aparato, tenemos que modificar todos los buscadores
        if (modo === "mismoAparato") {
            LoadBusquedaMultiple("wrapperBuscadores", filtros, true, "ASC", 8);
        } else {
            for (var i = 0; i < 8; i++) {
                filtros = getFiltrosBusqueda("container" + i.toString());
                filtros.Fecha = fechaSeleccionada;
                filtros.oidAparato = ($("#container" + i.toString()).find('.aparato option:selected')).val();
                LoadBusquedaMultiple("container" + i.toString(), filtros, true, "ASC");
            }
        }

    }
    else {
        LoadBusquedaMultiple(idSelect);
    }
}

//los evento change y mover son up y down de todos los filtros del calendario
$(document).on('change', '.aparato', function () {
    var idSelect = $(this).parents(".contenedorLista").attr('id');
    //Si el combo que ha modificado es el del primer container
    if (idSelect === "container0") {
        var modo = $('input[name=optModo]:checked').val();
        var filtros = getFiltrosBusqueda("container0");
        var fechaSeleccionada = filtros.Fecha;
        //si estamos en modo mismo aparato, tenemos que modificar todos los buscadores
        if (modo === "mismoAparato") {
            LoadBusquedaMultiple("wrapperBuscadores", filtros, true, "ASC", 8);
        } else {
            for (var i = 0; i < 8; i++) {
                filtros = getFiltrosBusqueda("container" + i.toString());
                filtros.Fecha = fechaSeleccionada;
                filtros.oidAparato = ($("#container" + i.toString()).find('.aparato option:selected')).val();
                LoadBusquedaMultiple("container" + i.toString(), filtros, true, "DESC");
            }
        }
    }
    else {
        LoadBusquedaMultiple(idSelect);
    }
});




//fecha de cada cuadro del listadia
$(document).on('click', '.date-picker,.aparato', function (ev) {

    $('.panel').removeClass('panel-primary');
    $(this).parents('.panel').addClass('panel-primary');    
  
});

$(document).on('click', '#btnAnterior', function (ev) {
    var modo = $('input[name=optModo]:checked').val();
    var filtros = getFiltrosBusqueda("container0");
    filtros.Fecha = addDays($("#container0").find(".fecha").val(), -1);
    var fechaSeleccionada = filtros.Fecha;

    if (modo === "mismoAparato") {
        $("#container3").empty();
        $("#container3").html($("#container2").find(".panel"));
        $("#container2").empty();
        $("#container2").html($("#container1").find(".panel"));
        $("#container1").empty();
        $("#container1").html($("#container0").find(".panel"));
        $("#container0").empty();
        LoadBusquedaMultiple("container0", filtros, true, "DESC");
    }
    else {
        for (var i = 0; i < 8; i++) {
            filtros = getFiltrosBusqueda("container" + i.toString());
            filtros.Fecha = fechaSeleccionada;
            filtros.oidAparato = ($("#container" + i.toString()).find('.aparato option:selected')).val();
            LoadBusquedaMultiple("container" + i.toString(), filtros, true, "DESC");        }
    }

    ev.stopPropagation();
    return false;
});

$(document).on('click', '#btnSiguiente', function (ev) {
    var modo = $('input[name=optModo]:checked').val();
    var filtros = getFiltrosBusqueda("container3");
    filtros.Fecha = addDays($("#container3").find(".fecha").val(), 1);
    var fechaSeleccionada = filtros.Fecha;
    if (modo === "mismoAparato") {
        $("#container0").empty();
        $("#container0").html($("#container1").find(".panel"));
        $("#container1").empty();
        $("#container1").html($("#container2").find(".panel"));
        $("#container2").empty();
        $("#container2").html($("#container3").find(".panel"));
        $("#container3").empty();
        LoadBusquedaMultiple("container3", filtros, true);
        ev.stopPropagation();
    } else {
        for (var i = 0; i < 8; i++) {
            filtros = getFiltrosBusqueda("container" + i.toString());
            filtros.Fecha = fechaSeleccionada;
            filtros.oidAparato = ($("#container" + i.toString()).find('.aparato option:selected')).val();
            LoadBusquedaMultiple("container" + i.toString(), filtros, true, "ASC");
        }
    }
    return false;
});




$(document).on('click', '.panel-body table tbody tr', function () {
    $('.ACTIVACONTEXTUAL').removeClass('ACTIVA');

    if (ctrlPressed) {
        if ($(this).hasClass('huecoLibre')) {
            if ($(this).hasClass('ACTIVA')) {
                $(this).removeClass('ACTIVA');
            } else {
                $(this).addClass('ACTIVA');
            }
        }
    } else {
        if ($(this).hasClass('ACTIVA')) {
            $(this).siblings().removeClass('ACTIVA');
            $(this).removeClass('ACTIVA');
            $(this).parents('.panel').removeClass('panel-danger');
        } else {
            if ($(this).hasClass('huecoLibre')) {
                $(this).siblings().removeClass('ACTIVA');
                $(this).addClass('ACTIVA');
            }
        }

    }
    if ($(this).hasClass('ACTIVA')) {
        $(this).parents('.panel').addClass('panel-danger');
    }

    if ($('.ACTIVA').length > 0) {
        $('#btnBuscarPaciente').removeClass('disabled');
    } else {
        $('#btnBuscarPaciente').addClass('disabled');
    }
});

//los evento change del modo en Agenda Multiple
$(document).on('change', 'input[name=optModo]', function () {
    var modo = $(this).val();   
    var filtros = getFiltrosBusqueda("container0");
    filtros.modoAgendaMultiple = modo;
    SaveFilters(filtros);

    $("#container0").find('.aparato').trigger('change');

});

$(document).on('change', '.grupo', function () {
    var oidCentro = $(this).val();
    $.ajax({
        type: 'POST',
        url: '/Aparato/GetAparatosPorGrupo',
        data: { oidGrupo: $(this).val() },
        async: 'false',
        success: function (data) {
            
            var sel = $('.aparato');
            sel.empty();
            var markup = '<option value="-1"> </option>';
            for (var x = 0; x < data.length; x++) {
                markup += '<option value="' + data[x].OID + '">' + data[x].COD_FIL + '</option>';
            }
            sel.html(markup).show();
            $('select[class=aparato]').val(-1);
        }
    });
});

function filterRows(statusName) {
    $(".listadiaMultiple tbody tr").each(function () {
        if (!$(this).hasClass(statusName) && !$(this).hasClass('ACTIVA')) {
            $(this).hide();
        }
    });
}
$(document).on('change', '#ddlCentros', function () {
    var oidCentro = $(this).val();
    $.ajax({
        type: 'POST',
        url: '/Aparato/GetAparatosPorCentro',
        data: { oidCentro: oidCentro },
        async: 'false',
        success: function (data) {
            var sel = $('.aparato');
            sel.empty();
            var markup = '<option value="-1"> </option>';
            for (var x = 0; x < data.length; x++) {
                markup += '<option value="' + data[x].OID + '">' + data[x].COD_FIL + '</option>';
            }
            sel.html(markup).show();
            $('select[class=aparato]').val(-1);
            var filtros = getFiltrosBusqueda("container0");
            filtros.oidCentro = oidCentro;
            SaveFilters(filtros);
        }
    });
});

$(document).on('change', '#ddlEmail', function () {
    var idPlantillaMail = $(this).data('oid');
    var texto = $("#ddlEmail option[value=" + $("#ddlEmail").val() + "]").data('contenido');
    $("#textoSMS").val(texto);
});


$(document).on("click", "#EnviarSMS", function () {
    var filaActiva = $("#ExploracionesTable tbody tr.ACTIVA");
    var oidExploracion = filaActiva.data('oid');

    var permissionSMS = $("#ENVIO_SMS").val();
    var acceptSend = (permissionSMS === "T" ? false : true);

    if (acceptSend) {
        var options = {
            url: "/SMS/Enviar",
            data: "phone=" + $("#movilEnvioSMS").val() + "&texto=" + $("#textoSMS").val() + "&idMensaje=" + oidExploracion,
            type: "GET"
        };

        $.ajax(options).complete(function (data) {
            toastr.success('Enviado correctamente', 'SMS', {
                timeOut: 3000,
                positionClass: 'toast-bottom-right'
            });
        });
    } else {
        toastr.error('Segun la LOPD, este paciente no consiente el envio de SMS', 'SMS No enviado', {
            timeOut: 3000,
            positionClass: 'toast-bottom-right'
        });
    }

});


$(document).ready(function () {
    
    $(".date-picker").datepicker({
        format: "dd/mm/yyyy",
        todayBtn: true,
        language: "es",
        autoclose: true,
        todayHighlight: true
    }).on('changeDate', dateChanged);

    if (getParameterByName("oidPaciente") ) {
        $(".huecoOcupado[data-ior_paciente='" + getParameterByName("oidPaciente") + "']").addClass('ACTIVAALTA');
    }

   
    // Add slimscroll to element
    $('.scroll_content').slimscroll({
        height: '900px',
        alwaysVisible: true
    });


    $('.contenedorLista').each(function () {
        activarMenuContextual("#" + this.id);        
    });

    loadExploracionesPersonales();
  

    $("#CarritoCitas").on('shown.bs.popover', function () {        
        $(".popover").css("min-width", "530px");
        $(".popover-content").css("min-width", "530px");
        //$(".trasladarExploracion ").addClass("hide");
    });

    var elementoHuecos = document.querySelector('#chkHuecos');
    var switcheryHuecos = new Switchery(elementoHuecos, { color: '#ED5565' });

    elementoHuecos.onchange = function () {
       
        if (oFiltros.SoloHuecos) {
            
            filterRows('huecoLibre', false);
        } else {            

            filterRows('huecoLibre', true);

        }
    };
});




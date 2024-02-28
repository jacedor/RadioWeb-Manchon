//Cuando el documento este cargado
var chat = null;

var arrayBusquedas;

$.validator.addMethod("nombrePacienteCheck", function (value, element) {
    return value.toString().contains(",");
}, 'Debe separar por una coma los apellidos y el nombre');

//$.validator.addMethod("ddlNoAsignadoCheck", function (value, element) {
//    return alert(value!=-1);
//});

jQuery(function ($) {
    $("#ajax_loader").ajaxStop(function () {
        $(this).hide();
    });
    $("#ajax_loader").ajaxStart(function () {

        $(this).show();
    });
});

$.urlParam = function (name, url) {
    if (!url) {
        url = window.location.href;
    }
    var results = new RegExp('[\\?&]' + name + '=([^&#]*)').exec(url);
    if (!results) {
        return undefined;
    }
    return results[1] || undefined;
}

$(document).ready(function () {



    $("body").tooltip({ selector: '[data-toggle=tooltip]' });
    $("body").popover({ selector: '[data-toggle=popover]' });

    $('[type="datetime"]').mask('00/00/0000');
    $('.time').mask('00:00');

    jQuery('.mainnav li ').each(function () {
        var href = jQuery(this).find('a').attr('href');
        if (href === window.location.pathname) {
            jQuery(this).addClass('active');
        }
    });

    $("input[data-autocomplete]").each(function () {
        CrearAutoComplete($(this));
    });

    //Evento que permite buscar al hacer intro en la caja de texto
    $("input[data-autocomplete]").keydown(function (e) {
        switch (e.which) {
            case 13:
                submitAutoCompleteForm();
            default: return;

        }
    });

    $.datepicker.regional['es'] = {
        closeText: 'Cerrar',
        prevText: '&#x3c;Ant',
        nextText: 'Sig&#x3e;',
        currentText: 'Hoy',
        monthNames: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio',
        'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
        monthNamesShort: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun',
        'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
        dayNames: ['Domingo', 'Lunes', 'Martes', 'Mi&eacute;rcoles', 'Jueves', 'Viernes', 'S&aacute;bado'],
        dayNamesShort: ['Dom', 'Lun', 'Mar', 'Mi&eacute;', 'Juv', 'Vie', 'S&aacute;b'],
        dayNamesMin: ['Do', 'Lu', 'Ma', 'Mi', 'Ju', 'Vi', 'S&aacute;'],
        weekHeader: 'Sm',
        dateFormat: 'dd/mm/yy',
        firstDay: 1,
        isRTL: false,
        showMonthAfterYear: false,
        yearSuffix: ''
    };

    $.datepicker.setDefaults($.datepicker.regional['es']);

    $("input[type='datetime']").datepicker({
        dateFormat: 'dd-mm-yy',
        changeMonth: true,
        changeYear: true

    });




    chat = $.connection.chat;

    if ($('#UsuarioLogeado').data('username').length > 0) {

        var usuarioLogeado = $('#UsuarioLogeado').data('username');
        $.connection.hub.start().done(function myfunction() {
            chat.server.addConnectedUser(usuarioLogeado);
        }

        );
    }
    // Add a client-side hub method that the server will call
    chat.client.updateLabelCount = function (total) {
        $('#OnlineUsers').text(total + ' User Online');
    }
    // Add client-side hub methods that the server will call
    $.extend(chat.client, {
        updateLabelCount: function (total) {
            $('#OnlineUsers').text(total + ' User Online');
        },
        updateExploracionStatus: function (oid) {
            if ($('*[data-oid=' + oid + ']').length > 0) {

                var tr = $('#ExploracionesTable tbody tr[data-oid=' + oid + ']');
                var options = {
                    url: "/Exploracion/GetExploracionRow",
                    data: "oidExploracion=" + oid + "&" + "hhora=" + tr.data('hhora'),
                    type: "GET",
                    dataType: "html"

                };

                $.ajax(options).done(function (data) {
                    var trServer = $(data).find('tr');
                    tr.replaceWith(trServer);
                    var bg = '154,240,117';
                    $row.flash(bg, 1000);
                });

            }
            else {

            }
        },
        updateExploracionAdded: function (oid, hhora) {
            if ($('#ExploracionesTable tbody tr[data-oid="' + oid + '"]').length > 0) {
                var trOrigen = $('#ExploracionesTable tbody tr[data-oid="' + oid + '"]');
                trOrigen.remove();
                if ($('#ExploracionesTable tbody tr[data-hhora="' + hhora + '"]').length > 0) {

                    var trDestino = $('#ExploracionesTable tbody tr[data-hhora="' + hhora + '"]');
                    var options = {
                        url: "/Exploracion/GetExploracionRow",
                        data: "oidExploracion=" + oid + "&" + "hhora=" + trDestino.data('hhora'),
                        type: "GET",
                        dataType: "html"

                    };

                    $.ajax(options).done(function (data) {
                        var trServer = $(data).find('tr');
                        trDestino.replaceWith(trServer);
                        var bg = '154,240,117';
                        $row.flash(bg, 1000);
                    });

                }
                else {

                }
            }
        }




    });



    if (sessionStorage.busquedas != null && sessionStorage.busquedas.length > 0) {

        arrayBusquedas = FixedQueue(10, JSON.parse(sessionStorage.busquedas));

    }
    else {
        arrayBusquedas = FixedQueue(10, []);
    }
});



//$(window).unload(
//    function () {
//        alert('cerrando');
//        $.connection.hub.stop();
//    });



(function ($) {
    $.fn.setCursorToTextEnd = function () {
        var $initialVal = this.val();
        this.val($initialVal);
    };
})(jQuery);


$(document).on('click', '.itemDropButton', function () {
    var texto = $(this).text();
    var idCajaTexto = $(this).parents('ul').siblings('button').attr('data-textbox-id')
    $('#' + idCajaTexto).focus();
    $(this).parents('ul').siblings('button').text(texto).append('&nbsp;<span class=caret></span>');

});

function addDays(date, days) {
    var strDate = date;
    var dateParts = strDate.split("-");
    var result = new Date(dateParts[2], parseInt(dateParts[1]) - 1, dateParts[0]);

    result.setDate(result.getDate() + days);
    return moment(result).format('DD-MM-YYYY');
}

function BuscarDesdeUltimasBusquedas(triger) {
    var filtroBusqueda = triger.data('filter');
    var url = "/Paciente/Index/"; //The Url to the Action  Method of the Controller
    $.ajax({
        type: 'POST',
        url: url,
        data: 'NumRows=' + $("#NumRows").val() + '&Paciente=' + filtroBusqueda + '&Field=' + $("#btnCriteria").text(),
        dataType: "html",
        success: function (evt) {
            $('.PacientesList').html(evt);
            $('.ui-tooltip').tooltip();
            $('.spinnerPacientes').addClass('hide');
            sessionStorage.filterPaciente = filtroBusqueda;
            $("#FindPaciente").val(filtroBusqueda);
            $("#panel-ultimas-busquedas").modal('hide');
        },
    });
}

function AnadirA10Busquedas(paciente) {

    if (!YaExisteEnArray(paciente)) {
        var busqueda = {};
        busqueda.paciente = paciente.trim().toUpperCase();
        var d = new Date();
        var time = d.getHours() + ":" + d.getMinutes() + ":" + d.getSeconds();
        busqueda.hora = time;
        arrayBusquedas.unshift(busqueda);
        sessionStorage["busquedas"] = JSON.stringify(arrayBusquedas);
    }
}


function AddExploracion(oExploracion) {
    var result = false;
    oExploracion.IOR_TIPOEXPLORACION = oExploracion.IOR_TIPOEXPLORACION;
    oExploracion.IOR_PACIENTE = oExploracion.IOR_PACIENTE;
    oExploracion.IOR_APARATO = sessionStorage.valAparato;
    oExploracion.IOR_ENTIDADPAGADORA = $("#MutuaSelectAlta").val();
    oExploracion.RECOGIDO = "F";
    oExploracion.HORA_IDEN = moment().format('HH:mm');
    oExploracion.HORAMOD = moment().format('HH:mm');
    oExploracion.FECHA_IDEN = moment().format('DD-MM-YYYY');
    oExploracion.TEXTO = oExploracion.TEXTO;
    oExploracion.IOR_TECNICO = "-1";
    oExploracion.IOR_CODIGORX = "-1";

    var request = $.ajax({
        url: "/Exploracion/Add",
        data: JSON.stringify({ oExploracion: oExploracion }),
        contentType: 'application/json',
        dataType: 'html',
        async: false,
        type: "POST"
    });

    request.done(function (data) {
        $.growl.notice({ title: "Exploracion creada.", message: oExploracion.HORA + " - " + oExploracion.TIPOEXPLORACIONDESC });
        result = true;

    });
    request.fail(function (jqXHR, textStatus) {
        $.growl.error({ title: "Error al crear la exploracion.", message: oExploracion.HORA + " - " + oExploracion.TIPOEXPLORACIONDESC });

    });

    return result;

}

function GuardarMultiplesExploraciones() {

    var huecosTotal = $("#huecosMultiplesContainer .row").length;
    var j = 1;
    $("#ModalAddExploracionMultiple").modal('hide');
    $("#ModalAddExploracion").modal('hide');
    $("#ajax_loader").show();

    $("#huecosMultiplesContainer .row").each(function (i, row) {
        var Exploracion = {};
        $("#lblInfo").html('').html('INSERTANDO ' + j + ' DE ' + huecosTotal);
        Exploracion.PACIENTE = $(this).data("paciente");// $("#PacienteSelect option[value=" + $("#PacienteSelect").val() + "]").text();
        Exploracion.IOR_PACIENTE = $(this).data("iorpaciente");
        Exploracion.HORA = $(this).data("hora");
        Exploracion.IOR_TIPOEXPLORACION = $(this).find("#ddlExploracionExplo" + j.toString()).val();
        //var ddlTipoExploracion =
        Exploracion.TIPOEXPLORACIONDESC = $(this).find("#ddlExploracionExplo" + j.toString() + " option[value=" + Exploracion.IOR_TIPOEXPLORACION + "]").data('text-value');// ddlTipoExploracion.find("option[value=" + ddlTipoExploracion.val() + "]").data('text');
        Exploracion.FECHA = $(this).data("fecha");
        Exploracion.TEXTO = $(this).find(".texto").val();
        sessionStorage.valAparato = $(this).find("#ddlAparatoMultiple" + j.toString()).val();
        if (AddExploracion(Exploracion)) {
            var rowHueco = $("tr.ACTIVA[data-hhora='" + Exploracion.HORA + "'][data-fecha='" + Exploracion.FECHA + "']");
            $(rowHueco).removeClass('huecoLibre').css("color", "white");
            $(rowHueco).find('.exploracion').html('').html(Exploracion.TIPOEXPLORACIONDESC).css("color", "white");;//.css("color", "blue");
            $(rowHueco).find('.hora').html('').html(Exploracion.HORA).css("color", "white");;//.css("color", "blue");
            $(rowHueco).find('.nombrePaciente').html('').html(Exploracion.PACIENTE).css("color", "white");;//.css("color", "blue");
            $(rowHueco).removeClass('ACTIVA').addClass('HUECODADO');
            j = j + 1;
        }
    });
    $("#ajax_loader").hide();
}


function CrearOEditarPaciente() {
    $("#frmPaciente").validate({
        rules: {
            email: {
                required: false,
                email: true
            },
            fNacimiento: {
                required: false,
                date: true
            },
            nombrePaciente: {
                required: true,
                nombrePacienteCheck: true
            }
        },
        messages: {
            required: {
                required: "Campo obligatorio"
            },
            email: {
                email: "El campo email debe tener este formato: direccion@dominio.com"
            },
            fNacimiento: {
                fNacimiento: "Fecha incorrecta"
            }
        }
    });
    if ($("#frmPaciente").valid()) {
        //Serializamos el objeto paciente
        Paciente.OID = $('#AddOrEdit').data('oid');
        Paciente.AVISO = $('#chkAviso').is(':checked');
        Paciente.CID = $("#ddlMutuasPaciente option[value=" + $("#ddlMutuasPaciente").val() + "]").val();
        Paciente.CIP = $('#CIP').val();
        Paciente.COMENTARIO = $('#TextoNoImprimible').val();
        Paciente.DNI = $('#dni').val();
        Paciente.EMAIL = $('#email').val();
        Paciente.FECHAN = $('#fNacimiento').val();
        Paciente.IOR_EMPRESA = 4;
        Paciente.OTROS2 = $('#chkLOPD').is(':checked');
        Paciente.PACIENTE1 = $('#nombrePaciente').val();
        Paciente.POLIZA = $('#Poliza').val();
        Paciente.PROFESION = $('#Profesion').val();
        Paciente.RIP = $('#chkRIP').is(':checked');
        Paciente.SEXO = $('#lblSexo').attr("data-sexo-val");
        Paciente.TARJETA = $('#Tarjeta').val();
        Paciente.TRAC = $("#ddlTratamiento option[value=" + $("#ddlTratamiento").val() + "]").val();

        Paciente.TEXTO = $('#TextoImprimible').val();
        Paciente.VIP = $('#chkVIP').is(':checked');
        var DIRECCIONES = [];
        $(".direccion").each(function () {
            var DIRECCION = {};
            var oidDireccion = $(this).attr('id');
            DIRECCION.OID = oidDireccion;
            DIRECCION.DIRECCION1 = $('#DIRECCION' + oidDireccion).val();
            DIRECCION.CP = $('#CP' + oidDireccion).val();
            DIRECCION.POBLACION = $('#POBLACION' + oidDireccion).val();
            DIRECCION.PROVINCIA = $('#PROVINCIA' + oidDireccion).val();
            DIRECCION.PAIS = $('#PAIS' + oidDireccion).val();
            DIRECCIONES.push(DIRECCION);
        });
        Paciente.DIRECCIONES = DIRECCIONES;

        var TELEFONOS = [];
        $(".telefono").each(function () {
            var TELEFONO = {};
            var oidTelefono = $(this).data('oid');
            TELEFONO.OID = oidTelefono;
            TELEFONO.LOCALIZACION = $('#LOCALIZACION' + oidTelefono).val();
            TELEFONO.NUMERO = $('#NUMERO' + oidTelefono).val();

            TELEFONOS.push(TELEFONO);
        });
        Paciente.TELEFONOS = TELEFONOS;

        //Insertamos el Paciente en la Base de Datos
        var optionsInsertar = {
            url: "/Paciente/Add",
            data: JSON.stringify({ oPaciente: Paciente }),
            contentType: 'application/json',
            type: "POST",
            async: false,
            cache: false

        };

        $.ajax(optionsInsertar).success(function (data) {
            //Si la inserción ha ido bien
            if (data.oid === -1) {

                sessionStorage.OidUltimoPaciente = -1;
               

            }
                //Si se produce un error al insertar en Paciente
            else {
                sessionStorage.OidUltimoPaciente = data.oid;
               
            }

        });

    }
    else {
        sessionStorage.OidUltimoPaciente = 1;
        $.growl.error({ title: "Revise todos los campos!", message: "" });
    }
}





function YaExisteEnArray(cadena) {
    var result = false;
    for (var x = 0; x < arrayBusquedas.length; x++) {
        if (arrayBusquedas[x].paciente == cadena) {
            result = true;
        }
    }
    return result;
}
$(document).on("dblclick", ".filterRow", function (e) {

    BuscarDesdeUltimasBusquedas($(this));
    $(".popover").popover('hide');

});

$(document).on("click", ".selectBusqueda", function () {
    BuscarDesdeUltimasBusquedas($(this));
    $(".popover").popover('hide');
});



var submitAutoCompleteForm = function (event, ui) {

    var $input = $(this);
    if (ui != null) {
        $input.val(ui.item.label);
    }
    var $form = $("form[data-manchon-ajax='true']");

    $form.submit();
}


// A simple background color flash effect that uses jQuery Color plugin
jQuery.fn.flash = function (color, duration) {
    var current = this.css('backgroundColor');
    this.animate({ backgroundColor: 'rgb(' + color + ')' }, duration / 2)
        .animate({ backgroundColor: current }, duration / 2);
};


function SignalRActualizarEstado(oid) {
    chat.server.actualizarEstadoExploracion(oid);
};

function SignalRActualizarExploracionAnadida(oid, hhora) {
    chat.server.actualizarExploracionAnadida(oid, hhora);
};


function CrearAutoComplete(element) {
    var $input = element;

    var options = {
        source: element.attr("data-autocomplete"),
        select: submitAutoCompleteForm
    }

    $input.autocomplete(options);

}






//********************************************************************************
//***********TABLAS CON FILAS QUE PERMITEN EDICIÓN********************************
//*******************************************************************************
$(document).on('click', '#NuevaDireccion', function () {
    var url_template = $(this).data('url');
    //Vamos al servidor a buscar los detalles de este paciente
    var options = {
        url: url_template,
        contentType: 'application/json',
        dataType: 'html',
        type: "GET"

    };
    $.ajax(options).done(function (data) {
        var $target = $("#DireccionesContainer");

        $("#DireccionesContainer > .direccion").removeClass('active');
        $target.append(data);
        $("#NuevaDireccionTab").show();
        $("#DIRECCION-1").addClass('active').focus();

    });



});


$(document).on('click', '#BorrarDireccion', function () {
    var url_template = '/Direccion/Delete';
    var oid = $(".direccion.active").attr('id');
    var owner = $(".direccion.active").data('owner');

    //Vamos al servidor a buscar los detalles de este paciente
    var options = {
        url: url_template,
        data: { 'oid': oid, 'owner': owner },
        contentType: 'application/json',
        dataType: 'html',
        type: "GET"

    };
    $.ajax(options).done(function (data) {
        $("fieldset.direcciones").html(data);

    });

});


$(document).on('click', '#NuevoTelefono', function () {
    var url_template = $(this).data('url');
    //Vamos al servidor a buscar los detalles de este paciente
    var options = {
        url: url_template,
        dataType: 'html',
        type: "GET"

    };
    $.ajax(options).done(function (data) {
        var $target = $(".listaTelefonos");
        $target.append(data);
    });



});


$(document).on('click', '.borrarTelefono', function () {
    $(this).parents('tr').remove();


});

$(document).on('change keyup', '#ddlExploracionExplo', $.debounce(100, function () {
    var IOR_APARATO = $("#ddlExploracionExplo").val();
    $('#textoDeLaMutua').remove();
    $('#lineaSeparadora').remove();
    //Nos Traemos el texto de la mutua
    $.ajax({
        type: 'POST',
        url: '/Exploracion/GetTextoExploraciones',
        data: { oidAparato: IOR_APARATO },
        async: 'false',
        beforeSend: function () {

        },
        success: function (data) {
            if (data.length > 0) {
                $("#ExploracionResumen").hide();
                $('#textoDeLaMutua').remove();
                $('#lineaSeparadora').remove();
                var $iframe = "<iframe id='textoDeLaMutua'  src='#' width='1000' height='60' style='border:none;' ></iframe><hr id='lineaSeparadora'/>";
                var $Paso3 = $('#ExploracionPaso3');
                $Paso3.before($iframe);
                $("#textoDeLaMutua").contents().find("body").html(data);

            }
            else {
                $("#ExploracionResumen").show();
            }
        }
    });
}));

$(document).on('change keyup', '#ddlMutuasExplo', $.debounce(100, function () {
    var IOR_ENTIDADPAGADORA = $("#ddlMutuasExplo").val();
    $('#textoDeLaMutua').remove();
    $('#lineaSeparadora').remove();
    //Nos Traemos el texto de la mutua
    $.ajax({
        type: 'POST',
        url: '/Mutua/TextoMutua',
        data: { oidMutua: IOR_ENTIDADPAGADORA },
        async: 'false',
        beforeSend: function () {

        },
        success: function (data) {
            if (data.length > 0) {
                $("#ExploracionResumen").hide();
                $('#textoDeLaMutua').remove();
                $('#lineaSeparadora').remove();
                var $iframe = "<iframe id='textoDeLaMutua'  src='#' width='1000' height='120' style='border:none;' ></iframe><hr id='lineaSeparadora'/>";
                var $Paso3 = $('#ExploracionPaso3');
                $Paso3.before($iframe);
                $("#textoDeLaMutua").contents().find("body").html(data);

            }
            else {
                $("#ExploracionResumen").show();
            }
        }
    });
}));


// evento click del SELECT DE TIPO DE EXPLORACION QUE CAMBIA EL PRECIO
$(document).on('change keyup', '#ddlExploracionExplo', function () {

    var IOR_TIPOEXPLORACION = $("#ddlExploracionExplo").val();
    var IOR_ENTIDADPAGADORA = $("#ddlMutuasExplo").val();


    var options = {
        url: "/Exploracion/GetPrecioExploracion",
        data: ({ IOR_TIPOEXPLORACION: IOR_TIPOEXPLORACION, IOR_MUTUA: IOR_ENTIDADPAGADORA }),
        dataType: "html",
        type: "GET"
    };

    $.ajax(options).complete(function (data) {

        $("#Cantidad").val(data.responseText);
    });



});

// evento click del SELECT DEL APARATO QUE RELLENA LAS EXPLORACIONES EN APARATOS COMPLEJOS
$(document).on('change keyup', 'select[id*=ddlAparatoMultiple]', function () {

    var IOR_ENTIDADPAGADORA = $("#MutuaSelectAlta").val();
    var ddlAparato = $(this);
    var options = {
        url: "/Exploracion/GetTipoExploraciones",
        data: "IOR_APARATO=" + $(this).val() + "&" + "IOR_MUTUA=" + IOR_ENTIDADPAGADORA
    };

    $.ajax(options).success(function (data) {

        var indiceActual = $(ddlAparato).attr("id").substr($(ddlAparato).attr("id").length - 1);
        var sel = $("#ddlExploracionExplo" + indiceActual);
        sel.empty();
        var markup = '';
        for (var x = 0; x < data.length; x++) {

            markup += '<option value="' + data[x].OID + '" data-cod="' + data[x].FIL + '" data-text-value="' + data[x].FIL + '">' + data[x].FIL + ' - ' + data[x].DES_FIL + '</option>';


        }
        sel.html(markup).show();




    });
});
// evento click del SELECT DEL APARATO QUE RELLENA LAS EXPLORACIONES
$(document).on('change keyup', '.ddlAparatosMutua, .ddlAparatosPri, .ddlMutuaExploracion', function () {


    var IOR_ENTIDADPAGADORA = $("#ddlMutuasExplo").val();
    sessionStorage.textGrupo = ' ';
    sessionStorage.textAparato = $("#ddlAparatosExplo option[value=" + $("#ddlAparatosExplo").val() + "]").data('cod');
    sessionStorage.valAparato = $("#ddlAparatosExplo option[value=" + $("#ddlAparatosExplo").val() + "]").val();

    var cantidad = $('#Cantidad').val('');
    var options = {
        url: "/Exploracion/GetTipoExploraciones",
        data: "IOR_APARATO=" + sessionStorage.valAparato + "&" + "IOR_MUTUA=" + IOR_ENTIDADPAGADORA
    };

    $.ajax(options).success(function (data) {

        var sel = $('#ddlExploracionExplo');
        sel.empty();
        var markup = '';
        for (var x = 0; x < data.length; x++) {

            markup += '<option value="' + data[x].OID + '" data-cod="' + data[x].FIL + '" data-text-value="' + data[x].DES_FIL + '">' + data[x].FIL + ' - ' + data[x].DES_FIL + '</option>';


        }
        sel.html(markup).show();

        $("#ddlExploracionExplo option:first").attr('selected', 'selected');
    });
});

$(document).on('click', '.PacienteItem', function () {

    $(this).siblings().removeClass('ACTIVA');
    if ($(this).hasClass('ACTIVA')) {
        $('#btnRecordarPaciente').addClass('disabled');
        $('#btnBorrar').addClass('disabled');
        $('#btnRecordarPaciente').removeClass('disabled');
        $('#btnBorrar').removeClass('disabled');

    }
    else {
        $(this).addClass('ACTIVA');
        $('#btnRecordarPaciente').removeClass('disabled');
        $('#btnBorrar').removeClass('disabled');
        //esto almacena el oid de la fila seleccionada, no confundir con la busqueda del ultimo paciente
        //esto solo selecciona el paciente activo en la lista.
        sessionStorage.oidPacienteActivo = $(this).data('oid');

    }

});

$(document).on('click', '.BusquedasTableBody > tr', function () {

    $(this).siblings().removeClass('ACTIVA');

    $(this).addClass('ACTIVA');


});



//al cambiar el tratamiento cambiamos el label con la descripcion del sexo
$(document).on('change keyup', '#ddlTratamiento', function () {

    var sexovalor = "Hombre";

    switch ($("#ddlTratamiento option[value=" + $("#ddlTratamiento").val() + "]").text().toUpperCase().trim()) {

        case "DRA.":
            sexovalor = "Mujer";
            break;
        case "SRA.":
            sexovalor = "Mujer";
            break;
        case "SRTA.":
            sexovalor = "Mujer";
            break;
        case "SRA.  DÑA.":
            sexovalor = "Mujer";
            break;
        case "NIÑO":
            sexovalor = "Niño";
            break;
        case "NIÑA":
            sexovalor = "Niña";
            break;
        default:

    }

    $('#lblSexo').html(sexovalor);

    if ($('#lblSexo').html() == "Hombre") {
        $('#lblSexo').attr("data-sexo-val", "H");
    }
    else if ($('#lblSexo').html() == "Niño") {

    }
    else {
        $('#lblSexo').attr("data-sexo-val", "M");

    }


});

$(document).on('change', '#fNacimiento', function () {
    $('#lblEdad').html(Math.floor(moment(new Date()).diff(moment($(this).val(), "DD/MM/YYYY"), 'years', true)) + " Años");

});

$(document).on('click', "input.TipoExploracion", function () {
    var oExploracion = {};

    $("#ExploWizard-p-2").empty();
    oExploracion.HORA = $("#horaExploracion").val();
    oExploracion.FECHA = $("#fechaExploracion").text().trim();
    oExploracion.IOR_APARATO = $("#ddlAparatosExplo option[value=" + $("#ddlAparatosExplo").val() + "]").val();
    oExploracion.TEXTO = $("#textoExploracion").val();
    oExploracion.RECOGIDO = $('#chkRecogido').checked ? "T" : "F";
    oExploracion.PAGADO = $('#chkPagado').checked ? "T" : "F";
    oExploracion.PERMISO = $('#chkPermiso').checked ? "T" : "F";

    var ENTIDAD_PAGADORA = {};
    oExploracion.ENTIDAD_PAGADORA = ENTIDAD_PAGADORA;
    switch ($(this).val()) {
        case "PRI":
            oExploracion.ENTIDAD_PAGADORA.OWNER = 1;
            oExploracion.ENTIDAD_PAGADORA.OID = 3820080;
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
        var $target = $("#ContenidoExplo");
        if ($target.length == 0) {
            $target = $("#ExploracionPaso3");
        }
        var $newHtml = $(data);
        $target.html(data);


    });
    request.fail(function (jqXHR, textStatus) {
        $.growl.error({ title: "Error al cambiar tipo de exploracion.", message: "" });

    });

});

$(document).on('click', '#btnSalirRadioWeb', function () {


    var options = {
        url: '/Users/LogOut',
        type: "get"
    };

    $.ajax(options).success(function myfunction() {
        window.location = "/";
        sessionStorage.clear();
    });
}
);



$(document).on("click", "#btnUltimoPaciente", function myfunction() {
    var url = "/Paciente/Index/"; //The Url to the Action  Method of the Controller
    $.ajax({
        type: 'POST',
        url: url,
        data: 'NumRows=' + $("#NumRows").val() + '&Paciente=' + sessionStorage.OidUltimoPaciente,
        dataType: "html",
        success: function (evt) {
            $('.PacientesList').html(evt);
            $('.ui-tooltip').tooltip();
            $('.spinnerPacientes').addClass('hide');
        },
    });
});

$(document).on("click", "#btnRecordarPaciente , #RecordarPaciente, #RecordarPacienteYCalendario", function myfunction() {


    var triger = $(this).attr('id');
    var oidPacienteSeleccionado;

    switch (triger) {


        case "btnRecordarPaciente":
            oidPacienteSeleccionado = $(".PacienteItem.ACTIVA").data('oid');
            break;

        case "RecordarPaciente":
            oidPacienteSeleccionado = $("#AddOrEdit").data('oid');
            break;

        case "RecordarPacienteYCalendario":
            oidPacienteSeleccionado = $("#AddOrEdit").data('oid');
            break;
        default:

    }

    sessionStorage.OidUltimoPaciente = oidPacienteSeleccionado;
    $.growl.notice({ title: "Ultimo Paciente actualizado.", message: "" });

    if (triger == "RecordarPacienteYCalendario") {
        window.location = '/Calendario/Index#Calendario';
    }

});


$(document).on("keypress", "input[id^='CP']", function (e) {

    if (e.keyCode == 13) {
        var oidDireccion = $(this).attr('id').substring(2);
        var url_template = '/Direccion/PuebloPorCodigo';
        //Vamos al servidor a buscar los detalles de este paciente
        var options = {
            url: url_template,
            data: { 'term': $(this).val() },
            dataType: 'json',


        };
        $.ajax(options).done(function (data) {

            $("#PROVINCIA" + oidDireccion).val(data.PROVINCIA);
            $("#POBLACION" + oidDireccion).val(data.PUEBLO);



        });


    }
});


//$(document).on("keypress", "input[id^='CP'])", function (e)) {

//    if (e.keyCode == 13) {
//        alert('HOLA');
//    }
//});


//ObtenerPoblacionPorCodigo
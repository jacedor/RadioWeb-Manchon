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

var currentStep = 1;

var oExploracion = {};
var Paciente = {};
var esAlta = false;

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



var stringStartsWith = function (string, startsWith) {
    string = string || "";
    if (startsWith.length > string.length)
        return false;
    return string.substring(0, startsWith.length) === startsWith;
};

function habilitarMenuContextual() {
    //************** MENU CONTEXTUAL DEL LISTA DIA****************
    $('table[id^="AgendaMultiple"]').each(function (i, row) {

        $(this).contextMenu({
            selector: 'tbody tr',
            callback: function (key, options) {
                //var m = "ha echo click en: " + key + " on " + $(this).text();         
                //alert(m);              
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
            },
            events: {
                show: function (opt) {
                    // this is the trigger element
                    var $this = this;
                }
            }
        });
    });



}


function ActualizarAgendasMultiples() {
    sessionStorage.fechaActual = $('#fechaSelect').val();
    sessionStorage.valAparato = $("#ddlAparatoAgMultiple1Apa").val();
    sessionStorage.textAparato = $("#ddlAparatoAgMultiple1Apa option[value=" + $("#ddlAparatoAgMultiple1Apa").val() + "]").text();
    var ContenedorAgendas = $('#bodyAgendasContainer');
    $.ajax({
        type: 'POST',
        url: '/Exploracion/AgendaMultipleMismoAparato',//?fechaInicial=' + sessionStorage.fechaActual + '&Aparato=' + sessionStorage.textAparato',
        data: { fechaInicial: $('#fechaSelect').val(), Aparato: sessionStorage.textAparato },
        beforeSend: function () {
            ContenedorAgendas.html('');
        },
        success: function (data) {
            ContenedorAgendas.html(data);
        }
    });
}

function ActualizarFechaPicker() {
    var strDate = sessionStorage.fechaActual;
    var dateParts = strDate.split("-");

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
$(document).on('click', '#GuardarExploracion', function () {
    GuardarMultiplesExploraciones();
});



$(document).on('change keyup', '#MutuaSelectAlta', function () {


    var IOR_ENTIDADPAGADORA = $("#MutuaSelectAlta").val();

    var options = {
        url: "/Exploracion/GetTipoExploraciones",
        data: "IOR_APARATO=" + sessionStorage.valAparato + "&" + "IOR_MUTUA=" + IOR_ENTIDADPAGADORA
    };

    $.ajax(options).success(function (data) {


        $(".ddlTipoExplo").each(function (i, ddlExplo) {
            var sel = $(ddlExplo);
            sel.empty();
            var markup = '';
            for (var x = 0; x < data.length; x++) {

                markup += '<option value="' + data[x].OID + '" data-cod="' + data[x].FIL + '" data-text-value="' + data[x].FIL + '">' + data[x].FIL + ' - ' + data[x].DES_FIL + '</option>';


            }
            sel.html(markup).show();
        });



    });
});


//los evento change y mover son up y down del filtro por aparato
$(document).on('change', '#ddlAparatoAgMultiple1Apa', function () {
    ActualizarAgendasMultiples();
});


$(document).on('click', ".contenidoListaDia tr", function () {
    //TO DO ACEDO

    $(".contenidoListaDia tr.HUECODADO").each(function(i, row){
        $(row).removeClass("HUECODADO");
        $(row).find("span").each(function(j,celda){
            $(celda).css("color", "blue");
        });
    });
       
   
    if (ctrlPressed) {
        // do something
    } else {
        $(this).siblings().removeClass('ACTIVA');
    }

    if ($(this).hasClass('ACTIVA')) {
        $(this).removeClass('ACTIVA');
    } else {
        $(this).addClass('ACTIVA');
    }

});

//esta funcion, en lugar de cargar los cuatro dias que se visualizan carga solo uno en concreto
//el parametro dirección indica si buscamos el siguiente dia hacia el futuro o hacia el pasado
function ActualizarUnDiaAgendasMultiples(ColumnaDia, fechaRemplazo, direccion) {
    direccion = direccion || "ADELANTE";
    sessionStorage.valAparato = $("#ddlAparatoAgMultiple1Apa").val();
    sessionStorage.textAparato = $("#ddlAparatoAgMultiple1Apa option[value=" + $("#ddlAparatoAgMultiple1Apa").val() + "]").text();
    var ContenedorAgendas = ColumnaDia;
    $.ajax({
        type: 'POST',
        url: '/Exploracion/AgendaMultipleMismoAparato',//?fechaInicial=' + sessionStorage.fechaActual + '&Aparato=' + sessionStorage.textAparato',
        data: { fechaInicial: fechaRemplazo, Aparato: sessionStorage.textAparato, numDias: 1, Direccion: direccion },
        beforeSend: function () {

        },
        success: function (data) {
            if (direccion == "ADELANTE") {
                $(ContenedorAgendas).append(data);
            } else {
                $(ContenedorAgendas).prepend(data);
            }

        }
    });
}


$(document).on('click', '#btnDiaSiguiente', function () {

    $('#fechaSelect').val(addDays(sessionStorage.fechaActual, 1));
    sessionStorage.fechaActual = addDays(sessionStorage.fechaActual, 1);
    $('#bodyAgendasContainer .col-lg-3:first-child').remove();
    var fechaUltimoDia = $('#bodyAgendasContainer .col-lg-3:last-child').data('fecha');
    ActualizarUnDiaAgendasMultiples($('#bodyAgendasContainer'), addDays(fechaUltimoDia, 1));

    return false;
});


$(document).on('click', '#btnDiaAnterior', function () {

    $('#fechaSelect').val(addDays(sessionStorage.fechaActual, -1));
    sessionStorage.fechaActual = addDays(sessionStorage.fechaActual, -1);
    var fechaUltimoDia = $('#bodyAgendasContainer .col-lg-3:first-child').data('fecha');
    $('#bodyAgendasContainer .col-lg-3:last-child').remove();
    ActualizarUnDiaAgendasMultiples($('#bodyAgendasContainer'), addDays(fechaUltimoDia, -1), "ATRAS");


    return false;
});

$(document).on('click', '#btnHoy', function () {

    $('#fechaSelect').val(moment(new Date()).format('DD-MM-YYYY'));
    ActualizarAgendasMultiples();
    return false;
});

$(document).on('click', '#SiguientePasoExploracion', function () {

    switch (currentStep) {

        case 0:
            var $iframe = $('#textoDeLaMutua');
            $iframe.remove();
            $('#ExploracionResumen').hide();
            $('#actionsPacienteList').show();
            break;
            //sI EL CURRENT STEP ES EL uno QUIERE DECIR QUE ESTAMOS PASANDO AL DOS, ES DECIR YA HEMOS SELECCIONADO UN PACIENTE O NO PARA NUEVOS
        case 1:
            var $iframe = $('#textoDeLaMutua');
            $iframe.remove();
            $('#ExploracionResumen').hide();
            $('#actionsPacienteList').hide();
            oExploracion = {};
            var url = '/Paciente/AddForm';
            var paso2Title = "";
            //Si se ha seleccionado algun paciente
            if ($('.PacienteItem.ACTIVA').data('oid') != undefined) {
                //Serialiazomas el objeto paciente para cogerlo de la BBDD
                Paciente.OID = $('.PacienteItem.ACTIVA').data('oid');
                Paciente.PACIENTE1 = $('.PacienteItem.ACTIVA td:first b').text()
                //Establecemos el IOR_PACIENTE de la Exploración
                oExploracion.IOR_PACIENTE = Paciente.OID;
                paso2Title = "Paso2 - Paciente Existente";
            }
            else {
                Paciente.OID = 0;
                Paciente.PACIENTE1 = $('#FindPaciente').val();
                esAlta = true;
                $("#ModalAddExploracion").children("modal-header").css("background-color", "red");
                paso2Title = "Paso2 - Paciente Nuevo";
            }
            //Vamos al servidor a buscar los detalles de este paciente
            var options = {
                url: url,
                data: JSON.stringify({ oMiPaciente: Paciente }),
                contentType: 'application/json',
                dataType: 'html',
                type: "POST"

            };
            $.ajax(options).done(function (data) {
                var $target = $("#contenido");
                var $newHtml = $(data);
                currentStep = 2;
                $('#AddOrEdit').remove();
                $target.append(data);
                $('#BusquedaPaciente').fadeOut(function () {
                    $('#AddOrEdit').fadeIn();
                    $('#myModalLabel').html(paso2Title);
                    $("#nombrePaciente").focus();
                    $('input[type="datetime"]').mask("00/00/0000", { placeholder: "__/__/____" });
                    $('input[type="datetime"]').datepicker({
                        dateFormat: 'dd-mm-yy',
                        changeMonth: true,
                        changeYear: true
                    });
                });
            });
            break;

        case 2:
            var $iframe = $('#textoDeLaMutua');
            $iframe.remove();
            oExploracion = {};
            CrearOEditarPaciente();

            if (sessionStorage.OidUltimoPaciente > 0) {
                oExploracion.IOR_PACIENTE = sessionStorage.OidUltimoPaciente;
                $('#AddOrEdit').data('oid', sessionStorage.OidUltimoPaciente);
                Paciente.OID = sessionStorage.OidUltimoPaciente;
                //En este punto nos guardamos en una variable de session del Navegador con el paciente seleccionado para asignar a la exploración                
                $("#ExploracionPaso3").empty();
                oExploracion.HORA = $('#ExploracionesTable tbody tr.ACTIVA').data('hhora');
                oExploracion.FECHA = sessionStorage.fechaActual;
                oExploracion.IOR_APARATO = sessionStorage.valAparato;
            }
            else {

                $.growl.error({ title: "Error al crear el paciente", message: "" });
            }

            if ($(".ACTIVA").length > 0) {
                var filasActivas = new Array();
                $(".ACTIVA").each(function () {
                    var fila = {};
                    if ($(this).hasClass("huecoLibre")) {
                        fila.ESHUECO = true;
                        fila.HHORA = $(this).data('hhora');
                        fila.FECHA = $(this).data('fecha');
                        fila.IOR_PACIENTE = sessionStorage.OidUltimoPaciente;
                        fila.IOR_APARATO = $("#ddlAparatoAgMultiple1Apa").val();
                        fila.COD_FIL = $("#ddlAparatoAgMultiple1Apa").val();
                    }
                    filasActivas.push(fila);
                });
                var postData = JSON.stringify(filasActivas);
                $.ajax({
                    url: '/Exploracion/AddMultiple',
                    data: postData,
                    contentType: 'application/json',
                    type: "POST",
                    dataType: "html",
                    beforeSend: function () {
                        $("#ModalAddExploracionMultiple").remove();
                    },
                    success: function (data) {

                        var $target = $("#ExploracionPaso3");
                        var $newHtml = $(data);


                        $('#AddOrEdit').fadeOut(function () {
                            $target.html($(data).find(".modal-body"));
                            $('#ExploracionPaso3').fadeIn();
                            $('#myModalLabel').html('Paso 3 - Asignando Exploraciones');

                            $('#footerAddExploracion').show();
                        });
                    }
                });

            }
            else {

                $.growl.error({ title: "Debe seleccionar huecos.", message: "" });
            }

            currentStep = 1;
            oExploracion = null;
            break;


        default:

    }


});


$(document).on('click', '#btnCreate', function () {

    if ($(".ACTIVA[data-anulada='True']").length > 0) {
        $.growl.error({ title: "No puede asignar horas anuladas.", message: "" });
        return false;
    }
    if ($("#bodyAgendasContainer").find(".ACTIVA").length == 0) {
        $.growl.error({ title: "Debe seleccionar huecos.", message: "" });
        return false;
    }
    currentStep = 1;
    var oidPaciente = 0;
    if ($("#PacienteSelect").val().length > 0) {
        oidPaciente = $("#PacienteSelect").val();

    }
    $.ajax({
        url: '/Exploracion/AddModal',
        data: 'oidPaciente=' + oidPaciente,
        type: "GET",
        dataType: "html",
        async:false,
        beforeSend: function () {
            $("#ModalAddExploracion").remove();
        },
        success: function (data) {
           
            $("body").append(data);
            $('#ModalAddExploracion').modal('show');

            if (oidPaciente != 0) {
                currentStep = 2;
                $('#AddOrEdit').show();
                $('#actionsPacienteList').hide();

            }
            else {
                currentStep = 1;
               
               
            }
        }
    });

});


$(document).on('keyup', '#FindPaciente', $.debounce(500, function () {
    //Al escribir sobre la caja de texto del modal popup de pacientes
    //$("#FindPaciente").keyup($.debounce(250, function () {

    var data = $(this).val();
    var url = "/Paciente/Index/"; //The Url to the Action  Method of the Controller
    var Paciente = {}; //The Object to Send Data Back to the Controller
    Paciente.PACIENTE1 = $("#FindPaciente").val();
    // Check whether the TextBox Contains text
    // if it does then make ajax call
    if ($("#FindPaciente").val().length > 3 && $("#FindPaciente").val() != "") {
        $.ajax({
            type: 'POST',
            url: url,
            data: 'NumRows=' + $("#NumRows").val() + '&Paciente=' + $("#FindPaciente").val() + '&Field=' + $("#btnCriteria").text(),
            dataType: "html",
            success: function (evt) {
                $('.PacientesList').html(evt);
                $('.ui-tooltip').tooltip();
                AnadirA10Busquedas($("#FindPaciente").val());

            },
        });
    }

}));

$(document).on('click', '#btnFastCreate', function () {
    if (!$("#PacienteSelect").val().length > 0) {
        $.growl.error({ title: "Debe seleccionar un paciente.", message: "" });
        return false;
    }
    if ($(".ACTIVA[data-anulada='True']").length > 0) {
        $.growl.error({ title: "No puede asignar horas anuladas.", message: "" });
        return false;
    }

    if ($(".ACTIVA").length > 0) {
        var filasActivas = new Array();
        $(".ACTIVA").each(function () {
            var fila = {};
            if ($(this).hasClass("huecoLibre")) {
                fila.ESHUECO = true;
                fila.HHORA = $(this).data('hhora');
                fila.FECHA = $(this).data('fecha');
                fila.IOR_PACIENTE = $("#PacienteSelect").val();
                fila.IOR_APARATO = $("#ddlAparatoAgMultiple1Apa").val();
                fila.COD_FIL = $("#ddlAparatoAgMultiple1Apa").val();
            }
            filasActivas.push(fila);
        });
        var postData = JSON.stringify(filasActivas);
        $.ajax({
            url: '/Exploracion/AddMultiple',
            data: postData,
            contentType: 'application/json',
            type: "POST",
            dataType: "html",
            beforeSend: function () {
                $("#ModalAddExploracionMultiple").remove();
            },
            success: function (data) {
                $("body").append(data);
                $("#ModalAddExploracionMultiple").modal("show");

            }
        });

    }
    else {

        $.growl.error({ title: "Debe seleccionar huecos.", message: "" });
    }
});

$(document).on('shown.bs.modal', '#ModalAddExploracionMultiple', function () { $("#telefono1").editable(); $("#telefono2").editable(); });


$("#ModalAddExploracion").on("shown.bs.modal", function (e) {
    //BUSCAR
    $('#FindPaciente').val('').focus();
   
});
$(document).on('shown.bs.modal', '#ModalAddExploracion', function () {

    $("#FindPaciente").focus();
    $('#ExploracionResumen').hide();

    if (currentStep === 1) {
        $('#ExploracionPaso3').hide();
        $('#BusquedaPaciente').show();
        //$('#actionsPacienteList').show();
    }
});

$(window).bind('scroll', function () {

    var navHeight = $(window).height() - 40;
    if ($(window).scrollTop() < navHeight) {
        $('#AgendasHeader').removeClass('headerAgendaMultiple').addClass('fixed');
    }

    if ($(window).scrollTop() - 40 < 0) {
        $('#AgendasHeader').addClass('headerAgendaMultiple').removeClass('fixed');
    }

});




$(document).ready(function () {
    habilitarMenuContextual();

    $("#fechaSelect").datepicker({
        dateFormat: 'dd-mm-yy',
        changeMonth: true,
        changeYear: true,
        onSelect: function (dateText, inst) {
            sessionStorage.fechaActual = dateText;
            ActualizarAgendasMultiples();
        }
    });


    $("#MutuaSelect").selectize({
        sortField: {
            field: 'text',
            direction: 'asc'
        },
        dropdownParent: 'body'
    });

    var $Select = $('#PacienteSelect').selectize({
        valueField: 'OID',
        labelField: 'PACIENTE1',
        searchField: 'PACIENTE1',
        sortField: 'PACIENTE1',

        options: [],
        create: false,
        //score: function() { return function() { return 1 }; },
        render: {
            item: function (item) {
                return '<div data-value="' + item.OID + '" data-text="' + item.PACIENTE1 + '">' + (item.PACIENTE1 ? '<span class="name">' + item.PACIENTE1 + '</span>' : '<span class="name">' + item.PACIENTE1 + '</span>') + '</div>';
            },
            option: function (item, escape) {
                var poblacion = "";
                var edad = "";
                if (item.DIRECCIONES.length > 0) {
                    poblacion = item.DIRECCIONES[0].POBLACION;
                }
                if (item.EDAD.length > 0) {
                    edad = item.EDAD;
                }

                return '<div>' +
                    '<span class="title">' +
							    '<span class="name">' + escape(item.PACIENTE1) + (edad.length ? ' - ' + edad : '') + '</span>' +
					'</span>' +
					'<span class="description">' + escape(item.DESCMUTUA || 'Privado') + '</span>' +
                    (poblacion.length ? '<span class="actors">' + poblacion + '</span>' : '') +
					'</div>';


            }
        },
        load: function (query, callback) {
            if (!query.length) return callback();
            $.ajax({
                url: '/Paciente/ObtenerJson',
                dataType: 'json',
                data: "buscar=" + query,
                type: 'GET',
                error: function () {
                    callback();
                },
                success: function (res) {
                    callback(res.Pacientes);
                }
            });
        }
    });
});








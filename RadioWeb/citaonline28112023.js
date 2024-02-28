var endPoint = "http://localhost:51839";
var mensajeContactoCDPI = "Para este tipo de Prueba necesita contactar con nosotros. Solicite información en 939393993";
//var endPoint = "https://radioibweb-es.affidea.com";
var huecosReservados = new Array();
var excepcionBalmesOperados = false;

function drawTableCarrito(data) {
    var content = "<table class='table'>";
    for (var i = 0; i < data.length; i++) {
        content = content + drawRowCarrito(data[i]);
    }

    return content + "</table>";
}

function drawRowCarrito(rowData) {
    var row = "<tr>";
    row = row + "<td>" + rowData.fechaHueco + "</td>";
    row = row + "<td>" + rowData.horaHueco + "</td>";
    row = row + "<td>" + rowData.aparatoDesc + "</td>";
    row = row + "<td>" + rowData.Centro.replace("CDPI", "TIBIDABO").replace("CUGAT", "CAN MORA") + "</td>";
    row = row + "<td>" + "<button  data-oid='" + rowData.idHueco + "'  class='btn btn-danger btn-xs BorrarCitaCarrito'><span class='glyphicon glyphicon-trash'></span></button></td>";
    row = row + "</tr>";
    return row;
}
function rellenarMisCitas(bPuedeBorrar) {
    $.ajax({
        url: '/API/Citaonline',
        type: 'GET',
        async: false,
        data: { oidPaciente: getParameterByName("oid") },
        dataType: 'json',
        success: function (huecosEncontrados) {

            if (huecosEncontrados.length > 0) {
                var panelResult = $("#CitasResult");
                var containerResult = $("#CitasResult").find(".panel-body .tablebody");
                $("#CitasResult").find(".spinnerBuscando").addClass('hide');
                // Asynchronously our Huecos Template's content.
                $.get('CitaRow.html', function (template) {
                    // Use that stringified template with $.tmpl() and
                    //  inject the rendered result into the body.
                    containerResult.html('');
                    $.tmpl(template, huecosEncontrados).appendTo(containerResult);
                    if (bPuedeBorrar) {
                        $(".tablebody").find(".hide").removeClass("hide");
                    }
                    else {

                        var centros = $("td[data-centro]");
                        var centro = $(centros)[0].data('centro');
                        $("#" + centro.toUpperCase()).removeClass('hide');
                    }

                });

            } else {
                panelResult = $("#CitasResult").addClass('panel-danger');
                containerResult = $("#CitasResult").find(".panel-body .table").html("No se han encontrado citas.");
                panelResult.find(".spinnerBuscando").addClass('hide');
            }
        },
        error: function myfunction() {

            //var containerResult = $("#" + centroActual + "Result").addClass('panel-danger');
        }
    });
}

function huecoReservadoMismaHora(fecha, hora) {
    var result = false;
    for (var i = 0; i < huecosReservados.length; i++) {
        if (huecosReservados[i].fechaHueco === fecha) {
            if (huecosReservados[i].horaHueco === hora) {
                result = true;
            }
        }
    }
    return result;
}

function AgregarHuecoArray() {
    var hueco = {};
    hueco.idLista = $("tr td.success").data("lista");
    hueco.idHueco = $("tr td.success").data("id");
    hueco.fechaHueco = $("tr td.success").data("fecha");
    hueco.horaHueco = $("tr td.success").data("hora");
    hueco.aparatoDesc = $("#ddlAparato").find(':selected').text();
    hueco.aparato = $("tr td.success").data("aparatooid");
    hueco.tipoExploracion = $("#ddlAparato").find(':selected').data("oidaparato");
    hueco.filExploracion = $("tr.success").data("filexploracion");
    hueco.Centro = $("tr td.success").data("centro");

    hueco.owner = $('#GrupoDeAparato').find(':selected').data("oid");
    huecosReservados.push(hueco);
    //$("#ReservarCita").attr('data-content', '');
    //$("#ReservarCita").attr('data-content', drawTableCarrito(JSON.parse(sessionStorage["huecosReserva"]))).data('bs.popover').setContent();
}

function QuitarHuecoArray(idHueco) {
    var arrayTemporal = new Array();
    for (var i = 0; i < huecosReservados.length - 1; i++) {
        if (!(huecosReservados[i].idHueco === idHueco)) {
            arrayTemporal.push(huecosReservados[i]);
        }
    }


    return arrayTemporal;
}

function MostrarTextoPreparacion(texto) {

    $("#textPreparatorio").html(texto);
    $("#textoInfo").removeClass('hide');


}


// Acepta NIEs (Extranjeros con X, Y o Z al principio)
function NifValido(dni) {
    var numero, letra2, letra;
    var expresion_regular_dni = /^[XYZ]?\d{5,8}[A-Z]$/;

    dni = dni.toUpperCase();

    if (expresion_regular_dni.test(dni) === true) {
        numero = dni.substr(0, dni.length - 1);
        numero = numero.replace('X', 0);
        numero = numero.replace('Y', 1);
        numero = numero.replace('Z', 2);
        letra2 = dni.substr(dni.length - 1, 1);
        numero = numero % 23;
        letra = 'TRWAGMYFPDXBNJZSQVHLCKET';
        letra = letra.substring(numero, numero + 1);
        if (letra !== letra2) {
            alert('Dni erroneo, la letra del NIF no se corresponde');
            return false;
        } else {
            //alert('Dni correcto');
            return true;
        }
    } else {
        //alert('Dni erroneo, formato no válido');
        return false;
    }
}


function getParameterByName(name) {
    var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
    return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
}

function rellenarMutuas() {
    //PRIMERO RELLENAMOS LAS EXPLORACIONES FILTRADAS POR GRUPO
    var sel = $('#MutuaList');
    $('#MutuaList').empty();
    $.ajax({
        url: endPoint + '/API/Mutuas',
        type: 'GET',
        data: { oidAparato: getParameterByName("APARATO") },
        dataType: 'json',
        async: false,
        success: function (data) {
            var markup = '';
            for (var y = 0; y < data.length; y++) {
                markup += '<option value="' + data[y].OID + '">' + data[y].NOMBRE + '</option>';
            }
            sel.html(markup).show();
        },
        error: function (xhr, status, error) {
            console.error("Error en la solicitud AJAX:", status, error);
        }
    });
}


function rellenarMutuasHome() {
    //PRIMERO RELLENAMOS LAS EXPLORACIONES FILTRADAS POR GRUPO
    var sel = $('#MutuaList');
    $('#MutuaList').empty();
    if (sessionStorage.CentroExternoNombre === "ISD - ISDIN") {

        var markup = '';

        markup += '<option value="3820177">CIGNA</option>';

        sel.html(markup).show();
    } else {
      
        $.ajax({
            url: endPoint + '/API/Mutuas',
            type: 'GET',
            data: { oidAparato: $('#ddlAparato').find(':selected').data('oidaparato') },
            dataType: 'json',
            async: false,
            success: function (data) {
                var markup = '';
                for (var y = 0; y < data.length; y++) {
                    if (data[y].mutuaOnlineDisponible === true) {                      
                      
                            markup += '<option value="' + data[y].OID + '">' + data[y].NOMBRE + '</option>';

                        
                    } else {
                      
                            markup += '<option class="text-danger" value="' + data[y].OID + '" title="Esta mutua no cubre las pruebas seleccionadas." disabled><span >' + data[y].NOMBRE + '</span></option>';

                       
                    }
                }
                sel.html(markup).show();
                sel.val("").change();
            },
            error: function (xhr, status, error) {
                console.error("Error en la solicitud AJAX:", status, error);
            }
        });
    }

}

function rellenarExploraciones(data) {
    //PRIMERO RELLENAMOS LAS EXPLORACIONES FILTRADAS POR GRUPO
    var sel = $('#ddlAparato');
    $('#ddlAparato').empty();
    $(".table").empty();
    var markup = '';
    for (var y = 0; y < data.length; y++) {
        markup += '<option data-Texto="' + data[y].TEXTO + '" data-Especial="' + data[y].SMS + '" data-oidAparato="' + data[y].OID + '" value="' + data[y].FIL + '">' + data[y].TEXTO_INTERNET + '</option>';
    }
    sel.html(markup).show();
    var grupoSeleccionado = $('#GrupoDeAparato');

    $.ajax({
        url: endPoint + '/API/COCentros',
        type: 'GET',
        async: false,
        data: { grupo: $("#GrupoDeAparato").val() },
        dataType: 'json',
        success: function (data) {

            $("a[href^='#tab-']").closest('li').addClass("disabled");

            if (sessionStorage.CentroExternoNombre === "ISD - ISDIN") {
                $("a[href^='#tab-']").closest('li').addClass("disabled").addClass('hide');
                $("a[href=#tab-Tibidabo]").closest('li').removeClass("disabled").removeClass("hide");
                if ($("#GrupoDeAparato").val() === "ECO") {
                    $('#ddlAparato').empty();
                     markup = '';
                    markup = "<option data-texto='No precisa preparación.Traer pruebas anteriores' data-especial='' data-oidaparato='149' value='MAA'>ECOGRAFIA DE MAMA</option>";
                    sel.html(markup).show();
                } else {
                    $('#ddlAparato').empty();
                     markup = '';
                    markup = "<option data-texto='No precisa preparación.Traer pruebas anteriores' data-especial='' data-oidaparato='447' value='MAM   '>MAMOGRAFIA</option>";
                    sel.html(markup).show();
                }
            }
            else {
                for (var y = 0; y < data.length; y++) {

                    switch (data[y].NOMBRE) {

                        case "TIBIDABO":
                            $("a[href=#tab-Tibidabo]").closest('li').removeClass("disabled");
                            break;
                        case "BALMES":
                            $("a[href=#tab-Balmes]").closest('li').removeClass("disabled");
                            break;
                        case "CUGAT":
                            $("a[href=#tab-CanMora]").closest('li').removeClass("disabled");
                            break;
                        case "MERIDIANA":
                            $("a[href=#tab-Meridiana]").closest('li').removeClass("disabled");
                            break;
                        case "HOSPITALET":
                            $("a[href=#tab-Hospitalet]").closest('li').removeClass("disabled");
                            break;
                        default:

                    }
                    $("a[href^='#tab-" + data[y].NOMBRE + "]").closest('li').removeClass("disabled");
                    $("#" + data[y].NOMBRE).removeClass("disabled");
                }
            }


            MostrarTextoPreparacion($('#ddlAparato option').first().data('texto'));

        },
        error: function (xhr, status, error) {
            console.error("Error en la solicitud AJAX:", status, error);
        }
    });
}

function rellenarHuecos() {
    var CentroExterno = sessionStorage.CentroExternoNombre;
    var fecha = $("#fechaInicial").data('date');
    var hora = $("#hora").val();
    var grupoAparato = $('#GrupoDeAparato').val();

    var nombreMutua = $("#MutuaList").find(':selected').text();

    //Regla 1 si escogen SANITAS NO MOSTRAMOS TIBIDABO
    //AUNQUE ME HAN PASADO ESTA REGLA Y TIBI NO ESTÁ ACTIVADO
    if (nombreMutua.toString().toUpperCase() === "SANITAS") {
        $("#TIBIDABO").addClass("disabled");
    }
    //SI LA MUTUA ES AXA Y EL GRUPO DE APARATOS ES MAM O RM NO MOSTRAMOS RESULTADOS DE CUGAT
    if (nombreMutua.toString().indexOf("AXA") >= 0 && (grupoAparato === "RM " || grupoAparato == "MAM")) {
        //$("#CUGAT").addClass("disabled");
        $("#MERIDIANA").addClass("disabled");
        $("#TIBIDABO").addClass("disabled");
        $("#BALMES").addClass("disabled");
    }

    //SI LA MUTUA ES ASSISTENCIA O CIGNA NO SALE MERIDIANA
    if ((nombreMutua.toString().indexOf("ASSISTENCIA SANITARIA") >= 0 || nombreMutua.toString().indexOf("CIGNA") >= 0)) {
        $("#MERIDIANA").addClass("disabled");
    }


    if (nombreMutua.toString().indexOf("ASSISTENCIA SANITARIA") >= 0 && grupoAparato === "DEN") {
        $("#CUGAT").addClass("disabled");
        $("#MERIDIANA").addClass("disabled");
        $("#TIBIDABO").addClass("disabled");
        $("#BALMES").addClass("disabled");

    }
    var centros = new Array();
    centros = $('#Centros button');

    $("#ReservarCita").addClass('disabled');
    $('#Resultados').removeClass('hide');
    $('.spinnerBuscando').removeClass('hide');
    $('#Resultados').find('.table').empty();
    var Aparato = $('#ddlAparato').val();
    $(".table").empty();
    var error = false;
    for (var i = 0; i < centros.length; i++) {

        if (!error) {

            $("#" + centros[i].id + "Result").hide();
            if (!$(centros[i]).hasClass('disabled')) {
                $("#" + centros[i].id + "Result").show();
                var esPrivado = $("#PRIMUT").find(':selected').val() === "PRI";
                var oidMutua = $("#MutuaList").find(':selected').val();
                if (esPrivado) {
                    oidMutua = 3820080;
                }
                //si responden pacientes que estan operados de columna no se muestra Balmes
                //if (!(excepcionBalmesOperados && centros[i].id === 'BALMES')) {
                $.ajax({
                    url: endPoint + '/API/Citaonline',
                    type: 'GET',                    
                    data: {
                        origenPeticion: sessionStorage.CentroExternoNombre,
                        fecha: fecha + ' ' + hora,
                        Centro: centros[i].id,
                        codigoGrupo: $('#GrupoDeAparato').val(),
                        codigoActo: Aparato,
                        idEspecialidad: $('#GrupoDeAparato').find(':selected').data('esp'),
                        claustrofobia: $("#chkClaustrofobia").is(":checked"),
                        centroExternoOid: sessionStorage.CentroExterno,
                        COLOPERADA: excepcionBalmesOperados,
                        oidMutua: oidMutua
                    },
                    dataType: 'json',
                    success: function (huecosEncontrados) {
                        if (huecosEncontrados[0].IDHUECO === "-999") {
                            alert("Servidor en tareas de manteniminto. Prueba  en 5 minutos");
                            $('#Resultados').addClass('hide');
                            error = true;
                            return false;
                        }

                        if (huecosEncontrados.length > 1) {
                            var panelResult = $("#" + huecosEncontrados[0].CENTRO + "Result");
                            var containerResult = $("#" + huecosEncontrados[0].CENTRO + "Result").find(".panel-body .table");
                            $("#" + huecosEncontrados[0].CENTRO + "Result").find(".spinnerBuscando").addClass('hide');
                            // Asynchronously our Huecos Template's content.

                            var tableResult = "";
                            var currentFecha = "";
                            var j = 0;
                            for (var i = 0; i < huecosEncontrados.length - 1; i++) {
                                if (!huecoReservadoMismaHora(huecosEncontrados[i].FECHA, huecosEncontrados[i].HORA)) {
                                    if (j % 12 === 0 && j > 0) {
                                        tableResult = tableResult + "</tr>";
                                    }
                                    if (currentFecha !== huecosEncontrados[i].FECHA) {
                                        currentFecha = huecosEncontrados[i].FECHA;
                                        j = 0;
                                        moment.locale('es-ES');
                                        var dt = moment(currentFecha, "DD-MM-YYYY");
                                        var dia = dt.format('dddd');
                                        tableResult = tableResult + "<tr class='warning'><td colspan='12'>" + dia.charAt(0).toUpperCase() + dia.slice(1) + ' ' + currentFecha + "</td></tr>";
                                    }
                                    if (j % 12 === 0) {
                                        tableResult = tableResult + "<tr>";
                                    }
                                    tableResult = tableResult + "<td  data-CENTRO='" + huecosEncontrados[i].CENTRO + "' data-id='" + huecosEncontrados[i].IDHUECO + "' data-fecha='" + huecosEncontrados[i].FECHA + "' data-lista='" + huecosEncontrados[i].IDLISTA + "' data-aparatoOID='" + huecosEncontrados[i].CODIGOAPARATO + "' data-hora='" + huecosEncontrados[i].HORA + "' data-filexploracion='" + huecosEncontrados[i].FIL_EXPLORACION + "' >" + "<div class='checkbox'><label><input type='checkbox' class=' huecoCheck'/>" + huecosEncontrados[i].HORA + "</label></div>" + "</td>";
                                    j = j + 1;
                                }


                            }
                            containerResult.html(tableResult);

                        } else {
                            //Ticket#2020022410000038 - Fix Tibidabo cita online "Buscando...".
                            if (huecosEncontrados[0].CENTRO === "TIBIDABO") {
                                huecosEncontrados[0].CENTRO = "CDPI";
                            }
                            panelResult = $("#" + huecosEncontrados[0].CENTRO + "Result").addClass('panel-danger');
                            // var containerResult = $("#" + huecosEncontrados[0].CENTRO + "Result").find(".panel-body .table").html("No se ha encontrado disponibilidad");
                            var textoVerano = "PARA PROGRAMAR PONGASE EN CONTACTO CON EL CENTRO";
                            var fechaBusqueda = huecosEncontrados[0].FECHA;
                            switch (huecosEncontrados[0].CENTRO) {
                                case "BALMES":
                                    if (moment(fechaBusqueda).isBetween('2017-08-07', '2017-08-31')) {
                                        textoVerano = textoVerano + " (93 445 64 50)";

                                    } else {
                                        textoVerano = "No se ha encontrado disponibilidad";
                                    }

                                    break;
                                case "CUGAT":
                                    if (moment(fechaBusqueda).isBetween('2017-08-17', '2017-09-05')) {
                                        textoVerano = textoVerano + " (93 681 16 01)";

                                    } else {
                                        textoVerano = "No se ha encontrado disponibilidad";
                                    }

                                    break;

                                case "MERIDIANA":

                                    if (moment(fechaBusqueda).isBetween('2017-08-31', '2017-09-01')) {
                                        textoVerano = textoVerano + " (93 547 58 68)";

                                    } else {
                                        textoVerano = "No se ha encontrado disponibilidad";
                                    }
                                    break;
                                default:
                                    textoVerano = "No se ha encontrado disponibilidad";
                                    break;
                            }
                            containerResult = $("#" + huecosEncontrados[0].CENTRO + "Result").find(".panel-body .table").html(textoVerano);
                            panelResult.find(".spinnerBuscando").addClass('hide');
                        }
                    },
                    error: function (xhr, status, error) {
                        console.error("Error en la solicitud AJAX:", status, error);
                    }
                });
                //}
                // else {
                //      $("#" + centros[i].id + "Result").hide();
                //    $("#" + centros[i].id + "Result").find(".spinnerBuscando").addClass('hide');
                //}


            }
            else {

                if (centros[i].id === "TIBIDABO") {
                    $("#CDPIResult").hide();
                    $("#CDPIResult").find(".spinnerBuscando").addClass('hide');
                }
                $("#" + centros[i].id + "Result").hide();
                $("#" + centros[i].id + "Result").find(".spinnerBuscando").addClass('hide');
            }
        }


    }

}

var gruposExploraciones = [
    { id: 'RX ', oid: '13', desc: 'RADIOGRAFIAS', esp: 'RX-RX' },
    { id: 'DEN', oid: '11', desc: 'DENSITOMETRIA', esp: 'RX-DEN' },
    { id: 'TAC', oid: '12', desc: 'TAC (SCANNER)', esp: 'RX-TAC' },
    { id: 'MAM', oid: '14', desc: 'MAMOGRAFIAS', esp: 'RX-MAM' },
    { id: 'ECO', oid: '15', desc: 'ECOGRAFIA', esp: 'RX-ECO' },
    { id: 'RM ', oid: '16', desc: 'RESONANCIA', esp: 'RX-RM' }
];


$(document).on("click", ".BorrarCitaCarrito", function () {
    var myCitaId = $(this).data('oid');
    huecosReservados = QuitarHuecoArray(myCitaId);
    $(this).parent().parent().remove();
    //$("#CarritoCitas").trigger('click'); 
    var cuenta = $("#CarritoCitas").find('.badge').html();
    cuenta = +cuenta - 1;
    $("#CarritoCitas").find('.badge').html(cuenta);

    // As pointed out in comments, 
    // it is superfluous to have to manually call the modal.
    // $('#addBookDialog').modal('show');
});


$(document).on('click', '#EnviarTelefonoPruebaEspecial', function myfunction() {

    var exploracion = {
        DIASEMANA: $("#ddlAparato").find(':selected').text(),
        IOR_TIPOEXPLORACION: $("#ddlAparato").find(':selected').data("oidaparato"),
        IOR_GRUPO: $('#GrupoDeAparato').find(':selected').data("oid")
    };

    var paciente =
    {
        Nombre: "PRUEBA ESPECIAL -" + $("#NombreEspecial").val(),
        Telefono: $("#TelefonoEspecial").val(),
        Origen: "ADPMCITAONLINE"


    };

    sessionStorage.clear();
    paciente.Exploracion = exploracion;
    $.ajax({
        url: endPoint + '/API/Service',
        type: "POST",
        data: JSON.stringify(paciente),
        contentType: 'application/json; charset=utf-8',

        success: function (data) {

            $('#okLlamadaEspecial').removeClass('hide');
            $('#koPruebaEspecial').addClass('hide');
            $('#LlamadaEspecialTelefono').addClass('hide');

            return false;
        },
        error: function (xhr, status, error) {
            console.error("Error en la solicitud AJAX:", status, error);
        }
    });



});

$(document).on('click', '#btnBuscarHora', function myfunction() {

    if ($("#chkMarcapasos").is(":checked") && $('#GrupoDeAparato').val() === "RM ") {
        $('#okLlamadaEspecial').addClass('hide');
        $('#koPruebaEspecial').removeClass('hide');
        $('#LlamadaEspecialTelefono').removeClass('hide');
        $(".jumbotron").removeClass("hide");
        $(".centrosLink").addClass("hide");
    }
    else {
        if ($("#ddlAparato").find(':selected').data("especial") === 1) {
            $('#okLlamadaEspecial').addClass('hide');
            $('#koPruebaEspecial').removeClass('hide');
            $('#LlamadaEspecialTelefono').removeClass('hide');
            $(".jumbotron").removeClass("hide");
            $(".centrosLink").addClass("hide");
        }
        else {
            $(".jumbotron").addClass("hide");
            $(".centrosLink").removeClass("hide");
            var TipoExploracion = $('#GrupoDeAparato').val().trim() + $('#ddlAparato').val().trim();
            if (TipoExploracion === "RMCC" || TipoExploracion === "RMCL" || TipoExploracion === "RMCD") {
                swal({
                    title: "¿Está usted operado de columna?",
                    type: "warning",
                    showCancelButton: true,
                    confirmButtonColor: "#DD6B55",
                    confirmButtonText: "Si, estoy operado",
                    cancelButtonText: "No, no estoy operado",
                    closeOnConfirm: true
                }, function (isConfirm) {
                    if (isConfirm) {
                        excepcionBalmesOperados = true;
                        rellenarHuecos();

                    } else {
                        excepcionBalmesOperados = false;
                        rellenarHuecos();
                    }
                });
            } else {
                excepcionBalmesOperados = false;
                rellenarHuecos();
            }

        }


    }

});

$(document).on('click', '.huecosResult tr td', function (e) {
    e.stopPropagation();
    var esClicSobreActiva = false;
    //Si en la busqueda actual ya hay un hueco reservado
    if ($('.table tr td.success').length === 0) {
        $(this).addClass('success');

        //En este caso hay que agregar un hueco al Array en memoria para luego reservar
        var cuenta = $("#CarritoCitas").find('.badge').html();
        cuenta = +cuenta + 1;
        $("#CarritoCitas").find('.badge').html(cuenta);

        $(this).find('.huecoCheck').attr('checked', true);

    }
    else {
        if ($(this).hasClass("success")) {
            //En este caso hay que quitar un hueco al Array en memoria para luego reservar
            var cuenta = $("#CarritoCitas").find('.badge').html();
            cuenta = +cuenta - 1;
            $("#CarritoCitas").find('.badge').html(cuenta);
            esClicSobreActiva = true;
            $(this).removeClass('success');
            $(this).find(".huecoCheck").prop("checked", false);

        }
        huecosReservados = QuitarHuecoArray($("tr td.success").data("id"));
    }
    if (!esClicSobreActiva) {
        //quitamos la clase success del resto de celdas
        $('.table tr td').removeClass('success');
        //añadimos la clase success a la actual
        $(this).addClass('success');
        $('.huecoCheck').attr('checked', false)
        $(this).find(".huecoCheck").prop("checked", true);

        //habilitamos el boton de reservar
        $("#ReservarCita").removeClass('disabled');
        AgregarHuecoArray();
    }



});

$(document).on('click', '#CrearCita', function () {

    if (!NifValido($("#dni").val())) {
        $("#dni").focus();
        return false
    }
    var form = $('#EnviarReservarForm');
    form.validate();
    if ($('#EnviarReservarForm').valid()) {
        //Si el codigo de validación es correcto
        var ior_mutua = sessionStorage.oidMutua;
        if (sessionStorage.codigoServer === $("#codigoRecibido").val() || sessionStorage.CentroExterno !== -1 || sessionStorage.dni.length > 0) {
            huecosReservados = JSON.parse(sessionStorage["huecosReserva"]);
            var exploracionesParaReserva = [];
            for (var i = 0; i < huecosReservados.length; i++) {
                var reserva = huecosReservados[i];


                var numero_Autorizacion = "";
                if (ior_mutua !== 3820080) {
                    numero_Autorizacion = $("#AUTORIZA").val();
                }

                var exploracion = {
                    ESTADODESCRIPCION: reserva.fechaHueco.replace('/', '-').replace('/', '-'),
                    HORA: reserva.horaHueco,
                    OWNER: 99,
                    IOR_APARATO: reserva.aparato,
                    IOR_TIPOEXPLORACION: reserva.tipoExploracion,
                    IOR_GRUPO: reserva.owner,
                    IOR_ENTIDADPAGADORA: ior_mutua,
                    NHCAP: numero_Autorizacion
                };
                if (sessionStorage.CentroExterno !== -1) {
                    exploracion.USERNAME = sessionStorage.CentroExternoCodigo;
                }
                exploracionesParaReserva.push(exploracion);
            }
            var SexoCheck = "H";
            if ($("#optionMujer").is(":checked")) {
                SexoCheck = "M";
            }


            var paciente =
            {
                Nombre: $("#NombrePaciente").val(),
                Apellidos: $("#ApellidosPaciente").val(),
                Dni: $("#dni").val(),
                Sexo: SexoCheck,
                FechaNacimiento: $("#fechaNacimiento").val(),
                Telefono: $("#Number1").val(),
                Email: $("#email").val(),
                Origen: "ADPMCITAONLINE",
                IOR_MUTUA: ior_mutua,
                Exploraciones: exploracionesParaReserva,
                IOR_CENTROEXTERNO: sessionStorage.CentroExterno
            };

            if (paciente.IOR_CENTROEXTERNO === -1) {
                paciente.IOR_CENTROEXTERNO = 1;
            }

            paciente.IdEnvio = sessionStorage.idEnvio;
            var AparatoDesc = huecosReservados[0].aparatoDesc;
            sessionStorage.clear();

            $.ajax({
                url: endPoint + '/API/Service',
                type: "POST",
                async: false,
                data: JSON.stringify(paciente),
                contentType: 'application/json; charset=utf-8',

                success: function (data) {


                    document.location.href = '/ResumenCita.html?oid=' + data.Exploraciones[0].IOR_PACIENTE;

                    return false;
                },
                error: function (x, y, z) {
                    console.log("Is the status " + x.status + " equals to this:" + 409 + " = " + x.status === 409);
                    if (x.status === 409) {
                        //TODO Mostrar el view
                        $("#errorCreaCita").removeClass("hide");
                    } else {
                        $("#errorCreaCita").addClass("hide");
                    }
                    alert(x + '\n' + y + '\n' + z);
                }
            });


        }
    }

    return false;
});


$(document).on('click', '#CarritoCitas', function () {
    $('.popover').addClass('hide');
    $("#CarritoCitas").attr('data-content', '');
    $("#CarritoCitas").attr('data-content', drawTableCarrito(huecosReservados)).data('bs.popover').setContent();
    $('.popover').removeClass('hide');
    return false;
    //window.location = '/ReservarCita.html?APARATO=' + huecosReservados[0].aparato;
});

$(document).on('click', '#ReservarCita', function () {
    $('.popover').addClass('hide');
    sessionStorage.huecosReserva = JSON.stringify(huecosReservados);
    sessionStorage.idLista = $("tr td.success").data("lista");
    sessionStorage.idHueco = $("tr td.success").data("id");
    sessionStorage.fechaHueco = $("tr td.success").data("fecha");
    sessionStorage.horaHueco = $("tr td.success").data("hora");
    sessionStorage.aparatoDesc = $("#ddlAparato").find(':selected').text();
    //Si el paciente es de mutua (desplegable mutua o privado)
    if ($("#PRIMUT").val() === "MUT") {
        sessionStorage.oidMutua = $("#MutuaList").val();
    } else {

        sessionStorage.oidMutua = 3820080;
    }

    window.location = '/ReservarCita.html?APARATO=' + huecosReservados[0].aparato;
});


$('html').on('mouseup', function (e) {
    if (!$(e.target).closest('.popover').length) {
        $('.popover').addClass('hide');

    }
});



//Al cambiar el grupo de exploraciones rellenamos el combo de las exploraciones de debajo
$(document).on('change', '#ddlAparato', function () {
    $('#Resultados').find('.table').empty();

    $(".table").empty();
    MostrarTextoPreparacion($("#ddlAparato").find(':selected').data("texto"));
    rellenarMutuasHome();
});

//Al cambiar el grupo de exploraciones rellenamos el combo de las exploraciones de debajo
$(document).on('change', '#GrupoDeAparato', function () {

    $.ajax({
        url: endPoint + '/API/COExploraciones',
        type: 'GET',
        data: {
            grupo: $("#GrupoDeAparato").val(),
            Claustro: $("#chkClaustrofobia").is(":checked")
        },
        dataType: 'json',
        success: function (data) {
            rellenarExploraciones(data);
            rellenarMutuasHome();
        },
        error: function (xhr, status, error) {
            console.error("Error en la solicitud AJAX:", status, error);
        }
    });
});


$(document).ready(function () {
    $('option').tooltip();

    if (getParameterByName("oid") !== null) {
        $("#cabecera").removeClass('hide');

    }

    if (sessionStorage.huecosReserva) {
        huecosReservados = JSON.parse(sessionStorage.huecosReserva);
        $("#CarritoCitas").find('.badge').html(huecosReservados.length);
        $("#ReservarCita").removeClass('disabled');

    }

});
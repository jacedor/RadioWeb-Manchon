//var endPoint = "http://localhost:49523";

var endPoint = "";
var huecosReservados = new Array();
var excepcionBalmesOperados = false;






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
        async:false,
        success: function (data) {
            var markup = '';
            for (var y = 0; y < data.length; y++) {
                markup += '<option value="' + data[y].OID + '">' + data[y].NOMBRE + '</option>';
            }
            sel.html(markup).show();
        },
        error: function (x, y, z) {
            alert(x + '\n' + y + '\n' + z);
        }
    });
}






$(document).on('click', '#btnBuscarHora', function myfunction() {

    if ($("#chkMarcapasos").is(":checked") && $('#GrupoDeAparato').val() == "RM ") {
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
    if ($('.table tr td.success').length == 0) {
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

        if (sessionStorage.codigoServer === $("#codigoRecibido").val() || sessionStorage.CentroExterno != -1 || sessionStorage.dni.length > 0) {
            huecosReservados = JSON.parse(sessionStorage["huecosReserva"]);
            var exploracionesParaReserva = [];
            for (var i = 0; i < huecosReservados.length; i++) {
                var reserva = huecosReservados[i];
                var ior_mutua = "";
                var numero_Autorizacion = "";
                if ($("#chkPrivado").is(':checked')) {
                    ior_mutua = 3820080;
                }
                else {
                    ior_mutua = $("#MutuaList").val();
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
                if (sessionStorage.CentroExterno != -1) {
                    exploracion.USERNAME = sessionStorage.CentroExternoCodigo;
                }
                exploracionesParaReserva.push(exploracion);
            }
            var SexoCheck = "H";
            if ($("#optionMujer").is(":checked")) {
                SexoCheck = "M";
            }

            var iorMutua = "3820080";
            if ($("#chkMutua").is(":checked")) {
                iorMutua = $("#MutuaList").val();
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
              IOR_MUTUA: iorMutua,
              Exploraciones: exploracionesParaReserva,
              IOR_CENTROEXTERNO: sessionStorage.CentroExterno
          };

            if (paciente.IOR_CENTROEXTERNO==-1) {
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
                    alert(x + '\n' + y + '\n' + z);
                }
            });


        }
    }

    return false;
});





$(document).ready(function () {

    rellenarMutuas();

  

});
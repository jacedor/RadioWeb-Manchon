var endPoint = "";
//var endPoint = "https://radioibweb-es.affidea.com";

function getParameterByName(name) {
    var match = RegExp('[?&]' + name + '=([^&]*)').exec(window.location.search);
    return match && decodeURIComponent(match[1].replace(/\+/g, ' '));
}

$(document).on('change', '#PRIMUT', function () {
    if ($("#PRIMUT").val() === "PRI") {
        $('#MutuaList').prop('disabled', true);
        $('#MutuaList').parent().addClass("hide");
        //Habilitamos el boton de Buscar Hora, ya que puede que este bloqueado por otro lado.
        $("#btnBuscarHora").removeClass("disabled");
    } else {
        $('#MutuaList').prop('disabled', false);
        $('#MutuaList').parent().removeClass("hide");
        if ($("#MutuaList option:selected").text() == "") {
            $("#btnBuscarHora").addClass("disabled");
            $("#ReservarCita").addClass("disabled");
        } else {
            $("#btnBuscarHora").removeClass("disabled");
        }
    }

});

$(document).on('change', '#chkClaustrofobia', function () {
  
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
        },
        error: function (xhr, status, error) {
            console.error("Error en la solicitud AJAX:", status, error);
        }
    });

});


$("#MutuaList").change(() => {
    //Comprobamos si vienen de ADESLAS
    if ($("#MutuaList option:selected").text() === "ADESLAS") {
        $("#adesModal").modal();
    }

    if ($("#MutuaList option:selected").text() == "" && $("#PRIMUT").val() == "MUT") {
        $("#btnBuscarHora").addClass("disabled");
        $("#ReservarCita").addClass("disabled");
    } else {
        $("#btnBuscarHora").removeClass("disabled");
    }

})


$(document).on('change', '#ddlAparato', function () {
    MostrarTextoPreparacion();
    
});

function inicializarParametros() {
    sessionStorage.dni = (getParameterByName("dni") || '');
    sessionStorage.nombre = (getParameterByName("nombre") || '');
    sessionStorage.apellidos = (getParameterByName("apellidos") || '');
    sessionStorage.fechaNacimiento = (getParameterByName("fechaNacimiento") || '');
    sessionStorage.sexo = (getParameterByName("sexo") || '');
    sessionStorage.email = (getParameterByName("email") || '');
    sessionStorage.telefono = (getParameterByName("telefono") || '');
    sessionStorage.autorizacion = (getParameterByName("autorizacion") || '');
    sessionStorage.idEnvio = (getParameterByName("id_Envio") || '');

}

$('#modalLoginIsdin').on('hide.bs.modal', function (e) {
    if ($('#claveIsDIN').val() !== "manchonmama") {
        e.preventDefault();
        e.stopImmediatePropagation();
        return false;
    } else {
        //entramos en la aplicación mirando si tenemos o no más de 40 años
        if ($("#MasDe40").is(':checked')) {
            $("#GrupoDeAparato").find('[value="ECO"]').remove();
            $('#GrupoDeAparato option[value=MAM]').prop('selected', 'selected').change();
        }
        else {
            $('#GrupoDeAparato option[value=ECO]').prop('selected', 'selected').change();
            $("#GrupoDeAparato").find('[value="MAM"]').remove();

        }
           
    }
});

$(document).on('click', '#validarClave', function () {
    if ($('#claveIsDIN').val() !== "manchonmama") {
        $('#claveIncorrecta').removeClass('hide');
    } else {
        $("#modalLoginIsdin").modal('hide');
    }

});

$(document).ready(function () {
    inicializarParametros();
    sessionStorage.CentroExterno = (getParameterByName("oid") || -1);
    if ($("#PRIMUT").val() === "PRI") {
        $('#MutuaList').prop('disabled', true);
        $('#MutuaList').parent().addClass("hide");
    } else {
        $('#MutuaList').prop('disabled', false);
        $('#MutuaList').parent().removeClass("hide");
    }

    if (sessionStorage.CentroExterno !== "-1") {
        sessionStorage.CentroExternoNombreOid = getParameterByName("oid");
        //Rellenamos los Aparatos
        $.ajax({
            url: '/API/CentroExterno',
            type: 'GET',
            data: { oid: sessionStorage.CentroExterno },
            success: function (data) {
                sessionStorage.CentroExternoNombre = data;
                sessionStorage.CentroExternoCodigo = data.split(' - ')[0];
                
              
                if (sessionStorage.CentroExternoNombreOid === "12810150") {
                    $('[href=#tab-CanMora]').tab('show');
                }
                else if (sessionStorage.CentroExternoNombre === "ISD - ISDIN") {
                    //Bloque else if para gestionar la campaña isdin DE julio de 2018
                    $("#PRIMUT").val("MUT");
                    $('#MutuaList').prop('disabled', false);
                    $('#MutuaList').parent().removeClass("hide");
                    $("#modalLoginIsdin").modal('show');
                } else {
                    alert("Bienvenido " + sessionStorage.CentroExternoNombre);
                }
               
            },
            error: function (xhr, status, error) {
                console.error("Error en la solicitud AJAX:", status, error);
            }
        });
        
    } else {

        sessionStorage.CentroExternoNombre = "-1";
    }

    

    $('.ui-popover').popover();
    //Rellenamos los tipos de exploraciones
    var sel = $('#GrupoDeAparato');
    $('#GrupoDeAparato').empty();
    var markup = '';
    if (sessionStorage.CentroExternoNombre === "ISD - ISDIN") {
        
        markup += '<option data-oid="14" data-esp="RX-MAM" value="MAM">MAMOGRAFIA</option>';
        markup += '<option data-oid="15" data-esp="RX-ECO" value="ECO">ECOGRAFIA</option>';

    } else {
        for (index = 0; index < gruposExploraciones.length; index++) {
            markup += '<option data-oid="' + gruposExploraciones[index].oid + '" data-esp="' + gruposExploraciones[index].esp + '" value="' + gruposExploraciones[index].id + '">' + gruposExploraciones[index].desc + '</option>';
        }
    }
    
    sel.html(markup).show();

    //Rellenamos los Aparatos (Tipo de prueba)
    
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
    var dateNow = new Date();
    $('#fechaInicial').datetimepicker({
        //inline: true,
        defaultDate: dateNow,
        format: 'DD-MM-YYYY',
        locale: 'es'
    });
    
    
    var d = new moment();
    d.add("days", 0);
    

    $('#fechaInicial').data("DateTimePicker").minDate(d.format('DD-MM-YYYY'));
    $('#fechaInicial').data("DateTimePicker").maxDate(moment().days(90));
    

});
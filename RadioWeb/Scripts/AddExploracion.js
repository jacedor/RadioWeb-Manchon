/// <reference path="_references.js" />
/// <reference path="jquery-2.0.3.min.js" />
/// <reference path="jquery-ui-1.10.3.min.js" /


var currentStep = 1;

var oExploracion = {};
var Paciente = {};
var esAlta = false;


var submitAutocompleteForm = function (event, ui) {
    $("#CP").val(ui.item.desc);
    $("#Provincia").val(ui.item.icon);
    $("#Pais").focus();
    return false;
};



$(document).on('click', '#accionesPaciente  a', function () {

    var iconoClicado = $(this).find('i');

    switch ($(this).attr('id')) {

        case "LOPD":
            if (iconoClicado.hasClass('fa-pencil-square-o')) {
                iconoClicado.removeClass('fa-pencil-square-o');
                iconoClicado.addClass('fa-pencil-square');
            }
            else {
                iconoClicado.removeClass('fa-pencil-square');
                iconoClicado.addClass('fa-pencil-square-o');
            }

            break;

        case "AVISO":
            if (iconoClicado.hasClass('fa-bell-o')) {
                iconoClicado.removeClass('fa-bell-o');
                iconoClicado.addClass('fa-bell');
            }
            else {
                iconoClicado.removeClass('fa-bell');
                iconoClicado.addClass('fa-bell-o');
            }

            break;
        case "VIP":
            if (iconoClicado.hasClass('fa-star-o')) {
                iconoClicado.removeClass('fa-star-o');
                iconoClicado.addClass('fa-star');
            }
            else {
                iconoClicado.removeClass('fa-star');
                iconoClicado.addClass('fa-star-o');
            }

            break;
        case "RIP":
            if (iconoClicado.hasClass('fa-plus-square-o')) {
                iconoClicado.removeClass('fa-plus-square-o');
                iconoClicado.addClass('fa-plus-square');
            }
            else {
                iconoClicado.removeClass('fa-plus-square');
                iconoClicado.addClass('fa-plus-square-o');
            }

            break;
        default:

    }


});



$('#ModalAddExploracion').on('show.bs.modal', function (e) {
    $("#FindPaciente").focus();
    $('#ExploracionResumen').hide();
    if (currentStep === 1) {
        $('#ExploracionPaso3').hide();
        $('#BusquedaPaciente').show();
        $('#actionsPacienteList').show();
    }
})
$(document).on('click', '#AnteriorPasoExploracion', function () {

    switch (currentStep) {

        //sI EL CURRENT STEP ES EL DOS QUIERE DECIR QUE ESTAMOS INTENTANDO PASAR AL 1
        case 2:
            $('#actionsPacienteList').show();
            $('#ExploracionResumen').hide();
            $('#AddOrEdit').fadeOut(function () {
                $('#BusquedaPaciente').fadeIn();
            });
            $('#myModalLabel').html('Paso 1 - Búsqueda de paciente');
            currentStep = 1
            $('#footerAddExploracion').hide();
            $('#FindPaciente').empty().focus();
            break;

        case 3:
            $('#ExploracionResumen').hide();
            $('#ExploracionPaso3').fadeOut(function () {
                $('#AddOrEdit').fadeIn();
                $('#footerAddExploracion').hide();
                $('#myModalLabel').html('Paso 2 - Paciente Existente');
                $('input[type="datetime"]').mask("00/00/0000", { placeholder: "__/__/____" });

            });
            currentStep = 2;
            break;
        case "4":


            break;

        default:

    }


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
            CrearOEditarPaciente();

            if (sessionStorage.OidUltimoPaciente>0) {
                oExploracion.IOR_PACIENTE =sessionStorage.OidUltimoPaciente;
                
                $('#AddOrEdit').data('oid', sessionStorage.OidUltimoPaciente);
                Paciente.OID = sessionStorage.OidUltimoPaciente;
                //En este punto nos guardamos en una variable de session del Navegador con el paciente seleccionado para asignar a la exploración                
                $("#ExploracionPaso3").empty();
                oExploracion.HORA = $('#ExploracionesTable tbody tr.ACTIVA').data('hhora');
                oExploracion.FECHA = sessionStorage.fechaActual;
                oExploracion.IOR_APARATO = sessionStorage.valAparato;
            }
            else{
            
                $.growl.error({title:"Error al crear el paciente",message:""});
            }

            var optionsGetExploracion = null;
            if ($('#ExploracionesTable tbody tr.ACTIVA').length > 1) {
                var filasActivas = new Array();
                $(".ACTIVA").each(function () {
                    var fila = {};
                    if ($(this).hasClass("huecoLibre")) {
                        fila.ESHUECO = true;
                        fila.HHORA = $(this).data('hhora');
                        fila.FECHA =sessionStorage.fechaActual;
                        fila.IOR_PACIENTE = sessionStorage.OidUltimoPaciente;
                        fila.IOR_APARATO = sessionStorage.valAparato;
                        fila.COD_FIL = sessionStorage.valAparato;
                        filasActivas.push(fila);
                    }
                   
                });
                var postData = JSON.stringify(filasActivas);
                optionsGetExploracion ={
                    url: '/Exploracion/AddMultiple',
                    data: postData,
                    contentType: 'application/json',
                    type: "POST",
                    dataType: "html",
                    async: false,
                    cache: false
                };

                

            } else {
                optionsGetExploracion = {
                    url: "/Exploracion/GetSpecificPanel",
                    data: JSON.stringify({ oExploracion: oExploracion }),
                    contentType: 'application/json',
                    dataType: 'html',
                    type: "POST",
                    async: false,
                    cache: false
                };
            }
           
            
                $.ajax(optionsGetExploracion).success(function (data) {
                    var $target = $("#ExploracionPaso3");
                    var $newHtml = $(data);


                    $('#AddOrEdit').fadeOut(function () {
                        if ($('#ExploracionesTable tbody tr.ACTIVA').length > 1) {
                            $target.html($(data).find(".modal-body"));
                        } else {
                            $target.html(data);
                            $('#Paciente').html(Paciente.PACIENTE1);
                            $('#Hora').html(oExploracion.HORA + '-' + sessionStorage.textAparato);
                            //if (oExploracion.
                            $('.time').mask('00:00');
                            $('#ExploracionResumen').show();
                            
                        }
                        
                        
                        $('#ExploracionPaso3').fadeIn();
                        $('#myModalLabel').html('Paso 3 - Asignando Exploración');
                        $('#footerAddExploracion').show();
                    });

                });
                currentStep = 3;
            
            

            break;


        default:

    }


});

$(document).on('click', '#GuardarExploracion', function () {
    if ($('#ExploracionesTable tbody tr.ACTIVA').length > 1) {
        GuardarMultiplesExploraciones();
        LoadListaDia(false);
    }

    else {
        $("#frmPaso3AddExploracion").validate({
            rules: {
                ddlExploracionExplo: {
                    required: true,
                    ddlNoAsignadoCheck: true
                },
                textoExploracion: {
                    required: true
                }
            },
            messages: {

                ddlExploracionExplo: {
                    ddlExploracionExplo: "Es obligatorio seleccionar un tipo de exploración."
                }
            }
        });

        if ($("#frmPaso3AddExploracion").valid()) {
            oExploracion.IOR_TIPOEXPLORACION = $("#ddlExploracionExplo").val();
            oExploracion.IOR_APARATO = $("#ddlAparatosExplo option[value=" + $("#ddlAparatosExplo").val() + "]").val();
            oExploracion.IOR_ENTIDADPAGADORA = $("#ddlMutuasExplo").val();
            oExploracion.CANTIDAD = $('#Cantidad').val();
            oExploracion.RECOGIDO = $('#chkRecogido').checked ? "T" : "F";
            oExploracion.HORA_IDEN = moment().format('HH:mm');
            oExploracion.HORAMOD = moment().format('HH:mm');
            oExploracion.HORA = $('#horaExploracion').val();
            oExploracion.FECHA_IDEN = moment().format('DD-MM-YYYY');
            oExploracion.TEXTO = $('#textoExploracion').val();
            oExploracion.IOR_TECNICO = "-1";
            oExploracion.IOR_CODIGORX = "-1";
            var options = {
                url: "/Exploracion/Add",
                data: JSON.stringify({ oExploracion: oExploracion }),
                contentType: 'application/json',
                dataType: 'html',
                type: "POST"
            };

            $.ajax(options).done(function (data) {
                $('#ExploracionResumen').hide();
                $('#AddOrEdit').fadeOut(function () {
                    $('#BusquedaPaciente').fadeIn();
                });
                $('#myModalLabel').html('Paso 1 - Búsqueda de paciente');
                $('#footerAddExploracion').hide();
                $('#ModalAddExploracion').modal('hide');
                currentStep = 1;
                oExploracion = null;

                $.growl.notice({ title: "Exploracion", message: "Exploracion añadida correctamente!" });
                $('#footerAddExploracion').hide();
                sessionStorage.ExploracionOidActual = data;
                LoadListaDia(false);

            });
        } else {

            $.growl.error({ title: "Revise todos los campos!", message: "" });
        }
    }
    //        
    
    
   

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

$(document).ready(function () {
    
    
    $("#FindPaciente").val().trim();
});
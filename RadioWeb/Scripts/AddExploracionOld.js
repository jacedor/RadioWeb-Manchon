/// <reference path="_references.js" />
/// <reference path="jquery-2.0.3.min.js" />
/// <reference path="jquery-ui-1.10.3.min.js" /
//09062014***********************************************************//
//*******************FUNCIONES ******************************//
//***********************************************************//
//***********************************************************//


function ValidarDatos() {

  
    jQuery('input[required]').each(function () {
        if ($(this).val().length == 0 || $(this).val().length == 1) {
            $(this).parent().addClass('has-error');

        }
        else {
            $(this).parent().removeClass('has-error');
        }
    });
  
}














//***********************************************************//
//*******************EVENTOS ******************************//
//***********************************************************//
//***********************************************************//
window.onbeforeunload = function () {
    alert('hola');
    return "You have not saved your document yet.  If you continue, your work will not be saved."
}




$(document).on('click', '#AgregarDireccion', function () {

    alert('hola');

});



$(document).on('keydown', 'input[required]', function () {

    if ($(this).val().length == 0 || $(this).val().length == 1) {
        $(this).parent().addClass('has-error');

    }
    else {
        $(this).parent().removeClass('has-error');
    }

});

//Al salir el foco de un campo required verificamos
$(document).on('focusout', 'input[required]', function () {

    if ($(this).val().length == 0)
    {
        if (!$(this).attr("type") === "datetime") {
            $(this).parent().removeClass('has-error');
        }
        
    }
    else {
        $(this).parent().removeClass('has-error');
    }

});


$(document).ready(function () {


    //$('.subnavbar-inner').hide();//escondemos el menu superior
    var oExploracion = {};
    var Paciente = {};

    $("#ExploWizard").steps({
        headerTag: "h3",
        bodyTag: "section",
        transitionEffect: "slideLeft",
        onStepChanging: function (event, currentIndex, newIndex) {

            switch (currentIndex) {
                //en la PRIMERA pestaña de selección del paciente
                case 0:
                    //si pasamos de la 0 a la 1
                    if (newIndex === 1) {
                        $("#ExploWizard-p-1").empty();//vaciamos la pestaña 1
                    }

                    //Si se ha seleccionado algun paciente
                    if ($('.PacienteItem.ACTIVA').data('oid') != undefined) {
                        //Serialiazomas el objeto paciente para cogerlo de la BBDD
                        Paciente.OID = $('.PacienteItem.ACTIVA').data('oid');
                        Paciente.PACIENTE1 = $('.PacienteItem.ACTIVA td:first b').text()
                        oExploracion.IOR_PACIENTE = Paciente.OID;
                        var options = {
                            url: "/Paciente/Detalle",
                            data: JSON.stringify({ oPaciente: Paciente, 'esAlta': 'false' }),//{ JSON.stringify('oPaciente':Paciente),'esAlta':'false'},
                            contentType: 'application/json',
                            dataType: 'html',
                            type: "POST"

                        };
                        //Hacemos una solicitud para obtener los detalles del paciente
                        $.ajax(options).done(function (data) {
                            var $target = $("#ExploWizard-p-1");
                            var $newHtml = $(data);
                            $target.html(data);

                        });
                        return true;
                    }
                        //Si se trata de una alta nueva
                    else {
                        Paciente.OID = 0;
                        Paciente.PACIENTE1 = $('#FindPaciente').val();
                        var options = {
                            url: "/Paciente/DetallesAlta",
                            data: JSON.stringify({ oPaciente: Paciente }),
                            dataType: 'html',
                            contentType: 'application/json',
                            type: "POST"

                        };
                        $.ajax(options).done(function (data) {
                            var $target = $("#ExploWizard-p-1");
                            var $newHtml = $(data);
                            $target.html(data);
                            $("input[type='datetime']").datepicker({
                                format: "dd/mm/yyyy",
                                weekStart: 1,
                                startView: 2,
                                language: "es",
                                autoclose: true,
                                todayHighlight: true

                            });
                            $("#POBLACION").autocomplete({
                                data: $(this).val,
                                source: '/Direccion/PueblosList',
                                focus: function (event, ui) {
                                    $(this).val(ui.item.label);
                                    return false;
                                },
                                select: submitAutocompleteForm

                            });
                            $("#POBLACION").autocomplete("option", "appendTo", "#PoblacionContainer");

                        });
                        return true;
                    }
                    break;
                case 1:
                    //si esta volviendo del segundo paso al primero
                    if (newIndex === 0) {
                        return true;
                    }

                    ValidarDatos();

                    //Alta  de la ficha de paciente
                    if ((Paciente.OID == 0) && ($('.has-error').length == 0)) {
                        //Serializamos el objeto paciente
                        Paciente.AVISO = $('#chkAviso').is(':checked');
                        Paciente.BORRADO = 'F';
                        Paciente.CID = $("#ddlMutuas option[value=" + $("#ddlMutuas").val() + "]").val();
                        Paciente.FECHAN = $('#fNacimiento').val();
                        Paciente.IOR_EMPRESA = 4;
                        Paciente.DNI = $('#dni').val();
                        Paciente.PACIENTE1 = $('#nombrePaciente').val();
                        Paciente.POLIZA = $('#Poliza').val();
                        Paciente.PROFESION = $('#Profesion').val();
                        Paciente.SEXO = $('#lblSexo').attr("data-sexo-val");
                        Paciente.TARJETA = $('#Tarjeta').val();
                        Paciente.TRAC = $("#Tratamiento option[value=" + $("#Tratamiento").val() + "]").val();
                        Paciente.EMAIL = $('#email').val();
                        Paciente.TEXTO = $('#TextoImprimible').val();
                        var DIRECCION = {};
                        DIRECCION.DIRECCION1 = $('#direccion').val();
                        DIRECCION.CP = $('#CP').val();
                        DIRECCION.POBLACION = $('#POBLACION').val();
                        DIRECCION.PROVINCIA = $('#PROVINCIA').val();
                        DIRECCION.PAIS = $('#PAIS').val();
                        Paciente.DIRECCIONES = [DIRECCION];

                        //Insertamos el Paciente en la Base de Datos
                        var optionsInsertar = {
                            url: "/Paciente/Insertar",
                            data: JSON.stringify({ oPaciente: Paciente }),
                            contentType: 'application/json',
                            type: "POST"

                        };

                        $.ajax(optionsInsertar).success(function (data) {
                            //Si la inserción ha ido bien
                            if (data.oid != -1) {
                                oExploracion.IOR_PACIENTE = data.oid;
                                $("#ExploWizard-p-2").empty();
                                oExploracion.HORA = sessionStorage.horaExploracion;
                                oExploracion.FECHA = sessionStorage.fechaActual;
                                oExploracion.IOR_APARATO = sessionStorage.valAparato;
                                var optionsGetExploracion = {
                                    url: "/Exploracion/GetDetailsToAdd",
                                    data: JSON.stringify({ oExploracion: oExploracion }),
                                    contentType: 'application/json',
                                    type: "POST"
                                };
                                $.ajax(optionsGetExploracion).success(function (data) {
                                    var $target = $("#ExploWizard-p-2");
                                    var $newHtml = $(data);
                                    $target.html(data);


                                });



                            }
                                //Si se produce un error al insertar en Paciente
                            else {
                                alert("Error al insertar el paciente");
                                return false;
                            }

                        });
                        return true;
                    }
                        //Modificación de un paciente
                    else {
                        //Si entramos por el Else y hay algun elemento con este atributo significa que es un alta nueva pero con errores hay errores en el formulario
                        if (($('.has-error').length > 0)) {
                            return false;
                        }
                            //Sino significa que es un alta de exploración de un paciente ya existente
                        else {
                            $("#ExploWizard-p-2").empty();
                            oExploracion.HORA = sessionStorage.horaExploracion;
                            oExploracion.FECHA = sessionStorage.fechaActual;
                            oExploracion.IOR_APARATO = sessionStorage.valAparato;
                            var options = {
                                url: "/Exploracion/GetDetailsToAdd",
                                data: JSON.stringify({ oExploracion: oExploracion }),
                                contentType: 'application/json',
                                dataType: 'html',
                                type: "POST"

                            };

                            $.ajax(options).done(function (data) {
                                var $target = $("#ExploWizard-p-2");
                                var $newHtml = $(data);
                                $target.html(data);


                            });
                            return true;

                        }

                    }

                    break;

                case 2:
                    if (newIndex === 0) {
                        return false;
                    }
                    if (newIndex === 1) {
                        return false;
                    }
                    return true;
                    break;


                default:

            }



        },
        onStepChanged: function (event, currentIndex, newIndex) {
            switch (currentIndex) {


                default:
                    break;

            }


            return true;

        },
        onFinishing: function (event, currentIndex) {
            oExploracion.IOR_TIPOEXPLORACION = $("#ddlExploracion option[value=" + $("#ddlExploracion").val() + "]").val();
            oExploracion.CANTIDAD = $('#Cantidad').html();
            var options = {
                url: "/Exploracion/Insertar",
                data: JSON.stringify({ oExploracion: oExploracion }),
                contentType: 'application/json',
                type: "POST"
            };

            $.ajax(options).done(function (data) {

                if (data.ok) {
                    sessionStorage.ExploracionOidActual = data.oid;
                    window.location = "../Calendario/Index#ListaDia";
                }
                else {

                    window.alert(data.message);
                }




            });

        },
        onFinished: function (event, currentIndex) {


            return true;
        }
    });

    //Al escribir sobre la caja de texto del modal popup de pacintes

    var timer;
    //Al escribir sobre la caja de texto del modal popup de pacintes
    $("#FindPaciente").keyup($.debounce(250, function () {
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

                },
            });
        }

    }));


    $("#FindPaciente").focus();
});



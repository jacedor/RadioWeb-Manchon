
var EstadosExploracion = {
    Pendiente: 0,
    Borrado: 1,
    Presencia: 2,
    Confirmado: 3,
    NoPresentado: 4,
    LlamaAnulando: 5
};

//mapeamos el evento click de redirección hacia la ficha del paciente para almacenar la URL Actual
$(document).on('click', '.linkExploracion', function () {
    saveURLRetorno(window.location.href);
});



$('#modalSMS').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget); // Button that triggered the modal
    var oidInforme = button.data('oid');// Extract info from data-* attributes
    $("#textoSMS").val($("#textoSMSPlantilla").val());
    var txtMensajeMovil = $("#textoSMS").val(function (i, v) {
        return v.replace("CDPI", oidInforme);
    }).val();
});

$(document).on("click", "#EnviarSMS", function () {
    //console.log("enviamos cosas.");
    $(this).parents("tr").siblings().removeClass('ACTIVA');
    $(this).parents("tr").addClass('ACTIVA');
    var filaActiva = $("#InformesList* .ACTIVA");
    var oidInforme = filaActiva.data('oid');

    var envio_SMS = $("#ENVIO_SMS").val();

    if (envio_SMS === "" || envio_SMS === "F") {
        var options = {
            url: "/SMS/Enviar",
            data: "phone=" + $("#movil").val() + "&texto=" + $("#textoSMS").val() + "&idMensaje=" + oidInforme,
            type: "GET"
        };

        $.ajax(options).complete(function (data) {
            toastr.success('Enviado correctamente', 'SMS', { timeOut: 5000 });
        });
    } else {
        //No ha dado su permiso para enviar SMS.
        toastr.error('Este usuario no permite notificaciones por SMS', 'DERECHO LOPD', { timeOut: 5000 });
    }
});




/**$(document).on("click", "#GuardaryVolver", function myfunction() {
    var $this = $(this);
    var id = $this.attr('id');
    //var formulario = $('form').serialize();
    //formulario = "MAILING3=VOLVER&" + formulario;

    $.ajax({
        type: 'PATCH',
        url: '/Paciente/Details',
        data: $('form').serialize(),       
       // dataType: 'json',
        success: function (xhr, status, errorThrown) {

            
            if (id === "GuardaryVolver") {
                loadURLRetorno();
            }
            else {
                toastr.success('Actualización!', 'Paciente modificado!', { timeOut: 5000 });
            }
            
        },
        error: function () {
            toastr.error('', 'Error al guardar!', { timeOut: 5000 });
        }

    });

    return false;

});**/

$(document).on("click", ".generarPDF", function () {

    var oidInforme = $(this).data('oid');
    
    var url = "/Informe/Imprimir?oid=" + oidInforme + "&password=" + oidInforme; //The Url to the Action  Method of the Controller
    window.open(url, 'popup', 'width=900,height=500');
    return false;


});

$(document).on("click", "#btnBorrar,#btnNoPresentado,#btnLlamaAnulando", function myfunction() {

    var trigger = $(this).attr("id");
    //hay que enseñar el pop up si es igual a 0 solamente
    var estadoExploracion = $(this).data('estado');
    var estadoNuevo = null;

    if (estadoExploracion === EstadosExploracion.Borrado || estadoExploracion === EstadosExploracion.NoPresentado || estadoExploracion === EstadosExploracion.LlamaAnulando) {
        estadoNuevo = EstadosExploracion.Pendiente;
    }

    if (estadoExploracion === EstadosExploracion.Pendiente) {
        switch (trigger) {
            case "btnBorrar":
                estadoNuevo = EstadosExploracion.Borrado;
                break;
            case "btnNoPresentado":
                estadoNuevo = EstadosExploracion.NoPresentado;
                break;
            case "btnLlamaAnulando":
                estadoNuevo = EstadosExploracion.LlamaAnulando;
                break;
            default:
                break;
        }

    }

    var hhora = $(this).data('hhora');

    var oidExploracionSeleccionada = $(this).data('oid');

    var request = $.ajax({
        url: "/Exploracion/CambiarEstado",
        data: "estadoActual=" + estadoExploracion + "&" + "estadoNuevo=" + estadoNuevo + "&" + "oidExploracion=" + oidExploracionSeleccionada + "&" + "hhora=" + hhora,
        type: "GET",
        dataType: "html",
        async: "false"
    });
    request.done(function (data) {
        location.reload();
    });

    toastr.success('Estado', 'Estado exploracion modificado', {
        timeOut: 3000,
        positionClass: 'toast-bottom-right'
    });
   
    return false;
});



$(document).ready(function () {
    $("li[data-view]").removeClass('active');
    $("[data-view=ViewFichaPaciente]").addClass("active");
    $("[data-view=ViewFichaPaciente]").parents("ul").removeClass("collapse"); 
    
    $('.FECHAN').editable({
        container: 'body'
      
    });

    $('#CP').editable({
        container: 'body',
        success: function (response, newValue) {
           
            if (newValue.length === 5) {
                var cp = newValue;
                $('#POBLACION"').val('');
                $.ajax({
                    url: "https://api.zippopotam.us/es/" + cp,
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
                            $('#POBLACION').val(cuidad[0].toUpperCase());
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
                            $('#PROVINCIA"]').val(provincia);
                        }

                    }
                });

            }



        }
    });
    makeBootstrapTable("ExploracionList");
  
});
    


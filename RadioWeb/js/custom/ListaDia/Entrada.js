var video;




function guardarPhoto() {
    var dataUrl = canvas.toDataURL("image/jpeg", 0.85);
    dataUrl = dataUrl.replace('data:image/jpeg;base64,', '');

    $("#uploading").show();
    var filaActiva = $("#ExploracionesTable* .ACTIVA");
    var oidExploracionSeleccionada = filaActiva.data('oid');
    $.ajax({
        type: 'POST',
        url: "/Imagenes/GuardarImagenWebCam",
        data: '{ "data_uri" : "' + dataUrl + '","oid" : "' + oidExploracionSeleccionada + '"  }',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (data) {
            toastr.success('Captura!', 'Imagen Asociada a la exploración!', { timeOut: 5000 });
        },
        error: function (data) {
            toastr.error('Captura!', 'Imagen NO ASOCIADA a la exploración!', { timeOut: 5000 });
        },
        complete: function () {
            var ContenedorModalImagenes = $('#ContenedorSliderImagenes');
            $.ajax({
                type: 'POST',
                url: '/Imagenes/ListaPartialImagenes',
                data: { oid: oidExploracionSeleccionada },
                beforeSend: function () {
                    ContenedorModalImagenes.html('');
                },
                success: function (data) {
                    ContenedorModalImagenes.html(data);
                }, complete: function () {
                    $("#uploading").hide();
                    $("#uploaded").show();
                    $('.slick_demo_3').slick({
                        infinite: true,
                        speed: 500,
                        fade: true,
                        cssEase: 'linear',
                        adaptiveHeight: true
                    });
                }
            });
        }
    });
}

function SubFormExploracion() {

    if ($("#form-Exploracion").valid()) {

        var filaActiva = $("#ExploracionesTable* .ACTIVA");

        if ($("#REGISTRE").val() === '' && $("#IOR_ENTIDADPAGADORA").val() === "20527263")
        {
            swal({
                title: "Esta exploracion requiere informar del NUMERO DE REGISTRO ¿Desea Continuar?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Si, continuar sin informar",
                cancelButtonText: "No, dejame corregirlo",
                closeOnConfirm: true
            }, function (isConfirm) {
                if (isConfirm) {
                    $.ajax({
                        url: '/Entrada/Exploracion',
                        type: 'post',
                        data: $('#form-Exploracion').serialize(),
                        success: function () {
                            toastr.success('Entrada!', 'Datos de la entrada modificados!', { timeOut: 5000 });
                        }
                    });

                }
            });
        } 

        //si es una linea master
        //ES UNA GC50 (OID 6)
        //O UNA GC06 (OID==43)
        if (($("#IOR_CARDIOLOGO").val() < 0 && filaActiva.data("ior_master") === -1) &&
            ($("#IOR_TIPOEXPLORACION").val() === "43" || $("#IOR_TIPOEXPLORACION").val() === "6")) {
            swal({
                title: "Esta exploracion requiere informar un cardiologo o cirujano ¿Desea Continuar?",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "Si, continuar sin informar",
                cancelButtonText: "No, dejame corregirlo",
                closeOnConfirm: true
            }, function (isConfirm) {
                if (isConfirm) {
                    $.ajax({
                        url: '/Entrada/Exploracion',
                        type: 'post',
                        data: $('#form-Exploracion').serialize(),
                        success: function () {
                            toastr.success('Entrada!', 'Datos de la entrada modificados!', { timeOut: 5000 });
                        }
                    });

                }
            });
        } else {
            $.ajax({
                url: '/Entrada/Exploracion',
                type: 'post',
                data: $('#form-Exploracion').serialize(),
                success: function () {
                    toastr.success('Entrada!', 'Datos de la entrada modificados!', { timeOut: 5000 });
                    $("#EnviarFiltros").trigger("click");
                }
            });
        }




    }
    return false;
}

function SubFormDatosContacto() {

    if ($("#form-Direccion").valid()) {
        $.ajax({
            url: '/Entrada/Contacto',
            type: 'post',
            data: $('#form-Direccion').serialize(),
            success: function () {
                toastr.success('Entrada!', 'Datos de contacto modificados!', { timeOut: 5000 });
            }, error: function () {
                toastr.error('Entrada!', 'Error al modificar los datos!', { timeOut: 5000 });
            }
        });
    }
    return false;
}

function SubFormPaciente() {

    if ($("#form-Paciente").valid()) {
        $.ajax({
            url: '/Entrada/Paciente',
            type: 'POST',
            data: $('#form-Paciente').serialize(),
            success: function () {
                toastr.success('Entrada!', 'Datos del paciente modificados!', { timeOut: 5000 });
            },
            error: function (data) {
                toastr.error('Entrada!', 'Datos del paciente erroneos!', { timeOut: 5000 });
            }
        });

    }

    return false;
}


function success_stream(stream) {
    //This is a callback. Please refer to javascripts callbacks for futher information
    console.log("Streaming successful");
    //    once we have the webcam stream, we shall display it in the
    //    html video element created
    video_frame.srcObject = stream;
    //video_frame.src = window.URL.createObjectURL(stream);
}

function error_stream(error) {
    console.log("error has occured" + error);
}

function capture() {
    /*
     When the button is called, this function is called.
     Once the button is clicked, the canvas will be updated with current frame
     */
    captureFlag = true;
    console.log("Button is clicked");
    imcanvas.drawImage(video_frame, 0, 0, canvas.width, canvas.height);
    // ipcanvas.getContext("2d").drawImage(video_frame, 0, 0, 640, 480);
}




//$(document).on("click", "#snap", function myfunction() {
//    var canvas = document.getElementById("canvas");
//    var context = canvas.getContext("2d");

//    context.drawImage(vid, 0, 0, 640, 480);
//    // the fade only works on firefox?
//    $("#video").fadeOut("slow");
//    $("#canvas").fadeIn("slow");
//    $("#snap").hide();
//    $("#reset").show();
//    $("#upload").show();
//});

$(document).on("click", "#btnEntrada", function myfunction() {

    var filaActivaEntrada = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActivaEntrada.data('oid');
    //var ContenedorModalImagenes = $('#contenedorModalImagenes');
    var ContenedorModaEntrada = $('#contenedorEntrada');
    $.ajax({
        type: 'GET',
        url: '/Entrada/Index/' + oidExploracion,
        beforeSend: function () {
            $(".spiner-cargando").removeClass('hide');
            ContenedorModaEntrada.html('');
            $('#modal-form-entrada').modal('show');
        },
        success: function (data) {
            $(".spiner-cargando").addClass('hide');
            ContenedorModaEntrada.html(data);
            $("#NombrePacienteEntrada").html($("#PACIENTE").val());
        },
        complete: function () {
            //Webcam.set({
            //    width: 500,
            //    height: 375,
            //    image_format: 'jpg',
            //    jpeg_quality: 90
            //});
            //Webcam.attach('#my_camera');
            
            $(".select2").select2({
                theme: "bootstrap"
            });

           
            $("#form-Paciente").validate({
                rules: {
                    EMAIL: {
                        email: true
                    },
                    PACIENTE: {
                        required: true,
                        contieneComa: true,
                    },
                    FECHANACIMIENTO: {
                        required: true,
                        validDate: true,
                    },
                    FECHAMAXENTREGA: {
                        required: true,
                        validDate: true,
                    },
                    TELEFONO: {
                        number: true
                    }
                },
                messages: {
                    required: "Campo obligatorio",
                    number: "Este campo es númerico",
                    EMAIL: "El campo email no es válido",
                    FECHANACIMIENTO: "El campo Fecha Nacimiento no es válido",
                    PACIENTE: "El nombre del paciente debe contener una coma"
                }
            });



            $("#form-Exploracion").validate({
                ignore: 'input[type=hidden]',
                rules: {
                    IOR_MEDICOINFORMANTE: {
                        required: true,
                        validMedicoInformante: true
                    },
                    IMPORTE: {
                        mynumber: true,
                        required: true
                    }
                },
                messages: {
                    required: "Campo obligatorio",
                    number: "Este campo es númerico",
                    IOR_MEDICOINFORMANTE: "Seleccione médico informante",
                    IMPORTE: "Importe obligatorio"
                }
            });

        }
    });




    return false;

});


$(document).on("shown.bs.tab", "a[data-toggle='tab']", function (e) {

    $(".select2").select2({
        theme: "bootstrap"
    }
    );
    //$($(this).attr('href')).find('button').trigger('click')
});
$(document).on("click", "#reset", function myfunction() {
    $("#video").fadeIn("slow");
    $("#canvas").fadeOut("slow");
    $("#snap").show();
    $("#reset").hide();
    $("#upload").hide();
});

$(document).on("click", "#upload", function myfunction() {
    guardarPhoto();

});
$(document).on('click', '#copiarColegiado', function () {

    var IOR_MEDICOREFERIDOR = $("#IOR_MEDICOREFERIDOR").val();
    var filaActiva = $("#ExploracionesTable* .ACTIVA");


    $.ajax({
        type: 'POST',
        url: "/Entrada/CopiarColegiado",
        data: "IOR_COLEGIADO=" + IOR_MEDICOREFERIDOR + "&" + "OIDEXPLORACION=" + filaActiva.data('oid'),
        complete: function () {
            toastr.success('Entrada!', 'Medico asignado a todas exploraciones mismo dia', { timeOut: 5000 });
        }

    });

});


// evento click del SELECT DEL APARATO QUE RELLENA LAS EXPLORACIONES
$(document).on('change keyup', '#IOR_APARATO, #IOR_ENTIDADPAGADORA', function () {

    var IOR_ENTIDADPAGADORA = $("#IOR_ENTIDADPAGADORA").val();

    var cantidad = $('#Cantidad').val('');
    var options = {
        url: "/Exploracion/GetTipoExploraciones",
        data: "IOR_APARATO=" + $("#IOR_APARATO").val() + "&" + "IOR_MUTUA=" + IOR_ENTIDADPAGADORA
    };

    $.ajax(options).success(function (data) {

        var sel = $('#IOR_TIPOEXPLORACION');
        sel.empty();
        var markup = '';
        for (var x = 0; x < data.length; x++) {
            if (!data[x].FIL.indexOf("OBSOLETO") !== -1) {
                markup += '<option value="' + data[x].OID + '" data-cod="' + data[x].FIL + '" data-text-value="' + data[x].DES_FIL + '">' + data[x].FIL + ' - ' + data[x].DES_FIL + '</option>';
            }
        }
        sel.html(markup).show();

        $("#IOR_TIPOEXPLORACION option:first").attr('selected', 'selected');
    });
});



// evento click del SELECT DE TIPO DE EXPLORACION QUE CAMBIA EL PRECIO
$(document).on('change keyup', '#IOR_TIPOEXPLORACION', function () {

    var IOR_TIPOEXPLORACION = $("#IOR_TIPOEXPLORACION").val();
    var IOR_ENTIDADPAGADORA = $("#IOR_ENTIDADPAGADORA").val();


    var options = {
        url: "/Exploracion/GetPrecioExploracion",
        data: ({ IOR_TIPOEXPLORACION: IOR_TIPOEXPLORACION, IOR_MUTUA: IOR_ENTIDADPAGADORA }),
        dataType: "html",
        type: "GET"
    };

    $.ajax(options).complete(function (data) {

        $("#IMPORTE").val(data.responseText);
    });
});


$(document).on("shown.bs.tab", "a[data-toggle='tab']", function (e) {

    var target = $(e.target).attr("href") // activated tab
    switch (target) {
        case "#tab-vidSigner":
            var filaActiva = $("#ExploracionesTable* .ACTIVA");
            var oidExploracion = filaActiva.data('oid');
            var ContenedorModalFirmar = $('#modalVidContentEntrada');
            $.ajax({
                type: 'POST',
                url: '/VidSigner/ListaPartial',
                data: {
                    oid: oidExploracion,
                    esEntrada: 'true'
                },
                beforeSend: function () {
                    $(".spiner-cargando").removeClass('hide');
                    ContenedorModalFirmar.html('');
                },
                success: function (data) {
                    $(".spiner-cargando").addClass('hide');

                    ContenedorModalFirmar.html(data);
                    if (localStorage.nombreTablet) {
                        $("#TABLETA_NAME").val(localStorage.nombreTablet);
                    }
                    $('.i-checks').iCheck({
                        checkboxClass: 'icheckbox_square-green',
                        radioClass: 'iradio_square-green',
                    });

                    $(".select2").select2({
                        theme: "bootstrap"
                    }
                    );




                }
            });
            break;
        case "#tab-imagenes":

            // Check that the browser supports getUserMedia.
            // If it doesn't show an alert, otherwise continue.
            //if (navigator.getUserMedia) {

            //    // Grab elements, create settings, etc.
            //    var canvas = document.getElementById("canvas"),
            //        context = canvas.getContext("2d"),
            //        video = document.getElementById("video"),
            //        videoObj = { "video": true },
            //        image_format = "jpeg",
            //        jpeg_quality = 85,
            //        errBack = function (error) {
            //            console.log("Video capture error: ", error.code);
            //        };

            //    // Request the camera.
            //    navigator.getUserMedia(
            //        // Constraints
            //        {
            //            video: true
            //        },

            //        // Success Callback
            //        function (localMediaStream) {
            //            // Get a reference to the video element on the page.
            //            //var vid = document.getElementById('video');

            //            // Create an object URL for the video stream and use this 
            //            // to set the video source.
            //            video.src = window.URL.createObjectURL(localMediaStream);

            //            $("#snap").show();
            //        },

            //        // Error Callback
            //        function (err) {
            //            // Log the error to the console.
            //            console.log('The following error occurred when trying to use getUserMedia: ' + err);
            //        }
            //    );

            ////    // Get-Save Snapshot - image 
            //document.getElementById("snap").addEventListener("click", function () {
            //    context.drawImage(video, 0, 0, 640, 480);
            //    // the fade only works on firefox?
            //    $("#video").fadeOut("slow");
            //    $("#canvas").fadeIn("slow");
            //    $("#snap").hide();
            //    $("#reset").show();
            //    $("#upload").show();
            //});

            //} else {
            //    alert('Sorry, your browser does not support getUserMedia');
            //}
             video = document.getElementById('video');
             // Get access to the camera!
             if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
                 // Not adding `{ audio: true }` since we only want video now
                 navigator.mediaDevices.getUserMedia({ video: true }).then(function (stream) {
                     video.srcObject = stream;
                     //video.src = window.URL.createObjectURL(stream);
                     video.play();
                     $("#snap").show();
                 });
                 // Elements for taking the snapshot
                 var canvas = document.getElementById('canvas');
                 var context = canvas.getContext('2d');
                 var video = document.getElementById('video');

                 // Trigger photo take
                 document.getElementById("snap").addEventListener("click", function () {
                     context.drawImage(video, 0, 0, 640, 480);
                     $("#video").fadeOut("slow");
                     $("#canvas").fadeIn("slow");
                     $("#snap").hide();
                     $("#reset").show();
                     $("#upload").show();
                 });
             }



/* Legacy code below: getUserMedia 
else if(navigator.getUserMedia) { // Standard
    navigator.getUserMedia({ video: true }, function(stream) {
        video.src = stream;
        video.play();
    }, errBack);
} else if(navigator.webkitGetUserMedia) { // WebKit-prefixed
    navigator.webkitGetUserMedia({ video: true }, function(stream){
        video.src = window.webkitURL.createObjectURL(stream);
        video.play();
    }, errBack);
} else if(navigator.mozGetUserMedia) { // Mozilla-prefixed
    navigator.mozGetUserMedia({ video: true }, function(stream){
        video.src = window.URL.createObjectURL(stream);
        video.play();
    }, errBack);
}
*/


            var filaActiva = $("#ExploracionesTable* .ACTIVA");
            var oidExploracionSeleccionada = filaActiva.data('oid');
            var ContenedorModalImagenes = $('#ContenedorSliderImagenes');
            $.ajax({
                type: 'POST',
                url: '/Imagenes/ListaPartialImagenes',
                data: { oid: oidExploracionSeleccionada },
                beforeSend: function () {
                    ContenedorModalImagenes.html('');
                },
                success: function (data) {
                    ContenedorModalImagenes.html(data);
                }, complete: function () {
                    $('.slick_demo_3').slick({
                        infinite: true,
                        speed: 500,
                        fade: true,
                        cssEase: 'linear',
                        adaptiveHeight: true
                    });
                    $(".slick-list").css("height", "360px");
                }
            });
            break;
        case "#tab-exploracion":
            if (!$("#IOR_MEDICOREFERIDOR").hasClass("select2-hidden-accessible")) {
                $('#IOR_MEDICOREFERIDOR').select2(
                    {
                        theme: "bootstrap",
                        width: "100%",
                        minimumInputLength: 3,
                        quietMillis: 150,
                        delay: 250,
                        //Does the user have to enter any data before sending the ajax request               
                        allowClear: true,
                        ajax: {
                            url: '/Colegiado/GetColegiados',
                            width: 'resolve',
                            data: function (params) {
                                return {
                                    q: params.term// search term
                                };
                            },
                            processResults: function (data) {
                                return {
                                    results: data.items
                                };
                            }
                        }

                    });
            }
      
            $(".fecha-picker,.date-picker").datepicker({
                format: "dd/mm/yyyy",
                todayBtn: true,
                language: "es",
                autoclose: true,
                todayHighlight: true
            });
            break;
        case "#tab-docs":
            var filaActiva = $("#ExploracionesTable* .ACTIVA");
            var oidExploracionSeleccionada = filaActiva.data('oid');
            var ContenedorListaDocumentos = $('#modalDocsContentEntrada');
            $.ajax({
                type: 'GET',
                url: '/Documentos/List/' + oidExploracionSeleccionada,
                beforeSend: function () {

                    ContenedorListaDocumentos.html('');
                },
                success: function (datosLista) {

                    ContenedorListaDocumentos.html(datosLista);
                },
                complete: function () {
                  
                }
            });
            break;
        case "#tab-consumibles":
            var filaActiva = $("#ExploracionesTable* .ACTIVA");
            var mutua = $("#IOR_ENTIDADPAGADORA").val();
            var oidExploracion = filaActiva.data('oid');
            var grupoOid = filaActiva.data('grupo');
            var ContenedorListaConsumibles = $('#contenedorListaConsumiblesMutua');
            $.ajax({
                type: 'GET',
                url: '/Consumible/ListaEntrada',
                data: {
                    codMut: mutua,
                    oidGrupo: grupoOid,
                    oidExploracion: oidExploracion
                },
                beforeSend: function () {
                    $(".spiner-cargando").removeClass('hide');
                    ContenedorListaConsumibles.html('');
                },
                success: function (datosLista) {
                    $(".spiner-cargando").addClass('hide');
                    ContenedorListaConsumibles.html(datosLista);
                },
                complete: function () {
                    $('.dual_select').bootstrapDualListbox({
                        selectorMinimalHeight: 160,
                        selectedListLabel: 'Consumibles Asociados',
                        nonSelectedListLabel: 'Consumibles con Tarifa',
                        infoText: 'Total {0}',
                        infoTextFiltered: '<span class="label label-warning">Filtrando</span> {0} from {1}'
                    });
                }
            });



            break;
        case "#tab-escaner":
            var ContenedorListaDocumentos = $('#contenedorDocumentosEscaneados');
            $.ajax({
                type: 'GET',
                url: '/Documentos/Index',
                beforeSend: function () {
                    $(".spiner-cargando").removeClass('hide');
                    ContenedorListaDocumentos.html('');
                },
                success: function (datosLista) {
                    $(".spiner-cargando").addClass('hide');
                    ContenedorListaDocumentos.html(datosLista);
                },
                complete: function () {
                    $('.footable').footable();
                }
            });
            break;
        case "#tab-informe":

            var filaActiva = $("#ExploracionesTable* .ACTIVA");
            var oidExploracion = filaActiva.data('oid');
            var url = "/Informe/GetParaEnvioMail/"; //Url con la vista parcial que agregamos a la ventana modal
            var email = {}; //The Object to Send Data Back to the Controller
            email.IOR_PACIENTE = filaActiva.data('ior_paciente');
            email.ASUNTO = "Informe";
            email.OWNER = 0;

            email.CID = oidExploracion;
            $.ajax({
                type: 'POST',
                url: url,
                data: email,
                dataType: "html",
                beforeSend: function () {
                    $(".spiner-cargando").removeClass('hide');
                },
                success: function (evt) {
                    $(".spiner-cargando").addClass('hide');
                    //$("#cargandoInforme").addClass('hide');
                    $('#cuerpoModelEnvioMail').html(evt);

                },
            });

            break;
        default:

    }
});

$(document).on("click", "#btnGuardarConsumibles", function myfunction() {

    var filaActivaEntrada = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActivaEntrada.data('oid');


    $.ajax({
        type: 'POST',
        url: '/Consumible/GuardaEntrada',
        dataType: "text xml",
        data: {
            consumibles: $('.dual_select').val(),
            oidExploracion: oidExploracion
        },
        success: function (data) {
            toastr.success('Entrada!', 'Datos modificados!', { timeOut: 5000 });
            filaActivaEntrada.find('.consumibleField')
                .html('<a href="#" data-toggle="modal" data-target="#modal-form-consumibles"><i class="fa fa-eyedropper" style="font-size: 11px; color: blue;" title="Consumible"></i></a>');

        }, error: function (data, textStatus, xhr) {
            toastr.error('Consumible!', xhr, { timeOut: 5000 });


        }
    });

    return false;

});

// evento click del SELECT DE TIPO DE EXPLORACION QUE CAMBIA EL PRECIO
$(document).on('click', '.asociarDocumento,.eliminarDocumentoCarpetaEscan', function () {

    var botonPresionado = $(this);
    var filaActivaEntrada = $("#ExploracionesTable* .ACTIVA");
    var oidExploracion = filaActivaEntrada.data('oid');
    var Accion = (botonPresionado.hasClass('eliminarDocumentoCarpetaEscan') ? 'ELIMINAR' : 'ASOCIAR');
    var filaActivaDocumento = botonPresionado.closest('tr');

    $.ajax({
        type: 'POST',
        url: '/Documentos/Index',
        dataType: "text xml",
        data: {
            oidExploracion: oidExploracion,
            NombreDocumento: botonPresionado.data('fullname'),
            Accion: Accion,
            TipoDocumento: filaActivaDocumento.find('.tipoDocumento').val()
        },
        success: function (data, textStatus, xhr) {

            toastr.success('Documentos!', xhr.statusText, { timeOut: 2000 });
            filaActivaDocumento.fadeOut("normal", function () {
                $(this).remove();
            });
        }, error: function (data, textStatus, xhr) {
            toastr.error('Documentos!', xhr.statusText, { timeOut: 2000 });


        }
    });


});








$(document).ready(function () {
   


    jQuery.validator.setDefaults({
        highlight: function (element, errorClass, validClass) {
            if (element.type === "radio") {
                this.findByName(element.name).addClass(errorClass).removeClass(validClass);
            } else {

                $(element).closest('.form-group').removeClass('has-success has-feedback').addClass('has-error has-feedback');
                $(element).closest('.form-group').find('i.fa').remove();
                $(element).closest('.form-group').append('<i  style="top:33px" class="fa fa-exclamation fa-lg form-control-feedback"></i>');
            }
        },
        unhighlight: function (element, errorClass, validClass) {
            if (element.type === "radio") {
                this.findByName(element.name).removeClass(errorClass).addClass(validClass);
            } else {
                $(element).closest('.form-group').removeClass('has-error has-feedback').addClass('has-success has-feedback');
                $(element).closest('.form-group').find('i.fa').remove();
                $(element).closest('.form-group').append('<i style="top:33px" class="fa fa-check fa-lg form-control-feedback"></i>');
            }
        }
    });

    if ($('#NombreEmpresaGlobal').val().toUpperCase().indexOf('DELFOS') > 0 && 1 === 2) {
        $.validator.addMethod('validMedicoInformante', function (value, element, param) {
            //Your Validation Here
            return value > 0;
        }, 'Valor no válido!');
    } else {
        $.validator.addMethod('validMedicoInformante', function (value, element, param) {
            //Your Validation Here
            return 4 > 0;
        }, 'Valor no válido!');
    }

    $.validator.addMethod('validSelect2', function (value, element, param) {
        //Your Validation Here
        return value > 0;
    }, 'Valor no válido!');

    jQuery.validator.addMethod("contieneComa", function (value, element) {
        return value.indexOf(',') > 0;
    }, "El nombre del paciente debe contener una coma");


    jQuery.validator.addMethod("validDate", function (value, element) {
        return this.optional(element) || moment(value, "DD/MM/YYYY").isValid();
    }, "La fecha de nacimiento tiene que ser en formato DD/MM/YYYY");

    jQuery.validator.addMethod("mynumber", function (value, element) {
        return this.optional(element) || /^(\d+|\d+,\d{1,2})$/.test(value);
    }, "Formato númerico no válido");
});
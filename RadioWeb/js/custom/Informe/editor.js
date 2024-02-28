var caracteresEscritos = [];

//webkitURL is deprecated but nevertheless
URL = window.URL || window.webkitURL;

var gumStream; 						//stream from getUserMedia()
var rec; 							//Recorder.js object
var input;
var recordButton;
var stopButton;
var pauseButton;
var AudioContext = window.AudioContext || window.webkitAudioContext;
var audioContext //audio context to help us record

function gettoken() {
    var token = $("input[name='__RequestVerificationToken']").val();
    return token;
}

// removes MS Office generated guff
function cleanHTML(input) {
    // 1. remove line breaks / Mso classes
    var stringStripper = /(\n|\r| class=(")?Mso[a-zA-Z]+(")?)/g;
    var output = input.replace(stringStripper, ' ');
    // 2. strip Word generated HTML comments
    var commentSripper = new RegExp('<!--(.*?)-->', 'g');
    var output = output.replace(commentSripper, '');
    var tagStripper = new RegExp('<(/)*(meta|link|span|\\?xml:|st1:|o:|font)(.*?)>', 'gi');
    // 3. remove tags leave content if any
    output = output.replace(tagStripper, '');
    // 4. Remove everything in between and including tags '<style(.)style(.)>'
    var badTags = ['style', 'script', 'applet', 'embed', 'noframes', 'noscript'];

    for (var i = 0; i < badTags.length; i++) {
        tagStripper = new RegExp('<' + badTags[i] + '.*?' + badTags[i] + '(.*?)>', 'gi');
        output = output.replace(tagStripper, '');
    }
    // 5. remove attributes ' style="..."'
    var badAttributes = ['style', 'start'];
    for (var i = 0; i < badAttributes.length; i++) {
        var attributeStripper = new RegExp(' ' + badAttributes[i] + '="(.*?)"', 'gi');
        output = output.replace(attributeStripper, '');
    }
    return output;
}

function startRecording() {



    var constraints = { audio: true, video: false }


    recordButton.disabled = true;
    recordButton.addClass('disabled').prop('disabled', true);
    stopButton.disabled = false;
    stopButton.removeClass('disabled').prop('disabled', false);
    pauseButton.disabled = false;
    pauseButton.removeClass('disabled').prop('disabled', false);



	/*
    	We're using the standard promise based getUserMedia() 
    	https://developer.mozilla.org/en-US/docs/Web/API/MediaDevices/getUserMedia
	*/

    navigator.mediaDevices.getUserMedia(constraints).then(function (stream) {


        audioContext = new AudioContext();

        //update the format 
        document.getElementById("formats").innerHTML = "Format: 1 channel pcm @ " + audioContext.sampleRate / 1000 + "kHz"

        /*  assign to gumStream for later use  */
        gumStream = stream;

        /* use the stream */
        input = audioContext.createMediaStreamSource(stream);

		/* 
			Create the Recorder object and configure to record mono sound (1 channel)
			Recording 2 channels  will double the file size
		*/
        rec = new Recorder(input, { numChannels: 1 })

        //start the recording process
        rec.record()



    });
}

function pauseRecording() {

    if (rec.recording) {
        //pause
        rec.stop();
        pauseButton.innerHTML = "Resume";
    } else {
        //resume
        rec.record()
        pauseButton.innerHTML = "Pause";

    }
}

function stopRecording() {


    //disable the stop button, enable the record too allow for new recordings
    stopButton.disabled = true;
    stopButton.addClass('disabled').prop('disabled', true);
    recordButton.disabled = false;
    recordButton.removeClass('disabled').prop('disabled', false);
    pauseButton.disabled = true;
    pauseButton.addClass('disabled').prop('disabled', true);

    //reset button just in case the recording is stopped while paused
    pauseButton.innerHTML = "Pause";

    //tell the recorder to stop the recording
    rec.stop();

    //stop microphone access
    gumStream.getAudioTracks()[0].stop();

    //create the wav blob and pass it on to createDownloadLink
    rec.exportWAV(createDownloadLink);
}

function UploadFile() {
    //we can create form by passing the form to Constructor of formData object
    //or creating it manually using append function 
    //but please note file name should be same like the action Parameter
    //var dataString = new FormData();
    //dataString.append("UploadedFile", selectedFile);

    var form = $('#audioFormSubmit')[0];
    var dataString = new FormData(form);
    $.ajax({
        url: '/Documentos/Upload?OIDEXPLORACIONDOCS=  ' + $("#btnExploracionesHijas").data('oidexploracion'),  //Server script to process data
        type: 'POST',
        //Ajax events
        success: successHandler,
        error: errorHandler,
        complete: completeHandler,
        // Form data
        data: dataString,
        //Options to tell jQuery not to process data or worry about content-type.
        cache: false,
        contentType: false,
        processData: false
    });
}
function createDownloadLink(blob) {

    var url = URL.createObjectURL(blob);
    var au = document.createElement('audio');
    var li = document.createElement('li');
    var link = document.createElement('a');

    //name of .wav file to use during upload and download (without extendion)
    var filename = new Date().toISOString();

    //add controls to the <audio> element
    au.controls = true;
    au.src = url;
    au.style.width = "300" + "px";
    

    //save to disk link
    link.href = url;   
    li.appendChild(au);

    ////add the filename to the li
    //li.appendChild(document.createTextNode(filename + ".wav "))

    ////add the save to disk link to li
    li.appendChild(link);

    var xhr = new XMLHttpRequest();
    xhr.onreadystatechange = function (e) {
        if (this.readyState === 4 && this.status == 200) {
            var span = document.createElement('p');
            span.innerHTML = e.target.responseText;
            span.className = "text-center";
            span.style.width = "300" + "px";
            var aborrar = document.createElement('a');
            aborrar.innerHTML = "<i class='fa fa-trash-o'></i> ";
            aborrar.className = "btn btn-danger eliminarDocumento";
            $(aborrar).attr('data-oid', e.target.responseText.substr(1, e.target.responseText.indexOf('.')-3 ));
            span.appendChild(aborrar);
            li.appendChild(span);
        }
    };
    var url = '/Documentos/UploadAudio?OIDEXPLORACIONDOCS=  ' + $("#btnExploracionesHijas").data('oidexploracion');
    xhr.onload = function (e) {
        if (this.readyState === 4) {
            console.log("Server returned: ", e.target.responseText);
        }
    };
    var fd = new FormData();
    fd.append("file", blob, filename);
    xhr.open("POST", url, true);
    xhr.send(fd);
    //var upload = document.createElement('a');
    //upload.href = "#";
    //upload.innerHTML = "Upload";
    //upload.addEventListener("click", function (event) {
    //    var xhr = new XMLHttpRequest();
    //    xhr.onload = function (e) {
    //        if (this.readyState === 4) {
    //            console.log("Server returned: ", e.target.responseText);
    //        }
    //    };
    //    var url = '/Documentos/UploadAudio?OIDEXPLORACIONDOCS=  ' + $("#btnExploracionesHijas").data('oidexploracion');

    //    var fd = new FormData();
    //    fd.append("file", blob, filename);
    //    xhr.open("POST", url, true);
    //    xhr.send(fd, );
    //})
    //li.appendChild(document.createTextNode(" "))//add a space in between
    //li.appendChild(upload)//add the upload link to li

    //add the li element to the ol
    recordingsList.appendChild(li);
}

$('#modal-form-documentos').on('shown.bs.modal', function (e) {
    var ContenedorListaDocumentos = $('#DocumentosAsociados');
    $.ajax({
        type: 'GET',
        url: '/Documentos/List/' + $("#OWNER").val(),
        beforeSend: function () {

            ContenedorListaDocumentos.html('');
        },
        success: function (datosLista) {

            ContenedorListaDocumentos.html(datosLista);
        },
        complete: function () {

        }
    });
});

$(document).on("click", "#btnExploracionesHijas", function (e) {
    var oidExploracion = $(this).data('oidexploracion');
    var url = "/Exploracion/Hijas/" + oidExploracion; //The Url to the Action  Method of the Controller

    $.ajax({
        type: 'GET',
        url: url,
        dataType: "html",
        beforeSend: function () {

            $('#modal-form-hijas').remove();
        },
        success: function (evt) {
            $('#modal-form-hijas').remove();
            $("body").append(evt);
            $('#modal-form-hijas').modal('show');

            $('.xEditableCabInfExplo').editable({
                placement: 'right',
                source: { 'T': 'Sí' },
                emptytext: 'No'
            });
            $('.xEditableCabInfConsumimble').editable({
                placement: 'right',
                source: { 'T': 'Sí' },
                emptytext: 'No'
            });


        },
    });
});


$(document).on("click", "#btnHistoriaClinica", function (e) {
    var oidPaciente = $("#IOR_PAC").val();
    var url = "/HistoriaClinica/HistoriaEnEditor/" + oidPaciente; //The Url to the Action  Method of the Controller

    $.ajax({
        type: 'GET',
        url: url,
        dataType: "html",
        success: function (evt) {
            $('#modal-form-historia').remove();
            $("body").append(evt);
            $('#modal-form-historia').modal('show');

        },
    });
});

function submitdata() {

    var oInforme = {};
    oInforme.OID = $("#OID").val();
    oInforme.TEXTOHTML = $("#TEXTOHTML").summernote("code");
    oInforme.DURACION = $("#DURACION").val();
    var request = $.ajax({
        url: '/Informe/AutoGuardarTexto',
        data: {
            __RequestVerificationToken: gettoken(),
            viewModel: oInforme,
        },
        type: 'POST',
        async: false,
        dataType: 'JSON',
        contentType: 'application/x-www-form-urlencoded; charset=utf-8'
    });


    request.done(function (data) {

        var now = moment().locale('es').format("dddd, D MMMM YYYY, hh:mm:ss ");

        toastr.success('Informe Guardado - ' + now, 'Autoguardado', { timeOut: 5000 });
    });


    return false;
}




$(document).on("click", "#btnBorrar", function myfunction() {

    swal({
        title: 'Desea borrar este informe',
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "Si",
        cancelButtonText: "No",
        closeOnConfirm: true
    }, function (isConfirm) {
        if (isConfirm) {
            var oidInforme = $("#OID").val();

            $.ajax({
                type: 'DELETE',
                url: '/Informe/DELETE/' + oidInforme,

                success: function (data) {//debereriamos coger del body response
                    toastr.success('', 'Informe Borrado!', { timeOut: 5000 });
                },
                error: function () {
                    swal("Borrado", "Borrado", "error");
                }
            });

        }
    });

});

function validarInforme(oidInforme) {
    $.ajax({
        type: 'POST',
        url: '/Informe/Validar',
        dataType: 'json',
        data: {
            __RequestVerificationToken: gettoken(),
            oid: oidInforme
        },
        success: function (data) {//debereriamos coger del body respons
            switch (data) {
                case "T":
                    toastr.success('', 'Informe Validado!', { timeOut: 5000 });
                    location.reload();
                    break;
                case "A":
                    toastr.success('', 'Informe Anulado!', { timeOut: 5000 });
                    location.reload();
                    break;
                case "F":
                    toastr.success('', 'Su informe no está validado!', { timeOut: 5000 });
                    location.reload();
                    break;
                case "X":
                    swal("Validar", "Ya existe un informe validado para esta exploración", "error");
                    break;
                case "#":
                    swal("Medico Revisión", "El informe solo puede ser validado por el médico de la revisión", "error");
                    break;
                default:
                    break;
            }
            $("#VALIDACION").val(data);
        },
        error: function () {
            swal("Validar", "No se ha podido validar el informe.", "error");
        }
    });


}



$(document).on("click", "#validar", function myfunction() {
    if ($('.note-save .btn').hasClass('btn-danger')) {
        submitdata();
    }
    var oidInforme = $(this).data('oid');
    if ($("#VALIDACION").val() === 'F') {
        swal({
            title: "Desea Imprimir?",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Si",
            cancelButtonText: "No",
            closeOnConfirm: true
        }, function (isConfirm) {


            validarInforme(oidInforme);

            if (isConfirm) {
                var url = "/Informe/Imprimir?oid=" + oidInforme; //The Url to the Action  Method of the Controller
                window.open(url, 'popup', 'width=900,height=500');
            }
        });

    }
    else {
        validarInforme(oidInforme);
    }





});



var printButton = function (context) {
    var ui = $.summernote.ui;
    var button;
    if ($("#VALIDACION").val() === "T") {
        // create button
        button = ui.button({
            contents: '<i class="fa fa-print"/> ',
            tooltip: 'Imprimir',
            container: false,
            click: function () {

                // invoke insertText method with 'hello' on editor module.
                var oidInforme = $("#OID").val();
                var url = "/Informe/Imprimir?oid=" + oidInforme; //The Url to the Action  Method of the Controller
                window.open(url, 'popup', 'width=900,height=500');

            }
        });
    } else {
        button = ui.button({
            contents: '<i class="fa fa-print disabled"/> ',
            tooltip: 'Imprimir',
            container: false,
            click: function () {
                if ($("#UserLogged").data("privilegiado") === -1 || $("#UserLogged").data("perfil") === "Medicos") {
                    // invoke insertText method with 'hello' on editor module.
                    var oidInforme = $("#OID").val();
                    var url = "/Informe/Imprimir?oid=" + oidInforme; //The Url to the Action  Method of the Controller
                    window.open(url, 'popup', 'width=900,height=500');

                } else {
                    swal("Imprimir", "No se puede imprimir un informe sin VALIDAR", "error");

                }
            }
        });
    }
    return button.render();   // return button as jquery object
}

var qReportButton = function (context) {
    var ui = $.summernote.ui;
    var button;
    if ($("#VALIDACION").val() === "T") {

        button = ui.button({
            contents: '<i class="fa fa-search"/> Qreport',
            tooltip: 'Enviar QReport',
            container: false,
            click: function () {
                // invoke insertText method with 'hello' on editor module.
                var oidInforme = $("#OID").val();
                var url = "/Informe/Qreport?oid=" + oidInforme; //The Url to the Action  Method of the Controller
                if ($('.note-save .btn').hasClass('btn-danger')) {
                    submitdata();
                }
                $.ajax({
                    url: url,
                    type: 'GET',
                    success: function (data) {

                        toastr.success('Enviando a QReport ', 'QReport', { timeOut: 5000 });

                    },
                    error: function (x, y, z) {
                        toastr.success('Imposible enviar a QReport ', 'QReport', { timeOut: 5000 });
                    }
                });
            }
        });
    } else {
        button = ui.button({
            contents: '<i class="fa fa-search disabled"/>Qreport',
            tooltip: 'Qreport',
            container: false,
            click: function () {
                // invoke insertText method with 'hello' on editor module.
                swal("Valorar", "No se puede enviar a QReport un informe sin VALIDAR", "error");

            }
        });
    }
    // create button
    return button.render();   // return button as jquery object
}

var xeroxButton = function (context) {
    var ui = $.summernote.ui;
    var button;
    if ($("#VALIDACION").val() === "T") {
        button = ui.button({
            contents: '<i class="fa fa-upload"/> Xerox ',
            tooltip: 'Xerox',
            container: false,
            click: function () {

                var oidInforme = $("#OID").val();
                var url = "/Informe/Xerox?oid=" + oidInforme; //The Url to the Action  Method of the Controller
                //guardamos el contenido el texto
                if ($('.note-save .btn').hasClass('btn-danger')) {
                    submitdata();
                }
                if ($("#EMPRESA").val().toUpperCase().startsWith("GAMMA")) {
                    $.ajax({
                        url: "/Informe/XeroxDelfos?oid=" + oidInforme,
                        type: 'GET',
                        success: function (data) {
                            toastr.success('Enviando a Xerox', 'Imprimir', { timeOut: 2000 });
                        }, error: function (data, textStatus, xhr) {
                            toastr.error('Error Xerox!', xhr.statusText, { timeOut: 5000 });
                        }
                    });
                }
                else {
                    $.ajax({
                        url: url,
                        type: 'GET',
                        success: function (data) {
                            if ($("#MOVILPACIENTE").val() !== "-1") {
                                var stringAviso = "¿Quiere enviar SMS?";
                                if ($("#ALFAS2").val() === 'T') {
                                    stringAviso = "Mensaje enviado. ¿Quiere reenviarlo?";
                                }
                                swal({
                                    title: stringAviso,
                                    type: "warning",
                                    showCancelButton: true,
                                    confirmButtonColor: "#DD6B55",
                                    confirmButtonText: "Si",
                                    cancelButtonText: "No",
                                    closeOnConfirm: true
                                }, function (isConfirm) {
                                    if (isConfirm) {
                                        var options = {
                                            url: "/SMS/Enviar",
                                            data: "phone=" + $("#MOVILPACIENTE").val() + "&texto=" + $("#TEXTOSMS").val() + "&idMensaje=" + $("#OID").val(),
                                            type: "GET"
                                        };
                                        $.ajax(options).complete(function (data) {
                                            toastr.success('Enviando SMS', 'SMS', { timeOut: 5000 });
                                        });

                                        var option = {
                                            url: "/Exploracion/EditarCampo",
                                            data: "name='ALFAS2'&pk=" + $("#OWNER").val() + "&value='T'",
                                            type: "POST"
                                        };
                                        $.ajax(options).complete(function (data) {
                                            toastr.success('Enviado correctamente', 'SMS', { timeOut: 5000 });
                                        });

                                        $('#modal-form-valorar').modal('show');
                                    } else {

                                        $('#modal-form-valorar').modal('show');
                                    }
                                });
                            }
                        },
                        error: function (x, y, z) {
                            alert("Error");
                        }
                    });
                }
            }
        });
    } else {
        button = ui.button({
            contents: '<i class="fa fa-upload disabled"/> Xerox',
            tooltip: 'Xerox',
            container: false,
            click: function () {
                // invoke insertText method with 'hello' on editor module.
                swal("Valorar", "No se puede enviar a Xerox  un informe sin VALIDAR", "error");

            }
        });

    }
    // create button
    return button.render();   // return button as jquery object
}


//recordAudio: recordAudioButton,
//    pauseAudio: pauseAudioButton,
//        stopAudio:stopAudioButton
var stopAudioButton = function (context) {
    var ui = $.summernote.ui;
    var button;
    if ($("#VALIDACION").val() !== "T") {
        // create button
        button = ui.button({
            contents: '<i  class="fa fa-stop"/> ',
            tooltip: 'Parar Grabación',
            container: false,
            click: function () {

                stopRecording();

            }
        });
    } else {
        button = ui.button({
            contents: '<i class="fa fa-stop disabled"/> ',
            tooltip: 'Stop Audio',
            container: false,
            click: function () {

            }
        });
    }
    return button.render();   // return button as jquery object
}
var pauseAudioButton = function (context) {
    var ui = $.summernote.ui;
    var button;
    if ($("#VALIDACION").val() !== "T") {
        // create button
        button = ui.button({
            contents: '<i  class="fa fa-pause"/> ',
            tooltip: 'Pause Grabación',
            container: false,
            click: function () {

                pauseRecording();

            }
        });
    } else {
        button = ui.button({
            contents: '<i class="fa fa-pause disabled"/> ',
            tooltip: 'Grabar Audio',
            click: function () {

            }
        });
    }
    return button.render();   // return button as jquery object
}
var recordAudioButton = function (context) {
    var ui = $.summernote.ui;
    var button;
    if ($("#MODULOGRABARAUDIO").val() === "T") {

    

    if ($("#VALIDACION").val() !== "T") {
        // create button
        button = ui.buttonGroup([
            ui.button({
                contents: '<i style="color:red" class="grabarAudio fa fa-bullseye"/> ',
                tooltip: 'Grabar Audio',
                container: false,
                click: function () {
                    startRecording();
                }
            }),
            ui.button({
                contents: '<i  class="pauseAudio fa fa-pause"/> ',
                tooltip: 'Pausar Audio',
                container: false,
                click: function () {
                    pauseRecording();
                }
            }),
            ui.button({
                contents: '<i class="stopAudio fa fa-stop"/> ',
                tooltip: 'Stop Grabación',
                container: false,
                click: function () {
                    stopRecording();
                }
            })
        ]);
    } else {
        button = ui.button({
            contents: '<i class="fa fa-bullseye disabled"/> ',
            tooltip: 'Grabar Audio',
            container: false,
            click: function () {
                swal("Imprimir", "No se puede modificar la grabación de un informe Validado", "error");
            }
        });
        }
    
    return button.render();   // return button as jquery object
    }
}

$(document).on('change', '#PLANTILLASINFORMES', function () {
    var idPlantilla = $(this).val();


    swal({
        title: "Cambiar actual",
        type: "warning",
        showCancelButton: true,
        confirmButtonColor: "#DD6B55",
        confirmButtonText: "Si",
        cancelButtonText: "No",
        closeOnConfirm: true
    }, function (isConfirm) {
        if (isConfirm) {

            var options = {
                url: "/Informe/Plantilla/" + idPlantilla,
                dataType: 'json',
                type: "GET"
            };
            $.ajax(options).done(function (data) {
                $('#TEXTOHTML').summernote('code', data.TEXTOHTML);
                toastr.success('Plantilla aplicada', 'Plantilla Informe', { timeOut: 5000 });
            });
        }
    });


    return false;
});


function getWord() {
    var sel, word = "";
    if (window.getSelection && (sel = window.getSelection()).modify) {
        var selectedRange = sel.getRangeAt(0);
        sel.collapseToStart();
        sel.modify("move", "backward", "word");
        sel.modify("extend", "forward", "word");

        word = sel.toString();

        sel.deleteFromDocument();
        // Restore selection
        //sel.removeAllRanges();
        //sel.addRange(selectedRange);

    } else if ((sel = document.selection) && sel.type != "Control") {
        var range = sel.createRange();
        range.collapse(true);
        range.expand("word");
        word = range.text;
        range.deleteContents();
    }
    return word;
}

var lastCaretPos = 0;
var parentNode;
var range;
var selection;

$(document).ready(function () {

    $("li[data-view]").removeClass('active');
    $("li[data-view]").removeClass('active');
    $("[data-view=ViewEditorInforme]").addClass("active");
    $("[data-view=ViewEditorInforme]").parents("ul").removeClass("collapse");
    $("[data-view=ViewBusquedaAvanzada]").parents("ul").removeClass("collapse");

    if (localStorage.autoguardado) {
        $("#autoguardadoTime").val(localStorage.autoguardado);
    } else {
        $("#autoguardadoTime").val(5);
        localStorage.autoguardado = 5;
    }

    $("#autoguardadoTime").TouchSpin({
        verticalbuttons: true,
        buttondown_class: 'btn btn-white',
        buttonup_class: 'btn btn-white'
    });

    var hour = 0;
    var minutes = 0;
    var seconds = 0;

    if ($("#DURACION").val()) {
        var res = $("#DURACION").val().split(":");
        hour = parseInt(res[0]);
        minutes = parseInt(res[1]);
        seconds = parseInt(res[2]);
    }

    $('[class*="bootstrap-touchspin-"]').click(function (event) {
        var $this = $(this);
        localStorage.autoguardado = $("#autoguardadoTime").val();
    });

    $('#TEXTOHTML').summernote({
        tabsize: 2,
        height: 300,
        lang: 'es-ES',
        onInit: function () {
            $("#summernote").summernote('code', '<p style="font-family: Verdana;"><br></p>');
        },
        callbacks: {
            onKeyup: function (e) {
                selection = window.getSelection();
                range = selection.getRangeAt(0);
                parentNode = range.commonAncestorContainer.parentNode;

                if (e.which == 118) {
                    var ultimaPalabra = getWord();
                    if ($(parentNode).parents().is('.note-editable') || $(parentNode).is('.note-editable')) {
                        var span = document.createElement('span');
                        $.ajax({
                            type: 'GET',
                            url: '/TextosLibres/Obtener?Numero=' + ultimaPalabra,
                            success: function (data) {
                                span.innerHTML = data;
                            }
                        }
                        );
                        range.deleteContents();
                        range.insertNode(span);
                        //cursor at the last with this
                        range.collapse(false);
                        selection.removeAllRanges();
                        selection.addRange(range);
                    } else {
                        return;
                    }
                }
            }
        },
        onpaste: function (e) {
            var thisNote = $(this);
            var updatePastedText = function (someNote) {
                var original = someNote.code();
                var cleaned = CleanPastedHTML(original); //this is where to call whatever clean function you want. I have mine in a different file, called CleanPastedHTML.
                someNote.code('').html(cleaned); //this sets the displayed content editor to the cleaned pasted code.
            };
            setTimeout(function () {
                //this kinda sucks, but if you don't do a setTimeout, 
                //the function is called before the text is really pasted.
                updatePastedText(thisNote);
            }, 10);
        },
        toolbar: [
            // [groupName, [list of button]]
            ['save', ['save']],
            ['style', ['bold', 'italic', 'underline', 'clear']],
            ['fontsize', ['fontsize', 'fontname']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['misc', ['fullscreen', 'undo', 'redo']],
            ['insert', ['template']],
            ['print', ['print']],
            ['xerox', ['xerox']],
            ['qReport', ['qReport']],
            ['record', ['record']]
        ],
        buttons: {
            print: printButton,
            xerox: xeroxButton,
            qReport: qReportButton,
            record: recordAudioButton

        }
    });
    $('#TEXTOHTML').summernote('focus');

    $('.note-editable').css('font-size', '12px');
    $('.note-editable').css('font-name', 'Verdana');


    $("#PLANTILLASINFORMES").select2({
        theme: "bootstrap"
    });


    //Si el informe está validado solo dejamos imprimirlo
    if (($("#VALIDACION").val() === "T")) {
        $('#TEXTOHTML').summernote('disable');
        $(".fa-print").parent().removeAttr("disabled").removeClass("disabled");
        $(".fa-search").parent().removeAttr("disabled").removeClass("disabled");
    } else {
        //Si el usuario logeado es distinto del medico informante y del medico revisor
        if ($("#UserLogged").data("login") !== $("#LOGINMEDICOINFORMANTE").val()
            && $("#UserLogged").data("login") !== $("#LOGINMEDICOREVISOR").val()
            && $("#UserLogged").data("privilegiado") !== -1) {
            $('#TEXTOHTML').summernote('disable');
            $(".note-icon").parent().removeAttr("disabled").removeClass("disabled");
            $(".fa-print").parent().removeAttr("disabled").removeClass("disabled");
            $(".fa-search").parent().removeAttr("disabled").removeClass("disabled");
        }

    }



    $(".fa-upload").parent().removeAttr("disabled").removeClass("disabled");
    //Autoguardadao
    setInterval(function () {
        if ($('.note-save .btn').hasClass('btn-danger')) {
            submitdata();
        }

    }, localStorage.autoguardado * 60000); // every 5 second interval

    var duration = moment.duration({
        'seconds': seconds,
        'hour': hour,
        'minutes': minutes
    });


    var interval = 1;
    setInterval(function () {

        if ($('.note-save .btn').hasClass('btn-danger')) {
            duration = moment.duration(duration.asSeconds() + interval, 'seconds');
            //.asSeconds() 
            $('#DURACION').val(duration.hours().padLeft(2) + ':' + duration.minutes().padLeft(2) + ':' + duration.seconds().padLeft(2));
        }

    }, 1000);
    recordButton = $(".grabarAudio").parent();
    stopButton = $(".stopAudio").parent();
    pauseButton = $(".pauseAudio").parent();


    stopButton.addClass('disabled').prop('disabled', true);
    pauseButton.addClass('disabled').prop('disabled', true);


});
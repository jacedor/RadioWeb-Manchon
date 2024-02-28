function gettoken() {
    var token = $("input[name='__RequestVerificationToken']").val();
    return token;
}

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
        dataType: 'JSON',
        contentType: 'application/x-www-form-urlencoded; charset=utf-8'
    });


    request.done(function (data) {

        var now = moment().locale('es').format("dddd, D MMMM YYYY, hh:mm:ss ");

        toastr.success('Informe Guardado - ' + now, 'Autoguardado', { timeOut: 5000 });
    });


    return false;
}









$(document).ready(function () {


    $("li[data-view]").removeClass('active');
    $("[data-view=ViewEditorPlantillas]").addClass("active");
    $("[data-view=ViewEditorPlantillas]").parents("ul").removeClass("collapse");


    $('#TEXTOHTML').summernote({
        tabsize: 2,
        height: 300,
        lang: 'es-ES',
        onInit: function () {
            $("#summernote").summernote('code', '<p style="font-family: Verdana;"><br></p>')
        },
        toolbar: [
            // [groupName, [list of button]]
            ['save', ['save']],
            ['style', ['bold', 'italic', 'underline', 'clear']],
            ['fontsize', ['fontsize', 'fontname']],
            ['insert', ['table']],
            ['color', ['color']],
            ['para', ['ul', 'ol', 'paragraph']],
            ['misc', ['fullscreen', 'undo', 'redo', 'codeview']],
            ['insert', ['template']],
         
        ]
    });

    $('#TEXTOHTML').summernote('focus');
    $('.note-editable').css('font-size', '18px');
    $('.note-editable').css('font-name', 'Verdana');


});
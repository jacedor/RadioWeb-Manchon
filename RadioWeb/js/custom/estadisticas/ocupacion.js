


function loadActividadMensual() {


    //cargamos la lista de espera por grupos
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/Ocupacion',
        data: {
            Fecha_Inicio: $("#Fecha_Inicio").val(),
            Fecha_Fin: $("#Fecha_Fin").val(),
            oidGrupo: $("#Grupo").val()
        },
        beforeSend: function () {
            $("#ListaOcupacion").html('');
            $(".spiner-ocupacion").removeClass('hide');
        },
        success: function (data) {
            var $target = $("#ListaOcupacion");
            $target.html(data);
            $(".spiner-ocupacion").addClass('hide');
            makeBootstrapTable("tablaOcupacion");

        }
    });
}

function porcentajeFormat(value, row) {
    return '<div class="stat-percent">' + value + ' </div><div class="progress progress-mini"><div style="width: ' + value + '%;" class="progress-bar"></div></div>';

}
function fixedEncodeURIComponent(str) {
    return encodeURIComponent(str).replace(/[!'()]/g, escape).replace(/\*/g, "%2A");
}

$(document).on("click", ".ocupacionhoras", function myfunction() {

    var urlWithParams = "/Estadisticas/OcupacionPorHoras?Fecha_Inicio=" + $("#Fecha_Inicio").val() + "&Fecha_Fin=" + $("#Fecha_Fin").val() + "&ior_aparato=" + $(this).data('oid');
    $(".spiner-cargando-horasOcupacion").removeClass('hide');
    //cargamos la lista de espera por grupos
    $.ajax({
        type: 'GET',
        url: urlWithParams,

        beforeSend: function () {
            $("#modalCargandoOcupacion").removeClass('hide');
           

        },
        success: function (data) {
            $(".spiner-cargando-horasOcupacion").addClass('hide');

            $('#tblOcupacionHoras').bootstrapTable({
                data: data
            });
            $('#tblOcupacionHoras').bootstrapTable('load', data);
        }
    });
   






});
$(document).on('click', '#btnBuscar', function (ev) {

    loadActividadMensual();

});



$(function () {
    $("li[data-view]").removeClass('active');
    $("[data-view=ViewOcupacion]").addClass("active");

    $("[data-view=ViewOcupacion]").parents("ul").removeClass("collapse");


    $('#fechaSelect').datepicker({
        todayBtn: "linked",
        keyboardNavigation: false,
        forceParse: false,
        autoclose: true,
        language: "es",
        format: "mm-yyyy",
        startView: "months",
        minViewMode: "months"
    });



});
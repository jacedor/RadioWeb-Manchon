function LoadDetailsDay() {
    //filtrosBusqueda = getFiltrosBusqueda();

}

function SuccessFiltros(data) {

    $("#spiner-cargando-calendario").addClass('hide');
    var Contenedor = $("#ViewCalendario");
    Contenedor.removeClass('hide');

}
function CompleteFiltros() {

    LoadDetailsDay();
    var fecha = $("#FILTROS_FECHA").val();
    $("#calendar div[data-fecha='" + moment(fecha).format('MM-DD-YYYY') + "']").addClass('day-highlight');
    if ($("#UserLogged").data('privilegiado') < 0) {

        LoadResumen();

    }

    $('[rel="tooltip"]').tooltip();
    var l = $('#EnviarFiltros').addClass('btn-primary').ladda();
    l.ladda('stop');
}
function BeginFiltros() {
    var l = $('#EnviarFiltros').addClass('btn-info').ladda();
    l.ladda('start');

    $("li[data-view]").removeClass('active');
    $("[data-view=ViewCalendario]").addClass("active");
    var Contenedor = $("#ViewCalendario");
    Contenedor.addClass('hide');
    // Contenedor.empty();
    $("#spiner-cargando-calendario").removeClass('hide');
    if ($("div[data-fecha='" + $("#FILTROS_FECHA").val() + "']").hasClass('festivo')) {
        toastr.warning('Festivo', 'Día Festivo', { timeOut: 5000 });
        $('#addExploracion').addClass('disabled');
        $("#spiner-cargando-calendario").addClass('hide');
        return false;
    }
}
function FailureFiltro() {
    alert('fallo');
}

//funcion que carga el recuento de exploraciones
function LoadResumen() {
    //filtrosBusqueda = getFiltrosBusqueda();
    var options = {
        url: "/Calendario/ResumenDiario?fecha=" + $("#FILTROS_FECHA").val(),
        type: "GET"

    };
    $.ajax(options).done(function (data) {
        var $target = $("#resumen");
        var $newHtml = $(data);
        $target.html(data);
        $('#resumen').slimScroll({
            position: 'right',
            height: '340px',
            railVisible: true,
            alwaysVisible: false
        });

    });
}

$(document).on('click', 'div[data-fecha]', function (ev) {

    var a = $(this);
   
    
    var textoAgenda = $(this).attr('data-content');
    if ($(this).hasClass('festivo')) {
        toastr.warning('Festivo', 'D\xEDa Festivo', { timeOut: 5000 });
        $('#addExploracion').addClass('disabled');
        return false;
    }
    if (textoAgenda !== undefined && textoAgenda.indexOf("#") === 0) {
        swal({
            title: textoAgenda,
            text: $("#FILTROS_FECHA").val()
        });       
    }
   
    $("#norecargarcalendario").val('T');
    $("#FILTROS_FECHA").datepicker("setDate", a.attr('data-fecha'));
    if ($("#UserLogged").data('privilegiado') < 0) {

        LoadResumen();

    } else {
        window.location = "/Home/Index?fecha=" + a.attr('data-fecha');

    }

});

$(document).on('change', 'select[data-filter-calendar=true]', function () {
   

    $("#EnviarFiltros").trigger("click");

    //  SaveFilters();

});


$(document).on('changeDate', '#FILTROS_FECHA', function (ev) {

    if ($("#norecargarcalendario").val() === "F") {
        $("#EnviarFiltros").trigger("click");
    } else {
        $("#norecargarcalendario").val('F');              
        $("#calendar div[data-fecha]").removeClass('day-highlight');
        $("#calendar div[data-fecha='" + $("#FILTROS_FECHA").val() + "']").addClass('day-highlight');
    }

});
$(document).on('click', '#NavigationMonth.pager a', function (ev) {
    var a = $(this);
    var fecha;
    if (a.hasClass('btnHoy')) {
        fecha = moment(new Date()).format('DD/MM/YYYY');
        $("#FILTROS_FECHA").datepicker("setDate", fecha);
        // $("#EnviarFiltros").trigger("click");

    }
    else {
        //fecha = moment(a.attr('data-mes')).format('DD/MM/YYYY');
        $("#FILTROS_FECHA").datepicker("setDate", a.attr('data-mes'));

    }


});
//mapeamos el evento click de cada una de las filas del carrito sobre el aparato que lanza el loadListaDia
$(document).on('click', '.filtroAparatoCarrito', function () {

    $('#ddlAparatos option:contains("' + $(this).data('aparato') + '")').prop('selected', true);
    $("#ddlAparatos").trigger("change");
    $('.popover').hide();
});


$(document).ready(function () {
    $("li[data-view]").removeClass('active');
    $("li[data-view=ViewCalendario]").addClass('active');
    $("[data-view=ViewCalendario]").parents("ul").removeClass("collapse");
    $("#EnviarFiltros").trigger("click");

    $(".wrapper-content").css("padding-top", "0")
});














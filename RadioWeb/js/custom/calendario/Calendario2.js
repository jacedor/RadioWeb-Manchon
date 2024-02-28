/// <reference path="jquery-2.0.3.min.js" />
/// <reference path="jquery-ui-1.10.3.min.js" /

//***********************************************************//
//*******************FUNCIONES ******************************//
//***********************************************************//
//***********************************************************//
var EstadosExploracion = {
    Pendiente: 0,
    Borrado: 1,
    Presencia: 2,
    Confirmado: 3,
    NoPresentado: 4,
    LlamaAnulando: 5
};

var fechaActualGlobal;
var exploracionActualGlobal;
var filtrosBusqueda;

function ShowData() {
    if (sessionStorage.vistaActual === "ViewCalendario" || sessionStorage.vistaActual === undefined) {
        LoadCalendar();
    }
    if (sessionStorage.vistaActual === "ViewListaDia") {
        LoadListaDia();
    }
    //si entramos en este if es porque vamos a agregar una exploracion, y en lugar de ir a buscar datos del servidor con los nuevos filtros
    //nos vamos a la URL indicada en la variable de sesion
    if (sessionStorage.vistaActual.startsWith("/")) {

        window.location = sessionStorage.vistaActual;
    }


}

function SaveFilters() {

    //$.post('/Settings/SaveFiltros', getFiltrosBusqueda(), ShowData);

    var filtros = getFiltrosBusqueda();
    $.ajax({
        type: 'POST',
        url: '/Settings/SaveFiltros',
        data: JSON.stringify(filtros),
        contentType: 'application/json; charset=utf-8',
        complete: function () {
            ShowData();
        }
    });
}

function LoadFilters() {

    $.post('/Settings/LoadFiltros', OnFiltersLoaded);

}


var ctrlPressed = false;
$(window).keydown(function (evt) {
    if (evt.which === 17) { // ctrl
        ctrlPressed = true;
    }
}).keyup(function (evt) {
    if (evt.which === 17) { // ctrl
        ctrlPressed = false;
    }
});
$(window).keydown(function (evt) {
    if (evt.which === 119) {
        $(".huecoLibre").addClass("ACTIVA");
    }
});



var stringStartsWith = function (string, startsWith) {
    string = string || "";
    if (startsWith.length > string.length)
        return false;
    return string.substring(0, startsWith.length) === startsWith;
};




function ActualizarFechaPicker() {
    if (fechaActualGlobal !== undefined) {
        var strDate = fechaActualGlobal;
        var dateParts = strDate.split("-");

        if (dateParts.length === 1) {
            dateParts = strDate.split("/");
        }
        var fechaActual = new Date(dateParts[2], parseInt(dateParts[1]) - 1, dateParts[0]);
        var currentMonth = parseInt(dateParts[1]);
        if (currentMonth < 10) {
            currentMonth = '0' + currentMonth;
        } else {
            currentMonth = '' + currentMonth;
        }
        var currentDay = fechaActual.getDate();
        if (currentDay < 10) {
            currentDay = '0' + currentDay;
        } else {
            currentDay = '' + currentDay;
        }
        $("#fechaSelect").datepicker("setDate", currentDay + "-" + currentMonth + "-" + fechaActual.getFullYear());
        //$("#fechaSelect").val(currentDay + "-" + currentMonth + "-" + fechaActual.getFullYear());
    }

}

//funcion que carga el calendario con los filtros aplicados
function LoadCalendar() {
    $('.popover').hide();
    $("#ViewListaDia").addClass('hide');
    //filtrosBusqueda = getFiltrosBusqueda();
    var options = {
        url: "/Calendario",
        data: JSON.stringify(getFiltrosBusqueda()),
        contentType: 'application/json; charset=utf-8',
        type: "POST"
    };
    $('#calendar div[data-fecha]').removeClass('day-highlight');
    $.ajax(options).done(function (data) {
        var $target = $("#calendario");
        $target.html(data);
        $("#calendario div[data-fecha='" + fechaActualGlobal + "']").addClass('day-highlight');
        //llamamos a la función que cargar el resumen
        LoadDetailsDay();
        sessionStorage.vistaActual = "ViewCalendario";
        $("li[data-view]").removeClass('active');
        $("[data-view=ViewCalendario]").addClass("active");
        $("[data-view=ViewCalendario]").parents("ul").removeClass("collapse");
        $("#ViewCalendario").removeClass('hide');  


        
    });
}

//funcion que carga el recuento de exploraciones
function LoadDetailsDay() {
    //filtrosBusqueda = getFiltrosBusqueda();
    var options = {
        url: "/Calendario/ResumenDiario",
        data: JSON.stringify(getFiltrosBusqueda()),
        contentType: 'application/json; charset=utf-8',
        type: "POST"

    };
    $('#calendar div[data-fecha]').removeClass('day-highlight');

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
        $("#calendar div[data-fecha='" + fechaActualGlobal + "']").addClass('day-highlight');
    });
}





function SetDayDescription(fecha) {
    var dayNumber = moment(fecha, "DD-MM-YYYY").day();
    switch (dayNumber) {
        case 0:
            $('#fechaActualValue').html("Domingo, ");
            break;
        case 1:
            $('#fechaActualValue').html("Lunes, ");

            break;
        case 2:
            $('#fechaActualValue').html("Martes, ");

            break;
        case 3:
            $('#fechaActualValue').html("Mi\xe9rcoles, ");

            break;
        case 4:
            $('#fechaActualValue').html("Jueves, ");

            break;
        case 5:
            $('#fechaActualValue').html("Viernes, ");

            break;
        case 6:
            $('#fechaActualValue').html("S\xe1bado, ");

            break;
        default:

    }
}

function setCurrentDayActions(fechaString) {
    fechaActualGlobal = fechaString;
    ActualizarFechaPicker();  
    filtrosBusqueda.Fecha = fechaActualGlobal;
    SaveFilters();    
    SetDayDescription(fechaActualGlobal);
}




//***********************************************************//
//*******************EVENTOS ******************************//
//***********************************************************//
//***********************************************************//
// store the currently selected tab in the hash value




$(document).on("keypress", "#FindPaciente", function (e) {
    if (e.keyCode === 13) {
        var url = "/Paciente/Index/"; //The Url to the Action  Method of the Controller
        $.ajax({
            type: 'POST',
            url: url,
            data: 'NumRows=' + $("#NumRows").val() + '&Paciente=' + $("#FindPaciente").val() + '&Field=' + $("#btnCriteria").text(),
            dataType: "html",
            success: function (evt) {
                $('.PacientesList').html(evt);
                $('.ui-tooltip').tooltip();
                $('.spinnerPacientes').addClass('hide');
                sessionStorage.filterPaciente = $("#FindPaciente").val().trim().toUpperCase();
                //si esta busqueda no está incluida ya en el array de las 
                //10 ultimas búsquedas la incluímos
                AnadirA10Busquedas($("#FindPaciente").val());
            }
        });
    }
});








////mapeamos el evento cick de los dias activos para ver el resumen
//$(document).on('click', 'div[data-fecha]', function () {
//    var newDate = $(this).attr('data-fecha');

//    if (fechaActualGlobal != moment(newDate).format('DD-MM-YYYY')) {
//        fechaActualGlobal = moment(newDate).format('DD-MM-YYYY');
//        SetDayDescription(fechaActualGlobal);
//        SaveFilters();
//    }

//    setCurrentDayActions($(this).attr('data-fecha'));
//});
//mapeamos el evento click de cada uno de los dias


$(document).on('click', '#IrListadia', function () {

    $('.spinnerExploraciones').removeClass('hide');
    sessionStorage.vistaActual = "ViewListaDia";
    filtrosBusqueda.Fecha = fechaActualGlobal;
    setCurrentDayActions(fechaActualGlobal);


});

$(document).on('keyup', 'select[data-filter-calendar=true]', function (event) {

    if (event.which === 32) {
        var idSelect = $(this).attr('id');
        $('#' + idSelect).val($('#' + 'idSelect option:first').val());
        ShowData();
        return true;
    }
});





////mapeamos el evento click de agregar consmible
//$(document).on('click', '#AgregarConsumible', function () {
//    var exploracionActiva = $("#ExploracionesTable* .ACTIVA");
//    var filaActiva = $("#ConsumiblesList* .ACTIVA");
//    var consumible = {};
//    consumible.OID = filaActiva.data('oid');
//    consumible.IOR_ENTIDADPAGADORA = filaActiva.data('mutua');
//    consumible.PRECIO = filaActiva.data('precio');
//    consumible.OWNER = exploracionActiva.data('oid');
//    $.ajax({
//        type: 'POST',
//        url: '/Consumible/Add',
//        contentType: 'application/json; charset=utf-8',
//        data: JSON.stringify({ oConsumible: consumible }),
//        async: 'false',
//        success: function (data) {
               

//        }
//        });
//});

//mapeamos el evento click de la navegación para cambiar de meses
$(document).on('click', '#NavigationMonth.pager a', function () {
    var a = $(this);

    if (a.hasClass('btnHoy')) {
        fechaActualGlobal = moment(new Date()).format('DD-MM-YYYY');
        setCurrentDayActions(fechaActualGlobal);
    }
    else {
        setCurrentDayActions(a.attr('data-mes'));
    }
});



$(".wrapper-content").swipe({
    swipeRight: function (event, direction, distance, duration, fingerCount) {
        //This only fires when the user swipes Right
        var textoAgenda = $(this).attr('data-content');
        if ($(this).hasClass('festivo')) {
            toastr.warning('Festivo', 'D\xEDa Festivo', { timeOut: 5000 });
            $('#addExploracion').addClass('disabled');
            return false;
        }
        if (textoAgenda !== undefined && textoAgenda.indexOf("#") === 0) {
            alert(textoAgenda);
        }

        $('.spinnerExploraciones').removeClass('hide');
        sessionStorage.vistaActual = "ViewListaDia";
        fechaActualGlobal = $(this).attr('data-fecha');
        filtrosBusqueda.Fecha = fechaActualGlobal;
        setCurrentDayActions(fechaActualGlobal);

    },
    swipeLeft: function (event, direction, distance, duration, fingerCount) {
        sessionStorage.vistaActual = "ViewCalendario";
        LoadCalendar();

    }
});










$(document).ready(function () {

   // LoadFilters();

    ////Elem es el checkbox que determina si se ven los elementos borrados o no
    //var elem = document.querySelector('#chkBorrados');
    //var switchery = new Switchery(elem, { color: '#ED5565' });

    //var elementoHuecos = document.querySelector('#chkHuecos');
    //var switcheryHuecos = new Switchery(elementoHuecos, { color: '#ED5565' });

    //elem.onchange = function () {
    //    if (sessionStorage.vistaActual === "ViewCalendario" || sessionStorage.vistaActual === undefined) {
    //        LoadCalendar();          
    //    }
    //    if (sessionStorage.vistaActual ==="ViewListaDia") {
    //        LoadListaDia();
    //    }
    //};

    //elementoHuecos.onchange = function () {
    //    if (sessionStorage.vistaActual === "ViewCalendario" || sessionStorage.vistaActual === undefined) {
    //        LoadCalendar();
    //    }
    //    if (sessionStorage.vistaActual === "ViewListaDia") {
    //        LoadListaDia();
    //    }
    //};


    fechaActualGlobal = $("#fechaSelect").val();

    $('#fechaSelect').datepicker({
        todayBtn: "linked",
        keyboardNavigation: false,
        forceParse: false,        
        autoclose: true,
        language: "es",
        format: "dd-mm-yyyy",
        calendarWeeks: false
        
    });

   



});




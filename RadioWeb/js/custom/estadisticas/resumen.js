var condiciones = {};
var chartEvolutivo;
var chartMensual;
$.xhrPool = []; // array of uncompleted requests
$.xhrPool.abortAll = function () { // our abort function
    $(this).each(function (idx, jqXHR) {
        jqXHR.abort();
    });
    $.xhrPool.length = 0
};
$.ajaxSetup({
    beforeSend: function (jqXHR) { // before jQuery send the request we will push it to our array
        $.xhrPool.push(jqXHR);
    },
    complete: function (jqXHR) { // when some of the requests completed it will splice from the array
        var index = $.xhrPool.indexOf(jqXHR);
        if (index > -1) {
            $.xhrPool.splice(index, 1);
        }
    }
});



function cargarGraficoDiario() {
    var datos = [];
    var diasX = new Array();
    var diasList = new Array();
    diasList.push("x");

    var actualTotal = new Array();
    actualTotal.push("Actual");

    var actualExplo = new Array();
    actualExplo.push("ExploActual");

    var actualConsu = new Array();
    actualConsu.push("ConsuActual");

    var anteriorTotal = new Array();
    anteriorTotal.push("Anterior");

    var anteriorExplo = new Array();
    anteriorExplo.push("ExploAnterior");

    var anteriorConsu = new Array();
    anteriorConsu.push("ConsuAnterior");

    var selectorActual = ".DiarioTotalValorActual";
    var selectorAnterior = ".DiarioTotalValorAnterior";
    var selectorExploActual = ".exploracionesTotalDiarioValorActual";
    var selectorExploAnterior = ".exploracionesTotalDiarioValorAnterior";
    var selectorConsuActual = ".consumiblesTotalDiarioValorAnterior";
    var selectorConsuAnterior = ".consumiblesTotalDiarioValorActual";
    if ($("#ddlCuentaOSuma").val() === "COUNT") {
        selectorActual = ".exploracionesDiarioTotalCuentaAnterior";
        selectorAnterior = ".exploracionesDiarioCuentaActual";
    }
    if ($(selectorActual).length === 0 && $(selectorAnterior).length === 0 && $(selectorExploActual).length === 0
        && $(selectorExploAnterior).length === 0 && $(selectorConsuActual).length === 0 && $(selectorConsuAnterior).length === 0) {
        if ($("#ddlCuentaOSuma").val() === "SUM") {
            selectorActual = ".DiarioValorActual";
            selectorAnterior = ".DiarioValorAnterior";
            selectorExploActual = ".exploracionesValorActual";
            selectorExploAnterior = ".exploracionesValorAnterior";
            selectorConsuActual = ".consumiblesValorActual";
            selectorConsuAnterior = ".consumiblesValorAnterior";
        } else {
            selectorActual = ".exploracionesDiarioCuentaActual";
            selectorAnterior = ".exploracionesDiarioCuentaAnterior";
        }

    }


    $(".exploracionesDia").each(function () {
        if ($(this).val().substring(8, 11) === "01" || $(this).val().substring(8, 11) === "07" || $(this).val().substring(8, 11) === "15") {
            diasX.push($(this).val());
        }
        diasList.push($(this).val());
    });

    $(selectorActual).each(function () {
        actualTotal.push(parseFloat($(this).val()));
    });

    $(selectorAnterior).each(function () {
        anteriorTotal.push(parseFloat($(this).val()));
    });

    $(selectorExploActual).each(function () {
        actualExplo.push(parseFloat($(this).val()));
    });

    $(selectorExploAnterior).each(function () {
        anteriorExplo.push(parseFloat($(this).val()));
    });

    $(selectorConsuActual).each(function () {
        actualConsu.push(parseFloat($(this).val()));
    });

    $(selectorConsuAnterior).each(function () {
        anteriorConsu.push(parseFloat($(this).val()));
    });

    datos.push(diasList);
    datos.push(actualTotal);
    datos.push(anteriorTotal);

    //if ($("#ddlCuentaOSuma").val() != "COUNT") {
    //    datos.push(actualExplo);
    //    datos.push(actualConsu);
    //    datos.push(anteriorExplo);
    //    datos.push(anteriorConsu);
    //}
    $("#spiner-cargando-graficoDiario").addClass('hide');
    $('#chartDiario').removeClass('hide');
    $("#ViewGraficoDiario").removeClass('hide');
    var chart = c3.generate({
        bindto: '#chartDiario',
        data: {
            x: 'x',
            columns: datos

        },
        grid: {
            x: {
                show: true
            },
            y: {
                show: true
            }
        },
        zoom: {
            enabled: true
        },
        axis: {
            x: {
                type: 'timeseries',
                tick: {
                    // this also works for non timeseries data
                    values: diasX,
                    format: "%e %b"
                }
            },
            y: {
                tick: {
                    format: function (d) {
                        if ($("#ddlCuentaOSuma").val() != "COUNT") {
                            return addCommas(d) + $("#SimboloMonedaGlobal").val();//" \u20AC";
                        } else {
                            return d;
                        }

                    }
                }
            }
        }

    });
}

function cargarAcumulado() {
    var axisData = [];

    var datos = [];
    var actualTotal = new Array();
    actualTotal.push("Actual");

    var anteriorTotal = new Array();
    anteriorTotal.push("Anterior");

    var selectorActual = ".totalValorActualAcumulado";
    var selectorAnterior = ".totalValorAnteriorAcumulado";
    $(".grupoAcumulado").each(function () {
        axisData.push($(this).val());
    });

    if ($("#ddlCuentaOSuma").val() === "COUNT") {
        selectorAnterior = ".totalCuentaAnteriorAcumulado";
        selectorActual = ".totalCuentaActualAcumulado";

    }


    $(selectorActual).each(function () {
        actualTotal.push(parseFloat($(this).val()));
    });



    $(selectorAnterior).each(function () {
        anteriorTotal.push(parseFloat($(this).val()));
    });

    datos.push(actualTotal);
    datos.push(anteriorTotal);

    $("#spiner-cargando-graficoAcumulado").addClass('hide');
    $('#chartAcumulado').removeClass('hide');
    $("#ViewGraficoAcumulado").removeClass('hide');
    chartEvolutivo = c3.generate({
        bindto: '#chartAcumulado',
        data: {
            columns: datos,
            type: 'bar'
        },
        axis: {
            x: {
                type: 'category',
                categories: axisData

            },
            y: {
                tick: {
                    format: function (d) {
                        if ($("#ddlCuentaOSuma").val() != "COUNT") {
                            return addCommas(d) + $("#SimboloMonedaGlobal").val();// \u20AC";
                        } else {
                            return d;
                        }

                    }
                }
            }
        }

    });

}
function cargarEvolutivo() {
    var axisData = [];

    var datos = [];
    var actualTotal = new Array();
    actualTotal.push("Actual");


    var selectorActual = ".TotalEvolutivo";
    $(".evolutivoAnyo").each(function () {
        axisData.push($(this).val());
    });

    if ($("#ddlCuentaOSuma").val() === "COUNT") {
        selectorActual = ".TotalEvolutivoCuenta";
    }

    if ($(selectorActual).length === 0) {
        if ($("#ddlCuentaOSuma").val() === "SUM") {
            selectorActual = ".TotalEvolutivoFiltrado";
        } else {
            selectorActual = ".TotalEvolutivoCuentaFiltrado";
        }
        axisData = [];
        $(".evolutivoAnyoFiltrado").each(function () {
            axisData.push($(this).val());
        });
    }



    $(selectorActual).each(function () {
        actualTotal.push(parseFloat($(this).val()));
    });

    datos.push(actualTotal);


    $("#spiner-cargando-graficoEvolutivo").addClass('hide');
    $("#ViewGraficoEvolutivo").removeClass('hide');
    $("#chartEvolutivo").removeClass('hide');
    chartEvolutivo = c3.generate({
        bindto: '#chartEvolutivo',
        data: {
            columns: datos,
            types: {
                Actual: 'area-spline',

            }
        },
        colors: {
            Actual: '#00ff00'
        },
        axis: {
            x: {
                type: 'category',
                categories: axisData

            },
            y: {
                tick: {
                    format: function (d) {
                        if ($("#ddlCuentaOSuma").val() != "COUNT") {
                            return addCommas(d) + $("#SimboloMonedaGlobal").val();//" \u20AC";
                        } else {
                            return d;
                        }

                    }
                }
            }
        }

    });
    // $("#evolutivoGrafico").trigger('click');
}
//carga grafico mensual
function cargarGrafico() {
    var axisData = [];

    var datos = [];
    var actualTotal = new Array();
    actualTotal.push("Actual");

    var actualExplo = new Array();
    actualExplo.push("ExploActual");

    var actualConsu = new Array();
    actualConsu.push("ConsuActual");

    var anteriorTotal = new Array();
    anteriorTotal.push("Anterior");

    var anteriorExplo = new Array();
    anteriorExplo.push("ExploAnterior");

    var anteriorConsu = new Array();
    anteriorConsu.push("ConsuAnterior");

    var selectorActual = ".totalAnualValorActual";
    var selectorAnterior = ".totalAnualValorAnterior";
    var selectorExploActual = ".exploracionesAnualValorActual";
    var selectorExploAnterior = ".exploracionesAnualValorAnterior";
    var selectorConsuActual = ".consumiblesAnualValorActual";
    var selectorConsuAnterior = ".consumiblesAnualValorAnterior";
    var selectorAxis = ".mesNombre";
    if ($("#ddlCuentaOSuma").val() === "COUNT") {
        selectorActual = ".exploracionesAnualCuentaActual";
        selectorAnterior = ".exploracionesAnualCuentaAnterior";
    }
    if ($(selectorActual).length === 0 && $(selectorAnterior).length === 0 && $(selectorExploActual).length === 0
        && $(selectorExploAnterior).length === 0 && $(selectorConsuActual).length === 0 && $(selectorConsuAnterior).length === 0) {
        if ($("#ddlCuentaOSuma").val() === "SUM") {
            selectorActual = ".totalValorActual";
            selectorAnterior = ".totalValorAnterior";
            selectorExploActual = ".exploracionesValorActual";
            selectorExploAnterior = ".exploracionesValorAnterior";
            selectorConsuActual = ".consumiblesValorActual";
            selectorConsuAnterior = ".consumiblesValorAnterior";
            selectorAxis = ".mesNombreFiltrado";
        } else {
            selectorActual = ".exploracionesCuentaActual";
            selectorAnterior = ".exploracionesCuentaAnterior";
        }

    }

    $(selectorAxis).each(function () {
        axisData.push($(this).val());
    });

    $(selectorActual).each(function () {
        actualTotal.push(parseFloat($(this).val()));
    });

    $(selectorAnterior).each(function () {
        anteriorTotal.push(parseFloat($(this).val()));
    });

    //$(selectorExploActual).each(function () {
    //    actualExplo.push(parseFloat($(this).val()));
    //});

    //$(selectorExploAnterior).each(function () {
    //    anteriorExplo.push(parseFloat($(this).val()));
    //});

    //$(selectorConsuActual).each(function () {
    //    actualConsu.push(parseFloat($(this).val()));
    //});

    //$(selectorConsuAnterior).each(function () {
    //    anteriorConsu.push(parseFloat($(this).val()));
    //});

    datos.push(actualTotal);
    datos.push(anteriorTotal);
    if ($("#ddlCuentaOSuma").val() != "COUNT") {
        datos.push(actualExplo);
        // datos.push(actualConsu);
        datos.push(anteriorExplo);
        //datos.push(anteriorConsu);
    }
    $("#spiner-cargando-graficoLineas").addClass('hide');
    $("#ViewGraficoMensual").removeClass('hide');
    $('#chart').removeClass('hide');
    chartMensual = c3.generate({
        bindto: '#chart',
        data: {
            columns: datos,
            types: {
                Anterior: 'area-spline',
                //ExploAnterior: 'bar',
                //ConsuAnterior: 'bar',
                Actual: 'area-spline',
                //ExploActual: 'bar',
                // ConsuActual: 'bar'

            }
        },
        colors: {
            Anterior: '#0000ff',
            //ExploAnterior: '#0000ff',
            //ConsuAnterior: '#0000ff',
            Actual: '#00ff00',
            //ExploActual: '#00ff00',
            //ConsuActual: '#00ff00'
        },
        axis: {
            x: {
                type: 'category',
                categories: axisData,
                padding: { left: 0 }

            },
            y: {
                tick: {
                    format: function (d) {
                        if ($("#ddlCuentaOSuma").val() != "COUNT") {
                            return addCommas(d) + $("#SimboloMonedaGlobal").val();//" \u20AC";
                        } else {
                            return d;
                        }

                    }
                }
            }
        }

    });
    //$("#mensualGrafico").trigger('click');
}


function cargarDatosDiario() {
    $("#spiner-cargando-diario").removeClass('hide');
    $("#spiner-cargando-graficoDiario").removeClass('hide');
    $('#ViewDiariaTable>.table-responsive').empty();
    $('#ViewGraficoDiario').addClass('hide');

    condiciones.fechaInicial = $('input[id="FILTROS_FECHA"]').data('daterangepicker').startDate.format('DD-MM-YYYY');
    condiciones.fechaFinal = $('input[id="FILTROS_FECHA"]').data('daterangepicker').endDate.format('DD-MM-YYYY');
    condiciones.tipoPago = $("#FILTROS_IOR_TIPO").val();
    condiciones.centro = $("#FILTROS_IOR_CENTRO").val();
    condiciones.mutua = $("#FILTROS_IOR_ENTIDADPAGADORA").val();
    condiciones.pagado = $("#FILTROS_PAGADO").val();
    condiciones.facturado = $("#FILTROS_FACTURADA").val();
    condiciones.grupo = $("#FILTROS_IOR_GRUPO").val();
    condiciones.oidMedicoInformante = $("#FILTROS_IOR_MEDICO").val();
    condiciones.ior_colegiado = $("#FILTROS_IOR_COLEGIADO").val();
    condiciones.anyo = $("#ddlAnyo").val();
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/ResumenDiario/',
        data: condiciones,
        dataType: "html",
        success: function (evt) {

            $("#spiner-cargando-diario").addClass('hide');
            $('#ViewDiariaTable>.table-responsive').html(evt);
            $('#tblResumenDiario tbody tr:last').addClass('bg-success');
            $("#tblResumenDiario").bootstrapTable();

            cargarGraficoDiario();
        },
    });
}

function cargarAnual() {
    $("#spiner-cargando-acumulado").removeClass('hide');
    $("#spiner-cargando-graficoAcumulado").removeClass('hide');
    $('#chartAcumulado').addClass('hide');

    condiciones.fechaInicial = $('input[id="FILTROS_FECHA"]').data('daterangepicker').startDate.format('DD-MM-YYYY');
    condiciones.fechaFinal = $('input[id="FILTROS_FECHA"]').data('daterangepicker').endDate.format('DD-MM-YYYY');
    condiciones.tipoPago = $("#FILTROS_IOR_TIPO").val();
    condiciones.centro = $("#FILTROS_IOR_CENTRO").val();
    condiciones.mutua = $("#FILTROS_IOR_ENTIDADPAGADORA").val();
    condiciones.pagado = $("#FILTROS_PAGADO").val();
    condiciones.facturado = $("#FILTROS_FACTURADA").val();

    condiciones.grupo = $("#FILTROS_IOR_GRUPO").val();
    condiciones.oidMedicoInformante = $("#FILTROS_IOR_MEDICO").val();
    condiciones.ior_colegiado = $("#FILTROS_IOR_COLEGIADO").val();
    condiciones.anyo = $("#ddlAnyo").val();
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/ResumenAcumulado/',
        data: condiciones,
        dataType: "html",
        success: function (evt) {
            $("#spiner-cargando-acumulado").addClass('hide');
            $("#spiner-cargando-graficoAcumulado").addClass('hide');
            $('#ViewAcumuladoTable>.table-responsive').html(evt);
            $('#tbltotales tbody tr:last').addClass('bg-success');
            $("#tbltotales").bootstrapTable();
            cargarAcumulado();
        },
    });
}

function cargarMensual() {
    $("#spiner-cargando-resumen").removeClass('hide');
    $("#spiner-cargando-graficoLineas").removeClass('hide');
    $('#chart').addClass('hide');

    condiciones.fechaInicial = $('input[id="FILTROS_FECHA"]').data('daterangepicker').startDate.format('DD-MM-YYYY');
    condiciones.fechaFinal = $('input[id="FILTROS_FECHA"]').data('daterangepicker').endDate.format('DD-MM-YYYY');
    condiciones.tipoPago = $("#FILTROS_IOR_TIPO").val();
    condiciones.centro = $("#FILTROS_IOR_CENTRO").val();
    condiciones.mutua = $("#FILTROS_IOR_ENTIDADPAGADORA").val();
    condiciones.pagado = $("#FILTROS_PAGADO").val();
    condiciones.facturado = $("#FILTROS_FACTURADA").val();

    condiciones.grupo = $("#FILTROS_IOR_GRUPO").val();
    condiciones.oidMedicoInformante = $("#FILTROS_IOR_MEDICO").val();
    condiciones.ior_colegiado = $("#FILTROS_IOR_COLEGIADO").val();
    condiciones.anyo = $("#ddlAnyo").val();

    $("#tblMensual").addClass('hide');
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/ResumenMensual/',
        data: condiciones,
        dataType: "html",
        success: function (evt) {
            // $('#ViewResumen').html(evt);
            $('#ViewMensualTable>.table-responsive').html(evt);
            cargarGrafico();
            $("#spiner-cargando-resumen").addClass('hide');
            $("#spiner-cargando-graficoLineas").addClass('hide');
            $("#tblMensual").removeClass('hide');

            $("#tblMensual").bootstrapTable();

        }
    });
}

function cargarTablaEvolutivo() {
    $("#spiner-cargando-evolutivo").removeClass('hide');
    $("#spiner-cargando-graficoEvolutivo").removeClass('hide');
    $('#chartEvolutivo').addClass('hide');

    condiciones.fechaInicial = $('input[id="FILTROS_FECHA"]').data('daterangepicker').startDate.format('DD-MM-YYYY');
    condiciones.fechaFinal = $('input[id="FILTROS_FECHA"]').data('daterangepicker').endDate.format('DD-MM-YYYY');
    condiciones.tipoPago = $("#FILTROS_IOR_TIPO").val();
    condiciones.centro = $("#FILTROS_IOR_CENTRO").val();
    condiciones.mutua = $("#FILTROS_IOR_ENTIDADPAGADORA").val();
    condiciones.pagado = $("#FILTROS_PAGADO").val();
    condiciones.facturado = $("#FILTROS_FACTURADA").val();

    condiciones.grupo = $("#FILTROS_IOR_GRUPO").val();
    condiciones.oidMedicoInformante = $("#FILTROS_IOR_MEDICO").val();
    condiciones.ior_colegiado = $("#FILTROS_IOR_COLEGIADO").val();
    condiciones.anyo = $("#ddlAnyo").val();
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/ResumenAcumuladoEvolutivo/',
        data: condiciones,
        dataType: "html",
        success: function (evt) {
            $("#spiner-cargando-graficoEvolutivo").addClass('hide');
            $("#spiner-cargando-evolutivo").addClass('hide');

            $('#ViewEvolutivaTable>.table-responsive').html(evt);
            $('#totalesEvolutivo tbody tr:last').addClass('bg-success');
            $("#totalesEvolutivo").bootstrapTable();
            $("#busquedaJson").val(JSON.stringify(condiciones));
            cargarEvolutivo();

        }
    });
}

function cargarDatos(start, end, label) {
    $("#spiner-cargando-resumen").removeClass('hide');
    $("#spiner-cargando-graficoLineas").removeClass('hide');
    $("#spiner-cargando-graficoEvolutivo").removeClass('hide');
    $("#spiner-cargando-graficoAcumulado").removeClass('hide');
    $("#spiner-cargando-acumulado").removeClass('hide');
    $("#spiner-cargando-evolutivo").removeClass('hide');
    $('#ViewAcumuladoTable>.table-responsive').empty();
    $('#ViewMensualTable>.table-responsive').empty();
    $('#ViewEvolutivaTable>.table-responsive').empty();
    $('#chart').addClass('hide');
    $("#chartEvolutivo").addClass('hide');
    $("#chartDiario").addClass('hide');
    condiciones.fechaInicial = start.format('DD-MM-YYYY');
    condiciones.fechaFinal = end.format('DD-MM-YYYY');
    condiciones.tipoPago = $("#FILTROS_IOR_TIPO").val();
    condiciones.centro = $("#FILTROS_IOR_CENTRO").val();
    condiciones.mutua = $("#FILTROS_IOR_ENTIDADPAGADORA").val();
    condiciones.pagado = $("#FILTROS_PAGADO").val();
    condiciones.facturado = $("#FILTROS_FACTURADA").val();

    condiciones.grupo = $("#FILTROS_IOR_GRUPO").val();
    condiciones.oidMedicoInformante = $("#FILTROS_IOR_MEDICO").val();
    condiciones.ior_colegiado = $("#FILTROS_IOR_COLEGIADO").val();
    condiciones.anyo = $("#ddlAnyo").val();
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/ResumenAcumulado/',
        data: condiciones,
        dataType: "html",
        success: function (evt) {
            $("#spiner-cargando-acumulado").addClass('hide');
            $("#spiner-cargando-graficoAcumulado").addClass('hide');
            $('#ViewAcumuladoTable>.table-responsive').html(evt);
            $('#tbltotales tbody tr:last').addClass('bg-success');
            $("#tbltotales").bootstrapTable();
            cargarAcumulado();
        },
    });

    $.ajax({
        type: 'POST',
        url: '/Estadisticas/ResumenMensual/',
        data: condiciones,
        dataType: "html",
        success: function (evt) {
            // $('#ViewResumen').html(evt);
            $('#ViewMensualTable>.table-responsive').html(evt);
            cargarGrafico();
            $("#spiner-cargando-resumen").addClass('hide');
            $("#spiner-cargando-graficoLineas").addClass('hide');
            $("#tblMensual").bootstrapTable();

        }
    });
    var myJSON = JSON.stringify(condiciones);
    if ($("#busquedaJson").val() !== myJSON) {
        swal({
            title: 'Desea cargar la estadistica diaria?',
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Si",
            cancelButtonText: "No",
            closeOnConfirm: true
        }, function (isConfirm) {
            if (isConfirm) {
                cargarDatosDiario($('input[id="FILTROS_FECHA"]').data('daterangepicker').startDate, $('input[id="FILTROS_FECHA"]').data('daterangepicker').endDate, '');
            }
            else {
                $("#chartDiario").removeClass('hide');
            }
        });

        $.ajax({
            type: 'POST',
            url: '/Estadisticas/ResumenAcumuladoEvolutivo/',
            data: condiciones,
            dataType: "html",
            success: function (evt) {
                $("#spiner-cargando-graficoEvolutivo").addClass('hide');
                $("#spiner-cargando-evolutivo").addClass('hide');
                $('#ViewEvolutivaTable>.table-responsive').html(evt);
                $('#totalesEvolutivo tbody tr:last').addClass('bg-success');
                $("#totalesEvolutivo").bootstrapTable();
                $("#busquedaJson").val(JSON.stringify(condiciones));
                cargarEvolutivo();

            }
        });
    }


}



function addCommas(nStr) {

    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? ',' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + '.' + '$2');
    }
    return x1 + x2;
}

$(document).on('change', '#ddlCuentaOSuma', function myfunction() {
    var graficoActivo = $(".graficobutton.active").find("input").val();

    switch (graficoActivo) {
        case "ViewGraficoAcumulado":
            cargarAcumulado();
            break;
        case "ViewGraficoMensual":
            cargarGrafico();
            break;
        case "ViewGraficoEvolutivo":
            cargarEvolutivo();
            break;
        case "ViewGraficoDiario":
            cargarGraficoDiario();
            break;
        default:
    }

});

$(document).on('click', '#EnviarFiltros', function myfunction() {

    $('.graficoContainer').addClass('hide');
    switch ($(".opcionActiva").val()) {
        case "ViewAcumuladoTable":
            cargarAnual();
            break;
        case "ViewMensualTable":
            cargarMensual();
            break;
        case "ViewEvolutivaTable":
            cargarTablaEvolutivo();

            break;
        case "ViewDiariaTable":
            cargarDatosDiario();
            break;

    }


});

$(document).on('change', 'input[name=optionsTabla]', function myfunction() {
    
    var selector = $(this).val();
   
    $(".opcionActiva").removeClass('opcionActiva');
    $(this).addClass('opcionActiva');
    $('#' + selector).siblings('[id^="View"]').addClass('hide');
    $('.graficoContainer').addClass('hide');
    $('#' + selector).removeClass('hide');   
    $('#' + selector).parent().addClass('active');
    if ($('#' + selector).find("table").length > 0) {
        $(".xlsx").removeAttr("onclick");
        $(".xlsx").off("click");
        $(".xlsx").on("click", function () {
            exportExcel('xlsx', $($('#' + selector).find("table")[1]).attr('id'), selector);
        });
    }
    //$("#EnviarFiltros").trigger('click');
    switch (selector) {
        case "ViewAcumuladoTable":
            
            $("#ViewGraficoAcumulado").removeClass('hide');
            break;
        case "ViewMensualTable":
            $("#ViewGraficoMensual").removeClass('hide');

            break;
        case "ViewEvolutivaTable":
            $("#ViewGraficoEvolutivo").removeClass('hide');

            break;
        case "ViewDiariaTable":
            $("#ViewGraficoDiario").removeClass('hide');
            


            break;
        default:
    }

});

//$(document).on('change', 'input[name=optionsGrafico]', function myfunction() {
//    var selector = $(this).val();
//    $('#' + selector).siblings('[id^="View"]').addClass('hide');
//    $('#' + selector).removeClass('hide');
//    switch (selector) {
//        case "ViewGraficoAcumulado":
//            $("#anualTabla").trigger('click');
//            break;
//        case "ViewGraficoMensual":
//            $("#mensualTabla").trigger('click');
//            break;
//        case "ViewGraficoEvolutivo":
//            $("#evolutivaTabla").trigger('click');

//            break;
//        case "ViewGraficoDiario":
//            $("#diariaTabla").trigger('click');


//            break;
//        default:
//    }
//});

// Fullscreen ibox function
$('.fullscreen-link').on('click', function (e) {
    e.preventDefault();
    var ibox = $(this).closest('div.ibox');
    var button = $(this).find('i');
    $('body').toggleClass('fullscreen-ibox-mode');
    button.toggleClass('fa-expand').toggleClass('fa-compress');
    ibox.toggleClass('fullscreen');
    setTimeout(function () {
        chartEvolutivo.resize();
        $(window).trigger('resize');
    }, 100);

});

$(document).on("search.bs.table", "#tblResumenDiario", function (e) {

    cargarGraficoDiario();
});

$(document).on("search.bs.table", "#totalesEvolutivo", function (e) {

    cargarEvolutivo();
});

$(document).on("search.bs.table", "#tblMensual", function (e) {

    cargarGrafico();
});






$(function () {
    $("li[data-view]").removeClass('active');
    $("[data-view=ViewResumen]").addClass("active");

    $("[data-view=ViewResumen]").parents("ul").removeClass("collapse");
    moment.locale('es-ES');
    $('input[id="FILTROS_FECHA"]').daterangepicker(
        {
            format: 'DD/MM/YYYY',
            ranges: {
                'Ultimos 7 Dias': [moment().subtract(6, 'days'), moment()],
                'Ultimos 30 Dias': [moment().subtract(29, 'days'), moment()],
                'Hoy': [moment(), moment()]
            },
            locale: {
                applyLabel: 'Buscar',
                cancelLabel: 'Cancelar',
                fromLabel: 'Desde',
                toLabel: 'Hasta',
                firstDay: 1
            }
        }
        //,
        //function (start, end, label) {

        //    cargarDatos(start, end, label);


        //}
    );
    $("select[data-filter-calendar=true]").on("change", function () { return false; }); //this works.

    //  cargarDatos($('input[id="FILTROS_FECHA"]').data('daterangepicker').startDate, $('input[id="FILTROS_FECHA"]').data('daterangepicker').endDate, '');



});
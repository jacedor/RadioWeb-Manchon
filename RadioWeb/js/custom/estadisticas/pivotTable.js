


var tableToExcel = (function () {
    var uri = 'data:application/vnd.ms-excel;base64,'
        , template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet ></x: ExcelWorksheets ></x: ExcelWorkbook ></xml ><![endif]-- > <meta http-equiv="content-type" content="text/plain; charset=UTF-8" /></head > <body><table>{table}</table></body></html > '
        , base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))) }
        , format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) }
    return function (table, name) {
        if (!table.nodeType) table = document.getElementById(table)
        var ctx = { worksheet: name || 'Worksheet', table: table.innerHTML }
        window.location.href = uri + base64(format(template, ctx))
    }
})()





$("#GuardarCrossTable").click(function (e) {
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/PivotTableData',
        data: { oid: $("#OID").val(), data: $("#output").text(), "fechaInicio": $("#FECHA_INICIO").val(), "fechafin": $("#FECHA_FIN").val() },
        complete: function () {
            toastr.success('', 'Tabla Dinamica almacenada!', { timeOut: 5000 });
        }
    });
});

//los evento change y mover son up y down de todos los filtros del calendario
$(document).on('change', '#IORPIVOTTABLE', function () {
    window.location.href = '/Estadisticas/PivotTable?oid=' + $("#IORPIVOTTABLE").val();
});

$("form").on("submit", function () {
    var datosPivotTable = JSON.parse($("#datosPivotTable").val());
    var derivers = $.pivotUtilities.derivers;
    //var renderers = $.extend($.pivotUtilities.renderers,
    //    $.pivotUtilities.plotly_renderers);
    var derivers = $.pivotUtilities.derivers;

    var renderers = $.extend(
        $.pivotUtilities.renderers,
        $.pivotUtilities.plotly_renderers,
        $.pivotUtilities.d3_renderers,
        $.pivotUtilities.export_renderers
    );
    $.getJSON('/Estadisticas/PivotTableData', {
        "FECHA_INICIO": $("#FECHA_INICIO").val(), "FECHA_FIN": $("#FECHA_FIN").val()
    }, function (result) {
        $("#ContentTable").pivotUI(result, {
            renderers: renderers,
            rows: datosPivotTable.rows,
            cols: datosPivotTable.cols,
            aggregatorName: datosPivotTable.aggregatorName,
            vals: datosPivotTable.vals,
            inclusions: datosPivotTable.inclusions,
            exclusions: datosPivotTable.exclusions,
            rowOrder: datosPivotTable.rowOrder,
            colOrder: datosPivotTable.colOrder,
            rendererName: datosPivotTable.rendererName,
            table: {
                clickCallback: function (e, value, filters, pivotData) {
                    var names = [];
                    pivotData.forEachMatchingRecord(filters,
                        function (record) { names.push(record.Name); });
                    alert(names.join("\n"));
                }
            },
            onRefresh: function (config) {
                var config_copy = JSON.parse(JSON.stringify(config));
                ////delete some values which are functions
                //delete config_copy["aggregators"];
                //delete config_copy["renderers"];
                ////delete some bulky default values
                delete config_copy["rendererOptions"];
                delete config_copy["localeStrings"];
                $("#output").text(JSON.stringify(config_copy));
            }

        }, false, "es");
        //  $(".pvtUi").tableExport();

    });

    return false;
})

$(document).ready(function () {
    $("li[data-view]").removeClass('active');
    $("[data-view=ViewPivotTable]").addClass("active");
    $("body").toggleClass("mini-navbar");
    SmoothlyMenu();
    $('form').submit();

});


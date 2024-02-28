



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



function cargarGrafico(datos, axisData, tipo) {
    switch (tipo) {
        case "barras":
            var chart = c3.generate({
                bindto: '#chart',
                data: {
                    columns: datos,
                    type: 'bar'
                },
                grid: {
                    x: {
                        show: true
                    },
                    y: {
                        show: true
                    }
                },
                axis: {
                    x: {
                        type: 'category',
                        categories: axisData,
                        tick: {
                            // this also works for non timeseries data                       
                            format: "%e %b"
                        }
                    },
                    y: {
                        tick: {
                            format: function (d) {

                                return addCommas(d) + $("#SimboloMonedaGlobal").val();//" \u20AC";


                            }
                        }
                    }
                },
                bar: {
                    width: {
                        ratio: 0.9 // this makes bar width 50% of length between ticks
                    }
                    // or
                    //width: 100 // this makes bar width 100px
                }

            });
            break;

        case "lineas":
            var chartMensual = c3.generate({
                bindto: '#chart',
                data: {
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
                        type: 'category',
                        categories: axisData,
                        tick: {
                            // this also works for non timeseries data                       
                            format: "%e %b"
                        }
                    },
                    y: {
                        tick: {
                            format: function (d) {

                                return addCommas(d) + $("#SimboloMonedaGlobal").val();// " \u20AC";


                            }
                        }
                    }
                }

            });
            break;
        case "pie":
           
            var chart = c3.generate({
                bindto: '#chart',
                data: {
                    columns: datos,
                    type: 'pie'
                },
                pie: {
                    label: {
                        format: function (value, ratio, id) {
                            return addCommas(value) +  $("#SimboloMonedaGlobal").val(); //" \u20AC";
                        }
                    }
                }
            });
            break;

        default:
    }
  

}



function CompleteBusqueda() {
    makeBootstrapTable("tblFacturasPorMutua");
}
function printDiv(divName) {
    var printContents = document.getElementById(divName).innerHTML;
    var originalContents = document.body.innerHTML;

    document.body.innerHTML = printContents;
    window.print();
    document.body.innerHTML = originalContents;
    return false;
}

function getGraficoPorcentajeMutua() {
    var datosTotal = [];
    var totalOtras = parseFloat($("#TotalGlobal").val());
    $.map($("#tblFacturasPorMutua").bootstrapTable('getSelections'), function (row) {
        var datos = [];
        datos.push(row.Mutua);
        var valorMutuaActual = +row.Total.substring(0, row.Total.length - 5).replace(/\./g, "")
        datos.push(valorMutuaActual);
        totalOtras = totalOtras - valorMutuaActual;
        datosTotal.push(datos);
    });
    var datosGlobal = [];
    datosGlobal.push("OTRAS");
    datosGlobal.push(totalOtras);
    datosTotal.push(datosGlobal);
    return datosTotal;
}


function getGraficoSelections() {
    var datosTotal = [];
    $.map($("#tblFacturasPorMutua").bootstrapTable('getSelections'), function (row) {
        var datos = [];
        datos.push(row.Mutua);
        datos.push(+row.Enero.substring(0, row.Enero.length - 5).replace(/\./g, ""));
        datos.push(+row.Febrero.substring(0, row.Febrero.length - 5).replace(/\./g, ""));
        datos.push(+row.Marzo.substring(0, row.Marzo.length - 5).replace(/\./g, ""));
        datos.push(+row.Abril.substring(0, row.Abril.length - 5).replace(/\./g, ""));
        datos.push(+row.Mayo.substring(0, row.Mayo.length - 5).replace(/\./g, ""));
        datos.push(+row.Junio.substring(0, row.Junio.length - 5).replace(/\./g, ""));
        datos.push(+row.Julio.substring(0, row.Julio.length - 5).replace(/\./g, ""));
        datos.push(+row.Agosto.substring(0, row.Agosto.length - 5).replace(/\./g, ""));
        datos.push(+row.Septiembre.substring(0, row.Septiembre.length - 5).replace(/\./g, ""));
        datos.push(+row.Octubre.substring(0, row.Octubre.length - 5).replace(/\./g, ""));
        datos.push(+row.Noviembre.substring(0, row.Noviembre.length - 5).replace(/\./g, ""));
        datos.push(+row.Diciembre.substring(0, row.Diciembre.length - 5).replace(/\./g, ""));
        datosTotal.push(datos);
    });
    return datosTotal;
}

$(function () {
    $("li[data-view]").removeClass('active');
    $("[data-view=ViewFacturacionMeses]").addClass("active");

    $("[data-view=ViewFacturacionMeses]").parents("ul").removeClass("collapse");

    $('#modal-grafico').on('shown.bs.modal', function (e) {

        var datos = [];

        if ($(e.relatedTarget).attr("id") === "verGraficoQuesito") {
            datos = getGraficoPorcentajeMutua();
        } else {
            datos = getGraficoSelections();
        }        

        if (datos.length === 0) {
            toastr.error('Debe seleccionar una o varias filas para hacer la Gráfica', 'Info', { timeOut: 5000 });
        } else {
            var meses = [];
            meses.push("Enero");
            meses.push("Febrero");
            meses.push("Marzo");
            meses.push("Abril");
            meses.push("Mayo");
            meses.push("Junio");
            meses.push("Julio");
            meses.push("Agosto");
            meses.push("Septiembre");
            meses.push("Octubre");
            meses.push("Noviembre");
            meses.push("Diciembre");
            var tipoGrafico = "lineas";
            //aqui obtenemos el tipo de grafico en el que ha clicado el usuario para ver
            if ($(e.relatedTarget).attr("id") === "verGraficoBarras") {
                tipoGrafico = "barras";
            }
            if ($(e.relatedTarget).attr("id") === "verGraficoQuesito") {
                tipoGrafico = "pie";
            }
            cargarGrafico(datos, meses, tipoGrafico);
        }
      
    });

    $('#tblFacturasPorMutua').on('uncheck.bs.table', function () {
        $(".ACTIVA").removeClass('ACTIVA');
    });

   

    $(document).ready(function () {

        $("#IOR_ENTIDADPAGADORA").attr('name', 'MutuaList');
        $("#FILTROS_FECHA").attr('name', 'FILTROS_FECHA');
        $('head').append("<style> @media print{ .c3 path, .c3 line { fill: none; stroke: #000; } }</style>");

    });


});
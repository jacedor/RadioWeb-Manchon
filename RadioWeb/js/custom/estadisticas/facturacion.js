

function addCommas(nStr) {
 
    nStr += '';
    x = nStr.split('.');
    x1 = x[0];
    x2 = x.length > 1 ? ',' + x[1] : '';
    var rgx = /(\d+)(\d{3})/;
    while (rgx.test(x1)) {
        x1 = x1.replace(rgx, '$1' + '.' + '$2' );
    }
    return x1 + x2;
}

function loadPorTipoRegimenDeExploracion() {


    //cargamos la lista de espera por grupos
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/GetFacturacionAnualPorICSPRIMUT',
        data: { anyo: new Date().getFullYear().toString() },
        beforeSend: function () {
            $("#spiner-facturacioTipoEntidad").removeClass("hide");
        },
        success: function (data) {
            $("#spiner-facturacioTipoEntidad").addClass("hide");

            var arrayValores = new Array();
            var colores = [];
            colores.push("#a3e1d4");
            colores.push("#46BFBD");
            colores.push("#dedede");
            var color = 0;
            for (var x = 0; x < data.length; x++) {

                var objeto = {};
                objeto.value = data[x].Ventas;
                objeto.color = colores[color];
                objeto.label = data[x].Grupo;
                if (color > 2) {
                    color = 0;
                }
                color = color + 1;
                arrayValores.push(objeto);
            }


            var doughnutOptions = {
                segmentShowStroke: true,
                segmentStrokeColor: "#fff",
                segmentStrokeWidth: 2,
                percentageInnerCutout: 45, // This is 0 for Pie charts
                animationSteps: 100,
                tooltipTemplate: "<%= label %> - <%= addCommas(value) %> \u20AC ",
                animationEasing: "easeOutBounce",
                animateRotate: true,
                animateScale: false,
                responsive: true,
                onAnimationComplete: function () {
                    //this.showTooltip(this.segments, true);

                    //Show tooltips in bar chart (issue: multiple datasets doesnt work http://jsfiddle.net/5gyfykka/14/)
                    // this.showTooltip(this.datasets[0].bars, true);

                    //Show tooltips in line chart (issue: multiple datasets doesnt work http://jsfiddle.net/5gyfykka/14/)
                    //this.showTooltip(this.datasets[0].points, true);  
                }
            };

            var ctx = document.getElementById("donutPorTipoRegimenExploracion").getContext("2d");
            var myNewChart = new Chart(ctx).Doughnut(arrayValores, doughnutOptions);

        }
    });
}


function LoadFacturacionPorGrupos() {

    //cargamos la lista de espera por grupos
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/GetFacturacionAnualPorGrupo',
        data: { anyo: new Date().getFullYear().toString() },
        beforeSend: function () {
            $("#spiner-facturacionMensualGrupo").removeClass("hide");
        },
        success: function (data) {
            $("#spiner-facturacionMensualGrupo").addClass("hide");
            var arrayEtiquetas = new Array();
            var arrayValores = new Array();
            for (var x = 0; x < data.length; x++) {
                arrayEtiquetas.push(data[x].Grupo);
                arrayValores.push(data[x].Ventas);
            }
            var barData = {
                labels: arrayEtiquetas,
                datasets: [
                    {
                        label: "My First dataset",
                        fillColor: "rgba(26,179,148,0.5)",
                        strokeColor: "rgba(26,179,148,0.8)",
                        highlightFill: "rgba(26,179,148,0.75)",
                        highlightStroke: "rgba(26,179,148,1)",
                        data: arrayValores
                    }
                ]
            };

            var barOptions = {
                scaleBeginAtZero: true,
                scaleShowGridLines: true,
                scaleGridLineColor: "rgba(0,0,0,.05)",
                scaleGridLineWidth: 1,
                tooltipTemplate: "<%= addCommas(value) %> \u20AC ",
                barShowStroke: true,
                barStrokeWidth: 2,
                barValueSpacing: 3,
                barDatasetSpacing: 1,
                responsive: true,
                onAnimationComplete: function () {
                    //this.showTooltip(this.segments, true);

                    //Show tooltips in bar chart (issue: multiple datasets doesnt work http://jsfiddle.net/5gyfykka/14/)
                    // this.showTooltip(this.datasets[0].bars, true);

                    //Show tooltips in line chart (issue: multiple datasets doesnt work http://jsfiddle.net/5gyfykka/14/)
                    //this.showTooltip(this.datasets[0].points, true);  
                }
            }

            var ctx = document.getElementById("listaEsperaGrupo").getContext("2d");
            var myNewChart = new Chart(ctx).Bar(barData, barOptions);

        }
    });
}

function LoadFacturacionDiaria() {
    var datosGrupo = new Array();
    var datosCentro = new Array();

    //cargamos la lista de espera por grupos
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/GetFacturacionDeUnDia',
        data: { dia: moment(new Date()).format('MM/DD/YYYY')  },
        beforeSend: function () {
            $("#MediaFacturacion").html('');
          
        },
        success: function (data) {
            if (data.Simbolo === "$") {
                $("#MediaFacturacion").html('$ ' + addCommas(data.Ventas) );
            } else {
                $("#MediaFacturacion").html(addCommas(data.Ventas) + $("#SimboloMonedaGlobal").val()); //"\u20AC");
            }
          
        }
           
    });

}
function formatNumber(number, decimalsLength, decimalSeparator, thousandSeparator) {
    var n = number,
        decimalsLength = isNaN(decimalsLength = Math.abs(decimalsLength)) ? 2 : decimalsLength,
        decimalSeparator = decimalSeparator == undefined ? "," : decimalSeparator,
        thousandSeparator = thousandSeparator == undefined ? "." : thousandSeparator,
        sign = n < 0 ? "-" : "",
        i = parseInt(n = Math.abs(+n || 0).toFixed(decimalsLength)) + "",
        j = (j = i.length) > 3 ? j % 3 : 0;

    return sign +
        (j ? i.substr(0, j) + thousandSeparator : "") +
        i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thousandSeparator) +
        (decimalsLength ? decimalSeparator + Math.abs(n - i).toFixed(decimalsLength).slice(2) : "");
}

function LoadFacturacionPorMeses() {
    var datosGrupo = new Array();
    var datosCentro = new Array();

    //cargamos la lista de espera por grupos
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/GetFacturacionAnualPorMeses',
        data: { anyo: new Date().getFullYear().toString() },
        beforeSend: function () {
            $("#facturacionPorMeses").addClass("hide");
            $("#spiner-facturacionMensual").removeClass("hide");
        },
        success: function (data) {
            $("#spiner-facturacionMensual").addClass("hide");
            var arrayEtiquetas = new Array();
            var arrayValores = new Array();
            var arrayValoresAnyoAnterior = new Array();
            var anyoActual = new Date().getFullYear().toString();
            for (var x = 0; x < data.length; x++) {
                if (data[x].Anyo === anyoActual) {
                    if ($("#CuentaFacturacionMensual").is(":checked")) {
                        arrayValores.push(data[x].Cuenta);

                    } else {
                        arrayValores.push(data[x].Ventas);
                    }
                } else {
                    if ($("#CuentaFacturacionMensual").is(":checked")) {
                        arrayValoresAnyoAnterior.push(data[x].Cuenta);
                    } else {
                        arrayValoresAnyoAnterior.push(data[x].Ventas);
                    }
                    arrayEtiquetas.push(data[x].Mes);
                }

            }

            
            var lineData = {
                labels: arrayEtiquetas,
                datasets: [
                    {
                        label: "Actual",
                        fillColor: "rgba(26,179,148,0.5)",
                        strokeColor: "rgba(26,179,148,0.7)",
                        pointColor: "rgba(26,179,148,1)",
                        pointStrokeColor: "#fff",
                        pointHighlightFill: "#fff",
                        pointHighlightStroke: "rgba(26,179,148,1)",
                        data: arrayValores
                    },
                {
                    label: "Anterior",
                    fillColor: "rgba(220,220,220,0.5)",
                    strokeColor: "rgba(220,220,220,1)",
                    pointColor: "rgba(220,220,220,1)",
                    pointStrokeColor: "#fff",
                    pointHighlightFill: "#fff",
                    pointHighlightStroke: "rgba(220,220,220,1)",
                    data: arrayValoresAnyoAnterior
                }
                ]
            };

            var lineOptions = {
                scaleLabel:
                        function (label)
                        {
                            if ($("#CuentaFacturacionMensual").is(":checked")) {
                                return label.value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",") ;
                            } else {
                                return label.value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".") + $("#SimboloMonedaGlobal").val();// '\u20AC';
                            }
                            
                        },
                scaleShowGridLines: true,
                scaleGridLineColor: "rgba(0,0,0,.05)",
                scaleGridLineWidth: 1,
              
                multiTooltipTemplate: function (label) {
                    
                    if ($("#CuentaFacturacionMensual").is(":checked")) {
                        return label.datasetLabel + ': ' + label.value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".") ;
                    } else {

                        return label.datasetLabel + ': ' + label.value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".") + $("#SimboloMonedaGlobal").val();// 'fas\u20AC';
                    }
                    
                } ,
                bezierCurve: true,
                bezierCurveTension: 0.4,
                pointDot: true,
                pointDotRadius: 4,
                pointDotStrokeWidth: 1,
                pointHitDetectionRadius: 20,
                datasetStroke: true,
                datasetStrokeWidth: 2,
                datasetFill: true,
                responsive: true,


            };

            $("#facturacionPorMeses").removeClass("hide");
            $('#DivChartContainer').empty();
            $('#DivChartContainer').append('<canvas id="facturacionPorMeses" height="70"></canvas>');
            var ctxFacturacionAnual = document.getElementById("facturacionPorMeses").getContext("2d");
            var myLineChartFacturacionAnual = new Chart(ctxFacturacionAnual).Line(lineData, lineOptions);


            // The values array passed into addData should be one for each dataset in the chart


        }
    });

}

function myfilter(array, dia) {
    var passedTest = [];
    var chivatoPrivado = false;
    var chivatoMutua = false;
    for (var i = 0; i < array.length; i++) {
        if (array[i].Mes === dia.toString()) {
            var obj = { label: array[i].Mes, y: array[i].Ventas, Grupo: array[i].Grupo };
            passedTest.push(obj);
            if (array[i].Grupo==="privado") {
                chivatoPrivado = true;
            }
            if (array[i].Grupo === "mutuas") {
                chivatoMutua = true;
            }
        }
        
    }
    if (!chivatoMutua && !chivatoPrivado) {
        return passedTest;
    }
    if (!chivatoPrivado  ) {
        var obj2 = { label: dia, y: "0", Grupo: "1" };
        passedTest.push(obj2);

    }
    if (!chivatoMutua) {
        var obj3 = { label: dia, y: "0", Grupo: "2" };
        passedTest.push(obj3);

    }
    
    return passedTest;
}

function LoadFacturacionSemanal() {
    

    //cargamos la lista de espera por grupos
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/GetFacturacionSemanas',       
        beforeSend: function () {
            $("#spiner-facturacionSemanal").removeClass("hide");
            $("#facturacionSemanal").addClass('hide');
        },
        success: function (data) {
            $("#spiner-facturacionSemanal").addClass("hide");
            var arrayAnyoActual = new Array();
            var arrayAnyoAnterior = new Array();

            var anyoActual = moment(new Date()).format('YYYY');
            var anyoAnterior = +anyoActual - 1;
           
            var i= 0;
            while (i < data.length -1 ) {               
                var item= data[i];
                var obj = { x: item.Grupo, y: item.Ventas, label: item.Grupo + ' Sem: ' + item.Grupo };
               
                if (item.Anyo.toString()===anyoActual) {
                    arrayAnyoActual.push(obj);
                    }else{
                        arrayAnyoAnterior.push(obj);
                    }
                    
                    i++;
             
            }

            var chart = new CanvasJS.Chart("facturacionSemanal",
             {
                 
                 animationEnabled: true,
                 axisX:{
                     maximum: 53,
                 },
                 toolTip: {
                     shared: true
                 },
                 theme: "theme2",
                 axisY: {
                     gridColor: "Silver",
                     tickColor: "silver"
                 },
                 legend: {
                     verticalAlign: "center",
                     horizontalAlign: "right"
                 },
                 data: [
                 {
                     type: "line",
                     showInLegend: true,
                     lineThickness: 2,
                     name: "Actual",
                     markerType: "square",
                     color: "#20B2AA",
                     dataPoints: arrayAnyoActual
                 },
                 {
                     type: "line",
                     showInLegend: true,
                     name: "Anterior",
                    
                     color: "#F08080",
                     lineThickness: 2,
                     dataPoints: arrayAnyoAnterior
                 }
                 ]
             });
                $("#facturacionSemanal").removeClass('hide');
                chart.render();           
                
        }          

    
        });
    }
    



function LoadFacturacionDiariaDeUnMes(start, end) {
    $('#facturacionMensualDiasContainer').empty();

    //cargamos la lista de espera por grupos
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/GetFacturacionDiariaDeUnMes',
        data: { start: start, end: end },
        beforeSend: function () {
            $("#spiner-facturacionMensualDiaria").removeClass("hide");
            $("#chartContainer").addClass('hide');
        },
        success: function (data) {
            $("#spiner-facturacionMensualDiaria").addClass("hide");
            var arrayPrivados = [];
            var arrayMutua = [];
            var arrayICS = [];

            var momentInicial = moment(start,'DD/MM/YYYY');
            var momentFinal = moment(end, 'DD/MM/YYYY');

            var diasDiferencia = momentFinal.diff(momentInicial, 'days') + 1 ;
           
           
            var diaInicial =start.substring(0,2);
            var diaFinal = end.substring(0, 2);

            //for (var x = diaInicial; x <= diaFinal; x++) {
            
            while (momentFinal.diff(momentInicial, 'days') + 1 !== 0) {
                
                myfilter(data, parseInt(momentInicial.format('DD'))).forEach(function (item) {
                    var obj = { label: momentInicial.format('DD'), y: item.y };
                    if (item.Grupo === "privados") {
                        arrayPrivados.push(obj);
                    }
                    if (item.Grupo === "mutua") {
                        arrayMutua.push(obj);
                    }
                    if (item.Grupo === "3") {
                        arrayICS.push(obj);

                    }
                });

               


                var chart = new CanvasJS.Chart("chartContainer",
                {
                    
                    axisY: {
                        valueFormatString: "#.##",
                    },
                    data: [
                    {
                        type: "stackedColumn",
                        legendText: "Privado",
                        showInLegend: "true",
                        dataPoints: arrayPrivados
                    },
                    {
                        type: "stackedColumn",
                        legendText: "Mutua",
                        showInLegend: "true",
                        indexLabel: "#total ",
                        yValueFormatString: "#,##0",
                        indexLabelPlacement: "outside",
                        dataPoints: arrayMutua
                    }//,
                    //{
                    //    type: "stackedColumn",
                    //    legendText: "ICS",
                    //    showInLegend: "true",

                    //    yValueFormatString: "#,##0",
                    //    indexLabelPlacement: "outside",
                    //    dataPoints: arrayICS
                    //}
                    ]
                });

                
                $("#chartContainer").removeClass('hide');
                chart.render();


                momentInicial = momentInicial.add(1, 'days');
                
            }

            

        }
    });
}

$('#CuentaFacturacionMensual,#SumaFacturacionMensual').change(function () {
    LoadFacturacionPorMeses();
});

$(document).on('changeDate', '#data_5 .input-daterange #end', function (ev) {

        var newDate = new Date(ev.date);
   
    LoadFacturacionDiariaDeUnMes($("#start").val(), $("#end").val());

        return false;

});

$(document).on('click', '#facturacionDiariaAnyoAnterior', function (ev) {

  
    ////var nuevaFechaInicial = moment($("#start").val()).subtract(1, 'years').format('DD/MM/YYYY');

    //var anyoInicial = moment($("#start").val()).year - 1;
    //var nuevaFechaInicial = moment($("#start").val()).subtract(1, 'years').format('DD/MM/') + anyoInicial;

    //var anyoFinal = moment($("#end").val()).year - 1;
    //var nuevaFechaInicial = moment($("#end").val()).subtract(1, 'years').format('DD/MM/') + anyoFinal;
    
    //$("#start").val(nuevaFechaInicial);
    //$("#end").val(nuevaFechaFinal);
    //return false;

});




$(function () {
    $("li[data-view]").removeClass('active');
    $("[data-view=ViewFacturacion]").addClass("active");

    setTimeout(   LoadFacturacionDiaria(), 5000);
 
    $("[data-view=ViewFacturacion]").parents("ul").removeClass("collapse");
   LoadFacturacionPorMeses();
    LoadFacturacionPorGrupos();
    loadPorTipoRegimenDeExploracion();

    //LoadFacturacionSemanal();
    //Esconde el menu del a izquierda
    SmoothlyMenu();
    
});
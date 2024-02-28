function LoadRecuentoPoraTipoExploracion(aparato) {

    $.ajax({
        type: 'POST',
        url: '/Estadisticas/GetListaDeEsperaDeAparato',
        data: { Aparato: aparato},
        success: function (data) {
            $('#containerListaExploraciones').html(data);
        }
    });
}
function LoadRecuentoPorAparato(Centro, Grupo) {

    $.ajax({
        type: 'POST',        
        url: '/Estadisticas/GetListaDeEsperaDeUnCentroYGrupo',
        data: { centro: Centro, grupo: Grupo },
        success: function (data) {
            $('#containerListaAparato').html(data);
            LoadRecuentoPoraTipoExploracion($(".botonAparato:first").data('aparato'));
        }
    });
}
//primera lista de la izquierda
function LoadRecuentoCentroLista(Centro) {

    $.ajax({
        type: 'POST',
        url: '/Estadisticas/GetListaDeEsperaDeUnCentroList',
        data: { centro: Centro },
        success: function (data) {
            $('#containerListaGrupos').html(data);
        }
    });
}

function LoadRecuentoPorCentro(Centro) {

    //cargamos la lista de espera por aparats
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/GetListaDeEsperaDeUnCentro',
        data: { centro: Centro },
        success: function (data) {

            var doughnutOptions = {
                segmentShowStroke: true,
                segmentStrokeColor: "#fff",
                segmentStrokeWidth: 2,
                percentageInnerCutout: 0, // This is 0 for Pie charts
                animationSteps: 100,
                animationEasing: "easeOutBounce",
                animateRotate: true,
                animateScale: false

            };


            var arrayValores = new Array();
            var doughnutData = [];


            for (var y = 0; y < data.length; y++) {

                var objeto = {};
                objeto.value = data[y].Valor;
                objeto.label = data[y].Grupo;
                objeto.color = data[y].Color;
                objeto.labelColor = 'black';
                objeto.labelFontSize = '16';
                doughnutData.push(objeto);

            }


            var currentChart = "doughnutCentro";
            $('#DivChartContainer').empty();
            $('#DivChartContainer').append('<canvas id="doughnutCentro" height="200" width="200"></canvas>');
            var ctx = document.getElementById(currentChart).getContext("2d");
            var myNewChart = new Chart(ctx).Doughnut(doughnutData, doughnutOptions);


            $("#doughnutCentro").click(
                     function (evt) {
                         var activePoints = myNewChart.getSegmentsAtEvent(evt);
                         LoadRecuentoCentroLista($(".centrosListaEspera .active").data('val'));
                         LoadRecuentoPorAparato($(".centrosListaEspera .active").data('val'), activePoints[0].label);

                     }
                );



        }
    });

}

//Carga el grafico de barras
function LoadListasDeEspera() {

    //cargamos la lista de espera por grupos
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/GetListaEsperaPorGrupos',
        success: function (data) {
            var arrayEtiquetas = new Array();
            var arrayValores = new Array();
            for (var x = 0; x < data.length; x++) {
                arrayEtiquetas.push(data[x].Grupo);
                arrayValores.push(data[x].Valor);
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
                barShowStroke: true,
                barStrokeWidth: 1,
                barValueSpacing: 1,
                barDatasetSpacing: 1,
                responsive: true,
                showLabelsOnBars: true,
                barLabelFontColor: "gray",
                showTooltips: false,
                showInlineValues: true,
                centeredInllineValues: true,
                tooltipCaretSize: 0,
                tooltipTemplate: "<%= value %>"
            }

            var ctx = document.getElementById("listaEsperaGrupo").getContext("2d");
            var myNewChart = new Chart(ctx).Bar(barData, barOptions);

        }
    });


    //cargamos la lista de espera por centros
    $.ajax({
        type: 'POST',
        url: '/Estadisticas/GetListaEsperaPorCentros',
        success: function (data) {
            var arrayEtiquetas = new Array();
            var arrayValores = new Array();
            var valorMaximo = 0;
            for (var x = 0; x < data.length; x++) {
                arrayEtiquetas.push(data[x].Centro);
                arrayValores.push(data[x].Valor);
                if (valorMaximo < data[x].Valor) {
                    valorMaximo = data[x].Valor;
                }

            }
            var barData = {
                labels: arrayEtiquetas,
                datasets: [
                    {
                        label: "My First dataset",
                        fillColor: "rgba(0,255,0,0.5)",
                        strokeColor: "rgba(0,255,0,0.8)",
                        highlightFill: "rgba(0,255,0,0.75)",
                        highlightStroke: "rgba(0,255,0,1)",
                        data: arrayValores
                    }
                ]
            };

            var barOptions = {
                scaleBeginAtZero: true,
                scaleShowGridLines: true,
                scaleGridLineColor: "rgba(0,0,0,.05)",
                scaleGridLineWidth: 1,
                barShowStroke: true,
                barStrokeWidth: 1,
                barValueSpacing: 3,
                barDatasetSpacing: 1,
                responsive: true,
                showLabelsOnBars: true,
                barLabelFontColor: "gray",
                showLabelsOnBars: true,
                barLabelFontColor: "gray",
                showTooltips: false,
                showInlineValues: true,
                centeredInllineValues: true,
                tooltipCaretSize: 0,
                tooltipTemplate: "<%= value %>",
                scaleOverride: true,
                scaleSteps: 1,
                scaleStepWidth: valorMaximo,
                scaleStartValue: 0
            }

            var ctx = document.getElementById("listaEsperaCentros").getContext("2d");
            var myNewChart = new Chart(ctx).Bar(barData, barOptions);
        }
    });



    

}

$(document).on('click', '.centrosListaEspera .btn', function myfunction() {
    $(this).siblings().removeClass('active');
    $(this).toggleClass('active');
   //Rellena la lista de al lado del Quesito de cuenta por grupo
    LoadRecuentoCentroLista($(".centrosListaEspera .active").data('val'));
    LoadRecuentoPorCentro($(".centrosListaEspera .active").data('val'));
    LoadRecuentoPorAparato($(".centrosListaEspera .active").data('val'), "RM");
   
});

$(document).on('click', '[data-aparato]', function myfunction() {
    LoadRecuentoPoraTipoExploracion($(this).data('aparato'));
});

$(document).on('click', '[data-grupo]', function myfunction() {
    LoadRecuentoPorAparato($(".centrosListaEspera .active").data('val'), $(this).data('grupo'));
  


});

    $(function () {
        $("li[data-view]").removeClass('active');
        $("[data-view=ViewListasDeEspera]").addClass("active");
        $("[data-view=ViewListasDeEspera]").parents("ul").removeClass("collapse");
        //Cargamos los graficos de barras
        LoadListasDeEspera();
        
        //Rellena el Quesito de cuenta por grupo
        LoadRecuentoPorCentro($(".centrosListaEspera .active").data('val'));

        //Rellena la lista de al lado del Quesito de cuenta por grupo
        LoadRecuentoCentroLista($(".centrosListaEspera .active").data('val'));
        //al cargar la pagina carga el centro y el grupo RM
        LoadRecuentoPorAparato($(".centrosListaEspera .active").data('val'), "RM");

        
    });


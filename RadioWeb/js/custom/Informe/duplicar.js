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




$(document).on('change', '#oidPlantilla', function myfunction() {
    
    var oidPersonal = $(this).val();
    var oidExploracion = $("#oidExploracion").val();
    //var ContenedorModalImagenes = $('#contenedorModalImagenes');
    var ContenedorPlantillas = $('#PlantillaListContainer');
    $.ajax({
        type: 'POST',
        url: '/P_Informe/Index',
        data: {
            oidPersonal: oidPersonal,
            oidExploracion: oidExploracion
        },
        beforeSend: function () {
            ContenedorPlantillas.empty();            
        },
        success: function (data) {           
            ContenedorPlantillas.html(data);
            makeBootstrapTable('PlantillaList');
            
        }
    });

});


$(document).on('click', '.previsualizarInforme', function myfunction() {
    var oidInforme = $(this).data('oid');
    //var ContenedorModalImagenes = $('#contenedorModalImagenes');
    var ContenedorModalPlantilla = $('#contenedorPlantilla');
    $.ajax({
        type: 'POST',
        url: '/Informe/Previsualizar/' + oidInforme,
      
        beforeSend: function () {

            $(".spiner-cargando").removeClass('hide');
            ContenedorModalPlantilla.html('');
            $('#modal-form-previsualizar').modal('show');
        },
        success: function (data) {
            $(".spiner-cargando").addClass('hide');
            ContenedorModalPlantilla.html(data);

        }
    });

});

$(document).on('click', '.previsualizar', function myfunction() {
    var oidPlantilla = $(this).data('oid');
    //var ContenedorModalImagenes = $('#contenedorModalImagenes');
    var ContenedorModalPlantilla = $('#contenedorPlantilla');
    $.ajax({
        type: 'POST',
        url: '/P_Informe/PrevisualizarPlantilla',
        data: { oidPlantilla: oidPlantilla },
        beforeSend: function () {

            $(".spiner-cargando").removeClass('hide');
            ContenedorModalPlantilla.html('');
            $('#modal-form-previsualizar').modal('show');
        },
        success: function (data) {
            $(".spiner-cargando").addClass('hide');
            ContenedorModalPlantilla.html(data);

        }
    });

});



$(function () {
    $("li[data-view]").removeClass('active');
    $("[data-view=ViewInformesPendientes]").addClass("active");

    moment.locale('es-ES');
    


});
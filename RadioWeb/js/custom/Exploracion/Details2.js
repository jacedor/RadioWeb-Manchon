

$(document).on("shown.bs.tab", "a[data-toggle='tab']", function (e) {

    var target = $(e.target).attr("href") // activated tab
    switch (target) {
        case "#tab-vidSigner":
            var filaActiva = $("#ExploracionesTable* .ACTIVA");
            var oidExploracion = filaActiva.data('oid');
            var ContenedorModalFirmar = $('#modalVidContentEntrada');
            $.ajax({
                type: 'POST',
                url: '/VidSigner/ListaPartial',
                data: {
                    oid: oidExploracion,
                    esEntrada: 'true'
                },
                beforeSend: function () {
                    $(".spiner-cargando").removeClass('hide');
                    ContenedorModalFirmar.html('');
                },
                success: function (data) {
                    $(".spiner-cargando").addClass('hide');

                    ContenedorModalFirmar.html(data);
                    if (localStorage.nombreTablet) {
                        $("#TABLETA_NAME").val(localStorage.nombreTablet);
                    }
                    $('.i-checks').iCheck({
                        checkboxClass: 'icheckbox_square-green',
                        radioClass: 'iradio_square-green',
                    });
                    $(".select2").select2({
                        theme: "bootstrap"
                    }
                    );
                }
            });
            break;
        case "#tab-imagenes":
            var filaActiva = $("#ExploracionesTable* .ACTIVA");
            var oidExploracionSeleccionada = filaActiva.data('oid');
            var ContenedorModalImagenes = $('#ContenedorSliderImagenes');
            $.ajax({
                type: 'POST',
                url: '/Imagenes/ListaPartialImagenes',
                data: { oid: oidExploracionSeleccionada },
                beforeSend: function () {
                    ContenedorModalImagenes.html('');
                },
                success: function (data) {
                    ContenedorModalImagenes.html(data);
                }, complete: function () {
                    $('.slick_demo_3').slick({
                        infinite: true,
                        speed: 500,
                        fade: true,
                        cssEase: 'linear',
                        adaptiveHeight: true
                    });
                    $(".slick-list").css("height", "360px");
                }
            });
            break;
        case "#tab-documentos":
           
            var oidPaciente = $("#IOR_PACIENTE").val();
            alert(oidPaciente);
            var url = "/Documentos/ListaPaciente?oid=" + oidPaciente; //Url con la vista parcial que agregamos a la ventana modal

            $.ajax({
                type: 'GET',
                url: url,
                dataType: "html",
                beforeSend: function () {
                    $(".spiner-cargando-documentos-paciente").removeClass('hide');
                },
                success: function (evt) {
                    $(".spiner-cargando-documentos-paciente").addClass('hide');
                    $('#documentosPaciente').html(evt);

                },
            });

            break;  
        case "#tab-informes":
            var oidPaciente = $("#IOR_PACIENTE").val();;
            var url = "/Informe/ListaPaciente?oidPaciente=" + oidPaciente; //Url con la vista parcial que agregamos a la ventana modal

            $.ajax({
                type: 'GET',
                url: url,
                dataType: "html",
                beforeSend: function () {
                    $(".spiner-cargando-informes-paciente").removeClass('hide');
                },
                success: function (evt) {
                    $(".spiner-cargando-informes-paciente").addClass('hide');
                    $('#informesPaciente').html(evt);

                },
            });

            break;

        case "#tab-exploraciones":           
            var oidPaciente = $("#IOR_PACIENTE").val();;
            var url = "/Exploracion/ListaExploracionesPaciente?oid=" + oidPaciente; //Url con la vista parcial que agregamos a la ventana modal
            
            $.ajax({
                type: 'GET',
                url: url,               
                dataType: "html",
                beforeSend: function () {
                    $(".spiner-cargando-documentos-paciente").removeClass('hide');
                },
                success: function (evt) {
                    $(".spiner-cargando-documentos-paciente").addClass('hide');
                    $('#documentosPaciente').html(evt);

                },
            });

            break;
        default:

    }
});













$(document).ready(function () {
 
    $("li[data-view]").removeClass('active');
    $("[data-view=Exploracion]").parents("ul").removeClass("collapse");
    $("[data-view=Exploracion]").parents("li").addClass('active');
    $("[data-view=Exploracion]").addClass("active");
    

  



});


$(document).on('change keyup', '.posiblesRespuestas', function () {
    //Evento para cambiar el tipo de una pregunta

  
});

$(document).on('click', '.editarPregunta', function () {
    //Evento para cambiar el tipo de una pregunta
    var oidPregunta = $(this).data('oid');
    var contenedorEditPregunta = $('#contenedorEditarPregunta');
    $.ajax({
        type: 'GET',
        url: '/Formulario/EditPregunta',
        data: { oid: oidPregunta },
        beforeSend: function () {
            $(".spiner-cargando").removeClass('hide');
            contenedorEditPregunta.html('');
            $('#modal-form-pregunta').modal('show');
        },
        success: function (data) {
            $(".spiner-cargando").addClass('hide');
            contenedorEditPregunta.html(data);
            $('.chosen-select').chosen({ width: "100%" });
        }
    });

});




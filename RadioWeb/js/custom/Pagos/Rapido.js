
function LoadPagosConsumible(oidValue) {
    $.ajax({
        type: 'GET',
        url: '/Pagos/LoadPagos',
        data: { oid: oidValue },
        beforeSend: function () {
            $('.pagosConsumiblesContainer').html("");
        },
        success: function (data) {
            $('.pagosConsumiblesContainer').html(data);
          
        }
    });


}

function LoadConsumibles(oidValue) {
    $.ajax({
        type: 'GET',
        url: '/Pagos/LoadConsumibles',
        data: { oid: oidValue },
        beforeSend: function () {
            $(".consumiblesEmpty").addClass("hide");
            $(".consumiblesContainer").removeClass("hide");
            $('.consumiblesContainer').html("")
        },
        success: function (data) {
            $('.consumiblesContainer').html(data);
            if ($(data).find("tbody").find("tr").length > 0) {
                LoadPagosConsumible($("#ConsumiblesList tbody tr:first").data("oid"));
            } else {
                $(".consumiblesEmpty").removeClass("hide");
                $(".consumiblesContainer").addClass("hide");
                

            }
            
            
        }
    });


}

function Pagar(idPago, idExploracion,fechaExploracion, formaPago,cantidad,pagoRapido) {
    $.ajax({
        type: 'POST',
        url: '/Pagos/PagoRapido',
        data: {
            oidPago: idPago,
            oidExploracion: idExploracion,
            fechaExploracion: fechaExploracion,
            cantidad: cantidad,
            formaPago: formaPago,
            pagoRapido: pagoRapido
        },
        beforeSend: function () {
           
        },
        success: function (data) {
            toastr.success('', 'Pago realizado!', { timeOut: 5000 });
            location.reload();
          
        }
    });
}

function PagarConsumible(idPago, idConsumible, fechaExploracion, formaPago, cantidad, pagoRapido) {
    $.ajax({
        type: 'POST',
        url: '/Pagos/PagarConsumible',
        data: {
            oidPago: idPago,
            oidConsumible: idConsumible,
            fechaExploracion: fechaExploracion,
            cantidad: cantidad,
            formaPago: formaPago,
            pagoRapido: pagoRapido
        },
        beforeSend: function () {

        },
        success: function (data) {
            toastr.success('', 'Pago realizado!', { timeOut: 5000 });
            location.reload();

        }
    });
}

$(document).on('click', '.pagarConsumible', function () {

    var idPago = $(this).data('idpago');
    var idConsumible = $(this).data('idconsumible');
    var idPaciente = $(this).data('iorpaciente');
    var fecha = $(this).data('fecha');
    var tipoPago = $("#ddlTipoPago" + $(this).data('idpago')).val();
    var cantidad = $("#" + $(this).data('idpago')).val().replace("€", "").replace(" ", "");

    PagarConsumible(idPago, idConsumible, fecha, tipoPago, cantidad, false);

});

$(document).on('click', '.pagarExploracion', function () {
    
    var idPago = $(this).data('idpago');
    var idExploracion = $(this).data('idexploracion');
    var idPaciente = $(this).data('iorpaciente');
    var fecha = $(this).data('fecha');
    var tipoPago = $("#ddlTipoPago" + $(this).data('idpago')).val();
    var cantidad = $("#" + $(this).data('idpago')).val().replace("€","").replace(" ","") ;
    
    Pagar(idPago, idExploracion, fecha, tipoPago, cantidad, false);

});


$(document).on('click', '.copiarDeuda', function () {
    
    var inputCantidad = $("#" + $(this).data('idinput'));    
    inputCantidad.val($(this).data('deuda').toFixed(2).replace('.', ',') + '€');
    inputCantidad.blur();
    inputCantidad.inputmask({ mask: "*99,99€", placeholder: "" });
});





$(function () {    

    $('.pagoRapido').click(function () {
        swal({
            title: "Pago Rápido",
            text: "Confirme pagar con fecha de hoy las exploraciones y consumibles marcados del paciente",
            type: "warning",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55",
            confirmButtonText: "Si, pagar marcado con fecha de hoy!",
            closeOnConfirm: false
        }, function () {
        
                //var idPago = $(this).data('idpago');
                //var idExploracion = $(this).data('idexploracion');
                //var idPaciente = $(this).data('iorpaciente');
                //var fecha = $(this).data('fecha');
                //var tipoPago = $("#ddlTipoPago" + $(this).data('idpago')).val();
                //var cantidad = $("#" + $(this).data('idpago')).val().replace("€", "").replace(" ", "");

                //Pagar(idPago, idExploracion, fecha, tipoPago, cantidad, true);
            swal("Pagado!", "Las exploraciones y consumibles han sido pagados", "success");
        });
    });
});
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

function LoadPagos(oidValue) {
    $.ajax({
        type: 'GET',
        url: '/Pagos/LoadPagos',
        data: { oid: oidValue },
        beforeSend: function () {
            $('.pagosContainer').html("");
            $('.pagosConsumiblesContainer').html("");
        },
        success: function (data) {
            $('.pagosContainer').html(data);
            //LoadConsumibles(oidValue);
        }
    });


}

$(document).on('click', ' #ExploracionesList tbody tr', function () {
    $(this).siblings().removeClass('ACTIVA');
    $(this).addClass('ACTIVA');
    LoadPagos($(this).data("oid"));



});

$(document).on('click', ' #ConsumiblesList tbody tr', function () {
    $(this).siblings().removeClass('ACTIVA');
    $(this).addClass('ACTIVA');
    LoadPagosConsumible($(this).data("oid"));



});


$(function () {
    
    LoadPagos($("#OID").val());
    
});
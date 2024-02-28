var edit = function () {

    $(".EditButton").addClass("disabled");
    $(".SaveButton").removeClass("disabled");
    $('.click2edit').summernote({ focus: true, height: $(".click2edit").height() });
    $(".note-editor").height($(".click2edit").height());

};

var save = function () {
    var aHTML = $('.click2edit').code(); //save HTML If you need(aHTML: array).
    var oidInforme = $(".SaveButton").data('oid');
    var informe = { OID: oidInforme, TEXTOHTML: aHTML }
    var url = "/Informe/Guardar/"; //The Url to the Action  Method of the Controller
    $.ajax({
        type: 'POST',
        url: url,
        data: informe,
        dataType: "html",
        success: function () {
            
            location.reload();
            $(".EditButton").removeClass("disabled");
            $(".SaveButton").addClass("disabled");
            $('.click2edit').destroy();
        },
    });
  
};



var imprimir = function () {
    
    var oidInforme = $(".ImprimirButton").data('oid');    
    var url = "/Informe/Imprimir/" + oidInforme; //The Url to the Action  Method of the Controller
    window.open(url, 'popup', 'width=900,height=500');
    return false;
   
};



$(document).on("click", "#AplicarPlantilla", function myfunction() {

    $('#InformeContent').html($('#contenidoPlantilla').html());
    $("#modalPlantilla").modal('hide');
    edit();
    return false;
   
});




$('#modalPlantilla').on('shown.bs.modal', function (e) {

});

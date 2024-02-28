

$(document).ready(function () {
    
   
    

    var oidQueryString = $.urlParam('oid');
    sessionStorage.oidInformeActual = oidQueryString;
    //$.ajax({
    //    type: 'POST',
    //    url: '/Informes/GetInforme',
    //    data: { oidInforme: oidQueryString },
    //    async: 'false',
    //    beforeSend: function () {

    //    },
    //    success: function (data) {
    //        if (data.length > 0) {

    //            if (data == "ACCESO DENEGADO") {
    //                $.growl.error({ title: "No tiene permisos para ver informes!", message: "" });
                    
    //            }

    //            else {
    //                CKEDITOR.instances.editor1.setData(data);
                    
    //            }
               
                


    //        }

    //    }
    //});
    
});

$(document).on('click', '#GuardarInforme', function () {
    
    var encodedHTML = escape($("#editor").html());
    var informe = {
        OWNER:sessionStorage.oidInformeActual,
        TEXTO: $("#editor").html()
    };
    var options = {
        url: "/Informes/Add",       
        type: 'POST',
        cache: false,
        data: JSON.stringify(informe),
        
        contentType: 'application/json; charset=utf-8'
    };

    $.ajax(options).done(function (data) {
        

        $.growl.notice({ title: "Informe", message: "Informe Guardado" });
      

    });




});
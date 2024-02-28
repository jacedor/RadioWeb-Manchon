var submitAutocompleteForm = function (event, ui) {
    $("#CP").val(ui.item.desc);
    $("#PROVINCIA").val(ui.item.icon);
    $("#PAIS").focus();
    return false;
};






$(document).ready(function () {



    $("#POBLACION").autocomplete({
            data: $(this).val,
            source: '/Direccion/PueblosList',
            focus: function (event, ui) {
                $(this).val(ui.item.label);
                return false;
            },
            select: submitAutocompleteForm
              
    });

    $("#POBLACION").autocomplete("option", "appendTo", "#PoblacionContainer");

});

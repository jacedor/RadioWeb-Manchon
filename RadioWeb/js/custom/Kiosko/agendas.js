$(document).ready(function () {

    //Prevent intro submit on inputs/forms
    $('form input').keypress(function (e) {
        var charCode = e.charCode || e.keyCode || e.which;
        if (charCode == 13) {
            return false;
        }
    });

    var itemContainers = [].slice.call(document.querySelectorAll('.board-column-content'));
    var columnGrids = [];
    var boardGrid;

    itemContainers.forEach(function (container) {

        var grid = new Muuri(container, {
            items: '.board-item',
            layoutDuration: 400,
            layoutEasing: 'ease',
            dragEnabled: true,
            layoutOnResize: true,
            dragSort: function () {
                return columnGrids;
            },
            dragCssProps: {
                touchAction: 'pan-y',
                userSelect: '',
                userDrag: '',
                tapHighlightColor: '',
                touchCallout: '',
                contentZooming: ''
            },
            dragSortInterval: 0,
            dragContainer: document.body,
            dragReleaseDuration: 400,
            dragReleaseEasing: 'ease'
        })
            .on('dragStart', function (item) {
                item.getElement().style.width = item.getWidth() + 'px';
                item.getElement().style.height = item.getHeight() + 'px';
                columnGrids.forEach(function (grid) {
                    grid.refreshItems();
                });
            })
            .on('dragReleaseEnd', function (item) {
                item.getElement().style.width = '';
                item.getElement().style.height = '';
                columnGrids.forEach(function (grid) {
                    grid.refreshItems();
                });

                var daparatoId = item.getElement().getAttribute('data-id');
                var tvId = grid._element.id;


                $.ajax({
                    type: "POST",
                    url: '/kiosko/agendaUpdate',
                    data: {
                        'tvId': tvId,
                        'daparatoId': daparatoId
                    },
                    dataType: "JSON",
                    success: function (data) {
                        console.log('Sala actualizada correctamente.');
                    }
                });

            })
            .on('layoutStart', function () {
                boardGrid.refreshItems().layout();
            });
        columnGrids.push(grid);

    });

    boardGrid = new Muuri('.board', {
        layoutDuration: 400,
        layoutEasing: 'ease',
        dragEnabled: true,
        dragSortInterval: 0,
        dragStartPredicate: {
            handle: '.board-column-header'
        },
        dragReleaseDuration: 400,
        dragReleaseEasing: 'ease'
    });

});
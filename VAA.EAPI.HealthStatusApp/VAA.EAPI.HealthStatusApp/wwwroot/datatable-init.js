function initializeDataTable(tableId, childData) {
    var table = $('#' + tableId).DataTable({
        "columns": [
            {
                "className": 'details-control',
                "orderable": false,
                "data": null,
                "defaultContent": ''
            },
            // Add your column definitions here
        ],
        "order": [[1, 'asc']]
    });

    // Add event listener for opening and closing details
    $('#' + tableId + ' tbody').on('click', 'td.details-control', function () {
        var tr = $(this).closest('tr');
        var row = table.row(tr);

        if (row.child.isShown()) {
            // This row is already open - close it
            row.child.hide();
            tr.removeClass('shown');
        }
        else {
            // Open this row
            row.child(format(childData[row.index()])).show();
            tr.addClass('shown');
        }
    });
}

function format(d) {
    // `d` is the original data object for the row
    return '<table cellpadding="5" cellspacing="0" border="0" style="padding-left:50px;">' +
        '<tr>' +
        '<td>Child row data:</td>' +
        '<td>' + d.join(', ') + '</td>' +
        '</tr>' +
        '</table>';
}
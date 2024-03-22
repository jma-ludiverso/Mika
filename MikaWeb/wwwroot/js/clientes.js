$(document).ready(function () {
    $.fn.dataTable.moment("DD/MM/YYYY HH:mm:ss");
    $.fn.dataTable.moment("DD/MM/YYYY");

    $("#tbClientes").DataTable({
        // Design Assets
        stateSave: true,
        autoWidth: true,
        // ServerSide Setups
        processing: true,
        serverSide: true,
        // Paging Setups
        paging: true,
        // Searching Setups
        searching: { regex: true },
        // Ajax Filter
        ajax: {
            url: "/Clientes/LoadTable",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "idCliente" },
            { data: "nombre" },
            { data: "telefono" },
            { data: "email" }
        ],
        // Column Definitions
        columnDefs: [
            {
                targets: 0,
                render: $.fn.dataTable.render.linkCliente()
            },
            { targets: "no-sort", orderable: false },
            { targets: "no-search", searchable: false },
            {
                targets: "trim",
                render: function (data, type, full, meta) {
                    if (type === "display") {
                        data = strtrunc(data, 10);
                    }

                    return data;
                }
            },
            { targets: "date-type", type: "date-eu" }
        ],
        language: {
            url: "//cdn.datatables.net/plug-ins/1.10.21/i18n/Spanish.json"
        }
    });
});

function strtrunc(str, num) {
    if (str.length > num) {
        return str.slice(0, num) + "...";
    }
    else {
        return str;
    }
}

jQuery.fn.dataTable.render.linkCliente = function () {
    return function (d, type, row) {
        if (d.toString() != "0") {
            return '<span class="badge badge-warning"><a href="./Clientes/Edit/' + d.toString() + '">Modificar</a></span>&nbsp;' +
                '<span class="badge badge-light"><a href="./Clientes/Fichas?Id=' + d.toString() + '" style="color:#313539;">Últimas fichas</a></span>';
        } else {
            return '---<input type="hidden" value="' + d.toString() + '"/>';
        }
    };
};


$(document).ready(function () {
    $.fn.dataTable.moment("DD/MM/YYYY HH:mm:ss");
    $.fn.dataTable.moment("DD/MM/YYYY");

    $("#test-registers").DataTable({
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
            url: "/Empleados/LoadTable",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "id" },
            { data: "codigo" },
            { data: "nombre" },
            { data: "apellidos" },
            { data: "email" },
            { data: "activo" },
            { data: "administrador" }
        ],
        // Column Definitions
        columnDefs: [
            {
                targets: 0,
                render: $.fn.dataTable.render.linkEmpleado()
            },
            {
                targets: [5, 6],
                render: $.fn.dataTable.render.switch()
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

jQuery.fn.dataTable.render.linkEmpleado = function () {
    return function (d, type, row) {
        var ret = '<span class="badge badge-warning"><a href="./Identity/Account/Manage?id=' + d.toString() + '">Modificar</a></span>&nbsp;' +
            '<span class="badge badge-light"><a href="Empleados/Comisiones?id=' + d.toString() + '" style="color:#313539;">Comisiones</a></span>';
        return ret;
    };
};

jQuery.fn.dataTable.render.switch = function () {
    return function (d, type, row) {
        var s = "";
        var b = JSON.parse(d.toString());
        if (b == true) {
            s = '<div style="text-align: center;"><i class="fa fa-check-circle" aria-hidden="true"></i></div>';
        }
        else
        {
            s = '';
        }
        return s;
    };
};

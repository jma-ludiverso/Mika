$(document).ready(function () {
    $.fn.dataTable.moment("DD/MM/YYYY HH:mm:ss");
    $.fn.dataTable.moment("DD/MM/YYYY");

    $("#tbCajas").DataTable({
        // Design Assets
        stateSave: false,
        autoWidth: true,
        // ServerSide Setups
        processing: true,
        serverSide: true,
        // Paging Setups
        paging: true,
        // Searching Setups
        searching: false,
        order: [[1, "desc"]],
        //searching: { regex: true },
        // Ajax Filter
        ajax: {
            url: "/Cajas/LoadTable",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: function (d) {
                let additionalValues = [];
                additionalValues[0] = $("#filtroAnio").val();
                additionalValues[1] = $("#cboMes").val();
                d.AdditionalValues = additionalValues;
                return JSON.stringify(d);
            }
        },
        // Columns Setups
        columns: [
            { data: "nCaja" },
            { data: "fecha" },
            { data: "cerrada" },
            { data: "metalico" },
            { data: "visas" },
            { data: "gastos" },
            { data: "ivaSoportado" },
            { data: "ivaRepercutido" },
            { data: "saldoNeto" }
        ],
        // Column Definitions
        columnDefs: [
            {
                targets: 0,
                render: $.fn.dataTable.render.linkCaja()
            },
            {
                targets: 1,
                render: $.fn.dataTable.render.fecha()
            },
            {
                targets: 2,
                render: $.fn.dataTable.render.estadoCaja()
            },
            {
                targets: [3, 4, 5, 6, 7, 8],
                render: $.fn.dataTable.render.numero()
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

jQuery.fn.dataTable.render.estadoCaja = function () {
    return function (d, type, row) {
        var s = "";
        if (d) {
            s = '<div style="text-align: center;"><i class="fas fa-lock" title="cerrada"></i>&nbsp;<span class="badge badge-warning"><a href="./Cajas/Caja?ncaja=' + row.nCaja + '&acc=ROP">Reabrir caja</a></span></div>';
        }
        else {
            s = '<div style="text-align: center;"><i class="fas fa-lock-open" title="abierta"></i></div>';
        }
        return s;
    };
};

jQuery.fn.dataTable.render.fecha = function () {
    return function (d, type, row) {
        var iData = moment(d).format('DD/MM/YYYY');
        return '<div style="text-align: right;">' + iData + '</div>';
    };
};


jQuery.fn.dataTable.render.linkCaja = function () {
    return function (d, type, row) {
        return '<span class="badge badge-warning"><a href="./Cajas/Caja?ncaja=' + d.toString() + '">' + d.toString() + '</a></span>';
    };
};

jQuery.fn.dataTable.render.numero = function () {
    return function (d, type, row) {
        var iData = $.fn.dataTable.render.number('.', ',', 2).display(d); 
        return '<div style="text-align: right;">' + iData + ' €</div>';
    };
};

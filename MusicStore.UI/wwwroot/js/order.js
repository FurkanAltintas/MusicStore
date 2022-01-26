var dataTable;

$(document).ready(function () {
    var url = window.location.search;

    if (url.includes("inprocess")) {
        loadDataTable("GetOrderList?status=inprocess");
    } else {
        if (url.includes("pending")) {
            loadDataTable("GetOrderList?status=pending");
        } else {
            if (url.includes("completed")) {
                loadDataTable("GetOrderList?status=completed");
            } else {
                if (url.includes("rejected")) {
                    loadDataTable("GetOrderList?status=rejected");
                } else {
                    loadDataTable("GetOrderList?status=all");
                }
            }
        }


        // loadDataTable();
    });


function loadDataTable(url) {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/Order/" +url
        },
        "columns": [
            { "data": "name", "width": "60%" },
            { "data": "phoneNumber", "width": "60%" },
            { "data": "email", "width": "60%" },
            { "data": "orderStatus", "width": "60%" },
            { "data": "orderTotal", "width": "60%" },
            {
                "data": "id",
                "render": function (data) {
                    return `
                            <div class="text-center">
                                <a href="/Admin/Order/Details/${data}" class="btn btn-success text-white" style="cursor:pointer">
                                    <i class="fas fa-edit"></i> 
                                </a>
                            </div>
                           `;
                }, "width": "40%"
            }
        ]
    });
}